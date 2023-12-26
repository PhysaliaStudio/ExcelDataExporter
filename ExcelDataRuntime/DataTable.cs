using System;
using System.Collections.Generic;

namespace Physalia.ExcelDataRuntime
{
    public abstract class DataTable
    {
        public abstract Type DataType { get; }
    }

    public abstract class DataTable<T> : DataTable
    {
        private readonly List<T> _items;
        private readonly Dictionary<int, T> _table;

        public int Count => _items.Count;
        public override Type DataType => typeof(T);

        public DataTable(List<T> items)
        {
            _items = new List<T>(items);

            _table = new Dictionary<int, T>(items.Count);
            if (typeof(IHasId).IsAssignableFrom(typeof(T)))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    T item = items[i];
                    int id = (item as IHasId).Id;
                    _ = _table.TryAdd(id, item);
                }
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    _table.Add(i, items[i]);
                }
            }
        }

        public IReadOnlyList<T> GetDataList()
        {
            return _items;
        }

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
                throw new ArgumentException($"[{nameof(DataTable<T>)}] Data with '{id}' is not found.");
            }

            return data;
        }
    }
}
