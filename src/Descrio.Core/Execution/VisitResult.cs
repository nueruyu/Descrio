namespace Descrio.Execution
{
    /// <summary>
    /// A structure that encapsulates the result of visiting an AST node.
    /// It contains the resulting value and the current state of the control flow.
    /// </summary>
    public readonly struct VisitResult
    {
        public readonly object Value;
        public readonly FlowState Flow;

        private VisitResult(object value, FlowState flow)
        {
            Value = value;
            Flow = flow;
        }

        public static readonly VisitResult Normal = new VisitResult(null, FlowState.Normal);

        public static VisitResult NormalWithValue(object value) => new VisitResult(value, FlowState.Normal);

        public static readonly VisitResult Break = new VisitResult(null, FlowState.Break);
        public static readonly VisitResult Continue = new VisitResult(null, FlowState.Continue);

        public static VisitResult Return(object value) => new VisitResult(value, FlowState.Return);
    }
}