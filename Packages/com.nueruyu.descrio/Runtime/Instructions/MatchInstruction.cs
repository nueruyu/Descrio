using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents a block of instructions for a single case in a !match statement.
    /// </summary>
    public class MatchCaseBlock
    {
        public object CaseValue { get; }
        public IInstruction[] ThenBlock { get; }

        public MatchCaseBlock(object caseValue, IInstruction[] thenBlock)
        {
            CaseValue = caseValue;
            ThenBlock = thenBlock;
        }
    }

    /// <summary>
    /// Represents a !match instruction that executes a block of code based on value equality.
    /// </summary>
    public class MatchInstruction : IInstruction
    {
        public IExpression ValueExpression { get; }
        public IReadOnlyList<MatchCaseBlock> Cases { get; }
        public IInstruction[] DefaultBlock { get; }

        public MatchInstruction(IExpression valueExpression, IReadOnlyList<MatchCaseBlock> cases, IInstruction[] defaultBlock)
        {
            ValueExpression = valueExpression;
            Cases = cases;
            DefaultBlock = defaultBlock;
        }

        public ValueTask<VisitResult> AcceptAsync<VisitResult>(IAstVisitor<VisitResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}