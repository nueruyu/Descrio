using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class MatchBuilder : IStatementBuilder
    {
        private readonly IExpression _valueExpression;
        private readonly List<MatchCaseBlock> _cases = new();
        private IStatement[] _defaultBlock = null;

        internal MatchBuilder(IExpression valueExpression)
        {
            _valueExpression = valueExpression;
        }

        public MatchBuilder Case(object value, params IStatement[] thenBlock)
        {
            _cases.Add(new MatchCaseBlock(value, thenBlock));
            return this;
        }

        public MatchBuilder Default(params IStatement[] thenBlock)
        {
            _defaultBlock = thenBlock;
            return this;
        }

        public MatchStatement Build()
        {
            return new MatchStatement(_valueExpression, _cases, _defaultBlock);
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator MatchStatement(MatchBuilder builder) => builder.Build();
    }
}