using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Descrio.Modules;
using Descrio.Data;

namespace Descrio.Modules.ModuleProviders
{
    public class InMemoryModuleProvider : IModuleProvider
    {
        private readonly IReadOnlyDictionary<ModulePath, string> _modules;

        public InMemoryModuleProvider(IReadOnlyDictionary<string, string> modules) : this(
            modules.ToDictionary(
                kvp => new ModulePath(kvp.Key),
                kvp => kvp.Value))
        {
        }

        public InMemoryModuleProvider(IReadOnlyDictionary<ModulePath, string> modules)
        {
            _modules = new Dictionary<ModulePath, string>(modules);
        }

        public Task<string> ReadContentAsync(ModulePath path, CancellationToken cancellationToken)
        {
            if (_modules.TryGetValue(path, out var content))
            {
                return Task.FromResult(content);
            }

            throw new FileNotFoundException("Module not found in memory.", path.ToString());
        }
    }
}