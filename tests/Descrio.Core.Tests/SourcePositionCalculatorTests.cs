using NUnit.Framework;
using Descrio.Parsing.Utils;

namespace Descrio.EditorTests
{
    [TestFixture]
    public class SourcePositionCalculatorTests
    {
        // Test data is now defined in a much more readable format.
        private static IEnumerable<TestCaseData> LocationTestCases
        {
            get
            {
                yield return new TestCaseData("abc", 3, (1, 4), (1, 3))
                    .SetName("GetPreviousCharacterLocation_SameLine_Simple");

                yield return new TestCaseData("abc\ndef", 4, (2, 1), (1, 3))
                    .SetName("GetPreviousCharacterLocation_MovesToPreviousLine_WithContent");

                yield return new TestCaseData("line1\n  line2\n\tline3", 14, (3, 1), (2, 7))
                    .SetName("GetPreviousCharacterLocation_MovesToPreviousLine_WithLeadingWhitespace");

                yield return new TestCaseData("a\n\nc", 3, (3, 1), (2, 0))
                    .SetName("GetPreviousCharacterLocation_MovesToPreviousLine_WhichIsEmpty");

                yield return new TestCaseData("a", 1, (1, 2), (1, 1))
                    .SetName("GetPreviousCharacterLocation_EndOfFirstLine");

                yield return new TestCaseData("", 0, (1, 1), (1, 1))
                    .SetName("GetPreviousCharacterLocation_EdgeCase_EmptyContent");

                yield return new TestCaseData("a", 0, (1, 1), (1, 1))
                    .SetName("GetPreviousCharacterLocation_EdgeCase_StartOfFile");

                yield return new TestCaseData("abc\r\ndef", 5, (2, 1), (1, 3))
                    .SetName("GetPreviousLocation_MovesToPrevLine_CRLF");

                yield return new TestCaseData("a\r\n\r\nc", 5, (3, 1), (2, 0))
                    .SetName("GetPreviousLocation_MovesToEmptyPrevLine_CRLF");
            }
        }

        [TestCaseSource(nameof(LocationTestCases))]
        public void TestGetPreviousCharacterLocation(
            string content, int index, (int line, int col) position, (int line, int col) expectedPos)
        {
            // Act
            var (actualLine, actualColumn) = SourcePositionCalculator.GetPreviousCharacterLocation(
                content, index, position.line, position.col);

            // Assert
            Assert.AreEqual(expectedPos.line, actualLine, "Line mismatch");
            Assert.AreEqual(expectedPos.col, actualColumn, "Column mismatch");
        }
    }
}