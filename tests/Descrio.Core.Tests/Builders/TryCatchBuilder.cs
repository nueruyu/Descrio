using Descrio.Abstractions;
using Descrio.Syntax.Statements;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.EditorTests.Builders
{
    public class TryCatchBuilder : IStatementBuilder
    {
        private readonly List<IStatement> _tryBlock = new();
        private readonly List<CatchClause> _catchClauses = new();
        private List<IStatement> _finallyBlock = null;

        // internal constructor to be called from TestStatementFactory
        internal TryCatchBuilder() { }

        public TryCatchBuilder WithTry(params IStatement[] statements)
        {
            _tryBlock.AddRange(statements);
            return this;
        }

        public TryCatchBuilder Catch(string errorName, string varName, params IStatement[] statements)
        {
            _catchClauses.Add(new CatchClause(errorName, varName, statements));
            return this;
        }

        public TryCatchBuilder CatchDefault(params IStatement[] statements)
        {
            _catchClauses.Add(new CatchClause(null, null, statements));
            return this;
        }

        public TryCatchBuilder Finally(params IStatement[] statements)
        {
            _finallyBlock = new List<IStatement>(statements);
            return this;
        }

        public TryCatchStatement Build()
        {
            return new TryCatchStatement(_tryBlock.ToArray(), _catchClauses, _finallyBlock?.ToArray());
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator TryCatchStatement(TryCatchBuilder builder) => builder.Build();
    }
}