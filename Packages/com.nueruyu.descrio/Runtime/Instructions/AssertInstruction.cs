using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents an assertion that checks a condition and throws an exception if it's false.
    /// </summary>
    public class AssertInstruction : IInstruction
    {
        public IExpression Condition { get; }
        public string Message { get; }

        public AssertInstruction(IExpression condition, string message)
        {
            Condition = condition;
            Message = message;
        }

        public ValueTask<VisitResult> AcceptAsync<VisitResult>(IAstVisitor<VisitResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}