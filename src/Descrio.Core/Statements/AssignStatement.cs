using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class AssignStatement : IStatement
    {
        public IExpression Target { get; }
        public IExpression ValueExpression { get; }
        public SourceRange Location { get; }

        public AssignStatement(IExpression target, IExpression valueExpression, SourceRange location = null)
        {
            Target = target;
            ValueExpression = valueExpression;
            Location = location;
        }
    }
}