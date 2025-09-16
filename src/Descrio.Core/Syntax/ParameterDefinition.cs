using Descrio.Abstractions;
using Descrio.Data;

namespace Descrio.Syntax
{
    public class ParameterDefinition : ISyntaxNode
    {
        public ParameterDefinition(string name, string type, object defaultValue, SourceRange location = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Location = location;
        }

        public string Name { get; }
        public string Type { get; } // TODO: typing support
        public object DefaultValue { get; }
        public SourceRange Location { get; }
    }
}