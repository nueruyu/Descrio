using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class SetNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpression Value { get; set; }

        public IInstruction ToInstruction() => new SetInstruction(Name, Value);
    }
}