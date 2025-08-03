using System.Threading.Tasks;

namespace Descrio
{
    public class WhenInstruction : IInstruction
    {
        static readonly ExpressionEvaluator Evaluator = new();

        public WhenInstruction(WhenCaseBlock[] cases)
        {
            Cases = cases;
        }

        public WhenCaseBlock[] Cases { get; }

        public async ValueTask ExecuteAsync(ExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            foreach (var caseBlock in Cases)
            {
                bool conditionMet = false;
                if (caseBlock.Condition == null)
                {
                    conditionMet = true;
                }
                else
                {
                    var result = await caseBlock.Condition.EvaluateAsync(context);
                    conditionMet = Evaluator.IsTruthy(result);
                }

                if (conditionMet)
                {
                    foreach (var instruction in caseBlock.ThenBlock)
                    {
                        await instruction.ExecuteAsync(context);
                    }
                    return;
                }
            }
        }
    }
}