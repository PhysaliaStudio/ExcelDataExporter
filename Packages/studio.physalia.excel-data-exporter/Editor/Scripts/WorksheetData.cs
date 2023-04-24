using System;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    [Serializable]
    public class WorksheetData
    {
        private readonly FileInfo fileInfo;
        private readonly string name;
        private readonly string relativeFolder;

        private bool isSelected;

        public string FullPath => fileInfo.FullName;
        public string Name => name;
        public string RelativeFolder => relativeFolder;

        public bool IsSelected => isSelected;

        public WorksheetData(string rootFolderPath, FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            name = fileInfo.Name[..^".xlsx".Length];
            relativeFolder = fileInfo.DirectoryName[rootFolderPath.Length..].Replace('\\', '/');
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
