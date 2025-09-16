using Descrio.Abstractions;
using Descrio.Data;
using System.Collections.Generic;

namespace Descrio.Syntax.Expressions
{
    public class DictionaryExpression : IExpression
    {
        public IReadOnlyDictionary<IExpression, IExpression> Entries { get; }
        public SourceRange Location { get; }

        public DictionaryExpression(IReadOnlyDictionary<IExpression, IExpression> entries, SourceRange location = null)
        {
            Entries = entries ?? new Dictionary<IExpression, IExpression>();
            Location = location;
        }
    }
}