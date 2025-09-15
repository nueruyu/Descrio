using Descrio.Abstractions;
using Descrio.Syntax.Expressions;
using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class DictionaryExpressionBuilder : IExpressionBuilder
    {
        private readonly Dictionary<IExpression, IExpression> _elements = new();

        internal DictionaryExpressionBuilder()
        {
        }

        public DictionaryExpressionBuilder With(IExpression key, IExpression value)
        {
            _elements.Add(key, value);
            return this;
        }

        public DictionaryExpressionBuilder With(object key, object value)
        {
            _elements.Add(new LiteralExpression(key), new LiteralExpression(value));
            return this;
        }

        public DictionaryExpression Build()
        {
            return new DictionaryExpression(_elements);
        }

        IExpression IExpressionBuilder.BuildExpression()
        {
            return Build();
        }

        public static implicit operator DictionaryExpression(DictionaryExpressionBuilder builder) => builder.Build();
    }
}