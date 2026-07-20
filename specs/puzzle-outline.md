# Staged Puzzle Outline And Tray Layout

- Status: Implemented; Play Mode regression pending
- Scope: Editor-baked group outlines and runtime Piece tray stability

## Requirements

1. Pieces are laid out once when a group is created. Successfully placing one Piece must not change any remaining Piece's X or Y position.
2. Group 1 displays only its own contribution to the final puzzle exterior.
3. Every later group displays its own final-puzzle exterior plus its contact edges with all lower-number completed groups.
4. The current image excludes completed groups' unrelated boundaries, current-to-future-group edges, and seams between Pieces in the same group.
5. Previous `GroupNN.png` images are not overlaid onto the current stage.
6. Existing `CardBagNNN` prefabs remain the source of Piece layout and numeric grouping.
7. The stroke color is `#3f423e`; scenes, Canvas dimensions, and authored prefab transforms are not modified by baking.
8. A missing baked Sprite logs a warning and does not block draggable Piece creation.

## Design

1. Decode each prefab's GameBoard and Piece source textures without changing import settings.
2. Transform Piece Alpha masks into GameBoard pixel coordinates and build the complete puzzle mask plus one mask per numbered group.
3. Calculate the complete puzzle exterior once and calculate each current group's own boundary separately.
4. For Group 1, select only final-exterior pixels assigned to Group 1.
5. For every later group, add current-group boundary pixels adjacent to the accumulated lower-number completed mask. `ColorBridgeRadius` bridges narrow anti-aliased artwork gaps.
6. Write the current image before advancing the completed mask so future-group boundaries cannot leak into an earlier stage.
7. Bake one transparent full-board Sprite to `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png` and display only that current-group Sprite at runtime.

## Content Workflow

- Run `Puffies -> Puzzles -> Bake Outline Masks` after adding or changing a CardBag prefab or Piece texture.
- Current generated content contains 19 group masks for CardBag001, 002, 003, 008, and 017.

## Validation

- Unity 2022.3.62f2 successfully baked all 19 masks without compiler errors or exceptions.
- Regenerated tracked Group01 PNGs were byte-equivalent to the prior correct versions.
- CardBag001 Group01 contains 14,674 outline pixels; corrected Group02 contains 9,372 instead of the old 24,018-pixel overlay.
- Static code inspection confirms successful placement no longer calls the tray layout path.
- Play Mode still must verify fixed Piece coordinates and group transitions against completed and future groups.

