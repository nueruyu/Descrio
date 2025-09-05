using NUnit.Framework;
using System.Threading.Tasks;
using System;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class ThrowStatementTests : StatementTestBase
    {
        [Test]
        public void ExecuteAsync_RegisteredException_ThrowsCorrectException()
        {
            // Arrange
            Env.RegisterClass("MyError", typeof(InvalidOperationException));
            var throwStatement = Throw("MyError");

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Env.ExecuteAsync(throwStatement));
        }

        [Test]
        public async Task ExecuteAsync_WithMessageArgument_SetsExceptionMessage()
        {
            // Arrange
            Env.RegisterClass("MyError", typeof(ArgumentException));
            var message = "This is a test error.";
            var throwStatement = Throw("MyError").With("message", Literal(message));

            // Act
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await Env.ExecuteAsync(throwStatement));

            // Assert
            Assert.IsNotNull(ex);
            StringAssert.Contains(message, ex.Message);
        }

        [Test]
        public async Task ExecuteAsync_WithDataArgument_SetsExceptionData()
        {
            // Arrange
            Env.RegisterClass("MyError", typeof(Exception));
            var errorCode = 404L;
            var throwStatement = Throw("MyError").With(
                ("code", Literal(errorCode)),
                ("resource", Literal("TestResource"))
            );

            // Act
            var ex = Assert.ThrowsAsync<Exception>(async () => await Env.ExecuteAsync(throwStatement));

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual(errorCode, ex.Data["code"]);
            Assert.AreEqual("TestResource", ex.Data["resource"]);
        }

        [Test]
        public void ExecuteAsync_UnregisteredException_ThrowsInvalidOperationException()
        {
            // Arrange
            var throwStatement = Throw("NonExistentError");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Env.ExecuteAsync(throwStatement));
            StringAssert.Contains("'NonExistentError' is not a valid and registered exception class", ex.Message);
        }

        [Test]
        public void ExecuteAsync_NonExceptionType_ThrowsInvalidOperationException()
        {
            // Arrange
            Env.RegisterClass("NotAnError", typeof(string));
            var throwStatement = Throw("NotAnError");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Env.ExecuteAsync(throwStatement));
            StringAssert.Contains("'NotAnError' is not a valid and registered exception class", ex.Message);
        }

        [Test]
        public void Throw_RegisteredClassNameIsCaseSensitive_ShouldThrow()
        {
            // Arrange
            Env.RegisterClass("MyError", typeof(InvalidOperationException));
            var throwStatement = Throw("myerror"); // Using incorrect case

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Env.ExecuteAsync(throwStatement));
            StringAssert.Contains("'myerror' is not a valid and registered exception class", ex.Message);
        }
    }
}