using Descrio.Execution;
using System.Threading.Tasks;
using UnityEngine;

namespace Descrio
{
    public interface IInstruction
    {
        ValueTask AcceptAsync(IAstVisitor visitor);
    }
}