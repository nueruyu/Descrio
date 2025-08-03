using System.Threading.Tasks;
using UnityEngine;

namespace Descrio
{
    public interface IInstruction
    {
        ValueTask ExecuteAsync(ExecutionContext context);
    }
}