using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Descrio.Execution;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio
{
    public interface ICallable
    {
        ValueTask<object> CallAsync(Arguments args, ExecutionContext context);
    }
}