using System.Threading.Tasks;
using Descrio.Data;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio.Abstractions
{
    public interface ICallable
    {
        ValueTask<object> CallAsync(Arguments args, ExecutionContext context);
    }
}