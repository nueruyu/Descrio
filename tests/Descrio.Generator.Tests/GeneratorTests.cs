using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Descrio.Generator.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        private readonly IEnumerable<MetadataReference> _references;

        public GeneratorTests()
        {
            // Constructor to set up the necessary assembly references once for all tests.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Concat([typeof(Descrio.Attributes.CallableAttribute).Assembly])
                .Distinct();

            _references = assemblies
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToList();
        }

        private Task TestGenerator(string inputSource)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(inputSource);

            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: [syntaxTree],
                references: _references);

            var generator = new CallableGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            var runResult = driver.RunGenerators(compilation).GetRunResult();

            return Verify(runResult);
        }

        [Test]
        public Task SimpleMappableClass_GeneratesCorrectly()
        {
            const string inputSource = @"
using Descrio.Attributes;

namespace MyGame.Data
{
    public class EffectData
    {
        public string? Name { get; set; }
        public int Power { get; set; }
        public EffectData() {}
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
            return TestGenerator(inputSource);
        }

        [Test]
        public Task NestedAndComplexTypes_GeneratesAllConverters()
        {
            const string inputSource = @"
using Descrio.Attributes;
using System.Collections.Generic;

namespace MyGame.Data
{
    public enum TargetType { Player, Enemy, All }

    public class DamageInfo
    {
        public int Amount { get; set; }
        public bool IsCritical { get; set; }
        public DamageInfo() {}
    }

    public class SkillData
    {
        public string? SkillName { get; set; }
        public TargetType Target { get; set; }
        public List<DamageInfo>? DamageEffects { get; set; }
        public DamageInfo[]? BonusEffects { get; set; }
        public SkillData() {}
    }
}

namespace MyGame.Actions
{
    using MyGame.Data;
    public class CharacterActions
    {
        [Callable]
        public void ExecuteSkill(SkillData skill) {}
    }
}
";
            return TestGenerator(inputSource);
        }

        [Test]
        public Task NoParameterlessConstructor_DoesNotGenerateConverter()
        {
            const string inputSource = @"
using Descrio.Attributes;

namespace MyGame.Data
{
    // This class is missing a public parameterless constructor.
    public class InvalidData
    {
        public string? Message { get; set; }
        public InvalidData(string message) { Message = message; }
    }
}

namespace MyGame.Actions
{
    using MyGame.Data;
    public class BadActions
    {
        [Callable]
        public void Process(InvalidData data) {}
    }
}
";
            // We expect the generator to run without errors, but it should not produce a converter for InvalidData.
            return TestGenerator(inputSource);
        }

        [Test]
        public Task PrivateMembers_AreIgnored()
        {
            const string inputSource = @"
using Descrio.Attributes;

namespace MyGame.Data
{
    public class DataWithPrivateMembers
    {
        public string? PublicData { get; set; }
        private int PrivateField;
        public int PropertyWithPrivateSetter { get; private set; }
        public readonly int ReadOnlyField = 10;
        public DataWithPrivateMembers() {}
    }
}

namespace MyGame.Actions
{
    using MyGame.Data;
    public class SomeActions
    {
        [Callable]
        public void HandleData(DataWithPrivateMembers data) {}
    }
}
";
            // The generated converter should only attempt to map "PublicData".
            return TestGenerator(inputSource);
        }

        [Test]
        public Task StructParameter_IsMappable()
        {
            const string inputSource = @"
using Descrio.Attributes;

namespace MyGame.Data
{
    // Structs are also supported as they have an implicit parameterless constructor.
    public struct Vector2D
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}

namespace MyGame.Actions
{
    using MyGame.Data;
    public class MovementActions
    {
        [Callable]
        public void MoveTo(Vector2D position) {}
    }
}
";
            // Your generator will need to be updated to handle structs.
            // Parser.cs -> GetMappableMembers -> `type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct`
            return TestGenerator(inputSource);
        }
    }
}