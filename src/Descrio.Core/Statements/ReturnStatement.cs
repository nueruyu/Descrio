using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class ReturnStatement : IStatement
    {
        public IExpression ValueExpression { get; }

        public ReturnStatement(IExpression valueExpression)
        {
            ValueExpression = valueExpression; // Can be null
        }
    }
}