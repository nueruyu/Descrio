using Descrio.Execution;
using Descrio.Parse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio
{
    public class ScriptRunner
    {
        private readonly IScriptParser _parser;
        private readonly IModuleProvider _moduleProvider;
        private readonly Dictionary<string, ICallable> _callables = new();
        private readonly Dictionary<string, Type> _types = new();

        public ScriptRunner(IScriptParser parser, IModuleProvider moduleProvider)
        {
            _parser = parser;
            _moduleProvider = moduleProvider;
        }

        /// <summary>
        /// Registers a single callable function.
        /// </summary>
        /// <param name="name">The name of the callable as it will be used in the script.</param>
        /// <param name="callable">The ICallable instance.</param>
        public ScriptRunner AddCallable(string name, ICallable callable)
        {
            if (_callables.ContainsKey(name))
            {
                throw new InvalidOperationException($"A callable with the name '{name}' is already registered.");
            }
            _callables.Add(name, callable);
            return this;
        }

        /// <summary>
        /// Registers a custom type that can be used within scripts (e.g., for exceptions).
        /// </summary>
        /// <param name="nameInScript">The name of the type as it will be used in the script.</param>
        /// <param name="type">The C# type.</param>
        public ScriptRunner AddType(string nameInScript, Type type)
        {
            if (_types.ContainsKey(nameInScript))
            {
                throw new InvalidOperationException($"A type with the name '{nameInScript}' is already registered.");
            }
            _types.Add(nameInScript, type);
            return this;
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