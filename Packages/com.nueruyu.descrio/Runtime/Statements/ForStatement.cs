using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class ForStatement : IStatement
    {
        public IExpression EnumerableExpression { get; }
        public string VariableName { get; }
        public IStatement[] Statements { get; }

        public ForStatement(IExpression enumerableExpression, string variableName, IStatement[] statements)
        {
            EnumerableExpression = enumerableExpression;
            VariableName = variableName;
            Statements = statements;
        }
    }
}