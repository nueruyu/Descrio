using System.Threading.Tasks;
using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using Descrio.Syntax;

namespace Descrio.Syntax.Expressions
{
    /// <summary>
    /// Represents an operation with a single operand (e.g., !is_ready).
    /// </summary>
    public class UnaryExpression : IExpression
    {
        public OperatorType OperatorType { get; }
        public IExpression Operand { get; }
        public SourceRange Location { get; }

        public UnaryExpression(OperatorType operatorType, IExpression operand, SourceRange location = null)
        {
            OperatorType = operatorType;
            Operand = operand;
            Location = location;
        }
    }
}