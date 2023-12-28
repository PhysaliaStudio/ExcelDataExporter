using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class GameDatabase : ScriptableObject
    {
        public event Action Reloaded;

        public List<WorksheetData> dataTables = new();

        private readonly ExcelDataExporterUnity _exporterUnity = new();
        private readonly ExcelDataExporterDotNet _exporterDotNet = new();

        public int CurrentSettingIndex => ExporterSettings.CurrentIndex;
        public ExporterSetting CurrentSetting => ExporterSettings.CurrentSetting;
        public IReadOnlyList<ExporterSetting> Settings => ExporterSettings.Settings;

        public string DataPath => ExporterSettings.CurrentSetting.sourceDataDirectory;
        public string CodePath => ExporterSettings.CurrentSetting.exportScriptDirectory;
        public string ExportPath => ExporterSettings.CurrentSetting.exportDataDirectory;

        private void Awake()
        {
            ExporterSettings.LoadSettings();
            ExporterSettings.CurrentIndex = PlayerPrefs.GetInt("ExcelDataExporter.SettingIndex", 0);
        }

        private void OnEnable()
        {
            ExporterSetting setting = ExporterSettings.CurrentSetting;

            _exporterUnity.baseDirectory = Application.dataPath[..^"/Assets".Length];
            _exporterUnity.sourceDataDirectory = setting.sourceDataDirectory;
            _exporterUnity.exportDataDirectory = setting.exportDataDirectory;
            _exporterUnity.exportScriptDirectory = setting.exportScriptDirectory;
            _exporterUnity.filters = setting.filters;

            _exporterDotNet.baseDirectory = Application.dataPath[..^"/Assets".Length];
            _exporterDotNet.sourceDataDirectory = setting.sourceDataDirectory;
            _exporterDotNet.exportDataDirectory = setting.exportDataDirectory;
            _exporterDotNet.exportScriptDirectory = setting.exportScriptDirectory;
            _exporterDotNet.filters = setting.filters;
        }

        public void SetCodePath(string path)
        {
            ExporterSetting setting = ExporterSettings.CurrentSetting;
            setting.exportScriptDirectory = path;
            _exporterUnity.exportScriptDirectory = path;
            ExporterSettings.SaveSettings();
        }

        public void SetExportPath(string path)
        {
            ExporterSetting setting = ExporterSettings.CurrentSetting;
            setting.exportDataDirectory = path;
            _exporterUnity.exportDataDirectory = path;
            ExporterSettings.SaveSettings();
        }

        public void SetSettingIndex(int index)
        {
            ExporterSettings.CurrentIndex = index;
            PlayerPrefs.SetInt("ExcelDataExporter.SettingIndex", index);
        }

        public void Load(string path)
        {
            ExporterSetting setting = ExporterSettings.CurrentSetting;
            string relativePath = Path.GetRelativePath(Application.dataPath + "/../", path);
            relativePath = relativePath.Replace('\\', '/');
            setting.sourceDataDirectory = relativePath;
            _exporterUnity.sourceDataDirectory = relativePath;
            ExporterSettings.SaveSettings();

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
            var directoryInfo = new DirectoryInfo(fullDataPath);
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
            ExporterSetting setting = ExporterSettings.CurrentSetting;
            switch (setting.exportType)
            {
                default:
                    Debug.LogError($"Unknown export format: {setting.exportType}");
                    return;
                case ExporterSetting.ExportType.Unity:
                    results = _exporterUnity.GenerateCodeForTables(paths);
                    break;
                case ExporterSetting.ExportType.DotNet:
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
            ExporterSetting setting = ExporterSettings.CurrentSetting;
            switch (setting.exportType)
            {
                default:
                    Debug.LogError($"Unknown export format: {setting.exportType}");
                    return;
                case ExporterSetting.ExportType.Unity:
                    results = _exporterUnity.GenerateDataForTables(paths);
                    break;
                case ExporterSetting.ExportType.DotNet:
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
