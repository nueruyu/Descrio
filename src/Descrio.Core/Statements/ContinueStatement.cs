using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
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