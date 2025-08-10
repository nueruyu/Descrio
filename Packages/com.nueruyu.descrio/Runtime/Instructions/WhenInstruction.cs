using Descrio.Execution;
using System;
using System.Threading.Tasks;

namespace Descrio
{
    public class WhenInstruction : IInstruction
    {
        public WhenCaseBlock[] Cases { get; }

        public WhenInstruction(WhenCaseBlock[] cases)
        {
            Cases = cases ?? throw new ArgumentNullException(nameof(cases));
        }

        public ValueTask AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}