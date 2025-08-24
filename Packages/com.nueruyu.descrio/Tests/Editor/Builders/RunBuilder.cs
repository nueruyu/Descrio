using System.Collections.Generic;

namespace Descrio.EditorTests.Builders
{
    public class RunBuilder : IStatementBuilder
    {
        private readonly string _callableName;
        private readonly Dictionary<string, IExpression> _args = new();

        internal RunBuilder(string callableName) => _callableName = callableName;

        public RunBuilder With(string key, IExpression value)
        {
            _args[key] = value;
            return this;
        }

        public RunBuilder With(params (string key, IExpression value)[] args)
        {
            foreach (var (key, value) in args)
            {
                _args[key] = value;
            }
            return this;
        }

        public RunStatement Build() => new RunStatement(_callableName, _args);

        IStatement IStatementBuilder.BuildStatement()
        {
            return Build();
        }

        public static implicit operator RunStatement(RunBuilder builder) => builder.Build();
    }
}