using System.Collections.Generic;

namespace Descrio
{
    public class Module
    {
        public Module(IStatement[] statements, IReadOnlyList<string> importPaths)
        {
            Statements = statements;
            ImportPaths = importPaths;
        }

        public IStatement[] Statements { get; }
        public IReadOnlyList<string> ImportPaths { get; }
    }
}