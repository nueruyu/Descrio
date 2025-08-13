namespace Descrio.Execution
{
    /// <summary>
    /// Represents the state of execution flow after an instruction is executed.
    /// </summary>
    public enum FlowState
    {
        /// <summary>
        /// Execution proceeds normally to the next instruction.
        /// </summary>
        Normal,

        /// <summary>
        /// A 'return' was executed, and the function should exit with a value.
        /// </summary>
        Return,

        /// <summary>
        /// A 'break' was executed, and the current loop should terminate.
        /// </summary>
        Break,

        /// <summary>
        /// A 'continue' was executed, and the current loop should proceed to the next iteration.
        /// </summary>
        Continue
    }
}