using System.Collections.Generic;
using System.Linq;

namespace Descrio.Yaml.Nodes
{
    internal class DictionaryExpressionNode : IExpressionNode
    {
        public Dictionary<IExpressionNode, IExpressionNode> Entries { get; set; }

        public DictionaryExpressionNode(Dictionary<IExpressionNode, IExpressionNode> entries)
        {
            Entries = entries;
        }

        public IExpression ToExpression()
        {
            var expressions = Entries.ToDictionary(
                kvp => kvp.Key.ToExpression(),
                kvp => kvp.Value.ToExpression());
            return new DictionaryExpression(expressions);
        }
    }
}