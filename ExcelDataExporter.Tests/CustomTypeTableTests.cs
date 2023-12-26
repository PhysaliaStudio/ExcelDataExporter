using System;
using NUnit.Framework;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter.Tests
{
    public class CustomTypeTableTests
    {
        [Test]
        public void ParseToTypeTable()
        {
            var sheetRawData = new SheetRawData(4);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow("class Vector2", "", "");
            sheetRawData.AddRow("", "x", "y");
            sheetRawData.AddRow("", "int", "int");
            sheetRawData.AddRow("class Vector3", "", "", "");
            sheetRawData.AddRow("", "x", "y", "z");
            sheetRawData.AddRow("", "int", "int", "int");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.That(customTypeTable.Count, Is.EqualTo(2));
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.Multiple(() =>
                {
                    Assert.That(typeData.define, Is.EqualTo(TypeData.Define.Class));
                    Assert.That(typeData.name, Is.EqualTo("Vector2"));
                    Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                    Assert.That(typeData.fieldDatas[0].name, Is.EqualTo("x"));
                    Assert.That(typeData.fieldDatas[1].name, Is.EqualTo("y"));
                    Assert.That(typeData.fieldDatas[0].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[1].TypeName, Is.EqualTo("int"));
                });
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
                Assert.Multiple(() =>
                {
                    Assert.That(typeData.define, Is.EqualTo(TypeData.Define.Class));
                    Assert.That(typeData.name, Is.EqualTo("Vector3"));
                    Assert.That(typeData.fieldDatas, Has.Count.EqualTo(3));
                    Assert.That(typeData.fieldDatas[0].name, Is.EqualTo("x"));
                    Assert.That(typeData.fieldDatas[1].name, Is.EqualTo("y"));
                    Assert.That(typeData.fieldDatas[2].name, Is.EqualTo("z"));
                    Assert.That(typeData.fieldDatas[0].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[1].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[2].TypeName, Is.EqualTo("int"));
                });
            }
        }

        [Test]
        public void ParseToTypeTable_ContainsEmptyRow()
        {
            var sheetRawData = new SheetRawData(4);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow("struct Vector2", "", "");
            sheetRawData.AddRow("", "x", "y");
            sheetRawData.AddRow("", "int", "int");
            sheetRawData.AddRow();
            sheetRawData.AddRow("struct Vector3", "", "", "");
            sheetRawData.AddRow("", "x", "y", "z");
            sheetRawData.AddRow("", "int", "int", "int");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.That(customTypeTable.Count, Is.EqualTo(2));
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.Multiple(() =>
                {
                    Assert.That(typeData.define, Is.EqualTo(TypeData.Define.Struct));
                    Assert.That(typeData.name, Is.EqualTo("Vector2"));
                    Assert.That(typeData.fieldDatas, Has.Count.EqualTo(2));
                    Assert.That(typeData.fieldDatas[0].name, Is.EqualTo("x"));
                    Assert.That(typeData.fieldDatas[1].name, Is.EqualTo("y"));
                    Assert.That(typeData.fieldDatas[0].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[1].TypeName, Is.EqualTo("int"));
                });
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
                Assert.Multiple(() =>
                {
                    Assert.That(typeData.define, Is.EqualTo(TypeData.Define.Struct));
                    Assert.That(typeData.name, Is.EqualTo("Vector3"));
                    Assert.That(typeData.fieldDatas, Has.Count.EqualTo(3));
                    Assert.That(typeData.fieldDatas[0].name, Is.EqualTo("x"));
                    Assert.That(typeData.fieldDatas[1].name, Is.EqualTo("y"));
                    Assert.That(typeData.fieldDatas[2].name, Is.EqualTo("z"));
                    Assert.That(typeData.fieldDatas[0].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[1].TypeName, Is.EqualTo("int"));
                    Assert.That(typeData.fieldDatas[2].TypeName, Is.EqualTo("int"));
                });
            }
        }

        [Test]
        public void ParseToTypeTable_ContainsInvalidData()
        {
            var sheetRawData = new SheetRawData(4);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow("struct Vector2", "", "");
            sheetRawData.AddRow("", "x", "y");
            sheetRawData.AddRow("", "int", "");  // Missing type name, which is invalid.
            sheetRawData.AddRow();
            sheetRawData.AddRow("struct Vector3", "", "", "");
            sheetRawData.AddRow("", "x", "y", "z");
            sheetRawData.AddRow("", "int", "int", "int");

            Assert.Catch<Exception>(() =>
            {
                _ = CustomTypeTable.Parse(sheetRawData);
            });
        }

        [Test]
        public void ParseToTypeTable_ContainsEnum()
        {
            var sheetRawData = new SheetRawData(4);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow("enum EnemyType", "普通", "菁英", "魔王");
            sheetRawData.AddRow("", "Normal", "Elite", "Boss");
            sheetRawData.AddRow("", "0", "1", "2");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.That(customTypeTable.Count, Is.EqualTo(1));
            TypeData typeData = customTypeTable.GetTypeData("EnemyType");
            Assert.Multiple(() =>
            {
                Assert.That(typeData.define, Is.EqualTo(TypeData.Define.Enum));
                Assert.That(typeData.name, Is.EqualTo("EnemyType"));
                Assert.That(typeData.fieldDatas, Has.Count.EqualTo(3));
                Assert.That(typeData.fieldDatas[0].name, Is.EqualTo("Normal"));
                Assert.That(typeData.fieldDatas[1].name, Is.EqualTo("Elite"));
                Assert.That(typeData.fieldDatas[2].name, Is.EqualTo("Boss"));
                Assert.That(typeData.fieldDatas[0].enumValue, Is.EqualTo(0));
                Assert.That(typeData.fieldDatas[1].enumValue, Is.EqualTo(1));
                Assert.That(typeData.fieldDatas[2].enumValue, Is.EqualTo(2));
            });
        }
    }
}
