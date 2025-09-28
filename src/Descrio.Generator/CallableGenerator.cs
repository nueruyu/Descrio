using Descrio.Generator.Callable;
using Descrio.Generator.Callable.Emitter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Descrio.Generator
{
    [Generator]
    public class CallableGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var callableClasses = Parser.GetCallableClasses(context, context.SyntaxReceiver!);
            if (!callableClasses.Any())
            {
                return;
            }

            var allMappableTypes = CollectAllMappableTypes(callableClasses);

            if (allMappableTypes.Any())
            {
                foreach (var type in allMappableTypes)
                {
                    var converterSource = TypeConverterEmitter.BuildTypeConverterClass(type, allMappableTypes);
                    var safeName = type.ToDisplayString()
                        .Replace("global::", "")
                        .Replace(".", "_")
                        .Replace("<", "_")
                        .Replace(">", "_")
                        .Replace("?", "_");

                    context.AddSource($"__Descrio_{safeName}_Converter.g.cs", SourceText.From(converterSource, Encoding.UTF8));
                }

                var initializerSource = InitializerEmitter.BuildInitializerClass(allMappableTypes);
                context.AddSource("__DescrioGeneratedInitializers.g.cs", SourceText.From(initializerSource, Encoding.UTF8));
            }

            foreach (var classInfo in callableClasses)
            {
                foreach (var methodInfo in classInfo.Methods)
                {
                    var wrapperSource = CallableWrapperEmitter.BuildWrapperClass(classInfo, methodInfo);
                    var sourceHintName = $"__Descrio_{classInfo.SafeFileName}_{methodInfo.MethodName}_Callable.g.cs";
                    context.AddSource(sourceHintName, SourceText.From(wrapperSource, Encoding.UTF8));
                }
            }

            var publicClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Public).ToList();
            if (publicClasses.Any())
            {
                var registrationSource = ExtensionMethodEmitter.BuildRegistrationClass(
                    "public",
                    "__DescrioPublicAddCallablesExtensions",
                    publicClasses);
                context.AddSource("__DescrioPublicAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }

            var internalClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Internal).ToList();
            if (internalClasses.Any())
            {
                var registrationSource = ExtensionMethodEmitter.BuildRegistrationClass(
                    "internal",
                    "__DescrioInternalAddCallablesExtensions",
                    internalClasses);
                context.AddSource("__DescrioInternalAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }
        }

        private IReadOnlyCollection<ITypeSymbol> CollectAllMappableTypes(List<Callable.Models.CallableClassInfo> callableClasses)
        {
            var collectedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var typesToProcess = new Queue<ITypeSymbol>();

            var initialTypes = callableClasses
                .SelectMany(c => c.Methods)
                .SelectMany(m => m.Parameters)
                .Where(p => p.IsMappableComplexType)
                .Select(p => p.Type);

            foreach (var type in initialTypes)
            {
                typesToProcess.Enqueue(type);
            }

            while (typesToProcess.Count > 0)
            {
                var currentType = typesToProcess.Dequeue();
                if (currentType == null || collectedTypes.Contains(currentType))
                {
                    continue;
                }

                if (currentType is IArrayTypeSymbol arrayType)
                {
                    typesToProcess.Enqueue(arrayType.ElementType);
                    continue;
                }
                if (currentType is INamedTypeSymbol namedType && namedType.IsGenericType)
                {
                    foreach (var typeArg in namedType.TypeArguments)
                    {
                        typesToProcess.Enqueue(typeArg);
                    }
                }

                if (SymbolAnalyzer.IsMappableType(currentType))
                {
                    collectedTypes.Add(currentType);

                    foreach (var memberType in SymbolAnalyzer.GetAllMappableMemberTypes(currentType))
                    {
                        typesToProcess.Enqueue(memberType);
                    }
                }
            }
            return collectedTypes;
        }
    }
}