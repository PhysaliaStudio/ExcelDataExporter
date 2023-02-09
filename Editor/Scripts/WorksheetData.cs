using System;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    [Serializable]
    public class WorksheetData
    {
        private readonly string rootFolderPath;
        private readonly FileInfo fileInfo;
        private readonly string name;

        private bool isSelected;

        public string FullPath => fileInfo.FullName;
        public string RelativePath => fileInfo.FullName[rootFolderPath.Length..];
        public string Name => name;

        public bool IsSelected => isSelected;

        public WorksheetData(string rootFolderPath, FileInfo fileInfo)
        {
            this.rootFolderPath = rootFolderPath;
            this.fileInfo = fileInfo;
            name = fileInfo.Name[..^".xlsx".Length];
        }

        public void Switch()
        {
            isSelected = !isSelected;
        }
    }
}
