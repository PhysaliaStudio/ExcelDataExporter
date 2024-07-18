using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public int ExportFlags => ExporterSettings.CurrentFlags;
        public IReadOnlyList<ExporterSetting> Settings => ExporterSettings.Settings;

        private void OnEnable()
        {
            ExporterSettings.LoadSettings();
            ExporterSettings.CurrentFlags = PlayerPrefs.GetInt("ExcelDataExporter.ExportFlags", 0);
            _exporterUnity.baseDirectory = Application.dataPath[..^"/Assets".Length];
            _exporterDotNet.baseDirectory = Application.dataPath[..^"/Assets".Length];
        }

        public void SetExportFlags(int value)
        {
            ExporterSettings.CurrentFlags = value;
            PlayerPrefs.SetInt("ExcelDataExporter.ExportFlags", value);
        }

        public void Reload()
        {
            if (ExporterSettings.Settings.Count == 0)
            {
                return;
            }

            string sourceDataDirectory = ExporterSettings.Settings[0].sourceDataDirectory;
            if (string.IsNullOrEmpty(sourceDataDirectory))
            {
                return;
            }

            string fullDataPath = RelativePathToFullPath(sourceDataDirectory);
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
            string sourceDataDirectory = ExporterSettings.Settings[0].sourceDataDirectory;
            string fullDataPath = RelativePathToFullPath(sourceDataDirectory);
            var directoryInfo = new DirectoryInfo(fullDataPath);
            IEnumerable<FileInfo> fileInfos = directoryInfo.GetFiles("*.xlsx", SearchOption.AllDirectories)
                .Union(directoryInfo.GetFiles("*.xlsm", SearchOption.AllDirectories));

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

        private Exporter SetupAndGetExporter(ExporterSetting setting)
        {
            switch (setting.exportType)
            {
                default:
                    Debug.LogError($"Unknown export format: {setting.exportType}");
                    return null;
                case ExporterSetting.ExportType.Unity:
                    _exporterUnity.sourceDataDirectory = setting.sourceDataDirectory;
                    _exporterUnity.exportDataDirectory = setting.exportDataDirectory;
                    _exporterUnity.exportScriptDirectory = setting.exportScriptDirectory;
                    _exporterUnity.filters = setting.filters;
                    return _exporterUnity;
                case ExporterSetting.ExportType.DotNet:
                    _exporterDotNet.sourceDataDirectory = setting.sourceDataDirectory;
                    _exporterDotNet.exportDataDirectory = setting.exportDataDirectory;
                    _exporterDotNet.exportScriptDirectory = setting.exportScriptDirectory;
                    _exporterDotNet.filters = setting.filters;
                    return _exporterDotNet;
            }
        }

        public void GenerateCodeForCustomTypes()
        {
            int exportFlags = ExporterSettings.CurrentFlags;
            for (var i = 0; i < ExporterSettings.Settings.Count; i++)
            {
                if ((exportFlags & (1 << i)) == 0)
                {
                    continue;
                }

                Exporter exporter = SetupAndGetExporter(ExporterSettings.Settings[i]);
                exporter.GenerateCodeForCustomTypes();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Generate custom types successfully!", "OK");
        }

        public void GenerateCodeForSelectedTables()
        {
            List<string> paths = GetSelectedWorksheetPaths();
            var results = new List<TypeDataValidator.Result>();

            int exportFlags = ExporterSettings.CurrentFlags;
            for (var i = 0; i < ExporterSettings.Settings.Count; i++)
            {
                if ((exportFlags & (1 << i)) == 0)
                {
                    continue;
                }

                Exporter exporter = SetupAndGetExporter(ExporterSettings.Settings[i]);
                results = exporter.GenerateCodeForTables(paths);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowValidationResults(results);
        }

        public void ExportSelectedTables()
        {
            List<string> paths = GetSelectedWorksheetPaths();
            var results = new List<TypeDataValidator.Result>();

            int exportFlags = ExporterSettings.CurrentFlags;
            for (var i = 0; i < ExporterSettings.Settings.Count; i++)
            {
                if ((exportFlags & (1 << i)) == 0)
                {
                    continue;
                }

                Exporter exporter = SetupAndGetExporter(ExporterSettings.Settings[i]);
                results = exporter.GenerateDataForTables(paths);
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
