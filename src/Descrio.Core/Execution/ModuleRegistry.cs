using System.Collections.Generic;

namespace Descrio.Execution
{
    public class ModuleRegistry
    {
        private readonly Dictionary<ModulePath, Module> _loadedModules = new();

        public bool TryGetModule(ModulePath absolutePath, out Module module)
        {
            return _loadedModules.TryGetValue(absolutePath, out module);
        }

        public void RegisterModule(ModulePath absolutePath, Module module)
        {
            _loadedModules[absolutePath] = module;
        }
    }
}