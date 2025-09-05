using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Descrio.Generator.Callable
{
    /// <summary>
    /// Receives syntax nodes, filtering for methods with attributes
    /// to pass to the generator for semantic analysis.
    /// </summary>
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Collect methods with any attribute lists for performance.
            // Semantic analysis will be done later in the generator.
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax &&
                methodDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateMethods.Add(methodDeclarationSyntax);
            }
        }
    }
}