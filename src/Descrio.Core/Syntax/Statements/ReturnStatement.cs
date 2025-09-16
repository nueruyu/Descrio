using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Statements
{
    public class ReturnStatement : IStatement
    {
        public IExpression ValueExpression { get; }
        public SourceRange Location { get; }

        public ReturnStatement(IExpression valueExpression, SourceRange location = null)
        {
            ValueExpression = valueExpression; // Can be null
            Location = location;
        }
    }
}