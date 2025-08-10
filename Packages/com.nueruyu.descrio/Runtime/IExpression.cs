using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public interface IExpression
    {
        ValueTask<object> AcceptAsync(IAstVisitor visitor);
    }
}