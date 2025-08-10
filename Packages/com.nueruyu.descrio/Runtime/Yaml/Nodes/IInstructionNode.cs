namespace Descrio.Yaml.Nodes
{
    /// <summary>
    /// Common interface for all instruction nodes.
    /// </summary>
    internal interface IInstructionNode
    {
        IInstruction ToInstruction();
    }
}