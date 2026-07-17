# Task Progress UI Refresh

## Requirements

### User Story

As a player, I want the shared task UI in MainScene and GameScene to reflect my persisted score progress so that the task text, numeric progress, and progress bar always agree with settlement data.

### Acceptance Criteria

1. WHEN MainScene starts AND the current task is `AccumulateScore` THEN the system SHALL display the current persisted progress and configured target in its shared `TaskItem`.
2. WHEN task progress is displayed THEN `TextProgress` SHALL show `displayValue / CompleteValue` and `ProgressMask` SHALL use the same value with its ratio clamped to `[0, 1]`.
3. WHEN GameScene settlement begins THEN the system SHALL display the pre-settlement task value before animating the newly earned score.
4. WHILE the settlement score rolls upward THEN `TaskScore`, `TextProgress`, and `ProgressMask` SHALL update from the same animated score value and finish in the same frame.
5. WHEN the updated task is incomplete THEN GameScene SHALL keep `TaskItem` visible with the updated progress and SHALL NOT grant a reward.
6. WHEN the updated task is complete THEN GameScene SHALL show the completed task wording, grant its reward exactly once, and advance the task exactly once.
7. IF a scene is missing optional task UI nodes THEN the system SHALL log a warning without blocking score persistence, task reward, or task advancement.

## Design

- Add `TaskProgressUIUtility` under `Assets/Scripts/View` as the single binding implementation for both scene instances of the shared prefab.
- Resolve `TaskContent`, `TextProgress`, `ProgressMask/Progress`, `BagBg/BagIcon`, and `BagBg/TextAddNum` relative to the supplied `TaskItem` root.
- Derive the full progress width from the authored `Progress` child width so repeated refreshes do not lose the original width after shrinking the mask.
- Refresh MainScene once during `Start`; returning from GameScene creates MainScene again and therefore reloads the latest persisted task.
- Convert GameScene task settlement to a coroutine. Persist the score and perform reward/advance business logic before the first animation frame, while rendering from captured pre/post-settlement values.
- Use an initial score-roll duration of 0.8 seconds with unscaled time. This is an adjustable presentation constant, not a scoring rule.
- This iteration animates the currently implemented base score only. Later bonus steps can call the same progress-update path.

## Tasks

- [x] Add the shared TaskItem binding utility.
- [x] Refresh MainScene task UI from current task data.
- [x] Animate GameScene settlement task progress and total score together.
- [x] Preserve completion reward and task advancement behavior.
- [x] Update project notes and compile runtime and Editor assemblies.

## Out Of Scope

- Hint, level-outline, sticker-outline, and completion-time bonus calculation.
- Multi-step bonus reveal sequencing.
- Scene or prefab layout changes.
