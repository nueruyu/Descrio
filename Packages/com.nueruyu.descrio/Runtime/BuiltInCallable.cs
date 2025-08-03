using System;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public class BuiltInCallable : ICallable
    {
        readonly Func<object[], CancellationToken, ValueTask<object>> _function;

        public BuiltInCallable(Func<object[], CancellationToken, ValueTask<object>> function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public ValueTask<object> CallAsync(object[] args, CancellationToken cancellationToken)
        {
            return _function.Invoke(args, cancellationToken);
        }
    }
}