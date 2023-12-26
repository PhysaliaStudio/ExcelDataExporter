using NUnit.Framework;
using System;

namespace Physalia.ExcelDataExporter.Loader.Tests
{
    public class SheetRawDataTests
    {
        [Test]
        public void Construct()
        {
            var sheetRawData = new SheetRawData(5, 3);
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.RowCount, Is.EqualTo(5));
                Assert.That(sheetRawData.ColumnCount, Is.EqualTo(3));
            });
        }

        [Test]
        public void SetRow()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetRow(1, "a", "b", "c");
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.Get(1, 0), Is.EqualTo("a"));
                Assert.That(sheetRawData.Get(1, 1), Is.EqualTo("b"));
                Assert.That(sheetRawData.Get(1, 2), Is.EqualTo("c"));
            });
        }

        [Test]
        public void AddRow()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.RowCount, Is.EqualTo(2));
                Assert.That(sheetRawData.Get(0, 0), Is.EqualTo("a"));
                Assert.That(sheetRawData.Get(0, 1), Is.EqualTo("b"));
                Assert.That(sheetRawData.Get(0, 2), Is.EqualTo("c"));
                Assert.That(sheetRawData.Get(1, 0), Is.EqualTo("d"));
                Assert.That(sheetRawData.Get(1, 1), Is.EqualTo("e"));
                Assert.That(sheetRawData.Get(1, 2), Is.EqualTo("f"));
            });
        }

        [Test]
        public void RemoveRow()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");
            sheetRawData.RemoveRow(0);
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.RowCount, Is.EqualTo(1));
                Assert.That(sheetRawData.Get(0, 0), Is.EqualTo("d"));
                Assert.That(sheetRawData.Get(0, 1), Is.EqualTo("e"));
                Assert.That(sheetRawData.Get(0, 2), Is.EqualTo("f"));
            });
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
        public void RemoveColumn()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");

            sheetRawData.RemoveColumn(1);
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.RowCount, Is.EqualTo(2));
                Assert.That(sheetRawData.ColumnCount, Is.EqualTo(2));
                Assert.That(sheetRawData.Get(0, 0), Is.EqualTo("a"));
                Assert.That(sheetRawData.Get(0, 1), Is.EqualTo("c"));
                Assert.That(sheetRawData.Get(1, 0), Is.EqualTo("d"));
                Assert.That(sheetRawData.Get(1, 1), Is.EqualTo("f"));
            });
        }

        [Test]
        public void RemoveColumn_IndexOutOfRange_ThrowsException()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.AddRow("a", "b", "c");
            sheetRawData.AddRow("d", "e", "f");
            Assert.Catch<IndexOutOfRangeException>(() => sheetRawData.RemoveColumn(3));
        }

        [Test]
        public void SetNameAsTest_NameReturnsTest()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetName("Test");
            Assert.That(sheetRawData.Name, Is.EqualTo("Test"));
        }

        [Test]
        public void SetNameAsTest_SetOverrideNameAsTestOverride_NameReturnsTestOverride()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetMetadata("name=TestOverride");
            sheetRawData.SetName("Test");
            Assert.That(sheetRawData.Name, Is.EqualTo("TestOverride"));
        }

        [Test]
        public void ResizeBounds()
        {
            var sheetRawData = new SheetRawData(5, 3);
            sheetRawData.SetRow(1, "a", "b");
            sheetRawData.ResizeBounds();
            Assert.Multiple(() =>
            {
                Assert.That(sheetRawData.RowCount, Is.EqualTo(2));
                Assert.That(sheetRawData.ColumnCount, Is.EqualTo(2));
            });
        }
    }
}
