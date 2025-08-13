using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Execution
{
    public class ExecutionVisitor : IAstVisitor<VisitResult>
    {
        private readonly ExecutionContext _context;

        public ExecutionVisitor(ExecutionContext context)
        {
            _context = context;
        }

        public async ValueTask<VisitResult> VisitAsync(LetInstruction instruction)
        {
            var valueResult = await instruction.ValueExpression.AcceptAsync(this);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;

            _context.Variables.Define(instruction.Name, valueResult.Value, isMutable: false);
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(VarInstruction instruction)
        {
            var valueResult = await instruction.ValueExpression.AcceptAsync(this);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;

            _context.Variables.Define(instruction.Name, valueResult.Value, isMutable: true);
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(AssignInstruction instruction)
        {
            var valueResult = await instruction.ValueExpression.AcceptAsync(this);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;

            _context.Variables.Assign(instruction.Name, valueResult.Value);
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(RunInstruction instruction)
        {
            if (!_context.Callables.TryGet(instruction.Name, out var callable))
                throw new InvalidOperationException($"Callable '{instruction.Name}' not found.");

            var args = new Arguments();
            foreach (var (key, valueExpr) in instruction.ArgExpressions)
            {
                var argResult = await valueExpr.AcceptAsync(this);
                if (argResult.Flow != FlowState.Normal)
                    return argResult;
                args[key] = argResult.Value;
            }

            var resultValue = await callable.CallAsync(args, _context);
            return VisitResult.NormalWithValue(resultValue);
        }

        public ValueTask<VisitResult> VisitAsync(FunctionInstruction instruction)
        {
            async ValueTask<object> CallAsync(Arguments args, ExecutionContext context)
            {
                var localContext = context.CreateChildContext();

                foreach (var param in instruction.Parameters)
                {
                    if (args.TryGetValue(param.Name, out var value))
                    {
                        localContext.Variables.Define(param.Name, value, isMutable: true); // Function params are mutable
                    }
                    else
                    {
                        localContext.Variables.Define(param.Name, param.DefaultValue, isMutable: true);
                    }
                }

                var visitor = new ExecutionVisitor(localContext);
                foreach (var statement in instruction.Statements)
                {
                    var result = await statement.AcceptAsync(visitor);
                    if (result.Flow == FlowState.Return)
                    {
                        return result.Value;
                    }
                    if (result.Flow != FlowState.Normal)
                    {
                        throw new InvalidOperationException($"'{result.Flow}' is not valid outside of a loop.");
                    }
                }
                return null; // Implicit return null
            }

            var callable = new DelegateCallable(CallAsync);
            _context.Callables.Register(instruction.Name, callable);
            return new ValueTask<VisitResult>(VisitResult.Normal);
        }

        public async ValueTask<VisitResult> VisitAsync(WhenInstruction instruction)
        {
            foreach (var caseBlock in instruction.Cases)
            {
                bool conditionMet = false;
                if (caseBlock.Condition == null)
                {
                    conditionMet = true; // Default case
                }
                else
                {
                    var conditionResult = await caseBlock.Condition.AcceptAsync(this);
                    if (conditionResult.Flow != FlowState.Normal)
                        return conditionResult;
                    conditionMet = IsTruthy(conditionResult.Value);
                }

                if (conditionMet)
                {
                    foreach (var inst in caseBlock.ThenBlock)
                    {
                        var result = await inst.AcceptAsync(this);
                        // If a statement alters control flow (return, break), propagate it up.
                        if (result.Flow != FlowState.Normal)
                            return result;
                    }
                    return VisitResult.Normal; // Exit after the first met case.
                }
            }
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(WhileInstruction instruction)
        {
            while (IsTruthy((await instruction.Condition.AcceptAsync(this)).Value))
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                foreach (var statement in instruction.Statements)
                {
                    var result = await statement.AcceptAsync(this);

                    if (result.Flow == FlowState.Break)
                        return VisitResult.Normal;
                    if (result.Flow == FlowState.Continue)
                        break;
                    if (result.Flow == FlowState.Return)
                        return result;
                }
            }
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(ForInstruction instruction)
        {
            var collectionResult = await instruction.EnumerableExpression.AcceptAsync(this);
            if (collectionResult.Flow != FlowState.Normal)
                return collectionResult;

            if (collectionResult.Value is not IEnumerable enumerable)
            {
                throw new InvalidOperationException("!for instruction requires an enumerable collection.");
            }

            foreach (var item in enumerable)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                var loopContext = _context.CreateChildContext();
                loopContext.Variables.Define(instruction.VariableName, item, isMutable: false);
                var loopVisitor = new ExecutionVisitor(loopContext);

                var shouldContinue = false;
                foreach (var statement in instruction.Statements)
                {
                    var result = await statement.AcceptAsync(loopVisitor);
                    if (result.Flow == FlowState.Break)
                        return VisitResult.Normal;
                    if (result.Flow == FlowState.Continue)
                    {
                        shouldContinue = true;
                        break;
                    }
                    if (result.Flow == FlowState.Return)
                        return result;
                }
                if (shouldContinue)
                    continue;
            }
            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(ReturnInstruction instruction)
        {
            var returnValue = instruction.ValueExpression != null
                ? (await instruction.ValueExpression.AcceptAsync(this)).Value
                : null;
            return VisitResult.Return(returnValue);
        }

        public ValueTask<VisitResult> VisitAsync(BreakInstruction instruction) => new(VisitResult.Break);

        public ValueTask<VisitResult> VisitAsync(ContinueInstruction instruction) => new(VisitResult.Continue);

        public async ValueTask<VisitResult> VisitAsync(DispatchInstruction instruction)
        {
            if (!_context.Callables.TryGet(instruction.Name, out var callable))
                throw new InvalidOperationException($"Callable '{instruction.Name}' not found.");

            var args = new Arguments();
            foreach (var (key, valueExpr) in instruction.ArgExpressions)
            {
                var argResult = await valueExpr.AcceptAsync(this);
                if (argResult.Flow != FlowState.Normal)
                    return argResult;
                args[key] = argResult.Value;
            }

            // Do not await the CallAsync here. The ValueTask itself is the result.
            var valueTask = callable.CallAsync(args, _context);
            return VisitResult.NormalWithValue(valueTask);
        }

        public ValueTask<VisitResult> VisitAsync(LiteralExpression expression) => new(VisitResult.NormalWithValue(expression.Value));

        public ValueTask<VisitResult> VisitAsync(VariableExpression expression)
        {
            var value = _context.Variables.Get(expression.VariableName);
            return new ValueTask<VisitResult>(VisitResult.NormalWithValue(value));
        }

        public async ValueTask<VisitResult> VisitAsync(BinaryExpression expression)
        {
            var leftResult = await expression.Left.AcceptAsync(this);
            if (leftResult.Flow != FlowState.Normal)
                return leftResult;
            var rightResult = await expression.Right.AcceptAsync(this);
            if (rightResult.Flow != FlowState.Normal)
                return rightResult;

            var left = leftResult.Value;
            var right = rightResult.Value;

            if (TryPromoteToCommonNumeric(left, right, out var numLeft, out var numRight))
            {
                object result = expression.OperatorType switch
                {
                    OperatorType.Add => numLeft + numRight,
                    OperatorType.Subtract => numLeft - numRight,
                    OperatorType.Multiply => numLeft * numRight,
                    OperatorType.Divide => numRight == 0 ? throw new DivideByZeroException() : numLeft / numRight,
                    OperatorType.Equal => numLeft == numRight,
                    OperatorType.NotEqual => numLeft != numRight,
                    OperatorType.GreaterThan => numLeft > numRight,
                    OperatorType.LessThan => numLeft < numRight,
                    OperatorType.GreaterThanOrEqual => numLeft >= numRight,
                    OperatorType.LessThanOrEqual => numLeft <= numRight,
                    _ => throw new InvalidOperationException($"Unsupported numeric operator '{expression.OperatorType}'.")
                };
                return VisitResult.NormalWithValue(result);
            }

            return expression.OperatorType switch
            {
                OperatorType.Equal => VisitResult.NormalWithValue(Equals(left, right)),
                OperatorType.NotEqual => VisitResult.NormalWithValue(!Equals(left, right)),
                _ => throw new InvalidOperationException($"Operator '{expression.OperatorType}' cannot be applied to operands of type '{left?.GetType().Name ?? "null"}' and '{right?.GetType().Name ?? "null"}'.")
            };
        }

        public async ValueTask<VisitResult> VisitAsync(UnaryExpression expression)
        {
            var operandResult = await expression.Operand.AcceptAsync(this);
            if (operandResult.Flow != FlowState.Normal)
                return operandResult;
            var operand = operandResult.Value;

            return expression.OperatorType switch
            {
                OperatorType.Not => VisitResult.NormalWithValue(!IsTruthy(operand)),
                OperatorType.Subtract => VisitResult.NormalWithValue(NegateNumeric(operand)),
                _ => throw new InvalidOperationException($"Unary operator '{expression.OperatorType}' is not supported.")
            };
        }

        public async ValueTask<VisitResult> VisitAsync(InterpolatedStringExpression expression)
        {
            var sb = new StringBuilder();
            foreach (var part in expression.Parts)
            {
                var partResult = await part.AcceptAsync(this);
                if (partResult.Flow != FlowState.Normal)
                    return partResult;
                sb.Append(partResult.Value?.ToString());
            }
            return VisitResult.NormalWithValue(sb.ToString());
        }

        public async ValueTask<VisitResult> VisitAsync(ListExpression expression)
        {
            var results = new List<object>();
            foreach (var elementExpr in expression.Elements)
            {
                var elementResult = await elementExpr.AcceptAsync(this);
                if (elementResult.Flow != FlowState.Normal)
                {
                    return elementResult;
                }
                results.Add(elementResult.Value);
            }
            return VisitResult.NormalWithValue(results);
        }

        public async ValueTask<VisitResult> VisitAsync(DictionaryExpression expression)
        {
            var results = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var (key, valueExpr) in expression.Entries)
            {
                var valueResult = await valueExpr.AcceptAsync(this);
                if (valueResult.Flow != FlowState.Normal)
                {
                    return valueResult;
                }
                results[key] = valueResult.Value;
            }
            return VisitResult.NormalWithValue(results);
        }

        public async ValueTask<VisitResult> VisitAsync(MemberAccessExpression expression)
        {
            var objResult = await expression.ObjectExpression.AcceptAsync(this);
            if (objResult.Flow != FlowState.Normal)
                return objResult;

            var targetObject = objResult.Value;
            if (targetObject == null)
            {
                throw new NullReferenceException($"Attempted to access member '{expression.MemberName}' on a null object.");
            }

            if (targetObject is IDictionary dictionary)
            {
                // For member access, we assume the key is a string.
                if (dictionary.Contains(expression.MemberName))
                {
                    return VisitResult.NormalWithValue(dictionary[expression.MemberName]);
                }
            }

            var type = targetObject.GetType();
            var property = type.GetProperty(expression.MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null && property.CanRead)
            {
                return VisitResult.NormalWithValue(property.GetValue(targetObject));
            }

            return VisitResult.NormalWithValue(null);
        }

        public async ValueTask<VisitResult> VisitAsync(MatchInstruction instruction)
        {
            var matchValueResult = await instruction.ValueExpression.AcceptAsync(this);
            if (matchValueResult.Flow != FlowState.Normal)
                return matchValueResult;

            var matchValue = matchValueResult.Value;

            foreach (var caseBlock in instruction.Cases)
            {
                if (Equals(caseBlock.CaseValue, matchValue))
                {
                    foreach (var stmt in caseBlock.ThenBlock)
                    {
                        var result = await stmt.AcceptAsync(this);
                        if (result.Flow != FlowState.Normal)
                            return result;
                    }
                    return VisitResult.Normal;
                }
            }

            if (instruction.DefaultBlock != null)
            {
                foreach (var stmt in instruction.DefaultBlock)
                {
                    var result = await stmt.AcceptAsync(this);
                    if (result.Flow != FlowState.Normal)
                        return result;
                }
            }

            return VisitResult.Normal;
        }

        public async ValueTask<VisitResult> VisitAsync(TryCatchInstruction instruction)
        {
            VisitResult finalResult = VisitResult.Normal;
            try
            {
                foreach (var stmt in instruction.TryBlock)
                {
                    var result = await stmt.AcceptAsync(this);
                    if (result.Flow != FlowState.Normal)
                    {
                        finalResult = result;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CatchClause matchedClause = null;
                foreach (var clause in instruction.CatchClauses)
                {
                    if (clause.ErrorName == null) // Default catch-all
                    {
                        if (matchedClause == null)
                            matchedClause = clause;
                        continue;
                    }
                    if (_context.Classes.TryGet(clause.ErrorName, out var expectedType) && expectedType.IsInstanceOfType(ex))
                    {
                        matchedClause = clause;
                        break;
                    }
                }

                if (matchedClause != null)
                {
                    var catchContext = _context.CreateChildContext();
                    if (!string.IsNullOrEmpty(matchedClause.VariableName))
                    {
                        catchContext.Variables.Define(matchedClause.VariableName, ex, isMutable: false);
                    }

                    var catchVisitor = new ExecutionVisitor(catchContext);
                    foreach (var stmt in matchedClause.ThenBlock)
                    {
                        var result = await stmt.AcceptAsync(catchVisitor);
                        if (result.Flow != FlowState.Normal)
                        {
                            finalResult = result;
                            break;
                        }
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (instruction.FinallyBlock != null)
                {
                    foreach (var stmt in instruction.FinallyBlock)
                    {
                        var result = await stmt.AcceptAsync(this);
                        if (result.Flow != FlowState.Normal && finalResult.Flow == FlowState.Normal)
                        {
                            finalResult = result;
                        }
                    }
                }
            }
            return finalResult;
        }

        public async ValueTask<VisitResult> VisitAsync(ThrowInstruction instruction)
        {
            if (!_context.Classes.TryGet(instruction.Name, out var exceptionType) || !typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new InvalidOperationException($"'{instruction.Name}' is not a valid and registered exception class.");
            }

            var args = new Dictionary<string, object>();
            foreach (var (key, valueExpr) in instruction.ArgExpressions)
            {
                var argResult = await valueExpr.AcceptAsync(this);
                if (argResult.Flow != FlowState.Normal)
                    return argResult;
                args[key] = argResult.Value;
            }

            args.TryGetValue("message", out var messageObj);
            var message = messageObj?.ToString();

            Exception instance;
            try
            {
                instance = message == null
                    ? (Exception)Activator.CreateInstance(exceptionType)
                    : (Exception)Activator.CreateInstance(exceptionType, message);
            }
            catch (MissingMethodException)
            {
                instance = (Exception)Activator.CreateInstance(exceptionType);
            }

            foreach (var (key, value) in args)
            {
                instance.Data[key] = value;
            }

            throw instance;
        }

        public async ValueTask<VisitResult> VisitAsync(AssertInstruction instruction)
        {
            var conditionResult = await instruction.Condition.AcceptAsync(this);
            if (conditionResult.Flow != FlowState.Normal)
                return conditionResult;

            if (!IsTruthy(conditionResult.Value))
            {
                throw new InvalidOperationException(instruction.Message ?? "Assertion failed.");
            }

            return VisitResult.Normal;
        }

        private static bool IsTruthy(object value)
        {
            if (value == null)
                return false;
            if (value is bool b)
                return b;
            if (value is string s)
                return !string.IsNullOrEmpty(s);
            if (value is IConvertible convertible)
            {
                try
                { return Convert.ToDouble(convertible, CultureInfo.InvariantCulture) != 0.0; }
                catch (Exception) { }
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
            catch (Exception) { return false; }
        }

        private static bool IsNumeric(object value) => value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;

        private static object NegateNumeric(object operand)
        {
            if (operand is decimal d)
                return -d;
            if (operand is double db)
                return -db;
            if (operand is float f)
                return -f;
            if (operand is long l)
                return -l;
            if (operand is int i)
                return -i;
            throw new InvalidOperationException($"Unary operator '-' cannot be applied to operand of type '{operand?.GetType().Name ?? "null"}'.");
        }
    }
}