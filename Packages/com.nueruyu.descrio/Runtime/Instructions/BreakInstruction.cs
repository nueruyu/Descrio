using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class BreakInstruction : IInstruction
    {
        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}