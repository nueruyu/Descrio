using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class WhenCaseBlock : ISyntaxNode
    {
        public WhenCaseBlock(IExpression condition, IStatement[] thenBlock, SourceRange location = null)
        {
            Condition = condition;
            ThenBlock = thenBlock ?? throw new ArgumentNullException(nameof(thenBlock));
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