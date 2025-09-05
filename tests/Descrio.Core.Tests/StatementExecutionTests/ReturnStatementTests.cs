using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class ReturnStatementTests : StatementTestBase
    {
        [Test]
        public async Task Return_ShouldExitFunctionWithValue()
        {
            // Arrange
            var definition = Function("myFunc")
                .Body(
                    Run("log", ("val", Literal("before_return"))).Build(),
                    Return(Literal("returnValue")),
                    Run("log", ("val", Literal("after_return"))).Build()
                );
            var call = Run("myFunc");

            // Act
            await Env.ExecuteAsync(definition);
            var result = await Env.ExecuteAsync(call);

            // Assert
            Assert.AreEqual("returnValue", result.Value);
            CollectionAssert.AreEqual(new[] { "before_return" }, Env.LogHistory);
        }

        [Test]
        public async Task Return_ShouldExitFunctionWithNull()
        {
            // Arrange
            var definition = Function("myFunc")
                .Body(
                    Return()
                );
            var call = Run("myFunc");

            // Act
            await Env.ExecuteAsync(definition);
            var result = await Env.ExecuteAsync(call);

            // Assert
            Assert.IsNull(result.Value);
        }
    }
}