using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class DataManager
    {
        private readonly Dictionary<Type, DataTable> tables = new(128);
        private readonly Dictionary<Type, SettingTable> settings = new(128);

        public void AddDataTable(DataTable dataTable)
        {
            Type type = dataTable.DataType;
            if (tables.ContainsKey(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] DataTable with '{type}' is already added.");
                return;
            }
            tables.Add(type, dataTable);
        }

        public T GetData<T>(int id)
        {
            Type type = typeof(T);
            if (!tables.TryGetValue(type, out DataTable dataTable))
            {
                Debug.LogError($"[{nameof(DataManager)}] DataTable with '{type}' is not found.");
                return default;
            }

            var genericTable = dataTable as DataTable<T>;
            T data = genericTable.GetData(id);
            return data;
        }

        public IReadOnlyList<T> GetDataList<T>()
        {
            Type type = typeof(T);
            if (!tables.TryGetValue(type, out DataTable dataTable))
            {
                Debug.LogError($"[{nameof(DataManager)}] DataTable with '{type}' is not found.");
                return default;
            }

            var genericTable = dataTable as DataTable<T>;
            IReadOnlyList<T> dataList = genericTable.GetDataList();
            return dataList;
        }

        public void AddSettingTable(SettingTable settingTable)
        {
            Type type = settingTable.GetType();

            if (!typeof(SettingTable).IsAssignableFrom(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] Type '{type}' is not assignable from Setting.");
                return;
            }

            if (settings.ContainsKey(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] Setting with '{type}' is already added.");
                return;
            }

            settings.Add(type, settingTable);
        }

        public T GetSettingTable<T>() where T : class
        {
            Type type = typeof(T);

            if (!typeof(SettingTable).IsAssignableFrom(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] Type '{type}' is not assignable from SettingTable.");
                return default;
            }

            if (!settings.TryGetValue(type, out SettingTable settingTable))
            {
                Debug.LogError($"[{nameof(DataManager)}] SettingTable with '{type}' is not found.");
                return default;
            }

            return settingTable as T;
        }

        public void Clear()
        {
            tables.Clear();
            settings.Clear();
        }
    }
}
