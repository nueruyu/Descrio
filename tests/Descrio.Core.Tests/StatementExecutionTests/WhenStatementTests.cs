using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using Descrio.Syntax;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class WhenStatementTests : StatementTestBase
    {
        [Test]
        public async Task When_ShouldExecuteCorrectCase()
        {
            // Arrange
            await Env.ExecuteAsync(Var("x", Literal(10)));
            var statement = When()
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(5)), Run("log", ("val", Literal("A"))).Build())
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(10)), Run("log", ("val", Literal("B"))).Build())
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(15)), Run("log", ("val", Literal("C"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "B" }, Env.LogHistory);
        }

        [Test]
        public async Task When_ShouldExecuteElseCase()
        {
            // Arrange
            await Env.ExecuteAsync(Var("x", Literal(20)));
            var statement = When()
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(5)), Run("log", ("val", Literal("A"))).Build())
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(10)), Run("log", ("val", Literal("B"))).Build())
                .Default(Run("log", ("val", Literal("Default"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "Default" }, Env.LogHistory);
        }

        [Test]
        public async Task When_ShouldDoNothingWhenNoMatchAndNoElse()
        {
            // Arrange
            await Env.ExecuteAsync(Var("x", Literal(20)));
            var statement = When()
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(5)), Run("log", ("val", Literal("A"))).Build())
                .Case(Binary(Variable("x"), OperatorType.Equal, Literal(10)), Run("log", ("val", Literal("B"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.IsEmpty(Env.LogHistory);
        }
    }
}