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
            Assert.AreEqual(expected, TypeUtility.ParseBool(text));
        }

        [TestCase(59, "59")]
        [TestCase(42, "0042")]
        [TestCase(76, "+0076")]
        [TestCase(-76, "-0076")]
        public void ParseIntTest(int expected, string text)
        {
            Assert.AreEqual(expected, TypeUtility.ParseInt(text));
        }

        [TestCase(0, "0b0000")]
        [TestCase(3, "0b0011")]
        [TestCase(15, "1111")]
        public void ParseIntWithBinaryTest(int expected, string text)
        {
            Assert.AreEqual(expected, TypeUtility.ParseIntWithBinary(text));
        }
    }
}
