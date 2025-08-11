using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class LetNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        // [YamlMember(Alias = "type")]
        // public string Type { get; set; }

        public IInstruction ToInstruction() => new LetInstruction(Name, Value.ToExpression());
    }
}