using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Descrio.Generator.Callable.Models
{
    internal class CallableClassInfo
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string FullClassName { get; set; }
        public ITypeSymbol TypeSymbol { get; set; }
        public List<CallableMethodInfo> Methods { get; set; } = new List<CallableMethodInfo>();
        public string SafeFileName => FullClassName.Replace("global::", "").Replace(":", "_").Replace(".", "_").Replace("<", "_").Replace(">", "_");
    }
}