using System.Threading.Tasks;

namespace Descrio
{
    public class VariableExpression : IExpression
    {
        readonly string _variableName;

        public VariableExpression(string variableName) => _variableName = variableName;

        public ValueTask<object> EvaluateAsync(ExecutionContext context)
        {
            var result = context.Variables.TryGet(_variableName, out var value) ? value : null;
            return new ValueTask<object>(result);
        }
    }
}