using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public interface IInstruction
    {
        ValueTask AcceptAsync(IAstVisitor visitor);
    }
}