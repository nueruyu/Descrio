namespace Descrio
{
    /// <summary>
    /// Represents an expression that was embedded as a string within the source syntax (e.g., using a YAML !expr tag).
    /// This node acts as a wrapper, preserving the source location of the entire construct (e.g., the tag and the string),
    /// while the actual expression logic is contained within the <see cref="InnerExpression"/>.
    /// </summary>
    public class EmbeddedExpression : IExpression
    {
        /// <summary>
        /// The parsed expression from the embedded string.
        /// </summary>
        public IExpression InnerExpression { get; }

        /// <summary>
        /// The source location of the entire embedded construct, including any syntax markers like tags or quotes.
        /// </summary>
        public SourceRange Location { get; }

        public EmbeddedExpression(IExpression innerExpression, SourceRange location)
        {
            InnerExpression = innerExpression;
            Location = location;
        }
    }
}