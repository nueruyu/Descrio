using Descrio.Data;
using System;
using System.Collections.Generic;
using System.Text;
using static Descrio.Parsing.Expressions.TokenType;

namespace Descrio.Parsing.Expressions
{
    /// <summary>
    /// A lexical analyzer (or scanner) for the Descrio expression language.
    /// It takes a source string and breaks it down into a sequence of tokens.
    /// </summary>
    public class Tokenizer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _startOfLineOffset = 0;
        private int _tokenStartLine;
        private int _tokenStartColumnOffset;

        // A static dictionary to map keyword strings to their token types.
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            {"true", TRUE},
            {"false", FALSE},
            {"null", NULL}
        };

        public Tokenizer(string source)
        {
            _source = source;
        }

        /// <summary>
        /// Scans the entire source string and returns a list of tokens.
        /// </summary>
        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                _start = _current;
                _tokenStartLine = _line;
                _tokenStartColumnOffset = _startOfLineOffset;

                ScanToken();
            }

            int eofColumn = _current - _startOfLineOffset + 1;
            var eofLocation = new SourceRange(_line, eofColumn, _line, eofColumn);
            _tokens.Add(new Token(EOF, "", null, _source.Length, eofLocation));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single-character tokens
                case '(':
                    AddToken(LEFT_PAREN);
                    break;

                case ')':
                    AddToken(RIGHT_PAREN);
                    break;

                case '.':
                    AddToken(DOT);
                    break;

                case '+':
                    AddToken(PLUS);
                    break;

                case '-':
                    AddToken(MINUS);
                    break;

                case '*':
                    AddToken(STAR);
                    break;

                case '/':
                    AddToken(SLASH);
                    break;

                // One or two character tokens
                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;

                case '=':
                    if (Match('='))
                    {
                        AddToken(EQUAL_EQUAL);
                    }
                    else
                    {
                        // A single '=' is not a valid operator in our expression language.
                        throw new FormatException($"Unexpected character '{c}' at position {_current - 1}. Did you mean '=='?");
                    }
                    break;

                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;

                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    _line++;
                    _startOfLineOffset = _current;
                    break;

                // String literals
                case '\'':
                    StringLiteral('\'');
                    break;

                case '"':
                    StringLiteral('"');
                    break;

                default:
                    if (IsDigit(c))
                    {
                        NumberLiteral();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        throw new FormatException($"Unexpected character '{c}' at position {_current - 1}.");
                    }
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();

            string text = _source.Substring(_start, _current - _start);

            // Check if the identifier is a reserved keyword.
            if (!keywords.TryGetValue(text, out var type))
            {
                type = IDENTIFIER;
            }
            AddToken(type);
        }

        private void NumberLiteral()
        {
            while (IsDigit(Peek()))
                Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();
                while (IsDigit(Peek()))
                    Advance();
            }

            string numberString = _source.Substring(_start, _current - _start);
            AddToken(NUMBER, ValueConverter.Convert(numberString));
        }

        private void StringLiteral(char quoteType)
        {
            var valueBuilder = new StringBuilder();

            while (Peek() != quoteType && !IsAtEnd())
            {
                char c = Advance();

                // Track line breaks inside the string.
                if (c == '\n')
                {
                    _line++;
                    _startOfLineOffset = _current;
                }

                // Handle escape character
                if (c == '\\')
                {
                    if (IsAtEnd())
                        break;

                    char escaped = Advance();
                    switch (escaped)
                    {
                        case '\'':
                            valueBuilder.Append('\'');
                            break;

                        case '"':
                            valueBuilder.Append('"');
                            break;

                        case '\\':
                            valueBuilder.Append('\\');
                            break;

                        case 'n':
                            valueBuilder.Append('\n');
                            // Also track this as a line break for location purposes.
                            _line++;
                            _startOfLineOffset = _current;
                            break;

                        case 't':
                            valueBuilder.Append('\t');
                            break;

                        default:
                            valueBuilder.Append(escaped);
                            break;
                    }
                }
                else
                {
                    valueBuilder.Append(c);
                }
            }

            if (IsAtEnd())
            {
                throw new FormatException($"Unterminated string starting at position {_start}.");
            }

            Advance(); // Consume the closing quote.

            AddToken(STRING, valueBuilder.ToString());
        }

        private bool Match(char expected)
        {
            if (IsAtEnd() || _source[_current] != expected)
                return false;

            _current++;
            return true;
        }

        private char Peek() => IsAtEnd() ? '\0' : _source[_current];

        private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private bool IsAtEnd() => _current >= _source.Length;

        private char Advance() => _source[_current++];

        private void AddToken(TokenType type) => AddToken(type, null);

        private void AddToken(TokenType type, object literal)
        {
            string text = _source.Substring(_start, _current - _start);
            int startColumn = _start - _tokenStartColumnOffset + 1;
            int endColumn = _current - _startOfLineOffset;

            var location = new SourceRange(_tokenStartLine, startColumn, _line, endColumn);
            _tokens.Add(new Token(type, text, literal, _start, location));
        }
    }
}