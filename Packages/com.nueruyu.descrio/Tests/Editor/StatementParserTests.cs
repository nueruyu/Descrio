using NUnit.Framework;
using Descrio.Parse;
using Descrio.Yaml;
using System.Linq;

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
            var statement = _parser.ParseStatement(yaml);

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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<RunStatement>(statement);
            var run = (RunStatement)statement;
            Assert.AreEqual("my_func", run.Name);
            Assert.AreEqual(2, run.ArgExpressions.Count);
            Assert.IsInstanceOf<LiteralExpression>(run.ArgExpressions["param1"]);
            Assert.IsInstanceOf<VariableExpression>(run.ArgExpressions["param2"]);
        }

        [Test]
        public void ParseStatement_Assign_ShouldParseCorrectly()
        {
            var yaml = @"!assign
name: my_var
value: !expr my_var + 1";
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<AssignStatement>(statement);
            var assign = (AssignStatement)statement;
            Assert.AreEqual("my_var", ((VariableExpression)assign.Target).VariableName);
            Assert.IsInstanceOf<BinaryExpression>(assign.ValueExpression);
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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<ForStatement>(statement);
            var forStatement = (ForStatement)statement;
            Assert.AreEqual("item", forStatement.VariableName);
            Assert.IsInstanceOf<VariableExpression>(forStatement.EnumerableExpression);
            Assert.AreEqual(1, forStatement.Statements.Length);
            Assert.IsInstanceOf<RunStatement>(forStatement.Statements[0]);
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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<WhenStatement>(statement);
            var when = (WhenStatement)statement;
            Assert.AreEqual(2, when.Cases.Length);
            Assert.IsInstanceOf<BinaryExpression>(when.Cases[0].Condition);
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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<FunctionStatement>(statement);
            var func = (FunctionStatement)statement;
            Assert.AreEqual("my_func", func.Name);
            Assert.AreEqual(1, func.Parameters.Length);
            Assert.AreEqual("p1", func.Parameters[0].Name);
            Assert.AreEqual("string", func.Parameters[0].Type);
            Assert.AreEqual("default", func.Parameters[0].DefaultValue);
            Assert.AreEqual(1, func.Statements.Length);
            Assert.IsInstanceOf<RunStatement>(func.Statements[0]);
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
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<LambdaExpression>(expression);
            var lambda = (LambdaExpression)expression;
            Assert.AreEqual(1, lambda.Parameters.Length);
            Assert.AreEqual("p1", lambda.Parameters[0].Name);
            Assert.AreEqual("string", lambda.Parameters[0].Type);
            Assert.AreEqual("default", lambda.Parameters[0].DefaultValue);
            Assert.AreEqual(1, lambda.Statements.Length);
            Assert.IsInstanceOf<RunStatement>(lambda.Statements[0]);
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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<TryCatchStatement>(statement);
            var tryCatch = (TryCatchStatement)statement;
            Assert.AreEqual(1, tryCatch.TryBlock.Length);
            Assert.IsInstanceOf<ThrowStatement>(tryCatch.TryBlock[0]);
            Assert.AreEqual(1, tryCatch.CatchClauses.Count);
            Assert.AreEqual("MyError", tryCatch.CatchClauses[0].ErrorName);
            Assert.AreEqual("e", tryCatch.CatchClauses[0].VariableName);
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
            var statement = _parser.ParseStatement(yaml);

            Assert.IsInstanceOf<MatchStatement>(statement);
            var match = (MatchStatement)statement;
            Assert.IsInstanceOf<VariableExpression>(match.ValueExpression);
            Assert.AreEqual(2, match.Cases.Count);
            Assert.AreEqual(1L, match.Cases[0].CaseValue);
            Assert.IsInstanceOf<RunStatement>(match.DefaultBlock[0]);
        }

        [Test]
        public void ParseStatement_SimpleStatements_ShouldParseCorrectly()
        {
            Assert.IsInstanceOf<BreakStatement>(_parser.ParseStatement("!break"));
            Assert.IsInstanceOf<ContinueStatement>(_parser.ParseStatement("!continue"));
            Assert.IsInstanceOf<ReturnStatement>(_parser.ParseStatement("!return"));
        }

        [Test]
        public void ParseExpression_Literal_ShouldParseCorrectly()
        {
            var yaml = "'hello world'";
            var expression = _parser.ParseExpression(yaml);

            // The user's intuition is correct. During deserialization, this becomes a C# string first.
            // Then, the ExpressionNodeDeserializer wraps it in a LiteralNode,
            // which in turn becomes a LiteralExpression. So the test is correct.
            Assert.IsInstanceOf<LiteralExpression>(expression);
            Assert.AreEqual("hello world", ((LiteralExpression)expression).Value);
        }

        [Test]
        public void ParseExpression_Variable_ShouldParseCorrectly()
        {
            var yaml = "!expr my_variable";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<VariableExpression>(expression);
            Assert.AreEqual("my_variable", ((VariableExpression)expression).VariableName);
        }

        [Test]
        public void ParseExpression_List_ShouldParseCorrectly()
        {
            var yaml = "[1, 'two', !expr three]";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<ListExpression>(expression);
            var list = (ListExpression)expression;
            Assert.AreEqual(3, list.Elements.Count);
            Assert.IsInstanceOf<LiteralExpression>(list.Elements[0]);
            Assert.IsInstanceOf<LiteralExpression>(list.Elements[1]);
            Assert.IsInstanceOf<VariableExpression>(list.Elements[2]);
        }

        [Test]
        public void ParseExpression_Dictionary_ShouldParseCorrectly()
        {
            var yaml = @"{
  key1: 'value1',
  123: 456,
  !expr my_var: !expr my_val
}";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<DictionaryExpression>(expression);
            var dict = (DictionaryExpression)expression;
            Assert.AreEqual(3, dict.Entries.Count);

            var entry1 = dict.Entries.First(kvp => ((LiteralExpression)kvp.Key).Value.Equals("key1"));
            Assert.IsInstanceOf<LiteralExpression>(entry1.Value);
            Assert.AreEqual("value1", ((LiteralExpression)entry1.Value).Value);

            var entry2 = dict.Entries.First(kvp => ((LiteralExpression)kvp.Key).Value.Equals(123L));
            Assert.IsInstanceOf<LiteralExpression>(entry2.Value);
            Assert.AreEqual(456L, ((LiteralExpression)entry2.Value).Value);

            var entry3 = dict.Entries.First(kvp => kvp.Key is VariableExpression);
            Assert.AreEqual("my_var", ((VariableExpression)entry3.Key).VariableName);
            Assert.IsInstanceOf<VariableExpression>(entry3.Value);
            Assert.AreEqual("my_val", ((VariableExpression)entry3.Value).VariableName);
        }

        [Test]
        public void ParseExpression_MemberAccess_ShouldParseCorrectly()
        {
            var yaml = "!expr my_obj.my_prop.my_field";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<MemberAccessExpression>(expression);
            var memberAccess = (MemberAccessExpression)expression;
            Assert.AreEqual("my_field", memberAccess.MemberName);
            Assert.IsInstanceOf<MemberAccessExpression>(memberAccess.ObjectExpression);
        }

        [Test]
        public void ParseExpression_Binary_ShouldParseCorrectly()
        {
            var yaml = "!expr 1 + 2";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<BinaryExpression>(expression);
            var binary = (BinaryExpression)expression;
            Assert.AreEqual(OperatorType.Add, binary.OperatorType);
            Assert.IsInstanceOf<LiteralExpression>(binary.Left);
            Assert.IsInstanceOf<LiteralExpression>(binary.Right);
        }

        [Test]
        public void ParseExpression_Unary_ShouldParseCorrectly()
        {
            var yaml = "!expr '!my_bool'";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<UnaryExpression>(expression);
            var unary = (UnaryExpression)expression;
            Assert.AreEqual(OperatorType.Not, unary.OperatorType);
            Assert.IsInstanceOf<VariableExpression>(unary.Operand);
        }

        [Test]
        public void ParseExpression_InterpolatedString_ShouldParseCorrectly()
        {
            var yaml = "!expr '\"Hello ${my_name}!\"'";
            var expression = _parser.ParseExpression(yaml);

            Assert.IsInstanceOf<InterpolatedStringExpression>(expression);
            var interpolated = (InterpolatedStringExpression)expression;
            Assert.AreEqual(3, interpolated.Parts.Count);
            Assert.AreEqual("Hello ", ((LiteralExpression)interpolated.Parts[0]).Value);
            Assert.IsInstanceOf<VariableExpression>(interpolated.Parts[1]);
            Assert.AreEqual("!", ((LiteralExpression)interpolated.Parts[2]).Value);
        }
    }
}