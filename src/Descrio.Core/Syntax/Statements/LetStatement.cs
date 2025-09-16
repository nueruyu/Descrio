using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class LetStatement : IStatement
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }
        public SourceRange Location { get; }

        public LetStatement(string name, IExpression valueExpression, SourceRange location = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
            Location = location;
        }
    }
}