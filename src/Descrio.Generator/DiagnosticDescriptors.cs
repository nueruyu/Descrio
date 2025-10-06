using Microsoft.CodeAnalysis;

namespace Descrio.Generator
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MissingParameterlessConstructorWarning = new(
            id: "DESCGEN001",
            title: "Mappable type is missing a parameterless constructor",
            messageFormat: "The type '{0}' requires a public parameterless constructor to be used as a mappable parameter in a [Callable] method. No TypeConverter will be generated.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A public parameterless constructor is necessary for the source generator to create an instance of the type before mapping properties from the script.");

        public static readonly DiagnosticDescriptor DuplicateCallableNameError = new(
            id: "DESCGEN002",
            title: "Duplicate callable name detected",
            messageFormat: "The callable name '{0}' is already used by another method in the class '{1}'. Callable names must be unique within a class.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}