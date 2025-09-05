using System.Collections.Generic;
using System.Linq;

namespace Descrio.Yaml.Nodes
{
    internal class ListExpressionNode : IExpressionNode
    {
        public List<IExpressionNode> Elements { get; set; } = new List<IExpressionNode>();

        public ListExpressionNode(List<IExpressionNode> elements)
        {
            Elements = elements;
        }

        public IExpression ToExpression()
        {
            var expressions = Elements.Select(e => e.ToExpression()).ToList();
            return new ListExpression(expressions);
        }
    }
}