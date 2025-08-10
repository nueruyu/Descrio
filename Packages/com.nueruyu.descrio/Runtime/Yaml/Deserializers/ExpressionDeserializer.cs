using System;
using Descrio.Parse;
using Descrio.Parse.Expressions;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Deserializers
{
    public class ExpressionDeserializer : INodeDeserializer
    {
        public bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object> nestedObjectDeserializer,
            out object value,
            ObjectDeserializer rootDeserializer)
        {
            if (!typeof(IExpression).IsAssignableFrom(expectedType))
            {
                value = null;
                return false;
            }

            if (parser.Accept(out Scalar scalar) &&
                scalar.Value != null &&
                scalar.Value.StartsWith("${") && scalar.Value.EndsWith("}"))
            {
                parser.Consume<Scalar>();

                var expressionString = scalar.Value.Length > 3
                    ? scalar.Value.Substring(2, scalar.Value.Length - 3).Trim()
                    : "";

                var expressionParser = new ExpressionParser(expressionString);
                value = expressionParser.Parse();
                return true;
            }

            var rawLiteral = nestedObjectDeserializer(parser, typeof(object));

            if (rawLiteral is string stringValue)
            {
                value = new LiteralExpression(ValueConverter.Convert(stringValue));
            }
            else
            {
                value = new LiteralExpression(rawLiteral);
            }

            return true;
        }
    }
}