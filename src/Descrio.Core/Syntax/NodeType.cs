namespace Descrio.Syntax
{
    /// <summary>
    /// Defines the semantic type of a syntax node, independent of the source format (YAML tag, JSON key, etc.).
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// The node type is not specified (e.g., a literal value without a tag).
        /// </summary>
        None,

        Let,
        Var,
        Assign,
        When,
        For,
        While,
        Match,
        Function,
        Return,
        Break,
        Continue,
        Try,
        Throw,
        Assert,
        Run,
        Dispatch,
        Expression,
        Lambda,
    }
}