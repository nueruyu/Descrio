using Descrio.Abstractions;
using Descrio.Syntax;
using Descrio.Syntax.Statements;
using System.Collections.Generic;
using System.Linq;

namespace Descrio.EditorTests.Builders
{
    public class FunctionBuilder : IStatementBuilder
    {
        private readonly string _functionName;
        private readonly List<ParameterDefinition> _parameters = new();
        private readonly List<IStatement> _body = new();

        internal FunctionBuilder(string functionName)
        {
            _functionName = functionName;
        }

        public FunctionBuilder WithParams(params string[] paramNames)
        {
            _parameters.AddRange(paramNames.Select(p => new ParameterDefinition(p, null, null)));
            return this;
        }

        public FunctionBuilder WithParams(params ParameterDefinition[] parameters)
        {
            _parameters.AddRange(parameters);
            return this;
        }

        public FunctionBuilder Body(params IStatement[] statements)
        {
            _body.AddRange(statements);
            return this;
        }

        public FunctionStatement Build()
        {
            return new FunctionStatement(_functionName, _parameters.ToArray(), _body.ToArray());
        }

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator FunctionStatement(FunctionBuilder builder) => builder.Build();
    }
}