using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.Parse.ModuleLoaders
{
    /// <summary>
    /// Loads module content as a string from a provider and parses it using a script parser.
    /// </summary>
    public class StringModuleLoader : IModuleLoader
    {
        private readonly IScriptParser _parser;
        private readonly IModuleProvider _provider;

        public StringModuleLoader(IModuleProvider provider, IScriptParser parser)
        {
            _provider = provider;
            _parser = parser;
        }

        public async Task<Module> LoadAsync(ModulePath path, CancellationToken cancellationToken)
        {
            var scriptText = await _provider.ReadContentAsync(path, cancellationToken);

            var parseResult = _parser.Parse(scriptText);

            if (parseResult.Errors.Any())
            {
                var errorMessageBuilder = new StringBuilder();
                errorMessageBuilder.AppendLine($"Failed to parse module '{path}'. Found {parseResult.Errors.Count} error(s):");
                foreach (var error in parseResult.Errors)
                {
                    errorMessageBuilder.AppendLine($"- [Line {error.Location.StartLine}, Col {error.Location.StartColumn}] {error.Message}");
                }
                throw new InvalidDataException(errorMessageBuilder.ToString());
            }

            if (parseResult.Module == null)
            {
                throw new InvalidDataException($"Parsing module '{path}' resulted in a null module without any reported errors.");
            }

            return parseResult.Module;
        }
    }
}