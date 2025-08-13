using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class WhileInstruction : IInstruction
    {
        public IExpression Condition { get; }
        public IInstruction[] Statements { get; }

        public WhileInstruction(IExpression condition, IInstruction[] statements)
        {
            Condition = condition;
            Statements = statements;
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}