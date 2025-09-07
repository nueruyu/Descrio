using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
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