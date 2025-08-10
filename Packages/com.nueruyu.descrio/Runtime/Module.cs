using Descrio.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Descrio
{
    public class Module
    {
        public Module(IInstruction[] statements, IReadOnlyList<string> importPaths)
        {
            Statements = statements;
            ImportPaths = importPaths;
        }

        public IInstruction[] Statements { get; }
        public IReadOnlyList<string> ImportPaths { get; }

        public async ValueTask AcceptAsync(IAstVisitor visitor)
        {
            foreach (var statement in Statements)
            {
                await statement.AcceptAsync(visitor);
            }
        }
    }
}