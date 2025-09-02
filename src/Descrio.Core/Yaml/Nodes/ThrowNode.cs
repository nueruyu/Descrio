using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ThrowNode : IStatementNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public Dictionary<string, IExpressionNode> Args { get; set; }

        public IStatement ToStatement()
        {
            var args = Args?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToExpression()) ?? new Dictionary<string, IExpression>();
            return new ThrowStatement(Name, args);
        }
    }
}