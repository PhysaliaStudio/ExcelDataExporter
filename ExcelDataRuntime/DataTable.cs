using System;
using System.Collections;
using System.Collections.Generic;

namespace Physalia.ExcelDataRuntime
{
    public abstract class DataTable
    {
        public abstract Type DataType { get; }

        public abstract void AddData(object data);
        public abstract void AddDataList(IList dataList);
    }

    public abstract class DataTable<T> : DataTable
    {
        private readonly List<T> _items = new List<T>();
        private readonly Dictionary<int, T> _table = new Dictionary<int, T>();

        public int Count => _items.Count;
        public override Type DataType => typeof(T);

        public override void AddData(object data)
        {
            if (data is T item)
            {
                AddData(item);
            }
            else
            {
                throw new ArgumentException($"[{nameof(DataTable<T>)}] AddData failed! Data is not {typeof(T)}.");
            }
        }

        public override void AddDataList(IList dataList)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                if (dataList is IList<T> itemList)
                {
                    AddDataList(itemList);
                }
                else
                {
                    throw new ArgumentException($"[{nameof(DataTable<T>)}] AddDataList failed! Data is not IList<{typeof(T)}>.");
                }
            }
        }

        public void AddData(T data)
        {
            if (typeof(IHasId).IsAssignableFrom(typeof(T)))
            {
                int id = (data as IHasId).Id;
                _ = _table.TryAdd(id, data);
            }
            else
            {
                int id = _items.Count;
                _table.Add(id, data);
            }
            _items.Add(data);
        }

        public void AddDataList(IList<T> dataList)
        {
            if (typeof(IHasId).IsAssignableFrom(typeof(T)))
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    T item = dataList[i];
                    int id = (item as IHasId).Id;
                    bool success = _table.TryAdd(id, item);
                    if (success)
                    {
                        _items.Add(item);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    _table.Add(i, dataList[i]);
                    _items.Add(dataList[i]);
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
