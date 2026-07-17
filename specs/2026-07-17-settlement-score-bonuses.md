# Settlement Score Bonuses

## Requirements

### User Story

As a player, I want GameScene settlement to include every qualified bonus so that the displayed total score and accumulated task progress use the same correct final score.

### Acceptance Criteria

1. WHEN a game settles THEN the system SHALL start from the selected card pack size's configured base score.
2. IF `BtnTips` was not clicked during the current game THEN the system SHALL add a 5% no-hint bonus.
3. IF MainScene `Toggle1` (`UsableOption1`, level outer frame) was disabled when the game started THEN the system SHALL add a 2% level-outline-disabled bonus.
4. IF MainScene `Toggle2` (`UsableOption2`, sticker/full contour) was disabled when the game started THEN the system SHALL add a 5% sticker-outline-disabled bonus.
5. `Toggle3` (`UsableOption3`, high contrast) SHALL NOT affect score.
6. WHEN completion time is `<=15`, `(15,30]`, `(30,60]`, or `>60` seconds THEN the system SHALL add 3%, 2%, 1%, or 0% respectively.
7. WHEN bonuses are calculated THEN the system SHALL add all qualified percentages, multiply once by base score, and round the final result upward.
8. WHEN settlement UI and task progress update THEN both SHALL use the same final score result.
9. WHEN an outline setting is read for a game THEN it SHALL come from the persisted `GameSettings/Runtime` SQLite record rather than a Scene placeholder.
10. WHEN the first Piece is successfully placed THEN the system SHALL start the gameplay timer; failed placement attempts SHALL NOT start it.
11. WHEN the completed puzzle begins `ShowRewardPanel` THEN the system SHALL stop the gameplay timer before calculating score.

## Confirmed Mapping

| MainScene Control | Persisted Field | Meaning | Disabled Bonus |
|---|---|---|---:|
| `PanelUsable/Toggle1` | `UsableOption1` | Level outer frame | +2% |
| `PanelUsable/Toggle2` | `UsableOption2` | Sticker/full contour | +5% |
| `PanelUsable/Toggle3` | `UsableOption3` | High contrast | None |

## Design

- Preserve `UsableOption1/2/3` serialized field names for SQLite JSON compatibility, while adding semantic accessors for scoring code.
- Snapshot outline settings when GameScene gameplay begins; settings cannot be changed inside GameScene.
- Start an unscaled realtime timer on the first successful Piece placement and freeze it when `ShowRewardPanel` starts.
- Bind `BtnTips` and mark the session as having used a hint on its first click.
- Expand `GameScoreUtility` to return a structured result containing base score, individual bonus percentages, total bonus percentage, elapsed time, and final score.
- Calculate the upward-rounded result with integer ceiling arithmetic to avoid floating-point boundary errors.
- Replace GameScene's base-only task increment and `TaskScore` target with the final score.
- Keep `TaskBagNum` independent from scoring.

## Tasks

- [x] Add semantic outline-setting accessors without changing persisted field names.
- [x] Track session hint use, outline settings, and elapsed time in GameScene.
- [x] Implement full additive bonus calculation and upward rounding.
- [x] Use final score for TaskScore animation and task progress persistence.
- [x] Update project notes and compile runtime and Editor assemblies.

## Out Of Scope

- Implementing the visual hint action itself; this task only tracks `BtnTips` use for scoring.
- Adding new settlement layout for individual bonus labels.
- Changing MainScene or GameScene authored UI layout.
