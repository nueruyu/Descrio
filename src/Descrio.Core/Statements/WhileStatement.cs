using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class WhileStatement : IStatement
    {
        public IExpression Condition { get; }
        public IStatement[] Statements { get; }

        public WhileStatement(IExpression condition, IStatement[] statements)
        {
            Condition = condition;
            Statements = statements;
        }
    }
}