namespace Descrio.Syntax
{
    /// <summary>
    /// Defines the types of operators for binary expressions.
    /// </summary>
    public enum OperatorType
    {
        // Comparison

        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,

        // Arithmetic

        Add,
        Subtract,
        Multiply,
        Divide,

        // Unary

        Not,
    }
}