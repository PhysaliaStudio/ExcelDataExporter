using NUnit.Framework;

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