using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
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

        public List<WorksheetData> dataTables = new();

        private readonly ExcelDataLoader excelDataLoader = new();
        private readonly SheetParser sheetParser = new();

        public string DataPath => dataPath;
        public string CodePath => codePath;
        public string ExportPath => exportPath;

        private void Awake()
        {
            dataPath = PlayerPrefs.GetString("ExcelDataExporter.DataPath", null);
            codePath = PlayerPrefs.GetString("ExcelDataExporter.CodePath", null);
            exportPath = PlayerPrefs.GetString("ExcelDataExporter.ExportPath", null);
            namespaceName = PlayerPrefs.GetString("ExcelDataExporter.NamespaceName", null);
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

        public void SaveNamespace()
        {
            PlayerPrefs.SetString("ExcelDataExporter.NamespaceName", namespaceName);
        }

        public void Load(string path)
        {
            this.dataPath = path;
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

                if (fileInfo.Name == Const.CustomTypeTableName)
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
            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawDatas[0]);
            foreach (TypeData customType in customTypeTable.CustomTypes)
            {
                string scriptText = TypeCodeGenerator.Generate(namespaceName, customType);
                string path = $"{codePath}/CustomTypes/{customType.name}.cs";
                SaveFile(path, scriptText);
            }

            AssetDatabase.Refresh();
        }

        public void GenerateCodeForSelectedTables()
        {
            for (var i = 0; i < dataTables.Count; i++)
            {
                if (dataTables[i].IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(dataTables[i].FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        string typeName = dataTables[i].Name.EndsWith("Table") ? dataTables[i].Name[..^"Table".Length] + "Data" : dataTables[i].Name + "Data";
                        TypeData typeData = sheetParser.ExportTypeData(typeName, sheetRawDatas[j]);
                        string scriptText = TypeCodeGenerator.Generate(namespaceName, typeData);

                        string relativePath = dataTables[i].NameWithFolder.EndsWith("Table") ? dataTables[i].NameWithFolder[..^"Table".Length] + "Data" : dataTables[i].NameWithFolder + "Data";
                        string path = $"{codePath}{relativePath}.cs";
                        SaveFile(path, scriptText);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void ExportSelectedTables()
        {
            for (var i = 0; i < dataTables.Count; i++)
            {
                if (dataTables[i].IsSelected)
                {
                    List<SheetRawData> sheetRawDatas = excelDataLoader.LoadExcelData(dataTables[i].FullPath);
                    for (var j = 0; j < sheetRawDatas.Count; j++)
                    {
                        string json = sheetParser.ExportDataTableAsJson(sheetRawDatas[j]);
                        string path = $"{exportPath}{dataTables[i].NameWithFolder}.json";
                        SaveFile(path, json);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SaveFile(string path, string data)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write(data);
        }
    }
}
