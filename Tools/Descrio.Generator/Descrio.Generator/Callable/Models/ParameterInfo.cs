using Microsoft.CodeAnalysis;

namespace Descrio.Generator.Callable.Models
{
    internal class ParameterInfo
    {
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
        public bool HasDefaultValue { get; set; }
        public object DefaultValue { get; set; }
        public string TypeName => Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        public string DefaultValueAsString
        {
            get
            {
                if (!HasDefaultValue)
                    return "default";
                if (DefaultValue == null)
                    return "null";
                if (DefaultValue is string s)
                    return $"\"{s.Replace("\"", "\\\"")}\"";
                if (DefaultValue is bool b)
                    return b ? "true" : "false";
                return $"({TypeName}){DefaultValue}";
            }
        }
    }
}