using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;

namespace Descrio.Syntax.Statements
{
    /// <summary>
    /// Represents an instruction that throws a script-level exception.
    /// </summary>
    public class ThrowStatement : IStatement
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }
        public SourceRange Location { get; }

        public ThrowStatement(string name, IReadOnlyDictionary<string, IExpression> argExpressions, SourceRange location = null)
        {
            Name = name;
            ArgExpressions = argExpressions;
            Location = location;
        }
    }
}