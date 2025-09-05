using Descrio.Yaml.Nodes;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    class ScriptRootNode
    {
        [YamlMember(Alias = "imports")]
        public List<ImportNode> Imports { get; set; } = new();

        [YamlMember(Alias = "statements")]
        public List<IStatementNode> Statements { get; set; } = new();

        public Module ToModule()
        {
            var statements = Statements?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();
            var importPaths = Imports?.Select(i => i.From).Where(p => !string.IsNullOrEmpty(p)).ToArray() ?? System.Array.Empty<string>();
            return new Module(statements, importPaths);
        }
    }
}