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
                if (fileInfo.Name.Contains("$"))
                {
                    continue;
                }

                var worksheetData = new WorksheetData(dataPath, fileInfo);
                dataTables.Add(worksheetData);
            }
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
