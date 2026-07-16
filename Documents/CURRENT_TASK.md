# Current Task

- Task: Add active puzzle area outline
- Status: In Progress
- Updated At: 2026-07-14

## User Intent

- Add a visible outline around the current puzzle target area in `GameScene`.
- The outline should tell the player the boundary of the currently active group.
- Outline color should be `#3f423e`; outline width can start at 3 pixels.

## Working Notes

- `GameScene` already groups puzzle slots by `PieceNN / 10`.
- Active group slots are active but transparent; completed groups are visible; future groups are inactive.
- The outline is generated at runtime as `ActiveGroupOutline`, a world-space `SpriteRenderer` aligned to the current group bounds.
- Runtime combines the active group's slot sprite alpha into one mask, then draws the boundary ring.
- If a sprite texture is not CPU-readable, runtime reads its real alpha through a temporary GPU render texture; the Unity sprite mesh remains the final fallback.
- Puzzle textures such as `Pieces11` use `isReadable: 0` and Tight sprite meshes; outline generation must not call `Texture2D.GetPixels` for them.
- The project has hundreds of card packs, so per-card or per-group authored outline masks are not an acceptable content requirement.
- `GameBoard` contains gray puzzle gaps plus colored characters that some piece cuts cross; a piece Alpha edge is not always a player-facing target boundary.
- First implementation generated the outline before draggable pieces were created; an outline-generation exception could interrupt `CreateDraggableGroup` before the tray pieces were instantiated.

## Files Changed

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Decisions

- Generate a single merged outline for the active group, not separate outlines per piece.
- Render the outline with the same world-space `SpriteRenderer` path used by draggable pieces, one sorting order below placed pieces.
- Destroy and rebuild the outline when groups switch, and clear it before the reward board reveal.
- Use `#3f423e` and `3` pixels as initial visual values.
- Draggable piece creation and layout must run before outline generation.
- Outline generation is isolated behind `try/catch`; a failed outline must not block puzzle creation or interaction.
- Check `Texture2D.isReadable` before CPU pixel access and use GPU readback for non-readable puzzle textures, without changing their import settings.
- Derive all outlines automatically from existing `PieceNN` sprites and prefab placement; do not require additional outline-mask assets.
- Treat Alpha `>= 128` as the intended piece boundary, close gaps up to 2 pixels after merging the group, and draw the 3-pixel ring inside the merged region so it does not enlarge the target shape.
- Keep an outline segment only when pixels immediately outside it are predominantly the existing light `GameBoard` background. Suppress boundaries against gray future-group areas and boundaries within 40 pixels of colored `GameBoard` artwork outside the active mask.

## Validation

- Static-checked that `GameScene.cs` creates `ActiveGroupOutline` after active group/camera setup.
- Static-checked that the outline is cleared during group switch, runtime piece cleanup, and reward reveal.
- Static-checked that the outline color is `#3f423e` and width is controlled by `ActiveGroupOutlinePixels = 3`.
- Static-checked that `RefreshActiveGroupOutline` now runs after `LayoutTrayPieces()` / `CachePieceTrayOriginalPosition()`.
- Static-checked that outline exceptions are caught and logged as warnings.
- Confirmed current `Pieces11` assets are imported with `isReadable: 0` and `spriteMeshType: 1` (Tight).
- Static-checked that non-readable textures now bypass `Texture2D.GetPixels`, read actual alpha through a temporary render texture, and still fall back to the sprite mesh if GPU readback fails.
- Static-checked that the mask is merged before outlining and that only inside-mask boundary pixels are colored, so the line does not enlarge the target region.
- Measured `Pieces11` through `Pieces14`: each source contains roughly 800-2,000 antialiased pixels between Alpha 1 and 254, confirming that the previous `Alpha > 8` threshold expanded the inferred region.
- Static-checked the revised automatic pipeline: Alpha 50% threshold -> merge active group -> 2-pixel morphological closing -> 3-pixel inside outline.
- Compared normal and vertically flipped piece Alpha overlays against `GameBoard`; normal orientation aligns, so the marked errors are boundary-classification errors rather than GPU Y-flip errors.
- Sampled `CardBag001/GameBoard.png`: gray puzzle areas are around RGB `180/175/170`, while light board background channels are generally above `200`; colored character samples have substantially wider channel spread.
- Offline-rendered the first background-only filter and found character sticker borders were wider than the 4-12 pixel sample ring.
- Offline-rendered the final filter against `CardBag001/GameBoard.png`: a strict 40-pixel colored-art exclusion removes the marked purple/blue character lines and the near-background test removes the lower gray group seam while retaining the top/left outer boundary.
- Implemented colored-art proximity with separable sliding windows so the 40-pixel exclusion is linear in texture size instead of scanning the full neighborhood for every outline pixel.
- Updated `ActiveGroupOutline` display from a `GameBoard` child `Image` to a world-space `SpriteRenderer`, so it shares the same camera coordinate path as puzzle pieces.
- Ran scoped `git diff --check` for touched script/docs; no whitespace errors. Git only reported LF-to-CRLF working-copy warnings.
- Unity Play Mode was not run from this shell.

## Next Action

1. Open `GameScene` in Unity and enter Play Mode.
2. Verify the current puzzle target group shows a `#3f423e` outline.
3. Complete the group and verify the outline rebuilds around the next target group.
4. Complete all groups and verify the outline is gone when the reward panel shows.

## Resume Prompt

Continue Puffies active puzzle area outline work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify GameScene outline behavior in Unity Play Mode.
