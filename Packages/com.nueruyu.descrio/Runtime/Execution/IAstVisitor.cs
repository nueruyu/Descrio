using System.Threading.Tasks;

namespace Descrio.Execution
{
    public interface IAstVisitor
    {
        ValueTask VisitAsync(LetInstruction instruction);

        ValueTask<object> VisitAsync(RunInstruction instruction);

        ValueTask VisitAsync(FunctionInstruction instruction);

        ValueTask VisitAsync(WhenInstruction instruction);

        ValueTask<object> VisitAsync(LiteralExpression expression);

        ValueTask<object> VisitAsync(VariableExpression expression);

        ValueTask<object> VisitAsync(BinaryExpression expression);
    }
}