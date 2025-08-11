using Descrio.Yaml.Nodes;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    class ScriptRootNode
    {
        [YamlMember(Alias = "imports")]
        public List<string> Imports { get; set; } = new();

        [YamlMember(Alias = "statements")]
        public List<IInstructionNode> Statements { get; set; } = new();

        public Module ToModule()
        {
            var instructions = Statements?.Select(s => s.ToInstruction()).ToArray() ?? System.Array.Empty<IInstruction>();
            var imports = Imports?.ToArray() ?? System.Array.Empty<string>();
            return new Module(instructions, imports);
        }
    }
}