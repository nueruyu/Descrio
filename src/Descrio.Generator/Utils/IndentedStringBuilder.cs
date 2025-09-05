using System;
using System.Text;

namespace Descrio.Generator.Utils
{
    /// <summary>
    /// A helper class for building indented source code strings.
    /// Manages indentation levels through a disposable pattern.
    /// </summary>
    internal class IndentedStringBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent = 0;
        private const string IndentString = "    ";

        public void AppendLine(string line)
        {
            for (int i = 0; i < _indent; i++)
                _sb.Append(IndentString);
            _sb.AppendLine(line);
        }

        public void AppendLine()
        {
            for (int i = 0; i < _indent; i++)
                _sb.Append(IndentString);
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends an opening brace and increases the indentation level.
        /// Returns a disposable object that will decrease the indentation when disposed.
        /// </summary>
        public IDisposable IndentedBlock()
        {
            AppendLine("{");
            _indent++;
            return new IndentHandler(this);
        }

        private void DecreaseIndent()
        {
            _indent--;
            AppendLine("}");
        }

        public override string ToString() => _sb.ToString();

        /// <summary>
        /// A private struct that handles the disposal action for an indented block.
        /// </summary>
        private readonly struct IndentHandler : IDisposable
        {
            private readonly IndentedStringBuilder _builder;

            public IndentHandler(IndentedStringBuilder builder) => _builder = builder;

            public void Dispose() => _builder.DecreaseIndent();
        }
    }
}