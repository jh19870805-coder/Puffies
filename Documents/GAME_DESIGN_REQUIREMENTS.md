# Game Design Requirements

- Purpose: Long-term source of truth for confirmed game-design requirements
- Status: In Progress
- Last Updated: 2026-07-17

This document records confirmed design rules as provided by the game designer. Items marked `Confirmed` are implementation requirements. Items marked `Pending` must not be inferred during implementation.

---

## 1. Card Pack Scoring

### 1.1 Base Score By Card Pack Size

Status: `Confirmed`

Each card pack size has one base score.

| Card Pack Size | Base Score |
|---|---:|
| XS | 60 |
| S | 80 |
| M | 100 |
| L | 120 |
| XL | 140 |
| 2XL | 160 |
| 3XL | 200 |

### 1.2 Score Calculation Timing

Status: `Confirmed`

- Calculate the score during game settlement after the current puzzle game is completed.
- The score calculation belongs to the GameScene settlement flow.

### 1.3 No-Hint Bonus

Status: `Confirmed`

- Track whether the in-game hint feature was used during the current puzzle game.
- If the player completes the game without using a hint, add 5% of the card pack's base score during settlement.
- If at least one hint was used, this 5% bonus is not applied.

| Card Pack Size | Base Score | Score With No-Hint Bonus |
|---|---:|---:|
| XS | 60 | 63 |
| S | 80 | 84 |
| M | 100 | 105 |
| L | 120 | 126 |
| XL | 140 | 147 |
| 2XL | 160 | 168 |
| 3XL | 200 | 210 |

### 1.4 Outline-Disabled Bonus

Status: `Confirmed`

- Track whether the level-outline feature was enabled during the current puzzle game.
- If the player completes the game without enabling the level outline, apply an additional 2% score bonus during settlement.
- If the level outline was enabled during the current game, this 2% bonus is not applied.

### 1.5 Sticker-Outline-Disabled Bonus

Status: `Confirmed`

- Track whether the sticker-outline feature was enabled during the current puzzle game.
- If the player completes the game without enabling the sticker outline, apply an additional 5% score bonus during settlement.
- If the sticker outline was enabled during the current game, this 5% bonus is not applied.
- This rule is independent from the level-outline 2% bonus.

### 1.6 Completion-Time Bonus

Status: `Confirmed`

- Record the elapsed time for the current puzzle game and evaluate a time bonus during settlement.
- The three time thresholds are configurable and will be tuned later.
- Initial threshold values:

| Parameter | Initial Value |
|---|---:|
| A | 15 seconds |
| B | 30 seconds |
| C | 60 seconds |

| Completion Time | Time Bonus |
|---|---:|
| Time `<= A` | +3% |
| Time `> A` and `<= B` | +2% |
| Time `> B` and `<= C` | +1% |
| Time `> C` | No time bonus |

### 1.7 Final Score Formula

Status: `Confirmed`

- The card pack size determines `BaseScore`.
- Add together every percentage bonus for which the current game qualifies.
- Multiply `BaseScore` by one plus the total bonus percentage.
- Round the resulting score upward to the next integer.

```text
TotalBonusRate = NoHintBonus
               + LevelOutlineDisabledBonus
               + StickerOutlineDisabledBonus
               + CompletionTimeBonus

FinalScore = Ceil(BaseScore * (1 + TotalBonusRate))
```

Example: an M card pack has `BaseScore=100`. If it qualifies for no hint `+5%`, level outline disabled `+2%`, sticker outline disabled `+5%`, and the fastest time tier `+3%`, then:

```text
TotalBonusRate = 5% + 2% + 5% + 3% = 15%
FinalScore = Ceil(100 * 1.15) = 115
```

### 1.8 Settlement Score Presentation

Status: `Confirmed`

The settlement score must be presented as a sequence instead of appearing immediately at its final value:

1. Show the card pack's base score first.
2. Reveal one qualified bonus.
3. Animate the displayed score rolling upward to that step's cumulative score.
4. Repeat bonus reveal and score rolling for every qualified bonus.
5. Finish the final roll at `FinalScore`.

During every score-roll animation:

- The progress bar and its progress value must refresh continuously.
- The score displayed on the settlement page must roll at the same time.
- Both displays must use the same animated score value and finish simultaneously.
- Score calculation produces the base score, qualified bonus entries, cumulative step scores, and final score before presentation begins; UI code only presents this result.

### 1.9 Pending Scoring Details

Status: `Pending`

- How a CardBag is assigned a size.
- Where the size/base-score mapping is stored in game data.
- The exact point inside settlement at which the score is persisted, displayed, and applied to task progress.
- The order in which qualified bonuses are revealed during settlement.
- Duration, easing, and minimum visual step for each score-roll animation.
- Whether intermediate cumulative step scores are rounded upward or only `FinalScore` is rounded upward.
- Define when game-time recording starts, pauses, resumes, and stops.
- Whether switching the outline on and then off during the same game disqualifies the bonus; the current wording is recorded as "never enabled during the current game."
- Whether switching the sticker outline on and then off during the same game disqualifies the bonus; the current wording is recorded as "never enabled during the current game."
- What exact hint-button action counts as using a hint if the hint cannot be completed or displayed.
- Whether future score modifiers or caps apply in addition to the confirmed bonuses.

---

## Change Log

| Date | Change |
|---|---|
| 2026-07-17 | Confirmed additive bonus stacking, `Ceil(BaseScore * (1 + TotalBonusRate))`, and sequential settlement score-roll presentation with synchronized progress/score updates. |
| 2026-07-17 | Confirmed that exact threshold values belong to the faster tier: <=A, (A,B], and (B,C]. Time above C has no time bonus. |
| 2026-07-17 | Confirmed three configurable completion-time bonus tiers; initial thresholds are A=15s, B=30s, C=60s. |
| 2026-07-17 | Confirmed an additional 5% settlement bonus when the sticker outline is not enabled during the current game. |
| 2026-07-17 | Confirmed an additional 2% settlement bonus when the level outline is not enabled during the current game. |
| 2026-07-17 | Confirmed a 5% base-score bonus when the current game is completed without using an in-game hint. |
| 2026-07-17 | Confirmed that score is calculated during GameScene settlement after puzzle completion. |
| 2026-07-17 | Recorded the confirmed XS through 3XL card-pack base-score table. |
