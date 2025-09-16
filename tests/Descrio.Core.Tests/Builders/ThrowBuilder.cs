using Descrio.Abstractions;
using Descrio.Syntax.Statements;
using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class ThrowBuilder : IStatementBuilder
    {
        private readonly string _errorName;
        private readonly Dictionary<string, IExpression> _args = new();

        internal ThrowBuilder(string errorName) => _errorName = errorName;

        public ThrowBuilder With(string key, IExpression value)
        {
            _args[key] = value;
            return this;
        }

        public ThrowBuilder With(params (string key, IExpression value)[] args)
        {
            foreach (var (key, value) in args)
            {
                _args[key] = value;
            }
            return this;
        }

        public ThrowStatement Build() => new ThrowStatement(_errorName, _args);

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator ThrowStatement(ThrowBuilder builder) => builder.Build();
    }
}