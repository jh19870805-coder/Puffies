# Current Task

- Task: Implement confirmed MainScene card-pack ordering
- Status: Completed
- Updated At: 2026-07-19

## User Intent

- Update card-pack sorting according to the confirmed product document.

## Working Notes

- MainScene now consumes a dedicated ordered PackId list instead of displaying the SQLite PackId order.
- Packs granted since the previous MainScene presentation appear first once; multiple new grants use newest unlock first.
- Default implemented priority is `InProgress`, `Unlocked`, then `Completed`; daily challenge is not part of the first release.
- `Unlocked` packs use ascending unlock time. `Completed` packs use ascending first-completion time. PackId resolves timestamp ties.
- Added `CompletionTime` to SQLite `CardPacks`; first completion writes it and replay preserves it.
- Unlock and completion timestamps now include milliseconds so multiple rewards in one settlement remain orderable.
- The temporary newly-granted set is in memory, is consumed by MainScene, and resets on application startup.

## Files Changed

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Model/SqliteLocalStore.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Add a real completion timestamp rather than overloading unlock time, because both ordering rules must remain independently correct.
- Keep `GetUnlockedPackIds` unchanged for count-only callers and expose a MainScene-specific consuming order API.
- Do not migrate the old development schema.

## Validation

- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- Five deterministic comparator tests passed: lifecycle priority, new-pack priority, unlock order, completion order, and PackId tie-breaking.
- `git diff --check` completed without whitespace errors.
- Unity Play Mode with a clean SQLite database still requires verification.

## Next Action

1. Close Unity and delete `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`, then reopen the project.
2. Verify current-session grants appear first once, then fall back to lifecycle ordering on the next MainScene visit or application restart.
3. Seed mixed `InProgress`, `Unlocked`, and `Completed` packs and confirm the 6 x 3 list follows the documented order.

## Resume Prompt

Continue Puffies MainScene card-pack ordering verification. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the clean-save Play Mode ordering regression or follow the user's latest instruction.
