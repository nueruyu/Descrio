using Descrio.LspServer.Server;
using StreamJsonRpc;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new DescrioLanguageServer();
        var jsonRpc = new JsonRpc(Console.OpenStandardInput(), Console.OpenStandardOutput());

        jsonRpc.AddLocalRpcTarget(server);

        server.Initialize(jsonRpc);

        jsonRpc.StartListening();
        await jsonRpc.Completion;
    }
}