# Card-Pack Dismantle Effect Preview

- Status: In Progress
- Scope: Editor-only assembled preview for the full card-pack opening animation plus dismantle particles

## Requirements

**User Story:** As a developer, I want to see the complete supplied card-pack animation and dismantle particles together with a real project cover in the Unity Editor, so that I can judge whether the delivery matches the effect reference.

1. WHEN the preview Prefab is opened THEN Unity SHALL show all six authored card-pack animation layers using `PackIcon001` as their dynamic cover.
2. WHEN the combined preview plays THEN all five authored ParticleSystem nodes SHALL remain present and use their imported materials.
3. WHEN the preview is rebuilt THEN the editor tool SHALL preserve every imported source Prefab unchanged.
4. IF the project uses URP Renderer2D THEN the imported particle passes SHALL use an SRP-compatible unlit LightMode.
5. The preview SHALL remain editor-only and SHALL NOT change MainScene playback.
6. WHEN the preview menu is opened THEN the six card-pack Animators and dismantle particles SHALL loop together in the Scene View.

## Design

- Generate `CardPackDismantlePreview.prefab` beside the imported dismantle effect assets.
- Instantiate `CardPackOpening.prefab` plus `_002` through `_006` as nested Prefab layers and apply `PackIcon001` through renderer property blocks, matching MainScene's dynamic-cover approach.
- Rotate and fit the combined animated model to the visible tear-line width at approximately `2.4` world units, with its top edge at the particle origin. The source trail's `x=-2.67` position is an off-card spawn point, not the card width.
- Keep `CardPackDismantle_001.prefab` as a nested instance so its hierarchy and asset references remain authoritative.
- Provide `Puffies/Effects/Preview Card Pack Dismantle` to open Prefab Mode, frame the result from above, and loop the animation and particles together.
- Change only the two legacy `ForwardBase` particle pass tags to `SRPDefaultUnlit`; retain the authored shader calculations and blend modes.

## Tasks

- [x] Audit all three source packages by GUID.
- [x] Update the editor generator from a static-cover particle test to a complete combined preview.
- [x] Adapt the two legacy particle pass tags for Renderer2D.
- [x] Regenerate the combined preview Prefab in Unity.
- [x] Validate compilation and asset references.
- [x] Record the result in `Documents/CURRENT_TASK.md`.
- [ ] Visually compare the unobstructed combined animation against the artist reference.

## Validation

- Package GUID audit confirmed every delivered asset is present: shader update 16/16, card-pack animation 44/44, and dismantle particles 13/13.
- The shader update is an update package for shared card-pack dependencies and the sixth animated layer, not a third standalone visual sequence.
- The dismantle source hierarchy contains five ParticleSystems. Four are visible Burst systems; the root system is an authored non-rendering controller.
- The previous preview only contained a static `PackIcon001` Sprite and the dismantle particles; it did not demonstrate the six animated card-pack layers.
- The regenerated Prefab contains `AnimatedCardPack`, all six nested `CardPackOpening` layers, and the untouched nested `DismantleEffect`; the old static `PackCover` node is gone.
- The preview menu applies `PackIcon001` with renderer property blocks, restores Scene visibility, opens a Unity-managed `Card Pack Preview` SceneView, and samples six Animators plus the particle hierarchy together.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- Unity logged `Card-pack combined preview started. animators=6, particles=5` without an exception.
- Final unobstructed visual comparison is pending because the workstation's multi-monitor Unity layout and a foreground SourceTree window obscured the preview during automated capture.
