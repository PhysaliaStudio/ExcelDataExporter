using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class SheetParserTests
    {
        [Test]
        public void ExportTypeData_AllFieldsAreBuiltInTypes()
        {
            var sheetRawData = new SheetRawData(2, 3);
            sheetRawData.SetRow(0, "Field1", "Field2", "Field3");
            sheetRawData.SetRow(1, "int", "string", "bool");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);
            Assert.AreEqual(3, typeData.fieldDatas.Count);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("string"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[2].typeData);
        }

        [Test]
        public void ExportTypeData_AllFieldsAreArrayOfBuiltInTypes()
        {
            var sheetRawData = new SheetRawData(2, 2);
            sheetRawData.SetRow(0, "Field1", "Field2");
            sheetRawData.SetRow(1, "int[]", "bool[]");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
        }

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
