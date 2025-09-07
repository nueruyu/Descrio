using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    public class InterpolatedStringExpression : IExpression
    {
        public IReadOnlyList<IExpression> Parts { get; }
        public SourceRange Location { get; }

        public InterpolatedStringExpression(IReadOnlyList<IExpression> parts, SourceRange location = null)
        {
            Parts = parts;
            Location = location;
        }
    }
}