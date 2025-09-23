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
            var reportedDiagnostics = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var methodSyntax in syntaxReceiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                if (methodSymbol == null)
                    continue;

                var attributeData = methodSymbol.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) ?? false);
                if (attributeData == null)
                    continue;

                var classSymbol = methodSymbol.ContainingType;

                if (methodSymbol.DeclaredAccessibility != Accessibility.Public &&
                    methodSymbol.DeclaredAccessibility != Accessibility.Internal)
                {
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
                        Namespace = classSymbol.ContainingNamespace?.IsGlobalNamespace ?? false ?
                            null :
                            classSymbol.ContainingNamespace?.ToDisplayString(),
                        Methods = new List<CallableMethodInfo>()
                    };
                    classInfos[classSymbol] = classInfo;
                }

                var (isAwaitable, returnsValue) = SymbolAnalyzer.AnalyzeReturnType(compilation, methodSymbol.ReturnType);

                var methodInfo = new CallableMethodInfo
                {
                    MethodSymbol = methodSymbol,
                    MethodName = methodSymbol.Name,
                    CallableName = SymbolAnalyzer.GetCallableName(methodSymbol, attributeData),
                    ReturnType = methodSymbol.ReturnType,
                    IsAwaitable = isAwaitable,
                    ReturnsValue = returnsValue,
                    Parameters = methodSymbol.Parameters.Select(p =>
                    {
                        var mappableMembers = SymbolAnalyzer.IsMappableType(p.Type) ?
                            SymbolAnalyzer.GetMappableMemberInfos(p.Type) :
                            null;

                        var parameterInfo = new ParameterInfo
                        {
                            Name = p.Name,
                            Type = p.Type,
                            HasDefaultValue = p.HasExplicitDefaultValue,
                            DefaultValue = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue : null,
                            MappableMembers = mappableMembers
                        };

                        if (mappableMembers == null)
                        {
                            SymbolAnalyzer.ReportDiagnosticsForMappableType(p.Type, context, reportedDiagnostics);
                        }

                        return parameterInfo;
                    }).ToList()
                };
                classInfo.Methods.Add(methodInfo);
            }

            CheckCallableNameDuplication(classInfos, context);

            return classInfos.Values.ToList();
        }

        static void CheckCallableNameDuplication(Dictionary<ISymbol, CallableClassInfo> classInfos, GeneratorExecutionContext context)
        {
            foreach (var classInfo in classInfos.Values)
            {
                var duplicateGroups = classInfo.Methods
                    .GroupBy(m => m.CallableName)
                    .Where(g => g.Count() > 1);

                foreach (var group in duplicateGroups)
                {
                    var callableName = group.Key;
                    foreach (var methodInfo in group)
                    {
                        var location = methodInfo.MethodSymbol.Locations.FirstOrDefault() ?? Location.None;
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateCallableNameError,
                            location,
                            callableName,
                            classInfo.ClassName));
                    }
                }
            }
        }
    }
}