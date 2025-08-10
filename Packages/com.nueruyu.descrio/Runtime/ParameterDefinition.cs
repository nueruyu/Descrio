namespace Descrio
{
    public class ParameterDefinition
    {
        public ParameterDefinition(string name, string type, object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string Type { get; } // TODO: "int", "string" などを列挙型で定義
        public object DefaultValue { get; }
    }
}