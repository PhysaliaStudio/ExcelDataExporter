using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public abstract class DataTable<T> : ScriptableObject where T : IHasId
    {
        [SerializeField]
        private List<T> items = new();

        private readonly Dictionary<int, T> table = new();

        public int Count => items.Count;

        public void BuildTable()
        {
            table.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                T item = items[i];
                table.Add(item.Id, item);
            }
        }

        public bool Contains(int id)
        {
            return table.ContainsKey(id);
        }

        public bool TryGetData(int id, out T data)
        {
            return table.TryGetValue(id, out data);
        }

        public T GetData(int id)
        {
            bool success = table.TryGetValue(id, out T data);
            if (!success)
            {
                Debug.LogError($"Data not found. id: {id}");
                return default;
            }

            return data;
        }
    }
}
