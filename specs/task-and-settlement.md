# Task And Settlement

- Status: Implemented; Play Mode regression pending
- Scope: Accumulated-score tasks, settlement scoring, shared task UI, rewards, and overflow carry

## Requirements

1. `TaskType=1` represents `AccumulateScore`; Piece placement does not directly increase task progress.
2. GameScene adds the final settlement score to the active score task exactly once, then evaluates completion, grants the configured reward, and advances the task exactly once.
3. Base scores by card-pack size are XS 60, S 80, M 100, L 120, XL 140, XXL 160, and XXXL 200.
4. Qualified bonuses are additive: no hint +5%, level outline disabled +2%, sticker outline disabled +5%, and completion time `<=15` / `<=30` / `<=60` seconds +3% / +2% / +1%.
5. The final score is `ceil(baseScore * (1 + totalBonusPercent / 100))`.
6. Timing starts on the first successful Piece placement and freezes when settlement begins. Clicking `BtnTips` at any time during the game disables the no-hint bonus.
7. MainScene `Toggle1` and `Toggle2` values come from persisted `UsableOption1` and `UsableOption2`; `Toggle3` does not affect score.
8. MainScene and GameScene use the shared `TaskItem` binding. `TextProgress`, `ProgressMask`, and settlement `TaskScore` must animate from the same score value.
9. Settlement `TaskBagNum` displays the current unlocked card-pack count, including a pack unlocked by the current reward.
10. Score above a completed accumulate-score task target carries into the next accumulate-score task. Manual task-id changes still reset progress to zero.
11. Existing numeric enum values and persisted fields, including `CurrentCompleteValue` and `UsableOption1/2/3`, remain compatible.

## Design

- `GameScoreUtility` owns base-score mapping, bonus calculation, time tiers, and upward rounding.
- `GameTaskUtility` owns persisted progress, completion, reward transition, and overflow carry.
- `TaskProgressUIUtility` is the single binding path for the shared MainScene and GameScene `TaskItem` instances.
- GameScene snapshots settings at session start, tracks hint and timing state, persists settlement before presentation, and animates captured pre/post values.
- RewardPanel references are cached during configuration; optional missing UI nodes warn without blocking score persistence or task advancement.
- MainScene panel visibility and usable-option persistence share internal helpers while retaining semantic public setters.

## Validation

- Runtime, first-pass, and Editor assemblies compile successfully.
- Removed helpers and the old task-progress API have no remaining callers.
- Scenes, prefabs, Canvas values, CSV files, enum numeric values, and persisted field names were not changed.
- Play Mode still needs verification for settings persistence, score animation, bag count, reward output, and overflow carry.

