using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class AssertNode : IInstructionNode
    {
        [YamlMember(Alias = "condition")]
        public IExpressionNode Condition { get; set; }

        [YamlMember(Alias = "message")]
        public string Message { get; set; }

        public IInstruction ToInstruction() => new AssertInstruction(Condition.ToExpression(), Message);
    }
}