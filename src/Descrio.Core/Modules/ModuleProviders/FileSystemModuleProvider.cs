using Descrio.Data;
using Descrio.Modules;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Modules.ModuleProviders
{
    public class FileSystemModuleProvider : IModuleProvider
    {
        public Task<string> ReadContentAsync(ModulePath path, CancellationToken cancellationToken)
        {
            var pathString = path.ToString();
            if (!File.Exists(pathString))
            {
                throw new FileNotFoundException("Module file not found.", pathString);
            }

            return File.ReadAllTextAsync(pathString, cancellationToken);
        }
    }
}