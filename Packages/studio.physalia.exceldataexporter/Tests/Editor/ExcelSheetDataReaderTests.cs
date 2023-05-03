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
            source.AddRow();
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
            var source = new SheetRawData(5);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test\nlayout=vertical");
            source.AddRow("", "field1", "int", "", "042");
            source.AddRow("", "field2", "string", "", "abc");
            source.AddRow("", "field3", "bool", "", "true");

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

        [Test]
        public void ReadDataTable_Horizontally_WithSkippedLines()
        {
            var source = new SheetRawData(5);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test");
            source.AddRow();
            source.AddRow("", "field1", "field2", "", "field3");
            source.AddRow("", "int", "string", "", "bool");
            source.AddRow("0", "", "", "0", "");
            source.AddRow("", "42", "abc", "", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Horizontal, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 1, sheetRawData.RowCount);
            Assert.AreEqual(source.ColumnCount - 2, sheetRawData.ColumnCount);  // 2 column is removed.

            Assert.AreEqual("field1", sheetRawData.Get(1, 0));
            Assert.AreEqual("field2", sheetRawData.Get(1, 1));
            Assert.AreEqual("field3", sheetRawData.Get(1, 2));
            Assert.AreEqual("int", sheetRawData.Get(2, 0));
            Assert.AreEqual("string", sheetRawData.Get(2, 1));
            Assert.AreEqual("bool", sheetRawData.Get(2, 2));
            Assert.AreEqual("42", sheetRawData.Get(4, 0));
            Assert.AreEqual("abc", sheetRawData.Get(4, 1));
            Assert.AreEqual("true", sheetRawData.Get(4, 2));
        }

        [Test]
        public void ReadDataTable_Vertically_WithSkippedLines()
        {
            var source = new SheetRawData(5);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test\nlayout=vertical");
            source.AddRow("", "", "", "0", "");
            source.AddRow("", "field1", "int", "", "042");
            source.AddRow("", "field2", "string", "", "abc");
            source.AddRow("", "field3", "bool", "", "true");
            source.AddRow("", "", "", "0", "");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Vertical, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 3, sheetRawData.RowCount);  // 2 additional row is removed.
            Assert.AreEqual(source.ColumnCount, sheetRawData.ColumnCount);

            Assert.AreEqual("field1", sheetRawData.Get(0, 1));
            Assert.AreEqual("field2", sheetRawData.Get(1, 1));
            Assert.AreEqual("field3", sheetRawData.Get(2, 1));
            Assert.AreEqual("int", sheetRawData.Get(0, 2));
            Assert.AreEqual("string", sheetRawData.Get(1, 2));
            Assert.AreEqual("bool", sheetRawData.Get(2, 2));
            Assert.AreEqual("042", sheetRawData.Get(0, 4));
            Assert.AreEqual("abc", sheetRawData.Get(1, 4));
            Assert.AreEqual("true", sheetRawData.Get(2, 4));
        }

        [Test]
        public void ReadDataTable_Horizontally_WithSkipDataLine()
        {
            var source = new SheetRawData(5);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test");
            source.AddRow();
            source.AddRow("#active", "field1", "field2", "#", "field3");
            source.AddRow("bool", "int", "string", "", "bool");
            source.AddRow("0", "", "", "0", "");
            source.AddRow("1", "42", "abc", "", "true");
            source.AddRow("0", "66", "dfg", "", "false");
            source.AddRow("1", "77", "iop", "", "true");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Horizontal, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 2, sheetRawData.RowCount);  // 1 data row is removed.
            Assert.AreEqual(source.ColumnCount - 2, sheetRawData.ColumnCount);  // 2 column is removed.

            Assert.AreEqual("field1", sheetRawData.Get(1, 0));
            Assert.AreEqual("field2", sheetRawData.Get(1, 1));
            Assert.AreEqual("field3", sheetRawData.Get(1, 2));
            Assert.AreEqual("int", sheetRawData.Get(2, 0));
            Assert.AreEqual("string", sheetRawData.Get(2, 1));
            Assert.AreEqual("bool", sheetRawData.Get(2, 2));
            Assert.AreEqual("42", sheetRawData.Get(4, 0));
            Assert.AreEqual("abc", sheetRawData.Get(4, 1));
            Assert.AreEqual("true", sheetRawData.Get(4, 2));
            Assert.AreEqual("77", sheetRawData.Get(5, 0));
            Assert.AreEqual("iop", sheetRawData.Get(5, 1));
            Assert.AreEqual("true", sheetRawData.Get(5, 2));
        }

        [Test]
        public void ReadDataTable_Vertically_WithSkipDataLine()
        {
            var source = new SheetRawData(7);
            source.SetName("TestSheet");
            source.AddRow("namespace=Test\nlayout=vertical");
            source.AddRow("", "#active", "bool", "0", "1", "0", "1");
            source.AddRow("", "field1", "int", "", "042", "066", "077");
            source.AddRow("", "field2", "string", "", "abc", "dfg", "iop");
            source.AddRow("", "field3", "bool", "", "true", "false", "true");
            source.AddRow("", "", "", "0", "", "");

            IExcelDataReader reader = new FakeDataReader(source);
            var sheetDataReader = new ExcelSheetDataReader();
            SheetRawData sheetRawData = sheetDataReader.ReadSheet(reader);

            Assert.AreEqual("Test", sheetRawData.Metadata.NamespaceName);
            Assert.AreEqual(SheetType.DataTable, sheetRawData.Metadata.SheetType);
            Assert.AreEqual(SheetLayout.Vertical, sheetRawData.Metadata.SheetLayout);

            // Remember that the metadata row is removed.
            Assert.AreEqual(source.Name, sheetRawData.Name);
            Assert.AreEqual(source.RowCount - 3, sheetRawData.RowCount);  // 2 additional row is removed.
            Assert.AreEqual(source.ColumnCount - 1, sheetRawData.ColumnCount);  // 1 data column is removed.

            Assert.AreEqual("field1", sheetRawData.Get(0, 1));
            Assert.AreEqual("field2", sheetRawData.Get(1, 1));
            Assert.AreEqual("field3", sheetRawData.Get(2, 1));
            Assert.AreEqual("int", sheetRawData.Get(0, 2));
            Assert.AreEqual("string", sheetRawData.Get(1, 2));
            Assert.AreEqual("bool", sheetRawData.Get(2, 2));
            Assert.AreEqual("042", sheetRawData.Get(0, 4));
            Assert.AreEqual("abc", sheetRawData.Get(1, 4));
            Assert.AreEqual("true", sheetRawData.Get(2, 4));
            Assert.AreEqual("077", sheetRawData.Get(0, 5));
            Assert.AreEqual("iop", sheetRawData.Get(1, 5));
            Assert.AreEqual("true", sheetRawData.Get(2, 5));
        }
    }
}
