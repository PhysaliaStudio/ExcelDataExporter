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
        private ExporterSetting setting;
        [SerializeField]
        private ExportFormat exportFormat;

        public List<WorksheetData> dataTables = new();

        private readonly ExcelDataLoader excelDataLoader = new("client", "both");

        public string DataPath => setting.excelFolderPath;
        public string CodePath => setting.codeExportFolderPath;
        public string ExportPath => setting.dataExportFolderPath;
        public ExportFormat ExportFormat => exportFormat;

        private void Awake()
        {
            setting = ExporterSetting.Load();
            exportFormat = (ExportFormat)PlayerPrefs.GetInt("ExcelDataExporter.ExportFormat", 0);
        }

        public void SetCodePath(string path)
        {
            setting.codeExportFolderPath = path;
            setting.Save();
        }

        public void SetExportPath(string path)
        {
            setting.dataExportFolderPath = path;
            setting.Save();
        }

        public void SetExportFormat(int index)
        {
            exportFormat = (ExportFormat)index;
            PlayerPrefs.SetInt("ExcelDataExporter.ExportFormat", index);
        }

        public void Load(string path)
        {
            string relativePath = Path.GetRelativePath(Application.dataPath + "/../", path);
            relativePath = relativePath.Replace('\\', '/');
            setting.excelFolderPath = relativePath;
            setting.Save();

            CollectAllWorksheetDatas();
            Reloaded?.Invoke();
        }

        public void Reload()
        {
            if (string.IsNullOrEmpty(DataPath))
            {
                return;
            }

            string fullDataPath = RelativePathToFullPath(DataPath);
            if (!Directory.Exists(fullDataPath))
            {
                return;
            }

            CollectAllWorksheetDatas();
            Reloaded?.Invoke();
        }

        private void CollectAllWorksheetDatas()
        {
            // Note: We use scriptable object to cache each file path and selection status, so use replace instead of clear
            var existedDataTables = new Dictionary<string, WorksheetData>(dataTables.Count);
            for (var i = 0; i < dataTables.Count; i++)
            {
                existedDataTables.Add(dataTables[i].FullPath, dataTables[i]);
            }

            // Get all Excel files
            string fullDataPath = RelativePathToFullPath(DataPath);
            DirectoryInfo directoryInfo = new DirectoryInfo(fullDataPath);
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

                if (!existedDataTables.Remove(fileInfo.FullName))
                {
                    var worksheetData = new WorksheetData(fullDataPath, fileInfo);
                    dataTables.Add(worksheetData);
                }
            }

            // Remove those not existed anymore
            foreach (var existedDataTable in existedDataTables)
            {
                dataTables.Remove(existedDataTable.Value);
            }

            dataTables.Sort((a, b) => a.Name.CompareTo(b.Name));
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

                string path = $"{AssetPathToFullPath(CodePath)}/CustomTypes/{customType.name}.cs";
                SaveFile(path, scriptText);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Generate {customTypeTable.Count} custom types successfully!", "OK");
        }

        public void GenerateCodeForSelectedTables()
        {
            switch (exportFormat)
            {
                default:
                    Debug.LogError($"Unknown export format: {exportFormat}");
                    break;
                case ExportFormat.Asset:
                    GenerateCodeForSelectedTablesForAsset();
                    break;
                case ExportFormat.Json:
                    GenerateCodeForSelectedTablesForJson();
                    break;
            }
        }

        private void GenerateCodeForSelectedTablesForJson()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var results = new List<TypeDataValidator.Result>();
            CodeGeneratorBase codeGeneratorForData = new CodeGeneratorForJsonData();
            CodeGeneratorBase codeGeneratorForSetting = new CodeGeneratorForJsonSetting();

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
                        results.Add(result);

                        if (!result.IsValid)
                        {
                            continue;
                        }

                        if (typeData.IsTypeWithId)
                        {
                            string scriptText = codeGeneratorForData.Generate(typeData);
                            string path = $"{AssetPathToFullPath(CodePath)}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                            SaveFile(path, scriptText);
                        }
                        else
                        {
                            string scriptText = codeGeneratorForSetting.Generate(typeData);
                            string path = $"{AssetPathToFullPath(CodePath)}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                            SaveFile(path, scriptText);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowValidationResults(results);
        }

        private void GenerateCodeForSelectedTablesForAsset()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var results = new List<TypeDataValidator.Result>();
            CodeGeneratorBase codeGeneratorForData = new CodeGeneratorForAssetData();
            CodeGeneratorBase codeGeneratorForDataTable = new CodeGeneratorForAssetDataTable();
            CodeGeneratorBase codeGeneratorForSettingTable = new CodeGeneratorForAssetSettingTable();

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
                        results.Add(result);

                        if (!result.IsValid)
                        {
                            continue;
                        }

                        if (typeData.IsTypeWithId)
                        {
                            {
                                string scriptText = codeGeneratorForData.Generate(typeData);
                                string path = $"{AssetPathToFullPath(CodePath)}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                                SaveFile(path, scriptText);
                            }

                            {
                                string scriptText = codeGeneratorForDataTable.Generate(typeData);
                                string path = $"{AssetPathToFullPath(CodePath)}{worksheetData.RelativeFolder}/{sheetRawData.Name + "Table"}.cs";
                                SaveFile(path, scriptText);
                            }
                        }
                        else
                        {
                            string scriptText = codeGeneratorForSettingTable.Generate(typeData);
                            string path = $"{AssetPathToFullPath(CodePath)}{worksheetData.RelativeFolder}/{sheetRawData.Name}.cs";
                            SaveFile(path, scriptText);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowValidationResults(results);
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

            var results = new List<TypeDataValidator.Result>();
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
                        results.Add(result);

                        if (!result.IsValid)
                        {
                            continue;
                        }

                        string json = dataExporter.Export(typeData, sheetRawData);
                        string path = $"{AssetPathToFullPath(ExportPath)}{worksheetData.RelativeFolder}/{sheetRawData.Name}.json";
                        SaveFile(path, json);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowValidationResults(results);
        }

        private void ExportSelectedTablesAsAsset()
        {
            CustomTypeTable customTypeTable = GenerateCustomTypeTable();
            var parser = new TypeDataParser(customTypeTable);

            var results = new List<TypeDataValidator.Result>();

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
                        results.Add(result);

                        if (!result.IsValid)
                        {
                            continue;
                        }

                        if (typeData.isTypeWithId)
                        {
                            ScriptableObject scriptableObject = ExporterScriptableObjectDataTable.Export(typeData, sheetRawData);
                            string assetPath = $"{ExportPath}{worksheetData.RelativeFolder}/{sheetRawData.Name + "Table"}.asset";

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
                                string fullPath = AssetPathToFullPath(assetPath);
                                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                            }
                        }
                        else
                        {
                            ScriptableObject scriptableObject = ExporterScriptableObjectSetting.Export(typeData, sheetRawData);
                            string assetPath = $"{ExportPath}{worksheetData.RelativeFolder}/{sheetRawData.Name}.asset";

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
                                string fullPath = AssetPathToFullPath(assetPath);
                                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                            }
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowValidationResults(results);
        }

        private CustomTypeTable GenerateCustomTypeTable()
        {
            // Get CustomTypeTable file
            string fullDataPath = RelativePathToFullPath(DataPath);
            var fileInfo = new FileInfo($"{fullDataPath}/{Const.CustomTypeTableName}");
            if (!fileInfo.Exists)
            {
                Debug.LogWarning($"{Const.CustomTypeTableName} not found.");
                return null;
            }

            // Load sheet
            var worksheetData = new WorksheetData(fullDataPath, fileInfo);
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

        public static string RelativePathToFullPath(string relativePath)
        {
            return Path.GetFullPath(relativePath, Application.dataPath + "/../");
        }

        private static string AssetPathToFullPath(string assetPath)
        {
            return Application.dataPath + assetPath["Assets".Length..];
        }

        private static string FullPathToAssetPath(string absolutePath)
        {
            return "Assets" + absolutePath[Application.dataPath.Length..];
        }

        private static void ShowValidationResults(IReadOnlyList<TypeDataValidator.Result> results)
        {
            if (results.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning", $"You didn't select any worksheet!", "Oh...");
                return;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < results.Count; i++)
            {
                TypeDataValidator.Result result = results[i];
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

            if (sb.Length > 0)
            {
                EditorUtility.DisplayDialog("Some tables failed!", sb.ToString(), "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Success", $"Exported {results.Count} sheets successfully!", "OK");
            }
        }
    }
}
