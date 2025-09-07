using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio
{
    public class VarStatement : IStatement
    {
        public string Name { get; }
        public IExpression ValueExpression { get; }
        public SourceRange Location { get; }

        public VarStatement(string name, IExpression valueExpression, SourceRange location = null)
        {
            Name = name;
            ValueExpression = valueExpression;
            Location = location;
        }
    }
}