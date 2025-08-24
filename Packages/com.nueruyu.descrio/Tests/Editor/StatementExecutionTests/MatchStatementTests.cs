using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class MatchStatementTests : StatementTestBase
    {
        [Test]
        public async Task Match_ShouldExecuteCorrectCase()
        {
            // Arrange
            var statement = Match(Literal("b"))
                .Case("a", Run("log", ("val", Literal("A"))).Build())
                .Case("b", Run("log", ("val", Literal("B"))).Build())
                .Case("c", Run("log", ("val", Literal("C"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "B" }, Env.LogHistory);
        }

        [Test]
        public async Task Match_ShouldExecuteDefaultCase()
        {
            // Arrange
            var statement = Match(Literal("d"))
                .Case("a", Run("log", ("val", Literal("A"))).Build())
                .Case("b", Run("log", ("val", Literal("B"))).Build())
                .Default(Run("log", ("val", Literal("Default"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "Default" }, Env.LogHistory);
        }

        [Test]
        public async Task Match_ShouldDoNothingWhenNoMatchAndNoDefault()
        {
            // Arrange
            var statement = Match(Literal("d"))
                .Case("a", Run("log", ("val", Literal("A"))).Build())
                .Case("b", Run("log", ("val", Literal("B"))).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            Assert.IsEmpty(Env.LogHistory);
        }
    }
}