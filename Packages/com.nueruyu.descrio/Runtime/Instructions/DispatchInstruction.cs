using Descrio.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    // Dispatch is an Expression because it returns a Task object immediately.
    public class DispatchInstruction : IExpression
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public DispatchInstruction(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name;
            ArgExpressions = argExpressions;
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}