using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class LetNode : IStatementNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        // [YamlMember(Alias = "type")]
        // public string Type { get; set; }

        public IStatement ToStatement() => new LetStatement(Name, Value.ToExpression());
    }
}