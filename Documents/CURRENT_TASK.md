# Current Task

- Task: Implement chapter-based card pack distribution
- Status: Completed
- Updated At: 2026-07-19

## User Intent

- Decide and grant new card packs after each completed game according to the internal chapter stages, while excluding replay from the probability reward.

## Working Notes

- Restored the four-state card pack lifecycle and backward-compatible SQLite migration because the current branch still contained the old boolean-only model.
- Added `ChapterId` to card-pack config. Current development data assigns PackIds 1-18 to chapter 1 and 19-21 to chapter 2.
- GameScene snapshots whether the selected pack was already completed, so replay never runs the first-completion reward roll.
- Completing the first group of a multi-group pack now persists `InProgress`; final completion persists `Completed`.
- First-completion probability is 100% below the stage target minimum, 50% inside the target band, and 0% at the maximum.
- Task completion always attempts to grant the next locked pack and is not blocked by held-pack stage caps.
- Task and first-completion rewards can both grant in one settlement; distinct reward animations play sequentially.

## Files Changed

- `Assets/Resources/Configs/CardPacks.csv`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/SqliteLocalStore.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/Sticker_Puzzle_功能概览整理.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Use ascending card-pack `Index` as the current deterministic selection order inside a chapter.
- Treat TaskConfig `RewardId` as a preferred candidate, not permission to re-grant an unlocked pack.
- When the current chapter has no locked candidate, continue with the next chapter that has one.
- Keep the 50% in-band probability as a tunable provisional value.

## Validation

- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- Executed 13 boundary cases for `R=17/9/8/3/2/1/0`, held-count minima/maxima, and 50% roll outcomes; all passed.
- Confirmed current config has 18 packs in chapter 1 and 3 development packs in chapter 2, with no invalid ChapterId.
- `git diff --check` completed without whitespace errors.
- Full SQLite migration, replay gating, reward persistence, and sequential animations still require Unity Play Mode regression.

## Next Action

1. Run a clean-save and old-save Play Mode regression through first completion, replay, task-only reward, and dual reward.
2. Tune the provisional 50% probability from gameplay data.
3. Add ChapterId assignments as the remaining card packs are authored.

## Resume Prompt

Continue Puffies chapter-based card pack distribution. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the Play Mode reward regression or follow the user's latest instruction.
