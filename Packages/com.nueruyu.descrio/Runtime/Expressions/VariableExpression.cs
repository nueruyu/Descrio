using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class VariableExpression : IExpression
    {
        private readonly string _variableName;

        public VariableExpression(string variableName) => _variableName = variableName;

        public string VariableName => _variableName;

        public ValueTask<object> AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}