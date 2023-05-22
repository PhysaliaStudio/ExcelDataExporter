using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public abstract class DataTable : ScriptableObject
    {
        public abstract Type DataType { get; }
    }

    public abstract class DataTable<T> : DataTable, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> items = new();

        private readonly Dictionary<int, T> table = new();

        public int Count => items.Count;
        public override Type DataType => typeof(T);

        #region Implement ISerializationCallbackReceiver
        public void OnAfterDeserialize()
        {
            table.Clear();

            if (typeof(IHasId).IsAssignableFrom(typeof(T)))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    T item = items[i];
                    int id = (item as IHasId).Id;

                    bool success = table.TryAdd(id, item);
                    if (!success)
                    {
                        Debug.LogError($"ID Conflict! <{GetType().Name}> Id: {id} at index {i}", this);
                    }
                }
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    table.Add(i, items[i]);
                }
            }
        }

        public void OnBeforeSerialize()
        {

        }
        #endregion

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
                Debug.LogError($"Data not found. type: {typeof(T)}, id: {id}");
                return default;
            }

            return data;
        }

        public IReadOnlyList<T> GetDataList()
        {
            return items;
        }
    }
}
