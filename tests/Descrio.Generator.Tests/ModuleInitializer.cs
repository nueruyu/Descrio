using System.Runtime.CompilerServices;
using VerifyTests;

/// <summary>
/// Initializes Verify settings for the test assembly.
/// </summary>
public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // Tells Verify to use file-scoped namespaces for generated snapshot files,
        // matching modern C# conventions.
        VerifySourceGenerators.Initialize();
    }
}