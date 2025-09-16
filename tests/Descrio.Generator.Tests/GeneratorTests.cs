using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Descrio.Attributes;

namespace Descrio.Generator.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        [Test]
        public Task SimpleMappableClass_GeneratesCorrectly()
        {
            const string inputSource = @"
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
    using Descrio.Attributes;

    public class GameActions
    {
        [Callable]
        public void ApplyEffect(EffectData data) {}
    }
}
";
            //var callableType = typeof(Descrio.Attributes.CallableAttribute);

            var syntaxTree = CSharpSyntaxTree.ParseText(inputSource);

            var referenceAssemblies = new[]
            {
                typeof(object).Assembly, // mscorlib
                typeof(Enumerable).Assembly, // System.Linq
                typeof(CallableAttribute).Assembly, // Descrio.Core
            };
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