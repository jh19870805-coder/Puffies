# Project Instructions

This repository is maintained with Codex-first project notes.

Before changing code, Unity scenes, assets, configuration, or documented behavior:

1. Read `Documents/WORKFLOW.md`.
2. Read `Documents/CURRENT_TASK.md`.
3. Read `Documents/PROJECT_CONTEXT.md` for stable project facts relevant to the task.
4. Follow the user's latest instruction when it differs from the recorded next action.

After behavior-changing work, update `Documents/CURRENT_TASK.md` in the same turn with what changed, how it was checked, and the next useful action. If a stable project fact changes, update `Documents/PROJECT_CONTEXT.md`.

For read-only investigation, simple questions, or workspace housekeeping, do not update task notes unless the user asks.

## New Device Bootstrap

When this repository is opened on a different device, proactively verify the development environment before diagnosing C# project-load errors:

1. Check for a .NET 8 SDK with `dotnet --list-sdks`.
2. Check for the VS Code extensions `ms-dotnettools.csharp`, `ms-dotnettools.csdevkit`, and `visualstudiotoolsforunity.vstuc`.
3. If an item is missing, ask for the required system/network approval and install it for the user. Prefer `winget install --id Microsoft.DotNet.SDK.8 --exact` for the SDK when WinGet is available, and `code --install-extension <extension-id>` for VS Code extensions.
4. Reload the VS Code window after installing or changing C# tooling.

Do not edit Unity-generated `Assembly-CSharp*.csproj` files to work around editor project-load errors. Unity 2022.3 generates legacy project files that require the Microsoft Unity VS Code extension or the existing `dotnet.preferCSharpExtension` compatibility setting.

## Code Layout Preference

- Prefer a shallow, flat source layout with a small number of meaningful folders.
- Do not introduce deep directory hierarchies or many single-purpose folders unless the user explicitly requests them.
- Within one flat folder, related pure C# types may share a file when they belong to the same domain and the resulting file remains understandable.
- Preserve separate files for large or independently owned modules; reducing file count must not create a new catch-all utility file.
