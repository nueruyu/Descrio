using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Descrio.Generator.Callable.Models
{
    internal class ParameterInfo
    {
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
        public bool HasDefaultValue { get; set; }
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets a value indicating whether this parameter is a complex type that can be mapped from a dictionary.
        /// True if MappableMembers has been populated.
        /// </summary>
        public bool IsMappableComplexType => MappableMembers != null;

        /// <summary>
        /// If this parameter is a mappable complex type, this list contains information
        /// about its public fields and properties. Otherwise, this is null.
        /// </summary>
        public List<MappableMemberInfo> MappableMembers { get; set; }

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