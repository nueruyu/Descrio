using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhenCaseBlockNode
    {
        [YamlMember(Alias = "condition")]
        public IExpression Condition { get; set; }

        [YamlMember(Alias = "then")]
        public List<IInstructionNode> ThenBlock { get; set; }

        public WhenCaseBlock ToCaseBlock()
        {
            var instructions = ThenBlock?.Select(s => s.ToInstruction()).ToArray();
            return new WhenCaseBlock(Condition, instructions);
        }
    }
}