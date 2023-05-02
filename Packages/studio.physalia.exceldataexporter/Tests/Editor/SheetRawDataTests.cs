using NUnit.Framework;
using System;

namespace Physalia.ExcelDataExporter.Tests
{
    public class SheetRawDataTests
    {
        [Test]
        public void Construct()
        {
            var sheetRawData = new SheetRawData(5, 3);
            Assert.AreEqual(5, sheetRawData.RowCount);
            Assert.AreEqual(3, sheetRawData.ColumnCount);
        }

        [Test]
        public void SetRow()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetRow(1, "a", "b", "c");
            Assert.AreEqual("a", sheetRawData.Get(1, 0));
            Assert.AreEqual("b", sheetRawData.Get(1, 1));
            Assert.AreEqual("c", sheetRawData.Get(1, 2));
        }

        [Test]
        public void AddRow()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");

            Assert.AreEqual(2, sheetRawData.RowCount);
            Assert.AreEqual("a", sheetRawData.Get(0, 0));
            Assert.AreEqual("b", sheetRawData.Get(0, 1));
            Assert.AreEqual("c", sheetRawData.Get(0, 2));
            Assert.AreEqual("d", sheetRawData.Get(1, 0));
            Assert.AreEqual("e", sheetRawData.Get(1, 1));
            Assert.AreEqual("f", sheetRawData.Get(1, 2));
        }

        [Test]
        public void RemoveRow()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");
            sheetRawData.RemoveRow(0);

            Assert.AreEqual(1, sheetRawData.RowCount);
            Assert.AreEqual("d", sheetRawData.Get(0, 0));
            Assert.AreEqual("e", sheetRawData.Get(0, 1));
            Assert.AreEqual("f", sheetRawData.Get(0, 2));
        }

        [Test]
        public void RemoveRow_IndexOutOfRange_ThrowsException()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");

            Assert.Catch<IndexOutOfRangeException>(() => sheetRawData.RemoveRow(2));
        }

        [Test]
        public void SetNameAsTest_NameReturnsTest()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetName("Test");
            Assert.AreEqual("Test", sheetRawData.Name);
        }

        [Test]
        public void SetNameAsTest_SetOverrideNameAsTestOverride_NameReturnsTestOverride()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetMetadata("name=TestOverride");
            sheetRawData.SetName("Test");
            Assert.AreEqual("TestOverride", sheetRawData.Name);
        }

        [Test]
        public void ResizeBounds()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetRow(1, "a", "b");
            sheetRawData.ResizeBounds();

            Assert.AreEqual(2, sheetRawData.RowCount);
            Assert.AreEqual(2, sheetRawData.ColumnCount);
        }
    }
}
