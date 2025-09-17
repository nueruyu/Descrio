using Descrio.LspServer.Server;
using StreamJsonRpc;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new DescrioLanguageServer();
        var jsonRpc = new JsonRpc(
            Console.OpenStandardOutput(),
            Console.OpenStandardInput());

        jsonRpc.AddLocalRpcTarget(server);

        server.SetJsonRpc(jsonRpc);

        jsonRpc.StartListening();
        await jsonRpc.Completion;
    }
}