# Current Task

- Task: Add MainScene menu panel persistence
- Status: In Progress
- Updated At: 2026-07-12

## User Intent

- Support the newly edited `PanelSet` UI in MainScene.
- `PanelSet` contains music volume, effect volume, and windowed-mode controls.
- Persist those three settings locally.
- Support the newly added `PanelUsable` UI in MainScene.
- Bind `BtnUsable` to open `PanelUsable`, and persist its three toggle values locally.
- Support the newly added `PanelSave` UI in MainScene.
- Bind its menu entry to show `PanelSave`; only display/hide behavior is required for now.

## Working Notes

- `MainScene.unity` contains inactive `PanelSet` under the main Canvas.
- `PanelSet` contains `SliderMusic`, `SliderEffect`, `ToggleFrame`, `BtnClose`, and `BtnReturn`.
- `PanelMenu/BtnSet` opens `PanelSet`.
- `GameCommonUtility.FindSceneObject` can find inactive scene objects, so runtime binding can resolve `PanelSet` without scene edits.
- Scene slider roots were 160x20 while their background sprites were 258x52, and Fill/Handle children had fixed offsets that made the UI deform and prevented dragging cleanly to the left edge.
- The current fix uses editor-built fake sliders directly.
- `PanelSet/SliderMusic` and `PanelSet/SliderEffect` are Image roots with `SliderFill` and `SliderHandle` child Images.
- Runtime attaches `FakeSettingsSliderInput` to each root, updates `SliderFill` width and `SliderHandle` x-position during pointer down/drag, and saves the resulting value.
- `PanelUsable` contains `Toggle1`, `Toggle2`, and `Toggle3`; their concrete gameplay meaning is not defined yet, so they are stored as neutral usable option fields.
- `PanelSave` is inactive in `MainScene.unity` and contains close/return buttons.
- The scene currently uses `PanelMenu/BtnData` as the `PanelSave` entry button; there is no `BtnSave` object.

## Files Changed

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/View/FakeSettingsSliderInput.cs`
- `Assets/Scripts/Model/GameSettingsUtility.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Store settings in SQLite `AppRecords` collection/key `GameSettings/Runtime`.
- Keep `PanelSet` hidden during MainScene startup.
- Keep `PanelUsable` hidden during MainScene startup.
- Bind `PanelMenu/BtnSet` to open `PanelSet` and hide `PanelMenu`.
- Bind `PanelMenu/BtnUsable` to open `PanelUsable` and hide `PanelMenu`.
- Bind `PanelMenu/BtnData` to open `PanelSave` and hide `PanelMenu`.
- Bind `PanelSet/BtnClose` and `PanelSet/BtnReturn` to hide `PanelSet`.
- Bind `PanelUsable/BtnClose` and `PanelUsable/BtnReturn` to hide `PanelUsable`.
- Bind `PanelSave/BtnClose` and `PanelSave/BtnReturn` to hide `PanelSave`.
- Slider and toggle changes save immediately.
- `PanelUsable` toggles save immediately as `UsableOption1`, `UsableOption2`, and `UsableOption3`.
- Windowed toggle maps `true` to `Screen.fullScreen = false`.
- Audio settings are stored separately as music/effect volumes; existing AudioSources are applied by object name (`music`/`bgm` => music, others => effect).
- Fake settings sliders use the editor layout directly and only refresh fill width / handle position at runtime.

## Validation

- Static-checked that `MainScene.unity` contains inactive `PanelSet`, `SliderMusic`, `SliderEffect`, `ToggleFrame`, `PanelSet/BtnClose`, and `PanelSet/BtnReturn`.
- Static-checked that `PanelMenu` contains `BtnSet`.
- Static-checked that `MainScene.unity` contains inactive `PanelUsable`, `PanelMenu/BtnUsable`, and `Toggle1` / `Toggle2` / `Toggle3`.
- Static-checked that `MainScene.unity` contains inactive `PanelSave`, `PanelMenu/BtnData`, and `PanelSave` close/return buttons.
- Added `GameSettingsUtility` for local persistence and runtime application.
- Extended `GameSettingsUtility` with three persisted usable option booleans.
- Replaced standard Unity `Slider` usage with `FakeSettingsSliderInput` for the hand-built three-image sliders.
- Ran scoped `git diff --check` for touched scripts/docs; no whitespace errors. Git only reported existing LF-to-CRLF working-copy warnings.
- `dotnet`, `msbuild`, and command-line Unity were not available in this shell, so compile/Play Mode automation was not run.
- Unity Play Mode was not run from this shell.

## Next Action

1. Open `MainScene` in Unity and enter Play Mode.
2. Open menu, click `BtnSet`, and verify `PanelSet` opens.
3. Open menu, click `BtnUsable`, and verify `PanelUsable` opens.
4. Open menu, click `BtnData`, and verify `PanelSave` opens.
5. Toggle `Toggle1` / `Toggle2` / `Toggle3` and verify values persist after restarting Play Mode.
6. Move `SliderMusic` / `SliderEffect` and verify values persist after restarting Play Mode.

## Resume Prompt

Continue Puffies MainScene menu panel work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify the settings/usable/save UI and local persistence in Unity Play Mode.
