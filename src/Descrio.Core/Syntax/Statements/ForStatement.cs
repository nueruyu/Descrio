using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class ForStatement : IStatement
    {
        public IExpression EnumerableExpression { get; }
        public string VariableName { get; }
        public IStatement[] Statements { get; }
        public SourceRange Location { get; }

        public ForStatement(IExpression enumerableExpression, string variableName, IStatement[] statements, SourceRange location = null)
        {
            EnumerableExpression = enumerableExpression;
            VariableName = variableName;
            Statements = statements;
            Location = location;
        }
    }
}