using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class DataExporterJsonTests
    {
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

        [Test]
        public void ExportDataTableToJson_UnityTypeArraysWithNA()
        {
            var sheetRawData = new SheetRawData(4, 7);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.SetRow(2, "Vector2Int", "", "", "", "Vector3Int", "", "");
            sheetRawData.SetRow(3, "1", "2", "N/A", "N/A", "5", "6", "7");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[{\"x\":1,\"y\":2}],\"field2\":[{\"x\":5,\"y\":6,\"z\":7}]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }
    }
}
