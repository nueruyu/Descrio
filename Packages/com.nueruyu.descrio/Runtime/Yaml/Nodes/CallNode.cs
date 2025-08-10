using System;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class CallNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public IExpression[] Args { get; set; } = Array.Empty<IExpression>();

        [YamlMember(Alias = "return")]
        public string Return { get; set; }

        public IInstruction ToInstruction() => new CallInstruction(Name, Args, Return);
    }
}