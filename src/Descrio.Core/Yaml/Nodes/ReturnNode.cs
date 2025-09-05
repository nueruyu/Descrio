using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ReturnNode : IStatementNode
    {
        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; } // Can be null

        public IStatement ToStatement() => new ReturnStatement(Value?.ToExpression());
    }
}