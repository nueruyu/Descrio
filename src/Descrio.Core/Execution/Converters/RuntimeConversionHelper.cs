#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Descrio.Execution.Converters
{
    [ExcludeFromCodeCoverage]
    public static class RuntimeConversionHelper
    {
        public static T? ConvertItem<T>(object? item)
        {
            var targetType = typeof(T);

            if (item == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                {
                    throw new InvalidCastException($"Cannot convert null to non-nullable type '{targetType.FullName}'.");
                }
                return default;
            }
            
            if (item is T correctlyTypedItem)
            {
                return correctlyTypedItem;
            }

            if (TypeConverterRegistry.TryGetConverter(targetType, out var converter))
            {
                return (T?)converter.Convert(item);
            }

            // Special handling for enums from strings
            if (targetType.IsEnum && item is string enumString)
            {
                return (T?)Enum.Parse(targetType, enumString, true);
            }

            return (T?)Convert.ChangeType(item, targetType);
        }
    }
}
#nullable disable