using ExcelDataReader;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
                var sheetRawData = new SheetRawData(rowCount - 1, columnCount);

                // Read the first row as metadata
                bool success = reader.Read();
                if (!success)
                {
                    continue;
                }

                string metadataText = reader.GetValue(0)?.ToString();
                sheetRawData.SetMetadata(metadataText);
                if (!sheetRawData.Metadata.Export)
                {
                    var fileInfo = new FileInfo(filePath);
                    Debug.LogWarning($"Skip {fileInfo.Name[..^".xlsx".Length]}/{reader.Name}, export=false");
                    continue;
                }

                // Read remain rows
                var rowIndex = 1;
                while (reader.Read())
                {
                    for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        string text = reader.GetValue(columnIndex)?.ToString();
                        sheetRawData.Set(rowIndex - 1, columnIndex, text);
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
