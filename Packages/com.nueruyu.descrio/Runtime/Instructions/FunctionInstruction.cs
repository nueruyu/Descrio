using Descrio.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    public class FunctionInstruction : IInstruction
    {
        public string Name { get; }
        public ParameterDefinition[] Parameters { get; }
        public IInstruction[] Statements { get; }

        public FunctionInstruction(string name, ParameterDefinition[] parameters, IInstruction[] statements)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }

        public ValueTask AcceptAsync(IAstVisitor visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}