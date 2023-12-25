using NUnit.Framework;

namespace Physalia.ExcelDataExporter.Tests
{
    public class TypeUtilityTests
    {
        [TestCase(false, "")]
        [TestCase(false, "0")]
        [TestCase(true, "1")]
        [TestCase(false, "false")]
        [TestCase(true, "true")]
        [TestCase(false, "FALSE")]
        [TestCase(true, "TRUE")]
        [TestCase(false, "False")]
        [TestCase(true, "True")]
        public void ParseBoolTest(bool expected, string text)
        {
            Assert.That(TypeUtility.ParseBool(text), Is.EqualTo(expected));
        }

        [TestCase(59, "59")]
        [TestCase(42, "0042")]
        [TestCase(76, "+0076")]
        [TestCase(-76, "-0076")]
        public void ParseIntTest(int expected, string text)
        {
            Assert.That(TypeUtility.ParseInt(text), Is.EqualTo(expected));
        }
    }
}
