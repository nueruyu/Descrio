using Descrio.Parse.ModuleProviders;
using Descrio.Yaml;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Descrio.EditorTests
{
    public class ScriptRunnerTests
    {
        private class LogCallable : ICallable
        {
            public readonly List<object> ReceivedValues = new();

            public ValueTask<object> CallAsync(Arguments args, ExecutionContext context)
            {
                foreach (var kvp in args.OrderBy(kv => kv.Key))
                {
                    ReceivedValues.Add(kvp.Value);
                }
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
  - !let
    name: my_message
    value: 'Hello'
  - !run
    name: log
    args:
      text: !expr my_message
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
imports:
  - ./utils/common.yaml
statements:
  - !run
    name: log
    args:
      text: 'from main'
  - !run
    name: util_func
";
            var utilScript = @"
statements:
  - !function
    name: util_func
    statements:
      - !run
        name: log
        args:
          text: 'from util_func'
";
            var modules = new Dictionary<string, string>
            {
                { "/scripts/main.yaml", mainScript },
                { "/scripts/utils/common.yaml", utilScript }
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/scripts/main.yaml", "/", CancellationToken.None);

            Assert.AreEqual(2, logCallable.ReceivedValues.Count);
            Assert.Contains("from main", logCallable.ReceivedValues);
            Assert.Contains("from util_func", logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_WhenConditionIsTrue_ShouldExecuteThenBlock()
        {
            var script = @"
statements:
  - !let
    name: execute_first_branch
    value: true
  - !when
    cases:
      - condition: !expr execute_first_branch
        then:
          - !run
            name: log
            args:
              message: 'Branch A'
      - then: # else case
          - !run
            name: log
            args:
              message: 'Branch B'
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
  - !let
    name: execute_first_branch
    value: false
  - !when
    cases:
      - condition: !expr execute_first_branch
        then:
          - !run
            name: log
            args:
              message: 'Branch A'
      - then: # else case
          - !run
            name: log
            args:
              message: 'Branch B'
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
  - !function
    name: greet
    parameters:
      - name: name
        type: string
      - name: greeting
        type: string
        default: 'Hello'
    statements:
      - !run
        name: log
        args:
          p1_greeting: !expr greeting # Using different keys to test sorting
          p2_name: !expr name
  - !run
    name: greet
    args:
      name: 'World'
  - !run
    name: greet
    args:
      name: 'Galaxy'
      greeting: 'Hi'
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            var expected = new List<object> { "Hello", "World", "Hi", "Galaxy" };
            CollectionAssert.AreEqual(expected, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_VariableScope_LocalVariableShouldNotExistOutsideFunction()
        {
            var script = @"
statements:
  - !let
    name: outer_var
    value: 'outer'
  - !function
    name: my_func
    statements:
      - !let
        name: inner_var
        value: 'inner'
      - !run
        name: log
        args:
          inner: !expr inner_var
          outer: !expr outer_var
  - !run
    name: my_func
  - !run
    name: log
    args:
      outer_only: !expr outer_var
  - !run
    name: log
    args:
      inner_only: !expr inner_var
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            var expected = new List<object> { "inner", "outer", "outer", null };
            CollectionAssert.AreEqual(expected, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_NestedModuleImport_ShouldExecuteInCorrectOrder()
        {
            var mainScript = @"
imports: ['./moduleA.yaml']
statements:
  - !run
    name: log
    args: { text: 'main' }
  - !run
    name: func_a
";
            var moduleA = @"
imports: ['./moduleB.yaml']
statements:
  - !function
    name: func_a
    statements:
      - !run
        name: log
        args: { text: 'func_a' }
      - !run
        name: func_b
";
            var moduleB = @"
statements:
  - !function
    name: func_b
    statements:
      - !run
        name: log
        args: { text: 'func_b' }
";
            var modules = new Dictionary<string, string>
            {
                { "/main.yaml", mainScript },
                { "/moduleA.yaml", moduleA },
                { "/moduleB.yaml", moduleB },
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            var expected = new List<object> { "main", "func_a", "func_b" };
            CollectionAssert.AreEqual(expected, logCallable.ReceivedValues);
        }

        [Test]
        public void ExecuteAsync_CallingUndefinedFunction_ShouldThrowException()
        {
            var script = @"
statements:
  - !run
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
imports: ['./moduleB.yaml']
statements:
  - !run
    name: log
    args: { text: 'module_a' }
";
            var moduleB = @"
imports: ['./moduleA.yaml']
statements:
  - !run
    name: log
    args: { text: 'module_b' }
";
            var modules = new Dictionary<string, string>
            {
                { "/moduleA.yaml", moduleA },
                { "/moduleB.yaml", moduleB },
            };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/moduleA.yaml", "/", CancellationToken.None);

            var expected = new List<object> { "module_b", "module_a" };
            CollectionAssert.AreEqual(expected, logCallable.ReceivedValues);
        }
    }
}