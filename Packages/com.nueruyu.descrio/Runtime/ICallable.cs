using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public interface ICallable
    {
        ValueTask<object> CallAsync(object[] args, ExecutionContext context);
    }
}