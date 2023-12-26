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

        private readonly ExcelDataExporterUnity _exporterUnity = new();
        private readonly ExcelDataExporterDotNet _exporterDotNet = new();

        public string DataPath => setting.excelFolderPath;
        public string CodePath => setting.codeExportFolderPath;
        public string ExportPath => setting.dataExportFolderPath;
        public ExportFormat ExportFormat => exportFormat;

        private void Awake()
        {
            setting = ExporterSetting.Load();
            exportFormat = (ExportFormat)PlayerPrefs.GetInt("ExcelDataExporter.ExportFormat", 0);
        }

        private void OnEnable()
        {
            _exporterUnity.baseDirectory = Application.dataPath[..^"/Assets".Length];
            _exporterUnity.sourceDataDirectory = setting.excelFolderPath;
            _exporterUnity.exportDataDirectory = setting.dataExportFolderPath;
            _exporterUnity.exportScriptDirectory = setting.codeExportFolderPath;
            _exporterUnity.filters = new List<string> { "client", "both" };

            _exporterDotNet.baseDirectory = Application.dataPath[..^"/Assets".Length];
            _exporterDotNet.sourceDataDirectory = setting.excelFolderPath;
            _exporterDotNet.exportDataDirectory = setting.dataExportFolderPath;
            _exporterDotNet.exportScriptDirectory = setting.codeExportFolderPath;
            _exporterDotNet.filters = new List<string> { "server", "both" };
        }

        public void SetCodePath(string path)
        {
            setting.codeExportFolderPath = path;
            _exporterUnity.exportScriptDirectory = path;
            setting.Save();
        }

        public void SetExportPath(string path)
        {
            setting.dataExportFolderPath = path;
            _exporterUnity.exportDataDirectory = path;
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
            _exporterUnity.sourceDataDirectory = relativePath;
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
            _exporterUnity.GenerateCodeForCustomTypes();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Generate custom types successfully!", "OK");
        }

        public void GenerateCodeForSelectedTables()
        {
            List<string> paths = GetSelectedWorksheetPaths();

            List<TypeDataValidator.Result> results;
            switch (exportFormat)
            {
                default:
                    Debug.LogError($"Unknown export format: {exportFormat}");
                    return;
                case ExportFormat.Asset:
                    results = _exporterUnity.GenerateCodeForTables(paths);
                    break;
                case ExportFormat.Json:
                    results = _exporterDotNet.GenerateCodeForTables(paths);
                    break;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowValidationResults(results);
        }

        public void ExportSelectedTables()
        {
            List<string> paths = GetSelectedWorksheetPaths();

            List<TypeDataValidator.Result> results;
            switch (exportFormat)
            {
                default:
                    Debug.LogError($"Unknown export format: {exportFormat}");
                    return;
                case ExportFormat.Asset:
                    results = _exporterUnity.GenerateDataForTables(paths);
                    break;
                case ExportFormat.Json:
                    results = _exporterDotNet.GenerateDataForTables(paths);
                    break;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowValidationResults(results);
        }

        private List<string> GetSelectedWorksheetPaths()
        {
            var paths = new List<string>(dataTables.Count);
            for (var i = 0; i < dataTables.Count; i++)
            {
                WorksheetData worksheetData = dataTables[i];
                if (worksheetData.IsSelected)
                {
                    paths.Add(worksheetData.FullPath);
                }
            }

            return paths;
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
