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
        public List<MappableMemberInfo> MappableMembers { get; set; }

        public string TypeName => Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        public bool IsMappableComplexType => MappableMembers != null;
    }
}