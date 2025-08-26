using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class VarStatementTests : StatementTestBase
    {
        [Test]
        public async Task Var_ShouldDefineMutableVariable()
        {
            // Arrange
            var varStatement = Var("x", Literal(10));
            var assignStatement = Assign(Variable("x"), Literal(20));

            // Act
            await Env.ExecuteAsync(varStatement);
            await Env.ExecuteAsync(assignStatement);
            var result = await Env.ExecuteAsync(Variable("x"));

            // Assert
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public void Var_AccessIsCaseSensitive_ShouldThrow()
        {
            // Arrange
            var varStatement = Var("myVar", Literal(100));

            // Act & Assert
            var ex = Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
            {
                await Env.ExecuteAsync(varStatement);
                // This should fail because 'myvar' is not 'myVar'
                await Env.ExecuteAsync(Variable("myvar"));
            });
            StringAssert.Contains("Variable 'myvar' is not defined", ex.Message);
        }
    }
}