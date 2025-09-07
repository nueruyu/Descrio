namespace Descrio
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
        public string Type { get; } // TODO: "int", "string" などを列挙型で定義
        public object DefaultValue { get; }
        public SourceRange Location { get; }
    }
}