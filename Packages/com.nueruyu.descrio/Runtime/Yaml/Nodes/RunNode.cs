using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.Linq;

namespace Descrio.Yaml.Nodes
{
    internal class RunNode : IInstructionNode, IExpressionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public Dictionary<string, IExpressionNode> Args { get; set; }

        RunInstruction ToRunInstruction() => new RunInstruction(
            Name,
            Args?.ToDictionary(x => x.Key, x => x.Value.ToExpression()) ?? new());

        public IInstruction ToInstruction() => ToRunInstruction();

        public IExpression ToExpression() => ToRunInstruction();
    }
}