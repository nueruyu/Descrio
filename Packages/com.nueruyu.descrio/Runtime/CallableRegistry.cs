using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public class CallableRegistry
    {
        readonly Dictionary<string, ICallable> _callables =
           new(StringComparer.OrdinalIgnoreCase);

        public void Register(string name, Func<object[], CancellationToken, ValueTask<object>> function)
        {
            Register(name, new BuiltInCallable(function));
        }

        public void Register(string name, ICallable callable)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (callable is null)
                throw new ArgumentNullException(nameof(callable));

            _callables[name] = callable;
        }

        public bool TryGet(string name, out ICallable callable)
        {
            return _callables.TryGetValue(name, out callable);
        }
    }
}