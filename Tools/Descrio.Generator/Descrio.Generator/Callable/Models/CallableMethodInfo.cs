using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Descrio.Generator.Callable.Models
{
    internal class CallableMethodInfo
    {
        public string MethodName { get; set; }
        public string CallableName { get; set; }
        public ITypeSymbol ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    }
}