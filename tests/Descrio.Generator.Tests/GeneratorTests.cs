using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Descrio.Generator.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        [Test]
        public Task SimpleMappableClass_GeneratesCorrectly()
        {
            // 1. Input source code for the generator.
            // Placeholders for Descrio types are still needed for the test compilation.
            const string inputSource = @"
using Descrio;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Descrio
{
    public class Arguments : Dictionary<string, object> {}
    public interface ICallable { ValueTask<object?> CallAsync(Arguments args, Execution.ExecutionContext context); }
    public class ScriptRunner { public ScriptRunner AddCallable(string name, ICallable callable) => this; }

    namespace Execution
    {
        public class ExecutionContext {}
        namespace Converters
        {
            public interface ITypeConverter { object? Convert(object? source); }
            public static partial class TypeConverterRegistry
            {
                static partial void InitializeConverters();
                internal static void RegisterCore(Type t, ITypeConverter c) {}
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CallableAttribute : Attribute { public CallableAttribute(string? name = null) {} }
}

namespace MyGame.Data
{
    public class EffectData
    {
        public string? Name { get; set; }
        public int Power { get; set; }
    }
}

namespace MyGame.Actions
{
    using MyGame.Data;

    public class GameActions
    {
        [Callable]
        public void ApplyEffect(EffectData data) {}
    }
}
";

            var syntaxTree = CSharpSyntaxTree.ParseText(inputSource);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToList();

            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new CallableGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            var runResult = driver.RunGenerators(compilation).GetRunResult();

            return Verify(runResult);
        }
    }
}