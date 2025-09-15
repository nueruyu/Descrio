using Descrio.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Modules
{
    public interface IModuleProvider
    {
        Task<string> ReadContentAsync(ModulePath path, CancellationToken cancellationToken);
    }
}