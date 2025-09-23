using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Generator.Callable
{
    internal static class SymbolAnalyzer
    {
        private const string CallableAttributeName = "Descrio.Attributes.CallableAttribute";

        public static string GetCallableName(IMethodSymbol methodSymbol, AttributeData attributeData)
        {
            // [Callable] -> methodSymbol.Name
            // [Callable(null)] -> methodSymbol.Name
            // [Callable("")] -> methodSymbol.Name
            // [Callable("MyName")] -> "MyName"
            // [Callable(Name = "MyName")] -> "MyName"

            // First, check for named arguments like [Callable(Name = "MyName")]
            var namedArgument = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == "Name");
            if (namedArgument.Key != null &&
                namedArgument.Value.Value is string namedArgValue &&
                !string.IsNullOrEmpty(namedArgValue))
            {
                return namedArgValue;
            }

            // If not found, check for constructor arguments like [Callable("MyName")]
            var constructorArgument = attributeData.ConstructorArguments.FirstOrDefault();
            if (constructorArgument.Value is string ctorArgValue &&
                !string.IsNullOrEmpty(ctorArgValue))
            {
                return ctorArgValue;
            }

            // If no name is provided, use the method name itself.
            return methodSymbol.Name;
        }

        public static (bool IsAwaitable, bool ReturnsValue) AnalyzeReturnType(Compilation compilation, ITypeSymbol type)
        {
            if (type == null)
                return (false, false);

            var getAwaiterMethod = type.GetMembers("GetAwaiter")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    !m.IsStatic &&
                    m.Parameters.Length == 0 &&
                    m.DeclaredAccessibility == Accessibility.Public);

            if (getAwaiterMethod == null)
            {
                return (false, type.SpecialType != SpecialType.System_Void);
            }

            var awaiterType = getAwaiterMethod.ReturnType;
            if (awaiterType == null)
                return (false, type.SpecialType != SpecialType.System_Void);

            var isCompletedProperty = awaiterType.GetMembers("IsCompleted")
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Type.SpecialType == SpecialType.System_Boolean);

            var notifyCompletion = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.INotifyCompletion");
            var getResultMethod = awaiterType.GetMembers("GetResult")
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (isCompletedProperty == null ||
                notifyCompletion == null ||
                !awaiterType.AllInterfaces.Contains(notifyCompletion, SymbolEqualityComparer.Default) ||
                getResultMethod == null)
            {
                return (false, type.SpecialType != SpecialType.System_Void);
            }

            bool returnsValue = getResultMethod.ReturnType.SpecialType != SpecialType.System_Void;
            return (true, returnsValue);
        }

        public static List<MappableMemberInfo>? GetMappableMembers(ITypeSymbol type, GeneratorExecutionContext? context = null)
        {
            if (type == null)
                return null;

            if (type.SpecialType != SpecialType.None)
            {
                return null;
            }

            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            {
                return null;
            }

            if (type.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.List<T>" or "System.Collections.Generic.Dictionary<TKey, TValue>")
            {
                return null;
            }

            // The class must have a public parameterless constructor to be instantiated.
            bool hasDefaultConstructor = type.IsValueType || type.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Length == 0 && m.DeclaredAccessibility == Accessibility.Public);

            if (!hasDefaultConstructor)
            {
                // TODO: Add a diagnostic warning here if context is not null.
                return null;
            }

            var members = new List<MappableMemberInfo>();

            // Collect public properties with a public setter.
            foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.DeclaredAccessibility == Accessibility.Public && prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                {
                    members.Add(new MappableMemberInfo
                    {
                        Name = prop.Name,
                        TypeName = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    });
                }
            }

            // Collect public, non-readonly fields.
            foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.DeclaredAccessibility == Accessibility.Public && !field.IsReadOnly)
                {
                    members.Add(new MappableMemberInfo
                    {
                        Name = field.Name,
                        TypeName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    });
                }
            }

            // Return the list of members, or null if no mappable members were found.
            return members.Any() ? members : null;
        }
    }
}