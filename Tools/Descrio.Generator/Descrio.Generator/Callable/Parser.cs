using Descrio.Generator.Callable.Models;
using Microsoft.CodeAnalysis;
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

                var attribute = methodSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
                if (attribute == null)
                    continue;

                var classSymbol = methodSymbol.ContainingType;
                if (classSymbol.DeclaredAccessibility != Accessibility.Public &&
                    classSymbol.DeclaredAccessibility != Accessibility.Internal)
                {
                    continue;
                }

                if (!classInfos.TryGetValue(classSymbol, out var classInfo))
                {
                    classInfo = new CallableClassInfo
                    {
                        TypeSymbol = classSymbol,
                        ClassName = classSymbol.Name,
                        FullClassName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Namespace = classSymbol.ContainingNamespace.IsGlobalNamespace ? null : classSymbol.ContainingNamespace.ToDisplayString()
                    };
                    classInfos[classSymbol] = classInfo;
                }

                var methodInfo = new CallableMethodInfo
                {
                    MethodName = methodSymbol.Name,
                    CallableName = attribute.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? methodSymbol.Name,
                    ReturnType = methodSymbol.ReturnType,
                };

                foreach (var paramSymbol in methodSymbol.Parameters)
                {
                    methodInfo.Parameters.Add(new ParameterInfo
                    {
                        Name = paramSymbol.Name,
                        Type = paramSymbol.Type,
                        HasDefaultValue = paramSymbol.HasExplicitDefaultValue,
                        DefaultValue = paramSymbol.HasExplicitDefaultValue ? paramSymbol.ExplicitDefaultValue : null
                    });
                }

                classInfo.Methods.Add(methodInfo);
            }

            return classInfos.Values.ToList();
        }
    }
}