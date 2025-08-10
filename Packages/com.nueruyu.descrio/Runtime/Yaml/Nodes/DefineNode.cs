using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class DefineNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "params")]
        public ParameterDefinition[] Parameters { get; set; } = Array.Empty<ParameterDefinition>();

        [YamlMember(Alias = "statements")]
        public IInstructionNode[] Statements { get; set; } = Array.Empty<IInstructionNode>();

        public IInstruction ToInstruction()
        {
            var instructions = Statements.Select(s => s.ToInstruction()).ToArray();
            return new DefineInstruction(Name, Parameters, instructions);
        }
    }
}