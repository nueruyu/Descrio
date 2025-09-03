# Descrio

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Descrio** is a simple, YAML-based scripting interpreter designed to be embedded in Unity projects. It allows you to write and dynamically execute logic for event sequences, NPC dialogues, quest progression, and more using human-readable YAML files, without needing to recompile C# code.

### Why use Descrio?

*   **Rapid Iteration**: Edit game logic in text files and see changes instantly without waiting for Unity to recompile. This allows planners and writers to adjust logic directly.
*   **Readable Logic**: Leverage YAML's structure to write declarative and intuitive event flows, making complex scenarios easy to understand.
*   **Extensible**: Easily expose your own C# methods to the script by simply adding a `[Callable]` attribute. This provides a safe way to grant script access to project-specific functionality.
*   **Dynamic Content Management**: Since scripts are external files, they can be easily managed, delivered, and updated at runtime using AssetBundles or other methods.

## Features

*   **YAML-based Scripting**: Use intuitive custom tags like `!let`, `!run`, and `!when`.
*   **C# Interoperability**: Expose C# methods to your scripts with the `[Callable]` attribute.
*   **Variables and Scopes**: Define immutable (`!let`) and mutable (`!var`) variables.
*   **Control Flow**: Full support for conditional branches (`!when`, `!match`) and loops (`!for`, `!while`).
*   **Functions**: Define and call functions within your scripts (`!function`).
*   **Async Support**: Call asynchronous C# methods (`!dispatch`) and await them with `!run all` / `!run any`.
*   **Exception Handling**: Safe error handling with `!try`, `!catch`, `!finally`, and `!throw`.
*   **Module System**: Split and reuse scripts using the `imports` keyword.
*   **Expression Evaluation**: Evaluate mathematical, comparison, and logical operations within an `!expr` tag (e.g., `health > 50`, `name == 'hero'`).

## Installation

1.  In the Unity Editor, open the Package Manager window (`Window > Package Manager`).
2.  Click the `+` button and select `Add package from git URL...`.
3.  Enter the following URL and click Add:
   ```
   https://github.com/nueruyu/Descrio.git?path=Descrio.Unity/Packages/com.nueruyu.descrio
   ```

## Quick Start

### 1. Create a YAML Script
Create a YAML file, such as `MyDialogue.yaml`, in your project assets.

**`MyDialogue.yaml`:**
```yaml
statements:
  # Calls the 'ShowMessage' function implemented in C#
  - !run
    name: ShowMessage
    args:
      text: "Welcome, adventurer."

  # Calls the C# 'ShowChoices' function and stores the return value in the 'choice' variable
  - !let
    name: choice
    value: !run
      name: ShowChoices
      args:
        choices: ["Introduce myself", "Walk away"]

  # Branch the flow based on the value of the 'choice' variable
  - !match
    value: !expr choice
    cases:
      - case: 0 # First choice
        then:
          - !run
            name: ShowMessage
            args:
              text: "I am the guide of this world."
      - case: 1 # Second choice
        then:
          - !run
            name: ShowMessage
            args:
              text: "Farewell."
    default:
      - !run
        name: ShowMessage
        args:
          text: "......"
```

### 2. Run the Script from C#
Create a C# class to execute the script.

**`DialogueManager.cs`:**
```csharp
using UnityEngine;
using Descrio;
using Descrio.Yaml;
using Descrio.Parse.ModuleProviders;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    public TextAsset entrypointScript; // Set the YAML file from the Unity Editor

    void Start()
    {
        ExecuteDialogueAsync();
    }

    private async void ExecuteDialogueAsync()
    {
        // A provider to read the script from memory
        var modules = new Dictionary<string, string> { { "/main.yaml", entrypointScript.text } };
        var moduleProvider = new InMemoryModuleProvider(modules);
        
        // The YAML parser
        var parser = new YamlScriptParser();

        var moduleLoader = new StringModuleLoader(moduleProvider, parser);

        // Initialize the ScriptRunner and register the Callable methods from this class
        var runner = new ScriptRunner(moduleLoader)
            .AddCallables(this);

        // Execute the script
        try
        {
            await runner.ExecuteAsync("/main.yaml", "/");
            Debug.Log("Dialogue finished.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Descrio script execution failed: {ex}");
        }
    }

    // --- Methods to be called from the script ---

    [Callable]
    public void ShowMessage(string text)
    {
        // Your implementation to show a message in the UI
        Debug.Log($"Message: {text}");
    }

    [Callable]
    public async Task<int> ShowChoices(List<object> choices)
    {
        // Your implementation to show choices and wait for user input
        Debug.Log("Please make a choice:");
        for (int i = 0; i < choices.Count; i++)
        {
            Debug.Log($"{i}: {choices[i]}");
        }
        
        // For this example, we'll automatically select option 0 after a short delay.
        await Task.Delay(1000); // Wait for 1 second
        Debug.Log("Choice 0 was selected.");
        return 0;
    }
}
```

## YAML Syntax Reference

Descrio uses custom YAML tags to represent commands.

| Tag | Description | Example |
| :--- | :--- | :--- |
| `!let` | Defines an immutable variable. | `!let { name: health, value: 100 }` |
| `!var` | Defines a mutable variable. | `!var { name: score, value: 0 }` |
| `!assign`| Reassigns a new value to a mutable variable. | `!assign { name: score, value: !expr score + 10 }` |
| `!run` | Executes a function synchronously and waits for the result. | `!run { name: Func, args: { key: val } }` |
| `!dispatch` | Calls an asynchronous C# function. Immediately returns a Task. | `!dispatch { name: AsyncFunc }` |
| `!when` | Builds a conditional branch (`if-elseif-else`). | See Quick Start example. |
| `!match` | Builds a branch based on value equality (`switch-case`). | See Quick Start example. |
| `!for` | Iterates over a list or an array. | `!for { in: !expr items, as: item, statements: [...] }` |
| `!while` | Continues a loop as long as a condition is true. | `!while { condition: !expr IsActive, statements: [...] }`|
| `!return`| Returns a value from a function. | `!return { value: 42 }` |
| `!break` | Breaks out of a loop. | `!break` |
| `!continue`| Skips to the next iteration of a loop. | `!continue` |
| `!try` | Defines an exception handling block. | `!try { statements: [...], catch: [...], finally: [...] }` |
| `!throw` | Throws an exception. | `!throw { name: InvalidOperationError, args: { ... } }` |
| `!expr` | Evaluates a string as an expression. | `!expr "score > 100 && is_clear == false"` |

## For Contributors & Development Setup

This project has been structured to separate the core, platform-agnostic logic from the Unity-specific environment. If you wish to contribute, please familiarize yourself with the following structure and workflow.

### Project Structure
```
/
├── src/
│   ├── Descrio.Core/        # The core .NET Standard library (interpreter, parser, etc.)
│   └── Descrio.Generator/   # The source generator for the [Callable] attribute.
├── tests/
│   └── Descrio.Core.Tests/  # NUnit tests for the core library.
└── Descrio.Unity/
    ├── Packages/
    │   └── com.nueruyu.descrio/ # The UPM package source (consumes DLLs from Descrio.Core).
    └── ...                  # The Unity project for samples and integration testing.
```

### Development Workflow

1.  **Clone the repository.**
2.  **Build the Core Library:** Open a terminal at the repository root and run `dotnet build`.
    *   This compiles the `Descrio.Core` project.
    *   A post-build step will automatically copy the necessary DLLs (`Descrio.Core.dll` and `YamlDotNet.dll`) into the `Descrio.Unity/Packages/com.nueruyu.descrio/Runtime/` directory.
3.  **Open in Unity:** Open the `Descrio.Unity` folder as a project in Unity Hub. Any changes you make to the core library will be reflected in this project after you rebuild.
4.  **Run Tests:** To run the fast, non-Unity tests, use `dotnet test` from the root directory.

## License

This project is released under the [MIT License](LICENSE).
