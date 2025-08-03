using System;
using System.Collections.Generic;

namespace Descrio
{
    public class VariableRegistry
    {
        readonly Dictionary<string, object> _variables =
           new(StringComparer.OrdinalIgnoreCase);

        public void Set(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _variables[name] = value;
        }

        public bool TryGet(string name, out object value)
        {
            return _variables.TryGetValue(name, out value);
        }
    }
}