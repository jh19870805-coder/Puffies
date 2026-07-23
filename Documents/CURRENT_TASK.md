# Current Task

- Task: Replace MainScene static card-pack covers with persistent effects
- Status: Completed
- Updated At: 2026-07-23

## User Intent

- Replace every card-pack static cover in the MainScene list with a persistent lightweight card-pack effect using that pack's real cover.
- Start a subtle breathing animation automatically when MainScene opens.
- Keep the card-pack size image at its authored position and visual layer.
- On click, stop breathing, enlarge the selected pack from list size to the original `600 x 680` design size, play the complete six-layer opening animation, then enter GameScene for that PackId.

## Completed

- `GameAnimationUtility` builds one shared frame-zero mesh from all six authored `CardPackOpening` layers, avoiding six SkinnedMeshRenderers and Animators per list item.
- Each visible MainScene package owns one lightweight MeshRenderer display with its real cover and completed-state tint.
- MainScene aligns displays to their `PackCover` anchors in `LateUpdate`, applies a staggered `0.98..1.02` breathing scale over `2.4s`, disables off-page renderers, and clips visible fragments to the ScrollRect viewport.
- The existing `PackSize` Image remains the source of position, Sprite, and tint. A matching clipped world overlay preserves its foreground presentation over the world card without changing the authored RectTransform or hierarchy.
- Clicking a pack switches from its idle display to the reusable six-layer opener at the same pose, scales to `600 x 680` over `0.3s`, starts the authored animation, waits for its longest clip, and enters GameScene.
- Static cover/shadow fallback remains available when effect creation fails. Pending reward-flight packs remain hidden until the existing reveal callback.
- Generated per-pack objects and shared runtime meshes/materials are released when the list rebuilds or MainScene is destroyed.
- Removed `TemporaryOpenGameView`, which incorrectly forced Play Mode after every script reload.

## Files Changed

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Resources/Effects/CardPack/CardPackOpening.shader`
- `Assets/Scripts/View/PackageInteractionHandler.cs`
- Removed `Assets/Scripts/Editor/TemporaryOpenGameView.cs` and its `.meta`.
- Updated `specs/2026-07-22-home-card-pack-effects.md`.
- Updated `Documents/PROJECT_CONTEXT.md`.

## Decisions

- Reuse one shared baked idle mesh plus one reusable six-layer animated opener. Do not create six live animated layers for every list item.
- The list effect uses the selected pack's real cover and preserves existing `PackSize` authoring data.
- The click sequence is scale-up first, authored opening animation second, GameScene transition last.
- The imported dismantle particle Prefab remains separate from this accepted MainScene sequence until its timing is explicitly approved.

## Validation

- Automated MainScene Play Mode validation created four idle card-pack effects and selected PackId 1.
- The selected idle effect breathed from scale `2.449215` to `2.52881527`; its PackSize position, sibling index, Sprite, and tint remained unchanged.
- The click flow enlarged the reusable opener to scale `6.246120`, started all six Animators and six Renderers, then entered GameScene.
- `2560 x 1440` idle and opening screenshots were visually inspected. Dynamic covers, foreground size badges, list alignment, and the visible tear animation rendered without overlap corruption.
- Unity completed a clean batch refresh with no C# or Shader compile errors.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors across runtime, first-pass, and Editor assemblies.
- No local JSON or SQLite reset is required.

## Next Action

1. Let the user evaluate the breathing amplitude and `0.3s` scale-up pacing in the normal Unity Editor workflow.
2. Keep the dismantle particle timing as a separate follow-up; do not add it to the accepted sequence without explicit approval.

## Resume Prompt

Continue Puffies development. Read `AGENTS.md`, `Documents/WORKFLOW.md`, and `Documents/CURRENT_TASK.md`; the MainScene persistent card-pack effect sequence is implemented and validated, so follow the user's next instruction without changing it implicitly.
