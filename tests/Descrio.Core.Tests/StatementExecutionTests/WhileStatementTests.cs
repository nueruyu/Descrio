using Descrio.Syntax;
using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestExpressionFactory;
using static Descrio.EditorTests.TestStatementFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class WhileStatementTests : StatementTestBase
    {
        [Test]
        public async Task WhileStatement_ExecutesWhileConditionIsTrue()
        {
            // Arrange
            await Env.ExecuteAsync(Var("i", Literal(0)));

            var statement = While(
                Binary(Variable("i"), OperatorType.LessThan, Literal(3)),
                Assign(Variable("i"), Binary(Variable("i"), OperatorType.Add, Literal(1)))
            );

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            var finalValue = Env.VariableRegistry.Get("i");
            Assert.AreEqual(3, finalValue);
        }

        [Test]
        public async Task WhileStatement_CanBeBroken()
        {
            // Arrange
            await Env.ExecuteAsync(Var("i", Literal(0)));

            var statement = While(
                Literal(true),
                Assign(Variable("i"), Binary(Variable("i"), OperatorType.Add, Literal(1))),
                When().Case(Binary(Variable("i"), OperatorType.GreaterThanOrEqual, Literal(5)), Break()).Build()
            );

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            var finalValue = Env.VariableRegistry.Get("i");
            Assert.AreEqual(5, finalValue);
        }
    }
}