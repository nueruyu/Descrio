using Microsoft.VisualStudio.LanguageServer.Protocol;
using DescrioError = Descrio.Parsing.SyntaxError;
using LspDiagnostic = Microsoft.VisualStudio.LanguageServer.Protocol.Diagnostic;
using LspRange = Microsoft.VisualStudio.LanguageServer.Protocol.Range;
using LspPosition = Microsoft.VisualStudio.LanguageServer.Protocol.Position;

namespace Descrio.LspServer.Utils
{
    public static class Converters
    {
        public static LspRange ToLspRange(Descrio.Data.SourceRange sourceRange)
        {
            var start = new LspPosition(sourceRange.StartLine - 1, sourceRange.StartColumn - 1);
            var end = new LspPosition(sourceRange.EndLine - 1, sourceRange.EndColumn - 1);
            return new LspRange { Start = start, End = end };
        }

        public static LspDiagnostic ToDiagnostic(DescrioError error)
        {
            return new LspDiagnostic
            {
                Range = ToLspRange(error.Location),
                Severity = DiagnosticSeverity.Error,
                Source = "descrio",
                Message = error.Message
            };
        }
    }
}