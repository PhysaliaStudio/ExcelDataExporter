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
                ClassData classData = customTypeTable.GetClassData("Vector2");
                Assert.AreEqual("Vector2", classData.name);
                Assert.AreEqual(2, classData.fieldDatas.Count);
                Assert.AreEqual("x", classData.fieldDatas[0].name);
                Assert.AreEqual("y", classData.fieldDatas[1].name);
                Assert.AreEqual("int", classData.fieldDatas[0].typeName);
                Assert.AreEqual("int", classData.fieldDatas[1].typeName);
            }
            {
                ClassData classData = customTypeTable.GetClassData("Vector3");
                Assert.AreEqual("Vector3", classData.name);
                Assert.AreEqual(3, classData.fieldDatas.Count);
                Assert.AreEqual("x", classData.fieldDatas[0].name);
                Assert.AreEqual("y", classData.fieldDatas[1].name);
                Assert.AreEqual("z", classData.fieldDatas[2].name);
                Assert.AreEqual("int", classData.fieldDatas[0].typeName);
                Assert.AreEqual("int", classData.fieldDatas[1].typeName);
                Assert.AreEqual("int", classData.fieldDatas[2].typeName);
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
                ClassData classData = customTypeTable.GetClassData("Vector2");
                Assert.AreEqual("Vector2", classData.name);
                Assert.AreEqual(2, classData.fieldDatas.Count);
                Assert.AreEqual("x", classData.fieldDatas[0].name);
                Assert.AreEqual("y", classData.fieldDatas[1].name);
                Assert.AreEqual("int", classData.fieldDatas[0].typeName);
                Assert.AreEqual("int", classData.fieldDatas[1].typeName);
            }
            {
                ClassData classData = customTypeTable.GetClassData("Vector3");
                Assert.AreEqual("Vector3", classData.name);
                Assert.AreEqual(3, classData.fieldDatas.Count);
                Assert.AreEqual("x", classData.fieldDatas[0].name);
                Assert.AreEqual("y", classData.fieldDatas[1].name);
                Assert.AreEqual("z", classData.fieldDatas[2].name);
                Assert.AreEqual("int", classData.fieldDatas[0].typeName);
                Assert.AreEqual("int", classData.fieldDatas[1].typeName);
                Assert.AreEqual("int", classData.fieldDatas[2].typeName);
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
                ClassData classData = customTypeTable.GetClassData("Vector3");
                Assert.AreEqual("Vector3", classData.name);
                Assert.AreEqual(3, classData.fieldDatas.Count);
                Assert.AreEqual("x", classData.fieldDatas[0].name);
                Assert.AreEqual("y", classData.fieldDatas[1].name);
                Assert.AreEqual("z", classData.fieldDatas[2].name);
                Assert.AreEqual("int", classData.fieldDatas[0].typeName);
                Assert.AreEqual("int", classData.fieldDatas[1].typeName);
                Assert.AreEqual("int", classData.fieldDatas[2].typeName);
            }

            LogAssert.Expect(LogType.Error, new Regex(".+"));
        }
    }
}
