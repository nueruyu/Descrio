using Descrio.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    // Dispatch is an Expression because it returns a Task object immediately.
    public class DispatchStatement : IStatement, IExpression
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public DispatchStatement(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name;
            ArgExpressions = argExpressions;
        }
    }
}