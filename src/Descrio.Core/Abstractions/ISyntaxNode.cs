using Descrio.Data;

namespace Descrio.Abstractions
{
    /// <summary>
    /// The root interface for all nodes in the syntax tree.
    /// It guarantees that every syntactical element has a location in the source code.
    /// </summary>
    public interface ISyntaxNode
    {
        /// <summary>
        /// The location of this node in the source code.
        /// </summary>
        SourceRange Location { get; }
    }
}