using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Syntax;
using System.Collections.Generic;

namespace Descrio.Parsing
{
    /// <summary>
    /// Represents a syntax error encountered during parsing.
    /// </summary>
    public record SyntaxError(string Message, SourceRange Location);

    /// <summary>
    /// A container for the final result of a parsing operation for a full Module.
    /// </summary>
    public record ParseResult(Module? Module, IReadOnlyList<SyntaxError> Errors);

    /// <summary>
    /// A container for the final result of parsing a single Statement.
    /// </summary>
    public record StatementParseResult(IStatement? Statement, IReadOnlyList<SyntaxError> Errors);

    /// <summary>
    /// A container for the final result of parsing a single Expression.
    /// </summary>
    public record ExpressionParseResult(IExpression? Expression, IReadOnlyList<SyntaxError> Errors);

    /// <summary>
    /// Defines an interface for a parser that builds an Abstract Syntax Tree (AST) from a script text.
    /// </summary>
    public interface IScriptParser
    {
        /// <summary>
        /// Parses the given text and attempts to build a Module AST.
        /// </summary>
        ParseResult Parse(string text);

        /// <summary>
        /// Parses the given text as a single statement.
        /// </summary>
        StatementParseResult ParseStatement(string text);

        /// <summary>
        /// Parses the given text as a single expression.
        /// </summary>
        ExpressionParseResult ParseExpression(string text);
    }
}