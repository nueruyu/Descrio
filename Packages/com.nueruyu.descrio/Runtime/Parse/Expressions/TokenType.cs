namespace Descrio.Parse.Expressions
{
    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN,

        PLUS, MINUS, STAR, SLASH,
        BANG, EQUAL, GREATER, LESS,

        // Two-character tokens.
        BANG_EQUAL,

        EQUAL_EQUAL,
        GREATER_EQUAL,
        LESS_EQUAL,

        // Literals.
        IDENTIFIER,

        STRING,
        NUMBER,

        // Keywords.
        TRUE,

        FALSE,
        NULL,
        // Potentially `and`, `or` in the future.

        EOF
    }
}