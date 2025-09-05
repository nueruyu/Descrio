using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ParameterDefinitionNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "default")]
        public object DefaultValue { get; set; }
    }
}