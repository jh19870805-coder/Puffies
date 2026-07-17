# Current Task

- Task: Record cross-device development environment bootstrap
- Status: Completed
- Updated At: 2026-07-17

## User Intent

- When Puffies is opened on another device, have Codex check and help install the required VS Code Unity/C# tooling and .NET SDK.

## Working Notes

- New-device checks are now part of the repository-level agent instructions.
- VS Code recommends the required C#, C# Dev Kit, and Microsoft Unity extensions when the workspace opens.
- The .NET 8 SDK remains a per-device system installation and requires system/network authorization when missing.
- Unity-generated `Assembly-CSharp*.csproj` files must not be edited as a compatibility workaround.

## Files Changed

- `AGENTS.md`
- `.gitignore`
- `.vscode/extensions.json`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Require .NET 8 without pinning a patch version.
- Prefer WinGet for SDK installation when available.
- Install missing prerequisites only after obtaining the approval required by the device.

## Validation

- Confirmed `.vscode/extensions.json` contains all three required extension identifiers.
- Confirmed this device has .NET SDK 8.0.423 and all three VS Code extensions installed.
- Confirmed the Unity solution compiles successfully with the installed SDK.

## Next Action

1. On the other device, open this repository with Codex and allow the prerequisite checks and any requested installations.
2. Continue the pending Unity Play Mode regression described in `specs/task-and-settlement.md`.

## Resume Prompt

Continue Puffies on this device. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first. If this is a new device, complete New Device Bootstrap before diagnosing C# errors or changing project files.
