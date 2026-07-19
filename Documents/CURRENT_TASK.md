# Current Task

- Task: Remove stale BuildSync CardBag folder warning
- Status: Completed
- Updated At: 2026-07-19

## User Intent

- Resolve the editor-load warning that BuildSync skipped the missing `Assets/UI/CardBag001` folder.

## Working Notes

- Puzzle source textures were previously moved to `Assets/UI/CardBags/CardBagNNN`, but `BuildSync` still listed the old single-pack folder.
- CardBag source textures do not need StreamingAssets copies because `Resources/CardBagPrefabs/CardBagNNN` carries their Sprite references into the Unity build.
- Removed `CardBag001` from `UiStreamingFolders` instead of copying the entire CardBags source tree.
- Added `AchieveScene` and `RankScene`, whose controllers load UI files through disk paths in player builds.
- The final synchronized folders are `PackImages`, `BasicUI`, `AchieveScene`, and `RankScene`.
- Replaced raw legacy-folder deletion with AssetDatabase deletion plus an orphan-meta fallback, fixing the `StreamingAssets/Configs.meta` warning.
- Added obsolete singular `StreamingAssets/Config` to cleanup; its `Package001.json` belongs to the retired JSON puzzle flow.

## Files Changed

- `Assets/Scripts/Editor/BuildSync.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Keep StreamingAssets limited to UI files loaded through `GameCommonUtility.LoadSpriteByPath` rather than duplicating every imported UI source texture.
- Always remove a legacy StreamingAssets folder and its `.meta` together.

## Validation

- Confirmed all four configured source folders exist under `Assets/UI`.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- `git diff --check` completed without whitespace errors.
- Confirmed `StreamingAssets/Config`, `StreamingAssets/Configs`, and both corresponding `.meta` files are absent after cleanup.
- Running the Unity menu command without warnings still needs verification after Unity recompiles.

## Next Action

1. Let Unity compile, then run `Puffies -> Sync Build Resources` and confirm no skipped-folder warning appears.
2. Resume MainScene downward-shadow visual tuning using the latest generated-shadow parameters.

## Resume Prompt

Continue Puffies BuildSync verification. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the sync menu regression or follow the user's latest instruction.
