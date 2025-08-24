using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using System;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class AssertStatementTests : StatementTestBase
    {
        [Test]
        public async Task Assert_ShouldDoNothing_WhenConditionIsTrue()
        {
            // Arrange
            var statement = Assert_(Literal(true));

            // Act & Assert
            await Env.ExecuteAsync(statement);
        }

        [Test]
        public void Assert_ShouldThrowException_WhenConditionIsFalse()
        {
            // Arrange
            var statement = Assert_(Literal(false), "Assertion failed");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Env.ExecuteAsync(statement));
            Assert.AreEqual("Assertion failed", ex.Message);
        }
    }
}