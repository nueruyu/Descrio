using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Parse
{
    /// <summary>
    /// Represents a component responsible for loading and parsing a module from a given path.
    /// </summary>
    public interface IModuleLoader
    {
        /// <summary>
        /// Asynchronously loads and parses a module from the specified path.
        /// </summary>
        /// <param name="path">The path to the module.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded and parsed <see cref="Module"/>.</returns>
        Task<Module> LoadAsync(ModulePath path, CancellationToken cancellationToken);
    }
}