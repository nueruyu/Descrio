using Descrio.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    public class LetInstruction : IInstruction
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }

        public LetInstruction(string name, IExpression valueExpression)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        }

        public ValueTask AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}