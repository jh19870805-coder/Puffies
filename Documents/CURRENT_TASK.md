# Current Task

- Task: Integrate OutlineFx for active puzzle area outline
- Status: In Progress
- Updated At: 2026-07-16

## User Intent

- Use a reliable free third-party Unity plugin for the current puzzle target outline.
- The outline should identify the boundary of the active puzzle group in `GameScene`.
- Use color `#3f423e` and start near 3 pixels wide.

## Working Notes

- Selected `NullTale/OutlineFx`, licensed under MIT, and embedded its verified upstream commit in the project.
- The active groove objects are transparent UGUI `Image` components, while OutlineFx accepts `Renderer` components.
- `GameScene` now creates transparent world-space `SpriteRenderer` proxies from the active groove sprites.
- OutlineFx renders all proxies into one screen-space mask before outlining, so adjacent pieces form one group outline instead of separate per-piece outlines.
- Proxy rendering samples sprite alpha on the GPU and does not require readable source textures.
- `OutlineFxRendererSetupEditor` automatically adds and configures `ActiveGroupOutlineFx` on `Assets/Settings/Renderer2D.asset` after Unity resolves the package.

## Files Changed

- `Packages/www.nulltale.outlinefx/`
- `.gitignore`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/OutlineFxRendererSetupEditor.cs`
- `Assets/Scripts/Editor/OutlineFxRendererSetupEditor.cs.meta`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Decisions

- Embed OutlineFx commit `394abc8f69e5362737759c7cca1a221a7a30dc67` under `Packages/www.nulltale.outlinefx` for reproducible, offline package resolution.
- Keep the existing UGUI puzzle prefab format; create runtime SpriteRenderer proxies instead of converting authored content.
- Render proxies fully transparent in the normal camera pass; their sprite alpha remains available to OutlineFx's override mask pass.
- Configure a hard box outline, no solid fill, 50% alpha cutoff, no depth attachment, and thickness `0.075`, which targets roughly 3 pixels at the 1440-pixel design height.
- Clear and rebuild the proxy root when groups switch, and clear it before the reward board reveal.
- Catch outline setup failures so the optional visual effect cannot block puzzle creation or interaction.

## Validation

- Confirmed the project uses Unity `2022.3.62f2c1`, URP `14.0.12`, and `Renderer2D.asset`.
- Reviewed OutlineFx package `1.1.0`, MIT license, Unity `2021.3` baseline, and its Unity 2022 `RTHandle` code path.
- Static-checked that the plugin combines all registered renderers into one mask before applying the outline pass.
- Static-checked active group proxy creation after camera/card layout and proxy cleanup during group switching and reward preparation.
- Static-checked that transparent proxies reuse existing sprites without CPU texture reads.
- Confirmed OutlineFx's override vertex/fragment path ignores SpriteRenderer tint alpha while sampling sprite texture alpha, so transparent proxies remain available to its mask pass.
- Confirmed against the installed URP 14 source that `ScriptableRendererData.rendererFeatures`, `ScriptableRendererFeature.isActive`, and `SetActive` used by the setup utility are available.
- Parsed both package JSON files successfully and ran `git diff --check`; only existing LF-to-CRLF working-copy warnings were reported.
- Unity initially failed to resolve the Git dependency because the GitHub connection was reset; the exact verified commit is now embedded so future project opens do not require GitHub access.
- First Play Mode run reported `Hidden/Internal-Loading: invalid pass index 1` because OutlineFx created its material while the embedded shader was still importing after the Library rebuild.
- Patched the embedded `OutlineFxFeature` to defer its pass until `Hidden/OutlineFx/Main` exposes all three passes and to rebuild a stale loading-placeholder material automatically.
- First visual test showed the outline following active-piece Alpha along internal group seams instead of consistently matching the player-facing gray puzzle boundary.
- Added transparent `OutlineBlocker` proxies for every non-active puzzle group so OutlineFx suppresses group-to-group seams while retaining the active mask's exposed outer boundary.
- The next screenshot clarified that 1-2 pixel Alpha gaps between pieces in the same active group were still producing unwanted interior lines; only the two segments touching the visible outer border should remain.
- Added a GPU morphological closing stage to the embedded OutlineFx mask before its outline pass. The first 3x3 attempt only broke interior lines into fragments, so it was replaced with separable 7x7 dilation/erosion to close gaps up to roughly 6 screen pixels while restoring the original external mask extent.
- The 7x7 closing alone still left fragmented interior lines. The final pipeline now renders separate active-group and all-groups union masks, outlines only the active mask, then rejects every outline pixel covered by the closed union mask. This explicitly keeps only the intersection between the active boundary and the full puzzle exterior.
- Unity resolved the embedded package, compiled the integration, and serialized `ActiveGroupOutlineFx` into `Renderer2D.asset`.
- Play Mode reached `GameScene` and rendered the outline; the initial loading-shader error and first visual fit issue were captured from the Unity log and game screenshot.
- The loading-shader and non-active-group blocker fixes still need a clean recompile and Play Mode verification.

## Next Action

1. Exit Play Mode and wait for Unity to recompile the embedded OutlineFx and GameScene changes.
2. Clear Console, enter `GameScene` again, and confirm no new `Hidden/Internal-Loading` pass error appears.
3. Verify the active group displays a `#3f423e` outline without lines against non-active puzzle groups.
4. Complete a group and verify the outline rebuilds for the next group.
5. Complete the puzzle and verify the outline is removed before `RewardPanel` appears.

## Resume Prompt

Continue the Puffies OutlineFx integration. Read `AGENTS.md`, `Documents/WORKFLOW.md`, and `Documents/CURRENT_TASK.md` first, then resolve the package in Unity and verify the active group outline in Play Mode.
