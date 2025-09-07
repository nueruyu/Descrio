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

        public BinaryExpression(IExpression left, IExpression right, OperatorType operatorType, SourceRange location = null)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _operatorType = operatorType;
            Location = location;
        }

        public IExpression Left => _left;

        public IExpression Right => _right;

        public OperatorType OperatorType => _operatorType;
        public SourceRange Location { get; }
    }
}