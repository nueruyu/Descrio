using Descrio.Execution;
using System;
using System.Threading.Tasks;

namespace Descrio
{
    public class WhenCaseBlock
    {
        public WhenCaseBlock(IExpression condition, IStatement[] thenBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock ?? throw new System.ArgumentNullException(nameof(thenBlock));
        }

        public IExpression Condition { get; }
        public IStatement[] ThenBlock { get; }
    }

    public class WhenStatement : IStatement
    {
        public WhenCaseBlock[] Cases { get; }

        public WhenStatement(WhenCaseBlock[] cases)
        {
            Cases = cases ?? throw new ArgumentNullException(nameof(cases));
        }
    }
}