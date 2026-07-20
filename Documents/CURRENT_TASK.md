# Current Task

- Task: Rename generic card-pack opening assets
- Status: Completed
- Updated At: 2026-07-20

## User Intent

- Remove the obsolete `001` suffix from the generic card-pack opening animation and related assets.
- Use clear English semantic names for the reusable opening presentation.

## Working Notes

- MainScene no longer derives a `CardPackSkin_NNN` prefab from PackId.
- Every pack now uses `CardPackOpening.prefab` and its `CardPackOpening` Animator state.
- MainScene passes the selected entry's already-loaded cover Sprite into `GameAnimationUtility` before hiding the 2D entry.
- `MaterialPropertyBlock` overrides `_BaseMap` and `_MainTex` per renderer, so `CardPackLit.mat` remains unchanged.
- Cover UV uses the Sprite texture rect and center-crops to the authored reference cover aspect (`1822 / 2301`) instead of stretching different aspect ratios.
- Existing renderer-bounds fitting uniformly scales the model inside the clicked RectTransform bounds and recenters the rendered bounds on the entry.
- Missing cover data falls back to the authored `CardPackDefaultCover.png`; failure to load the generic model retains the existing 2D click animation.

## Files Changed

- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Resources/Effects/CardPack/CardPackOpening.prefab`
- `Assets/Resources/Effects/CardPack/CardPackOpening.controller`
- `Assets/Resources/Effects/CardPack/CardPackOpeningAnimation.FBX`
- `Assets/Resources/Effects/CardPack/CardPackOpeningModel.FBX`
- `Assets/Resources/Effects/CardPack/CardPackDefaultCover.png`
- `Assets/Resources/Effects/CardPack/CardPackLit.mat`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/task-and-settlement.md`

## Decisions

- Reuse MainScene's loaded Sprite rather than reading the same cover from disk again.
- Use one generic animated model for all current and future PackIds.
- Preserve asset GUIDs while renaming so Prefab, controller, material, and legacy asset references remain valid.
- Rename the skinned-mesh child to `CardPackMesh`; the animation FBX binds only to the bone hierarchy, not to the mesh object name.
- Use per-renderer material properties instead of cloning or modifying shared material assets.
- Preserve uniform model scaling to avoid deforming the 3D pack; fitting may leave a small margin when the model and UI aspect ratios differ.
- Keep unused legacy `CardPackSkin_002` through `006` assets for now; they are no longer required by the runtime path but were not removed in this task.

## Validation

- Confirmed the generic prefab has one `SkinnedMeshRenderer` and one material slot using `CardPackLit.mat`.
- Confirmed all current `PackIcon001` through `PackIcon021` textures are `600 x 680` with full-image Alpha coverage.
- Static references show MainScene is the only opening-animation caller and now uses the generic API.
- Unity 2022.3.62f2c1 batch import completed successfully and preserved the renamed FBX clip mapping, Animator motion, Prefab controller, mesh, material, and default-cover references.
- Confirmed the five renamed assets retained their original `.meta` GUIDs.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors for first-pass, runtime, and Editor assemblies.
- Final `git diff --check` completed without whitespace errors; Git only reported line-ending conversion notices.
- Final old-name scan found no obsolete generic `CardPackAni_001`, `CardPackSkin_001`, `Take 001`, or `001.png` references; numbered `PackIcon001` remains intentionally unchanged as content naming.
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
