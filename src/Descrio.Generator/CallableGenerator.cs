using Descrio.Generator.Callable;
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
            // Parse the compilation to get a list of model objects
            var callableClasses = Parser.GetCallableClasses(context, context.SyntaxReceiver);
            if (!callableClasses.Any())
            {
                return;
            }

            // Build the source code for each wrapper class
            foreach (var classInfo in callableClasses)
            {
                foreach (var methodInfo in classInfo.Methods)
                {
                    var wrapperSource = CodeBuilder.BuildWrapperClass(classInfo, methodInfo);
                    var sourceHintName = $"__Descrio_{classInfo.SafeFileName}_{methodInfo.MethodName}_Callable.g.cs";
                    context.AddSource(sourceHintName, SourceText.From(wrapperSource, Encoding.UTF8));
                }
            }

            // Group classes by their accessibility
            var publicClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Public).ToList();
            var internalClasses = callableClasses.Where(c => c.Accessibility == Accessibility.Internal).ToList();

            if (publicClasses.Any())
            {
                var registrationSource = Callable.CodeBuilder.BuildRegistrationClass(
                    "public",
                    "__DescrioPublicAddCallablesExtensions",
                    publicClasses);
                context.AddSource("__DescrioPublicAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }

            if (internalClasses.Any())
            {
                var registrationSource = Callable.CodeBuilder.BuildRegistrationClass(
                    "internal",
                    "__DescrioInternalAddCallablesExtensions",
                    internalClasses);
                context.AddSource("__DescrioInternalAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
            }
        }
    }
}