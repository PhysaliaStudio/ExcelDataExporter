using ExcelDataReader;
using System.Collections.Generic;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataLoader
    {
        public List<SheetRawData> LoadExcelData(string filePath)
        {
            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

            var sheetRawDatas = new List<SheetRawData>();

            do
            {
                int rowCount = reader.RowCount;
                int columnCount = reader.FieldCount;
                var sheetRawData = new SheetRawData(rowCount, columnCount);

                var rowIndex = 0;
                while (reader.Read())
                {
                    for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        string text = reader.GetValue(columnIndex)?.ToString();
                        sheetRawData.Set(rowIndex, columnIndex, text);
                    }

                    rowIndex++;
                }

                sheetRawDatas.Add(sheetRawData);
            }
            while (reader.NextResult());

            return sheetRawDatas;
        }
    }
}
