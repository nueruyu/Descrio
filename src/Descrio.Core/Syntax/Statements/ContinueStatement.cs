using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class ContinueStatement : IStatement
    {
        public SourceRange Location { get; }

        public ContinueStatement(SourceRange location = null)
        {
            Location = location;
        }
    }
}