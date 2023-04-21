using System.Text;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class SheetRawData
    {
        private string name = string.Empty;
        private Metadata metadata;
        private int rowCount;
        private int columnCount;
        private string[][] table;

        public string Name => name;
        public Metadata Metadata => metadata;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;

        public SheetRawData(int rowCount, int columnCount)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            table = new string[rowCount][];
            for (var i = 0; i < rowCount; i++)
            {
                table[i] = new string[columnCount];
            }
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }

            this.name = name;
        }

        public void SetMetadata(string metadataText)
        {
            metadata = Metadata.Parse(metadataText);
        }

        public string Get(int rowIndex, int columnIndex)
        {
            return table[rowIndex][columnIndex];
        }

        public void Set(int rowIndex, int columnIndex, string text)
        {
            table[rowIndex][columnIndex] = text;
        }

        public string[] GetRow(int rowIndex)
        {
            var row = new string[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                row[i] = table[rowIndex][i];
            }
            return row;
        }

        public string[] GetColumn(int columnIndex)
        {
            var column = new string[rowCount];
            for (var i = 0; i < rowCount; i++)
            {
                column[i] = table[i][columnIndex];
            }
            return column;
        }

        public void SetRow(int rowIndex, params string[] texts)
        {
            if (texts.Length > columnCount)
            {
                Debug.LogError($"[{nameof(SheetRawData)}] The column count of the inserted row doesn't match.\n" +
                    $"Expected: <={columnCount}  But was: {texts.Length}");
                return;
            }

            for (var i = 0; i < texts.Length; i++)
            {
                table[rowIndex][i] = texts[i];
            }
        }

        public void ResizeBounds()
        {
            // Calculate new table size
            var newRowCount = 0;
            for (var i = rowCount - 1; i >= 0; i--)
            {
                if (!IsRowEmpty(i))
                {
                    newRowCount = i + 1;
                    break;
                }
            }

            var newColumnCount = 0;
            for (var i = columnCount - 1; i >= 0; i--)
            {
                if (!IsColumnEmpty(i))
                {
                    newColumnCount = i + 1;
                    break;
                }
            }

            // Create new table
            if (newRowCount == rowCount && newColumnCount == columnCount)
            {
                return;
            }

            string[][] newTable = new string[newRowCount][];
            for (var i = 0; i < newRowCount; i++)
            {
                newTable[i] = new string[newColumnCount];
                for (var j = 0; j < newColumnCount; j++)
                {
                    newTable[i][j] = table[i][j];
                }
            }

            // Replace table
            rowCount = newRowCount;
            columnCount = newColumnCount;
            table = newTable;
        }

        private bool IsRowEmpty(int rowIndex)
        {
            for (var i = 0; i < columnCount; i++)
            {
                if (!string.IsNullOrEmpty(table[rowIndex][i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsColumnEmpty(int columnIndex)
        {
            for (var i = 0; i < rowCount; i++)
            {
                if (!string.IsNullOrEmpty(table[i][columnIndex]))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    string text = table[rowIndex][columnIndex];
                    sb.Append(text);
                    if (columnIndex != columnCount - 1)
                    {
                        sb.Append('|');
                    }
                }

                if (rowIndex != rowCount - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
