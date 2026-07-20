# Current Task

- Task: Synchronize latest development progress and game rules
- Status: Completed
- Updated At: 2026-07-20

## User Intent

- Consolidate the latest cross-device implementation progress and rules into the project's durable documentation.
- Correct stale outline and scoring data-path descriptions without creating more dated spec files.

## Working Notes

- `GAME_DESIGN_REQUIREMENTS.md` remains the source of truth for confirmed and pending design rules.
- `PROJECT_CONTEXT.md` records the actual current architecture, persistence, content, and runtime behavior.
- `specs/task-and-settlement.md` now covers scoring, task progress, card-pack lifecycle/distribution, list ordering, and reward presentation.
- `specs/puzzle-outline.md` now records stable tray positions and the latest current-group exterior/contact-edge rule.
- The latest implementation is commit `2236f9f` on `develop`, synchronized with `origin/develop` when this review was performed.

## Implemented Progress

- Accumulated-score settlement with hint, outline, sticker, and time bonuses plus upward rounding.
- Four-state card-pack lifecycle, pending task entitlements, stage-gated first-completion grants, and internal chapter pools.
- MainScene 18-item paging, lifecycle ordering, new-pack priority, completed tint, size icons, and generated cover shadows.
- Multi-pack RewardPanel-to-MainScene collection flight.
- CardBag017 content and 19 baked outline masks across five playable CardBag prefabs.
- Stable Piece tray positions and staged current-group outline boundaries.
- RankScene local-player presentation and 20-item mock AchieveScene content.

## Files Changed

- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/task-and-settlement.md`
- `specs/puzzle-outline.md`

## Decisions

- Keep only two long-lived spec files rather than creating one dated document per change.
- Separate confirmed design rules, current playtest parameters, implementation facts, and pending decisions.
- Record the current single 0-to-final score roll as an implementation gap against the confirmed sequential bonus presentation.
- Preserve the active-development policy: no legacy SQLite migration unless explicitly requested.

## Validation

- Reviewed commit history from `121e593` through `2236f9f` and checked current code/config ownership for each documented feature.
- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors at `2236f9f`.
- Confirmed `develop` matched `origin/develop` and the workspace was clean before this documentation update.
- Static consistency checks found no stale outline rule, resolved PackSize pending item, or unexpected non-document change.
- No code, scene, prefab, generated outline, or gameplay resource was changed in this documentation task.

## Data Reset Before Regression

- Close Unity before resetting development data.
- Delete `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db` because the CardPacks schema changed incompatibly.
- Delete `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.json` for a clean task/distribution cross-store regression.
- Do not delete either file automatically without explicit user permission.

## Next Action

1. Verify stable tray coordinates and staged group outlines in Play Mode.
2. Verify lifecycle transitions, pending and dual grants, reward flight, and MainScene paging/order with clean local data.
3. Implement the confirmed sequential bonus reveal and cumulative score-roll presentation after its pending timing/order details are confirmed.
4. Confirm final chapter allocation, advancement, candidate selection, and empty-pool behavior before scaling content toward 150 packs.

## Resume Prompt

Continue Puffies from the synchronized project state. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/GAME_DESIGN_REQUIREMENTS.md, and the two specs first, then follow the latest user instruction or the Next Action list.
