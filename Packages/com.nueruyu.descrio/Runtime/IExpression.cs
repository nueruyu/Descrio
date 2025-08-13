using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public interface IExpression
    {
        ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor);
    }
}