using Descrio.Execution;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Descrio
{
    /// <summary>
    /// Represents a binary operation between two expressions
    /// </summary>
    public class BinaryExpression : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;
        private readonly OperatorType _operatorType;

        public BinaryExpression(IExpression left, IExpression right, OperatorType operatorType)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _operatorType = operatorType;
        }

        public IExpression Left => _left;

        public IExpression Right => _right;

        public OperatorType OperatorType => _operatorType;

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}