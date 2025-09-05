using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Descrio.Execution;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio
{
    public class DelegateCallable : ICallable
    {
        private readonly Func<Arguments, ExecutionContext, ValueTask<object>> _function;

        public DelegateCallable(Func<Arguments, ExecutionContext, ValueTask<object>> function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public DelegateCallable(Func<Arguments, CancellationToken, ValueTask<object>> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = (args, context) => function(args, context.CancellationToken);
        }

        public DelegateCallable(Func<Arguments, ExecutionContext, ValueTask> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = async (args, context) =>
            {
                await function(args, context);
                return null;
            };
        }

        public DelegateCallable(Func<Arguments, CancellationToken, ValueTask> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _function = async (args, context) =>
            {
                await function(args, context.CancellationToken);
                return null;
            };
        }

        public ValueTask<object> CallAsync(Arguments args, ExecutionContext context)
        {
            return _function.Invoke(args, context);
        }
    }
}