using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Descrio.Yaml.Nodes
{
    internal class CatchClauseNode
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "as")]
        public string As { get; set; }

        [YamlMember(Alias = "then")]
        public List<IStatementNode> Then { get; set; }

        public CatchClause ToCatchClause()
        {
            var instructions = Then?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();
            return new CatchClause(Name, As, instructions);
        }
    }

    internal class TryCatchNode : IStatementNode
    {
        [YamlMember(Alias = "statements")]
        public List<IStatementNode> Try { get; set; } = new List<IStatementNode>();

        [YamlMember(Alias = "catch")]
        public List<CatchClauseNode> Catch { get; set; } = new List<CatchClauseNode>();

        [YamlMember(Alias = "finally")]
        public List<IStatementNode> Finally { get; set; } = new List<IStatementNode>();

        public IStatement ToStatement()
        {
            var tryBlock = Try.Select(s => s.ToStatement()).ToArray();
            var catchClauses = Catch.Select(c => c.ToCatchClause()).ToList();
            var finallyBlock = Finally?.Select(s => s.ToStatement()).ToArray() ?? System.Array.Empty<IStatement>();

            return new TryCatchStatement(tryBlock, catchClauses, finallyBlock);
        }
    }
}