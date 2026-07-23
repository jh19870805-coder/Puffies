# Home Card-Pack Effects

- Status: Completed
- Scope: Replace MainScene card-pack cover images with persistent lightweight 3D displays and preserve the authored opening transition.

## Requirements

**User Story:** As a player, I want every available card pack on the home page to appear as a living card-pack effect, so that selecting and opening a pack feels like one continuous presentation.

1. WHEN MainScene finishes creating the package list THEN every visible package SHALL show its real cover on the card-pack effect instead of the `PackCover` image.
2. WHILE a package is idle THEN its effect SHALL play a subtle looping breathing scale animation.
3. WHEN a package is clicked THEN its breathing animation SHALL stop and the same visual SHALL scale from the list size to the original `600 x 680` design size.
4. WHEN the scale-up completes THEN the system SHALL play the existing six-layer card-pack opening animation with the selected real cover.
5. WHEN the opening animation completes THEN the system SHALL enter GameScene for the selected PackId.
6. WHILE effects replace covers THEN `PackSize` SHALL retain its existing RectTransform position, sibling order, Sprite, and completed-state tint.
7. WHEN an effect cannot be created THEN the system SHALL keep the static cover and existing click fallback functional.
8. WHEN card packs move with the ScrollRect THEN their effects SHALL remain aligned to their `PackCover` RectTransforms and SHALL not render outside the package viewport.
9. WHEN MainScene is destroyed or its list is rebuilt THEN all generated display objects and meshes SHALL be released.
10. WHEN a pack is pending reward-flight presentation THEN its home display SHALL stay suppressed until the existing reveal callback runs.

## Design

### Decision: Shared idle mesh plus one animated opener

**Context:** The delivered card pack is composed of six SkinnedMeshRenderer/Animator layers. MainScene can eventually contain about 150 packs, with 18 shown per page.

**Options considered:**

1. Six live animated layers per pack. Simple composition, but up to 900 skinned layers and Animators is not acceptable.
2. One RenderTexture camera per pack. Preserves UGUI clipping, but requires many cameras and render targets.
3. One shared frame-zero mesh for idle displays plus one reusable six-layer animated opener. Keeps the same authored geometry and dynamic covers with bounded animation cost.

**Decision:** Use option 3.

### Runtime flow

1. `GameAnimationUtility` builds one shared idle Mesh from the six animated layers sampled at frame zero.
2. Each `PackageEntry` owns one lightweight MeshRenderer display using the shared Mesh and material, with its own cover/tint property block.
3. MainScene LateUpdate aligns enabled displays to their UI anchors, applies a `0.98..1.02` breathing multiplier over `2.4s`, and passes the viewport screen clip rectangle to the shader.
4. Clicking a pack switches from its idle MeshRenderer to the reusable six-layer animated effect at the identical pose.
5. MainScene interpolates the animated effect scale to `600 / 240 = 2.5` over `0.3s`, then starts the authored animation and waits for its clip duration.
6. The static `PackCover` and `PackShadow` remain data/fallback objects but are hidden after a display is ready. `PackSize` remains unchanged during idle and is hidden only for the selected pack during opening.

### Rendering and clipping

- MainScene keeps its existing ScreenSpaceCamera conversion so world models appear over the root UI.
- `CardPackOpening.shader` receives `_UiClipRect` and `_UseUiClipRect` per renderer and clips fragments outside the ScrollRect viewport.
- Idle displays outside the viewport are renderer-disabled to avoid unnecessary draw calls.
- Existing UI interaction remains on the transparent `PackItem` root Image; click validation no longer depends on `PackCover.enabled`.

### Failure handling

- If the shared idle Mesh or animated effect cannot be created, keep `PackCover` and `PackShadow` visible.
- If the animated opener fails after click, restore/use the existing UI scale fallback and still enter GameScene.
- No persistence schema or player data changes are required.

## Tasks

- [x] Inspect package list, ScrollRect, canvas conversion, effect lifecycle, and animation resources.
- [x] Add shared idle-mesh display APIs and reusable selected-pack animation pose APIs.
- [x] Add shader viewport clipping properties.
- [x] Bind one idle display to each package entry and update alignment/breathing/visibility.
- [x] Change click flow to stop breathing, scale to `600 x 680`, play opening, and enter GameScene.
- [x] Preserve static fallback, reward-flight suppression, PackSize placement/layer, and cleanup.
- [x] Perform MainScene Play Mode visual validation and compile all C# assemblies.
- [x] Update `Documents/CURRENT_TASK.md` and stable project context.

## Implementation Validation

- Full compilation: `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- MainScene creates lightweight frame-zero displays from one shared mesh, applies each pack's real cover and lifecycle tint, and drives the `0.98..1.02` breathing scale in `LateUpdate`.
- The existing `PackSize` Image remains authoritative. Its RectTransform, Sprite, and tint are unchanged; a clipped world overlay mirrors that same Image only to preserve its visual foreground layer over the world-space card.
- Click handling prepares the reusable six-layer opener at the idle pose, scales it by exactly `min(600/240, 680/272) = 2.5`, starts the authored animation, waits for its longest clip, and enters the selected GameScene.
- Static cover/shadow fallback, reward-flight suppression/reveal, viewport clipping, off-page renderer disabling, and generated object/mesh cleanup are implemented.
- Automated MainScene Play Mode validation passed with four generated idle displays. PackId 1 breathed from scale `2.449215` to `2.52881527`, retained its PackSize authoring data, enlarged to opening scale `6.246120`, started six Animators/six Renderers, and entered GameScene.
- Captured and visually inspected `2560 x 1440` idle/opening frames. The list effects use the correct covers and size badges; the selected cover enlarges and visibly enters the authored tear animation without overlap corruption.
- A final Unity batch refresh exited successfully with no C# or Shader compile errors.

