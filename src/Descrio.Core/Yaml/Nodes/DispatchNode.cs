using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class DispatchNode : IStatementNode, IExpressionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public Dictionary<string, IExpressionNode> Args { get; set; }

        DispatchStatement ToDispatchStatement() => new DispatchStatement(
            Name,
            Args?.ToDictionary(x => x.Key, x => x.Value.ToExpression()) ?? new());

        public IStatement ToStatement() => ToDispatchStatement();

        public IExpression ToExpression() => ToDispatchStatement();
    }
}