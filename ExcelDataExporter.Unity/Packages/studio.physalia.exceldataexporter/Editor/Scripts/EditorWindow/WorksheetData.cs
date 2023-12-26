using System;
using System.IO;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    [Serializable]
    public class WorksheetData
    {
        [SerializeField]
        private bool isSelected;
        [SerializeField]
        private string name;
        [SerializeField]
        private string relativeFolder;
        [SerializeField]
        private string fullPath;

        public bool IsSelected => isSelected;
        public string Name => name;
        public string RelativeFolder => relativeFolder;
        public string FullPath => fullPath;

        public WorksheetData(string rootFolderPath, FileInfo fileInfo)
        {
            name = fileInfo.Name[..^".xlsx".Length];
            relativeFolder = fileInfo.DirectoryName[rootFolderPath.Length..].Replace('\\', '/');
            fullPath = fileInfo.FullName;
        }

        public void Switch()
        {
            isSelected = !isSelected;
        }

        public void SetSelected(bool value)
        {
            isSelected = value;
        }
    }
}
