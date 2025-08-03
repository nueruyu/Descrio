using System.Threading.Tasks;

namespace Descrio
{
    public class LiteralExpression : IExpression
    {
        readonly object _value;

        public LiteralExpression(object value) => _value = value;

        public ValueTask<object> EvaluateAsync(ExecutionContext context) => new ValueTask<object>(_value);
    }
}