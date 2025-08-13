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
            // 1. Parse the compilation to get a list of model objects
            var callableClasses = Parser.GetCallableClasses(context, context.SyntaxReceiver);
            if (!callableClasses.Any())
            {
                return;
            }

            // 2. Build the source code for each wrapper class
            foreach (var classInfo in callableClasses)
            {
                foreach (var methodInfo in classInfo.Methods)
                {
                    var wrapperSource = CodeBuilder.BuildWrapperClass(classInfo, methodInfo);
                    var sourceHintName = $"__Descrio_{classInfo.SafeFileName}_{methodInfo.MethodName}_Callable.g.cs";
                    context.AddSource(sourceHintName, SourceText.From(wrapperSource, Encoding.UTF8));
                }
            }

            // 3. Build the registration class source code
            var registrationSource = CodeBuilder.BuildRegistrationClass(callableClasses);
            context.AddSource("__DescrioAddCallablesExtensions.g.cs", SourceText.From(registrationSource, Encoding.UTF8));
        }
    }
}