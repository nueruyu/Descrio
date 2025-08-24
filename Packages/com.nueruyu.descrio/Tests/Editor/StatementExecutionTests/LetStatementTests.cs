using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class LetStatementTests : StatementTestBase
    {
        [Test]
        public async Task Let_ShouldDefineImmutableVariable()
        {
            // Arrange
            var statement = Let("x", Literal(10));

            // Act
            await Env.ExecuteAsync(statement);
            var result = await Env.ExecuteAsync(Variable("x"));

            // Assert
            Assert.AreEqual(10, result.Value);
        }

        [Test]
        public void Let_ShouldThrowWhenTryingToMutate()
        {
            // Arrange
            var letStatement = Let("x", Literal(10));
            var assignStatement = Assign(Variable("x"), Literal(20));

            // Act & Assert
            Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
            {
                await Env.ExecuteAsync(letStatement);
                await Env.ExecuteAsync(assignStatement);
            });
        }
    }
}
