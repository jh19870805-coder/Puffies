# Current Task

- Task: Assemble complete card-pack opening and dismantle preview
- Status: In Progress; visual acceptance pending
- Updated At: 2026-07-22

## User Intent

- Assemble all delivered card-pack animation layers and dismantle particles with a real project card-pack cover.
- Make the complete result directly previewable in the Unity Editor like the effect artist's reference image.

## Completed

- Audited all three source packages by GUID: shader update 16/16, card-pack animation 44/44, and dismantle particles 13/13 are present in the project.
- Rebuilt `CardPackDismantlePreview.prefab` with all six `CardPackOpening` animated layers plus the imported dismantle effect as nested Prefab instances. The old static `PackCover` preview object was removed.
- The preview applies `PackIcon001` through renderer property blocks, fits the animated card pack to `2.4` world units wide, and aligns its top edge with the authored tear origin.
- Updated `Puffies -> Effects -> Preview Card Pack Dismantle` (`Ctrl+Shift+D`) to restore Scene visibility, open a Unity-managed `Card Pack Preview` SceneView, and loop six Animators with the five-particle hierarchy.
- Updated the two Shader Forge particle passes from legacy `ForwardBase` to `SRPDefaultUnlit` for URP Renderer2D.
- Removed three unavailable legacy custom material Inspector declarations so selecting the imported materials no longer logs editor warnings.

## Files Changed

- Added `Assets/Resources/Effects/CardPackDismantle/CardPackDismantlePreview.prefab`.
- Added `Assets/Scripts/Editor/CardPackDismantlePreviewEditor.cs`.
- Updated the three imported dismantle shaders for Renderer2D/editor compatibility.
- Added `specs/2026-07-22-card-pack-dismantle-preview.md`.
- Updated `Documents/PROJECT_CONTEXT.md` with the preview asset and menu.

## Decisions

- Keep all imported `CardPackOpening` and `CardPackDismantle_001` Prefabs authoritative. The preview references them instead of copying or editing their hierarchies.
- Treat the source trail's `x=-2.67` as an off-card spawn position, not the cover width. The preview width is calibrated from the visible tear line.
- Treat the shader update package as an update for shared card-pack dependencies and animated layer 006, not a third standalone animation sequence.
- Keep the assembled Prefab editor-only for now. MainScene runtime playback remains the existing six-layer `CardPackOpening` animation.

## Validation

- Confirmed the generated Prefab contains `AnimatedCardPack`, six nested animated Prefab references, and `DismantleEffect`, with no static `PackCover` node.
- Unity logged `Card-pack combined preview started. animators=6, particles=5` without an exception.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors across runtime, first-pass, and Editor assemblies.
- Final unobstructed visual comparison remains pending because the current multi-monitor Unity layout and a foreground SourceTree window obscured automated capture.
- No local JSON or SQLite reset is required.

## Next Action

1. Bring Unity to the foreground and open `Puffies -> Effects -> Preview Card Pack Dismantle` to compare the combined animation against the artist reference.
2. If the silver tear edge or upper/lower package separation is still absent, inspect the six-layer source animation at the relevant sampled frame before requesting another artist export.
3. After visual acceptance, decide the particle timing offset and wire the accepted sequence into MainScene without changing the imported source Prefabs.

## Resume Prompt

Continue Puffies card-pack effect integration. Read `AGENTS.md`, `Documents/WORKFLOW.md`, and `Documents/CURRENT_TASK.md`, then review `Resources/Effects/CardPackDismantle/CardPackDismantlePreview` and decide the runtime sequence before changing MainScene playback.
