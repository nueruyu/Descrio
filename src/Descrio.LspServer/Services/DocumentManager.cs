using Descrio.Parsing;
using Descrio.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Descrio.LspServer.Services
{
    public class DocumentManager
    {
        private record DocumentState(
            Uri Uri,
            string Text,
            Module? Ast,
            IReadOnlyList<SyntaxError> Errors);

        private readonly ConcurrentDictionary<Uri, DocumentState> _documents = new();
        private readonly IScriptParser _parser = new Descrio.Parsing.Yaml.YamlScriptParser();

        public IReadOnlyList<SyntaxError> OnDocumentOpened(Uri uri, string text)
        {
            var parseResult = _parser.Parse(text);
            var state = new DocumentState(uri, text, parseResult.Module, parseResult.Errors);
            _documents[uri] = state;
            return state.Errors;
        }

        public IReadOnlyList<SyntaxError> OnDocumentChanged(Uri uri, string newText)
        {
            var parseResult = _parser.Parse(newText);
            var state = new DocumentState(uri, newText, parseResult.Module, parseResult.Errors);
            _documents[uri] = state;
            return state.Errors;
        }

        public void OnDocumentClosed(Uri uri)
        {
            _documents.TryRemove(uri, out _);
        }

        public Module? GetAst(Uri uri)
        {
            return _documents.TryGetValue(uri, out var state) ? state.Ast : null;
        }
    }
}