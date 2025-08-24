using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class ForBuilder : IStatementBuilder
    {
        private readonly string _variableName;
        private readonly IExpression _collection;
        private readonly List<IStatement> _body = new();

        internal ForBuilder(string variableName, IExpression collection)
        {
            _variableName = variableName;
            _collection = collection;
        }

        public ForBuilder Do(params IStatement[] statements)
        {
            _body.AddRange(statements);
            return this;
        }

        public ForStatement Build()
        {
            return new ForStatement(_collection, _variableName, _body.ToArray());
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator ForStatement(ForBuilder builder) => builder.Build();
    }
}