using System;
using System.Collections.Generic;
using System.IO;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter
{
    public abstract class Exporter
    {
        public string baseDirectory;
        public string sourceDataDirectory;
        public string exportDataDirectory;
        public string exportScriptDirectory;
        public List<string> filters = new List<string>();

        private readonly ExcelDataLoader _excelDataLoader = new ExcelDataLoader();

        #region Common Methods
        private CustomTypeTable GenerateCustomTypeTable()
        {
            const string CustomTypeTableName = "$CustomTypes.xlsx";

            // Get CustomTypeTable file
            string fullDataPath = Path.GetFullPath(sourceDataDirectory, baseDirectory);
            var fileInfo = new FileInfo($"{fullDataPath}/{CustomTypeTableName}");
            if (!fileInfo.Exists)
            {
                throw new Exception($"{CustomTypeTableName} not found.");
            }

            // Load sheets
            List<SheetRawData> sheetRawDatas = _excelDataLoader.LoadExcelData(fileInfo.FullName);
            if (sheetRawDatas.Count == 0)
            {
                throw new Exception($"The count of sheet in {CustomTypeTableName} is 0.");
            }

            // Parse
            CustomTypeTable customTypeTable = new CustomTypeTable();
            List<TypeData> additionalTypes = GetAdditionalTypesForDefault();
            if (additionalTypes != null)
            {
                customTypeTable.AddAdditionalTypes(additionalTypes);
            }

            customTypeTable = customTypeTable.Parse(sheetRawDatas.ToArray());
            return customTypeTable;
        }

        protected virtual List<TypeData> GetAdditionalTypesForDefault()
        {
            return null;
        }

        protected static void SaveFile(string fullPath, string data)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using var stream = new FileStream(fullPath, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(data);
        }

        protected string RelativeToBasePath(string fullPath)
        {
            string relativePath = Path.GetRelativePath(baseDirectory, fullPath);
            relativePath = relativePath.Replace("\\", "/");
            return relativePath;
        }

        protected string GetExportScriptPath(string relativePath)
        {
            string directoryFullPath = Path.GetFullPath(exportScriptDirectory, baseDirectory);
            string fullPath = Path.GetFullPath(relativePath, directoryFullPath);
            fullPath = fullPath.Replace("\\", "/");
            return fullPath;
        }

        protected string GetExportDataPath(string relativePath)
        {
            string directoryFullPath = Path.GetFullPath(exportDataDirectory, baseDirectory);
            string fullPath = Path.GetFullPath(relativePath, directoryFullPath);
            fullPath = fullPath.Replace("\\", "/");
            return fullPath;
        }

        protected void CreateScriptFile(string relativePath, string text)
        {
            string fullPath = GetExportScriptPath(relativePath);
            SaveFile(fullPath, text);
        }

        protected void CreateDataFile(string relativePath, string text)
        {
            string fullPath = GetExportDataPath(relativePath);
            SaveFile(fullPath, text);
        }
        #endregion

        public void GenerateCodeForCustomTypes()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            foreach (TypeData typeData in customTypeTable.CustomTypes)
            {
                string scriptText;
                if (typeData.define != TypeData.Define.Enum)
                {
                    scriptText = new CodeGeneratorCustomType().Generate(typeData);
                }
                else
                {
                    scriptText = new CodeGeneratorCustomEnum().Generate(typeData);
                }

                CreateScriptFile($"CustomTypes/{typeData.name}.cs", scriptText);
            }
        }

        public List<TypeDataValidator.Result> GenerateCodeForTables(IReadOnlyList<string> relativePaths)
        {
            var results = new List<TypeDataValidator.Result>();

            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            for (var i = 0; i < relativePaths.Count; i++)
            {
                string fullDataPath = Path.GetFullPath(relativePaths[i], baseDirectory);
                List<SheetRawData> sheetRawDatas = _excelDataLoader.LoadExcelData(fullDataPath);
                for (var j = 0; j < sheetRawDatas.Count; j++)
                {
                    SheetRawData sheetRawData = sheetRawDatas[j];
                    PostProcessSheetRawData(sheetRawData);

                    TypeData typeData = parser.ExportTypeData(sheetRawData);
                    TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                    results.Add(result);
                    if (!result.IsValid)
                    {
                        continue;
                    }

                    if (typeData.IsTypeWithId)
                    {
                        GenerateCodeForDataTable(typeData, sheetRawData);
                    }
                    else
                    {
                        GenerateCodeForSettingTable(typeData, sheetRawData);
                    }
                }
            }

            return results;
        }

        public List<TypeDataValidator.Result> GenerateDataForTables(IReadOnlyList<string> relativePaths)
        {
            var results = new List<TypeDataValidator.Result>();

            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            for (var i = 0; i < relativePaths.Count; i++)
            {
                string fullDataPath = Path.GetFullPath(relativePaths[i], baseDirectory);
                List<SheetRawData> sheetRawDatas = _excelDataLoader.LoadExcelData(fullDataPath);
                for (var j = 0; j < sheetRawDatas.Count; j++)
                {
                    SheetRawData sheetRawData = sheetRawDatas[j];
                    PostProcessSheetRawData(sheetRawData);

                    TypeData typeData = parser.ExportTypeData(sheetRawData);
                    TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                    results.Add(result);
                    if (!result.IsValid)
                    {
                        continue;
                    }

                    if (typeData.IsTypeWithId)
                    {
                        ExportDataForDataTable(typeData, sheetRawData);
                    }
                    else
                    {
                        ExportDataForSettingTable(typeData, sheetRawData);
                    }
                }
            }

            return results;
        }

        private SheetRawData PostProcessSheetRawData(SheetRawData sheetRawData)
        {
            // Remove inactive lines
            SheetType sheetType = sheetRawData.Metadata.GetSheetType();
            if (sheetType == SheetType.DataTable)
            {
                RemoveInactiveData(sheetRawData);
            }

            // Remove skipped lines
            if (sheetType == SheetType.DataTable || sheetType == SheetType.Setting)
            {
                RemoveNotFilteredLines(sheetRawData);
            }

            return sheetRawData;
        }

        /// <summary>
        /// Check #active lines and remove skipped data, only necessary for DataTable
        /// </summary>
        private void RemoveInactiveData(SheetRawData sheetRawData)
        {
            const string DefineDataActive = "#active";

            // Find all #active lines and remove
            SheetLayout sheetLayout = sheetRawData.Metadata.GetSheetLayout();
            if (sheetLayout == SheetLayout.Horizontal)
            {
                for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    string name = sheetRawData.Get(Const.DataTableNameLine, columnIndex);
                    if (name == DefineDataActive)
                    {
                        for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= Const.DataTableStartLine; rowIndex--)
                        {
                            string text = sheetRawData.Get(rowIndex, columnIndex);
                            bool isExport = TypeUtility.ParseBool(text);
                            if (!isExport)
                            {
                                sheetRawData.RemoveRow(rowIndex);
                            }
                        }

                        sheetRawData.RemoveColumn(columnIndex);  // Remove the skip column
                    }
                }
            }
            else if (sheetLayout == SheetLayout.Vertical)
            {
                for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= 1; rowIndex--)
                {
                    string name = sheetRawData.Get(rowIndex, Const.DataTableNameLine);
                    if (name == DefineDataActive)
                    {
                        for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= Const.DataTableStartLine; columnIndex--)
                        {
                            string text = sheetRawData.Get(rowIndex, columnIndex);
                            bool isExport = TypeUtility.ParseBool(text);
                            if (!isExport)
                            {
                                sheetRawData.RemoveColumn(columnIndex);
                            }
                        }

                        sheetRawData.RemoveRow(rowIndex);  // Remove the skip row
                    }
                }
            }
        }

        /// <summary>
        /// Remove skipped lines
        /// </summary>
        private void RemoveNotFilteredLines(SheetRawData sheetRawData)
        {
            SheetLayout sheetLayout = sheetRawData.Metadata.GetSheetLayout();
            if (sheetLayout == SheetLayout.Horizontal)
            {
                for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    string text = sheetRawData.Get(Const.DataTableFilterLine, columnIndex);
                    bool isExported = filters.Contains(text);
                    if (!isExported)
                    {
                        sheetRawData.RemoveColumn(columnIndex);
                    }
                }
            }
            else if (sheetLayout == SheetLayout.Vertical)
            {
                for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= 0; rowIndex--)
                {
                    string text = sheetRawData.Get(rowIndex, Const.DataTableFilterLine);
                    bool isExported = filters.Contains(text);
                    if (!isExported)
                    {
                        sheetRawData.RemoveRow(rowIndex);
                    }
                }
            }
        }

        protected abstract void GenerateCodeForDataTable(TypeData typeData, SheetRawData sheetRawData);
        protected abstract void GenerateCodeForSettingTable(TypeData typeData, SheetRawData sheetRawData);

        protected abstract void ExportDataForDataTable(TypeData typeData, SheetRawData sheetRawData);
        protected abstract void ExportDataForSettingTable(TypeData typeData, SheetRawData sheetRawData);
    }
}
