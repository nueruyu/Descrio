using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestExpressionFactory;
using static Descrio.EditorTests.TestStatementFactory;
using Descrio.Execution;

namespace Descrio.EditorTests.StatementExecutionTests
{
    public class BreakStatementTests : StatementTestBase
    {
        [Test]
        public async Task BreakStatement_ChangesFlowStateToBreak()
        {
            // Arrange
            var statement = While(Literal(true), Break());

            // Act
            var result = await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual(FlowState.Normal, result.Flow);
        }
    }
}