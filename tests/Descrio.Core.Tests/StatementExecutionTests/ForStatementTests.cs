using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using Descrio.Syntax;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class ForStatementTests : StatementTestBase
    {
        [Test]
        public async Task For_ShouldIterateOverList()
        {
            // Arrange
            var list = new List<object> { 1, 2, 3 };
            var statement = For("item", Literal(list))
                .Do(Run("log").With("val", Variable("item")).Build());

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, Env.LogHistory);
        }

        [Test]
        public async Task For_ShouldHandleBreakStatement()
        {
            // Arrange
            var list = new List<object> { 1, 2, 3 };
            var statement = For("item", Literal(list))
                .Do(
                    Run("log").With("val", Variable("item")).Build(),
                    When()
                        .Case(Binary(Variable("item"), OperatorType.Equal, Literal(2)), Break()).Build(),
                    Run("log", ("val", Literal("after_when"))).Build()
                );

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new object[] { 1, "after_when", 2 }, Env.LogHistory);
        }

        [Test]
        public async Task For_ShouldHandleContinueStatement()
        {
            // Arrange
            var list = new List<object> { 1, 2, 3 };
            var statement = For("item", Literal(list))
                .Do(
                    When()
                        .Case(Binary(Variable("item"), OperatorType.Equal, Literal(2)), Continue()).Build(),
                    Run("log").With("val", Variable("item")).Build()
                );

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new object[] { 1, 3 }, Env.LogHistory);
        }
    }
}