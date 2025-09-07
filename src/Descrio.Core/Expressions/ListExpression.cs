using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    public class ListExpression : IExpression
    {
        public IReadOnlyList<IExpression> Elements { get; }
        public SourceRange Location { get; }

        public ListExpression(IReadOnlyList<IExpression> elements, SourceRange location = null)
        {
            Elements = elements ?? new List<IExpression>();
            Location = location;
        }
    }
}