using Descrio.Abstractions;
using Descrio.EditorTests.Builders;
using Descrio.Syntax.Statements;

namespace Descrio.EditorTests
{
    public static class TestStatementFactory
    {
        // --- Builder Entry Points ---
        public static TryCatchBuilder Try(params IStatement[] statements)
        {
            return new TryCatchBuilder().WithTry(statements);
        }

        public static RunBuilder Run(string callableName, params (string key, IExpression value)[] args)
        {
            return new RunBuilder(callableName).With(args);
        }

        public static ThrowBuilder Throw(string errorName, params (string key, IExpression value)[] args)
        {
            return new ThrowBuilder(errorName).With(args);
        }

        public static WhenBuilder When()
        {
            return new WhenBuilder();
        }

        public static DispatchBuilder Dispatch(string eventName)
        {
            return new DispatchBuilder(eventName);
        }

        public static ForBuilder For(string variableName, IExpression collection)
        {
            return new ForBuilder(variableName, collection);
        }

        public static FunctionBuilder Function(string functionName)
        {
            return new FunctionBuilder(functionName);
        }

        public static MatchBuilder Match(IExpression valueExpression)
        {
            return new MatchBuilder(valueExpression);
        }

        public static WhileBuilder While(IExpression condition, params IStatement[] statements)
        {
            return new WhileBuilder(condition).Do(statements);
        }

        // --- Simple Factory Methods ---

        public static AssertStatement Assert_(IExpression condition, string message = null)
        {
            return new AssertStatement(condition, message);
        }

        public static AssignStatement Assign(IExpression target, IExpression value)
        {
            return new AssignStatement(target, value);
        }

        public static BreakStatement Break()
        {
            return new BreakStatement();
        }

        public static ContinueStatement Continue()
        {
            return new ContinueStatement();
        }

        public static LetStatement Let(string variableName, IExpression expression)
        {
            return new LetStatement(variableName, expression);
        }

        public static ReturnStatement Return(IExpression value = null)
        {
            return new ReturnStatement(value);
        }

        public static VarStatement Var(string variableName, IExpression expression)
        {
            return new VarStatement(variableName, expression);
        }
    }
}