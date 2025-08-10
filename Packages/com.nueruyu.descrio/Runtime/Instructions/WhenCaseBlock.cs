namespace Descrio
{
    public class WhenCaseBlock
    {
        public WhenCaseBlock(IExpression condition, IInstruction[] thenBlock)
        {
            Condition = condition;
            ThenBlock = thenBlock ?? throw new System.ArgumentNullException(nameof(thenBlock));
        }

        public IExpression Condition { get; }
        public IInstruction[] ThenBlock { get; }
    }
}