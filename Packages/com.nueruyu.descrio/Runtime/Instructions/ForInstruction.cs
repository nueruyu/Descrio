using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class ForInstruction : IInstruction
    {
        public IExpression EnumerableExpression { get; }
        public string VariableName { get; }
        public IInstruction[] Statements { get; }

        public ForInstruction(IExpression enumerableExpression, string variableName, IInstruction[] statements)
        {
            EnumerableExpression = enumerableExpression;
            VariableName = variableName;
            Statements = statements;
        }

        public ValueTask<TResult> AcceptAsync<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAsync(this);
        }
    }
}