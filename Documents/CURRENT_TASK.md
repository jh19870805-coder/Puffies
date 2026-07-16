# Current Task

- Task: Center active puzzle area above the piece tray and verify OutlineFx
- Status: In Progress
- Updated At: 2026-07-16

## User Intent

- Use a reliable free third-party Unity plugin for the current puzzle target outline.
- The outline should identify the boundary of the active puzzle group in `GameScene`.
- Use color `#3f423e` and start near 3 pixels wide.
- Center the current puzzle area inside the background region above `PieceBoard`, excluding the bottom piece tray from the centering area.

## Working Notes

- Selected `NullTale/OutlineFx`, licensed under MIT, and embedded its verified upstream commit in the project.
- The active groove objects are transparent UGUI `Image` components, while OutlineFx accepts `Renderer` components.
- `GameScene` now creates transparent world-space `SpriteRenderer` proxies from the active groove sprites.
- OutlineFx renders all proxies into one screen-space mask before outlining, so adjacent pieces form one group outline instead of separate per-piece outlines.
- Proxy rendering samples sprite alpha on the GPU and does not require readable source textures.
- `OutlineFxRendererSetupEditor` automatically adds and configures `ActiveGroupOutlineFx` on `Assets/Settings/Renderer2D.asset` after Unity resolves the package.
- `GameScene` now centers the active group between the viewport top and the top edge of the piece tray, rather than against the full viewport height.
- The centering uses runtime world bounds, so it follows the authored `PieceBoard` height and remains responsive across resolutions; the generated tray fallback uses the same rule.

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
- Configure a centered hard box outline, no solid fill, 50% alpha cutoff, no depth attachment, and radius thickness `0.0375`, which targets roughly 3 pixels total at the 1440-pixel design height.
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
- Adding the Renderer Feature produced the same `ArgumentException: Invalid path` twice from `OutlineFxFeature._validateContent`, once through `OnValidate` and once through `OnEnable`.
- Patched the embedded package's editor-only default-pattern lookup to tolerate a temporarily empty Shader asset path and to load the correctly cased `Checker.png`; it now marks the feature dirty only when the texture is actually assigned.
- Unity rebuilt `OutlineFx.dll` after the path fix; the subsequent domain reload logged no new `ArgumentException: Invalid path` and no C# compilation errors.
- A later Play Mode run exposed `Hidden/Internal-Loading: invalid pass index` errors for passes 1 and 3-7. The feature was still creating a material during `Create()` while the package Shader was importing, and the render pass used fixed numeric indexes.
- Changed OutlineFx to create its material lazily, verify the real Shader and every required named pass before enqueue/execute, and resolve pass indexes by name instead of hardcoded numbers. A not-yet-ready Shader now skips the frame without submitting invalid commands.
- Repairing the empty `RangeFloatDrawer.cs.meta` exposed that the orphaned editor drawer referenced a missing `RangeFloat` type. No runtime or editor code uses that type, so its implementation was replaced with an inert tracked placeholder instead of adding a fake dependency. Keeping the file path also supports Unity's cached compilation graph.
- Unity rebuilt the OutlineFx runtime assembly after the Shader lifecycle fix; a clean full-project recompile is required after neutralizing the orphaned drawer.
- Ran Unity's generated `OutlineFx.Editor.rsp` and `OutlineFx.Editor.rsp2` directly through the same Roslyn compiler; compilation completed with exit code 0 and no `RangeFloat` error.
- Unity had also auto-reassigned `PackIcon003.png.meta` after detecting a duplicate GUID with `PackIcon002`; neither GUID is referenced by scenes or prefabs, and pack images are loaded by disk path, so the reassignment does not break runtime loading.
- Static-checked the new centering order: camera sizing completes first, the tray is aligned, then the active group is translated to the center of the remaining upper area without changing prefab-internal piece coordinates.
- Compiled `Assembly-CSharp` with Unity's generated Roslyn response files after the centering change; compilation completed with exit code 0.
- The screenshot showed the dark generated line running parallel to the lighter authored seam. Proxy transforms match the same sprite scale and snap position used by gameplay; the mismatch came from OutlineFx drawing its full stroke only outside the Alpha mask.
- Changed the shader to build a centered boundary band from dilation minus erosion. The final full-puzzle mask now keeps the intersection of centered active/full-puzzle boundaries, preserving internal-seam suppression while placing the dark stroke over the actual Alpha edge.
- Halved the configured radius from `0.075` to `0.0375` so the centered band retains the requested total width instead of doubling it.
- Compiled `Assembly-CSharp-Editor` with Unity's generated Roslyn response files after updating the renderer setup; compilation completed with exit code 0. The latest Shader visual result still requires a fresh Play Mode run.
- The centered Alpha stroke remained offset because the authored light seam is part of the rendered `GameBoard` artwork and does not match the independently cut piece Alpha geometry.
- The final OutlineFx pass now copies the clean camera image, uses the Alpha boundary only as an 8-pixel search window, detects the rendered seam by local color contrast, and expands matching pixels to an approximately 3-pixel stroke. This remains GPU-only and requires no readable textures or per-pack masks.
- Added renderer setting `_colorEdgeThreshold=0.1` as the single tuning value for color-edge sensitivity. Both `OutlineFx` and `Assembly-CSharp-Editor` compiled with Unity's generated Roslyn response files with exit code 0; Unity still needs to import the latest Shader and run Play Mode for visual validation.
- The first color-edge run only detected high-contrast fragments on the left while missing the softly blended top boundary. Replaced the 1-2 pixel comparison with averaged samples 3, 5, and 7 pixels to either side of each candidate and lowered `_colorEdgeThreshold` to `0.04`; this targets the broad gray/background transition instead of local antialiasing and texture noise.
- Unity imported the multi-scale color detector without Shader, pass, or C# errors. `OutlineFx` and `Assembly-CSharp-Editor` also compiled with exit code 0 after the adjustment; visual continuity around the top and left boundary remains to be checked in Play Mode.
- The multi-scale detector found most of the correct contour but produced dotted texture responses just inside it. The detector now derives an approximate boundary normal from the closed full-puzzle mask, compares colors only across that normal, and averages responses along the tangent before expanding the line. The threshold is now `0.03` to improve continuity after directional noise rejection.
- Added a fine 1-2 pixel directional gradient gate on top of the broad 3/5/7-pixel region contrast. Broad contrast confirms the gray/background transition, while fine contrast localizes its center so the output does not become a wide band.
- Unity imported the directional detector without Shader, render-pass, or C# errors. `OutlineFx` and `Assembly-CSharp-Editor` compiled with exit code 0; the updated continuity and noise rejection still require a fresh Play Mode visual check.
- The directional color detector regressed visually because authored texture noise and antialiasing prevent a single threshold from producing both a continuous and clean contour. Removed camera-color sampling and all color-threshold settings instead of continuing to tune unstable heuristics.
- The current deterministic pipeline closes both the active and full-puzzle masks, expands both masks by 3 screen pixels, outlines the expanded active mask, then intersects it with the expanded full-puzzle exterior. No mask inset remains.
- Unity imported the two new inset Shader passes without Shader, pass, or C# errors. `OutlineFx` and `Assembly-CSharp-Editor` compiled with exit code 0; visual validation of the 6-pixel inset remains pending.
- The 6-pixel inset produced a clean continuous contour but placed it consistently inside the visible seam. Reduced the uniform inset to 2 pixels, moving the entire contour outward by 4 screen pixels without changing line width or boundary filtering.
- Unity reimported the 2-pixel inset Shader without Shader or pass errors; visual alignment remains to be checked in a fresh Play Mode run.
- Per the latest direction, removed mask inset entirely and replaced it with a uniform 3-pixel outward expansion for both active and full-puzzle masks. Line width and internal-seam filtering are unchanged.
- Unity imported both expansion passes without Shader or invalid-pass errors, and `OutlineFx` compiled with exit code 0. Visual alignment of the 3-pixel expansion remains pending.
- Cross-pack screenshots showed the generated contour consistently translated left and down, which is a proxy registration error rather than a mask-radius error. Added a uniform 6-screen-pixel right/up offset to active and blocker outline proxies only; gameplay piece and snap coordinates remain unchanged.
- `Assembly-CSharp` compiled with Unity's generated response files after the proxy offset change, and the latest Editor log contains no C#, Shader, or pass errors. Visual registration remains to be checked in a fresh Play Mode run.
- Group 2 could move the board far into the upper-right because its bounds were captured before changing the orthographic camera size, then reused after the screen-to-world scale changed. `FitCameraToActiveGroup` now recomputes the active-group bounds after camera fitting and uses only the fitted-coordinate bounds for centering.
- `Assembly-CSharp` compiled with Unity's generated response files after the group-centering fix, and the latest Editor log contains no C# or rendering errors. The group 1 to group 2 transition still requires Play Mode validation.
- Recomputing camera-world bounds did not change the second-group result because `GameBoard`, grooves, and `PieceBoard` are Screen Space Camera UI under the same Canvas; world conversion was the wrong abstraction. Centering now unions the active groove rectangles in screen coordinates, computes the target from the background top and `PieceBoard` top, converts both screen centers into the card-bag parent's local space, and applies that local delta directly.
- The screen/local-space centering implementation compiled in `Assembly-CSharp` with exit code 0, and the latest Unity log contains no runtime or rendering exceptions. Group transition visual validation remains pending.
- OutlineFx cannot render UGUI `Image` objects directly, so the outline is represented by independent world-space SpriteRenderer proxies rather than actual board children. Their positions were previously synchronized only at creation, allowing board movement or deferred Canvas updates to desynchronize them.
- `GameScene.LateUpdate` now keeps every active/blocker proxy position and scale synchronized to its source groove RectTransform before rendering. Board centering also forces a Canvas update immediately after changing the CardBag anchored position, and proxy mappings are cleared with the outline root during group changes.
- `Assembly-CSharp` compiled with exit code 0 after adding proxy following, and the latest Unity log contains no C# or rendering exceptions. Board movement plus group-transition visual validation remains pending.

## Next Action

1. Exit Play Mode, let Unity recompile, then enter `GameScene` and verify the active group is centered in the area above `PieceBoard`.
2. Complete a group and verify the next group is centered in the same upper area without changing snap alignment.
3. Confirm neither `ArgumentException: Invalid path` nor `Hidden/Internal-Loading` pass errors appear.
4. Verify the expanded `#3f423e` stroke tracks the lighter authored seam continuously and still omits lines against non-active puzzle groups; adjust only the uniform expansion radius if needed.
5. Complete the puzzle and verify the outline is removed before `RewardPanel` appears.

## Resume Prompt

Continue the Puffies OutlineFx integration. Read `AGENTS.md`, `Documents/WORKFLOW.md`, and `Documents/CURRENT_TASK.md` first, then resolve the package in Unity and verify the active group outline in Play Mode.
