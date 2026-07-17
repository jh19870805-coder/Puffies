# Current Task

- Task: Replace runtime outline plugin with baked puzzle outlines
- Status: Completed
- Updated At: 2026-07-17

## User Intent

- Draw only the active group's segments along the complete puzzle exterior.
- Never draw Piece or group seams in the middle of the puzzle area.
- Support large batches of `CardBagNNN` prefabs through one Editor command.
- Remove the third-party outline plugin and redundant fallback code after the baked path replaces it.
- Keep color `#3f423e` and an initial source width of approximately three pixels.
- Do not modify scene Canvas dimensions or authored CardBag prefab transforms.

## Working Notes

- `PuzzleOutlineBakerEditor` transforms authored Piece Alpha into GameBoard pixel space, closes narrow gaps, flood-fills the complete puzzle exterior, and writes one full-board PNG per numbered group.
- CardBag001 has a real gray puzzle area, so the baker uses its gray/light transition to validate the boundary.
- CardBag002, CardBag003, and CardBag008 do not have a gray missing area; low color coverage automatically falls back to the continuous closed geometric exterior.
- Runtime derives the group number from the actual `PieceNN` name and displays the baked Sprite as a stretched, non-interactive `GameBoard` child.
- Missing baked resources now produce a warning that names the required Editor menu; they do not affect puzzle interaction.
- Removed the OutlineFx runtime proxy/blocker path, per-frame proxy synchronization, screen offset, Renderer Feature, setup script, embedded package, lock entry, and package ignore exceptions.

## Files Changed

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/`
- `Assets/Settings/Renderer2D.asset`
- `Packages/packages-lock.json`
- `.gitignore`
- `specs/2026-07-17-baked-puzzle-outline.md`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Removed

- `Packages/www.nulltale.outlinefx/`
- `Assets/Scripts/Editor/OutlineFxRendererSetupEditor.cs`

## Decisions

- Baked Unity Sprites are the only production outline path.
- The project no longer includes a runtime outline Shader or third-party rendering dependency.
- Every new or changed CardBag must pass `Puffies -> Puzzles -> Bake Outline Masks` before delivery.
- A missing bake is an authoring error surfaced by a warning, not a reason to restore runtime edge detection.

## Validation

- Previously baked 14 non-empty group Sprites for CardBag001, CardBag002, CardBag003, and CardBag008 in Unity batch mode.
- Visually inspected both gray/light and geometric-fallback outputs; the lines are continuous exterior segments without Piece seams.
- Global search after cleanup found no live OutlineFx namespace, package name, Shader GUID, or Renderer Feature reference under Assets, Packages, specs, or `.gitignore`.
- `Renderer2D.asset` now serializes an empty `m_RendererFeatures` list.
- Unity detected the package removal and rebuilt both `Assembly-CSharp` and `Assembly-CSharp-Editor` successfully.
- The latest Editor log contains no Missing Renderer Feature, Missing Script, package resolution, C#, Shader, or exception error after cleanup.
- Recompiled both assemblies with Unity's generated Roslyn response files after the final runtime simplification; both exited with code 0.
- Confirmed all 14 generated PNGs still have valid Unity Sprite import metadata.
- Confirmed no stale OutlineFx assembly remains under `Library/ScriptAssemblies`.

## Next Action

1. Enter `GameScene`, switch groups, and verify outline cleanup before `RewardPanel` appears.
2. Run `Puffies -> Puzzles -> Bake Outline Masks` whenever a CardBag image, Piece Alpha, or prefab layout changes.

## Resume Prompt

Continue the baked puzzle outline workflow. Read `AGENTS.md`, `Documents/WORKFLOW.md`, and `Documents/CURRENT_TASK.md`; do not restore a runtime outline plugin.
