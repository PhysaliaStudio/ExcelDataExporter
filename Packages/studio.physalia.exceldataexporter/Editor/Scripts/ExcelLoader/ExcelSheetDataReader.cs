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

            // If the type is DataTable, check skipped columns/rows
            if (sheetRawData.Metadata.SheetType == SheetType.DataTable)
            {
                FilterOutLines(sheetRawData);
            }

            sheetRawData.RemoveRow(0);  // Remove metadata row
            return sheetRawData;
        }

        private void FilterOutLines(SheetRawData sheetRawData)
        {
            if (sheetRawData.Metadata.SheetLayout == SheetLayout.Horizontal)
            {
                for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    // Note: +1 because the metadata row is still there.
                    string text = sheetRawData.Get(Const.DataTableFilterLine + 1, columnIndex);
                    int filter = string.IsNullOrWhiteSpace(text) ? -1 : TypeUtility.ParseInt(text);
                    bool isExport = IsExport(filter);
                    if (!isExport)
                    {
                        sheetRawData.RemoveColumn(columnIndex);
                    }
                }
            }
            else if (sheetRawData.Metadata.SheetLayout == SheetLayout.Vertical)
            {
                // Note: +1 because the metadata row is still there.
                for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= 1; rowIndex--)
                {
                    string text = sheetRawData.Get(rowIndex, Const.DataTableFilterLine);
                    int filter = string.IsNullOrWhiteSpace(text) ? -1 : TypeUtility.ParseInt(text);
                    bool isExport = IsExport(filter);
                    if (!isExport)
                    {
                        sheetRawData.RemoveRow(rowIndex);
                    }
                }
            }
        }

        // Check filter with bit flag
        private bool IsExport(int filter)
        {
            return (filter & 1 << ExporterSetting.FilterFlag) != 0;
        }
    }
}
