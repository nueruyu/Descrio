using Descrio.Abstractions;
using Descrio.Data;
using Descrio.Execution;
using System.Threading.Tasks;

namespace Descrio.Syntax.Expressions
{
    public class VariableExpression : IExpression
    {
        private readonly string _variableName;

        public VariableExpression(string variableName, SourceRange location = null)
        {
            _variableName = variableName;
            Location = location;
        }

        public string VariableName => _variableName;
        public SourceRange Location { get; }
    }
}