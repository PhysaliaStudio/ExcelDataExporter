using System;
using System.Collections.Generic;
using System.Text;

namespace Physalia.ExcelDataExporter.Loader
{
    public class SheetRawData
    {
        private string _name = string.Empty;
        private Metadata _metadata;
        private int _rowCount;
        private int _columnCount;
        private List<List<string>> _table;

        public string Name
        {
            get
            {
                if (_metadata != null && !string.IsNullOrEmpty(_metadata.OverrideName))
                {
                    return _metadata.OverrideName;
                }

                return _name;
            }
        }

        public Metadata Metadata => _metadata;
        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;

        public SheetRawData(int columnCount) : this(0, columnCount)
        {

        }

        public SheetRawData(int rowCount, int columnCount)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;

            _table = new List<List<string>>(rowCount);
            for (var i = 0; i < rowCount; i++)
            {
                var row = new List<string>(columnCount);
                _table.Add(row);

                for (var j = 0; j < columnCount; j++)
                {
                    row.Add(null);
                }
            }
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }

            _name = name;
        }

        public void SetMetadata(string metadataText)
        {
            _metadata = Metadata.Parse(metadataText);
        }

        public string Get(int rowIndex, int columnIndex)
        {
            return _table[rowIndex][columnIndex];
        }

        public void Set(int rowIndex, int columnIndex, string text)
        {
            _table[rowIndex][columnIndex] = text;
        }

        public string[] GetRow(int rowIndex)
        {
            var row = new string[_columnCount];
            for (var i = 0; i < _columnCount; i++)
            {
                row[i] = _table[rowIndex][i];
            }
            return row;
        }

        public string[] GetColumn(int columnIndex)
        {
            var column = new string[_rowCount];
            for (var i = 0; i < _rowCount; i++)
            {
                column[i] = _table[i][columnIndex];
            }
            return column;
        }

        public void SetRow(int rowIndex, params string[] texts)
        {
            if (texts.Length > _columnCount)
            {
                throw new ArgumentException("The column count of the inserted row doesn't match. " +
                    $"Expected: <={_columnCount}  But was: {texts.Length}");
            }

            for (var i = 0; i < texts.Length; i++)
            {
                _table[rowIndex][i] = texts[i];
            }
        }

        public void AddRow(params string[] texts)
        {
            if (texts.Length > _columnCount)
            {
                throw new ArgumentException("The column count of the inserted row doesn't match. " +
                    $"Expected: <={_columnCount}  But was: {texts.Length}");
            }

            var newRow = new List<string>(_columnCount);
            _table.Add(newRow);
            _rowCount++;

            for (var i = 0; i < texts.Length; i++)
            {
                newRow.Add(texts[i]);
            }
            for (var i = texts.Length; i < _columnCount; i++)
            {
                newRow.Add(null);
            }
        }

        public void RemoveRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rowCount)
            {
                throw new IndexOutOfRangeException();
            }

            _table.RemoveAt(rowIndex);
            _rowCount--;
        }

        public void RemoveColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _columnCount)
            {
                throw new IndexOutOfRangeException();
            }

            for (var i = 0; i < _rowCount; i++)
            {
                _table[i].RemoveAt(columnIndex);
            }
            _columnCount--;
        }

        /// <summary>
        /// Trim empty rows and columns
        /// </summary>
        public void ResizeBounds()
        {
            // Calculate new table size
            var newRowCount = 0;
            for (var i = _rowCount - 1; i >= 0; i--)
            {
                if (!IsRowEmpty(i))
                {
                    newRowCount = i + 1;
                    break;
                }
            }

            var newColumnCount = 0;
            for (var i = _columnCount - 1; i >= 0; i--)
            {
                if (!IsColumnEmpty(i))
                {
                    newColumnCount = i + 1;
                    break;
                }
            }

            // Create new table
            if (newRowCount == _rowCount && newColumnCount == _columnCount)
            {
                return;
            }

            var newTable = new List<List<string>>(newRowCount);
            for (var i = 0; i < newRowCount; i++)
            {
                var row = new List<string>(newColumnCount);
                newTable.Add(row);

                for (var j = 0; j < newColumnCount; j++)
                {
                    row.Add(_table[i][j]);
                }
            }

            // Replace table
            _rowCount = newRowCount;
            _columnCount = newColumnCount;
            _table = newTable;
        }

        private bool IsRowEmpty(int rowIndex)
        {
            for (var i = 0; i < _columnCount; i++)
            {
                if (!string.IsNullOrEmpty(_table[rowIndex][i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsColumnEmpty(int columnIndex)
        {
            for (var i = 0; i < _rowCount; i++)
            {
                if (!string.IsNullOrEmpty(_table[i][columnIndex]))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var rowIndex = 0; rowIndex < _rowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < _columnCount; columnIndex++)
                {
                    string text = _table[rowIndex][columnIndex];
                    sb.Append(text);
                    if (columnIndex != _columnCount - 1)
                    {
                        sb.Append('|');
                    }
                }

                if (rowIndex != _rowCount - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
