using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents a single 'catch' block within a 'try-catch' statement.
    /// </summary>
    public class CatchClause
    {
        public string ErrorName { get; }
        public string VariableName { get; }
        public IStatement[] ThenBlock { get; }

        public CatchClause(string errorName, string variableName, IStatement[] thenBlock)
        {
            ErrorName = errorName; // Can be null for a default catch-all
            VariableName = variableName;
            ThenBlock = thenBlock;
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

        public TryCatchStatement(IStatement[] tryBlock, IReadOnlyList<CatchClause> catchClauses, IStatement[] finallyBlock)
        {
            TryBlock = tryBlock;
            CatchClauses = catchClauses;
            FinallyBlock = finallyBlock;
        }
    }
}