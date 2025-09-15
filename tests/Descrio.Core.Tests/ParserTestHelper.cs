using NUnit.Framework;
using Descrio.Parsing;
using System.Linq;
using Descrio.Abstractions;

namespace Descrio.EditorTests
{
    public static class ParserTestHelper
    {
        /// <summary>
        /// Parses the text as a single statement, failing the test if any errors occur.
        /// </summary>
        public static IStatement ParseStatementOrFail(this IScriptParser parser, string text)
        {
            var result = parser.ParseStatement(text);

            // Fail the test immediately if there are parsing errors.
            Assert.IsEmpty(result.Errors,
                $"Parsing failed with errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            // Assert that a statement was actually produced.
            Assert.IsNotNull(result.Statement, "Parsing succeeded but produced a null statement.");

            return result.Statement!;
        }

        /// <summary>
        /// Parses the text as a single expression, failing the test if any errors occur.
        /// </summary>
        public static IExpression ParseExpressionOrFail(this IScriptParser parser, string text)
        {
            var result = parser.ParseExpression(text);
            Assert.IsEmpty(result.Errors,
                $"Parsing failed with errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            Assert.IsNotNull(result.Expression, "Parsing succeeded but produced a null expression.");
            return result.Expression!;
        }
    }
}