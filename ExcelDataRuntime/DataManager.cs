using System;
using System.Collections.Generic;

namespace Physalia.ExcelDataRuntime
{
    public class DataManager
    {
        private readonly Dictionary<Type, DataTable> _dataTables = new Dictionary<Type, DataTable>(64);
        private readonly Dictionary<Type, SettingTable> _settingTables = new Dictionary<Type, SettingTable>(8);

        public void AddDataTable(DataTable dataTable)
        {
            Type type = dataTable.DataType;
            if (_dataTables.ContainsKey(type))
            {
                throw new ArgumentException($"[{nameof(DataManager)}] DataTable with '{type}' is already added.");
            }
            _dataTables.Add(type, dataTable);
        }

        public T GetData<T>(int id)
        {
            Type type = typeof(T);
            if (!_dataTables.TryGetValue(type, out DataTable dataTable))
            {
                throw new ArgumentException($"[{nameof(DataManager)}] DataTable with '{type}' is not found.");
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
                throw new ArgumentException($"[{nameof(DataManager)}] DataTable with '{type}' is not found.");
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
                throw new ArgumentException($"[{nameof(DataManager)}] Type '{type}' is not assignable from SettingTable.");
            }

            if (_settingTables.ContainsKey(type))
            {
                throw new ArgumentException($"[{nameof(DataManager)}] SettingTable with '{type}' is already added.");
            }

            _settingTables.Add(type, settingTable);
        }

        public T GetSettingTable<T>() where T : class
        {
            Type type = typeof(T);

            if (!typeof(SettingTable).IsAssignableFrom(type))
            {
                throw new ArgumentException($"[{nameof(DataManager)}] Type '{type}' is not assignable from SettingTable.");
            }

            if (!_settingTables.TryGetValue(type, out SettingTable settingTable))
            {
                throw new ArgumentException($"[{nameof(DataManager)}] SettingTable with '{type}' is not found.");
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
