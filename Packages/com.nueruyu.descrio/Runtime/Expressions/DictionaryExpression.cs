using System.Collections.Generic;

namespace Descrio
{
    public class DictionaryExpression : IExpression
    {
        public IReadOnlyDictionary<IExpression, IExpression> Entries { get; }

        public DictionaryExpression(IReadOnlyDictionary<IExpression, IExpression> entries)
        {
            Entries = entries ?? new Dictionary<IExpression, IExpression>();
        }
    }
}