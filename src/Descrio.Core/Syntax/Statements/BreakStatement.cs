using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class BreakStatement : IStatement
    {
        public SourceRange Location { get; }

        public BreakStatement(SourceRange location = null)
        {
            Location = location;
        }
    }
}