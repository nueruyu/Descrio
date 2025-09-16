using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;

namespace Descrio.Syntax.Statements
{
    /// <summary>
    /// Represents a single 'catch' block within a 'try-catch' statement.
    /// </summary>
    public class CatchClause : ISyntaxNode
    {
        public string ErrorName { get; }
        public string VariableName { get; }
        public IStatement[] ThenBlock { get; }
        public SourceRange Location { get; }

        public CatchClause(string errorName, string variableName, IStatement[] thenBlock, SourceRange location = null)
        {
            ErrorName = errorName; // Can be null for a default catch-all
            VariableName = variableName;
            ThenBlock = thenBlock;
            Location = location;
        }
    }

    /// <summary>
    /// Represents a try-catch-finally block for exception handling.
    /// </summary>
    public class TryCatchStatement : IStatement
    {
        public IStatement[] TryBlock { get; }
        public IReadOnlyList<CatchClause> CatchClauses { get; }
        public IStatement[] FinallyBlock { get; }
        public SourceRange Location { get; }

        public TryCatchStatement(IStatement[] tryBlock, IReadOnlyList<CatchClause> catchClauses, IStatement[] finallyBlock, SourceRange location = null)
        {
            TryBlock = tryBlock;
            CatchClauses = catchClauses;
            FinallyBlock = finallyBlock;
            Location = location;
        }
    }
}