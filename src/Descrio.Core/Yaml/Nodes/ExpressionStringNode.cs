namespace Descrio.Yaml.Nodes
{
    internal class ExpressionStringNode : IExpressionNode
    {
        public string ExpressionString { get; set; }

        public ExpressionStringNode(string expressionString)
        {
            ExpressionString = expressionString;
        }

        public IExpression ToExpression()
        {
            var parser = new Parse.Expressions.ExpressionParser(ExpressionString);
            return parser.Parse();
        }
    }
}