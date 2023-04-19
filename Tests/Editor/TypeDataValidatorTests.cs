using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class TypeDataValidatorTests
    {
        [Test]
        public void Validate_LegalType()
        {
            var sheetRawData = new SheetRawData(2, 3);
            sheetRawData.SetRow(0, "Id", "Field2", "Field3");
            sheetRawData.SetRow(1, "int", "string", "bool");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.AreEqual(true, result.IsValid);
        }

        [Test]
        public void Validate_NoIdField()
        {
            var sheetRawData = new SheetRawData(2, 3);
            sheetRawData.SetRow(0, "Field1", "Field2", "Field3");
            sheetRawData.SetRow(1, "int", "string", "bool");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.AreEqual(false, result.IsValid);
            Assert.AreEqual(false, result.HasIntIdField);
        }

        [Test]
        public void Validate_IdFieldIsNotInt()
        {
            var sheetRawData = new SheetRawData(2, 3);
            sheetRawData.SetRow(0, "Id", "Field2", "Field3");
            sheetRawData.SetRow(1, "bool", "string", "bool");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.AreEqual(false, result.IsValid);
            Assert.AreEqual(false, result.HasIntIdField);
        }

        [Test]
        public void Validate_HasDuplicatedNames()
        {
            var sheetRawData = new SheetRawData(2, 3);
            sheetRawData.SetRow(0, "Id", "Field2", "Field2");
            sheetRawData.SetRow(1, "int", "string", "bool");

            var parser = new SheetParser();
            TypeData typeData = parser.ExportTypeData("", sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.AreEqual(false, result.IsValid);
            Assert.AreEqual(false, result.HasNoDuplicatedName);
        }
    }
}