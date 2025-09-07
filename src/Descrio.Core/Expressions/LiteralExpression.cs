using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class LiteralExpression : IExpression
    {
        private readonly object _value;

        public LiteralExpression(object value, SourceRange location = null)
        {
            _value = value;
            Location = location;
        }

        public object Value => _value;
        public SourceRange Location { get; }
    }
}