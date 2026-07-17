# Accumulate Score Task Migration

## Requirements

### User Story

As a player, I want task progress to accumulate the score earned from completed puzzle games so that task completion reflects settlement score rather than the number of placed Pieces.

### Acceptance Criteria

1. WHEN task config contains `TaskType=1` THEN the system SHALL interpret it as `AccumulateScore`.
2. WHEN a Piece is placed THEN the system SHALL NOT change accumulate-score task progress.
3. WHEN a puzzle game enters settlement AND the current task is `AccumulateScore` THEN the system SHALL add that game's settlement score exactly once.
4. WHEN the score has been added THEN the system SHALL evaluate completion, grant the configured reward, and advance the task using the updated progress.
5. WHEN GameScene updates the task settlement UI THEN it SHALL resolve nodes from the shared `TaskItem` prefab using its authored child names.
6. WHEN existing task progress is loaded THEN the system SHALL preserve the existing `CurrentCompleteValue` field and `TaskType=1` numeric compatibility.

## Design

- Rename `TaskType.CollectPuzzle` to `TaskType.AccumulateScore` without changing numeric value `1`.
- Rename the type predicate and active-state field to score terminology.
- Remove the Piece-placement progress increment.
- Add a score-specific task API that persists an integer score delta through the existing `CurrentCompleteValue` storage field.
- During GameScene settlement, resolve the selected pack's `CardPackSize`, map it to the confirmed base score, add it to the active score task, then run completion/reward logic.
- Keep score calculation isolated so the later hint/outline/time bonus implementation can replace the base-only settlement input without changing task persistence.
- Resolve `TaskItem`, `TaskContent`, `BagBg/BagIcon`, and `BagBg/TextAddNum` relative to the shared prefab instance.

## Tasks

- [x] Rename the task enum and utility predicate while preserving serialized values.
- [x] Add the card-pack-size base-score mapping.
- [x] Remove Piece-based progress and add settlement-based score progress.
- [x] Update GameScene task reward UI paths and score-task wording.
- [x] Update project documentation.
- [x] Compile runtime and Editor assemblies and verify no old task symbols remain.

## Out Of Scope

- Hint, level-outline, sticker-outline, and completion-time bonus tracking.
- Sequential settlement bonus presentation and score-roll animation.
- MainScene shared TaskItem runtime binding.
