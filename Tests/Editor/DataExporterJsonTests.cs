using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class DataExporterJsonTests
    {
        [Test]
        public void ExportDataTableToJson_SystemTypes()
        {
            var sheetRawData = new SheetRawData(3, 3);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.SetRow(0, "field1", "field2", "field3");
            sheetRawData.SetRow(1, "int", "string", "bool");
            sheetRawData.SetRow(2, "42", "abc", "true");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":42,\"field2\":\"abc\",\"field3\":true}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(3, 2);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.SetRow(0, "field1", "field2");
            sheetRawData.SetRow(1, "int[]", "bool[]");
            sheetRawData.SetRow(2, "1,2,3", "true,false,true");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[1,2,3],\"field2\":[true,false,true]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_UnityTypes()
        {
            var sheetRawData = new SheetRawData(3, 5);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.SetRow(0, "field1.x", "field1.y", "field2.x", "field2.y", "field2.z");
            sheetRawData.SetRow(1, "Vector2Int", "", "Vector3Int", "", "");
            sheetRawData.SetRow(2, "1", "2", "3", "4", "5");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":{\"x\":1,\"y\":2},\"field2\":{\"x\":3,\"y\":4,\"z\":5}}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_Horizontally_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(3, 7);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.SetRow(0, "field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.SetRow(1, "Vector2Int", "", "", "", "Vector3Int", "", "");
            sheetRawData.SetRow(2, "1", "2", "3", "4", "5", "6", "7");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[{\"x\":1,\"y\":2},{\"x\":3,\"y\":4}],\"field2\":[{\"x\":5,\"y\":6,\"z\":7}]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_Vertically_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(7, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.SetRow(0, "field1[0].x", "Vector2Int", "1");
            sheetRawData.SetRow(1, "field1[0].y", "", "2");
            sheetRawData.SetRow(2, "field1[1].x", "", "3");
            sheetRawData.SetRow(3, "field1[1].y", "", "4");
            sheetRawData.SetRow(4, "field2[0].x", "Vector3Int", "5");
            sheetRawData.SetRow(5, "field2[0].y", "", "6");
            sheetRawData.SetRow(6, "field2[0].z", "", "7");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[{\"x\":1,\"y\":2},{\"x\":3,\"y\":4}],\"field2\":[{\"x\":5,\"y\":6,\"z\":7}]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExportDataTableToJson_UnityTypeArraysWithNA()
        {
            var sheetRawData = new SheetRawData(3, 7);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.SetRow(0, "field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.SetRow(1, "Vector2Int", "", "", "", "Vector3Int", "", "");
            sheetRawData.SetRow(2, "1", "2", "N/A", "N/A", "5", "6", "7");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            string expected = "{\"field1\":[{\"x\":1,\"y\":2}],\"field2\":[{\"x\":5,\"y\":6,\"z\":7}]}\n";
            string actual = new DataExporterJson().Export(typeData, sheetRawData);
            Assert.AreEqual(expected, actual);
        }
    }
}
