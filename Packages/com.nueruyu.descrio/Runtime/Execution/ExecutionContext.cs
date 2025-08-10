using System.Threading;

namespace Descrio
{
    public class ExecutionContext
    {
        public ModulePath CurrentDirectory { get; }
        public VariableRegistry Variables { get; }
        public CallableRegistry Callables { get; }
        public CancellationToken CancellationToken { get; }

        public ExecutionContext(
            ModulePath currentDirectory,
            CallableRegistry callableRegistry,
            VariableRegistry variableRegistry,
            CancellationToken cancellationToken = default)
        {
            CurrentDirectory = currentDirectory;
            Variables = variableRegistry;
            Callables = callableRegistry;
            CancellationToken = cancellationToken;
        }

        public ExecutionContext CreateChildContext()
        {
            var childVariableRegistry = new VariableRegistry(Variables);
            var childCallableRegistry = new CallableRegistry(Callables);

            return new ExecutionContext(
                CurrentDirectory,
                childCallableRegistry,
                childVariableRegistry,
                CancellationToken);
        }

        public ExecutionContext CreateForNewPath(ModulePath newPath)
        {
            return new ExecutionContext(newPath, Callables, Variables, CancellationToken);
        }
    }
}