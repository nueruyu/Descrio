using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    public class DictionaryExpression : IExpression
    {
        public IReadOnlyDictionary<string, IExpression> Entries { get; }

        public DictionaryExpression(IReadOnlyDictionary<string, IExpression> entries)
        {
            Entries = entries ?? new Dictionary<string, IExpression>();
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}