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
                return ParseQuotedString();
            }

            var start = _position;
            while (_position < _text.Length &&
                !char.IsWhiteSpace(_text[_position]) &&
                "!=()".IndexOf(_text[_position]) == -1)
            {
                _position++;
            }

            var token = _text.Substring(start, _position - start);

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

        private IExpression ParseQuotedString()
        {
            var quote = _text[_position];
            _position++;
            var start = _position;
            while (_position < _text.Length && _text[_position] != quote)
            {
                _position++;
            }
            var value = _text.Substring(start, _position - start);
            _position++;
            return new LiteralExpression(value);
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