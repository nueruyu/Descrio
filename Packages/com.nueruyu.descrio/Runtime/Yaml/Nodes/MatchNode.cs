using Descrio.Parse;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class MatchCaseNode
    {
        [YamlMember(Alias = "case")]
        public object Case { get; set; }

        [YamlMember(Alias = "then")]
        public List<IStatementNode> Then { get; set; }

        public MatchCaseBlock ToCaseBlock()
        {
            var value = ValueConverter.Convert(Case as string) ?? Case;
            var instructions = Then?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();
            return new MatchCaseBlock(value, instructions);
        }
    }

    internal class MatchNode : IStatementNode
    {
        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        [YamlMember(Alias = "cases")]
        public List<MatchCaseNode> Cases { get; set; } = new List<MatchCaseNode>();

        [YamlMember(Alias = "default")]
        public List<IStatementNode> Default { get; set; }

        public IStatement ToStatement()
        {
            var cases = Cases.Select(c => c.ToCaseBlock()).ToList();
            var defaultInstructions = Default?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();
            return new MatchStatement(Value.ToExpression(), cases, defaultInstructions);
        }
    }
}