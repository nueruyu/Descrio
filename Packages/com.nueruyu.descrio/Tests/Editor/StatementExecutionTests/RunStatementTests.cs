using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestExpressionFactory;
using static Descrio.EditorTests.TestStatementFactory;
using Descrio.Execution;

namespace Descrio.EditorTests.StatementExecutionTests
{
    public class RunStatementTests : StatementTestBase
    {
        [Test]
        public async Task RunStatement_ExecutesCallable()
        {
            // Arrange
            var statement = Run("log").With("value", Literal("logged"));

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual(1, Env.LogHistory.Count);
            Assert.AreEqual("logged", Env.LogHistory[0]);
        }
    }
}