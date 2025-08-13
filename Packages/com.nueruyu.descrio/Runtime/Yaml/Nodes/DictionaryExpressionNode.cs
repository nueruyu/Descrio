using System.Collections.Generic;
using System.Linq;

namespace Descrio.Yaml.Nodes
{
    internal class DictionaryExpressionNode : IExpressionNode
    {
        public Dictionary<string, IExpressionNode> Entries { get; set; }

        public DictionaryExpressionNode(Dictionary<string, IExpressionNode> entries)
        {
            Entries = entries;
        }

        public IExpression ToExpression()
        {
            var expressions = Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToExpression());
            return new DictionaryExpression(expressions);
        }
    }
}