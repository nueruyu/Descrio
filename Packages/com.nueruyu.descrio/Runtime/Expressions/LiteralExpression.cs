using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class LiteralExpression : IExpression
    {
        private readonly object _value;

        public LiteralExpression(object value) => _value = value;

        public object Value => _value;

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}