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
        public IInstruction[] ThenBlock { get; }

        public CatchClause(string errorName, string variableName, IInstruction[] thenBlock)
        {
            ErrorName = errorName; // Can be null for a default catch-all
            VariableName = variableName;
            ThenBlock = thenBlock;
        }
    }

    /// <summary>
    /// Represents a try-catch-finally block for exception handling.
    /// </summary>
    public class TryCatchInstruction : IInstruction
    {
        public IInstruction[] TryBlock { get; }
        public IReadOnlyList<CatchClause> CatchClauses { get; }
        public IInstruction[] FinallyBlock { get; }

        public TryCatchInstruction(IInstruction[] tryBlock, IReadOnlyList<CatchClause> catchClauses, IInstruction[] finallyBlock)
        {
            TryBlock = tryBlock;
            CatchClauses = catchClauses;
            FinallyBlock = finallyBlock;
        }

        public ValueTask<VisitResult> AcceptAsync<VisitResult>(IAstVisitor<VisitResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}