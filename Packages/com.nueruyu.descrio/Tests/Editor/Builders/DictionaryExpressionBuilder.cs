using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class DictionaryExpressionBuilder : IExpressionBuilder
    {
        private readonly Dictionary<string, IExpression> _elements = new();

        internal DictionaryExpressionBuilder()
        {
        }

        public DictionaryExpressionBuilder With(string key, IExpression value)
        {
            _elements.Add(key, value);
            return this;
        }

        public DictionaryExpressionBuilder With(string key, object value)
        {
            _elements.Add(key, new LiteralExpression(value));
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