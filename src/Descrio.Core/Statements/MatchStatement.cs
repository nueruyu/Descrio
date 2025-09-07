using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents a block of instructions for a single case in a !match statement.
    /// </summary>
    public class MatchCaseBlock : ISyntaxNode
    {
        public object CaseValue { get; }
        public IStatement[] ThenBlock { get; }
        public SourceRange Location { get; }

        public MatchCaseBlock(object caseValue, IStatement[] thenBlock, SourceRange location = null)
        {
            CaseValue = caseValue;
            ThenBlock = thenBlock;
            Location = location;
        }
    }

    /// <summary>
    /// Represents a !match instruction that executes a block of code based on value equality.
    /// </summary>
    public class MatchStatement : IStatement
    {
        public IExpression ValueExpression { get; }
        public IReadOnlyList<MatchCaseBlock> Cases { get; }
        public IStatement[] DefaultBlock { get; }
        public SourceRange Location { get; }

        public MatchStatement(IExpression valueExpression, IReadOnlyList<MatchCaseBlock> cases, IStatement[] defaultBlock, SourceRange location = null)
        {
            ValueExpression = valueExpression;
            Cases = cases;
            DefaultBlock = defaultBlock;
            Location = location;
        }
    }
}