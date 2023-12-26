using ExcelDataReader;
using System.Collections.Generic;
using System.IO;

namespace Physalia.ExcelDataExporter.Loader
{
    public class ExcelDataLoader
    {
        private readonly ExcelSheetDataReader _sheetDataReader = new ExcelSheetDataReader();

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

                SheetRawData sheetRawData = _sheetDataReader.ReadSheet(reader);
                sheetRawData.SetFilePath(filePath);
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
