using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents an operation with a single operand (e.g., !is_ready).
    /// </summary>
    public class UnaryExpression : IExpression
    {
        public OperatorType OperatorType { get; }
        public IExpression Operand { get; }

        public UnaryExpression(OperatorType operatorType, IExpression operand)
        {
            OperatorType = operatorType;
            Operand = operand;
        }
    }
}