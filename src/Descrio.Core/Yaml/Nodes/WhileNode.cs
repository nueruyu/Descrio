using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhileNode : IStatementNode
    {
        [YamlMember(Alias = "condition")]
        public IExpressionNode Condition { get; set; }

        [YamlMember(Alias = "statements")]
        public IStatementNode[] Statements { get; set; } = System.Array.Empty<IStatementNode>();

        public IStatement ToStatement()
        {
            var instructions = Statements.Select(s => s.ToStatement()).ToArray();
            return new WhileStatement(Condition.ToExpression(), instructions);
        }
    }
}