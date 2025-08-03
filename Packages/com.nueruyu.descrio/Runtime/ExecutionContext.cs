using System.Threading;

namespace Descrio
{
    public class ExecutionContext
    {
        public VariableRegistry Variables { get; }
        public CallableRegistry Callables { get; }
        public CancellationToken CancellationToken { get; }

        public ExecutionContext(
            CallableRegistry callableRegistry,
            CancellationToken cancellationToken = default)
        {
            Variables = new VariableRegistry();
            Callables = callableRegistry;
            CancellationToken = cancellationToken;
        }
    }
}