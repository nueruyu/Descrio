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
        public SourceRange Location { get; }

        public MemberAccessExpression(IExpression objectExpression, string memberName, SourceRange location = null)
        {
            ObjectExpression = objectExpression;
            MemberName = memberName;
            Location = location;
        }
    }
}