using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestExpressionFactory;
using static Descrio.EditorTests.TestStatementFactory;
using Descrio.Execution;

namespace Descrio.EditorTests.StatementExecutionTests
{
    public class ContinueStatementTests : StatementTestBase
    {
        [Test]
        public async Task ContinueStatement_ChangesFlowStateToContinue()
        {
            // Arrange
            await Env.ExecuteAsync(Var("isCompleted", Literal(false)));
            var statement = While(
                Unary(OperatorType.Not, Variable("isCompleted")),
                Assign(Variable("isCompleted"), Literal(true)),
                Continue());

            // Act
            var result = await Env.ExecuteAsync(statement);

            // Assert
            Assert.AreEqual(FlowState.Normal, result.Flow);

            var isCompleted = await Env.ExecuteAsync(Variable("isCompleted"));
            Assert.AreEqual(true, isCompleted.Value);
        }
    }
}