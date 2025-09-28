using System;
using System.Collections.Generic;
using System.Threading;

namespace Descrio.Execution.Converters
{
    /// <summary>
    /// A central, static registry for all type converters used by Descrio.
    /// It features thread-safe lazy initialization to register all source-generated converters
    /// just before they are first needed.
    /// </summary>
    public static partial class TypeConverterRegistry
    {
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, ITypeConverter> _converters = new Dictionary<Type, ITypeConverter>();

        /// <summary>
        /// Ensures that all source-generated converters are registered.
        /// This method is called internally before any registry operation.
        /// </summary>
        private static void EnsureInitialized()
        {
            // Use double-checked locking for thread-safe and performant lazy initialization.
            if (_isInitialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_isInitialized)
                {
                    return;
                }

                InitializeConverters();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Registers a type converter for a specific target type.
        /// If a converter for the type already exists, it will be overwritten.
        /// </summary>
        /// <param name="targetType">The type that the converter can produce.</param>
        /// <param name="converter">The converter instance.</param>
        public static void Register(Type targetType, ITypeConverter converter)
        {
            EnsureInitialized();

            lock (_lock)
            {
                _converters[targetType] = converter;
            }
        }

        /// <summary>
        /// Attempts to retrieve a converter for the specified target type.
        /// </summary>
        /// <param name="targetType">The type to find a converter for.</param>
        /// <param name="converter">The found converter, or null if not found.</param>
        /// <returns>True if a converter was found, otherwise false.</returns>
        public static bool TryGetConverter(Type targetType, out ITypeConverter converter)
        {
            EnsureInitialized();

            lock (_lock)
            {
                return _converters.TryGetValue(targetType, out converter);
            }
        }

        /// <summary>
        /// A partial method that the source generator will implement.
        /// This method will contain all the calls to Register() for the auto-generated converters.
        /// </summary>
        static partial void InitializeConverters();

        /// <summary>
        /// Internal registration method for use by the source generator only.
        /// This method bypasses the initialization check to prevent infinite loops.
        /// It is NOT thread-safe and must only be called from within the lock of EnsureInitialized.
        /// </summary>
        private static void RegisterCore(Type targetType, ITypeConverter converter)
        {
            _converters[targetType] = converter;
        }
    }
}