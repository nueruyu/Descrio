using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class DispatchBuilder : IStatementBuilder
    {
        private readonly string _eventName;
        private readonly Dictionary<string, IExpression> _arguments = new();

        internal DispatchBuilder(string eventName)
        {
            _eventName = eventName;
        }

        public DispatchBuilder With(string key, object value)
        {
            _arguments.Add(key, new LiteralExpression(value));
            return this;
        }

        public DispatchBuilder With(string key, IExpression value)
        {
            _arguments.Add(key, value);
            return this;
        }

        public DispatchStatement Build()
        {
            return new DispatchStatement(_eventName, _arguments);
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator DispatchStatement(DispatchBuilder builder) => builder.Build();
    }
}