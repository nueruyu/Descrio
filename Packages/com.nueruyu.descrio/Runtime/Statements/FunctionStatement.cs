using Descrio.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    public class FunctionStatement : IStatement
    {
        public string Name { get; }
        public ParameterDefinition[] Parameters { get; }
        public IStatement[] Statements { get; }

        public FunctionStatement(string name, ParameterDefinition[] parameters, IStatement[] statements)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }
    }
}