using System;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Execution
{
    public class CallableRegistry
    {
        private readonly Dictionary<string, ICallable> _callables = new(StringComparer.OrdinalIgnoreCase);
        private readonly CallableRegistry _parent;

        public CallableRegistry(CallableRegistry parent = null)
        {
            _parent = parent;
        }

        public void Register(string name, ICallable callable)
        {
            _callables[name] = callable;
        }

        public bool TryGet(string name, out ICallable callable)
        {
            if (_callables.TryGetValue(name, out callable))
            {
                return true;
            }
            if (_parent != null)
            {
                return _parent.TryGet(name, out callable);
            }
            callable = null;
            return false;
        }

        public IReadOnlyDictionary<string, ICallable> GetDefinedCallables()
        {
            return new Dictionary<string, ICallable>(_callables, _callables.Comparer);
        }
    }
}