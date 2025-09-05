using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class AssignStatement : IStatement
    {
        public IExpression Target { get; }
        public IExpression ValueExpression { get; }

        public AssignStatement(IExpression target, IExpression valueExpression)
        {
            Target = target;
            ValueExpression = valueExpression;
        }
    }
}