#nullable enable
using System;
using System.Collections.Generic;

namespace Descrio.Execution.Converters
{
    public static class TypeConverterRegistry
    {
        private static readonly Dictionary<Type, ITypeConverter> _converters = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Registers a type converter for a specific target type.
        /// This is called automatically by the source-generated ModuleInitializer.
        /// </summary>
        public static void Register(Type targetType, ITypeConverter converter)
        {
            lock (_lock)
            {
                _converters[targetType] = converter;
            }
        }

        /// <summary>
        /// Attempts to retrieve a converter for the specified target type.
        /// </summary>
        public static bool TryGetConverter(Type targetType, out ITypeConverter? converter)
        {
            lock (_lock)
            {
                return _converters.TryGetValue(targetType, out converter);
            }
        }
    }
}
#nullable disable