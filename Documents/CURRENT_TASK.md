# Current Task

- Task: Support updated package ScrollView UI
- Status: In Progress
- Updated At: 2026-07-12

## User Intent

- Support the newly edited package list UI in `MainScene`.
- Use the `PackageScrollView -> Viewport -> Content -> Page_1` layout for displaying card packs.
- Keep package clicking functional while allowing ScrollView dragging.

## Working Notes

- `GameScene.unity` did not contain the package list change; the edited ScrollView is in `MainScene.unity`.
- The new scene structure has `PackageScrollView`, `Content`, and `Page_1`.
- `PackItem.prefab` contains `Cover` and `NameText`.
- Runtime code now prefers the new paged ScrollView layout, and falls back to the old `Package001` image template if the new layout is absent.
- The new page model uses 18 packages per page and creates additional `Page_N` objects as needed.
- Pack icons use original 600x680 source textures and should be proportionally fitted inside the MainScene package list cells.
- Package list should start from the upper-left and place pack icons horizontally before wrapping to the next row.

## Files Changed

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/View/PackageInteractionHandler.cs`
- `Documents/CURRENT_TASK.md`

## Decisions

- Use `PackageScrollView` as the new primary package list container.
- Use `Page_1` as the page template under ScrollView `Content`.
- Use `Assets/Prefabs/PackItem.prefab` in Editor; fall back to a runtime-created simple item if the prefab is unavailable.
- Treat short pointer movement as click-to-open, and forward drag events to `ScrollRect` for list scrolling.
- Runtime package item size is 240x272, matching the 600x680 source icon aspect ratio at 40% scale.
- `PackageScrollView` content, page, and grid layout settings are normalized at runtime to upper-left horizontal layout.
- Each page uses a 6-column by 3-row grid, so items fill left-to-right from the upper-left before creating the next horizontal page.
- `NameText` is hidden in runtime package items so it does not overflow the icon cell.

## Validation

- Static-checked that `MainScene.unity` contains `PackageScrollView`, `Content`, and `Page_1`.
- Static-checked that `PackItem.prefab` contains `Cover` and `NameText`.
- Static-checked that scene `Page_1` is serialized as 0x0 and can be affected by old centered alignment, so runtime now overrides page and grid layout values.
- Ran `git diff --check` for changed scripts; no script whitespace errors.
- Full `git diff --check` still reports existing Unity scene trailing whitespace in `Assets/Scenes/MainScene.unity`; the script fix does not rewrite that scene.
- Tried locating a Unity executable from command line; none was available, so Play Mode UI screenshot validation was not run.

## Next Action

1. Open `MainScene` in Unity and enter Play Mode.
2. Verify package items instantiate under `PackageScrollView/Viewport/Content/Page_1`.
3. Verify horizontal drag scrolls pages instead of opening a package.
4. Verify tapping a package still plays the pack animation and enters `GameScene`.

## Resume Prompt

Continue Puffies updated package ScrollView UI work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify the MainScene package list in Unity Play Mode.
