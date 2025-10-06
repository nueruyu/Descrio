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

        public void IncreaseIndent() => _indent++;

        public void DecreaseIndent() => _indent--;

        /// <summary>
        /// Appends an opening brace and increases the indentation level.
        /// Returns a disposable object that will decrease the indentation when disposed.
        /// </summary>
        public IDisposable IndentedBlock()
        {
            AppendLine("{");
            _indent++;
            return new IndentHandler(this, b => b.CloseIndentedBlock());
        }

        public IDisposable Indent()
        {
            _indent++;
            return new IndentHandler(this, b => b.DecreaseIndent());
        }

        private void CloseIndentedBlock()
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
            private readonly Action<IndentedStringBuilder> _disposeAction;

            public IndentHandler(IndentedStringBuilder builder, Action<IndentedStringBuilder> disposeAction)
            {
                _builder = builder;
                _disposeAction = disposeAction;
            }

            public void Dispose() => _disposeAction(_builder);
        }
    }
}