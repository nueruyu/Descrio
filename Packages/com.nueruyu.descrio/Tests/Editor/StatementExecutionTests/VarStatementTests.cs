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
    }
}
