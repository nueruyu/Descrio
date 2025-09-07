using System.Data.Common;

namespace Descrio.Parse.Expressions
{
    public class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object? Literal;
        public readonly int Position;
        public readonly SourceRange Location;

        public Token(TokenType type, string lexeme, object literal, int position, int line, int startColumn, int endColumn) : this(type, lexeme, literal, position, new SourceRange(line, startColumn, line, endColumn))
        {
        }

        public Token(TokenType type, string lexeme, object literal, int position, SourceRange location)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Position = position;
            Location = location;
        }

        public Token WithOffset(int lineOffset, int columnOffset)
        {
            var newLocation = Location.WithOffset(lineOffset, columnOffset);
            return new Token(Type, Lexeme, Literal, Position, newLocation);
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}