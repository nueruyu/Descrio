using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ForNode : IInstructionNode
    {
        [YamlMember(Alias = "in")]
        public IExpressionNode In { get; set; }

        [YamlMember(Alias = "as")]
        public string As { get; set; }

        [YamlMember(Alias = "statements")]
        public IInstructionNode[] Statements { get; set; } = System.Array.Empty<IInstructionNode>();

        public IInstruction ToInstruction()
        {
            var instructions = Statements.Select(s => s.ToInstruction()).ToArray();
            return new ForInstruction(In.ToExpression(), As, instructions);
        }
    }
}