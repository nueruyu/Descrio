using Descrio.Execution;
using Descrio.Parse;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio
{
    public class ScriptRunner
    {
        private readonly IScriptParser _parser;
        private readonly IModuleProvider _moduleProvider;
        private readonly Dictionary<string, ICallable> _callables;

        public ScriptRunner(
            IScriptParser parser,
            IModuleProvider moduleProvider,
            Dictionary<string, ICallable> callables)
        {
            _parser = parser;
            _moduleProvider = moduleProvider;
            _callables = callables;
        }

        public async Task ExecuteAsync(
            string entrypointPath,
            string currentWorkingDirectory,
            CancellationToken cancellationToken = default)
        {
            var loadedModules = new ModuleRegistry();
            var cwdPath = new ModulePath(currentWorkingDirectory);

            var globalCallables = new CallableRegistry();
            var globalVariables = new VariableRegistry();

            foreach (var (callableName, callable) in _callables)
            {
                globalCallables.Register(callableName, callable);
            }

            var entrypointModulePath = cwdPath.Resolve(entrypointPath);
            var context = new ExecutionContext(
                entrypointModulePath.GetDirectoryPath(),
                globalCallables,
                globalVariables,
                cancellationToken: cancellationToken);

            await LoadModuleAndDependenciesAsync(entrypointModulePath, context, loadedModules);
        }

        private async Task<Module> LoadModuleAndDependenciesAsync(
            ModulePath path,
            ExecutionContext parentContext,
            ModuleRegistry loadedModules)
        {
            parentContext.CancellationToken.ThrowIfCancellationRequested();

            if (loadedModules.TryGetModule(path, out var existingModule))
            {
                return existingModule;
            }

            var scriptText = await _moduleProvider.ReadContentAsync(path, parentContext.CancellationToken);
            var module = _parser.Parse(scriptText);

            loadedModules.RegisterModule(path, module);

            var moduleContext = parentContext.CreateForNewPath(path.GetDirectoryPath());

            foreach (var importPath in module.ImportPaths)
            {
                var absoluteImportPath = moduleContext.CurrentDirectory.Resolve(importPath);
                await LoadModuleAndDependenciesAsync(absoluteImportPath, moduleContext, loadedModules);
            }

            var visitor = new ExecutionVisitor(moduleContext);
            await module.AcceptAsync(visitor);

            return module;
        }
    }
}