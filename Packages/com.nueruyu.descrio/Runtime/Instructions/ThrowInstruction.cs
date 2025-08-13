using System.Collections.Generic;
using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents an instruction that throws a script-level exception.
    /// </summary>
    public class ThrowInstruction : IInstruction
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, IExpression> ArgExpressions { get; }

        public ThrowInstruction(string name, IReadOnlyDictionary<string, IExpression> argExpressions)
        {
            Name = name;
            ArgExpressions = argExpressions;
        }

        public ValueTask<VisitResult> AcceptAsync<VisitResult>(IAstVisitor<VisitResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}