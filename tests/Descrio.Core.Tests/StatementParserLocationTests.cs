using NUnit.Framework;
using Descrio.Parsing;
using Descrio.Parsing.Yaml;
using Descrio.Data;
using System.Linq;
using Descrio.Syntax.Expressions;
using Descrio.Syntax.Statements;

namespace Descrio.EditorTests
{
    /// <summary>
    /// Contains tests specifically for verifying the SourceRange (Location) of parsed statements and their components.
    /// These tests are crucial for ensuring that error reporting, debugging, and tooling features work correctly.
    /// </summary>
    [TestFixture]
    public class StatementParserLocationTests
    {
        private readonly IScriptParser _parser = new YamlScriptParser();

        [Test]
        public void Location_SimpleLetStatement_ShouldHaveCorrectSpan()
        {
            // The statement is on a single line, which is a simple case.
            var yaml = "!let { name: my_var, value: 123 }";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<LetStatement>(statement);
            // Location should span the entire line.
            var expected = new SourceRange(1, 1, 1, 33);
            Assert.AreEqual(expected, statement.Location);
        }

        [Test]
        public void Location_MultiLineLetStatement_ShouldHaveCorrectSpan()
        {
            var yaml = @"
# This is a comment, should be ignored.
!let
  name: my_var
  value: 123
# Another comment
";
            // The parser should trim leading/trailing whitespace lines.
            // The relevant content starts at line 3 and ends at line 5.
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<LetStatement>(statement);
            // The location should span from the start of the tag "!let"
            // to the end of the value "123".
            var expected = new SourceRange(3, 1, 5, 12);
            Assert.AreEqual(expected, statement.Location);
        }

        [Test]
        public void Location_WhenStatement_AndItsComponents_ShouldHaveCorrectSpans()
        {
            var yaml = @"
!when
  cases:
    - # Case 1
      condition: !expr x > 10
      then:
        - !run
          name: foo
    - # Case 2 (default)
      then:
        - !run
          name: bar
";
            var statement = _parser.ParseStatementOrFail(yaml);
            Assert.IsInstanceOf<WhenStatement>(statement);
            var whenStmt = (WhenStatement)statement;

            // Test the location of the entire !when statement.
            // It should span from "!when" on line 1 to the end of the last run statement on line 12.
            Assert.AreEqual(new SourceRange(2, 1, 12, 19), whenStmt.Location, "WhenStatement location is wrong.");

            // --- Verify Case 1 ---
            var firstCase = whenStmt.Cases[0];
            // It should span from the sequence indicator "-" on line 3
            // to the end of its inner run statement on line 8.
            Assert.AreEqual(new SourceRange(5, 7, 8, 19), firstCase.Location, "First WhenCaseBlock location is wrong.");

            // Check the location of the condition expression inside Case 1.
            // It's just the "!expr" node.
            Assert.IsNotNull(firstCase.Condition, "Condition should not be null.");
            Assert.AreEqual(new SourceRange(5, 18, 5, 29), firstCase.Condition.Location, "Condition expression location is wrong.");

            // Check the location of the statement inside Case 1's "then" block.
            var firstRun = firstCase.ThenBlock[0];
            Assert.AreEqual(new SourceRange(7, 11, 8, 19), firstRun.Location, "Inner RunStatement in Case 1 is wrong.");

            // --- Verify Case 2 (the default case) ---
            var secondCase = whenStmt.Cases[1];
            // It should span from its sequence indicator "-" on line 9
            // to the end of its inner run statement on line 12.
            Assert.AreEqual(new SourceRange(10, 7, 12, 19), secondCase.Location, "Second WhenCaseBlock location is wrong.");

            // The default case has no condition.
            Assert.IsNull(secondCase.Condition, "Default case should have a null condition.");
        }

        [Test]
        public void Location_EmptyStatementList_ShouldHaveCorrectSpan()
        {
            var yaml = @"
!for
  in: []
  as: item
  statements: [] # Empty block
";
            var statement = _parser.ParseStatementOrFail(yaml);
            Assert.IsInstanceOf<ForStatement>(statement);

            // The location should span the entire node definition.
            var expected = new SourceRange(2, 1, 5, 16);
            Assert.AreEqual(expected, statement.Location);
        }

        [Test]
        public void Location_MultiLineExpression_ShouldHaveCorrectSpansForGlobalAndLocalCoordinates()
        {
            var yaml = @"
!let
  name: result
  value: !expr |
    x +
      y
";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<LetStatement>(statement);
            var letStmt = (LetStatement)statement;
            Assert.AreEqual(new SourceRange(2, 1, 6, 7), letStmt.Location, "Outer LetStatement location is wrong.");

            Assert.IsInstanceOf<EmbeddedExpression>(letStmt.ValueExpression);
            var embeddedExpr = (EmbeddedExpression)letStmt.ValueExpression;
            Assert.AreEqual(new SourceRange(4, 10, 6, 7), embeddedExpr.Location, "EmbeddedExpression wrapper location is wrong.");

            var innerExpr = embeddedExpr.InnerExpression;
            Assert.IsInstanceOf<BinaryExpression>(innerExpr);
            Assert.AreEqual(new SourceRange(1, 1, 2, 3), innerExpr.Location, "Inner expression's local location is wrong.");
        }
    }
}