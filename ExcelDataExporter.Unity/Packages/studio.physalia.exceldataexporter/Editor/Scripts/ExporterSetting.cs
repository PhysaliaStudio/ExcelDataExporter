using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    [Serializable]
    public class ExporterSetting
    {
        public enum ExportType { Unity, DotNet }

        public ExportType exportType = ExportType.Unity;
        public string name = "";
        public string sourceDataDirectory = "";
        public string exportDataDirectory = "";
        public string exportScriptDirectory = "";
        public List<string> filters = new();

        internal ExporterSetting()
        {

        }
    }

    public static class ExporterSettings
    {
        private const string FileName = "ExcelDataExporterSetting.json";

        private static string _settingPath;

        private static int _currentFlags = 0;
        private static List<ExporterSetting> _settings;

        private static string SettingPath
        {
            get
            {
                if (string.IsNullOrEmpty(_settingPath))
                {
                    var directory = new DirectoryInfo(Application.dataPath);
                    directory = directory.Parent;
                    _settingPath = Path.Combine(directory.FullName, FileName);
                    _settingPath = _settingPath.Replace('\\', '/');
                }

                return _settingPath;
            }
        }

        public static int CurrentFlags { get { return _currentFlags; } set { _currentFlags = value; } }
        public static IReadOnlyList<ExporterSetting> Settings => _settings;

        public static void LoadSettings()
        {
            if (!File.Exists(SettingPath))
            {
                _settings = new List<ExporterSetting>();
                SaveSettings();
            }

            string json = File.ReadAllText(SettingPath);
            _settings = JsonConvert.DeserializeObject<List<ExporterSetting>>(json);
        }

        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(SettingPath, json);
        }
    }
}
