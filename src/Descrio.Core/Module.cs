using System.Collections.Generic;

namespace Descrio
{
    public class Module : IAstNode
    {
        public Module(IStatement[] statements, IReadOnlyList<string> importPaths, SourceRange location = null)
        {
            Statements = statements;
            ImportPaths = importPaths;
            Location = location;
        }

        public IStatement[] Statements { get; }
        public IReadOnlyList<string> ImportPaths { get; }

        public SourceRange Location { get; }
    }
}