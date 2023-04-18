using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public abstract class DataTable<T> : ScriptableObject
    {
        [SerializeField]
        private List<T> items = new();

        private readonly Dictionary<int, T> table = new();

        public int Count => items.Count;

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
