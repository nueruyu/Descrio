using Descrio.Syntax;
using System.Collections.Generic;

namespace Descrio.Parse.Cst
{
    /// <summary>
    /// A generic container for the result of a CST parsing operation.
    /// </summary>
    public record CstRoot(CstNode? RootNode, List<SyntaxError> Errors);

    /// <summary>
    /// The abstract base record for all nodes in the language-agnostic Concrete Syntax Tree (CST).
    /// </summary>
    /// <param name="NodeType">The semantic type of the node, derived from the source format.</param>
    /// <param name="TypeHint">The raw string from the source that indicated the type (e.g., "!let"). Used for user-facing error messages.</param>
    /// <param name="Location">The location of the node in the source code.</param>
    public abstract record CstNode(NodeType NodeType, string? TypeHint, SourceRange Location);

    /// <summary>
    /// A CST node representing a mapping of key-value pairs.
    /// </summary>
    public record MappingCstNode(Dictionary<ScalarCstNode, CstNode> Children, NodeType NodeType, string? TypeHint, SourceRange Location) : CstNode(NodeType, TypeHint, Location);

    /// <summary>
    /// A CST node representing a sequence of values.
    /// </summary>
    public record SequenceCstNode(List<CstNode> Children, NodeType NodeType, string? TypeHint, SourceRange Location) : CstNode(NodeType, TypeHint, Location);

    /// <summary>
    /// A CST node representing a scalar value.
    /// </summary>
    public record ScalarCstNode(string Value, NodeType NodeType, string? TypeHint, SourceRange Location) : CstNode(NodeType, TypeHint, Location);
}