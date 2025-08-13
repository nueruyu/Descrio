using NUnit.Framework;
using Descrio.Parse.ModuleProviders;
using Descrio.Yaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Descrio.Execution;
using ExecutionContext = Descrio.Execution.ExecutionContext;

namespace Descrio.EditorTests
{
    [TestFixture]
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

        private class AsyncLogCallable : ICallable
        {
            private readonly int _delayMs;
            private readonly object _returnValue;

            public AsyncLogCallable(int delayMs = 10, object returnValue = null)
            {
                _delayMs = delayMs;
                _returnValue = returnValue;
            }

            public async ValueTask<object> CallAsync(Arguments args, ExecutionContext context)
            {
                await Task.Delay(_delayMs, context.CancellationToken);
                return _returnValue ?? args.First().Value;
            }
        }

        private ScriptRunner CreateRunner(Dictionary<string, string> modules, out LogCallable logCallable)
        {
            logCallable = new LogCallable();
            var callables = new Dictionary<string, ICallable> { { "log", logCallable } };
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();
            var runner = new ScriptRunner(parser, moduleProvider);
            foreach (var (name, callable) in callables)
            {
                runner.AddCallable(name, callable);
            }
            return runner;
        }

        private ScriptRunner CreateRunner(Dictionary<string, string> modules, Dictionary<string, ICallable> callables = null, Dictionary<string, Type> types = null)
        {
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();
            var runner = new ScriptRunner(parser, moduleProvider);
            if (callables != null)
            {
                foreach (var (name, callable) in callables)
                {
                    runner.AddCallable(name, callable);
                }
            }
            if (types != null)
            {
                foreach (var (name, type) in types)
                {
                    runner.AddType(name, type);
                }
            }
            return runner;
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
          val: !expr inner_var
  - !run
    name: my_func
  - !run
    name: log
    args:
      val: !expr outer_var
  - !try
    statements:
      - !run
        name: log
        args:
          val: !expr inner_var
    catch:
      - name: InvalidOperationError
        then:
          - !run
            name: log
            args:
              val: 'ScopeCheckPassed'
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var runner = CreateRunner(modules, out var logCallable);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            var expected = new List<object> { "inner", "outer", "ScopeCheckPassed" };
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

        [Test]
        public async Task ExecuteAsync_VarAndAssign_ShouldUpdateMutableVariable()
        {
            var script = @"
statements:
  - !var
    name: counter
    value: 10
  - !assign
    name: counter
    value: !expr counter + 1
  - !run
    name: log
    args: { val: !expr counter }
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            Assert.AreEqual(11L, logCallable.ReceivedValues.Single());
        }

        [Test]
        public void ExecuteAsync_AssignToLet_ShouldThrowException()
        {
            var script = @"
statements:
  - !let
    name: immutable_var
    value: 10
  - !assign
    name: immutable_var
    value: 20
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out _);
            Assert.ThrowsAsync<InvalidOperationException>(() => runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None));
        }

        [Test]
        public async Task ExecuteAsync_ForLoop_ShouldIterateOverCollection()
        {
            var script = @"
statements:
  - !let
    name: items
    value: ['A', 'B', 'C']
  - !for
    in: !expr items
    as: item
    statements:
      - !run
        name: log
        args: { val: !expr item }
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_BreakAndContinueInLoop_ShouldAlterFlow()
        {
            var script = @"
statements:
  - !for
    in: [1, 2, 3, 4, 5, 6]
    as: n
    statements:
      - !when
        cases:
          - condition: !expr n == 2
            then:
              - !continue
          - condition: !expr n == 5
            then:
              - !break
      - !run
        name: log
        args: { val: !expr n }
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { 1L, 3L, 4L }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_Return_ShouldExitFunctionWithValue()
        {
            var script = @"
statements:
  - !function
    name: get_double
    parameters: [{name: x}]
    statements:
      - !return
        value: !expr x * 2
      - !run # This should not be executed
        name: log
        args: { val: 'unreachable' }
  - !let
    name: result
    value: !run
      name: get_double
      args: { x: 21 }
  - !run
    name: log
    args: { val: !expr result }
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            Assert.AreEqual(42L, logCallable.ReceivedValues.Single());
        }

        [Test]
        public async Task ExecuteAsync_DispatchAndAll_ShouldExecuteInParallel()
        {
            var script = @"
statements:
  - !let
    name: tasks
    value:
      - !dispatch
        name: task_fast
        args: { val: 'fast' }
      - !dispatch
        name: task_slow
        args: { val: 'slow' }
  - !let
    name: results
    value: !run
      name: all
      args: { tasks: !expr tasks }
  - !for
    in: !expr results
    as: r
    statements:
      - !run
        name: log
        args: { val: !expr r }
";
            var logCallable = new LogCallable();
            var callables = new Dictionary<string, ICallable> {
                { "log", logCallable },
                { "task_fast", new AsyncLogCallable(10, "fast_result") },
                { "task_slow", new AsyncLogCallable(20, "slow_result") },
            };
            var runner = CreateRunner(new() { { "/main.yaml", script } }, callables);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            CollectionAssert.AreEquivalent(new[] { "fast_result", "slow_result" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_Match_ShouldExecuteCorrectCase()
        {
            var script = @"
statements:
  - !let
    name: choice
    value: 'Defend'
  - !match
    value: !expr choice
    cases:
      - case: 'Attack'
        then:
          - !run
            name: log
            args:
              val: 'Attacked'
      - case: 'Defend'
        then:
          - !run
            name: log
            args:
              val: 'Defended'
  - !run
    name: log
    args:
      val: 'Finished'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "Defended", "Finished" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_Match_ShouldExecuteDefaultWhenNoCaseMatches()
        {
            var script = @"
statements:
  - !let
    name: choice
    value: 'Flee'
  - !match
    value: !expr choice
    cases:
      - case: 'Attack'
        then:
          - !run
            name: log
            args:
              val: 'Attacked'
      - case: 'Defend'
        then:
          - !run
            name: log
            args:
              val: 'Defended'
    default:
      - !run
        name: log
        args:
          val: 'DefaultAction'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "DefaultAction" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_TryCatch_ShouldCatchSpecificException()
        {
            var script = @"
statements:
  - !try
    statements:
      - !run
        name: log
        args:
          val: 'TryEnter'
      - !throw
        name: InvalidOperationError
        args:
          message: 'Something went wrong'
      - !run
        name: log
        args:
          val: 'ShouldNotRun'
    catch:
      - name: InvalidOperationError
        as: err
        then:
          - !run
            name: log
            args:
              val: !expr '""Caught: ${err.Message}""'
    finally:
      - !run
        name: log
        args:
          val: 'Finally'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            var expected = new[] { "TryEnter", "Caught: Something went wrong", "Finally" };
            CollectionAssert.AreEqual(expected, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_TryCatch_ShouldExecuteFinallyOnSuccess()
        {
            var script = @"
statements:
  - !try
    statements:
      - !run
        name: log
        args:
          val: 'Success'
    catch:
      - name: Error
        then:
          - !run
            name: log
            args:
              val: 'ShouldNotBeCaught'
    finally:
      - !run
        name: log
        args:
          val: 'Finally'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "Success", "Finally" }, logCallable.ReceivedValues);
        }

        [Test]
        public void ExecuteAsync_TryCatch_ShouldRethrowUncaughtException()
        {
            var script = @"
statements:
  - !try
    statements:
      - !throw
        name: ArgumentError
    catch:
      - name: InvalidOperationError
        then:
          - !run
            name: log
            args:
              val: 'WrongCatch'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out _);
            Assert.ThrowsAsync<ArgumentException>(() => runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None));
        }

        [Test]
        public async Task ExecuteAsync_TryCatch_ShouldCatchWithDefaultClause()
        {
            var script = @"
statements:
  - !try
    statements:
      - !throw
        name: ArgumentError
    catch:
      - name: InvalidOperationError
        then:
          - !run
            name: log
            args:
              val: 'Wrong'
      - then:
          - !run
            name: log
            args:
              val: 'DefaultCatch'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "DefaultCatch" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_Assert_ShouldSucceedWhenConditionIsTrue()
        {
            var script = @"
statements:
  - !assert
    condition: !expr 1 == 1
    message: 'This should not fail'
  - !run
    name: log
    args:
      val: 'AssertionPassed'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "AssertionPassed" }, logCallable.ReceivedValues);
        }

        [Test]
        public void ExecuteAsync_Assert_ShouldThrowWhenConditionIsFalse()
        {
            var script = @"
statements:
  - !assert
    condition: !expr 1 == 2
    message: 'Math is broken'
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out _);
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None));
            StringAssert.Contains("Math is broken", ex.Message);
        }

        [Test]
        public async Task ExecuteAsync_MemberAccess_ShouldAccessObjectProperty()
        {
            var script = @"
statements:
  - !try
    statements:
      - !throw
        name: InvalidOperationError
        args:
          message: 'Custom error message'
    catch:
      - name: InvalidOperationError
        as: e
        then:
          - !run
            name: log
            args:
              val: !expr e.Message
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new[] { "Custom error message" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_MemberAccess_ShouldAccessDictionaryValue()
        {
            var script = @"
statements:
  - !let
    name: player
    value:
      name: 'Hero'
      level: 10
  - !run
    name: log
    args:
      val: !expr player.name
  - !run
    name: log
    args:
      val: !expr player.level
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new object[] { "Hero", 10L }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_MemberAccess_ShouldAccessExceptionData()
        {
            var script = @"
statements:
  - !try
    statements:
      - !throw
        name: Error
        args:
          code: 404
          resource: 'player_data'
    catch:
      - name: Error
        as: e
        then:
          - !run
            name: log
            args:
              val: !expr e.Data.code
          - !run
            name: log
            args:
              val: !expr e.Data.resource
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new object[] { 404L, "player_data" }, logCallable.ReceivedValues);
        }

        [Test]
        public async Task ExecuteAsync_MemberAccess_ShouldReturnNullForNonExistentProperty()
        {
            var script = @"
statements:
  - !let
    name: obj
    value: { name: 'test' }
  - !run
    name: log
    args:
      val: !expr obj.non_existent
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out var logCallable);
            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);
            CollectionAssert.AreEqual(new object[] { null }, logCallable.ReceivedValues);
        }

        [Test]
        public void ExecuteAsync_MemberAccess_ShouldThrowOnNullObject()
        {
            var script = @"
statements:
  - !let
    name: obj
    value: null
  - !run
    name: log
    args:
      val: !expr obj.property
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out _);
            Assert.ThrowsAsync<NullReferenceException>(() => runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None));
        }

        [Test]
        public void ExecuteAsync_AccessingUndefinedVariable_ShouldThrowException()
        {
            var script = @"
statements:
  - !run
    name: log
    args:
      val: !expr undefined_variable
";
            var runner = CreateRunner(new() { { "/main.yaml", script } }, out _);
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None));
            StringAssert.Contains("Variable 'undefined_variable' is not defined", ex.Message);
        }

        public class AttributedFunctions
        {
            public readonly List<object> LogHistory = new();

            [Callable("log_attr")]
            public void Log(object val) => LogHistory.Add(val);

            [Callable]
            public int Add(int a, int b) => a + b;

            [Callable]
            public string Greet(string name, string greeting = "Hello") => $"{greeting}, {name}!";

            [Callable("get_message")]
            public ValueTask<string> GetMessageAsync(CancellationToken ct)
            {
                return new ValueTask<string>("Async message");
            }
        }

        [Test]
        public async Task ExecuteAsync_WithAttributedCallables_ShouldSucceed()
        {
            var script = @"
statements:
  - !let
    name: result
    value: !run
      name: Add
      args: { a: 5, b: 10 }
  - !run
    name: log_attr
    args:
      val: !expr result
  - !let
    name: greeting
    value: !run
      name: Greet
      args:
        name: 'Attribute'
        greeting: 'Welcome'
  - !run
    name: log_attr
    args:
      val: !expr greeting
  - !let
    name: async_msg
    value: !run
      name: get_message
  - !run
    name: log_attr
    args:
      val: !expr async_msg
";
            var modules = new Dictionary<string, string> { { "/main.yaml", script } };
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();
            var funcs = new AttributedFunctions();

            var runner = new ScriptRunner(parser, moduleProvider)
                .AddCallables(funcs);

            await runner.ExecuteAsync("/main.yaml", "/", CancellationToken.None);

            CollectionAssert.AreEqual(new object[] { 15L, "Welcome, Attribute!", "Async message" }, funcs.LogHistory);
        }

        [Test]
        public void ExecuteAsync_AddCallablesWithDuplicateName_ShouldThrowException()
        {
            var parser = new YamlScriptParser();
            var provider = new InMemoryModuleProvider(new Dictionary<string, string>());
            var runner = new ScriptRunner(parser, provider);

            var funcs1 = new { };
            var funcs2 = new { };

            runner.AddCallable("my_func", new DelegateCallable((Arguments _, CancellationToken _) => new ValueTask<object>()));

            Assert.Throws<InvalidOperationException>(() =>
            {
                runner.AddCallable("my_func", new DelegateCallable((Arguments _, CancellationToken _) => new ValueTask<object>()));
            });
        }
    }
}