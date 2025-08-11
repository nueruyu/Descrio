using NUnit.Framework;
using Descrio.Execution;
using Descrio.Parse.Expressions;
using System.Threading.Tasks;
using System;

namespace Descrio.EditorTests
{
    public class ExpressionParserTests
    {
        private ExecutionContext _emptyContext;
        private IAstVisitor _visitor;

        [SetUp]
        public void SetUp()
        {
            var varRegistry = new VariableRegistry();
            varRegistry.Set("name", "Descrio");
            varRegistry.Set("version", "1.0");

            // Create a default context and visitor for each test.
            _emptyContext = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), varRegistry);
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

        // --- Tests for String Interpolation ---

        [Test]
        public async Task Parse_ShouldHandleSimpleInterpolatedString()
        {
            var expression = new ExpressionParser("'Hello, ${name}!'").Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual("Hello, Descrio!", result);
        }

        [Test]
        public async Task Parse_ShouldHandleMultipleInterpolations()
        {
            var expression = new ExpressionParser("'Project: ${name}, Version: ${version}'").Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual("Project: Descrio, Version: 1.0", result);
        }

        [Test]
        public async Task Parse_ShouldHandleInterpolationAtStartAndEnd()
        {
            var expression = new ExpressionParser("'${name} is version ${version}'").Parse();
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual("Descrio is version 1.0", result);
        }

        [Test]
        public async Task Parse_ShouldReturnSimpleStringWhenNoInterpolation()
        {
            var expression = new ExpressionParser("'Just a simple string.'").Parse();
            // It should be a LiteralExpression, not an InterpolatedStringExpression
            Assert.IsInstanceOf<LiteralExpression>(expression);
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual("Just a simple string.", result);
        }

        [Test]
        public async Task Parse_ShouldHandleEmptyString()
        {
            var expression = new ExpressionParser("''").Parse();
            Assert.IsInstanceOf<LiteralExpression>(expression);
            var result = await expression.AcceptAsync(_visitor);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public async Task Parse_ShouldHandleEmptyInterpolation()
        {
            var expression = new ExpressionParser("'Hello, ${}'").Parse();
            var result = await expression.AcceptAsync(_visitor);
            // Assuming an empty expression evaluates to null, and ToString() on null is an empty string.
            Assert.AreEqual("Hello, ", result);
        }

        [Test]
        public async Task Parse_ShouldHandleComplexExpressionInsideInterpolation()
        {
            // Setup a context with numeric variables for the binary expression
            var varRegistry = new VariableRegistry();
            varRegistry.Set("a", 10);
            varRegistry.Set("b", 20);
            var context = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), varRegistry);
            var visitor = new ExecutionVisitor(context);

            var expression = new ExpressionParser("'Result is ${a == b}'").Parse();
            var result = await expression.AcceptAsync(visitor);
            Assert.AreEqual("Result is False", result);
        }

        [Test]
        public void Parse_ShouldThrowFormatExceptionOnUnterminatedExpression()
        {
            Assert.Throws<FormatException>(() =>
            {
                new ExpressionParser("'Hello, ${name'").Parse();
            });
        }
    }
}