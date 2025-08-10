using System.Threading.Tasks;

namespace Descrio.Execution
{
    public interface IAstVisitor
    {
        ValueTask VisitAsync(SetInstruction instruction);

        ValueTask VisitAsync(CallInstruction instruction);

        ValueTask VisitAsync(DefineInstruction instruction);

        ValueTask VisitAsync(WhenInstruction instruction);

        ValueTask<object> VisitAsync(LiteralExpression expression);

        ValueTask<object> VisitAsync(VariableExpression expression);

        ValueTask<object> VisitAsync(BinaryExpression expression);
    }
}