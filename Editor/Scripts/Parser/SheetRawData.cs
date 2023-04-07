using System.Text;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class SheetRawData
    {
        private readonly int rowCount;
        private readonly int columnCount;
        private readonly string[][] table;

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
            return table[rowIndex];
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
