namespace Descrio.Yaml.Nodes
{
    internal class LiteralNode : IExpressionNode
    {
        public LiteralNode(object value)
        {
            Value = value;
        }

        public object Value { get; set; }

        public IExpression ToExpression()
        {
            return new LiteralExpression(Value);
        }
    }
}