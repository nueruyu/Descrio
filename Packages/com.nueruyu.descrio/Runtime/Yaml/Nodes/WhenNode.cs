using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhenNode : IInstructionNode
    {
        [YamlMember(Alias = "cases")]
        public List<WhenCaseBlockNode> Cases { get; set; }

        public IInstruction ToInstruction()
        {
            var cases = Cases?.Select(c => c.ToCaseBlock()).ToArray();
            return new WhenInstruction(cases);
        }
    }
}