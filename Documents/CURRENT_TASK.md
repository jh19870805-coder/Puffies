# Current Task

- Task: Replace the generic card-pack opening effect with the re-exported package
- Status: Imported and visible in MainScene; final-position alignment deferred
- Updated At: 2026-07-20

## User Intent

- Remove the old opening-effect resources and use the new effect artist export.
- Keep one reusable opening effect for every current and future `600 x 680` card-pack cover.
- Preserve the authored wave edge and make the static-cover-to-animation handoff visually seamless.

## Completed

- Extracted only the generic animation, model, prefab, controller, material, shader, default cover, back texture, and clip mask from `Assets/Resources/卡包.unitypackage`.
- Removed obsolete `CardPackSkin_002` through `CardPackSkin_006`, `CardPackLit.mat`, and the two unused normal/AO textures.
- Normalized the asset names under `Assets/Resources/Effects/CardPack/` to the `CardPackOpening` naming contract.
- Replaced the supplied Built-in/Amplify shader with `Puffies/CardPackOpening`, a URP-compatible two-sided alpha-clip shader.
- Corrected its pass tag to `LightMode=SRPDefaultUnlit`; the project's `Renderer2D` does not draw a `UniversalForward`-only pass.
- Bound the selected PackId Sprite to `_FrontFacesAlbedo` through `MaterialPropertyBlock`; the back texture and wave clip mask remain authored material inputs.
- Kept complete-Sprite UV sampling, renderer pre-hide, animation-time-zero skinning, uniform fit, and center alignment.

## Validation

- `dotnet build Puffies.sln --no-restore`: 0 warnings, 0 errors for first-pass, runtime, and Editor assemblies.
- Unity 2022.3.62f2c1 batch import completed successfully with no shader, missing-reference, or invalid-pass errors.
- Temporary Editor assertions verified shader support and pass layout, all three material textures, Prefab mesh/material, Animator Controller/default state, Resources paths, and absence of the old material/numbered Prefab.
- A Renderer2D offscreen render produced `320789` visible pixels at `600 x 680`; the preview showed the complete cover and both wave-clipped edges instead of a blank frame.
- Animation frame-zero mesh aspect is `0.884720`; target `15:17` is `0.882353`, an approximately `0.27%` difference.
- MainScene Play Mode retest confirmed that the opening animation is visible and generally correct after the Renderer2D pass fix.
- A slight position jump remains between the clicked cover and the animation. The user chose to defer final alignment because the production animation will not play at the current MainScene location.

## Source Package

- `Assets/Resources/卡包.unitypackage` is intentionally still present as the supplied import source until visual acceptance.
- It should be moved out of `Assets/Resources` or deleted after acceptance so the archive cannot enter a player build or repository by accident.

## Data Reset

- This presentation-only change does not modify persistence and does not require deleting `LocalData.db` or `LocalData.json`.

## Next Action

1. Confirm the production scene/container and final screen position where the opening animation will play.
2. Test PackId 1 and PackId 17 there, then calibrate only the remaining frame-zero position offset.
3. Confirm the final cover content, orientation, dimensions, wave edge, and static-to-animation handoff.
4. After final acceptance, remove or archive `Assets/Resources/卡包.unitypackage` outside `Assets`.

## Resume Prompt

Continue Puffies card-pack opening final-position integration. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/PROJECT_CONTEXT.md, and specs/task-and-settlement.md first, then identify the production animation container and calibrate the remaining frame-zero position offset there.
