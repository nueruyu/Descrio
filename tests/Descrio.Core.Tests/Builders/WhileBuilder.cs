using Descrio.Abstractions;
using Descrio.Syntax.Statements;
using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class WhileBuilder : IStatementBuilder
    {
        private readonly IExpression _condition;
        private readonly List<IStatement> _body = new();

        internal WhileBuilder(IExpression condition)
        {
            _condition = condition;
        }

        public WhileBuilder Do(params IStatement[] statements)
        {
            _body.AddRange(statements);
            return this;
        }

        public WhileStatement Build()
        {
            return new WhileStatement(_condition, _body.ToArray());
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator WhileStatement(WhileBuilder builder) => builder.Build();
    }
}