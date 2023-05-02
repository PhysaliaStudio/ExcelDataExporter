using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class TypeDataParserTests
    {
        [Test]
        public void ExportTypeData_Horizontally_SystemTypes()
        {
            var sheetRawData = new SheetRawData(0, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "欄位2", "欄位3");
            sheetRawData.AddRow("field1", "field2", "field3");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            Assert.AreEqual("Test", typeData.namespaceName);
            Assert.AreEqual(3, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("欄位3", typeData.fieldDatas[2].comment);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("string"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[2].typeData);
        }

        [Test]
        public void ExportTypeData_Horizontally_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(0, 2);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "欄位2");
            sheetRawData.AddRow("field1", "field2");
            sheetRawData.AddRow("int[]", "bool[]");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
        }

        [Test]
        public void ExportTypeData_Horizontally_UnityTypes()
        {
            var sheetRawData = new SheetRawData(0, 5);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "", "欄位2", "", "");
            sheetRawData.AddRow("field1.x", "field1.y", "field2.x", "field2.y", "field2.z");
            sheetRawData.AddRow("Vector2Int", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
        }

        [Test]
        public void ExportTypeData_Horizontally_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(0, 7);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "", "", "", "欄位2", "", "");
            sheetRawData.AddRow("field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.AddRow("Vector2Int", "", "", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
            Assert.AreEqual(2, typeData.fieldDatas[0].arraySize);
            Assert.AreEqual(1, typeData.fieldDatas[1].arraySize);
        }

        [Test]
        public void ExportTypeData_Horizontally_CustomEnums()
        {
            var sheetRawData = new SheetRawData(0, 1);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("類型");
            sheetRawData.AddRow("type");
            sheetRawData.AddRow("EnemyType");

            var sheetForCustomEnum = new SheetRawData(0, 4);
            sheetForCustomEnum.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetForCustomEnum.AddRow("enum EnemyType", "普通", "菁英", "魔王");
            sheetForCustomEnum.AddRow("", "Normal", "Elite", "Boss");
            sheetForCustomEnum.AddRow("", "0", "1", "2");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetForCustomEnum);
            var parser = new TypeDataParser(customTypeTable);
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            Assert.AreEqual(1, typeData.fieldDatas.Count);
            Assert.AreEqual("類型", typeData.fieldDatas[0].comment);
            Assert.AreEqual("EnemyType", typeData.fieldDatas[0].typeData.name);
        }

        [Test]
        public void ExportTypeData_Horizontally_CustomEnumArrays()
        {
            var sheetRawData = new SheetRawData(0, 1);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("類型");
            sheetRawData.AddRow("types");
            sheetRawData.AddRow("EnemyType[]");

            var sheetForCustomEnum = new SheetRawData(0, 4);
            sheetForCustomEnum.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetForCustomEnum.AddRow("enum EnemyType", "普通", "菁英", "魔王");
            sheetForCustomEnum.AddRow("", "Normal", "Elite", "Boss");
            sheetForCustomEnum.AddRow("", "0", "1", "2");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetForCustomEnum);
            var parser = new TypeDataParser(customTypeTable);
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            Assert.AreEqual(1, typeData.fieldDatas.Count);
            Assert.AreEqual("類型", typeData.fieldDatas[0].comment);
            Assert.AreEqual("EnemyType", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
        }

        [Test]
        public void ExportTypeData_Vertically_SystemTypes()
        {
            var sheetRawData = new SheetRawData(0, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1", "int");
            sheetRawData.AddRow("欄位2", "field2", "string");
            sheetRawData.AddRow("欄位3", "field3", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            Assert.AreEqual("Test", typeData.namespaceName);
            Assert.AreEqual(3, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("欄位3", typeData.fieldDatas[2].comment);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("string"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[2].typeData);
        }

        [Test]
        public void ExportTypeData_Vertically_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(0, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1", "int[]");
            sheetRawData.AddRow("欄位2", "field2", "bool[]");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual(TypeUtility.GetDefaultType("int"), typeData.fieldDatas[0].typeData);
            Assert.AreEqual(TypeUtility.GetDefaultType("bool"), typeData.fieldDatas[1].typeData);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
        }

        [Test]
        public void ExportTypeData_Vertically_UnityTypes()
        {
            var sheetRawData = new SheetRawData(0, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1.x", "Vector2Int");
            sheetRawData.AddRow("", "field1.y", "");
            sheetRawData.AddRow("欄位2", "field2.x", "Vector3Int");
            sheetRawData.AddRow("", "field2.y", "");
            sheetRawData.AddRow("", "field2.z", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
        }

        [Test]
        public void ExportTypeData_Vertically_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(0, 3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1[0].x", "Vector2Int");
            sheetRawData.AddRow("", "field1[0].y", "");
            sheetRawData.AddRow("", "field1[1].x", "");
            sheetRawData.AddRow("", "field1[1].y", "");
            sheetRawData.AddRow("欄位2", "field2[0].x", "Vector3Int");
            sheetRawData.AddRow("", "field2[0].y", "");
            sheetRawData.AddRow("", "field2[0].z", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.AreEqual(2, typeData.fieldDatas.Count);
            Assert.AreEqual("欄位1", typeData.fieldDatas[0].comment);
            Assert.AreEqual("欄位2", typeData.fieldDatas[1].comment);
            Assert.AreEqual("Vector2Int", typeData.fieldDatas[0].typeData.name);
            Assert.AreEqual("Vector3Int", typeData.fieldDatas[1].typeData.name);
            Assert.AreEqual(true, typeData.fieldDatas[0].IsArray);
            Assert.AreEqual(true, typeData.fieldDatas[1].IsArray);
            Assert.AreEqual(2, typeData.fieldDatas[0].arraySize);
            Assert.AreEqual(1, typeData.fieldDatas[1].arraySize);
        }
    }
}
