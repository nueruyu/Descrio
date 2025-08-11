using System;
using System.IO;
using Descrio.Yaml.Nodes;
using Descrio.Yaml.Serialization;
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
                .WithTagMapping("!let", typeof(LetNode))
                .WithTagMapping("!run", typeof(RunNode))
                .WithTagMapping("!function", typeof(FunctionNode))
                .WithTagMapping("!when", typeof(WhenNode))
                .WithTagMapping("!expr", typeof(ExpressionStringNode))
                .WithNodeDeserializer(new ExpressionNodeDeserializer(), s => s.OnTop())
                .WithTypeConverter(new ExpressionStringNodeTypeConverter())
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