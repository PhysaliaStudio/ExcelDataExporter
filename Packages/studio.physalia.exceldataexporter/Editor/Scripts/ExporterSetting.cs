using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    [Serializable]
    public class ExporterSetting
    {
        private static string settingPath;

        public string excelFolderPath = "";
        public string codeExportFolderPath = "";
        public string dataExportFolderPath = "";

        public static string SettingPath
        {
            get
            {
                if (string.IsNullOrEmpty(settingPath))
                {
                    DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
                    directory = directory.Parent;
                    settingPath = Path.Combine(directory.FullName, "ExcelDataExporterSetting.json");
                    settingPath = settingPath.Replace('\\', '/');
                }

                return settingPath;
            }
        }

        private ExporterSetting()
        {

        }

        public static ExporterSetting Load()
        {
            if (!File.Exists(SettingPath))
            {
                return new ExporterSetting();
            }

            string json = File.ReadAllText(SettingPath);
            ExporterSetting setting = JsonConvert.DeserializeObject<ExporterSetting>(json);
            return setting;
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingPath, json);
        }
    }
}
