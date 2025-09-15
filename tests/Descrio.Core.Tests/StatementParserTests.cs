using NUnit.Framework;
using Descrio.Parsing;
using Descrio.Parsing.Yaml;
using System.Linq;
using Descrio.Syntax;
using Descrio.Syntax.Expressions;
using Descrio.Syntax.Statements;

namespace Descrio.EditorTests
{
    [TestFixture]
    public class StatementParserTests
    {
        private readonly IScriptParser _parser = new YamlScriptParser();

        [Test]
        public void ParseStatement_Let_ShouldParseCorrectly()
        {
            var yaml = @"!let
name: my_var
value: 123";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<LetStatement>(statement);
            var let = (LetStatement)statement;
            Assert.AreEqual("my_var", let.Name);
            Assert.IsInstanceOf<LiteralExpression>(let.ValueExpression);
            Assert.AreEqual(123L, ((LiteralExpression)let.ValueExpression).Value);
        }

        [Test]
        public void ParseStatement_Run_ShouldParseCorrectly()
        {
            var yaml = @"!run
name: my_func
args:
  param1: 'hello'
  param2: !expr some_var";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<RunStatement>(statement);
            var run = (RunStatement)statement;
            Assert.AreEqual("my_func", run.Name);
            Assert.AreEqual(2, run.ArgExpressions.Count);

            // param1 is a literal, no change needed
            Assert.IsInstanceOf<LiteralExpression>(run.ArgExpressions["param1"]);

            // param2 is wrapped in EmbeddedExpression
            Assert.IsInstanceOf<EmbeddedExpression>(run.ArgExpressions["param2"]);
            var embeddedExpr = (EmbeddedExpression)run.ArgExpressions["param2"];
            Assert.IsInstanceOf<VariableExpression>(embeddedExpr.InnerExpression);
        }

        [Test]
        public void ParseStatement_Assign_ShouldParseCorrectly()
        {
            var yaml = @"!assign
name: my_var
value: !expr my_var + 1";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<AssignStatement>(statement);
            var assign = (AssignStatement)statement;
            Assert.AreEqual("my_var", ((VariableExpression)assign.Target).VariableName);

            Assert.IsInstanceOf<EmbeddedExpression>(assign.ValueExpression);
            var embeddedExpr = (EmbeddedExpression)assign.ValueExpression;
            Assert.IsInstanceOf<BinaryExpression>(embeddedExpr.InnerExpression);
        }

        [Test]
        public void ParseStatement_For_ShouldParseCorrectly()
        {
            var yaml = @"!for
in: !expr my_list
as: item
statements:
  - !run
    name: log
    args:
      val: !expr item";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<ForStatement>(statement);
            var forStatement = (ForStatement)statement;
            Assert.AreEqual("item", forStatement.VariableName);

            Assert.IsInstanceOf<EmbeddedExpression>(forStatement.EnumerableExpression);
            var embeddedEnumerable = (EmbeddedExpression)forStatement.EnumerableExpression;
            Assert.IsInstanceOf<VariableExpression>(embeddedEnumerable.InnerExpression);

            Assert.AreEqual(1, forStatement.Statements.Length);
            Assert.IsInstanceOf<RunStatement>(forStatement.Statements[0]);

            var runStmt = (RunStatement)forStatement.Statements[0];
            Assert.IsInstanceOf<EmbeddedExpression>(runStmt.ArgExpressions["val"]);
            var embeddedArg = (EmbeddedExpression)runStmt.ArgExpressions["val"];
            Assert.IsInstanceOf<VariableExpression>(embeddedArg.InnerExpression);
        }

        [Test]
        public void ParseStatement_When_ShouldParseCorrectly()
        {
            var yaml = @"!when
cases:
  - condition: !expr x > 10
    then:
      - !run
        name: log
        args: { val: 'greater' }
  - then:
      - !run
        name: log
        args: { val: 'less or equal' }";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<WhenStatement>(statement);
            var when = (WhenStatement)statement;
            Assert.AreEqual(2, when.Cases.Length);

            Assert.IsInstanceOf<EmbeddedExpression>(when.Cases[0].Condition);
            var embeddedCondition = (EmbeddedExpression)when.Cases[0].Condition;
            Assert.IsInstanceOf<BinaryExpression>(embeddedCondition.InnerExpression);

            Assert.IsNull(when.Cases[1].Condition); // else case
        }

        [Test]
        public void ParseStatement_Function_ShouldParseCorrectly()
        {
            var yaml = @"!function
name: my_func
parameters:
  - name: p1
    type: string
    default: 'default'
statements:
  - !run
    name: log
    args:
      val: !expr p1";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<FunctionStatement>(statement);
            var func = (FunctionStatement)statement;
            Assert.AreEqual("my_func", func.Name);
            Assert.AreEqual(1, func.Parameters.Length);
            Assert.AreEqual("p1", func.Parameters[0].Name);
            Assert.AreEqual("string", func.Parameters[0].Type);
            Assert.AreEqual("default", func.Parameters[0].DefaultValue);
            Assert.AreEqual(1, func.Statements.Length);

            Assert.IsInstanceOf<RunStatement>(func.Statements[0]);
            var runStmt = (RunStatement)func.Statements[0];
            Assert.IsInstanceOf<EmbeddedExpression>(runStmt.ArgExpressions["val"]);
            var embeddedArg = (EmbeddedExpression)runStmt.ArgExpressions["val"];
            Assert.IsInstanceOf<VariableExpression>(embeddedArg.InnerExpression);
        }

        [Test]
        public void ParseExpression_Lambda_ShouldParseCorrectly()
        {
            var yaml = @"!lambda
parameters:
  - name: p1
    type: string
    default: 'default'
statements:
  - !run
    name: log
    args:
      val: !expr p1";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<LambdaExpression>(expression);
            var lambda = (LambdaExpression)expression;
            Assert.AreEqual(1, lambda.Parameters.Length);
            Assert.AreEqual("p1", lambda.Parameters[0].Name);
            Assert.AreEqual("string", lambda.Parameters[0].Type);
            Assert.AreEqual("default", lambda.Parameters[0].DefaultValue);
            Assert.AreEqual(1, lambda.Statements.Length);

            Assert.IsInstanceOf<RunStatement>(lambda.Statements[0]);
            var runStmt = (RunStatement)lambda.Statements[0];
            Assert.IsInstanceOf<EmbeddedExpression>(runStmt.ArgExpressions["val"]);
            var embeddedArg = (EmbeddedExpression)runStmt.ArgExpressions["val"];
            Assert.IsInstanceOf<VariableExpression>(embeddedArg.InnerExpression);
        }

        [Test]
        public void ParseStatement_TryCatch_ShouldParseCorrectly()
        {
            var yaml = @"!try
statements:
  - !throw
    name: MyError
catch:
  - name: MyError
    as: e
    then:
      - !run
        name: log
        args: { val: !expr e } ";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<TryCatchStatement>(statement);
            var tryCatch = (TryCatchStatement)statement;
            Assert.AreEqual(1, tryCatch.TryBlock.Length);
            Assert.IsInstanceOf<ThrowStatement>(tryCatch.TryBlock[0]);
            Assert.AreEqual(1, tryCatch.CatchClauses.Count);
            Assert.AreEqual("MyError", tryCatch.CatchClauses[0].ErrorName);
            Assert.AreEqual("e", tryCatch.CatchClauses[0].VariableName);

            var catchRun = (RunStatement)tryCatch.CatchClauses[0].ThenBlock[0];
            Assert.IsInstanceOf<EmbeddedExpression>(catchRun.ArgExpressions["val"]);
            var embeddedArg = (EmbeddedExpression)catchRun.ArgExpressions["val"];
            Assert.IsInstanceOf<VariableExpression>(embeddedArg.InnerExpression);
        }

        [Test]
        public void ParseStatement_Match_ShouldParseCorrectly()
        {
            var yaml = @"!match
value: !expr my_value
cases:
  - case: 1
    then:
      - !run { name: log, args: { val: 1 } }
  - case: 2
    then:
      - !run { name: log, args: { val: 2 } }
default:
  - !run { name: log, args: { val: 'default' } }";
            var statement = _parser.ParseStatementOrFail(yaml);

            Assert.IsInstanceOf<MatchStatement>(statement);
            var match = (MatchStatement)statement;

            Assert.IsInstanceOf<EmbeddedExpression>(match.ValueExpression);
            var embeddedValue = (EmbeddedExpression)match.ValueExpression;
            Assert.IsInstanceOf<VariableExpression>(embeddedValue.InnerExpression);

            Assert.AreEqual(2, match.Cases.Count);
            Assert.AreEqual(1L, match.Cases[0].CaseValue);
            Assert.IsInstanceOf<RunStatement>(match.DefaultBlock[0]);
        }

        [Test]
        public void ParseStatement_SimpleStatements_ShouldParseCorrectly()
        {
            Assert.IsInstanceOf<BreakStatement>(_parser.ParseStatementOrFail("!break"));
            Assert.IsInstanceOf<ContinueStatement>(_parser.ParseStatementOrFail("!continue"));
            Assert.IsInstanceOf<ReturnStatement>(_parser.ParseStatementOrFail("!return"));
        }

        [Test]
        public void ParseExpression_Literal_ShouldParseCorrectly()
        {
            var yaml = "'hello world'";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<LiteralExpression>(expression);
            Assert.AreEqual("hello world", ((LiteralExpression)expression).Value);
        }

        [Test]
        public void ParseExpression_Variable_ShouldParseCorrectly()
        {
            var yaml = "!expr my_variable";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<EmbeddedExpression>(expression);
            var embedded = (EmbeddedExpression)expression;

            Assert.IsInstanceOf<VariableExpression>(embedded.InnerExpression);
            Assert.AreEqual("my_variable", ((VariableExpression)embedded.InnerExpression).VariableName);
        }

        [Test]
        public void ParseExpression_List_ShouldParseCorrectly()
        {
            var yaml = "[1, 'two', !expr three]";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<ListExpression>(expression);
            var list = (ListExpression)expression;
            Assert.AreEqual(3, list.Elements.Count);
            Assert.IsInstanceOf<LiteralExpression>(list.Elements[0]);
            Assert.IsInstanceOf<LiteralExpression>(list.Elements[1]);

            Assert.IsInstanceOf<EmbeddedExpression>(list.Elements[2]);
            var embedded = (EmbeddedExpression)list.Elements[2];
            Assert.IsInstanceOf<VariableExpression>(embedded.InnerExpression);
        }

        [Test]
        public void ParseExpression_Dictionary_ShouldParseCorrectly()
        {
            var yaml = @"{
  key1: 'value1',
  123: 456,
  !expr my_var: !expr my_val
}";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<DictionaryExpression>(expression);
            var dict = (DictionaryExpression)expression;
            Assert.AreEqual(3, dict.Entries.Count);

            var entry1 = dict.Entries.Single(kvp => kvp.Key is LiteralExpression l && l.Value.Equals("key1"));
            Assert.IsInstanceOf<LiteralExpression>(entry1.Value);
            Assert.AreEqual("value1", ((LiteralExpression)entry1.Value).Value);

            var entry2 = dict.Entries.Single(kvp => kvp.Key is LiteralExpression l && l.Value.Equals(123L));
            Assert.IsInstanceOf<LiteralExpression>(entry2.Value);
            Assert.AreEqual(456L, ((LiteralExpression)entry2.Value).Value);

            var entry3 = dict.Entries.Single(kvp => kvp.Key is EmbeddedExpression);
            var keyEmbedded = (EmbeddedExpression)entry3.Key;
            Assert.IsInstanceOf<VariableExpression>(keyEmbedded.InnerExpression);
            Assert.AreEqual("my_var", ((VariableExpression)keyEmbedded.InnerExpression).VariableName);

            var valueEmbedded = (EmbeddedExpression)entry3.Value;
            Assert.IsInstanceOf<VariableExpression>(valueEmbedded.InnerExpression);
            Assert.AreEqual("my_val", ((VariableExpression)valueEmbedded.InnerExpression).VariableName);
        }

        [Test]
        public void ParseExpression_MemberAccess_ShouldParseCorrectly()
        {
            var yaml = "!expr my_obj.my_prop.my_field";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<EmbeddedExpression>(expression);
            var embedded = (EmbeddedExpression)expression;
            Assert.IsInstanceOf<MemberAccessExpression>(embedded.InnerExpression);
            var memberAccess = (MemberAccessExpression)embedded.InnerExpression;
            Assert.AreEqual("my_field", memberAccess.MemberName);
            Assert.IsInstanceOf<MemberAccessExpression>(memberAccess.ObjectExpression);
        }

        [Test]
        public void ParseExpression_Binary_ShouldParseCorrectly()
        {
            var yaml = "!expr 1 + 2";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<EmbeddedExpression>(expression);
            var embedded = (EmbeddedExpression)expression;
            Assert.IsInstanceOf<BinaryExpression>(embedded.InnerExpression);
            var binary = (BinaryExpression)embedded.InnerExpression;
            Assert.AreEqual(OperatorType.Add, binary.OperatorType);
            Assert.IsInstanceOf<LiteralExpression>(binary.Left);
            Assert.IsInstanceOf<LiteralExpression>(binary.Right);
        }

        [Test]
        public void ParseExpression_Unary_ShouldParseCorrectly()
        {
            var yaml = "!expr '!my_bool'";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<EmbeddedExpression>(expression);
            var embedded = (EmbeddedExpression)expression;
            Assert.IsInstanceOf<UnaryExpression>(embedded.InnerExpression);
            var unary = (UnaryExpression)embedded.InnerExpression;
            Assert.AreEqual(OperatorType.Not, unary.OperatorType);
            Assert.IsInstanceOf<VariableExpression>(unary.Operand);
        }

        [Test]
        public void ParseExpression_InterpolatedString_ShouldParseCorrectly()
        {
            var yaml = "!expr '\"Hello ${my_name}!\"'";
            var expression = _parser.ParseExpressionOrFail(yaml);

            Assert.IsInstanceOf<EmbeddedExpression>(expression);
            var embedded = (EmbeddedExpression)expression;
            Assert.IsInstanceOf<InterpolatedStringExpression>(embedded.InnerExpression);
            var interpolated = (InterpolatedStringExpression)embedded.InnerExpression;
            Assert.AreEqual(3, interpolated.Parts.Count);
            Assert.AreEqual("Hello ", ((LiteralExpression)interpolated.Parts[0]).Value);
            Assert.IsInstanceOf<VariableExpression>(interpolated.Parts[1]);
            Assert.AreEqual("!", ((LiteralExpression)interpolated.Parts[2]).Value);
        }
    }
}