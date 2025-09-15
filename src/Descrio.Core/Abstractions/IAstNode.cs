namespace Descrio.Abstractions
{
    /// <summary>
    /// Represents an executable node in the Abstract Syntax Tree (AST).
    /// This serves as a common base for Statements and Expressions.
    /// </summary>
    public interface IAstNode : ISyntaxNode
    {
        // This interface is currently a marker and adds no new members,
        // but it distinguishes executable nodes from other syntax elements.
    }
}