using ExcelDataReader;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    internal class ExcelSheetDataReader
    {
        internal SheetRawData ReadSheet(IExcelDataReader reader)
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
