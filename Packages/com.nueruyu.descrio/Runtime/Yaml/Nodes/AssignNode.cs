using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class AssignNode : IStatementNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        public IStatement ToStatement() => new AssignStatement(Name, Value.ToExpression());
    }
}