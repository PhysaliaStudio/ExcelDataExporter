using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class GameDatabase : ScriptableObject
    {
        public event Action Reloaded;

        public string path;

        private string codePath;
        private string exportPath;
        public List<WorksheetData> dataTables = new();

        private readonly ExcelDataLoader excelDataLoader = new();
        private readonly SheetParser sheetParser = new();

        public string CodePath => codePath;
        public string ExportPath => exportPath;

        public void SetCodePath(string path)
        {
            codePath = path;
        }

        public void SetExportPath(string path)
        {
            exportPath = path;
        }

        public void Load(string path)
        {
            this.path = path;
            CollectAllWorksheetDatas();
            Reloaded?.Invoke();
        }

        public void Reload()
        {
            if (string.IsNullOrEmpty(path))
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
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*.xlsx", SearchOption.AllDirectories);

            foreach (FileInfo fileInfo in fileInfos)
            {
                // Skip if the file is a temp file
                if (fileInfo.Name.Contains("$"))
                {
                    continue;
                }

                var worksheetData = new WorksheetData(path, fileInfo);
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
                        Debug.Log(json);
                    }
                }
            }
        }
    }
}
