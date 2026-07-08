# Current Task

- Task: Fix CardBag active-area centering without breaking piece alignment
- Status: Done
- Updated At: 2026-07-08

## User Intent

- The active puzzle area inside `CardBag001.prefab` should be relatively centered when switching groups.
- The fix must not change the world/screen mapping in a way that makes pieces snap to the wrong positions.

## Working Notes

- The previous attempt moved the camera with `FitOrthographicCameraToWorldBounds`.
- Moving the camera changes the coordinate basis used by UI bounds and drag/snap calculations, which can make originally correct piece positions appear wrong.
- The corrected approach keeps the camera center stable and moves the loaded `CardBagNNN` prefab root by `anchoredPosition`.

## Files Changed

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`

## Decisions

- Cache the loaded `CardBagNNN` root `RectTransform` and its original anchored position.
- On each group creation, restore the CardBag root to its original position before recalculating.
- Use the active group's groove bounds as the centering target.
- Keep camera adjustment to size-only fitting; do not move camera position.
- Translate the whole CardBag root so grooves and pieces keep their relative local layout.

## Validation

- Confirmed `FitCameraToActiveGroup` now uses `FitOrthographicCameraSizeOnly`.
- Confirmed `CenterCardBagOnActivePage` applies a Canvas anchored-position delta to the loaded CardBag root.
- Confirmed centering target is the active groove group, not the full board or piece tray.
- Did not run Unity; this requires play-mode verification.

## Next Action

1. In Unity, enter PackId 1 and complete the first group.
2. Verify the second group's target area is visually centered.
3. Verify dragging/snapping still lands pieces on the correct original grooves.

## Resume Prompt

Continue Puffies CardBag centering work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify group switching and snap positions in Unity play mode.
