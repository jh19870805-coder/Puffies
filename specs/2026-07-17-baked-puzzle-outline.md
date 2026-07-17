# Baked Puzzle Boundary Outline

## Goal

Generate a deterministic outline for each numbered puzzle group without drawing any seam inside the puzzle area.

## Requirements

- The visible stroke follows only the boundary between the gray puzzle area and the light board background.
- Piece-to-piece and group-to-group seams inside the gray puzzle area are excluded.
- Existing `CardBagNNN` prefabs remain the source of piece layout and grouping.
- All existing card bags can be processed in one Unity Editor command.
- Runtime loads the baked sprite directly and reports a warning when a card bag has not been baked.
- The stroke color is `#3f423e` and the initial width is approximately three source pixels.
- The implementation must not modify scenes, Canvas dimensions, or authored prefab transforms.

## Design

1. Decode each prefab's `GameBoard` and `PieceNN` sprite source PNGs without changing their import settings.
2. Transform every Piece Alpha mask into GameBoard pixel coordinates and union all pieces.
3. Flood-fill transparent space from the board edge to identify the true exterior of the complete puzzle. Enclosed holes and internal seams are not exterior.
4. Use the GameBoard's gray-to-light color transition to validate the geometric exterior boundary and bridge small interruptions caused by artwork texture.
5. Intersect that exterior boundary with each numbered group mask and bake one full-board transparent PNG per group.
6. At runtime, attach the selected PNG as a stretched, non-interactive child of `GameBoard`.

## Output

`Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`

## Validation

- Bake all current card bags in Unity batch mode.
- Inspect generated masks for continuity and absence of interior lines.
- Compile runtime and Editor assemblies.
- Enter `GameScene`, switch groups, and verify outline cleanup before the reward view.
