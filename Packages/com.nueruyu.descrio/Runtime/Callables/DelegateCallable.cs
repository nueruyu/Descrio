using System;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public class DelegateCallable : ICallable
    {
        private readonly Func<object[], ExecutionContext, ValueTask<object>> _function;

        public DelegateCallable(Func<object[], ExecutionContext, ValueTask<object>> function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public DelegateCallable(Func<object[], CancellationToken, ValueTask<object>> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = (args, context) => function(args, context.CancellationToken);
        }

        public DelegateCallable(Func<object[], ExecutionContext, ValueTask> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = async (args, context) =>
            {
                await function(args, context);
                return null;
            };
        }

        public DelegateCallable(Func<object[], CancellationToken, ValueTask> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = async (args, context) =>
            {
                await function(args, context.CancellationToken);
                return null;
            };
        }

        public ValueTask<object> CallAsync(object[] args, ExecutionContext context)
        {
            return _function.Invoke(args, context);
        }
    }
}