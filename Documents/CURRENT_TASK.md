# Current Task

- Task: Carry excess score into the next task
- Status: Completed
- Updated At: 2026-07-17

## User Intent

- Preserve score above a completed task target instead of resetting all progress to zero.
- Example: 228 progress completing a 200-point task must start the next task at 28.

## Working Notes

- `TryCompleteAndSetNextTaskId` now calculates overflow before changing the current task id.
- Overflow carries only when both the completed task and next task are `AccumulateScore`.
- Task id and carried progress are written together through the existing `SaveTaskProgress` call.
- MainScene already reads persisted progress during `Start`, so it displays the carried value without additional UI changes.
- Manual `SetCurrentTaskId` calls still reset progress to zero.

## Files Changed

- `Assets/Scripts/Model/GameTaskUtility.cs`
- `specs/2026-07-17-task-score-overflow.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Decisions

- Do not carry score into a future non-score task type.
- Do not automatically complete multiple tasks from one settlement; the next task receives the full overflow and follows the normal completion flow.
- Do not modify settlement score calculation, scenes, prefabs, or Canvas layout.

## Validation

- Confirmed the previous implementation called `ResetCurrentCompleteValue` during every completed-task transition.
- Confirmed completed-task transition now stores `Math.Max(0, currentProgress - completedTarget)` for consecutive score tasks.
- Confirmed manual task-id changes retain the old reset behavior.
- Compiled `Assembly-CSharp-firstpass`, `Assembly-CSharp`, and `Assembly-CSharp-Editor` successfully without warnings.
- Interactive Unity play-mode verification remains to be performed.

## Next Action

1. Complete a 200-point task with a 228-point settlement total.
2. Return to MainScene and confirm the next task displays `28/next target` with matching progress width.

## Resume Prompt

Continue the Puffies accumulated-score task workflow. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, and specs/2026-07-17-task-score-overflow.md first.
