using System;
using System.Collections.Generic;

namespace Descrio.Execution
{
    public class VariableRegistry
    {
        private class Variable
        {
            public object Value;
            public readonly bool IsMutable;

            public Variable(object value, bool isMutable)
            {
                Value = value;
                IsMutable = isMutable;
            }
        }

        private readonly Dictionary<string, Variable> _variables = new(StringComparer.Ordinal);
        private readonly VariableRegistry _parent;

        public VariableRegistry(VariableRegistry parent = null)
        {
            _parent = parent;
        }

        public void Define(string name, object value, bool isMutable = true)
        {
            if (_variables.ContainsKey(name))
            {
                throw new InvalidOperationException($"Variable '{name}' is already defined in this scope.");
            }
            _variables[name] = new Variable(value, isMutable);
        }

        public void Assign(string name, object value)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                if (!variable.IsMutable)
                {
                    throw new InvalidOperationException($"Cannot assign to immutable variable '{name}' defined with '!let'.");
                }
                variable.Value = value;
                return;
            }

            if (_parent != null)
            {
                _parent.Assign(name, value);
                return;
            }

            throw new InvalidOperationException($"Variable '{name}' is not defined.");
        }

        public object Get(string name)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                return variable.Value;
            }

            if (_parent != null)
            {
                return _parent.Get(name);
            }

            throw new InvalidOperationException($"Variable '{name}' is not defined.");
        }

        public bool TryGet(string name, out object value)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                value = variable.Value;
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