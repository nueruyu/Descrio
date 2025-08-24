using System;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class LambdaNode : IExpressionNode
    {
        [YamlMember(Alias = "parameters")]
        public ParameterDefinitionNode[] Parameters { get; set; } = Array.Empty<ParameterDefinitionNode>();

        [YamlMember(Alias = "statements")]
        public IStatementNode[] Statements { get; set; } = Array.Empty<IStatementNode>();

        public IExpression ToExpression()
        {
            var instructions = Statements.Select(s => s.ToStatement()).ToArray();
            var parameters = Parameters.Select(p => new ParameterDefinition(
                p.Name,
                p.Type,
                p.DefaultValue)).ToArray();
            return new LambdaExpression(parameters, instructions);
        }
    }
}