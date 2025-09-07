using System;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.Parse.Yaml
{
    /// <summary>
    /// A parser for the Descrio language that uses YAML as its concrete syntax.
    /// It uses a two-stage process: YAML text -> CST -> AST.
    /// </summary>
    public class YamlScriptParser : IScriptParser
    {
        private T? TryConvert<T>(string text, Func<Cst.CstNode, T> converter, List<SyntaxError> errors) where T : class, IAstNode
        {
            var cstBuilder = new YamlCstBuilder();
            var cstRoot = cstBuilder.Build(text);
            errors.AddRange(cstRoot.Errors);

            if (cstRoot.RootNode == null)
            {
                if (!errors.Any())
                {
                    // Only add this error if no other parsing error occurred.
                    errors.Add(new SyntaxError("Input text is empty or does not contain a valid YAML document.", SourceRange.Unknown));
                }
                return null;
            }

            try
            {
                return converter(cstRoot.RootNode);
            }
            catch (AstConversionException ex)
            {
                errors.Add(new SyntaxError(ex.Message, ex.Location));
            }
            catch (Exception ex)
            {
                // Catch unexpected errors during conversion (e.g., from ExpressionParser)
                errors.Add(new SyntaxError($"An unexpected error occurred during AST conversion: {ex.Message}", cstRoot.RootNode.Location));
            }

            return null;
        }

        public ParseResult Parse(string text)
        {
            var errors = new List<SyntaxError>();
            var module = TryConvert(text, AstConverter.FromCstToModule, errors);
            return new ParseResult(module, errors);
        }

        public StatementParseResult ParseStatement(string text)
        {
            var errors = new List<SyntaxError>();
            var statement = TryConvert(text, AstConverter.FromCstToStatement, errors);
            return new StatementParseResult(statement, errors);
        }

        public ExpressionParseResult ParseExpression(string text)
        {
            var errors = new List<SyntaxError>();
            var expression = TryConvert(text, AstConverter.FromCstToExpression, errors);
            return new ExpressionParseResult(expression, errors);
        }
    }
}