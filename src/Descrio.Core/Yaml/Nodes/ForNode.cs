using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ForNode : IStatementNode
    {
        [YamlMember(Alias = "in")]
        public IExpressionNode In { get; set; }

        [YamlMember(Alias = "as")]
        public string As { get; set; }

        [YamlMember(Alias = "statements")]
        public IStatementNode[] Statements { get; set; } = System.Array.Empty<IStatementNode>();

        public IStatement ToStatement()
        {
            var instructions = Statements.Select(s => s.ToStatement()).ToArray();
            return new ForStatement(In.ToExpression(), As, instructions);
        }
    }
}