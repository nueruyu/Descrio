using System.Threading.Tasks;
using Descrio.Execution;

namespace Descrio
{
    /// <summary>
    /// Represents an assertion that checks a condition and throws an exception if it's false.
    /// </summary>
    public class AssertStatement : IStatement
    {
        public IExpression Condition { get; }
        public string Message { get; }
        public SourceRange Location { get; }

        public AssertStatement(IExpression condition, string message, SourceRange location = null)
        {
            Condition = condition;
            Message = message;
            Location = location;
        }
    }
}