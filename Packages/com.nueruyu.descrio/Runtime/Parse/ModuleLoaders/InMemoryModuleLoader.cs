using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Parse.ModuleLoaders
{
    public class InMemoryModuleLoader : IModuleLoader
    {
        private readonly Dictionary<ModulePath, Module> _modules = new();

        public InMemoryModuleLoader(Dictionary<string, Module> modules)
        {
            foreach (var kvp in modules)
            {
                _modules.Add(new ModulePath(kvp.Key), kvp.Value);
            }
        }

        public Task<Module> LoadAsync(ModulePath path, CancellationToken cancellationToken)
        {
            if (_modules.TryGetValue(path, out var module))
            {
                return Task.FromResult(module);
            }

            throw new FileNotFoundException($"Module with path '{path}' not found in memory.", path.ToString());
        }
    }
}