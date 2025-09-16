using Descrio.Abstractions;
using Descrio.Data;
using Descrio.EditorTests.Builders;
using Descrio.Execution;
using Descrio.Execution.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio.EditorTests
{
    public class TestEnvironment
    {
        public Interpreter Interpreter { get; }
        public ExecutionContext Context { get; }
        public ClassRegistry ClassRegistry { get; }
        public CallableRegistry CallableRegistry { get; }
        public VariableRegistry VariableRegistry { get; }

        private readonly LogCallable _logCallable;
        public IReadOnlyList<object> LogHistory => _logCallable.ReceivedValues;

        private TestEnvironment(CancellationToken cancellationToken = default)
        {
            ClassRegistry = new ClassRegistry();
            CallableRegistry = new CallableRegistry();
            VariableRegistry = new VariableRegistry();
            Context = new ExecutionContext(
                new ModulePath("/"),
                CallableRegistry,
                VariableRegistry,
                ClassRegistry,
                cancellationToken
            );
            Interpreter = new Interpreter(Context);

            // Setup built-in test helpers
            _logCallable = new LogCallable();
            CallableRegistry.Register("log", _logCallable);
        }

        public static TestEnvironment Create() => new TestEnvironment();

        public TestEnvironment RegisterClass(string name, Type type)
        {
            ClassRegistry.Register(name, type);
            return this; // Enable method chaining
        }

        public TestEnvironment RegisterCallable(string name, ICallable callable)
        {
            CallableRegistry.Register(name, callable);
            return this;
        }

        public ValueTask<VisitResult> ExecuteAsync(IStatement statement)
        {
            return Interpreter.ExecuteAsync(statement);
        }

        public ValueTask<VisitResult> ExecuteAsync(IStatementBuilder statementBuilder)
        {
            return ExecuteAsync(statementBuilder.BuildStatement());
        }

        public ValueTask<VisitResult> ExecuteAsync(IExpression expression)
        {
            return Interpreter.ExecuteAsync(expression);
        }

        public ValueTask<VisitResult> ExecuteAsync(IExpressionBuilder expressionBuilder)
        {
            return ExecuteAsync(expressionBuilder.BuildExpression());
        }

        private class LogCallable : ICallable
        {
            public readonly List<object> ReceivedValues = new();

            public ValueTask<object> CallAsync(Arguments args, ExecutionContext context)
            {
                // Log the first argument's value, or null if no arguments.
                ReceivedValues.Add(args.FirstOrDefault().Value);
                return new ValueTask<object>((object)null);
            }
        }
    }
}