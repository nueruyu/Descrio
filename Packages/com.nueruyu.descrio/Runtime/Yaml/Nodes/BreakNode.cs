namespace Descrio.Yaml.Nodes
{
    internal class BreakNode : IInstructionNode
    {
        public IInstruction ToInstruction() => new BreakInstruction();
    }
}