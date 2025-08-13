using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhenCaseBlockNode
    {
        [YamlMember(Alias = "condition")]
        public IExpressionNode Condition { get; set; }

        [YamlMember(Alias = "then")]
        public List<IStatementNode> ThenBlock { get; set; }

        public WhenCaseBlock ToCaseBlock()
        {
            var instructions = ThenBlock?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();
            return new WhenCaseBlock(Condition?.ToExpression(), instructions);
        }
    }
}