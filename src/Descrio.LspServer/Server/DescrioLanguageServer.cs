using Descrio.LspServer.Services;
using Descrio.LspServer.Utils;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;
using System;
using System.Threading.Tasks;

namespace Descrio.LspServer.Server
{
    public class DescrioLanguageServer
    {
        private JsonRpc _jsonRpc = null!;
        private readonly DocumentManager _documentManager = new DocumentManager();

        public void Initialize(JsonRpc jsonRpc)
        {
            _jsonRpc = jsonRpc;
        }

        [JsonRpcMethod(Methods.InitializeName)]
        public InitializeResult Initialize(InitializeParams @params)
        {
            return new InitializeResult
            {
                Capabilities = StaticServerCapabilities.Default
            };
        }

        [JsonRpcMethod(Methods.InitializedName, UseSingleObjectParameterDeserialization = true)]
        public async Task OnInitialized(InitializedParams @params)
        {
            await Task.CompletedTask;
        }

        [JsonRpcMethod(Methods.TextDocumentDidOpenName, UseSingleObjectParameterDeserialization = true)]
        public async Task OnDocumentOpened(DidOpenTextDocumentParams @params)
        {
            var uri = @params.TextDocument.Uri;
            var text = @params.TextDocument.Text;
            var errors = _documentManager.OnDocumentOpened(uri, text);
            await PublishDiagnosticsAsync(uri, errors);
        }

        [JsonRpcMethod(Methods.TextDocumentDidChangeName, UseSingleObjectParameterDeserialization = true)]
        public async Task OnDocumentChanged(DidChangeTextDocumentParams @params)
        {
            var uri = @params.TextDocument.Uri;
            var newText = @params.ContentChanges[0].Text;
            var errors = _documentManager.OnDocumentChanged(uri, newText);
            await PublishDiagnosticsAsync(uri, errors);
        }

        [JsonRpcMethod(Methods.TextDocumentDidCloseName, UseSingleObjectParameterDeserialization = true)]
        public async Task OnDocumentClosed(DidCloseTextDocumentParams @params)
        {
            _documentManager.OnDocumentClosed(@params.TextDocument.Uri);
            await PublishDiagnosticsAsync(@params.TextDocument.Uri, new List<Descrio.Parsing.SyntaxError>());
        }

        [JsonRpcMethod(Methods.ShutdownName)]
        public object? Shutdown()
        {
            return null;
        }

        [JsonRpcMethod(Methods.ExitName)]
        public void Exit()
        {
            Environment.Exit(0);
        }

        private async Task PublishDiagnosticsAsync(Uri uri, IReadOnlyList<Descrio.Parsing.SyntaxError> errors)
        {
            var diagnostics = errors.Select(Converters.ToDiagnostic).ToArray();
            await _jsonRpc.NotifyWithParameterObjectAsync(Methods.TextDocumentPublishDiagnosticsName, new PublishDiagnosticParams
            {
                Uri = uri,
                Diagnostics = diagnostics
            });
        }
    }
}