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
        public List<WorksheetData> dataTables = new();

        public void Load(string path)
        {
            if (this.path != path)
            {
                this.path = path;
                CollectAllWorksheetDatas();
                Reloaded?.Invoke();
            }
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
    }
}
