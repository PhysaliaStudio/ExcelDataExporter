using ExcelDataReader;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    internal class ExcelSheetDataReader
    {
        private readonly FilterSetting filterSetting = new();

        public ExcelSheetDataReader()
        {

        }

        public ExcelSheetDataReader(params string[] filterWords)
        {
            filterSetting.SetWords(filterWords);
        }

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

            // Check skipped columns/rows
            if (sheetRawData.Metadata.SheetType == SheetType.DataTable)
            {
                HandleDefineSkipData(sheetRawData);
            }

            // Check skipped lines
            if (sheetRawData.Metadata.SheetType == SheetType.DataTable || sheetRawData.Metadata.SheetType == SheetType.Setting)
            {
                FilterOutLines(sheetRawData);
            }

            sheetRawData.RemoveRow(0);  // Remove metadata row
            return sheetRawData;
        }

        /// <summary>
        /// Check #active column/row and remove skipped data, only necessary for DataTable
        /// </summary>
        private void HandleDefineSkipData(SheetRawData sheetRawData)
        {
            if (sheetRawData.Metadata.SheetLayout == SheetLayout.Horizontal)
            {
                for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    string name = sheetRawData.Get(Const.DataTableNameLine + 1, columnIndex);
                    if (name == Const.DefineDataActive)
                    {
                        for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= Const.DataTableStartLine + 1; rowIndex--)
                        {
                            string text = sheetRawData.Get(rowIndex, columnIndex);
                            bool isExport = TypeUtility.ParseBool(text);
                            if (!isExport)
                            {
                                sheetRawData.RemoveRow(rowIndex);
                            }
                        }

                        sheetRawData.RemoveColumn(columnIndex);  // Remove the skip column
                    }
                }
            }
            else if (sheetRawData.Metadata.SheetLayout == SheetLayout.Vertical)
            {
                for (var rowIndex = sheetRawData.RowCount - 1; rowIndex >= 1; rowIndex--)
                {
                    string name = sheetRawData.Get(rowIndex, Const.DataTableNameLine);
                    if (name == Const.DefineDataActive)
                    {
                        for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= Const.DataTableStartLine; columnIndex--)
                        {
                            string text = sheetRawData.Get(rowIndex, columnIndex);
                            bool isExport = TypeUtility.ParseBool(text);
                            if (!isExport)
                            {
                                sheetRawData.RemoveColumn(columnIndex);
                            }
                        }

                        sheetRawData.RemoveRow(rowIndex);  // Remove the skip row
                    }
                }
            }
        }

        /// <summary>
        /// Remove skipped lines
        /// </summary>
        private void FilterOutLines(SheetRawData sheetRawData)
        {
            if (sheetRawData.Metadata.SheetLayout == SheetLayout.Horizontal)
            {
                for (var columnIndex = sheetRawData.ColumnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    // Note: +1 because the metadata row is still there.
                    string text = sheetRawData.Get(Const.DataTableFilterLine + 1, columnIndex);
                    bool isExported = filterSetting.IsMatch(text);
                    if (!isExported)
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
                    bool isExported = filterSetting.IsMatch(text);
                    if (!isExported)
                    {
                        sheetRawData.RemoveRow(rowIndex);
                    }
                }
            }
        }
    }
}
