# Baked Puzzle Boundary Outline

- Status: Implemented; visual regression required for new card bags

## Goal

Generate a deterministic outline for each numbered puzzle group without drawing seams inside the puzzle area.

## Requirements

- The visible stroke follows only the boundary between the gray puzzle area and the light board background.
- Piece-to-piece and group-to-group seams inside the gray puzzle area are excluded.
- Existing `CardBagNNN` prefabs remain the source of Piece layout and grouping.
- All card bags can be processed through one Unity Editor command.
- Runtime loads the baked sprite directly and warns when a card bag has not been baked.
- The stroke color is `#3f423e` and the initial width is approximately three source pixels.
- Scenes, Canvas dimensions, and authored prefab transforms must not be modified.

## Design

1. Decode each prefab's `GameBoard` and `PieceNN` sprite source PNGs without changing import settings.
2. Transform Piece Alpha masks into GameBoard pixel coordinates and union all pieces.
3. Flood-fill transparent space from the board edge to identify the complete puzzle exterior; enclosed holes and internal seams are excluded.
4. Use the board's gray-to-light transition to validate the geometric boundary and bridge small artwork interruptions.
5. Intersect the exterior boundary with each numbered group mask and bake one transparent full-board PNG per group.
6. Attach the selected PNG at runtime as a stretched, non-interactive `GameBoard` child.

## Output

`Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`

## Validation

- Run `Puffies -> Puzzles -> Bake Outline Masks` after adding or changing a card bag.
- Inspect generated masks for continuity and absence of interior lines.
- Enter GameScene, switch groups, and verify outline cleanup before the reward view.

