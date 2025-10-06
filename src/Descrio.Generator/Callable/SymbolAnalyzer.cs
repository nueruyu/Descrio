using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Generator.Callable
{
    internal static class SymbolAnalyzer
    {
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

        public static bool IsMappableType(ITypeSymbol type)
        {
            if (!IsMappableCandidate(type) || !HasPublicParameterlessConstructor(type))
            {
                return false;
            }

            return true;
        }

        public static List<MappableMemberInfo>? GetMappableMemberInfos(ITypeSymbol type)
        {
            return GetAllMappableMembers(type)
                .Select(GetMemberInfo)
                .Where(x => x.Name != null)
                .Select(m => new MappableMemberInfo { Name = m.Name!, TypeName = m.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) })
                .ToList();
        }

        public static void ReportDiagnosticsForMappableType(ITypeSymbol type, GeneratorExecutionContext context, HashSet<ITypeSymbol> reportedDiagnostics)
        {
            if (!IsMappableCandidate(type) || reportedDiagnostics.Contains(type))
            {
                return;
            }

            if (!HasPublicParameterlessConstructor(type))
            {
                var location = type.Locations.FirstOrDefault() ?? Location.None;
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingParameterlessConstructorWarning,
                    location,
                    type.Name));

                reportedDiagnostics.Add(type);
            }
        }

        public static (string? Name, ITypeSymbol? Type, bool CanWrite) GetMemberInfo(ISymbol symbol)
        {
            if (symbol is IPropertySymbol prop && prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                return (prop.Name, prop.Type, true);
            }
            if (symbol is IFieldSymbol field && !field.IsReadOnly && field.DeclaredAccessibility == Accessibility.Public)
            {
                return (field.Name, field.Type, true);
            }
            return (null, null, false);
        }

        public static IEnumerable<ISymbol> GetAllMappableMembers(ITypeSymbol type)
        {
            if (!IsMappableCandidate(type) || !HasPublicParameterlessConstructor(type))
            {
                return Enumerable.Empty<ISymbol>();
            }

            return GetAllMembers(type)
                .Where(x =>
                {
                    var m = GetMemberInfo(x);
                    return m.CanWrite && m.Type != null;
                })
                .GroupBy(m => m.Name)
                .Select(g => g.First());
        }

        public static IEnumerable<ITypeSymbol> GetAllMappableMemberTypes(ITypeSymbol type)
        {
            if (!IsMappableCandidate(type) || !HasPublicParameterlessConstructor(type))
            {
                return Enumerable.Empty<ITypeSymbol>();
            }

            return GetAllMappableMembers(type)
                .Select(GetMemberInfo)
                .Select(x => x.Type!);
        }

        private static IEnumerable<ISymbol> GetAllMembers(ITypeSymbol type)
        {
            var members = new List<ISymbol>(type.GetMembers());
            var baseType = type.BaseType;
            while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
            {
                members.AddRange(baseType.GetMembers());
                baseType = baseType.BaseType;
            }
            return members;
        }

        private static bool IsMappableCandidate(ITypeSymbol type)
        {
            if (type == null || type.SpecialType != SpecialType.None)
            {
                return false;
            }

            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            if (type.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.List<T>" or "System.Collections.Generic.Dictionary<TKey, TValue>")
            {
                return false;
            }

            return true;
        }

        private static bool HasPublicParameterlessConstructor(ITypeSymbol type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            return type.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Length == 0 && m.DeclaredAccessibility == Accessibility.Public);
        }
    }
}