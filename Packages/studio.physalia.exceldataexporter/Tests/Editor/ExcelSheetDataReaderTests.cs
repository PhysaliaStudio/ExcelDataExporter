using ExcelDataReader;
using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class ExcelSheetDataReaderTests
    {
        [Test]
        public void ReadDataTable_Horizontally()
        {
            var source = new SheetRawData(3);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test");
            source.AddRow();
            source.AddRow("field1", "field2", "field3");
            source.AddRow("int", "string", "bool");
            source.AddRow("42", "abc", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Horizontal, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 1, sheetRawData.RowCount);
            Assert.AreEqual(source.ColumnCount, sheetRawData.ColumnCount);
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                for (var j = 0; j < sheetRawData.ColumnCount; j++)
                {
                    Assert.AreEqual(source.Get(i + 1, j), sheetRawData.Get(i, j));
                }
            }
        }

        [Test]
        public void ReadDataTable_Vertically()
        {
            var source = new SheetRawData(4);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test\nlayout=vertical");
            source.AddRow("", "field1", "int", "042");
            source.AddRow("", "field2", "string", "abc");
            source.AddRow("", "field3", "bool", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Vertical, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 1, sheetRawData.RowCount);
            Assert.AreEqual(source.ColumnCount, sheetRawData.ColumnCount);
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                for (var j = 0; j < sheetRawData.ColumnCount; j++)
                {
                    Assert.AreEqual(source.Get(i + 1, j), sheetRawData.Get(i, j));
                }
            }
        }
    }
}
