using Descrio.Abstractions;
using Descrio.Execution;
using Descrio.Execution.Callables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Descrio.Execution.ExecutionContext;
using Descrio.Modules;
using Descrio.Execution.Registries;
using Descrio.Data;

namespace Descrio
{
    public class ScriptRunner
    {
        private class ModuleExecutionResult
        {
            public CallableRegistry Callables { get; }
            public ClassRegistry Classes { get; }

            public ModuleExecutionResult(CallableRegistry callables, ClassRegistry classes)
            {
                Callables = callables;
                Classes = classes;
            }
        }

        private readonly IModuleLoader _moduleLoader;
        private readonly Dictionary<string, ICallable> _callables = new();
        private readonly Dictionary<string, Type> _types = new();

        public ScriptRunner(IModuleLoader moduleLoader)
        {
            _moduleLoader = moduleLoader;
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
            var executedModules = new Dictionary<ModulePath, ModuleExecutionResult>();
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

            var globalContext = new ExecutionContext(
                cwdPath,
                globalCallables,
                globalVariables,
                globalClasses,
                cancellationToken);

            var entrypointModulePath = cwdPath.Resolve(entrypointPath);
            await LoadAndExecuteModuleAsync(entrypointModulePath, globalContext, executedModules);
        }

        private async Task<ModuleExecutionResult> LoadAndExecuteModuleAsync(
            ModulePath path,
            ExecutionContext globalContext,
            Dictionary<ModulePath, ModuleExecutionResult> executedModules)
        {
            globalContext.CancellationToken.ThrowIfCancellationRequested();

            if (executedModules.TryGetValue(path, out var existingResult))
            {
                return existingResult;
            }

            var module = await _moduleLoader.LoadAsync(path, globalContext.CancellationToken);

            var moduleCallableRegistry = new CallableRegistry(globalContext.Callables);
            var moduleClassRegistry = new ClassRegistry(globalContext.Classes);
            var moduleVariableRegistry = new VariableRegistry(globalContext.Variables);

            var moduleExecutionContext = new ExecutionContext(
                path.GetDirectoryPath(),
                moduleCallableRegistry,
                moduleVariableRegistry,
                moduleClassRegistry,
                globalContext.CancellationToken);

            foreach (var importPath in module.ImportPaths)
            {
                var absoluteImportPath = moduleExecutionContext.CurrentDirectory.Resolve(importPath);
                var dependencyResult = await LoadAndExecuteModuleAsync(absoluteImportPath, globalContext, executedModules);

                foreach (var (name, callable) in dependencyResult.Callables.GetDefinedCallables())
                {
                    moduleCallableRegistry.Register(name, callable);
                }

                foreach (var (name, type) in dependencyResult.Classes.GetDefinedClasses())
                {
                    moduleClassRegistry.Register(name, type);
                }
            }

            var interpreter = new Interpreter(moduleExecutionContext);
            await interpreter.ExecuteAsync(module);

            var result = new ModuleExecutionResult(moduleCallableRegistry, moduleClassRegistry);
            executedModules[path] = result;
            return result;
        }
    }
}