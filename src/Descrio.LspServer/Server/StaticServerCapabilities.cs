using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Descrio.LspServer.Server
{
    public static class StaticServerCapabilities
    {
        public static ServerCapabilities Default { get; } = new ServerCapabilities
        {
            TextDocumentSync = new TextDocumentSyncOptions
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Full,
                Save = new SaveOptions { IncludeText = true }
            },

            CompletionProvider = null, // new CompletionOptions { TriggerCharacters = new[] { "!", ".", ":" } },
            HoverProvider = false,
            DefinitionProvider = false,
            DocumentSymbolProvider = false,
            RenameProvider = false,
        };
    }
}