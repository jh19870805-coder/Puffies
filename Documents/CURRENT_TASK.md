# Current Task

- Task: Freeze tray positions and correct staged outline boundaries
- Status: Completed
- Updated At: 2026-07-20

## User Intent

- Do not refresh the X spacing or positions of remaining tray Pieces after one Piece is placed.
- Group 1 draws only the part of its boundary that touches the final puzzle exterior.
- Every later group draws its own final-puzzle exterior plus its contact edges with all already completed groups.
- Do not draw completed groups' other boundaries, current-to-future-group edges, or seams between Pieces in the same group.

## Working Notes

- Successful placement no longer calls `LayoutTrayPieces`; all remaining Pieces keep their original X and Y positions and leave a gap where the placed Piece was located.
- Initial group creation still performs deterministic Sprite-bounds centering once.
- The outline baker calculates the complete puzzle exterior once, then calculates each current group's own boundary separately.
- Each `GroupNN.png` selects only final-exterior pixels assigned to the current group and current-group boundary pixels adjacent to lower-number completed groups.
- The completed mask is advanced only after the current image is written, so future-group boundaries cannot leak into an earlier stage.
- Previous stage PNGs are no longer overlaid onto later stages.
- All 19 outline masks for the 5 existing CardBag prefabs were regenerated with the corrected rule.

## Files Changed

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag001/Group02.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag002/Group02.png` through `Group04.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag003/Group02.png` through `Group04.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag008/Group02.png` through `Group04.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag017/`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Do not compact tray gaps during a group; stability takes priority over continuously repacking the row.
- Treat lower-number groups as completed when baking a later group.
- Use `ColorBridgeRadius` for completed-area adjacency so anti-aliased gaps between groups do not drop valid contact edges.
- Continue using the existing runtime resource paths and active-group loading logic.

## Validation

- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- Unity 2022.3.62f2 Editor bake completed successfully: 19 group masks from 5 CardBag prefabs; regenerated tracked Group01 PNGs were byte-equivalent to their repository versions.
- CardBag001 Group01 contains 14,674 outline pixels; Group02 contains 9,372 instead of the incorrect 24,018-pixel historical overlay.
- Visually inspected CardBag001 Group01 and Group02: Group01 keeps only its final-puzzle exterior, while Group02 contains its own exterior/contact lines without repeating Group01's unrelated exterior.
- Unity Editor log reported the completed 19-mask bake with no C# compiler error or exception.
- Confirmed successful Piece placement has no remaining tray-layout call.
- `git diff --check` completed without whitespace errors; Git only reported LF-to-CRLF working-copy notices.
- Unity Play Mode verification is still required.

## Next Action

1. Place any one Piece and confirm every remaining Piece stays at the exact same X and Y coordinates.
2. Complete CardBag001 Group01 and confirm Group02 draws the Group02-to-Group01 contact edge without keeping Group01's other outlines.
3. Spot-check a later transition and confirm the current group contacts all completed groups but not future groups.

## Resume Prompt

Continue Puffies fixed tray-position and staged outline-boundary verification. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the listed Play Mode checks or follow the user's latest instruction.
