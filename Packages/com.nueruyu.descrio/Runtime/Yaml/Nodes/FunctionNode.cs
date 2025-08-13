using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class FunctionNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "parameters")]
        public ParameterDefinitionNode[] Parameters { get; set; } = Array.Empty<ParameterDefinitionNode>();

        [YamlMember(Alias = "statements")]
        public IInstructionNode[] Statements { get; set; } = Array.Empty<IInstructionNode>();

        public IInstruction ToInstruction()
        {
            var instructions = Statements.Select(s => s.ToInstruction()).ToArray();
            var parameters = Parameters.Select(p => new ParameterDefinition(
                p.Name,
                p.Type,
                p.DefaultValue)).ToArray();
            return new FunctionInstruction(Name, parameters, instructions);
        }

        public class ParameterDefinitionNode
        {
            [YamlMember(Alias = "name")]
            public string Name { get; set; }

            [YamlMember(Alias = "type")]
            public string Type { get; set; }

            [YamlMember(Alias = "default")]
            public object DefaultValue { get; set; }
        }
    }
}