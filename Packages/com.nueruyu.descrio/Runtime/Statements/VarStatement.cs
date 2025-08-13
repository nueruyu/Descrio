using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class VarStatement : IStatement
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }

        public VarStatement(string name, IExpression valueExpression)
        {
            Name = name;
            ValueExpression = valueExpression;
        }
    }
}