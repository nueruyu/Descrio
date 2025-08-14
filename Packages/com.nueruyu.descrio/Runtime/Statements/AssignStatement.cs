using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class AssignStatement : IStatement
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }

        public AssignStatement(string name, IExpression valueExpression)
        {
            Name = name;
            ValueExpression = valueExpression;
        }
    }
}