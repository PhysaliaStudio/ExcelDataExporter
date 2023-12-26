using NUnit.Framework;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter.Tests
{
    public class TypeDataParserTests
    {
        [Test]
        public void ExportTypeData_Horizontally_SystemTypes()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "欄位2", "欄位3");
            sheetRawData.AddRow("field1", "field2", "field3");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.namespaceName, Is.EqualTo("Test"));
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(3));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[2].comment, Is.EqualTo("欄位3"));
                Assert.That(typeData.fieldDatas[0].typeData, Is.EqualTo(TypeUtility.GetDefaultType("int")));
                Assert.That(typeData.fieldDatas[1].typeData, Is.EqualTo(TypeUtility.GetDefaultType("string")));
                Assert.That(typeData.fieldDatas[2].typeData, Is.EqualTo(TypeUtility.GetDefaultType("bool")));
            });
        }

        [Test]
        public void ExportTypeData_Horizontally_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(2);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "欄位2");
            sheetRawData.AddRow("field1", "field2");
            sheetRawData.AddRow("int[]", "bool[]");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData, Is.EqualTo(TypeUtility.GetDefaultType("int")));
                Assert.That(typeData.fieldDatas[1].typeData, Is.EqualTo(TypeUtility.GetDefaultType("bool")));
                Assert.That(typeData.fieldDatas[0].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[1].IsArray, Is.EqualTo(true));
            });
        }

        [Test]
        public void ExportTypeData_Horizontally_UnityTypes()
        {
            var sheetRawData = new SheetRawData(5);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "", "欄位2", "", "");
            sheetRawData.AddRow("field1.x", "field1.y", "field2.x", "field2.y", "field2.z");
            sheetRawData.AddRow("Vector2Int", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("Vector2Int"));
                Assert.That(typeData.fieldDatas[1].typeData.name, Is.EqualTo("Vector3Int"));
            });
        }

        [Test]
        public void ExportTypeData_Horizontally_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(7);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("欄位1", "", "", "", "欄位2", "", "");
            sheetRawData.AddRow("field1[0].x", "field1[0].y", "field1[1].x", "field1[1].y", "field2[0].x", "field2[0].y", "field2[0].z");
            sheetRawData.AddRow("Vector2Int", "", "", "", "Vector3Int", "", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("Vector2Int"));
                Assert.That(typeData.fieldDatas[1].typeData.name, Is.EqualTo("Vector3Int"));
                Assert.That(typeData.fieldDatas[0].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[1].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[0].arraySize, Is.EqualTo(2));
                Assert.That(typeData.fieldDatas[1].arraySize, Is.EqualTo(1));
            });
        }

        [Test]
        public void ExportTypeData_Horizontally_CustomEnums()
        {
            var sheetRawData = new SheetRawData(1);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("類型");
            sheetRawData.AddRow("type");
            sheetRawData.AddRow("EnemyType");

            var sheetForCustomEnum = new SheetRawData(4);
            sheetForCustomEnum.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetForCustomEnum.AddRow("enum EnemyType", "普通", "菁英", "魔王");
            sheetForCustomEnum.AddRow("", "Normal", "Elite", "Boss");
            sheetForCustomEnum.AddRow("", "0", "1", "2");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetForCustomEnum);
            var parser = new TypeDataParser(customTypeTable);
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(1));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("類型"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("EnemyType"));
            });
        }

        [Test]
        public void ExportTypeData_Horizontally_CustomEnumArrays()
        {
            var sheetRawData = new SheetRawData(1);
            sheetRawData.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetRawData.AddRow("類型");
            sheetRawData.AddRow("types");
            sheetRawData.AddRow("EnemyType[]");

            var sheetForCustomEnum = new SheetRawData(4);
            sheetForCustomEnum.SetMetadata("namespace=Test\nlayout=horizontal");
            sheetForCustomEnum.AddRow("enum EnemyType", "普通", "菁英", "魔王");
            sheetForCustomEnum.AddRow("", "Normal", "Elite", "Boss");
            sheetForCustomEnum.AddRow("", "0", "1", "2");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetForCustomEnum);
            var parser = new TypeDataParser(customTypeTable);
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(1));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("類型"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("EnemyType"));
                Assert.That(typeData.fieldDatas[0].IsArray, Is.EqualTo(true));
            });
        }

        [Test]
        public void ExportTypeData_Vertically_SystemTypes()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1", "int");
            sheetRawData.AddRow("欄位2", "field2", "string");
            sheetRawData.AddRow("欄位3", "field3", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.namespaceName, Is.EqualTo("Test"));
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(3));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[2].comment, Is.EqualTo("欄位3"));
                Assert.That(typeData.fieldDatas[0].typeData, Is.EqualTo(TypeUtility.GetDefaultType("int")));
                Assert.That(typeData.fieldDatas[1].typeData, Is.EqualTo(TypeUtility.GetDefaultType("string")));
                Assert.That(typeData.fieldDatas[2].typeData, Is.EqualTo(TypeUtility.GetDefaultType("bool")));
            });
        }

        [Test]
        public void ExportTypeData_Vertically_SystemTypeArrays()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1", "int[]");
            sheetRawData.AddRow("欄位2", "field2", "bool[]");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData, Is.EqualTo(TypeUtility.GetDefaultType("int")));
                Assert.That(typeData.fieldDatas[1].typeData, Is.EqualTo(TypeUtility.GetDefaultType("bool")));
                Assert.That(typeData.fieldDatas[0].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[1].IsArray, Is.EqualTo(true));
            });
        }

        [Test]
        public void ExportTypeData_Vertically_UnityTypes()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test\nlayout=vertical");
            sheetRawData.AddRow("欄位1", "field1.x", "Vector2Int");
            sheetRawData.AddRow("", "field1.y", "");
            sheetRawData.AddRow("欄位2", "field2.x", "Vector3Int");
            sheetRawData.AddRow("", "field2.y", "");
            sheetRawData.AddRow("", "field2.z", "");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("Vector2Int"));
                Assert.That(typeData.fieldDatas[1].typeData.name, Is.EqualTo("Vector3Int"));
            });
        }

        [Test]
        public void ExportTypeData_Vertically_UnityTypeArrays()
        {
            var sheetRawData = new SheetRawData(3);
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
            Assert.Multiple(() =>
            {
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                Assert.That(typeData.fieldDatas[0].comment, Is.EqualTo("欄位1"));
                Assert.That(typeData.fieldDatas[1].comment, Is.EqualTo("欄位2"));
                Assert.That(typeData.fieldDatas[0].typeData.name, Is.EqualTo("Vector2Int"));
                Assert.That(typeData.fieldDatas[1].typeData.name, Is.EqualTo("Vector3Int"));
                Assert.That(typeData.fieldDatas[0].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[1].IsArray, Is.EqualTo(true));
                Assert.That(typeData.fieldDatas[0].arraySize, Is.EqualTo(2));
                Assert.That(typeData.fieldDatas[1].arraySize, Is.EqualTo(1));
            });
        }
    }
}
