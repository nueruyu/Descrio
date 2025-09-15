namespace Descrio.Parsing.Expressions
{
    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN,

        DOT, // Member access

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

        EOF
    }
}