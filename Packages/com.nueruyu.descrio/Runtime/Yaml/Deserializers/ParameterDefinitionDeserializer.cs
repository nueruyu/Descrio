using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using System.Linq;

namespace Descrio.Yaml.Deserializers
{
    public class ParameterDefinitionDeserializer : INodeDeserializer
    {
        public bool Deserialize(
            YamlDotNet.Core.IParser parser,
            Type expectedType,
            Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out object value,
            ObjectDeserializer rootDeserializer)
        {
            if (expectedType != typeof(ParameterDefinition[]))
            {
                value = null;
                return false;
            }

            var paramDetailsDict = nestedObjectDeserializer(
                parser,
                typeof(Dictionary<string, ParameterDetails>)) as Dictionary<string, ParameterDetails>;

            if (paramDetailsDict == null)
            {
                value = Array.Empty<ParameterDefinition>();
                return true;
            }

            value = paramDetailsDict
                .Select(kvp => new ParameterDefinition(kvp.Key, kvp.Value.Type, kvp.Value.DefaultValue))
                .ToArray();
            return true;
        }

        internal class ParameterDetails
        {
            [YamlMember(Alias = "type")]
            public string Type { get; set; } = "any";

            [YamlMember(Alias = "default")]
            public object DefaultValue { get; set; }
        }
    }
}