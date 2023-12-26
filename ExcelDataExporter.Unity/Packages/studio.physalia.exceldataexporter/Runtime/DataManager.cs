using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataRuntime
{
    public class DataManager
    {
        private readonly Dictionary<Type, DataTable> _dataTables = new(64);
        private readonly Dictionary<Type, SettingTable> _settingTables = new(8);

        public void AddDataTable(DataTable dataTable)
        {
            Type type = dataTable.DataType;
            if (_dataTables.ContainsKey(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] DataTable with '{type}' is already added.");
                return;
            }
            _dataTables.Add(type, dataTable);
        }

        public T GetData<T>(int id)
        {
            Type type = typeof(T);
            if (!_dataTables.TryGetValue(type, out DataTable dataTable))
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
            if (!_dataTables.TryGetValue(type, out DataTable dataTable))
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

            if (_settingTables.ContainsKey(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] Setting with '{type}' is already added.");
                return;
            }

            _settingTables.Add(type, settingTable);
        }

        public T GetSettingTable<T>() where T : class
        {
            Type type = typeof(T);

            if (!typeof(SettingTable).IsAssignableFrom(type))
            {
                Debug.LogError($"[{nameof(DataManager)}] Type '{type}' is not assignable from SettingTable.");
                return default;
            }

            if (!_settingTables.TryGetValue(type, out SettingTable settingTable))
            {
                Debug.LogError($"[{nameof(DataManager)}] SettingTable with '{type}' is not found.");
                return default;
            }

            return settingTable as T;
        }

        public void Clear()
        {
            _dataTables.Clear();
            _settingTables.Clear();
        }
    }
}
