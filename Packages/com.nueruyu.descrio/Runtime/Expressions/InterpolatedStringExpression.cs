using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    public class InterpolatedStringExpression : IExpression
    {
        public IReadOnlyList<IExpression> Parts { get; }

        public InterpolatedStringExpression(IReadOnlyList<IExpression> parts)
        {
            Parts = parts;
        }

        public ValueTask<object> AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}