using NUnit.Framework;
using Descrio.Execution;
using Descrio.Parse.Expressions;
using System.Threading.Tasks;

namespace Descrio.EditorTests
{
    public class ExpressionParserTests
    {
        private ExecutionContext _emptyContext;
        private IAstVisitor _visitor;

        [SetUp]
        public void SetUp()
        {
            // Create a default context and visitor for each test.
            _emptyContext = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), new VariableRegistry());
            _visitor = new ExecutionVisitor(_emptyContext);
        }

        [TestCase("123", 123L)]
        [TestCase("'hello'", "hello")]
        [TestCase("true", true)]
        public async Task Parse_ShouldHandleLiterals(string input, object expectedValue)
        {
            var expression = new ExpressionParser(input).Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual(expectedValue, result);
        }

        [Test]
        public async Task Parse_ShouldHandleVariable()
        {
            // Setup a context with a variable.
            var varRegistry = new VariableRegistry();
            varRegistry.Set("my_variable", "test_value");
            var context = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), varRegistry);
            var visitor = new ExecutionVisitor(context);

            var expression = new ExpressionParser("my_variable").Parse();
            var result = await expression.AcceptAsync(visitor);

            Assert.AreEqual("test_value", result);
        }

        [Test]
        public async Task Parse_ShouldReturnNullForUndefinedVariable()
        {
            var expression = new ExpressionParser("undefined_variable").Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.IsNull(result);
        }

        [TestCase("10 == 10", true)]
        [TestCase("10 == 20", false)]
        [TestCase("'a' != 'b'", true)]
        [TestCase("'a' != 'a'", false)]
        [TestCase("true == true", true)]
        public async Task Parse_ShouldEvaluateBinaryExpressions(string input, bool expected)
        {
            var expression = new ExpressionParser(input).Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Parse_ShouldHandleMultipleOperatorsWithLeftAssociativity()
        {
            // This test checks the structural correctness of the parse tree.
            var expression = new ExpressionParser("var1 == var2 != true").Parse();

            Assert.IsInstanceOf<BinaryExpression>(expression);
            var rootExpr = (BinaryExpression)expression;
            Assert.AreEqual(OperatorType.NotEqual, rootExpr.OperatorType);

            Assert.IsInstanceOf<BinaryExpression>(rootExpr.Left);
            var nestedExpr = (BinaryExpression)rootExpr.Left;
            Assert.AreEqual(OperatorType.Equal, nestedExpr.OperatorType);
        }

        [Test]
        public void Parse_ShouldHandleWhitespace()
        {
            var expression = new ExpressionParser("  my_var   ==   'test'  ").Parse();
            Assert.IsInstanceOf<BinaryExpression>(expression);
        }
    }
}