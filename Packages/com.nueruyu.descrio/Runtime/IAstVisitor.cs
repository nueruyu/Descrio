using System.Threading.Tasks;

namespace Descrio
{
    public interface IAstVisitor<TResult>
    {
        // Instructions
        ValueTask<TResult> VisitAsync(LetInstruction instruction);

        ValueTask<TResult> VisitAsync(VarInstruction instruction);

        ValueTask<TResult> VisitAsync(AssignInstruction instruction);

        ValueTask<TResult> VisitAsync(RunInstruction instruction);

        ValueTask<TResult> VisitAsync(FunctionInstruction instruction);

        ValueTask<TResult> VisitAsync(WhenInstruction instruction);

        ValueTask<TResult> VisitAsync(ForInstruction instruction);

        ValueTask<TResult> VisitAsync(WhileInstruction instruction);

        ValueTask<TResult> VisitAsync(ReturnInstruction instruction);

        ValueTask<TResult> VisitAsync(BreakInstruction instruction);

        ValueTask<TResult> VisitAsync(ContinueInstruction instruction);

        ValueTask<TResult> VisitAsync(MatchInstruction instruction);

        ValueTask<TResult> VisitAsync(TryCatchInstruction instruction);

        ValueTask<TResult> VisitAsync(ThrowInstruction instruction);

        ValueTask<TResult> VisitAsync(AssertInstruction instruction);

        // Expressions
        ValueTask<TResult> VisitAsync(DispatchInstruction instruction);

        ValueTask<TResult> VisitAsync(LiteralExpression expression);

        ValueTask<TResult> VisitAsync(VariableExpression expression);

        ValueTask<TResult> VisitAsync(BinaryExpression expression);

        ValueTask<TResult> VisitAsync(UnaryExpression expression);

        ValueTask<TResult> VisitAsync(InterpolatedStringExpression expression);

        ValueTask<TResult> VisitAsync(ListExpression expression);

        ValueTask<TResult> VisitAsync(DictionaryExpression expression);

        ValueTask<TResult> VisitAsync(MemberAccessExpression expression);
    }
}