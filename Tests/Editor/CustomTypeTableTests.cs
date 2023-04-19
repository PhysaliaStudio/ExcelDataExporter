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
            var sheetRawData = new SheetRawData(5, 4);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "class Vector2", "x", "y");
            sheetRawData.SetRow(2, "", "int", "int");
            sheetRawData.SetRow(3, "class Vector3", "x", "y", "z");
            sheetRawData.SetRow(4, "", "int", "int", "int");

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
            var sheetRawData = new SheetRawData(6, 4);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "struct Vector2", "x", "y");
            sheetRawData.SetRow(2, "", "int", "int");
            sheetRawData.SetRow(3);
            sheetRawData.SetRow(4, "struct Vector3", "x", "y", "z");
            sheetRawData.SetRow(5, "", "int", "int", "int");

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
            var sheetRawData = new SheetRawData(6, 4);
            sheetRawData.SetRow(0, "namespace=Test");
            sheetRawData.SetRow(1, "struct Vector2", "x", "y");
            sheetRawData.SetRow(2, "", "int", "");  // Missing type name, which is invalid.
            sheetRawData.SetRow(3);
            sheetRawData.SetRow(4, "struct Vector3", "x", "y", "z");
            sheetRawData.SetRow(5, "", "int", "int", "int");

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
    }
}
