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

        async ValueTask IInstruction.AcceptAsync(IAstVisitor visitor)
        {
            await visitor.VisitAsync(this);
        }

        ValueTask<object> IExpression.AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}