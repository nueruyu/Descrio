using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhileNode : IInstructionNode
    {
        [YamlMember(Alias = "condition")]
        public IExpressionNode Condition { get; set; }

        [YamlMember(Alias = "statements")]
        public IInstructionNode[] Statements { get; set; } = System.Array.Empty<IInstructionNode>();

        public IInstruction ToInstruction()
        {
            var instructions = Statements.Select(s => s.ToInstruction()).ToArray();
            return new WhileInstruction(Condition.ToExpression(), instructions);
        }
    }
}