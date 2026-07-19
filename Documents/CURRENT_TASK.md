# Current Task

- Task: Implement deterministic staged card pack distribution
- Status: Completed
- Updated At: 2026-07-19

## User Intent

- Implement the clarified initial, mid-to-late, and final chapter hand-count controls, then tune them through actual playtesting.
- During active development, apply persistence and SQL schema changes directly without supporting old local data; identify the old local files that must be deleted afterward.

## Working Notes

- Removed the provisional 50% random roll; first-completion grants are deterministic when the current stage gate is open.
- Initial `R>=9`: grant at `H<=5`, producing at most `H=6`.
- Mid transition `R=8`: grant at `H<=3`, producing at most `H=4`.
- Remaining mid stage `R=7..3`: grant at `H<=2`, producing at most `H=3`.
- Final `R=2..1`: grant at `H<=1`, producing at most `H=2`.
- A blocked first-completion grant is skipped. A blocked task reward is persisted and retried later instead of being lost.
- Pending task rewards are deduplicated by TaskId and attempted before the current first-completion grant.
- Task reward delivery occurs only after task advancement is persisted; an advancement failure leaves the entitlement queued and suppresses delivery for that settlement.
- Ordinary replay does not release old pending rewards unless that replay completes a task and creates a valid task reward attempt.
- Simplified the SQLite `CardPacks` table to `PackId`, `PackSize`, `LifecycleState`, and `UnlockTime` only.
- Removed the old `IsUnlocked` / `IsPlayed` columns and the initialization-time migration, backfill, and synchronization logic.

## Files Changed

- `Assets/Resources/Configs/CardPacks.csv`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/SqliteLocalStore.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/WORKFLOW.md`
- `Documents/Sticker_Puzzle_功能概览整理.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Persist pending task rewards in SQLite `AppRecords` as `CardPackDistribution/Progress`.
- Preserve guaranteed task reward entitlement while allowing chapter gates to delay delivery.
- Use the special `R=8,H=3 -> H=4` transition once; after `R` becomes 7, require `H<=2` for the next grant.
- Continue deterministic candidate selection by ascending chapter `Index`, with TaskConfig RewardId as a preferred locked candidate.
- Do not carry migration or legacy-schema code during active development. Reset local persistence after incompatible schema changes.

## Validation

- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- Executed 13 deterministic boundary cases covering `R=17/9/8/7/3/2/1/0`, all gate limits, and the mid-stage transition; all passed.
- `git diff --check` completed without whitespace errors.
- SQLite pending-queue persistence and complete settlement behavior still require Unity Play Mode regression.
- The schema cleanup compiled successfully, but it requires a clean local database before Play Mode testing.

## Next Action

1. Close Unity and delete `LocalData.db` and `LocalData.json` under `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/`, then reopen the project.
2. Play through clean-save initial-stage growth and verify five task completions move H from 1 to 6.
3. Seed `R=8,H=5..6` and verify draining, the one-time H=3 to H=4 transition, then H=2 to H=3 cycling.
4. Seed `R=2,H=1` and verify final-stage H=1 to H=2 cycling until R=0.
5. Verify blocked task rewards survive restart and release later without duplication.

## Resume Prompt

Continue Puffies deterministic chapter distribution playtesting. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the staged H/R and pending-task-reward Play Mode regression or follow the user's latest instruction.
