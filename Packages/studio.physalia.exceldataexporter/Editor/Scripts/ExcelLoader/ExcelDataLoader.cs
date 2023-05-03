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
            var sheetDataReader = new ExcelSheetDataReader();
            do
            {
                var sheetRawData = sheetDataReader.ReadSheet(reader);
                if (sheetRawData != null)
                {
                    sheetRawDatas.Add(sheetRawData);
                }
            }
            while (reader.NextResult());

            return sheetRawDatas;
        }
    }
}
