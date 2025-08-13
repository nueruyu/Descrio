using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class VarNode : IInstructionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; }

        public IInstruction ToInstruction() => new VarInstruction(Name, Value.ToExpression());
    }
}