using System;
using System.Collections.Generic;

namespace Descrio.Execution
{
    public class ClassRegistry
    {
        private readonly Dictionary<string, Type> _classes = new(StringComparer.Ordinal);
        private readonly ClassRegistry _parent;

        public ClassRegistry(ClassRegistry parent = null)
        {
            _parent = parent;
        }

        public void Register(string name, Type type)
        {
            _classes[name] = type;
        }

        public bool TryGet(string name, out Type type)
        {
            if (_classes.TryGetValue(name, out type))
            {
                return true;
            }

            if (_parent != null)
            {
                return _parent.TryGet(name, out type);
            }

            type = null;
            return false;
        }

        public IReadOnlyDictionary<string, Type> GetDefinedClasses()
        {
            return new Dictionary<string, Type>(_classes, _classes.Comparer);
        }
    }
}