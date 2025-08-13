using System;
using Descrio.Parse;
using Descrio.Parse.Expressions;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Descrio.Yaml.Nodes;
using System.Collections.Generic;
using System.Linq;

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

            var instance = nestedObjectDeserializer(parser, typeof(object));
            value = ConvertItem(instance);
            return true;
        }

        private IExpressionNode ConvertItem(object item)
        {
            switch (item)
            {
                case IExpressionNode expressionNode:
                    return expressionNode;

                case List<object> list:
                    return new ListExpressionNode(list.Select(ConvertItem).ToList());

                case Dictionary<object, object> dict:
                    return new DictionaryExpressionNode(dict.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => ConvertItem(kvp.Value)
                    ));

                case string s:
                    return new LiteralNode(ValueConverter.Convert(s));

                default:
                    return new LiteralNode(item);
            }
        }
    }
}