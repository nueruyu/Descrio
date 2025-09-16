using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using Descrio.Syntax;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class FunctionStatementTests : StatementTestBase
    {
        [Test]
        public async Task Function_ShouldDefineAndCallFunction()
        {
            // Arrange
            var definition = Function("myFunc")
                .WithParams("a", "b")
                .Body(Return(Binary(Variable("a"), OperatorType.Add, Variable("b"))));

            var call = Run("myFunc", ("a", Literal(10)), ("b", Literal(20)));

            // Act
            await Env.ExecuteAsync(definition);
            var result = await Env.ExecuteAsync(call);

            // Assert
            Assert.AreEqual(30, result.Value);
        }

        [Test]
        public async Task Function_ShouldHandleImplicitReturn()
        {
            // Arrange
            var definition = Function("myFunc").Body(Run("log", ("val", Literal("done"))).Build());
            var call = Run("myFunc");

            // Act
            await Env.ExecuteAsync(definition);
            var result = await Env.ExecuteAsync(call);

            // Assert
            Assert.IsNull(result.Value);
            CollectionAssert.AreEqual(new[] { "done" }, Env.LogHistory);
        }

        [Test]
        public void Function_CallIsCaseSensitive_ShouldThrow()
        {
            // Arrange
            var definition = Function("myFunc").Body(Return(Literal(true)));

            // Act
            var ex = Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
            {
                await Env.ExecuteAsync(definition);
                // This should fail because 'myfunc' is not 'myFunc'
                await Env.ExecuteAsync(Run("myfunc"));
            });

            // Assert
            StringAssert.Contains("Callable 'myfunc' not found", ex.Message);
        }
    }
}