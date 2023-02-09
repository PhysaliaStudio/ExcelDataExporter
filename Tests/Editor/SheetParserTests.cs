using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class SheetParserTests
    {
        [Test]
        public void DataTable_BuiltInTypes()
        {
            var sheetRawData = new SheetRawData(3, 3);
            sheetRawData.SetRow(0, "Field1", "Field2", "Field3");
            sheetRawData.SetRow(1, "int", "string", "bool");
            sheetRawData.SetRow(2, "42", "abc", "true");

            var parser = new SheetParser();

            string expected = "{\"Field1\":42,\"Field2\":\"abc\",\"Field3\":true}\n";
            string actual = parser.ExportDataTableAsJson(sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DataTable_BuiltInTypes_Array()
        {
            var sheetRawData = new SheetRawData(3, 2);
            sheetRawData.SetRow(0, "Field1", "Field2");
            sheetRawData.SetRow(1, "int[]", "bool[]");
            sheetRawData.SetRow(2, "1,2,3", "true,false,true");

            var parser = new SheetParser();

            string expected = "{\"Field1\":[1,2,3],\"Field2\":[true,false,true]}\n";
            string actual = parser.ExportDataTableAsJson(sheetRawData);
            Assert.AreEqual(expected, actual);
        }
    }
}
