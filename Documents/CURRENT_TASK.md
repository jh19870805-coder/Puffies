# Current Task

- Task: Consolidate flat Model scripts
- Status: Completed
- Updated At: 2026-07-20

## User Intent

- Keep source directories shallow and avoid creating many nested folders.
- Reduce the number of C# files in `Assets/Scripts/Model` by merging closely related functionality without changing behavior.

## Working Notes

- Model remains one flat directory and now contains 10 C# files instead of 14.
- `GameManager` moved into `GameDefine.cs`.
- `CsvTable` and `CsvRow` moved into `GameConfigRepository.cs`.
- `JsonLocalStore` and `SqliteLocalStore` now share the accurately named `LocalDataStore.cs` file.
- Score context/result types and `GameScoreUtility` moved into `GameTaskUtility.cs`.
- Public type names, APIs, constants, persisted fields, and runtime behavior remain unchanged.
- Garbled XML comments were not copied with the moved implementations; no unrelated source-wide encoding rewrite was performed.

## Files Changed

- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/LocalDataStore.cs`
- `Assets/Scripts/Model/GameTaskUtility.cs`
- Deleted `GameManager.cs`, `CsvTable.cs`, `JsonLocalStore.cs`, and `GameScoreUtility.cs` plus their `.meta` files.
- Renamed the combined `SqliteLocalStore.cs` file to `LocalDataStore.cs` while preserving its `.meta` GUID.
- `AGENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/task-and-settlement.md`

## Decisions

- Keep `CardPackDataUtility`, `GameAnimationUtility`, and `GameCommonUtility` separate because each is already a large independently owned module.
- Do not add Model subfolders solely to categorize ten files.
- Delete the four obsolete script GUIDs only after confirming they had no Scene, Prefab, or Asset references.
- Do not modify Unity-generated project files manually; regenerate them through Unity.
- Name shared domain files after the domain rather than one contained implementation.

## Validation

- Confirmed the removed script GUIDs had no serialized Unity asset references.
- Unity 2022.3.62f2 batch import completed without C# compiler errors and regenerated `Assembly-CSharp.dll`.
- Unity solution synchronization removed all four deleted source paths from `Assembly-CSharp.csproj`.
- Unity solution synchronization replaced the old persistence source path with `Assets/Scripts/Model/LocalDataStore.cs`.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors for first-pass, runtime, and Editor assemblies.
- `git diff --check` completed without whitespace errors; Git only reported line-ending conversion notices.
- Play Mode smoke testing remains pending.

## Data Reset

- This refactor does not change serialized data or persistence schemas and does not require deleting `LocalData.db` or `LocalData.json`.
- The separate reset requirement for testing the previously changed CardPacks schema still applies when using pre-schema-change development data.

## Next Action

1. Open Unity and confirm LoadingScene reaches MainScene without missing-script or type-load errors.
2. Enter one card pack and smoke-test task score settlement plus return to MainScene.
3. Continue the broader Play Mode regression recorded in the two specs.

## Resume Prompt

Continue Puffies after the flat Model consolidation. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, and Documents/PROJECT_CONTEXT.md first, then run the Play Mode smoke checks or follow the user's latest instruction.
