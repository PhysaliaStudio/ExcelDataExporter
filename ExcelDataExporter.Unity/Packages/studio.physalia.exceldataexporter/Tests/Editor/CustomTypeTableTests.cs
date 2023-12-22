using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

            Assert.AreEqual(2, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.AreEqual(TypeData.Define.Class, typeData.define);
                Assert.AreEqual("Vector2", typeData.name);
                Assert.AreEqual(2, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
                Assert.AreEqual(TypeData.Define.Class, typeData.define);
                Assert.AreEqual("Vector3", typeData.name);
                Assert.AreEqual(3, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("z", typeData.fieldDatas[2].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[2].TypeName);
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

            Assert.AreEqual(2, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.AreEqual(TypeData.Define.Struct, typeData.define);
                Assert.AreEqual("Vector2", typeData.name);
                Assert.AreEqual(2, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
                Assert.AreEqual(TypeData.Define.Struct, typeData.define);
                Assert.AreEqual("Vector3", typeData.name);
                Assert.AreEqual(3, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("z", typeData.fieldDatas[2].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[2].TypeName);
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

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.AreEqual(1, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
                Assert.AreEqual(TypeData.Define.Struct, typeData.define);
                Assert.AreEqual("Vector3", typeData.name);
                Assert.AreEqual(3, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("z", typeData.fieldDatas[2].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[2].TypeName);
            }

            LogAssert.Expect(LogType.Error, new Regex(".+"));
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

            Assert.AreEqual(1, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("EnemyType");
                Assert.AreEqual(TypeData.Define.Enum, typeData.define);
                Assert.AreEqual("EnemyType", typeData.name);
                Assert.AreEqual(3, typeData.fieldDatas.Count);
                Assert.AreEqual("Normal", typeData.fieldDatas[0].name);
                Assert.AreEqual("Elite", typeData.fieldDatas[1].name);
                Assert.AreEqual("Boss", typeData.fieldDatas[2].name);
                Assert.AreEqual(0, typeData.fieldDatas[0].enumValue);
                Assert.AreEqual(1, typeData.fieldDatas[1].enumValue);
                Assert.AreEqual(2, typeData.fieldDatas[2].enumValue);
            }
        }
    }
}
