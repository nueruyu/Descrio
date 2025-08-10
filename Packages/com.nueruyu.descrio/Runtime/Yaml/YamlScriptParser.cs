using System;
using System.IO;
using Descrio.Yaml.Nodes;
using Descrio.Yaml.Deserializers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Descrio.Parse;

namespace Descrio.Yaml
{
    public class YamlScriptParser : IScriptParser
    {
        private readonly IDeserializer _deserializer;

        public YamlScriptParser()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTagMapping("!set", typeof(SetNode))
                .WithTagMapping("!call", typeof(CallNode))
                .WithTagMapping("!define", typeof(DefineNode))
                .WithTagMapping("!when", typeof(WhenNode))
                .WithNodeDeserializer(new ExpressionDeserializer(), s => s.OnTop())
                .WithNodeDeserializer(new ParameterDefinitionDeserializer(), s => s.OnTop())
                .Build();
        }

        public Module Parse(string scriptText)
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return new Module(Array.Empty<IInstruction>(), Array.Empty<string>());
            }

            var scriptRootNode = _deserializer.Deserialize<ScriptRootNode>(new StringReader(scriptText));

            return scriptRootNode?.ToModule() ?? new Module(Array.Empty<IInstruction>(), Array.Empty<string>());
        }
    }
}