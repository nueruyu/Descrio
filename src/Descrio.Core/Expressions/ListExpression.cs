using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    public class ListExpression : IExpression
    {
        public IReadOnlyList<IExpression> Elements { get; }

        public ListExpression(IReadOnlyList<IExpression> elements)
        {
            Elements = elements ?? new List<IExpression>();
        }
    }
}