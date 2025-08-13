using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class AssignInstruction : IInstruction
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }

        public AssignInstruction(string name, IExpression valueExpression)
        {
            Name = name;
            ValueExpression = valueExpression;
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}