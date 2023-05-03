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
    }
}
