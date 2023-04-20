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
        private readonly TypeDataParser sheetParser = new();

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
            // Get CustomTypeTable file
            var fileInfo = new FileInfo($"{dataPath}/{Const.CustomTypeTableName}");
            if (!fileInfo.Exists)
            {
                Debug.LogWarning($"{Const.CustomTypeTableName} not found.");
                return;
            }

            // Load sheet
            var worksheetData = new WorksheetData(dataPath, fileInfo);
            List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(worksheetData.FullPath);
            if (sheetRawDatas.Count == 0)
            {
                Debug.LogWarning($"The count of sheet in {Const.CustomTypeTableName}");
                return;
            }

            // Generate codes
            CodeGeneratorBase codeGeneratorForCustomType = new CodeGeneratorForCustomType();
            CodeGeneratorBase codeGeneratorForCustomEnum = new CodeGeneratorForCustomEnum();
            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawDatas.ToArray());
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
            var invalidResults = new List<TypeDataValidator.Result>();
            CodeGeneratorBase codeGeneratorForData = new CodeGeneratorForData();
            CodeGeneratorBase codeGeneratorForDataTable = new CodeGeneratorForDataTable();
            CodeGeneratorBase codeGeneratorForSetting = new CodeGeneratorForSetting();

            for (var i = 0; i < dataTables.Count; i++)
            {
                if (dataTables[i].IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(dataTables[i].FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        WorksheetData worksheetData = dataTables[i];
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = ParseToTypeData(worksheetData, sheetRawData);
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
                                string relativePath = worksheetData.NameWithFolder.EndsWith("Table") ? worksheetData.NameWithFolder[..^"Table".Length] : worksheetData.NameWithFolder;
                                string path = $"{codePath}{relativePath}.cs";
                                SaveFile(path, scriptText);
                            }

                            {
                                string scriptText = codeGeneratorForDataTable.Generate(typeData);
                                string relativePath = worksheetData.NameWithFolder.EndsWith("Table") ? worksheetData.NameWithFolder : worksheetData.NameWithFolder + "Table";
                                string path = $"{codePath}{relativePath}.cs";
                                SaveFile(path, scriptText);
                            }
                        }
                        else
                        {
                            string scriptText = codeGeneratorForSetting.Generate(typeData);
                            string relativePath = worksheetData.NameWithFolder;
                            string path = $"{codePath}{relativePath}.cs";
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
            var invalidResults = new List<TypeDataValidator.Result>();
            var dataExporter = new DataExporterJson();

            for (var i = 0; i < dataTables.Count; i++)
            {
                if (dataTables[i].IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(dataTables[i].FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        WorksheetData worksheetData = dataTables[i];
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = ParseToTypeData(worksheetData, sheetRawData);
                        TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                        if (!result.IsValid)
                        {
                            invalidResults.Add(result);
                            continue;
                        }

                        string json = dataExporter.Export(typeData, sheetRawDatas[j]);
                        string path = $"{exportPath}{dataTables[i].NameWithFolder}.json";
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
            var invalidResults = new List<TypeDataValidator.Result>();

            for (var i = 0; i < dataTables.Count; i++)
            {
                if (dataTables[i].IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(dataTables[i].FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        WorksheetData worksheetData = dataTables[i];
                        SheetRawData sheetRawData = sheetRawDatas[j];
                        TypeData typeData = ParseToTypeData(worksheetData, sheetRawData);
                        TypeDataValidator.Result result = new TypeDataValidator().Validate(typeData);
                        if (!result.IsValid)
                        {
                            invalidResults.Add(result);
                            continue;
                        }

                        ScriptableObject scriptableObject = ExporterScriptableObjectDataTable.Export(typeData, sheetRawDatas[j]);
                        string relativePath = worksheetData.NameWithFolder.EndsWith("Table") ? worksheetData.NameWithFolder : worksheetData.NameWithFolder + "Table";
                        string absolutePath = $"{exportPath}{relativePath}.asset";
                        string assetPath = AbsoluteToAssetPath(absolutePath);

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
                            _ = Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                            AssetDatabase.CreateAsset(scriptableObject, assetPath);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowInvalidResults(invalidResults);
        }

        private TypeData ParseToTypeData(WorksheetData worksheetData, SheetRawData sheetRawData)
        {
            string typeName = worksheetData.Name.EndsWith("Table") ? worksheetData.Name[..^"Table".Length] : worksheetData.Name;
            TypeData typeData = sheetParser.ExportTypeData(typeName, sheetRawData);
            return typeData;
        }

        private void SaveFile(string path, string data)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(data);
        }

        private static string AssetToAbsolutePath(string assetPath)
        {
            return Application.dataPath + assetPath["Assets".Length..];
        }

        private static string AbsoluteToAssetPath(string absolutePath)
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
