using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class ImportNode
    {
        [YamlMember(Alias = "from")]
        public string From { get; set; }

        [YamlMember(Alias = "as")]
        public string As { get; set; } // For future use
    }
}