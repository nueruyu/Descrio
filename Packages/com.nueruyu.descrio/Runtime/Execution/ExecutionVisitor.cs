using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Execution
{
    public class ExecutionVisitor : IAstVisitor
    {
        private readonly ExecutionContext _context;

        public ExecutionVisitor(ExecutionContext context)
        {
            _context = context;
        }

        public async ValueTask VisitAsync(SetInstruction instruction)
        {
            var value = await instruction.ValueExpression.AcceptAsync(this);
            _context.Variables.Set(instruction.Name, value);
        }

        public async ValueTask VisitAsync(CallInstruction instruction)
        {
            if (!_context.Callables.TryGet(instruction.Name, out var callable))
                throw new InvalidOperationException($"Callable '{instruction.Name}' not found.");

            var args = new object[instruction.ArgExpressions.Length];
            for (var i = 0; i < instruction.ArgExpressions.Length; i++)
            {
                args[i] = await instruction.ArgExpressions[i].AcceptAsync(this);
            }

            var result = await callable.CallAsync(args, _context);

            if (!string.IsNullOrEmpty(instruction.ReturnVariable))
            {
                _context.Variables.Set(instruction.ReturnVariable, result);
            }
        }

        public ValueTask VisitAsync(DefineInstruction instruction)
        {
            async ValueTask<object> CallAsync(object[] args, ExecutionContext context)
            {
                var localContext = context.CreateChildContext();

                for (var i = 0; i < instruction.Parameters.Length; i++)
                {
                    var param = instruction.Parameters[i];
                    var value = (i < args.Length) ? args[i] : param.DefaultValue;
                    localContext.Variables.Set(param.Name, value);
                }

                var visitor = new ExecutionVisitor(localContext);
                foreach (var instruction in instruction.Statements)
                {
                    await instruction.AcceptAsync(visitor);
                }

                // TODO: return命令が実行された場合、その値を返す
                return null;
            }

            var callable = new DelegateCallable(CallAsync);
            _context.Callables.Register(instruction.Name, callable);
            return default;
        }

        public async ValueTask VisitAsync(WhenInstruction instruction)
        {
            foreach (var caseBlock in instruction.Cases)
            {
                bool conditionMet = false;
                if (caseBlock.Condition == null)
                {
                    conditionMet = true;
                }
                else
                {
                    var result = await caseBlock.Condition.AcceptAsync(this);
                    conditionMet = IsTruthy(result);
                }

                if (conditionMet)
                {
                    foreach (var inst in caseBlock.ThenBlock)
                    {
                        await inst.AcceptAsync(this);
                    }
                    return;
                }
            }
        }

        public ValueTask<object> VisitAsync(LiteralExpression expression)
        {
            return new ValueTask<object>(expression.Value);
        }

        public ValueTask<object> VisitAsync(VariableExpression expression)
        {
            var result = _context.Variables.TryGet(expression.VariableName, out var value) ? value : null;
            return new ValueTask<object>(result);
        }

        public async ValueTask<object> VisitAsync(BinaryExpression expression)
        {
            var left = await expression.Left.AcceptAsync(this);
            var right = await expression.Right.AcceptAsync(this);

            if (TryPromoteToCommonNumeric(left, right, out var numLeft, out var numRight))
            {
                switch (expression.OperatorType)
                {
                    case OperatorType.Equal:
                        return numLeft == numRight;

                    case OperatorType.NotEqual:
                        return numLeft != numRight;

                    case OperatorType.GreaterThan:
                        return numLeft > numRight;

                    case OperatorType.LessThan:
                        return numLeft < numRight;

                    case OperatorType.GreaterThanOrEqual:
                        return numLeft >= numRight;

                    case OperatorType.LessThanOrEqual:
                        return numLeft <= numRight;
                }
            }

            switch (expression.OperatorType)
            {
                case OperatorType.Equal:
                    return Equals(left, right);

                case OperatorType.NotEqual:
                    return !Equals(left, right);

                default:
                    throw new InvalidOperationException(
                        $"Operator '{expression.OperatorType}' cannot be applied to operands of type '{left?.GetType().Name ?? "null"}' and '{right?.GetType().Name ?? "null"}'.");
            }
        }

        private static bool IsTruthy(object value)
        {
            if (value == null)
                return false;
            if (value is bool b)
                return b;
            if (value is string s)
            {
                if (bool.TryParse(s, out var boolValue))
                    return boolValue;
                return !string.IsNullOrEmpty(s);
            }
            if (value is IConvertible convertible)
            {
                try
                {
                    return Convert.ToDouble(convertible, CultureInfo.InvariantCulture) != 0.0;
                }
                catch (FormatException) { /* Fall through */ }
                catch (InvalidCastException) { /* Fall through */ }
                catch (OverflowException) { /* Fall through */ }
            }
            return true;
        }

        private static bool TryPromoteToCommonNumeric(object left, object right, out decimal leftDecimal, out decimal rightDecimal)
        {
            leftDecimal = 0;
            rightDecimal = 0;

            if (!IsNumeric(left) || !IsNumeric(right))
                return false;

            try
            {
                leftDecimal = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                rightDecimal = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsNumeric(object value)
        {
            return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
        }
    }
}