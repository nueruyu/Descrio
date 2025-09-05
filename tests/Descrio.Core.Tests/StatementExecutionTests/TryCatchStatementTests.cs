using NUnit.Framework;
using System.Threading.Tasks;
using System;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class TryCatchStatementTests : StatementTestBase
    {
        [SetUp]
        public void TryCatchSetUp()
        {
            Env.RegisterClass("InvalidOperationError", typeof(InvalidOperationException))
               .RegisterClass("ArgumentError", typeof(ArgumentException));
        }

        [Test]
        public async Task TryCatch_ShouldCatchSpecificExceptionAndRunFinally()
        {
            // Arrange
            var statement = Try(
                    Run("log", ("val", Literal("TryEnter"))).Build(),
                    Throw("InvalidOperationError").With("message", Literal("Something went wrong")).Build(),
                    Run("log", ("val", Literal("ShouldNotRun"))).Build()
                )
                .Catch("InvalidOperationError", "err",
                    Run("log", ("val", InterpolatedString(Literal("Caught: "), Variable("err").Member("Message")))).Build()
                )
                .Finally(
                    Run("log", ("val", Literal("Finally"))).Build()
                )
                .Build();

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            var expected = new[] { "TryEnter", "Caught: Something went wrong", "Finally" };
            CollectionAssert.AreEqual(expected, Env.LogHistory);
        }

        [Test]
        public async Task TryCatch_ShouldExecuteFinallyOnSuccess()
        {
            // Arrange
            var statement = Try(Run("log", ("val", Literal("Success"))).Build())
                .Catch("ArgumentError", "err", Run("log", ("val", Literal("ShouldNotBeCaught"))).Build())
                .Finally(Run("log", ("val", Literal("Finally"))).Build())
                .Build();

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "Success", "Finally" }, Env.LogHistory);
        }

        [Test]
        public void TryCatch_ShouldRethrowUncaughtException()
        {
            // Arrange
            var statement = Try(Throw("ArgumentError").Build())
                .Catch("InvalidOperationError", "err", Run("log", ("val", Literal("WrongCatch"))).Build())
                .Build();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await Env.ExecuteAsync(statement));
            Assert.IsEmpty(Env.LogHistory);
        }

        [Test]
        public async Task TryCatch_ShouldCatchWithDefaultClause()
        {
            // Arrange
            var statement = Try(Throw("ArgumentError").Build())
                .Catch("InvalidOperationError", "err", Run("log", ("val", Literal("Wrong"))).Build())
                .CatchDefault(Run("log", ("val", Literal("DefaultCatch"))).Build())
                .Build();

            // Act
            await Env.ExecuteAsync(statement);

            // Assert
            CollectionAssert.AreEqual(new[] { "DefaultCatch" }, Env.LogHistory);
        }
    }
}