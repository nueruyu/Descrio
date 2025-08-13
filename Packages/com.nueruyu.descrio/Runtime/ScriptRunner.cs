using Descrio.Execution;
using Descrio.Parse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio
{
    public class ScriptRunner
    {
        private readonly IScriptParser _parser;
        private readonly IModuleProvider _moduleProvider;
        private readonly Dictionary<string, ICallable> _callables;
        private readonly Dictionary<string, Type> _types;

        public ScriptRunner(
            IScriptParser parser,
            IModuleProvider moduleProvider,
            Dictionary<string, ICallable> callables,
            Dictionary<string, Type> types = null)
        {
            _parser = parser;
            _moduleProvider = moduleProvider;
            _callables = callables ?? new Dictionary<string, ICallable>();
            _types = types ?? new Dictionary<string, Type>();
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
            var globalClasses = new ClassRegistry();

            // Register user-defined callables
            foreach (var (callableName, callable) in _callables)
            {
                globalCallables.Register(callableName, callable);
            }

            // Register built-in callables
            globalCallables.Register("all", new DelegateCallable(BuiltInFunctions.All));
            globalCallables.Register("any", new DelegateCallable(BuiltInFunctions.Any));

            // Register user-defined types
            foreach (var (typeName, type) in _types)
            {
                globalClasses.Register(typeName, type);
            }

            // Register built-in exception types
            globalClasses.Register("Error", typeof(Exception));
            globalClasses.Register("InvalidOperationError", typeof(InvalidOperationException));
            globalClasses.Register("ArgumentError", typeof(ArgumentException));
            globalClasses.Register("FileNotFoundError", typeof(FileNotFoundException));

            var entrypointModulePath = cwdPath.Resolve(entrypointPath);
            var context = new ExecutionContext(
                entrypointModulePath.GetDirectoryPath(),
                globalCallables,
                globalVariables,
                globalClasses,
                cancellationToken);

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

            var interpreter = new Interpreter(moduleContext);
            await interpreter.ExecuteAsync(module);

            return module;
        }
    }
}