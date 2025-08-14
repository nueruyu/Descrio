using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Descrio.Execution;

namespace Descrio
{
    public class RunStatement : IStatement, IExpression
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public RunStatement(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ArgExpressions = argExpressions ?? throw new ArgumentNullException(nameof(argExpressions));
        }
    }
}