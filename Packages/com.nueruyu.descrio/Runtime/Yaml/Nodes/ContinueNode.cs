namespace Descrio.Yaml.Nodes
{
    internal class ContinueNode : IInstructionNode
    {
        public IInstruction ToInstruction() => new ContinueInstruction();
    }
}