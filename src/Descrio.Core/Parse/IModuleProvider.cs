using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Parse
{
    public interface IModuleProvider
    {
        Task<string> ReadContentAsync(ModulePath path, CancellationToken cancellationToken);
    }
}