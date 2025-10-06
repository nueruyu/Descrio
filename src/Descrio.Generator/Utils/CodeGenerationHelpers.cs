using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Descrio.Generator.Utils
{
    internal static class CodeGenerationHelpers
    {
        public static string ToFullTypeName(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        

        public static string GetCastExpression(ITypeSymbol typeSymbol, string variableName)
        {
            var typeName = typeSymbol.ToFullTypeName();
            if (typeSymbol.IsValueType && typeSymbol.NullableAnnotation != NullableAnnotation.Annotated)
            {
                return $"({typeName}){variableName}!";
            }
            return $"({typeName}?){variableName}";
        }

        public static string GetParameterDefaultValueLiteral(ParameterInfo paramInfo)
        {
            if (!paramInfo.HasDefaultValue)
                return "default";

            if (paramInfo.DefaultValue == null)
                return "null";

            if (paramInfo.DefaultValue is string s)
            {
                return $"\"{s.Replace("\"", "\\\"")}\"";
            }

            if (paramInfo.DefaultValue is bool b)
            {
                return b ? "true" : "false";
            }

            // For other literal types (numeric, etc.), their default C# literal representation is sufficient.
            // Explicit cast might be needed if the type is ambiguous without it (e.g., float vs double).
            if (paramInfo.DefaultValue is float f)
                return $"{f}f";

            return $"({paramInfo.Type.ToFullTypeName()}){paramInfo.DefaultValue}";
        }

        public static bool IsGenericList(ITypeSymbol type, out ITypeSymbol? elementType)
        {
            if (type is INamedTypeSymbol { IsGenericType: true } namedType &&
                namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.List<T>")
            {
                elementType = namedType.TypeArguments[0];
                return true;
            }
            elementType = null;
            return false;
        }

        public static bool IsArray(ITypeSymbol type, out ITypeSymbol? elementType)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                elementType = arrayType.ElementType;
                return true;
            }
            elementType = null;
            return false;
        }

        public static bool IsGenericDictionary(ITypeSymbol type, out ITypeSymbol? keyType, out ITypeSymbol? valueType)
        {
            if (type is INamedTypeSymbol { IsGenericType: true } namedType &&
                namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
            {
                keyType = namedType.TypeArguments[0];
                valueType = namedType.TypeArguments[1];
                return true;
            }
            keyType = null;
            valueType = null;
            return false;
        }
    }
}