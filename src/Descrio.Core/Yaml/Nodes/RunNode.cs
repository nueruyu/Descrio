using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.Linq;

namespace Descrio.Yaml.Nodes
{
    internal class RunNode : IStatementNode, IExpressionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "args")]
        public Dictionary<string, IExpressionNode> Args { get; set; }

        RunStatement ToRunInstruction() => new RunStatement(
            Name,
            Args?.ToDictionary(x => x.Key, x => x.Value.ToExpression()) ?? new());

        public IStatement ToStatement() => ToRunInstruction();

        public IExpression ToExpression() => ToRunInstruction();
    }
}