using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;

namespace Descrio.Syntax.Expressions
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