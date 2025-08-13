using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class ContinueInstruction : IInstruction
    {
        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}