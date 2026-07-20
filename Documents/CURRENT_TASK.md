# Current Task

- Task: Use real covers in a generic card-pack opening animation
- Status: Completed
- Updated At: 2026-07-20

## User Intent

- Keep the existing card-pack opening animation.
- Reuse it for every PackId and display the actual corresponding card-pack cover.
- Keep the animated model centered and sized consistently with the clicked MainScene card pack.

## Working Notes

- MainScene no longer derives a `CardPackSkin_NNN` prefab from PackId.
- Every pack now uses `CardPackSkin_001` and its existing `Take 001` Animator state.
- MainScene passes the selected entry's already-loaded cover Sprite into `GameAnimationUtility` before hiding the 2D entry.
- `MaterialPropertyBlock` overrides `_BaseMap` and `_MainTex` per renderer, so `CardPackLit.mat` remains unchanged.
- Cover UV uses the Sprite texture rect and center-crops to the authored reference cover aspect (`1822 / 2301`) instead of stretching different aspect ratios.
- Existing renderer-bounds fitting uniformly scales the model inside the clicked RectTransform bounds and recenters the rendered bounds on the entry.
- Missing cover data falls back to the authored `001.png`; failure to load the generic model retains the existing 2D click animation.

## Files Changed

- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/task-and-settlement.md`

## Decisions

- Reuse MainScene's loaded Sprite rather than reading the same cover from disk again.
- Use one generic animated model for all current and future PackIds.
- Use per-renderer material properties instead of cloning or modifying shared material assets.
- Preserve uniform model scaling to avoid deforming the 3D pack; fitting may leave a small margin when the model and UI aspect ratios differ.
- Keep unused legacy `CardPackSkin_002` through `006` assets for now; they are no longer required by the runtime path but were not removed in this task.

## Validation

- Confirmed the generic prefab has one `SkinnedMeshRenderer` and one material slot using `CardPackLit.mat`.
- Confirmed all current `PackIcon001` through `PackIcon021` textures are `600 x 680` with full-image Alpha coverage.
- Static references show MainScene is the only opening-animation caller and now uses the generic API.
- Unity 2022.3.62f2 batch import regenerated `Assembly-CSharp.dll` without C# compiler errors or exceptions.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors for first-pass, runtime, and Editor assemblies.
- Final `git diff --check` completed without whitespace errors; Git only reported line-ending conversion notices.
- Final change-scope check confirmed no Scene, Prefab, Material, FBX, controller, or cover image was modified.
- Play Mode visual verification remains pending.

## Data Reset

- This presentation change does not modify persistence and does not require deleting `LocalData.db` or `LocalData.json`.

## Next Action

1. In MainScene, open PackId 1 and confirm its real cover appears on the 3D model without stretching, rotation, or mirroring.
2. Open a PackId above 6, preferably 17, and confirm the same generic 3D animation plays instead of the 2D fallback.
3. Confirm the closed model is centered and fully contained within the clicked card-pack UI bounds before it opens.
4. If visual inspection reveals the model UV orientation differs from the UI Sprite, adjust only the UV transform; do not re-export the animation.

## Resume Prompt

Continue Puffies generic card-pack opening verification. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/PROJECT_CONTEXT.md, and specs/task-and-settlement.md first, then perform the listed Play Mode checks or follow the user's latest instruction.
