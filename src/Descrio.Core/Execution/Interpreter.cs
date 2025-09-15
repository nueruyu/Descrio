using Descrio.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Descrio.Execution.Callables;
using System.Threading.Tasks;
using Descrio.Syntax;
using Module = Descrio.Syntax.Module;
using Descrio.Syntax.Expressions;
using Descrio.Syntax.Statements;
using Descrio.Data;

namespace Descrio.Execution
{
    public class Interpreter
    {
        private readonly ExecutionContext _context;

        public Interpreter(ExecutionContext context)
        {
            _context = context;
        }

        public async ValueTask ExecuteAsync(Module module)
        {
            foreach (var statement in module.Statements)
            {
                var result = await ExecuteAsync(statement);
                // A top-level return could potentially halt the script, but for now, we continue execution.
                if (result.Flow == FlowState.Return)
                {
                    break; // Exit the module execution on a top-level return.
                }
            }
        }

        public ValueTask<VisitResult> ExecuteAsync(IStatement statement)
        {
            return statement switch
            {
                LetStatement s => ExecuteAsync(s),
                VarStatement s => ExecuteAsync(s),
                AssignStatement s => ExecuteAsync(s),
                RunStatement s => ExecuteAsync(s),
                FunctionStatement s => ExecuteAsync(s),
                WhenStatement s => ExecuteAsync(s),
                WhileStatement s => ExecuteAsync(s),
                ForStatement s => ExecuteAsync(s),
                ReturnStatement s => ExecuteAsync(s),
                BreakStatement s => ExecuteAsync(s),
                ContinueStatement s => ExecuteAsync(s),
                MatchStatement s => ExecuteAsync(s),
                TryCatchStatement s => ExecuteAsync(s),
                ThrowStatement s => ExecuteAsync(s),
                AssertStatement s => ExecuteAsync(s),
                IExpression e => ExecuteAsync(e),
                _ => throw new NotImplementedException($"Execution for {statement.GetType().Name} is not implemented.")
            };
        }

        public ValueTask<VisitResult> ExecuteAsync(IExpression expression)
        {
            return expression switch
            {
                LiteralExpression e => ExecuteAsync(e),
                VariableExpression e => ExecuteAsync(e),
                BinaryExpression e => ExecuteAsync(e),
                UnaryExpression e => ExecuteAsync(e),
                InterpolatedStringExpression e => ExecuteAsync(e),
                ListExpression e => ExecuteAsync(e),
                DictionaryExpression e => ExecuteAsync(e),
                MemberAccessExpression e => ExecuteAsync(e),
                LambdaExpression e => ExecuteAsync(e),
                DispatchStatement e => ExecuteAsync(e),
                RunStatement e => ExecuteAsync(e),
                GroupingExpression e => ExecuteAsync(e),
                EmbeddedExpression e => ExecuteAsync(e),
                _ => throw new NotImplementedException($"Execution for {expression.GetType().Name} is not implemented.")
            };
        }

        private async ValueTask<VisitResult> ExecuteAsync(LetStatement statement)
        {
            var valueResult = await ExecuteAsync(statement.ValueExpression);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;

            _context.Variables.Define(statement.Name, valueResult.Value, isMutable: false);
            return VisitResult.Normal;
        }

        private async ValueTask<VisitResult> ExecuteAsync(VarStatement statement)
        {
            var valueResult = await ExecuteAsync(statement.ValueExpression);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;

            _context.Variables.Define(statement.Name, valueResult.Value, isMutable: true);
            return VisitResult.Normal;
        }

        private async ValueTask<VisitResult> ExecuteAsync(AssignStatement statement)
        {
            var valueResult = await ExecuteAsync(statement.ValueExpression);
            if (valueResult.Flow != FlowState.Normal)
                return valueResult;
            var value = valueResult.Value;

            if (statement.Target is VariableExpression varExpr)
            {
                _context.Variables.Assign(varExpr.VariableName, value);
                return VisitResult.Normal;
            }

            if (statement.Target is MemberAccessExpression memberAccessExpr)
            {
                var objResult = await ExecuteAsync(memberAccessExpr.ObjectExpression);
                if (objResult.Flow != FlowState.Normal)
                    return objResult;

                var targetObject = objResult.Value;
                if (targetObject == null)
                {
                    throw new NullReferenceException($"Attempted to assign to member '{memberAccessExpr.MemberName}' on a null object.");
                }

                if (targetObject is IDictionary dict)
                {
                    // For dictionaries, we assume the key is a string for member access
                    dict[memberAccessExpr.MemberName] = value;
                    return VisitResult.Normal;
                }

                var type = targetObject.GetType();
                var property = type.GetProperty(memberAccessExpr.MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(targetObject, value);
                    return VisitResult.Normal;
                }

                var field = type.GetField(memberAccessExpr.MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    field.SetValue(targetObject, value);
                    return VisitResult.Normal;
                }

                throw new InvalidOperationException($"Member '{memberAccessExpr.MemberName}' not found or is not assignable on type '{type.Name}'.");
            }

            throw new InvalidOperationException("Invalid assignment target.");
        }

        private async ValueTask<VisitResult> ExecuteAsync(RunStatement statement)
        {
            if (!_context.Callables.TryGet(statement.Name, out var callable))
            {
                if (_context.Variables.TryGet(statement.Name, out var variableValue))
                {
                    if (variableValue is ICallable variableAsCallable)
                    {
                        callable = variableAsCallable;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Variable '{statement.Name}' is not a callable function.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Callable '{statement.Name}' not found.");
                }
            }

            var args = new Arguments();
            foreach (var (key, valueExpr) in statement.ArgExpressions)
            {
                var argResult = await ExecuteAsync(valueExpr);
                if (argResult.Flow != FlowState.Normal)
                    return argResult;
                args[key] = argResult.Value;
            }

            var resultValue = await callable.CallAsync(args, _context);
            return VisitResult.NormalWithValue(resultValue);
        }

        private ValueTask<VisitResult> ExecuteAsync(FunctionStatement statement)
        {
            var callable = CreateCallable(statement.Parameters, statement.Statements, _context);
            _context.Callables.Register(statement.Name, callable);
            return new ValueTask<VisitResult>(VisitResult.Normal);
        }

        private ValueTask<VisitResult> ExecuteAsync(LambdaExpression expression)
        {
            var callable = CreateCallable(expression.Parameters, expression.Statements, _context);
            return new ValueTask<VisitResult>(VisitResult.NormalWithValue(callable));
        }

        /// <summary>
        /// Creates a callable delegate that encapsulates the logic for executing a function's body.
        /// This method is shared by both named functions and lambda expressions.
        /// </summary>
        /// <param name="parameters">The list of parameter definitions for the function.</param>
        /// <param name="statements">The array of statements that form the function's body.</param>
        /// <param name="closureContext">The execution context at the point of definition, which will be captured to form a closure.</param>
        /// <returns>An ICallable instance ready to be executed.</returns>
        private ICallable CreateCallable(
            ParameterDefinition[] parameters,
            IStatement[] statements,
            ExecutionContext closureContext)
        {
            async ValueTask<object> CallAsync(Arguments args, ExecutionContext callSiteContext)
            {
                var localContext = closureContext.CreateChildContext(callSiteContext.CancellationToken);

                foreach (var param in parameters)
                {
                    if (args.TryGetValue(param.Name, out var value))
                    {
                        localContext.Variables.Define(param.Name, value, isMutable: true);
                    }
                    else
                    {
                        localContext.Variables.Define(param.Name, param.DefaultValue, isMutable: true);
                    }
                }

                var interpreter = new Interpreter(localContext);
                foreach (var stmt in statements)
                {
                    var result = await interpreter.ExecuteAsync(stmt);

                    if (result.Flow == FlowState.Return)
                    {
                        return result.Value;
                    }
                    if (result.Flow != FlowState.Normal)
                    {
                        throw new InvalidOperationException($"'{result.Flow}' is not valid outside of a loop.");
                    }
                }
                return null;
            }

            return new DelegateCallable(CallAsync);
        }

        private async ValueTask<VisitResult> ExecuteAsync(WhenStatement statement)
        {
            foreach (var caseBlock in statement.Cases)
            {
                bool conditionMet = false;
                if (caseBlock.Condition == null)
                {
                    conditionMet = true; // Default case
                }
                else
                {
                    var conditionResult = await ExecuteAsync(caseBlock.Condition);
                    if (conditionResult.Flow != FlowState.Normal)
                        return conditionResult;
                    conditionMet = IsTruthy(conditionResult.Value);
                }

                if (conditionMet)
                {
                    foreach (var inst in caseBlock.ThenBlock)
                    {
                        var result = await ExecuteAsync(inst);
                        // If a statement alters control flow (return, break), propagate it up.
                        if (result.Flow != FlowState.Normal)
                            return result;
                    }
                    return VisitResult.Normal; // Exit after the first met case.
                }
            }
            return VisitResult.Normal;
        }

        private async ValueTask<VisitResult> ExecuteAsync(WhileStatement statement)
        {
            while (IsTruthy((await ExecuteAsync(statement.Condition)).Value))
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                foreach (var stmt in statement.Statements)
                {
                    var result = await ExecuteAsync(stmt);

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

        private async ValueTask<VisitResult> ExecuteAsync(ForStatement statement)
        {
            var collectionResult = await ExecuteAsync(statement.EnumerableExpression);
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
                loopContext.Variables.Define(statement.VariableName, item, isMutable: false);
                var loopInterpreter = new Interpreter(loopContext);

                var shouldContinue = false;
                foreach (var stmt in statement.Statements)
                {
                    var result = await loopInterpreter.ExecuteAsync(stmt);
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

        private async ValueTask<VisitResult> ExecuteAsync(ReturnStatement statement)
        {
            var returnValue = statement.ValueExpression != null
                ? (await ExecuteAsync(statement.ValueExpression)).Value
                : null;
            return VisitResult.Return(returnValue);
        }

        private ValueTask<VisitResult> ExecuteAsync(BreakStatement statement) => new(VisitResult.Break);

        private ValueTask<VisitResult> ExecuteAsync(ContinueStatement statement) => new(VisitResult.Continue);

        private async ValueTask<VisitResult> ExecuteAsync(DispatchStatement statement)
        {
            if (!_context.Callables.TryGet(statement.Name, out var callable))
                throw new InvalidOperationException($"Callable '{statement.Name}' not found.");

            var args = new Arguments();
            foreach (var (key, valueExpr) in statement.ArgExpressions)
            {
                var argResult = await ExecuteAsync(valueExpr);
                if (argResult.Flow != FlowState.Normal)
                    return argResult;
                args[key] = argResult.Value;
            }

            // Do not await the CallAsync here. The ValueTask itself is the result.
            var valueTask = callable.CallAsync(args, _context);
            return VisitResult.NormalWithValue(valueTask);
        }

        private ValueTask<VisitResult> ExecuteAsync(LiteralExpression expression) => new(VisitResult.NormalWithValue(expression.Value));

        private ValueTask<VisitResult> ExecuteAsync(VariableExpression expression)
        {
            var value = _context.Variables.Get(expression.VariableName);
            return new ValueTask<VisitResult>(VisitResult.NormalWithValue(value));
        }

        private async ValueTask<VisitResult> ExecuteAsync(BinaryExpression expression)
        {
            var leftResult = await ExecuteAsync(expression.Left);
            if (leftResult.Flow != FlowState.Normal)
                return leftResult;
            var rightResult = await ExecuteAsync(expression.Right);
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

        private async ValueTask<VisitResult> ExecuteAsync(UnaryExpression expression)
        {
            var operandResult = await ExecuteAsync(expression.Operand);
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

        private async ValueTask<VisitResult> ExecuteAsync(InterpolatedStringExpression expression)
        {
            var sb = new StringBuilder();
            foreach (var part in expression.Parts)
            {
                var partResult = await ExecuteAsync(part);
                if (partResult.Flow != FlowState.Normal)
                    return partResult;
                sb.Append(partResult.Value?.ToString());
            }
            return VisitResult.NormalWithValue(sb.ToString());
        }

        private async ValueTask<VisitResult> ExecuteAsync(ListExpression expression)
        {
            var results = new List<object>();
            foreach (var elementExpr in expression.Elements)
            {
                var elementResult = await ExecuteAsync(elementExpr);
                if (elementResult.Flow != FlowState.Normal)
                {
                    return elementResult;
                }
                results.Add(elementResult.Value);
            }
            return VisitResult.NormalWithValue(results);
        }

        private async ValueTask<VisitResult> ExecuteAsync(DictionaryExpression expression)
        {
            var results = new Dictionary<object, object>();
            foreach (var (keyExpr, valueExpr) in expression.Entries)
            {
                var keyResult = await ExecuteAsync(keyExpr);
                if (keyResult.Flow != FlowState.Normal)
                {
                    return keyResult;
                }

                var valueResult = await ExecuteAsync(valueExpr);
                if (valueResult.Flow != FlowState.Normal)
                {
                    return valueResult;
                }
                results[keyResult.Value] = valueResult.Value;
            }
            return VisitResult.NormalWithValue(results);
        }

        private async ValueTask<VisitResult> ExecuteAsync(MemberAccessExpression expression)
        {
            var objResult = await ExecuteAsync(expression.ObjectExpression);
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

        private async ValueTask<VisitResult> ExecuteAsync(MatchStatement statement)
        {
            var matchValueResult = await ExecuteAsync(statement.ValueExpression);
            if (matchValueResult.Flow != FlowState.Normal)
                return matchValueResult;

            var matchValue = matchValueResult.Value;

            foreach (var caseBlock in statement.Cases)
            {
                if (Equals(caseBlock.CaseValue, matchValue))
                {
                    foreach (var stmt in caseBlock.ThenBlock)
                    {
                        var result = await ExecuteAsync(stmt);
                        if (result.Flow != FlowState.Normal)
                            return result;
                    }
                    return VisitResult.Normal;
                }
            }

            if (statement.DefaultBlock != null)
            {
                foreach (var stmt in statement.DefaultBlock)
                {
                    var result = await ExecuteAsync(stmt);
                    if (result.Flow != FlowState.Normal)
                        return result;
                }
            }

            return VisitResult.Normal;
        }

        private async ValueTask<VisitResult> ExecuteAsync(TryCatchStatement statement)
        {
            VisitResult finalResult = VisitResult.Normal;
            try
            {
                foreach (var stmt in statement.TryBlock)
                {
                    var result = await ExecuteAsync(stmt);
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
                foreach (var clause in statement.CatchClauses)
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

                    var catchInterpreter = new Interpreter(catchContext);
                    foreach (var stmt in matchedClause.ThenBlock)
                    {
                        var result = await catchInterpreter.ExecuteAsync(stmt);
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
                if (statement.FinallyBlock != null)
                {
                    foreach (var stmt in statement.FinallyBlock)
                    {
                        var result = await ExecuteAsync(stmt);
                        if (result.Flow != FlowState.Normal && finalResult.Flow == FlowState.Normal)
                        {
                            finalResult = result;
                        }
                    }
                }
            }
            return finalResult;
        }

        private async ValueTask<VisitResult> ExecuteAsync(ThrowStatement statement)
        {
            if (!_context.Classes.TryGet(statement.Name, out var exceptionType) || !typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new InvalidOperationException($"'{statement.Name}' is not a valid and registered exception class.");
            }

            var args = new Dictionary<string, object>();
            foreach (var (key, valueExpr) in statement.ArgExpressions)
            {
                var argResult = await ExecuteAsync(valueExpr);
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

        private async ValueTask<VisitResult> ExecuteAsync(AssertStatement statement)
        {
            var conditionResult = await ExecuteAsync(statement.Condition);
            if (conditionResult.Flow != FlowState.Normal)
                return conditionResult;

            if (!IsTruthy(conditionResult.Value))
            {
                throw new InvalidOperationException(statement.Message ?? "Assertion failed.");
            }

            return VisitResult.Normal;
        }

        private ValueTask<VisitResult> ExecuteAsync(GroupingExpression expression)
        {
            // Executing a grouping is the same as executing its inner expression.
            // The parentheses only affect parsing precedence, not evaluation logic.
            return ExecuteAsync(expression.Expression);
        }

        private ValueTask<VisitResult> ExecuteAsync(EmbeddedExpression expression)
        {
            return ExecuteAsync(expression.InnerExpression);
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