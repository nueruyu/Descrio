using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class ReturnInstruction : IInstruction
    {
        public IExpression ValueExpression { get; }

        public ReturnInstruction(IExpression valueExpression)
        {
            ValueExpression = valueExpression; // Can be null
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}