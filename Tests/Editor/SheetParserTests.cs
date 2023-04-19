using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class SheetParserTests
    {
        [Test]
        public void ExportTypeData_SystemTypes()
        {
            var sheetRawData = new SheetRawData(3, 3);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1", "field2", "field3");
            sheetRawData.SetRow(2, "int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            Assert.AreEqual("Test", typeData.namespaceName);
            Assert.AreEqual(3, typeData.fieldDatas.Count);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("string"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[2].typeData);
        }

        [Test]
        public void ExportTypeData_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(3, 2);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1", "field2");
            sheetRawData.SetRow(2, "int[]", "bool[]");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
        }

        [Test]
        public void ExportTypeData_UnityTypes()
        {
            var sheetRawData = new SheetRawData(3, 5);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1.x", "field1.y", "field2.x", "field2.y", "field2.z");
            sheetRawData.SetRow(2, "Vector2Int", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
        }

        [Test]
        public void ExportTypeData_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(3, 7);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.SetRow(2, "Vector2Int", "", "", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
            Assert.AreEqual(2, typeData.fieldDatas[0].arraySize);
            Assert.AreEqual(1, typeData.fieldDatas[1].arraySize);
        }

        [Test]
        public void ExportDataTableToJson_SystemTypes()
        {
            var sheetRawData = new SheetRawData(4, 3);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1", "field2", "field3");
            sheetRawData.SetRow(2, "int", "string", "bool");
            sheetRawData.SetRow(3, "42", "abc", "true");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":42,\"field2\":\"abc\",\"field3\":true}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(4, 2);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1", "field2");
            sheetRawData.SetRow(2, "int[]", "bool[]");
            sheetRawData.SetRow(3, "1,2,3", "true,false,true");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[1,2,3],\"field2\":[true,false,true]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_UnityTypes()
        {
            var sheetRawData = new SheetRawData(4, 5);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1.x", "field1.y", "field2.x", "field2.y", "field2.z");
            sheetRawData.SetRow(2, "Vector2Int", "", "Vector3Int", "", "");
            sheetRawData.SetRow(3, "1", "2", "3", "4", "5");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":{\"x\":1,\"y\":2},\"field2\":{\"x\":3,\"y\":4,\"z\":5}}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(4, 7);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.SetRow(2, "Vector2Int", "", "", "", "Vector3Int", "", "");
            sheetRawData.SetRow(3, "1", "2", "3", "4", "5", "6", "7");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[{\"x\":1,\"y\":2},{\"x\":3,\"y\":4}],\"field2\":[{\"x\":5,\"y\":6,\"z\":7}]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }
    }
}
