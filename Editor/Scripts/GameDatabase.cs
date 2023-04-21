using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public enum ExportFormat
    {
        Asset = 0,
        Json = 1,
    }

    public class GameDatabase : ScriptableObject
    {
        public event Action Reloaded;

        [SerializeField]
        private string dataPath;
        [SerializeField]
        private string codePath;
        [SerializeField]
        private string exportPath;
        [SerializeField]
        private string namespaceName;
        [SerializeField]
        private ExportFormat exportFormat;

        public List<WorksheetData> dataTables = new();

        private readonly ExcelDataLoader excelDataLoader = new();

        public string DataPath => dataPath;
        public string CodePath => codePath;
        public string ExportPath => exportPath;
        public ExportFormat ExportFormat => exportFormat;

        private void Awake()
        {
            dataPath = PlayerPrefs.GetString("ExcelDataExporter.DataPath", null);
            codePath = PlayerPrefs.GetString("ExcelDataExporter.CodePath", null);
            exportPath = PlayerPrefs.GetString("ExcelDataExporter.ExportPath", null);
            exportFormat = (ExportFormat)PlayerPrefs.GetInt("ExcelDataExporter.ExportFormat", 0);
        }

        public void SetCodePath(string path)
        {
            codePath = path;
            PlayerPrefs.SetString("ExcelDataExporter.CodePath", path);
        }

        public void SetExportPath(string path)
        {
            exportPath = path;
            PlayerPrefs.SetString("ExcelDataExporter.ExportPath", path);
        }

        public void SetExportFormat(int index)
        {
            exportFormat = (ExportFormat)index;
            PlayerPrefs.SetInt("ExcelDataExporter.ExportFormat", index);
        }

        public void Load(string path)
        {
            dataPath = path;
            PlayerPrefs.SetString("ExcelDataExporter.DataPath", path);

            CollectAllWorksheetDatas();
            Reloaded?.Invoke();
        }

        public void Reload()
        {
            if (string.IsNullOrEmpty(dataPath))
            {
                return;
            }

            CollectAllWorksheetDatas();
            Reloaded?.Invoke();
        }

        private void CollectAllWorksheetDatas()
        {
            dataTables.Clear();

            // Get all Excel files
            DirectoryInfo directoryInfo = new DirectoryInfo(dataPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*.xlsx", SearchOption.AllDirectories);

            foreach (FileInfo fileInfo in fileInfos)
            {
                // Skip if the file is a temp file
                if (fileInfo.Name.StartsWith("~"))
                {
                    continue;
                }

                // Skip if the file is a special file
                if (fileInfo.Name.StartsWith("$"))
                {
                    continue;
                }

                var worksheetData = new WorksheetData(dataPath, fileInfo);
                dataTables.Add(worksheetData);
            }
        }

        public void SelectAll()
        {
            for (var i = 0; i < dataTables.Count; i++)
            {
                dataTables[i].SetSelected(true);
            }
        }

        public void DeselectAll()
        {
            for (var i = 0; i < dataTables.Count; i++)
            {
                dataTables[i].SetSelected(false);
            }
        }

        public void GenerateCodeForCustomTypes()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();

            // Generate codes
            CodeGeneratorBase codeGeneratorForCustomType = new CodeGeneratorForCustomType();
            CodeGeneratorBase codeGeneratorForCustomEnum = new CodeGeneratorForCustomEnum();
            foreach (TypeData customType in customTypeTable.CustomTypes)
            {
                string scriptText;
                if (customType.define != TypeData.Define.Enum)
                {
                    scriptText = codeGeneratorForCustomType.Generate(customType);
                }
                else
                {
                    scriptText = codeGeneratorForCustomEnum.Generate(customType);
                }

                string path = $"{codePath}/CustomTypes/{customType.name}.cs";
                SaveFile(path, scriptText);
            }

            AssetDatabase.Refresh();
        }

        public void GenerateCodeForSelectedTables()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var invalidResults = new List<TypeDataValidator.Result>();
            CodeGeneratorBase codeGeneratorForData = new CodeGeneratorForData();
            CodeGeneratorBase codeGeneratorForDataTable = new CodeGeneratorForDataTable();
            CodeGeneratorBase codeGeneratorForSetting = new CodeGeneratorForSetting();

            for (var i = 0; i < dataTables.Count; i++)
            {
                WorksheetData worksheetData = dataTables[i];
                if (worksheetData.IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(worksheetData.FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = parser.ExportTypeData(sheetRawData);
                        TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                        if (!result.IsValid)
                        {
                            invalidResults.Add(result);
                            continue;
                        }

                        if (typeData.IsTypeWithId)
                        {
                            {
                                string scriptText = codeGeneratorForData.Generate(typeData);
                                string path = $"{codePath}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                                SaveFile(path, scriptText);
                            }

                            {
                                string scriptText = codeGeneratorForDataTable.Generate(typeData);
                                string path = $"{codePath}{worksheetData.RelativeFolder}/{sheetRawData.Name + "Table"}.cs";
                                SaveFile(path, scriptText);
                            }
                        }
                        else
                        {
                            string scriptText = codeGeneratorForSetting.Generate(typeData);
                            string path = $"{codePath}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                            SaveFile(path, scriptText);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowInvalidResults(invalidResults);
        }

        public void ExportSelectedTables()
        {
            switch (exportFormat)
            {
                default:
                    Debug.LogError($"Unknown export format: {exportFormat}");
                    break;
                case ExportFormat.Asset:
                    ExportSelectedTablesAsAsset();
                    break;
                case ExportFormat.Json:
                    ExportSelectedTablesAsJson();
                    break;
            }
        }

        private void ExportSelectedTablesAsJson()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var invalidResults = new List<TypeDataValidator.Result>();
            var dataExporter = new DataExporterJson();

            for (var i = 0; i < dataTables.Count; i++)
            {
                WorksheetData worksheetData = dataTables[i];
                if (worksheetData.IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(worksheetData.FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = parser.ExportTypeData(sheetRawData);
                        TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                        if (!result.IsValid)
                        {
                            invalidResults.Add(result);
                            continue;
                        }

                        string json = dataExporter.Export(typeData, sheetRawData);
                        string path = $"{exportPath}{worksheetData.RelativeFolder}/{sheetRawData.Name}.json";
                        SaveFile(path, json);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowInvalidResults(invalidResults);
        }

        private void ExportSelectedTablesAsAsset()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var invalidResults = new List<TypeDataValidator.Result>();

            for (var i = 0; i < dataTables.Count; i++)
            {
                WorksheetData worksheetData = dataTables[i];
                if (worksheetData.IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(worksheetData.FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = parser.ExportTypeData(sheetRawData);
                        TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                        if (!result.IsValid)
                        {
                            invalidResults.Add(result);
                            continue;
                        }

                        if (typeData.isTypeWithId)
                        {
                            ScriptableObject scriptableObject = ExporterScriptableObjectDataTable.Export(typeData, sheetRawData);
                            string fullPath = $"{exportPath}{worksheetData.RelativeFolder}/{sheetRawData.Name + "Table"}.asset";
                            string assetPath = FullPathToAssetPath(fullPath);

                            // Save asset
                            Type tableType = typeData.GetTableType();
                            UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath(assetPath, tableType);
                            if (@object != null)
                            {
                                scriptableObject.name = @object.name;  // Unity Rule: The name is need to be same as the original one
                                EditorUtility.CopySerialized(scriptableObject, @object);
                                EditorUtility.SetDirty(@object);
                            }
                            else
                            {
                                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                            }
                        }
                        else
                        {
                            ScriptableObject scriptableObject = ExporterScriptableObjectSetting.Export(typeData, sheetRawData);
                            string fullPath = $"{exportPath}{worksheetData.RelativeFolder}/{sheetRawData.Name}.asset";
                            string assetPath = FullPathToAssetPath(fullPath);

                            // Save asset
                            Type dataType = typeData.GetDataType();
                            UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath(assetPath, dataType);
                            if (@object != null)
                            {
                                scriptableObject.name = @object.name;  // Unity Rule: The name is need to be same as the original one
                                EditorUtility.CopySerialized(scriptableObject, @object);
                                EditorUtility.SetDirty(@object);
                            }
                            else
                            {
                                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                            }
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowInvalidResults(invalidResults);
        }

        private CustomTypeTable GenerateCustomTypeTable()
        {
            // Get CustomTypeTable file
            var fileInfo = new FileInfo($"{dataPath}/{Const.CustomTypeTableName}");
            if (!fileInfo.Exists)
            {
                Debug.LogWarning($"{Const.CustomTypeTableName} not found.");
                return null;
            }

            // Load sheet
            var worksheetData = new WorksheetData(dataPath, fileInfo);
            List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(worksheetData.FullPath);
            if (sheetRawDatas.Count == 0)
            {
                Debug.LogWarning($"The count of sheet in {Const.CustomTypeTableName}");
                return null;
            }

            // Parse
            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawDatas.ToArray());
            return customTypeTable;
        }

        private void SaveFile(string path, string data)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(data);
        }

        private static string FullPathToAssetPath(string absolutePath)
        {
            return "Assets" + absolutePath[Application.dataPath.Length..];
        }

        private static void ShowInvalidResults(IReadOnlyList<TypeDataValidator.Result> invalidResults)
        {
            if (invalidResults.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < invalidResults.Count; i++)
            {
                TypeDataValidator.Result result = invalidResults[i];
                if (result.IsValid)
                {
                    continue;
                }

                sb.Append($"{result.TypeData.name}: ");
                if (result.IsIdFieldMissing)
                {
                    sb.Append("NoIntIdField ");
                }

                if (result.HasDuplicatedName)
                {
                    sb.Append("NameDuplicated ");
                }

                sb.AppendLine();
            }

            EditorUtility.DisplayDialog("Some table failed!", sb.ToString(), "OK");
        }
    }
}
