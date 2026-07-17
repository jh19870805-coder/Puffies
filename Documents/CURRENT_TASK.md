# Current Task

- Task: Calculate full settlement score bonuses
- Status: Completed
- Updated At: 2026-07-17

## User Intent

- Include no-hint, two outline-setting, and completion-time bonuses in GameScene total score.
- Use persisted MainScene `PanelUsable` toggle values.
- Start gameplay timing when the first Piece is successfully placed.
- Apply the same final score to settlement UI and accumulated task progress.

## Working Notes

- `UsableOption1` already persisted Toggle1 and now has semantic meaning `IsLevelOutlineEnabled`; disabled grants +2%.
- `UsableOption2` already persisted Toggle2 and now has semantic meaning `IsStickerOutlineEnabled`; disabled grants +5%.
- `UsableOption3` is high contrast and does not affect score.
- Clicking `BtnTips` marks the current game as having used a hint; no click grants +5%.
- The timer starts on the first successful Piece placement and stops before RewardPanel settlement.
- Time bonuses are <=15s +3%, <=30s +2%, <=60s +1%, and above 60s +0%.
- All qualified percentages are added, multiplied by base score once, and rounded upward with integer ceiling arithmetic.
- `TaskScore` animation and `GameTaskUtility.AddCurrentScore` both use `GameScoreResult.FinalScore`.

## Files Changed

- `Assets/Scripts/Model/GameSettingsUtility.cs`
- `Assets/Scripts/Model/GameScoreUtility.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `specs/2026-07-17-settlement-score-bonuses.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## Decisions

- Preserve `UsableOption1/2/3` serialized field names so existing SQLite JSON remains compatible.
- Snapshot outline settings when GameScene starts because PanelUsable cannot be changed during gameplay.
- Treat the first `BtnTips` click as hint use even though the visual hint action itself is not implemented in GameScene code.
- Toggle1 also controls the existing baked active-group outer-frame display; Toggle2 currently contributes its saved setting to scoring but has no separate runtime sticker-outline renderer.
- Do not modify MainScene, GameScene, Prefabs, or Canvas layout data.

## Validation

- Confirmed Toggle1 is paired with the authored +2% outer-frame description and Toggle2 with the +5% full-contour description.
- Confirmed `BtnTips` exists in GameScene and added runtime click tracking.
- Confirmed exact time boundaries use the faster tier through <=15, <=30, and <=60 checks.
- Compiled `Assembly-CSharp-firstpass`, `Assembly-CSharp`, and `Assembly-CSharp-Editor` successfully without warnings.
- Interactive Unity play-mode verification remains to be performed.

## Next Action

1. Test with both outline toggles off and no hint; an M pack completed within 15 seconds should settle at 115 points.
2. Toggle one outline option at a time and confirm its percentage disappears from the `GameScene: score calculated` log.
3. Click BtnTips and test the 15, 30, and 60 second boundaries.
4. Design or add the actual hint action and individual bonus presentation UI as separate work.

## Resume Prompt

Continue the Puffies settlement scoring workflow. Read AGENTS.md, Documents/WORKFLOW.md, Documents/CURRENT_TASK.md, Documents/GAME_DESIGN_REQUIREMENTS.md, and specs/2026-07-17-settlement-score-bonuses.md first.
