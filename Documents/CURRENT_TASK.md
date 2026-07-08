# Current Task

- Task: Load CardBag prefab dynamically in GameScene
- Status: Done
- Updated At: 2026-07-08

## User Intent

- The user removed `GameBoard` from `GameScene`.
- When entering GameScene from a card pack, load the matching `CardBagNNN` prefab by dynamic pack id.
- Keep the existing puzzle gameplay logic and make board movement/adaptation work against the loaded card bag prefab.

## Working Notes

- Runtime path rule: pack id `1` loads `Resources/CardBagPrefabs/CardBag001`.
- `CardBag001.prefab` now lives under `Assets/Resources/CardBagPrefabs/` so it can be loaded in builds with `Resources.Load`.
- The loaded prefab is instantiated under the scene Canvas, then placed after `Background` in sibling order when possible.
- Existing logic still finds the child object named `GameBoard`, collects `Piece01`... images, handles grouping, and drives board movement through `_board.GameBoardImage`.

## Files Changed

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Resources/CardBagPrefabs.meta`
- `Assets/Resources/CardBagPrefabs/CardBag001.prefab`
- `Assets/Resources/CardBagPrefabs/CardBag001.prefab.meta`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Use `Resources/CardBagPrefabs/CardBagNNN.prefab` for now because the project already uses `Resources.Load` and has not introduced Addressables.
- Keep `GameBoard` as the required child object name inside each `CardBagNNN` prefab to preserve existing puzzle logic.
- Keep `Piece01`... naming and optional `PieceGroup01`... grouping inside the prefab.
- Put the loaded card bag after `Background` so it is not hidden behind the background image.

## Validation

- Confirmed `Assets/Resources/CardBagPrefabs/CardBag001.prefab` exists and contains a child object named `GameBoard`.
- Searched for dynamic loading symbols and verified `GameScene` now calls `EnsureCardBagLoaded(bagId)` before board/groove initialization.
- Confirmed no `GameBoard` object remains in `GameScene.unity`; it is expected to come from the loaded prefab.
- Did not run Unity; this was a source and asset-structure change.

## Next Action

1. Open Unity and let it import `Assets/Resources/CardBagPrefabs/CardBag001.prefab`.
2. Play from MainScene into PackId 1 and verify the board, grooves, dragging, group switching, and RewardPanel still work.
3. Add `CardBag002`, `CardBag003`, and `CardBag004` prefabs before making PackIds 2-4 playable, or add a fallback rule if they should reuse CardBag001 temporarily.

## Resume Prompt

Continue Puffies CardBag dynamic loading work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify Unity play mode for PackId 1 and add remaining CardBag prefabs or a fallback.
