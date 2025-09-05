namespace Descrio.Parse.Expressions
{
    public class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object Literal;
        public readonly int Position;

        public Token(TokenType type, string lexeme, object literal, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Position = position;
        }
    }
}