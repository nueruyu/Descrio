using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

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
    }
}