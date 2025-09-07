using System.Collections.Generic;

namespace Descrio
{
    /// <summary>
    /// Represents a lambda expression, an anonymous function that can capture its surrounding scope.
    /// </summary>
    public class LambdaExpression : IExpression
    {
        public ParameterDefinition[] Parameters { get; }
        public IStatement[] Statements { get; }
        public SourceRange Location { get; }

        public LambdaExpression(ParameterDefinition[] parameters, IStatement[] statements, SourceRange location = null)
        {
            Parameters = parameters ?? System.Array.Empty<ParameterDefinition>();
            Statements = statements ?? System.Array.Empty<IStatement>();
            Location = location;
        }
    }
}