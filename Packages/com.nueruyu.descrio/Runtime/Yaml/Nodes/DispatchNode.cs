using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class DispatchNode : IExpressionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public Dictionary<string, IExpressionNode> Args { get; set; }

        public IExpression ToExpression() => new DispatchInstruction(
            Name,
            Args?.ToDictionary(x => x.Key, x => x.Value.ToExpression()) ?? new());
    }
}