using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Descrio.Execution;

namespace Descrio
{
    public class RunInstruction : IInstruction, IExpression
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public RunInstruction(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ArgExpressions = argExpressions ?? throw new ArgumentNullException(nameof(argExpressions));
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}