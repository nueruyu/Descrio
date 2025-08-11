using System;
using Descrio.Parse;
using Descrio.Parse.Expressions;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Descrio.Yaml.Nodes;

namespace Descrio.Yaml.Serialization
{
    public class ExpressionNodeDeserializer : INodeDeserializer
    {
        public bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object> nestedObjectDeserializer,
            out object value,
            ObjectDeserializer rootDeserializer)
        {
            if (expectedType != typeof(IExpressionNode))
            {
                value = null;
                return false;
            }

            parser.Accept<NodeEvent>(out var nextNode);

            var instance = nestedObjectDeserializer(parser, typeof(object));

            if (instance is IExpressionNode expressionNode)
            {
                value = expressionNode;
                return true;
            }

            value = new LiteralNode(instance);
            return true;
        }
    }
}