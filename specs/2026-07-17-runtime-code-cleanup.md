# Runtime Code Cleanup

## Requirements

### User Story

As a maintainer, I want recently added task, scoring, settings, and settlement UI code consolidated so that the same behavior has fewer duplicate branches and lookups.

### Acceptance Criteria

1. WHEN task completion is evaluated THEN the system SHALL preserve current completion, overflow carry, reward, and persistence behavior while avoiding duplicate completion checks.
2. WHEN GameScene settles a completed task THEN the shared TaskItem SHALL be bound once before animation rather than fully rebound during reward output.
3. WHEN settlement score UI animates THEN cached UI references SHALL replace repeated RewardPanel hierarchy lookups.
4. WHEN usable settings are saved THEN `UsableOption1/2/3` serialized fields and public setter behavior SHALL remain compatible.
5. WHEN score is calculated THEN all base scores, bonus percentages, time boundaries, and upward rounding SHALL remain unchanged.
6. WHEN code is removed or merged THEN no Scene, Prefab, Canvas, CSV, or persisted field name SHALL change.

## Design

- Make `TryCompleteAndSetNextTaskId` perform one current-task config read and one completion condition; let `TryCompleteAndAdvanceTask` delegate directly.
- Cache GameScene RewardPanel references for TaskItem, TaskScore, TaskBagNum, and the reward image during panel configuration.
- Remove `UpdateTaskRewardPanel`; the earlier `TaskProgressUIUtility.RefreshTask` already binds completed wording and reward data.
- Keep only the external reward-pack image update in a narrowly scoped helper.
- Inline the unused `TryGetCardPackBaseScore` wrapper into full score calculation and make the time-tier helper internal-only.
- Consolidate usable-option persistence behind one indexed private method while preserving public setters.
- Apply the progress TMP font only during full TaskItem refresh, not on each animation frame.

## Tasks

- [x] Consolidate task completion and overflow logic.
- [x] Remove duplicate GameScene settlement UI work and cache references.
- [x] Simplify score and settings utilities.
- [x] Remove per-frame redundant font application.
- [x] Update project notes and compile runtime and Editor assemblies.
