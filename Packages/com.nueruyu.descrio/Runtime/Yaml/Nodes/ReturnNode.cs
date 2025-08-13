using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ReturnNode : IInstructionNode
    {
        [YamlMember(Alias = "value")]
        public IExpressionNode Value { get; set; } // Can be null

        public IInstruction ToInstruction() => new ReturnInstruction(Value?.ToExpression());
    }
}