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

        private static int _currentIndex = -1;
        private static List<ExporterSetting> _settings;

        public static string SettingPath
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

        public static int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex == value)
                {
                    return;
                }

                if (_currentIndex < 0 || _currentIndex >= _settings.Count)
                {
                    _currentIndex = 0;
                }

                _currentIndex = value;
            }
        }

        public static ExporterSetting CurrentSetting
        {
            get
            {
                if (_settings == null)
                {
                    LoadSettings();
                }

                if (_settings.Count == 0)
                {
                    _currentIndex = 0;
                    var setting = new ExporterSetting();
                    _settings.Add(setting);
                    return setting;
                }

                if (_currentIndex < 0 || _currentIndex >= _settings.Count)
                {
                    _currentIndex = 0;
                }

                return _settings[_currentIndex];
            }
        }

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
