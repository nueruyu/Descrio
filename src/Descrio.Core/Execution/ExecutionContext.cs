using Descrio.Data;
using Descrio.Execution.Registries;
using System.Threading;

namespace Descrio.Execution
{
    public class ExecutionContext
    {
        public ModulePath CurrentDirectory { get; }
        public VariableRegistry Variables { get; }
        public CallableRegistry Callables { get; }
        public ClassRegistry Classes { get; }
        public CancellationToken CancellationToken { get; }

        public ExecutionContext(
            ModulePath currentDirectory,
            CallableRegistry callableRegistry,
            VariableRegistry variableRegistry,
            ClassRegistry classRegistry,
            CancellationToken cancellationToken = default)
        {
            CurrentDirectory = currentDirectory;
            Variables = variableRegistry;
            Callables = callableRegistry;
            Classes = classRegistry;
            CancellationToken = cancellationToken;
        }

        public ExecutionContext CreateChildContext()
        {
            return CreateChildContext(CancellationToken);
        }

        public ExecutionContext CreateChildContext(CancellationToken cancellationToken)
        {
            var childVariableRegistry = new VariableRegistry(Variables);
            var childCallableRegistry = new CallableRegistry(Callables);
            var childClassRegistry = new ClassRegistry(Classes);

            return new ExecutionContext(
                CurrentDirectory,
                childCallableRegistry,
                childVariableRegistry,
                childClassRegistry,
                cancellationToken);
        }
    }
}