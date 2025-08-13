using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class WhenNode : IStatementNode
    {
        [YamlMember(Alias = "cases")]
        public List<WhenCaseBlockNode> Cases { get; set; }

        public IStatement ToStatement()
        {
            var cases = Cases?.Select(c => c.ToCaseBlock()).ToArray();
            return new WhenStatement(cases);
        }
    }
}