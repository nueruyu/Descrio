using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class AssignNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        public IInstruction ToInstruction() => new AssignInstruction(Name, Value.ToExpression());
    }
}