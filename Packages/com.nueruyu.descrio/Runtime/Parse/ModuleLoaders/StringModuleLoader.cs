using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Parse.ModuleLoaders
{
    /// <summary>
    /// Loads module content as a string from a provider and parses it using a script parser.
    /// </summary>
    public class StringModuleLoader : IModuleLoader
    {
        private readonly IModuleProvider _provider;
        private readonly IScriptParser _parser;

        public StringModuleLoader(IModuleProvider provider, IScriptParser parser)
        {
            _provider = provider;
            _parser = parser;
        }

        public async Task<Module> LoadAsync(ModulePath path, CancellationToken cancellationToken)
        {
            var scriptText = await _provider.ReadContentAsync(path, cancellationToken);
            return _parser.Parse(scriptText);
        }
    }
}