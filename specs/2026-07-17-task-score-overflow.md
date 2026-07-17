# Task Score Overflow Carry

## Requirements

### User Story

As a player, I want score beyond a completed task target to carry into the next score task so that earned settlement score is never discarded.

### Acceptance Criteria

1. WHEN an accumulate-score task has progress above its target THEN task advancement SHALL carry `CurrentCompleteValue - CompleteValue` into the next accumulate-score task.
2. WHEN progress equals the target exactly THEN the next task SHALL start at 0.
3. WHEN either the completed task or next task is not `AccumulateScore` THEN task advancement SHALL NOT carry score.
4. WHEN task advancement is saved THEN task id and carried progress SHALL be persisted together in the existing `TaskProgressData` JSON object.
5. WHEN MainScene starts after advancement THEN its shared TaskItem SHALL display the carried progress.
6. WHEN task id is changed manually through `SetCurrentTaskId` THEN progress SHALL continue to reset to 0.

## Design

- Update `TryCompleteAndSetNextTaskId` to read both current and next task configs before mutating state.
- Carry only the non-negative overflow when both configs use `TaskType.AccumulateScore`.
- Assign the next task id and carried value before the single existing `SaveTaskProgress` call.
- Keep GameScene settlement UI on the completed task's captured values; MainScene will display the new task and carried value after returning.

## Tasks

- [x] Implement type-safe score overflow carry.
- [x] Preserve manual task-switch reset behavior.
- [x] Update project notes and compile runtime and Editor assemblies.

## Example

```text
Task 1 target: 200
Progress after settlement: 228
Task 2 initial progress: 228 - 200 = 28
```
