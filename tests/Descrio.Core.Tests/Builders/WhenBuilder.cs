using Descrio.Abstractions;
using Descrio.Syntax.Statements;
using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class WhenBuilder : IStatementBuilder
    {
        private readonly List<WhenCaseBlock> _cases = new();

        public WhenBuilder Case(IExpression condition, params IStatement[] thenBlock)
        {
            _cases.Add(new WhenCaseBlock(condition, thenBlock));
            return this;
        }

        public WhenBuilder Default(params IStatement[] thenBlock)
        {
            _cases.Add(new WhenCaseBlock(null, thenBlock));
            return this;
        }

        public WhenStatement Build()
        {
            return new WhenStatement(_cases.ToArray());
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator WhenStatement(WhenBuilder builder) => builder.Build();
    }
}