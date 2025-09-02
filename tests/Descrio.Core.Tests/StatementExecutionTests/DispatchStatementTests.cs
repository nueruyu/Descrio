using NUnit.Framework;
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class DispatchStatementTests : StatementTestBase
    {
        [Test]
        public async Task Dispatch_ShouldCallFunctionAndReturnTask()
        {
            // Arrange
            var statement = Dispatch("log").With("val", "dispatched");

            // Act
            var result = await Env.ExecuteAsync(statement);

            // Assert
            Assert.IsInstanceOf<ValueTask<object>>(result.Value);
            await (ValueTask<object>)result.Value;
            CollectionAssert.AreEqual(new[] { "dispatched" }, Env.LogHistory);
        }
    }
}