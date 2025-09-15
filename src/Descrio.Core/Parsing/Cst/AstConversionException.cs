using Descrio.Data;
using System;

namespace Descrio.Parsing.Cst
{
    /// <summary>
    /// An exception thrown during the conversion from a Concrete Syntax Tree (CST) to an Abstract Syntax Tree (AST).
    /// </summary>
    internal class AstConversionException : Exception
    {
        public SourceRange Location { get; }

        public AstConversionException(string message, SourceRange location) : base(message)
        {
            Location = location;
        }
    }
}