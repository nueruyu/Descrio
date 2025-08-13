using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class AssertNode : IStatementNode
    {
        [YamlMember(Alias = "condition")]
        public IExpressionNode Condition { get; set; }

        [YamlMember(Alias = "message")]
        public string Message { get; set; }

        public IStatement ToStatement() => new AssertStatement(Condition.ToExpression(), Message);
    }
}