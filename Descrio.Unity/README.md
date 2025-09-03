# README for the Descrio.Unity Project

## Purpose of This Project

This Unity project serves as the **development sandbox, sample showcase, and integration test environment** for the `Descrio` scripting engine.

The core logic of Descrio is developed as a separate .NET library located in the `/src/Descrio.Core` directory of the repository. This Unity project consumes the compiled `Descrio.Core.dll`.

## Getting Started for Contributors

If you have cloned the main `Descrio` repository, follow these steps to get started with this Unity project:

1.  **Build the Core DLLs:**
    Before opening this project for the first time, you **must** build the core library. Open a terminal at the repository root (one level above this folder) and run:
    ```bash
    dotnet build
    ```
    This command compiles `Descrio.Core` and automatically copies the required DLLs into the embedded package in `Packages/com.nueruyu.descrio/Runtime/`.

2.  **Open in Unity:**
    You can now open this `Descrio.Unity` folder as a project in the Unity Hub.

3.  **Explore the Samples:**
    The sample scenes, such as the `NpcTalk` example, are located in the `Assets/Samples` directory (this is a symbolic link to the package's `Samples~` folder). You can open these scenes to see Descrio in action and test your changes.

> **Note:** For complete documentation about Descrio's features and installation for end-users, please refer to the main `README.md` file at the root of the repository.