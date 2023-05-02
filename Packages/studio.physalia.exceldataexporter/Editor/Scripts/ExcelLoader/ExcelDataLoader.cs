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
                var sheetRawData = ReadSheet(reader);
                if (sheetRawData != null)
                {
                    sheetRawDatas.Add(sheetRawData);
                }
            }
            while (reader.NextResult());

            return sheetRawDatas;
        }

        private SheetRawData ReadSheet(IExcelDataReader reader)
        {
            int rowCount = reader.RowCount;
            int columnCount = reader.FieldCount;
            var sheetRawData = new SheetRawData(rowCount, columnCount);
            sheetRawData.SetName(reader.Name);

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

            sheetRawData.ResizeBounds();

            return PostProcessSheetRawData(sheetRawData);
        }

        private SheetRawData PostProcessSheetRawData(SheetRawData sheetRawData)
        {
            string metadataText = sheetRawData.Get(0, 0);
            sheetRawData.SetMetadata(metadataText);

            // Skip if export=false
            if (!sheetRawData.Metadata.Export)
            {
                Debug.LogWarning($"Skip {sheetRawData.Name}, export=false");
                return null;
            }

            sheetRawData.RemoveRow(0);  // Remove metadata row
            return sheetRawData;
        }
    }
}
