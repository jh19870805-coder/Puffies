# Current Task

- Task: Switch CardBag piece grouping to numbered names
- Status: In Progress
- Updated At: 2026-07-08

## User Intent

- Remove `PieceGroup` hierarchy grouping.
- Use only `Piece11`, `Piece21`, `Piece31` style names to group puzzle pieces going forward.

## Working Notes

- `CardBag001` was renamed by the user to `Piece11`...`Piece14` and `Piece21`...`Piece25`.
- `CardBag002` already uses grouped numbers such as `Piece11`, `Piece21`, `Piece31`, `Piece41`.
- Runtime grouping now derives group number from `PieceNN / 10`.
- Group order is ascending numeric group number; pieces inside a group are sorted by full piece number.

## Files Changed

- `Assets/Resources/CardBagPrefabs/CardBag001.prefab`
- `Assets/Resources/CardBagPrefabs/CardBag002.prefab`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Remove `PieceGroup` support entirely.
- Remove the old fallback rule `Piece01-04` / `Piece05+`.
- Use numbered names only:
  - `Piece11`, `Piece12`, ... => group 1
  - `Piece21`, `Piece22`, ... => group 2
  - `Piece31`, `Piece32`, ... => group 3

## Validation

- Confirmed no script constants or methods remain for `PieceGroup` hierarchy grouping.
- Confirmed `CardBag001` groups as group 1: `Piece11`-`Piece14`, group 2: `Piece21`-`Piece25`.
- Confirmed `CardBag002` groups as group 1: `Piece11`-`Piece15`, group 2: `Piece21`-`Piece26`, group 3: `Piece31`-`Piece36`, group 4: `Piece41`-`Piece46`.
- Attempted `dotnet build Puffies.sln`, but `dotnet` is not installed in this environment.
- Unity play mode was not run in this turn.

## Next Action

1. Enter GameScene with PackId 1 and verify it creates 2 groups.
2. Complete group 1 and verify group 2 appears.
3. Enter GameScene with PackId 2 and verify it creates 4 groups.
4. Verify snapping still matches visible grooves for each group.

## Resume Prompt

Continue Puffies numbered CardBag grouping work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify CardBag001/CardBag002 grouping in Unity.
