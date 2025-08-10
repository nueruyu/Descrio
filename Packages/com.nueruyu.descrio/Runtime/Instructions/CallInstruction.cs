using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Descrio.Execution;

namespace Descrio
{
    public class CallInstruction : IInstruction
    {
        public string Name { get; }
        public IExpression[] ArgExpressions { get; }
        public string ReturnVariable { get; }

        public CallInstruction(string name, IExpression[] argExpressions, string returnVariable = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ArgExpressions = argExpressions ?? throw new ArgumentNullException(nameof(argExpressions));
            ReturnVariable = returnVariable;
        }

        public ValueTask AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}