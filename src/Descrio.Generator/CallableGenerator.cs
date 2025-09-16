using Descrio.Generator.Callable;
using Descrio.Generator.Callable.Models;
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
            // 1. Parse the compilation to get a list of model objects
            var callableClasses = Parser.GetCallableClasses(context, context.SyntaxReceiver);
            if (!callableClasses.Any())
            {
                return;
            }

            // 2. Collect all unique types that need a converter, recursively.
            var allMappableTypes = CollectAllMappableTypes(callableClasses);

            // 3. Build and add the source code for each TypeConverter class
            if (allMappableTypes.Any())
            {
                foreach (var type in allMappableTypes)
                {
                    var converterSource = ConverterBuilder.BuildTypeConverterClass(type, allMappableTypes);
                    var safeName = type.ToDisplayString().Replace("global::", "").Replace(".", "_").Replace("<", "_").Replace(">", "_");
                    context.AddSource($"__Descrio_{safeName}_Converter.g.cs", SourceText.From(converterSource, Encoding.UTF8));
                }

                // 4. Build and add the initializer class that registers all converters
                var initializerSource = ConverterBuilder.BuildInitializerClass(allMappableTypes);
                context.AddSource("__DescrioGeneratedInitializers.g.cs", SourceText.From(initializerSource, Encoding.UTF8));
            }

            // 5. Build and add the source code for each [Callable] wrapper class
            foreach (var classInfo in callableClasses)
            {
                foreach (var methodInfo in classInfo.Methods)
                {
                    var wrapperSource = CodeBuilder.BuildWrapperClass(classInfo, methodInfo);
                    var sourceHintName = $"__Descrio_{classInfo.SafeFileName}_{methodInfo.MethodName}_Callable.g.cs";
                    context.AddSource(sourceHintName, SourceText.From(wrapperSource, Encoding.UTF8));
                }
            }

            // 6. Build and add the AddCallables extension methods
            var publicClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Public).ToList();
            var internalClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Internal).ToList();

            if (publicClasses.Any())
            {
                var registrationSource = CodeBuilder.BuildRegistrationClass(
                    "public",
                    "__DescrioPublicAddCallablesExtensions",
                    publicClasses);
                context.AddSource("__DescrioPublicAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }

            if (internalClasses.Any())
            {
                var registrationSource = CodeBuilder.BuildRegistrationClass(
                    "internal",
                    "__DescrioInternalAddCallablesExtensions",
                    internalClasses);
                context.AddSource("__DescrioInternalAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }
        }

        private IReadOnlyCollection<ITypeSymbol> CollectAllMappableTypes(List<CallableClassInfo> callableClasses)
        {
            var collectedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var typesToProcess = new Queue<ITypeSymbol>();

            // Start with the direct parameter types from all [Callable] methods
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

                // If it's a collection, process its element type
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

                // Check if the type itself is mappable. If so, add it and its members to the queue.
                var members = Parser.GetMappableMembers(currentType);
                if (members != null)
                {
                    collectedTypes.Add(currentType);

                    foreach (var memberSymbol in currentType.GetMembers().Where(m => m is IPropertySymbol || m is IFieldSymbol))
                    {
                        (_, ITypeSymbol? memberType, bool canWrite) = ConverterBuilder.GetMemberInfo(memberSymbol);
                        if (canWrite && memberType != null)
                        {
                            typesToProcess.Enqueue(memberType);
                        }
                    }
                }
            }
            return collectedTypes;
        }
    }
}