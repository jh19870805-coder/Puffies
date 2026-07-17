# Current Task

- Task: Refresh shared task progress UI in MainScene and GameScene
- Status: Completed
- Updated At: 2026-07-17

## User Intent

- Make MainScene and GameScene display the accumulated-score task's real persisted progress.
- Keep task progress text and progress-bar width synchronized.
- During GameScene settlement, roll the page score and task progress together.

## Working Notes

- Both scenes use the same `TaskItem.prefab`; runtime binding is centralized in `TaskProgressUIUtility`.
- MainScene refreshes `TaskContent`, `TextProgress`, `ProgressMask`, reward icon, and reward count during `Start`.
- GameScene keeps TaskItem visible for incomplete score tasks instead of hiding it.
- GameScene captures progress before and after settlement, then rolls `TaskScore` from 0 to the base score while task progress rolls by the same value.
- The initial animation duration is 0.8 seconds and uses unscaled time.
- Completed task reward and advancement are saved before animation, so leaving the page early cannot lose business state.

## Files Changed

- `Assets/Scripts/View/TaskProgressUIUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `specs/2026-07-17-task-progress-ui-refresh.md`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Decisions

- Derive the progress bar's full width from the authored `Progress` child rather than the mutable `ProgressMask` width.
- Clamp only the visible bar ratio; keep the numeric text tied to the actual displayed task value.
- Do not modify MainScene, GameScene, TaskItem.prefab, or Canvas layout data.
- Keep bonus calculation and multi-step bonus presentation outside this UI binding task.

## Validation

- Confirmed shared prefab paths: `TaskContent`, `ProgressBg/TextProgress`, `ProgressBg/ProgressMask/Progress`, `BagBg/BagIcon`, and `BagBg/TextAddNum`.
- Confirmed GameScene settlement score path: `RewardPanel/TaskBg2/TaskScore`.
- Compiled `Assembly-CSharp-firstpass`, `Assembly-CSharp`, and `Assembly-CSharp-Editor` successfully with local .NET MSBuild.
- Confirmed no Scene, Prefab, Canvas, or generated C# project file is part of this change.
- Interactive Unity play-mode verification remains to be performed.

## Next Action

1. Open MainScene and confirm the task shows the persisted value and matching progress width.
2. Complete a puzzle in GameScene and confirm `TaskScore`, `TextProgress`, and `ProgressMask` finish together after 0.8 seconds.
3. Test one incomplete settlement and one task-completing settlement, including return to MainScene.

## Resume Prompt

Continue the Puffies task progress UI workflow. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/GAME_DESIGN_REQUIREMENTS.md, and specs/2026-07-17-task-progress-ui-refresh.md first.
