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
        public List<IInstructionNode> Then { get; set; }

        public CatchClause ToCatchClause()
        {
            var instructions = Then?.Select(s => s.ToInstruction()).ToArray() ?? System.Array.Empty<IInstruction>();
            return new CatchClause(Name, As, instructions);
        }
    }

    internal class TryCatchNode : IInstructionNode
    {
        [YamlMember(Alias = "statements")]
        public List<IInstructionNode> Try { get; set; } = new List<IInstructionNode>();

        [YamlMember(Alias = "catch")]
        public List<CatchClauseNode> Catch { get; set; } = new List<CatchClauseNode>();

        [YamlMember(Alias = "finally")]
        public List<IInstructionNode> Finally { get; set; } = new List<IInstructionNode>();

        public IInstruction ToInstruction()
        {
            var tryBlock = Try.Select(s => s.ToInstruction()).ToArray();
            var catchClauses = Catch.Select(c => c.ToCatchClause()).ToList();
            var finallyBlock = Finally?.Select(s => s.ToInstruction()).ToArray() ?? System.Array.Empty<IInstruction>();

            return new TryCatchInstruction(tryBlock, catchClauses, finallyBlock);
        }
    }
}