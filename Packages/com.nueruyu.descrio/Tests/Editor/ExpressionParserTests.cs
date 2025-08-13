using NUnit.Framework;
using Descrio.Execution;
using Descrio.Parse.Expressions;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace Descrio.EditorTests
{
    [TestFixture]
    public class ExpressionParserTests
    {
        private ExecutionContext _testContext;

        [SetUp]
        public void SetUp()
        {
            // Prepare a context with some variables for general use in tests.
            var varRegistry = new VariableRegistry();
            // Per user request, assuming Define defaults to isMutable: true
            varRegistry.Define("x", 10L);
            varRegistry.Define("y", 20L);
            varRegistry.Define("t", true);
            varRegistry.Define("f", false);
            varRegistry.Define("s", "Descrio");

            _testContext = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), varRegistry, new ClassRegistry());
        }

        // Helper to parse and evaluate an expression with a given context
        private async Task<object> Evaluate(string source, ExecutionContext context)
        {
            var parser = new ExpressionParser(source);
            var expression = parser.Parse();

            var interpreter = new Interpreter(context);
            var visitResult = await interpreter.ExecuteAsync(expression);
            // Expressions should not alter control flow, so we assert this rule.
            Assert.AreEqual(FlowState.Normal, visitResult.Flow, "Expression evaluation should not alter control flow.");
            return visitResult.Value;
        }

        // Helper to evaluate with the default test context
        private async Task<object> Evaluate(string source)
        {
            return await Evaluate(source, _testContext);
        }

        [Test]
        [TestCase("123", 123L)]
        [TestCase("'hello world'", "hello world")]
        [TestCase("\"double quotes\"", "double quotes")]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("null", null)]
        public async Task Parse_Literals_ShouldEvaluateToCorrectTypes(string source, object expected)
        {
            Assert.AreEqual(expected, await Evaluate(source));
        }

        [Test]
        public async Task Parse_Variable_ShouldEvaluateToItsValue()
        {
            Assert.AreEqual(10L, await Evaluate("x"));
            Assert.AreEqual("Descrio", await Evaluate("s"));
        }

        [Test]
        public void Parse_UndefinedVariable_ShouldThrowException()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => Evaluate("undefined_variable"));
            StringAssert.Contains("Variable 'undefined_variable' is not defined", ex.Message);
        }

        [Test]
        [TestCase("5 + 3", 8L)]
        [TestCase("10 - 4", 6L)]
        [TestCase("6 * 7", 42L)]
        [TestCase("42 / 6", 7L)]
        [TestCase("2 + 3 * 4", 14L)]
        [TestCase("(2 + 3) * 4", 20L)]
        [TestCase("100 / 10 / 2", 5L)] // Left-associativity
        public async Task Parse_ArithmeticExpressions_ShouldRespectPrecedenceAndAssociativity(string source, long expected)
        {
            var result = await Evaluate(source);
            Assert.AreEqual(Convert.ToDecimal(expected), Convert.ToDecimal(result));
        }

        [Test]
        [TestCase("10 > 5", true)]
        [TestCase("10 < 5", false)]
        [TestCase("10 >= 10", true)]
        [TestCase("10 <= 5", false)]
        [TestCase("x < y", true)]
        [TestCase("x > y", false)]
        public async Task Parse_ComparisonExpressions_ShouldEvaluateCorrectly(string source, bool expected)
        {
            Assert.AreEqual(expected, await Evaluate(source));
        }

        [Test]
        [TestCase("10 == 10", true)]
        [TestCase("10 == 20", false)]
        [TestCase("10 != 20", true)]
        [TestCase("'a' == 'a'", true)]
        [TestCase("t == true", true)]
        [TestCase("f == false", true)]
        [TestCase("s != 'Unity'", true)]
        [TestCase("null == null", true)]
        [TestCase("x != null", true)]
        public async Task Parse_EqualityExpressions_ShouldEvaluateCorrectly(string source, bool expected)
        {
            Assert.AreEqual(expected, await Evaluate(source));
        }

        [Test]
        [TestCase("!t", false)]
        [TestCase("!f", true)]
        [TestCase("!!t", true)]
        [TestCase("!(x > y)", true)] // !(10 > 20) -> !false -> true
        public async Task Parse_UnaryExpressions_ShouldEvaluateCorrectly(string source, bool expected)
        {
            Assert.AreEqual(expected, await Evaluate(source));
        }

        [Test]
        [TestCase("-10", -10L)]
        [TestCase("-x", -10L)]
        [TestCase("5 * -2", -10L)]
        [TestCase("-5 - -2", -3L)]
        public async Task Parse_UnaryMinus_ShouldEvaluateCorrectly(string source, long expected)
        {
            var result = await Evaluate(source);
            Assert.AreEqual(Convert.ToDecimal(expected), Convert.ToDecimal(result));
        }

        [Test]
        public async Task Parse_ComplexCombinedExpression_ShouldEvaluateCorrectly()
        {
            // (10 * 2) + (20 / 2) == 30 -> 20 + 10 == 30 -> 30 == 30 -> true
            Assert.AreEqual(true, await Evaluate("(x * 2) + (y / 2) == 30"));
        }

        [Test]
        public async Task Parse_VeryComplexExpression_ShouldRespectAllPrecedences()
        {
            // !false == (2 + (3 * 4) > 10 + 1)
            // true == (2 + 12 > 11)
            // true == (14 > 11)
            // true == true
            // -> true
            var source = "!f == (2 + 3 * 4 > 10 + 1)";
            Assert.AreEqual(true, await Evaluate(source));
        }

        [Test]
        [TestCase("'Hello, ${s}!'", "Hello, Descrio!")]
        [TestCase("'x=${x}, y=${y}'", "x=10, y=20")]
        [TestCase("'${s} is version ${1 + 0.1}'", "Descrio is version 1.1")]
        [TestCase("'Result is ${x > y}'", "Result is False")]
        [TestCase("'${s}'", "Descrio")] // Full string is an expression
        [TestCase("'${s} ${s}'", "Descrio Descrio")]
        public async Task Parse_StringInterpolation_ShouldEvaluateCorrectly(string source, string expected)
        {
            Assert.AreEqual(expected, await Evaluate(source));
        }

        [Test]
        public async Task Parse_NestedStringInterpolation_ShouldEvaluateCorrectly()
        {
            var varRegistry = new VariableRegistry();
            varRegistry.Define("inner", "World");
            var context = new ExecutionContext(new ModulePath("/"), new CallableRegistry(), varRegistry, new());

            var source = "'Hello, ${ \"${inner}!\" }'";
            Assert.AreEqual("Hello, World!", await Evaluate(source, context));
        }

        [Test]
        public async Task Parse_EmptyAndSimpleStrings_ShouldBeLiteral()
        {
            var emptyParser = new ExpressionParser("''");
            Assert.IsInstanceOf<LiteralExpression>(emptyParser.Parse());
            Assert.AreEqual("", await Evaluate("''"));

            var simpleParser = new ExpressionParser("'Just a test'");
            Assert.IsInstanceOf<LiteralExpression>(simpleParser.Parse());
            Assert.AreEqual("Just a test", await Evaluate("'Just a test'"));
        }

        [Test]
        [TestCase("5 +", "Unexpected end of expression.")]
        [TestCase("(10 + 5", "Expect ')' after expression.")]
        [TestCase("5 * (2 + 3))", "Unexpected token ')' found after the expression.")]
        [TestCase("'Hello, ${name'", "Unterminated expression in interpolated string.")]
        [TestCase("x + y *", "Unexpected end of expression.")]
        [TestCase("* 5", "Expected an expression but found '*' at position 0.")]
        public void Parse_InvalidSyntax_ShouldThrowFormatException(string source, string expectedMessageFragment)
        {
            var ex = Assert.Throws<FormatException>(() => new ExpressionParser(source).Parse());
            StringAssert.Contains(expectedMessageFragment, ex.Message);
        }
    }
}