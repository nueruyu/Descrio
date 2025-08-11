using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public interface ICallable
    {
        ValueTask<object> CallAsync(Arguments args, ExecutionContext context);
    }
}