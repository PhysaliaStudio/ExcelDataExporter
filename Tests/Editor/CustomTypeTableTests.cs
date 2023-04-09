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
            var sheetRawData = new SheetRawData(4, 4);
            sheetRawData.SetRow(0, "Vector2", "x", "y");
            sheetRawData.SetRow(1, "", "int", "int");
            sheetRawData.SetRow(2, "Vector3", "x", "y", "z");
            sheetRawData.SetRow(3, "", "int", "int", "int");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.AreEqual(2, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.AreEqual("Vector2", typeData.name);
                Assert.AreEqual(2, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
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
            var sheetRawData = new SheetRawData(5, 4);
            sheetRawData.SetRow(0, "Vector2", "x", "y");
            sheetRawData.SetRow(1, "", "int", "int");
            sheetRawData.SetRow(2);
            sheetRawData.SetRow(3, "Vector3", "x", "y", "z");
            sheetRawData.SetRow(4, "", "int", "int", "int");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.AreEqual(2, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector2");
                Assert.AreEqual("Vector2", typeData.name);
                Assert.AreEqual(2, typeData.fieldDatas.Count);
                Assert.AreEqual("x", typeData.fieldDatas[0].name);
                Assert.AreEqual("y", typeData.fieldDatas[1].name);
                Assert.AreEqual("int", typeData.fieldDatas[0].TypeName);
                Assert.AreEqual("int", typeData.fieldDatas[1].TypeName);
            }
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
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
            var sheetRawData = new SheetRawData(5, 4);
            sheetRawData.SetRow(0, "Vector2", "x", "y");
            sheetRawData.SetRow(1, "", "int", "");  // Missing type name, which is invalid.
            sheetRawData.SetRow(2);
            sheetRawData.SetRow(3, "Vector3", "x", "y", "z");
            sheetRawData.SetRow(4, "", "int", "int", "int");

            CustomTypeTable customTypeTable = CustomTypeTable.Parse(sheetRawData);

            Assert.AreEqual(1, customTypeTable.Count);
            {
                TypeData typeData = customTypeTable.GetTypeData("Vector3");
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
