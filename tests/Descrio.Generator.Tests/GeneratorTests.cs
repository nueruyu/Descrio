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
            var syntaxTree = CSharpSyntaxTree.ParseText(inputSource);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Concat(
                [
                    typeof(Descrio.Attributes.CallableAttribute).Assembly
                ])
                .Distinct();

            var references = assemblies
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