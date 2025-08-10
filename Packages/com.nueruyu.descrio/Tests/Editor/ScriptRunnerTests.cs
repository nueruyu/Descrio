using Descrio.Parse.ModuleProviders;
using Descrio.Yaml;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.EditorTests
{
    public class ScriptRunnerTests
    {
        private class LogCallable : ICallable
        {
            public readonly List<object> ReceivedValues = new();

            public ValueTask<object> CallAsync(object[] args, ExecutionContext context)
            {
                ReceivedValues.AddRange(args);
                return new ValueTask<object>((object)null);
            }
        }

        private ScriptRunner CreateRunner(Dictionary<string, string> modules, out LogCallable logCallable)
        {
            logCallable = new LogCallable();
            var callables = new Dictionary<string, ICallable> { { "log", logCallable } };
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();
            return new ScriptRunner(parser, moduleProvider, callables);
        }

        [Test]
        public async Task ExecuteAsync_WhenSettingVariableAndCallingFunction_ShouldSucceed()
        {
            var mainScript = @"
statements:
  - !set
    name: my_message
    value: 'Hello'
  - !call
    name: log
    args:
      - ${my_message}
";
            var modules = new Dictionary<string, string> { { "/main.yaml", mainScript } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(1, logCallable.ReceivedValues.Count);
            Assert.AreEqual("Hello", logCallable.ReceivedValues[0]);
        }

        [Test]
        public async Task ExecuteAsync_WithModuleImport_ShouldLoadAndExecuteAllScripts()
        {
            var mainScript = @"
import:
  - ./utils/common.yaml
statements:
  - !call
    name: log
    args: ['from main']
  - !call
    name: util_func
";
            var utilScript = @"
statements:
  - !define
    name: util_func
    statements:
      - !call
        name: log
        args: ['from util_func']
";
            var modules = new Dictionary<string, string>
            {
                { "/scripts/main.yaml", mainScript },
                { "/scripts/utils/common.yaml", utilScript }
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/scripts/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(2, logCallable.ReceivedValues.Count);
            Assert.AreEqual("from main", logCallable.ReceivedValues[0]);
            Assert.AreEqual("from util_func", logCallable.ReceivedValues[1]);
        }

        [Test]
        public async Task ExecuteAsync_WhenConditionIsTrue_ShouldExecuteThenBlock()
        {
            var script = @"
statements:
  - !set
    name: execute_first_branch
    value: true
  - !when
    cases:
      - condition: ${execute_first_branch}
        then:
          - !call
            name: log
            args: ['Branch A']
      - then: # else case
          - !call
            name: log
            args: ['Branch B']
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(1, logCallable.ReceivedValues.Count);
            Assert.AreEqual("Branch A", logCallable.ReceivedValues[0]);
        }

        [Test]
        public async Task ExecuteAsync_WhenConditionIsFalse_ShouldExecuteElseBlock()
        {
            var script = @"
statements:
  - !set
    name: execute_first_branch
    value: false
  - !when
    cases:
      - condition: ${execute_first_branch}
        then:
          - !call
            name: log
            args: ['Branch A']
      - then: # else case
          - !call
            name: log
            args: ['Branch B']
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(1, logCallable.ReceivedValues.Count);
            Assert.AreEqual("Branch B", logCallable.ReceivedValues[0]);
        }

        [Test]
        public async Task ExecuteAsync_UserDefinedFunctionWithParams_ShouldUseCorrectValues()
        {
            var script = @"
statements:
  - !define
    name: greet
    params:
      name:
        type: string
      greeting:
        type: string
        default: 'Hello'
    statements:
      - !call
        name: log
        args:
          - ${greeting}
          - ${name}
  - !call
    name: greet
    args: ['World']
  - !call
    name: greet
    args: ['Galaxy', 'Hi']
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(4, logCallable.ReceivedValues.Count);
            Assert.AreEqual("Hello", logCallable.ReceivedValues[0]);
            Assert.AreEqual("World", logCallable.ReceivedValues[1]);
            Assert.AreEqual("Hi", logCallable.ReceivedValues[2]);
            Assert.AreEqual("Galaxy", logCallable.ReceivedValues[3]);
        }

        [Test]
        public async Task ExecuteAsync_VariableScope_LocalVariableShouldNotExistOutsideFunction()
        {
            var script = @"
statements:
  - !set
    name: outer_var
    value: 'outer'
  - !define
    name: my_func
    statements:
      - !set
        name: inner_var
        value: 'inner'
      - !call
        name: log
        args:
          - ${inner_var}
          - ${outer_var}
  - !call
    name: my_func
  - !call
    name: log
    args:
      - ${outer_var} # This should work
  - !call
    name: log
    args:
      - ${inner_var} # This should result in null
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(4, logCallable.ReceivedValues.Count);
            Assert.AreEqual("inner", logCallable.ReceivedValues[0]);
            Assert.AreEqual("outer", logCallable.ReceivedValues[1]);
            Assert.AreEqual("outer", logCallable.ReceivedValues[2]);
            Assert.IsNull(logCallable.ReceivedValues[3]);
        }

        [Test]
        public async Task ExecuteAsync_NestedModuleImport_ShouldExecuteInCorrectOrder()
        {
            var mainScript = @"
import: ['./moduleA.yaml']
statements:
  - !call
    name: log
    args: ['main']
  - !call
    name: func_a
";
            var moduleA = @"
import: ['./moduleB.yaml']
statements:
  - !define
    name: func_a
    statements:
      - !call
        name: log
        args: ['func_a']
      - !call
        name: func_b
";
            var moduleB = @"
statements:
  - !define
    name: func_b
    statements:
      - !call
        name: log
        args: ['func_b']
";
            var modules = new Dictionary<string, string>
            {
                { "/main.yaml", mainScript },
                { "/moduleA.yaml", moduleA },
                { "/moduleB.yaml", moduleB },
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(3, logCallable.ReceivedValues.Count);
            Assert.AreEqual("main", logCallable.ReceivedValues[0]);
            Assert.AreEqual("func_a", logCallable.ReceivedValues[1]);
            Assert.AreEqual("func_b", logCallable.ReceivedValues[2]);
        }

        [Test]
        public void ExecuteAsync_CallingUndefinedFunction_ShouldThrowException()
        {
            var script = @"
statements:
  - !call
    name: non_existent_function
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out _);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            });
        }

        [Test]
        public async Task ExecuteAsync_CircularImport_ShouldNotCauseInfiniteLoop()
        {
            var moduleA = @"
import: ['./moduleB.yaml']
statements:
  - !call
    name: log
    args: ['module_a']
";
            var moduleB = @"
import: ['./moduleA.yaml']
statements:
  - !call
    name: log
    args: ['module_b']
";
            var modules = new Dictionary<string, string>
            {
                { "/moduleA.yaml", moduleA },
                { "/moduleB.yaml", moduleB },
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/moduleA.yaml", "/", CancellationToken.None);

            // The test passes if it completes without a stack overflow.
            // The logs should show that each module is loaded only once.
            Assert.AreEqual(2, logCallable.ReceivedValues.Count);
            Assert.AreEqual("module_b", logCallable.ReceivedValues[0]);
            Assert.AreEqual("module_a", logCallable.ReceivedValues[1]);
        }
    }
}