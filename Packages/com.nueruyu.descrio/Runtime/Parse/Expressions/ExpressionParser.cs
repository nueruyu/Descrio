using System;
using System.Collections.Generic;
using System.Linq;
using static Descrio.Parse.Expressions.TokenType;

namespace Descrio.Parse.Expressions
{
    /// <summary>
    /// A Pratt parser for parsing expressions. It uses a Tokenizer to first
    /// convert the source string into a stream of tokens, and then uses
    /// precedence rules to construct an abstract syntax tree (AST).
    /// </summary>
    public class ExpressionParser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        // Delegate types for Pratt parsing rules.
        private delegate IExpression PrefixParselet();

        private delegate IExpression InfixParselet(IExpression left);

        private readonly Dictionary<TokenType, PrefixParselet> _prefixParselets = new();
        private readonly Dictionary<TokenType, InfixParselet> _infixParselets = new();
        private readonly Dictionary<TokenType, Precedence> _precedences = new();

        /// <summary>
        /// Defines operator precedence levels, from lowest to highest.
        /// This controls the order of operations (e.g., multiplication before addition).
        /// </summary>
        private enum Precedence
        {
            NONE,       // Lowest precedence
            EQUALITY,   // == !=
            COMPARISON, // > < >= <=
            TERM,       // + -
            FACTOR,     // * /
            UNARY,      // ! -
            CALL        // .
        }

        public ExpressionParser(string text)
        {
            var tokenizer = new Tokenizer(text);
            _tokens = tokenizer.ScanTokens();

            // Register Prefix parselets, which handle tokens that appear at the start of an expression.
            Register(IDENTIFIER, () => new VariableExpression(Previous().Lexeme));
            Register(NUMBER, () => new LiteralExpression(Previous().Literal));
            Register(TRUE, () => new LiteralExpression(true));
            Register(FALSE, () => new LiteralExpression(false));
            Register(NULL, () => new LiteralExpression(null));
            Register(STRING, () => ParseInterpolatedString((string)Previous().Literal));

            // Unary operators are also prefix.
            Register(BANG, () => new UnaryExpression(OperatorType.Not, ParsePrecedence(Precedence.UNARY)));
            Register(MINUS, () => new UnaryExpression(OperatorType.Subtract, ParsePrecedence(Precedence.UNARY)));

            // Grouping with parentheses.
            Register(LEFT_PAREN, () =>
            {
                var expr = ParsePrecedence(Precedence.NONE);
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return expr;
            });

            // Register Infix parselets, which handle tokens that appear between two operands.
            RegisterInfix(PLUS, OperatorType.Add, Precedence.TERM);
            RegisterInfix(MINUS, OperatorType.Subtract, Precedence.TERM);
            RegisterInfix(STAR, OperatorType.Multiply, Precedence.FACTOR);
            RegisterInfix(SLASH, OperatorType.Divide, Precedence.FACTOR);
            RegisterInfix(EQUAL_EQUAL, OperatorType.Equal, Precedence.EQUALITY);
            RegisterInfix(BANG_EQUAL, OperatorType.NotEqual, Precedence.EQUALITY);
            RegisterInfix(GREATER, OperatorType.GreaterThan, Precedence.COMPARISON);
            RegisterInfix(GREATER_EQUAL, OperatorType.GreaterThanOrEqual, Precedence.COMPARISON);
            RegisterInfix(LESS, OperatorType.LessThan, Precedence.COMPARISON);
            RegisterInfix(LESS_EQUAL, OperatorType.LessThanOrEqual, Precedence.COMPARISON);

            RegisterInfix(DOT, Precedence.CALL, (left) =>
            {
                var member = Consume(IDENTIFIER, "Expect property name after '.'.");
                return new MemberAccessExpression(left, member.Lexeme);
            });
        }

        /// <summary>
        /// Parses the token stream and returns the root expression of the AST.
        /// </summary>
        public IExpression Parse()
        {
            // Handle empty input gracefully.
            if (Current().Type == EOF)
            {
                return new LiteralExpression(null);
            }

            var expression = ParsePrecedence(Precedence.NONE);

            // After parsing, we must be at the end of the input.
            // If not, it means there were extra, unexpected tokens.
            if (!IsAtEnd())
            {
                throw new FormatException($"Unexpected token '{Current().Lexeme}' found after the expression.");
            }

            return expression;
        }

        /// <summary>
        /// The core of the Pratt parser. It recursively parses expressions
        /// based on operator precedence.
        /// </summary>
        private IExpression ParsePrecedence(Precedence precedence)
        {
            if (IsAtEnd())
            {
                throw new FormatException("Unexpected end of expression.");
            }

            var token = Advance();
            if (!_prefixParselets.TryGetValue(token.Type, out var prefixParselet))
            {
                throw new FormatException($"Expected an expression but found '{token.Lexeme}' at position {token.Position}.");
            }

            var left = prefixParselet();

            // This loop is the magic of Pratt parsing. It continues as long as
            // the next operator has a higher precedence than the current one.
            while (precedence < GetPrecedence(Current().Type))
            {
                token = Advance();
                if (!_infixParselets.TryGetValue(token.Type, out var infixParselet))
                {
                    // This should ideally not be reached if the grammar is well-defined.
                    throw new FormatException($"No infix parselet found for operator '{token.Lexeme}'.");
                }
                left = infixParselet(left);
            }

            return left;
        }

        private void Register(TokenType type, PrefixParselet parselet) => _prefixParselets[type] = parselet;

        private void RegisterInfix(TokenType type, Precedence precedence, InfixParselet parselet)
        {
            _infixParselets[type] = parselet;
            _precedences[type] = precedence;
        }

        private void RegisterInfix(TokenType type, OperatorType opType, Precedence precedence)
        {
            RegisterInfix(type, precedence, (left) => new BinaryExpression(left, ParsePrecedence(precedence), opType));
        }

        private Precedence GetPrecedence(TokenType type)
        {
            return _precedences.GetValueOrDefault(type, Precedence.NONE);
        }

        private IExpression ParseInterpolatedString(string stringValue)
        {
            var parts = new List<IExpression>();
            var lastIndex = 0;
            var currentIndex = 0;

            while ((currentIndex = stringValue.IndexOf("${", currentIndex, StringComparison.Ordinal)) != -1)
            {
                // Add the literal part before the interpolation.
                if (currentIndex > lastIndex)
                {
                    parts.Add(new LiteralExpression(stringValue.Substring(lastIndex, currentIndex - lastIndex)));
                }

                var expressionStart = currentIndex + 2;
                var braceDepth = 1;
                var end = expressionStart;
                while (end < stringValue.Length && braceDepth > 0)
                {
                    char c = stringValue[end];
                    if (c == '{')
                        braceDepth++;
                    else if (c == '}')
                        braceDepth--;
                    end++;
                }

                if (braceDepth != 0)
                    throw new FormatException("Unterminated expression in interpolated string.");

                var innerExpressionString = stringValue.Substring(expressionStart, end - expressionStart - 1);

                // Recursively create a new parser for the inner expression.
                // This correctly handles nested and empty expressions.
                var innerParser = new ExpressionParser(innerExpressionString);
                parts.Add(innerParser.Parse());

                currentIndex = end;
                lastIndex = currentIndex;
            }

            // Add the final literal part after the last interpolation.
            if (lastIndex < stringValue.Length)
            {
                parts.Add(new LiteralExpression(stringValue.Substring(lastIndex)));
            }

            if (parts.Count == 0)
                return new LiteralExpression(stringValue); // No interpolation was found.

            // If the string consists of a single literal part, return it directly.
            if (parts.Count == 1 && parts[0] is LiteralExpression literal)
                return literal;

            return new InterpolatedStringExpression(parts);
        }

        // --- Token Stream Navigation Helpers ---

        private Token Current() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        private Token Advance() => !IsAtEnd() ? _tokens[_current++] : _tokens.Last();

        private bool IsAtEnd() => _current >= _tokens.Count || Current().Type == EOF;

        private Token Consume(TokenType type, string message)
        {
            if (Current().Type == type)
                return Advance();
            throw new FormatException($"{message} (Found {Current().Type} at position {Current().Position})");
        }
    }
}