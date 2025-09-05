using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents an instruction that throws a script-level exception.
    /// </summary>
    public class ThrowStatement : IStatement
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public ThrowStatement(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name;
            ArgExpressions = argExpressions;
        }
    }
}