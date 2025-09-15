using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class WhileStatement : IStatement
    {
        public IExpression Condition { get; }
        public IStatement[] Statements { get; }
        public SourceRange Location { get; }

        public WhileStatement(IExpression condition, IStatement[] statements, SourceRange location = null)
        {
            Condition = condition;
            Statements = statements;
            Location = location;
        }
    }
}