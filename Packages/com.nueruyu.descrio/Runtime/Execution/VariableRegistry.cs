using System;
using System.Collections.Generic;

namespace Descrio
{
    public class VariableRegistry
    {
        private readonly Dictionary<string, object> _variables = new(StringComparer.OrdinalIgnoreCase);
        private readonly VariableRegistry _parent;

        public VariableRegistry(VariableRegistry parent = null)
        {
            _parent = parent;
        }

        public void Set(string name, object value)
        {
            _variables[name] = value;
        }

        public bool TryGet(string name, out object value)
        {
            if (_variables.TryGetValue(name, out value))
            {
                return true;
            }

            if (_parent != null)
            {
                return _parent.TryGet(name, out value);
            }

            value = null;
            return false;
        }
    }
}