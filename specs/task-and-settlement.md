# Progression And Settlement

- Status: Core implementation complete; sequential bonus presentation and Play Mode regression pending
- Scope: Score tasks, settlement, card-pack lifecycle, acquisition, list presentation, and persistence

## Confirmed Rules

### Scoring And Tasks

1. `TaskType=1` is `AccumulateScore`; Piece placement does not directly increase task progress.
2. Base scores are XS 60, S 80, M 100, L 120, XL 140, XXL 160, and XXXL 200. Card-pack size comes from `CardPacks.csv`.
3. Qualified bonuses are additive: no hint +5%, level outline disabled +2%, sticker outline disabled +5%, and completion time `<=15` / `<=30` / `<=60` seconds +3% / +2% / +1%.
4. Timing starts on the first successful Piece placement and freezes when settlement starts. Clicking `BtnTips` disables the no-hint bonus.
5. The final score is `ceil(baseScore * (1 + totalBonusPercent / 100))` and is added to the active score task exactly once.
6. A completed task grants its configured reward and advances exactly once. Overflow carries only into the next `AccumulateScore` task.
7. Settlement score, `TextProgress`, and `ProgressMask` use the same animated score value.

### Card-Pack Progression

1. Persisted lifecycle states are `Locked`, `Unlocked`, `InProgress`, and `Completed`.
2. Granting changes `Locked` to `Unlocked`; completing the first group changes a multi-group pack to `InProgress`; completing the final group changes it to `Completed`.
3. Replaying a completed pack does not repeat the first-completion grant attempt, but can still complete a task and create its guaranteed entitlement.
4. Task rewards create deduplicated pending entitlements. A blocked entitlement remains persisted for a later retry.
5. First completion performs one stage-gated grant attempt. Task and first-completion sources may grant two different locked packs in one settlement.
6. Internal chapters constrain the eligible locked-pack pool and are not shown to the player.

### MainScene And Reward Presentation

1. MainScene displays 18 card packs per page in a 6-column x 3-row grid.
2. One-presentation newly granted packs appear first, then `InProgress`, `Unlocked`, and `Completed` packs; timestamps and PackId provide deterministic ordering.
3. Completed covers and size icons are tinted gray but remain replayable.
4. RewardPanel keeps its authored `ImgBag` Sprite. On `BtnFinish`, all packs granted by that settlement move into a centered row, pause, survive scene loading, and fly to their MainScene list slots.
5. MainScene card-pack opening reuses one generic 3D model and its existing Animator Controller for every PackId.
6. Before playback, the model receives the selected pack's real `PackIconNNN.png` through per-renderer material properties; shared material assets are not modified.
7. The cover UV is center-cropped to the reference model texture aspect ratio, and the animated model is fitted to the clicked card-pack UI bounds and center.
8. Missing cover data may fall back to the authored model texture, but a missing pack-specific 3D prefab must not prevent the generic animation from playing.

## Current Implementation

- `GameScoreUtility` (stored with task progression in `GameTaskUtility.cs`) calculates the full final score and all individual bonus percentages.
- GameScene persists task progress and reward state before running its settlement presentation.
- `TaskProgressUIUtility` binds the shared MainScene/GameScene `TaskItem`.
- The current score presentation performs one 0-to-final roll over 0.8 seconds. It does not yet reveal each qualified bonus or animate cumulative score steps.
- `CardPackDistributionUtility` applies the current deterministic `R` / held-pack gates and stores pending task entitlements in SQLite.
- Current content contains 21 configured packs across chapter 1 and chapter 2; only five playable CardBag prefabs currently exist: 001, 002, 003, 008, and 017.
- Card-pack opening uses `CardPackSkin_001` and its existing animation as the generic model; PackId selects only the runtime cover texture rather than a `CardPackSkin_NNN` prefab.

## Persistence

- Task progress remains in `LocalData.json` under `TaskProgressData`.
- Card-pack lifecycle and distribution progress are stored in `LocalData.db`.
- The `CardPacks` table uses `PackId`, `PackSize`, `LifecycleState`, `UnlockTime`, and `CompletionTime`; old `IsUnlocked` and `IsPlayed` columns are not supported.
- After syncing from the old schema, close Unity and reset both local data files before a clean cross-store regression. Never delete them without explicit user permission.

## Pending Decisions

- Qualified-bonus reveal order, per-step timing/easing, and intermediate rounding.
- Exact hint failure semantics and outline-toggle qualification semantics.
- Final candidate selection and empty locked-pool behavior.
- Exact chapter membership, initial chapter state, and chapter advancement rules.
- Special-pack accounting and final pacing toward approximately 150 packs.

## Validation

- Current HEAD `2236f9f` builds all runtime, first-pass, and Editor assemblies with 0 warnings and 0 errors.
- Static implementation and persisted schema were cross-checked against `PROJECT_CONTEXT.md` and `GAME_DESIGN_REQUIREMENTS.md`.
- Full Play Mode regression remains pending.
