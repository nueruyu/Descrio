using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Generator.Callable
{
    internal static class Parser
    {
        private const string CallableAttributeName = "Descrio.CallableAttribute";

        public static List<CallableClassInfo> GetCallableClasses(GeneratorExecutionContext context, ISyntaxReceiver receiver)
        {
            if (!(receiver is SyntaxReceiver syntaxReceiver))
            {
                return new List<CallableClassInfo>();
            }

            var compilation = context.Compilation;
            var attributeSymbol = compilation.GetTypeByMetadataName(CallableAttributeName);
            if (attributeSymbol == null)
                return new List<CallableClassInfo>();

            var classInfos = new Dictionary<ISymbol, CallableClassInfo>(SymbolEqualityComparer.Default);

            foreach (var methodSyntax in syntaxReceiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                if (methodSymbol == null)
                    continue;

                var attributeData = methodSymbol.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
                if (attributeData == null)
                    continue;

                var classSymbol = methodSymbol.ContainingType;

                if (methodSymbol.DeclaredAccessibility != Accessibility.Public &&
                    methodSymbol.DeclaredAccessibility != Accessibility.Internal)
                {
                    // Skip private/protected methods as they cannot be accessed from the generated internal wrapper class.
                    continue;
                }

                if (classSymbol.DeclaredAccessibility != Accessibility.Public &&
                    classSymbol.DeclaredAccessibility != Accessibility.Internal)
                {
                    continue;
                }

                if (!classInfos.TryGetValue(classSymbol, out var classInfo))
                {
                    classInfo = new CallableClassInfo
                    {
                        ClassName = classSymbol.Name,
                        FullClassName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Accessibility = classSymbol.DeclaredAccessibility,
                        Namespace = classSymbol.ContainingNamespace.IsGlobalNamespace ?
                            null :
                            classSymbol.ContainingNamespace.ToDisplayString(),
                        Methods = new List<CallableMethodInfo>()
                    };
                    classInfos[classSymbol] = classInfo;
                }

                var (isAwaitable, returnsValue) = AnalyzeReturnType(compilation, methodSymbol.ReturnType);

                var methodInfo = new CallableMethodInfo
                {
                    MethodName = methodSymbol.Name,
                    CallableName = GetCallableName(methodSymbol, attributeData),
                    ReturnType = methodSymbol.ReturnType,
                    IsAwaitable = isAwaitable,
                    ReturnsValue = returnsValue,
                    Parameters = methodSymbol.Parameters.Select(p => new ParameterInfo
                    {
                        Name = p.Name,
                        Type = p.Type,
                        HasDefaultValue = p.HasExplicitDefaultValue,
                        DefaultValue = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue : null
                    }).ToList()
                };
                classInfo.Methods.Add(methodInfo);
            }

            return classInfos.Values.ToList();
        }

        private static (bool IsAwaitable, bool ReturnsValue) AnalyzeReturnType(Compilation compilation, ITypeSymbol type)
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

        private static string GetCallableName(IMethodSymbol methodSymbol, AttributeData attributeData)
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
    }
}