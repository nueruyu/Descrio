using System;
using System.Collections.Generic;
using System.Globalization;

namespace Descrio.Parse.Expressions
{
    public class ExpressionParser
    {
        private readonly string _text;
        private int _position;

        public ExpressionParser(string text)
        {
            _text = text;
            _position = 0;
        }

        public IExpression Parse()
        {
            var left = ParsePrimary();

            while (true)
            {
                SkipWhitespace();
                if (_position >= _text.Length)
                    break;

                OperatorType? op = null;
                if (Peek(0, 2) == "==")
                {
                    op = OperatorType.Equal;
                    _position += 2;
                }
                else if (Peek(0, 2) == "!=")
                {
                    op = OperatorType.NotEqual;
                    _position += 2;
                }
                // TODO: Add other operators like >, <, >=, <= here if needed

                if (op == null)
                    break;

                var right = ParsePrimary();
                left = new BinaryExpression(left, right, op.Value);
            }

            return left;
        }

        private IExpression ParsePrimary()
        {
            SkipWhitespace();
            if (_position >= _text.Length)
                return new LiteralExpression(null);

            if (_text[_position] == '\'' || _text[_position] == '"')
            {
                return ParseInterpolatedString();
            }

            var start = _position;
            while (_position < _text.Length &&
                !char.IsWhiteSpace(_text[_position]) &&
                "!=()".IndexOf(_text[_position]) == -1)
            {
                _position++;
            }

            var token = _text.Substring(start, _position - start);

            // Try to convert the token to a literal value (bool, long, double).
            // If it remains a string, treat it as a variable.
            var literalValue = ValueConverter.Convert(token);
            if (literalValue is string stringValue && stringValue == token)
            {
                return new VariableExpression(token);
            }
            else
            {
                return new LiteralExpression(literalValue);
            }
        }

        private IExpression ParseInterpolatedString()
        {
            var quote = _text[_position];
            _position++; // Skip the opening quote.

            var parts = new List<IExpression>();
            var lastIndex = _position;

            while (_position < _text.Length && _text[_position] != quote)
            {
                // Look for the '${' pattern.
                if (Peek(0, 2) == "${")
                {
                    // Add the preceding literal part, if any.
                    if (_position > lastIndex)
                    {
                        parts.Add(new LiteralExpression(_text.Substring(lastIndex, _position - lastIndex)));
                    }

                    _position += 2; // Skip the '${'.
                    var expressionStart = _position;

                    // Find the matching '}' brace, respecting nested braces.
                    var braceDepth = 1;
                    while (_position < _text.Length && braceDepth > 0)
                    {
                        char currentChar = _text[_position];
                        if (currentChar == '{')
                        {
                            braceDepth++;
                        }
                        else if (currentChar == '}')
                        {
                            braceDepth--;
                        }
                        _position++;
                    }

                    // --- Implementation for the TODO part ---
                    if (braceDepth != 0)
                    {
                        // If braceDepth is not zero, it means we reached the end of the string
                        // without finding a matching closing brace.
                        throw new FormatException($"Unterminated expression in interpolated string starting at position {expressionStart - 2}. Missing '}}'.");
                    }

                    // Extract the content inside ${...}.
                    var expressionString = _text.Substring(expressionStart, _position - expressionStart - 1);

                    // Recursively parse the content as a new expression.
                    var innerExpressionParser = new ExpressionParser(expressionString);
                    parts.Add(innerExpressionParser.Parse());

                    lastIndex = _position;
                }
                else
                {
                    _position++;
                }
            }

            // Add the final literal part after the last expression.
            if (_position > lastIndex)
            {
                parts.Add(new LiteralExpression(_text.Substring(lastIndex, _position - lastIndex)));
            }

            _position++; // Skip the closing quote.

            // If there's no interpolation, return a simple LiteralExpression.
            // Otherwise, return an InterpolatedStringExpression.
            if (parts.Count == 0)
                return new LiteralExpression(string.Empty);
            if (parts.Count == 1 && parts[0] is LiteralExpression literal)
                return literal;

            return new InterpolatedStringExpression(parts);
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
                _position++;
        }

        private string Peek(int startOffset, int count) =>
            (_position + startOffset + count <= _text.Length) ?
            _text.Substring(_position + startOffset, count) :
            "";
    }
}