using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class FunctionNode : IStatementNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "parameters")]
        public ParameterDefinitionNode[] Parameters { get; set; } = Array.Empty<ParameterDefinitionNode>();

        [YamlMember(Alias = "statements")]
        public IStatementNode[] Statements { get; set; } = Array.Empty<IStatementNode>();

        public IStatement ToStatement()
        {
            var instructions = Statements.Select(s => s.ToStatement()).ToArray();
            var parameters = Parameters.Select(p => new ParameterDefinition(
                p.Name,
                p.Type,
                p.DefaultValue)).ToArray();
            return new FunctionStatement(Name, parameters, instructions);
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