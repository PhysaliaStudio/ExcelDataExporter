using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataRuntime
{
    public abstract class DataTable : ScriptableObject
    {
        public abstract Type DataType { get; }
    }

    public abstract class DataTable<T> : DataTable, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> _items = new();

        private readonly Dictionary<int, T> _table = new();

        public int Count => _items.Count;
        public override Type DataType => typeof(T);

        #region Implement ISerializationCallbackReceiver
        public void OnAfterDeserialize()
        {
            _table.Clear();

            if (typeof(IHasId).IsAssignableFrom(typeof(T)))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    T item = _items[i];
                    int id = (item as IHasId).Id;

                    bool success = _table.TryAdd(id, item);
                    if (!success)
                    {
                        Debug.LogError($"ID Conflict! <{GetType().Name}> Id: {id} at index {i}", this);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    _table.Add(i, _items[i]);
                }
            }
        }

        public void OnBeforeSerialize()
        {

        }
        #endregion

        public bool Contains(int id)
        {
            return _table.ContainsKey(id);
        }

        public bool TryGetData(int id, out T data)
        {
            return _table.TryGetValue(id, out data);
        }

        public T GetData(int id)
        {
            bool success = _table.TryGetValue(id, out T data);
            if (!success)
            {
                Debug.LogError($"Data not found. type: {typeof(T)}, id: {id}");
                return default;
            }

            return data;
        }

        public IReadOnlyList<T> GetDataList()
        {
            return _items;
        }
    }
}
