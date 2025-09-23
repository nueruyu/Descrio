using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Generator.Callable
{
    internal static class Parser
    {
        private const string CallableAttributeName = "Descrio.Attributes.CallableAttribute";

        public static List<CallableClassInfo> GetCallableClasses(GeneratorExecutionContext context, ISyntaxReceiver receiver)
        {
            if (receiver is not SyntaxReceiver syntaxReceiver)
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

                var (isAwaitable, returnsValue) = SymbolAnalyzer.AnalyzeReturnType(compilation, methodSymbol.ReturnType);

                var methodInfo = new CallableMethodInfo
                {
                    MethodName = methodSymbol.Name,
                    CallableName = SymbolAnalyzer.GetCallableName(methodSymbol, attributeData),
                    ReturnType = methodSymbol.ReturnType,
                    IsAwaitable = isAwaitable,
                    ReturnsValue = returnsValue,
                    Parameters = methodSymbol.Parameters.Select(p => new ParameterInfo
                    {
                        Name = p.Name,
                        Type = p.Type,
                        HasDefaultValue = p.HasExplicitDefaultValue,
                        DefaultValue = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue : null,
                        MappableMembers = SymbolAnalyzer.GetMappableMembers(p.Type, context) // Pass context for future diagnostics
                    }).ToList()
                };
                classInfo.Methods.Add(methodInfo);
            }

            return classInfos.Values.ToList();
        }
    }
}