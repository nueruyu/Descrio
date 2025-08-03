using System.Threading.Tasks;

namespace Descrio
{
    public interface IExpression
    {
        ValueTask<object> EvaluateAsync(ExecutionContext context);
    }
}