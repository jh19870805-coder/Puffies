# Current Task

- Task: Prepare the generic card-pack opening path for a re-exported effect
- Status: Waiting for re-exported effect asset
- Updated At: 2026-07-20

## User Intent

- Stop compensating for the old effect's incompatible cover aspect in runtime code.
- Have the effect artist re-export one generic opening effect that matches every current and future `600 x 680` card-pack cover.
- The final static-cover-to-animation handoff must be visually seamless.

## Working Notes

- `PackIcon001.png` through `PackIcon021.png` are all exactly `600 x 680`; this is the reusable runtime cover contract.
- The old effect was authored against a different cover aspect, so code-only attempts necessarily introduced cropping, stretching, or blank margins.
- Runtime now uses the complete Sprite texture rectangle without cropping or aspect compensation.
- The animation-time-zero skinned mesh is measured, uniformly fitted inside the clicked `240 x 272` UI bounds, and centered before its renderers are enabled.
- The replacement effect must provide a closed first-frame cover surface and UV layout with the same `600:680` aspect as the runtime cover.

## Files Changed

- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/task-and-settlement.md`

## Decisions

- Remove the temporary model-width calibration and all old-reference-aspect calculations.
- Keep complete-texture sampling, current-pose bounds measurement, renderer pre-hide, uniform fit, and centering as the neutral runtime contract.
- Re-export only one generic effect/model; do not create per-PackId models or animations.

## Effect Export Contract

- Closed animation frame at normalized time zero.
- Front cover surface aspect: exactly `600:680` (`15:17`).
- Front cover UV: complete normalized rectangle, no crop, padding, atlas border, or intentional distortion.
- Front view at frame zero: orthographic-facing, centered pivot, no perspective tilt or root offset.
- The closed model silhouette must fit the same `240 x 272` screen rectangle used by `PackCover`.
- Keep the existing generic Animator state contract `CardPackOpening`, or update the runtime state name together with the replacement asset.

## Validation

- Confirmed all 21 numbered runtime pack covers are `600 x 680` (`0.882353` aspect).
- `dotnet build Puffies.sln --no-restore` completed after the rollback with 0 warnings and 0 errors for first-pass, runtime, and Editor assemblies.
- Replacement-effect import and Play Mode visual acceptance remain pending.

## Data Reset

- This presentation-only change does not modify persistence and does not require deleting `LocalData.db` or `LocalData.json`.

## Next Action

1. Obtain the re-exported generic model/animation that follows the Effect Export Contract.
2. Replace the generic effect assets while preserving or deliberately updating their Unity references.
3. Test PackId 1 and PackId 17 in MainScene at the supported resolutions.
4. Accept only when the static cover and animation frame zero match in position, dimensions, aspect, visible content, and orientation.

## Resume Prompt

Continue Puffies generic card-pack effect replacement. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/PROJECT_CONTEXT.md, and specs/task-and-settlement.md first, then import the re-exported effect or follow the user's latest instruction.
