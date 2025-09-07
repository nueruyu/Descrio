namespace Descrio
{
    /// <summary>
    /// Represents an expression that is explicitly grouped by parentheses.
    /// This node is primarily used to control precedence and to hold the location
    /// of the parentheses themselves.
    /// </summary>
    public class GroupingExpression : IExpression
    {
        /// <summary>
        /// The expression contained within the parentheses.
        /// </summary>
        public IExpression Expression { get; }

        public SourceRange Location { get; }

        public GroupingExpression(IExpression expression, SourceRange location)
        {
            Expression = expression;
            Location = location;
        }
    }
}