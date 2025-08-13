using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public interface IInstruction
    {
        ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor);
    }
}