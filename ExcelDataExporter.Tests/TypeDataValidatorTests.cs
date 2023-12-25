using NUnit.Framework;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter.Tests
{
    public class TypeDataValidatorTests
    {
        [Test]
        public void Validate_LegalType()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow();
            sheetRawData.AddRow("Id", "Field2", "Field3");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.That(result.IsValid, Is.EqualTo(true));
        }

        [Test]
        public void Validate_NoIdField()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow();
            sheetRawData.AddRow("Field1", "Field2", "Field3");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.EqualTo(false));
                Assert.That(result.IsIdFieldMissing, Is.EqualTo(true));
            });
        }

        [Test]
        public void Validate_NoIdField_ButIsSettingType()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test\ntype=setting\nlayout=horizontal");
            sheetRawData.AddRow();
            sheetRawData.AddRow("Field1", "Field2", "Field3");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.That(result.IsValid, Is.EqualTo(true));
        }

        [Test]
        public void Validate_IdFieldIsNotInt()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow();
            sheetRawData.AddRow("Id", "Field2", "Field3");
            sheetRawData.AddRow("bool", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.EqualTo(false));
                Assert.That(result.IsIdFieldMissing, Is.EqualTo(true));
            });
        }

        [Test]
        public void Validate_HasDuplicatedNames()
        {
            var sheetRawData = new SheetRawData(3);
            sheetRawData.SetMetadata("namespace=Test");
            sheetRawData.AddRow();
            sheetRawData.AddRow("Id", "Field2", "Field2");
            sheetRawData.AddRow("int", "string", "bool");

            var parser = new TypeDataParser();
            TypeData typeData = parser.ExportTypeData(sheetRawData);

            var validator = new TypeDataValidator();
            TypeDataValidator.Result result = validator.Validate(typeData);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.EqualTo(false));
                Assert.That(result.HasDuplicatedName, Is.EqualTo(true));
            });
        }
    }
}
