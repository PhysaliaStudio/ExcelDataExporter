using ExcelDataReader;
using System.Collections.Generic;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataLoader
    {
        private readonly ExcelSheetDataReader sheetDataReader;

        public ExcelDataLoader()
        {
            sheetDataReader = new ExcelSheetDataReader();
        }

        public ExcelDataLoader(params string[] filterWords)
        {
            sheetDataReader = new ExcelSheetDataReader(filterWords);
        }

        public List<SheetRawData> LoadExcelData(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

            var sheetRawDatas = new List<SheetRawData>();
            do
            {
                if (reader.Name.StartsWith('$'))
                {
                    continue;
                }

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
