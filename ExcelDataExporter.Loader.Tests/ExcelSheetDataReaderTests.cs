using ExcelDataReader;
using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Loader.Tests
{
    public class ExcelSheetDataReaderTests
    {
        private const string KeyNamespace = "namespace";
        private const string KeyLayout = "layout";

        [Test]
        public void ReadDataTable_Horizontally()
        {
            var source = new SheetRawData(3);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test");
            source.AddRow();
            source.AddRow("field1", "field2", "field3");
            source.AddRow("int", "string", "bool");
            source.AddRow();
            source.AddRow("42", "abc", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.Metadata[KeyNamespace], Is.EqualTo("Test"));
                Assert.That(sheetRawData.Metadata[KeyLayout], Is.EqualTo(""));

                // Remember that the metadata row is removed.
                Assert.That(sheetRawData.Name, Is.EqualTo(source.Name));
                Assert.That(sheetRawData.RowCount, Is.EqualTo(source.RowCount - 1));
                Assert.That(sheetRawData.ColumnCount, Is.EqualTo(source.ColumnCount));
            });
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                for (var j = 0; j < sheetRawData.ColumnCount; j++)
                {
                    Assert.That(sheetRawData.Get(i, j), Is.EqualTo(source.Get(i + 1, j)));
                }
            }
        }

        [Test]
        public void ReadDataTable_Vertically()
        {
            var source = new SheetRawData(5);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test\nlayout=vertical");
            source.AddRow("", "field1", "int", "", "042");
            source.AddRow("", "field2", "string", "", "abc");
            source.AddRow("", "field3", "bool", "", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.Metadata[KeyNamespace], Is.EqualTo("Test"));
                Assert.That(sheetRawData.Metadata[KeyLayout], Is.EqualTo("vertical"));

                // Remember that the metadata row is removed.
                Assert.That(sheetRawData.Name, Is.EqualTo(source.Name));
                Assert.That(sheetRawData.RowCount, Is.EqualTo(source.RowCount - 1));
                Assert.That(sheetRawData.ColumnCount, Is.EqualTo(source.ColumnCount));
            });
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                for (var j = 0; j < sheetRawData.ColumnCount; j++)
                {
                    Assert.That(sheetRawData.Get(i, j), Is.EqualTo(source.Get(i + 1, j)));
                }
            }
        }
    }
}
