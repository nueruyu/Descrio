using Descrio.Abstractions;
using Descrio.EditorTests.Builders;
using Descrio.Syntax;
using Descrio.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.EditorTests
{
    public static class TestExpressionFactory
    {
        public static LiteralExpression Literal(object value)
        {
            return new LiteralExpression(value);
        }

        public static VariableExpression Variable(string name)
        {
            return new VariableExpression(name);
        }

        public static InterpolatedStringExpression InterpolatedString(params object[] parts)
        {
            var expressions = parts.Select(p => p switch
            {
                string s => new LiteralExpression(s),
                IExpression e => e,
                _ => throw new ArgumentException($"Unsupported type for interpolated string part: {p?.GetType().Name}")
            }).ToList();
            return new InterpolatedStringExpression(expressions);
        }

        public static ListExpression List(params IExpression[] elements)
        {
            return new ListExpression(elements.ToList());
        }

        public static DictionaryExpressionBuilder Dictionary()
        {
            return new DictionaryExpressionBuilder();
        }

        public static BinaryExpression Binary(IExpression left, OperatorType op, IExpression right)
        {
            return new BinaryExpression(left, right, op);
        }

        public static UnaryExpression Unary(OperatorType op, IExpression operand)
        {
            return new UnaryExpression(op, operand);
        }

        public static MemberAccessExpression Member(this IExpression source, string memberName)
        {
            return new MemberAccessExpression(source, memberName);
        }
    }
}