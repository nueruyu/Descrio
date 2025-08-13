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
        public List<IInstructionNode> Then { get; set; }

        public MatchCaseBlock ToCaseBlock()
        {
            var value = ValueConverter.Convert(Case as string) ?? Case;
            var instructions = Then?.Select(s => s.ToInstruction()).ToArray() ?? System.Array.Empty<IInstruction>();
            return new MatchCaseBlock(value, instructions);
        }
    }

    internal class MatchNode : IInstructionNode
    {
        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        [YamlMember(Alias = "cases")]
        public List<MatchCaseNode> Cases { get; set; } = new List<MatchCaseNode>();

        [YamlMember(Alias = "default")]
        public List<IInstructionNode> Default { get; set; }

        public IInstruction ToInstruction()
        {
            var cases = Cases.Select(c => c.ToCaseBlock()).ToList();
            var defaultInstructions = Default?.Select(s => s.ToInstruction()).ToArray() ?? System.Array.Empty<IInstruction>();
            return new MatchInstruction(Value.ToExpression(), cases, defaultInstructions);
        }
    }
}