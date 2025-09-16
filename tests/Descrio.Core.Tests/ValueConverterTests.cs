using NUnit.Framework;
using Descrio.Parsing;

namespace Descrio.EditorTests
{
    public class ValueConverterTests
    {
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", true)]
        [TestCase("FALSE", false)]
        [TestCase("123", 123L)]
        [TestCase("-45", -45L)]
        [TestCase("123.45", 123.45d)]
        [TestCase("-0.5", -0.5d)]
        [TestCase("0", 0L)]
        [TestCase("hello world", "hello world")]
        [TestCase("123a", "123a")] // Should not be converted
        [TestCase("", "")] // Empty string
        [TestCase(null, null)] // Null input
        public void Convert_ShouldReturnCorrectType(string input, object expected)
        {
            var result = ValueConverter.Convert(input);
            Assert.AreEqual(expected, result);
            if (expected != null)
            {
                Assert.AreEqual(expected.GetType(), result.GetType(), "The converted type was not as expected.");
            }
        }
    }
}