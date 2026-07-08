# Current Task

- Task: Check UI resource rename references
- Status: Done
- Updated At: 2026-07-08

## User Intent

- The user renamed a folder and image resources under `Assets/UI` in Unity.
- Check Git workspace changes and find code/config references that still need updating.

## Working Notes

- Detected old deleted path: `Assets/UI/Game001`.
- Detected new path: `Assets/UI/CardBag001`.
- Detected image renames:
  - `Bag00101/Pieces01`...`Pieces04` -> `Pieces11`...`Pieces14`
  - `Bag00102/Pieces05`...`Pieces09` -> `Pieces21`...`Pieces25`
- Confirmed the corresponding `.meta` GUIDs stayed the same, so Unity scene/prefab direct sprite references should remain valid.

## Files Changed

- `Assets/Scripts/Editor/BuildSync.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Updated `BuildSync` UI streaming folder whitelist from `Game001` to `CardBag001`.
- Did not change scene or prefab references because GUIDs match the old assets.
- Updated project context to document `CardBag001` as the current puzzle texture source.

## Validation

- Ran `git status --short` and inspected the UI rename set.
- Searched code, resources, scenes, prefabs, packages, project settings, and docs for old names:
  - `Game001`
  - `Bag00101`
  - `Bag00102`
  - `Pieces01`...`Pieces09`
- Found only one code reference requiring change: `BuildSync.cs`.
- Rechecked old/new `.meta` GUID pairs for `GameBoard.png` and all renamed puzzle pieces; all matched.
- Did not run Unity; this was a source/config reference check.

## Next Action

1. Open Unity and let it reimport the renamed assets.
2. Run `Puffies -> Sync Build Resources` so `Assets/StreamingAssets/UI` is regenerated from `Assets/UI/CardBag001`.
3. Enter GameScene and verify the puzzle sprites still appear through their GUID references.

## Resume Prompt

Continue Puffies UI resource rename work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify Unity import and StreamingAssets sync.
