using NUnit.Framework;
using System; // For Convert
using System.Threading.Tasks;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;

namespace Descrio.EditorTests.StatementExecutionTests
{
    [TestFixture]
    public class LambdaExpressionTests : StatementTestBase
    {
        private async Task ExecuteStatements(params IStatement[] statements)
        {
            foreach (var statement in statements)
            {
                await Env.ExecuteAsync(statement);
            }
        }

        [Test]
        public async Task Lambda_CanBeAssignedToVariableAndExecuted()
        {
            // Arrange
            var lambda = new LambdaExpression(
                new[] { new ParameterDefinition("x", null, null) },
                new[] { Return(Binary(Variable("x"), OperatorType.Multiply, Literal(2))) }
            );

            // Act
            await ExecuteStatements(
                Let("double_op", lambda),
                Let("result", Run("double_op", ("x", Literal(5))).Build())
            );

            // Assert
            var result = Env.VariableRegistry.Get("result");
            Assert.AreEqual(10m, Convert.ToDecimal(result));
        }

        [Test]
        public async Task Lambda_AsArgument_CanBeUsedAsCallback()
        {
            // Arrange
            var applyFunc = Function("Apply")
                .WithParams("value", "operation")
                .Body(Return(Run("operation", ("val", Variable("value"))).Build()));

            var lambda = new LambdaExpression(
                new[] { new ParameterDefinition("val", null, null) },
                new[] { Return(Binary(Variable("val"), OperatorType.Add, Literal(10))) }
            );

            // Act
            await ExecuteStatements(
                applyFunc.Build(),
                Let("result", Run("Apply", ("value", Literal(5)), ("operation", lambda)).Build())
            );

            // Assert
            var result = Env.VariableRegistry.Get("result");
            Assert.AreEqual(15m, Convert.ToDecimal(result));
        }

        [Test]
        public async Task Closure_LambdaCapturesOuterScopeVariable()
        {
            // Arrange
            var createAdderFunc = Function("CreateAdder")
                .WithParams("amount")
                .Body(
                    Return(new LambdaExpression(
                        new[] { new ParameterDefinition("x", null, null) },
                        new[] { Return(Binary(Variable("x"), OperatorType.Add, Variable("amount"))) }
                    ))
                );

            // Act
            await ExecuteStatements(
                createAdderFunc.Build(),
                Let("add5", Run("CreateAdder", ("amount", Literal(5))).Build()),
                Let("result", Run("add5", ("x", Literal(10))).Build())
            );

            // Assert
            var result = Env.VariableRegistry.Get("result");
            Assert.AreEqual(15m, Convert.ToDecimal(result));
        }

        [Test]
        public async Task Closure_LambdaSeesUpdatedValue_OfCapturedVariable()
        {
            // Arrange
            var lambda = new LambdaExpression(
                new[] { new ParameterDefinition("x", null, null) },
                new[] { Return(Binary(Variable("x"), OperatorType.Multiply, Variable("multiplier"))) }
            );

            // Act
            await ExecuteStatements(
                Var("multiplier", Literal(2)),
                Let("my_lambda", lambda),
                Assign(Variable("multiplier"), Literal(10)),
                Let("result", Run("my_lambda", ("x", Literal(5))).Build())
            );

            // Assert
            var result = Env.VariableRegistry.Get("result");
            Assert.AreEqual(50m, Convert.ToDecimal(result));
        }
    }
}