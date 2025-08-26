using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using static Descrio.EditorTests.TestStatementFactory;
using static Descrio.EditorTests.TestExpressionFactory;
using Descrio.Parse.ModuleLoaders;
using Descrio.Execution;

namespace Descrio.EditorTests
{
    [TestFixture]
    public class ScriptRunnerTests
    {
        private List<object> _logHistory;
        private ICallable _logCallable;

        [SetUp]
        public void SetUp()
        {
            _logHistory = new List<object>();
            _logCallable = new DelegateCallable((Arguments args, ExecutionContext context) =>
            {
                _logHistory.Add(args.FirstOrDefault().Value);
                return new ValueTask<object>((object)null);
            });
        }

        [Test]
        public void ExecuteAsync_ModuleScopeIsIsolated_VariableFromAnotherModuleIsNotAccessible()
        {
            // Arrange
            var modules = new Dictionary<string, Module>
            {
                { "/moduleA.yaml", new Module(
                    statements: new IStatement[] { Var("x", Literal(10)) },
                    importPaths: Array.Empty<string>())
                },
                { "/main.yaml", new Module(
                    statements: new IStatement[] { Run("log", ("val", Variable("x"))).Build() },
                    importPaths: new[] { "/moduleA.yaml" })
                }
            };
            var loader = new InMemoryModuleLoader(modules);
            var runner = new ScriptRunner(loader).AddCallable("log", _logCallable);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await runner.ExecuteAsync("/main.yaml", "/")
            );
            StringAssert.Contains("Variable 'x' is not defined", ex.Message);
        }

        [Test]
        public async Task ExecuteAsync_ModuleScopesAreIsolated_SameVariableNameDoesNotConflict()
        {
            // Arrange
            var modules = new Dictionary<string, Module>
            {
                { "/moduleA.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Var("x", Literal(100)),
                        Function("get_x_from_a").Body(Return(Variable("x"))).Build()
                    },
                    importPaths: Array.Empty<string>())
                },
                { "/main.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Var("x", Literal(200)),
                        Let("x_from_a", Run("get_x_from_a").Build()),
                        Run("log", ("val", Variable("x_from_a"))).Build(),
                        Run("log", ("val", Variable("x"))).Build()
                    },
                    importPaths: new[] { "/moduleA.yaml" })
                }
            };
            var loader = new InMemoryModuleLoader(modules);
            var runner = new ScriptRunner(loader).AddCallable("log", _logCallable);

            // Act
            await runner.ExecuteAsync("/main.yaml", "/");

            // Assert
            CollectionAssert.AreEqual(new object[] { 100L, 200L }, _logHistory);
        }

        [Test]
        public void ExecuteAsync_FunctionHasStaticScope_CannotAccessCallerScopeVariables()
        {
            // Arrange
            var modules = new Dictionary<string, Module>
            {
                { "/moduleB.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Function("get_y").Body(Return(Variable("y"))).Build()
                    },
                    importPaths: Array.Empty<string>())
                },
                { "/main.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Var("y", Literal("value_from_main")),
                        Run("get_y").Build()
                    },
                    importPaths: new[] { "/moduleB.yaml" })
                }
            };
            var loader = new InMemoryModuleLoader(modules);
            var runner = new ScriptRunner(loader).AddCallable("log", _logCallable);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await runner.ExecuteAsync("/main.yaml", "/")
            );
            StringAssert.Contains("Variable 'y' is not defined", ex.Message);
        }

        [Test]
        public async Task ExecuteAsync_FunctionHasStaticScope_CorrectlyCapturesItsOwnModuleScope()
        {
            // Arrange
            var modules = new Dictionary<string, Module>
            {
                { "/moduleB.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Var("y", Literal("value_from_b")),
                        Function("get_y").Body(Return(Variable("y"))).Build()
                    },
                    importPaths: Array.Empty<string>())
                },
                { "/main.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Var("y", Literal("value_from_main")),
                        Let("y_from_b", Run("get_y").Build()),
                        Run("log", ("val", Variable("y_from_b"))).Build()
                    },
                    importPaths: new[] { "/moduleB.yaml" })
                }
            };
            var loader = new InMemoryModuleLoader(modules);
            var runner = new ScriptRunner(loader).AddCallable("log", _logCallable);

            // Act
            await runner.ExecuteAsync("/main.yaml", "/");

            // Assert
            CollectionAssert.AreEqual(new object[] { "value_from_b" }, _logHistory);
        }

        [Test]
        public void ExecuteAsync_SiblingModuleFunctions_AreNotAccessible()
        {
            var modules = new Dictionary<string, Module>
            {
                { "/moduleA.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Function("func_A").Body(Return(Literal("from A"))).Build()
                    },
                    importPaths: Array.Empty<string>())
                },
                { "/moduleB.yaml", new Module(
                    statements: new IStatement[]
                    {
                        Run("func_A").Build()
                    },
                    importPaths: Array.Empty<string>())
                },
                { "/main.yaml", new Module(
                    statements: Array.Empty<IStatement>(),
                    importPaths: new[] { "moduleA.yaml", "moduleB.yaml" })
                }
            };
            var loader = new InMemoryModuleLoader(modules);
            var runner = new ScriptRunner(loader);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await runner.ExecuteAsync("/main.yaml", "/")
            );
            StringAssert.Contains("Callable 'func_A' not found", ex.Message);
        }
    }
}