using Descrio.Execution;
using System;
using System.Threading.Tasks;

namespace Descrio
{
    public class WhenCaseBlock : ISyntaxNode
    {
        public WhenCaseBlock(IExpression condition, IStatement[] thenBlock, SourceRange location = null)
        {
            Condition = condition;
            ThenBlock = thenBlock ?? throw new System.ArgumentNullException(nameof(thenBlock));
            Location = location;
        }

        public IExpression Condition { get; }
        public IStatement[] ThenBlock { get; }
        public SourceRange Location { get; }
    }

    public class WhenStatement : IStatement
    {
        public WhenCaseBlock[] Cases { get; }
        public SourceRange Location { get; }

        public WhenStatement(WhenCaseBlock[] cases, SourceRange location = null)
        {
            Cases = cases ?? throw new ArgumentNullException(nameof(cases));
            Location = location;
        }
    }
}