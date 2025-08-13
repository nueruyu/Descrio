using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents a member access operation (e.g., object.property).
    /// </summary>
    public class MemberAccessExpression : IExpression
    {
        public IExpression ObjectExpression { get; }
        public string MemberName { get; }

        public MemberAccessExpression(IExpression objectExpression, string memberName)
        {
            ObjectExpression = objectExpression;
            MemberName = memberName;
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}