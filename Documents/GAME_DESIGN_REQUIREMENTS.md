# Game Design Requirements

- Purpose: Long-term source of truth for confirmed game-design requirements
- Status: In Progress
- Last Updated: 2026-07-20

This document records confirmed design rules as provided by the game designer. Items marked `Confirmed` are implementation requirements. Items marked `Pending` must not be inferred during implementation.

---

## 1. Card Pack Scoring

### 1.1 Base Score By Card Pack Size

Status: `Confirmed`

Each card pack size has one base score.

- A card pack's size is configured by `PackSize` in `Resources/Configs/CardPacks.csv`.
- `PackSize` uses `CardPackSize` numeric values `XS=1` through `XXXL=7`.
- The base-score mapping is owned by `GameScoreUtility`.

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
- Start recording time when the first puzzle Piece is successfully placed. Failed placement attempts do not start the timer.
- Stop recording time when the completed puzzle begins the RewardPanel settlement flow.
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

- The exact point inside settlement at which the score is persisted, displayed, and applied to task progress.
- The order in which qualified bonuses are revealed during settlement.
- Duration, easing, and minimum visual step for each score-roll animation.
- Whether intermediate cumulative step scores are rounded upward or only `FinalScore` is rounded upward.
- Whether switching the outline on and then off during the same game disqualifies the bonus; the current wording is recorded as "never enabled during the current game."
- Whether switching the sticker outline on and then off during the same game disqualifies the bonus; the current wording is recorded as "never enabled during the current game."
- What exact hint-button action counts as using a hint if the hint cannot be completed or displayed.
- Whether future score modifiers or caps apply in addition to the confirmed bonuses.

---

## 2. Card Pack Lifecycle

Status: `Confirmed`

Every card pack has one persisted lifecycle state:

| State | Meaning |
|---|---|
| `Locked` | The card pack has not been granted and is not displayed in MainScene. |
| `Unlocked` | The card pack has been granted but the first puzzle group has not been completed. Opening the pack and leaving before completing the first group keeps this state. |
| `InProgress` | The first puzzle group has been completed, but at least one later group remains. |
| `Completed` | Every puzzle group in the card pack has been completed. |

Lifecycle transitions:

1. Granting a card pack changes `Locked` to `Unlocked`.
2. Entering GameScene or opening a pack does not by itself change `Unlocked`.
3. Completing the first group of a multi-group card pack changes `Unlocked` to `InProgress`.
4. Completing the final group changes the state to `Completed`.
5. `InProgress` and `Completed` do not downgrade during normal play. A future explicit replay-reset flow may define a separate reset transition.

MainScene presentation and ordering must use this lifecycle state.

### MainScene Card Pack Ordering

Status: `Confirmed`

The first release does not include daily-challenge packs. MainScene uses the following order:

1. Card packs granted since the previous MainScene list presentation. These packs are shown first once; when several are granted together, the latest grant appears first.
2. `InProgress` packs.
3. `Unlocked` packs, ordered by unlock time from earliest to latest.
4. `Completed` packs, ordered by first-completion time from earliest to latest.

After MainScene consumes the temporary newly-granted priority, those packs use their normal lifecycle priority. Restarting the application also clears this temporary priority. Equal or invalid timestamps use ascending PackId as the deterministic tie-breaker. A future daily-challenge implementation will insert its packs ahead of these priorities.

---

## 3. Card Pack Acquisition

Status: `Confirmed` except for the parameters listed as pending.

New card packs have two acquisition sources:

1. Completing a task always creates one guaranteed new-card-pack entitlement.
2. First-time completion of a card pack performs one stage-gated grant attempt.

Replay rules:

- Replaying a card pack that was already `Completed` does not perform the first-completion grant attempt.
- A replay can still complete a task. If it does, the task's guaranteed card pack reward is granted normally.
- A first completion from `Unlocked` or `InProgress` to `Completed` is eligible for the stage-gated grant attempt.

"New card pack" means a card pack that is currently `Locked`; these acquisition sources do not grant an already unlocked card pack.

Both acquisition sources select from the currently active internal chapter's eligible `Locked` card-pack pool.

Pending parameters:

- Final selection and ordering rules for choosing from eligible `Locked` card packs.
- Behavior when no eligible `Locked` card pack remains.
- Whether playtest results justify reintroducing a probability inside any stage.

Current playtest implementation:

- Initial stage (`R >= 9`): a grant is allowed while `H <= 5`, producing at most `H=6`.
- Mid-to-late transition (`R = 8`): one grant is allowed while `H <= 3`, producing at most `H=4`.
- Remaining mid-to-late stage (`R = 7..3`): a grant is allowed only while `H <= 2`, producing at most `H=3`.
- Final stage (`R = 2..1`): a grant is allowed while `H <= 1`, producing at most `H=2`.
- A blocked first-completion grant is skipped. A blocked task reward is persisted as pending and retried after a later first completion or task completion; it is never discarded.
- Pending task rewards are identified by TaskId so a failed task-advance save cannot enqueue the same task reward twice.
- A newly queued task reward is not delivered until task advancement has persisted successfully; if advancement fails, the entitlement remains queued for retry.
- Pending task rewards are attempted before the current first-completion grant.
- Task and first-completion sources may grant two different packs in one settlement when both pass the current stage gate.
- Locked candidates are selected by ascending `Index` inside the active chapter. A task's configured `RewardId` is preferred only when it is still locked and belongs to that chapter.
- RewardPanel keeps the authored default `ImgBag` icon instead of replacing it with a granted pack cover. When `BtnFinish` is clicked, all packs granted by the settlement fly together from `ImgBag` into a centered row, pause, survive the MainScene load, and then fly to their corresponding card-pack list slots.

---

## 4. Internal Chapter Model

Status: `Confirmed` except for the exact per-chapter allocation and transition parameters.

- The game has 8 internal chapters for card-pack data organization and acquisition control.
- The full game plans approximately 150 card packs in total.
- Each internal chapter contains approximately 18 card packs; the mathematical average is 18.75 packs per chapter.
- Chapters are not exposed to the player. MainScene does not display chapter names, chapter numbers, chapter selection, or chapter-transition messaging.
- The active internal chapter limits the card-pack pool used by task rewards and first-completion grants.

Chapter-stage counters:

- `R` is the number of card packs in the active chapter that have not been granted and remain `Locked`.
- Held playable pack count includes `Unlocked` and `InProgress` packs, and excludes `Completed` packs.
- A standard 18-pack chapter starts at `R=17` after its initial pack has been granted. A larger chapter's extra `R` values belong to the initial stage.

| Chapter Stage | `R` Range | Held Playable Pack Target |
|---|---:|---|
| Initial | 17 down to 9 | Grow toward approximately 5-6 packs. |
| Mid-to-late | 8 down to 3 | Decline toward approximately 2-3 packs. |
| Final | 2 down to 1 | Converge toward 1 pack before internal chapter advancement. |

Pending parameters:

- Exact card-pack count and PackId membership for each chapter.
- Initial active chapter and persisted chapter-progress data.
- Exact condition for advancing to the next internal chapter.
- How special card packs count toward the 150 total and chapter allocations.
- How task pacing and first-completion grants maintain the confirmed stage targets.

---

## 5. Puzzle Group Presentation

Status: `Confirmed`

### Tray Stability

- Pieces are laid out once when the current group is created.
- After a Piece is placed successfully, every remaining Piece keeps its established X and Y position.
- The empty position left by a placed Piece is not compacted until the next group is created.

### Staged Outline Boundary

- Group 1 displays only the part of Group 1's boundary that belongs to the final puzzle exterior.
- Every later group displays its own final-puzzle exterior plus its contact edges with all lower-number groups that are already completed.
- A current group must not display completed groups' unrelated boundaries.
- A current group must not display contact edges with future groups.
- Seams between individual Pieces inside the same group must not be displayed.
- Runtime displays only the current group's baked outline image; previous stage images are not overlaid.
- Outline baking is an Editor-time content step. Missing generated data must not block puzzle interaction.

---

## 6. Card Pack Opening Presentation

Status: `Confirmed`

- Every card pack reuses the existing generic 3D opening model and Animator animation.
- The animated model displays the selected PackId's real card-pack cover instead of a fixed authored cover.
- The animated cover uses the complete original Sprite rectangle and must not center-crop it to the generic model's old default-cover aspect.
- The generic effect itself must be authored for the common `600 x 680` (`15:17`) cover format; runtime code must not compensate for an incompatible effect aspect by cropping or non-uniform stretching.
- The generic effect uses a URP Renderer2D-compatible two-sided material: the selected PackId cover is the front face, the authored card-pack back is the back face, and the authored clip mask keeps the wave-shaped top and bottom edge.
- The runtime changes the front cover through per-renderer material properties and must not modify the shared material asset.
- Runtime centers and uniformly fits the compatible generic model inside the clicked MainScene card-pack UI bounds before playback.
- The closed 3D first frame must replace the clicked 2D cover without a visible position, size, aspect, crop, or blank-edge transition.
- Adding a card pack does not require creating a matching `CardPackSkin_NNN` prefab or animation FBX.
- If the generic model or animation cannot be loaded, MainScene keeps the existing 2D fallback interaction.

---

## Change Log

| Date | Change |
|---|---|
| 2026-07-20 | Confirmed a generic 3D opening model with the selected PackId's real cover and UI-bounds alignment. |
| 2026-07-20 | Confirmed stable tray positions and staged outline rules: current-group exterior plus contacts with completed groups only. |
| 2026-07-20 | Recorded `CardPacks.csv/PackSize` and `GameScoreUtility` as the implemented size/base-score data path. |
| 2026-07-19 | Confirmed the settlement-to-MainScene reward transition: keep the default ImgBag icon, center all granted packs after Finish, pause, then fly each one into its MainScene list slot. |
| 2026-07-19 | Confirmed internal chapter stages by remaining locked-pack count R: 17-9 initial, 8-3 mid-to-late, and 2-1 final, with corresponding held-pack targets. |
| 2026-07-19 | Replaced the provisional random roll with deterministic stage gates and persisted deferred task rewards for playtesting. |
| 2026-07-19 | Confirmed 8 player-invisible internal chapters for distributing approximately 150 card packs, averaging 18.75 packs per chapter. |
| 2026-07-19 | Confirmed two new-pack sources: guaranteed task entitlement and a first-completion grant attempt; replay does not receive the first-completion attempt. |
| 2026-07-18 | Confirmed the four-state card pack lifecycle: Locked, Unlocked, InProgress after the first completed group, and Completed after the final group. |
| 2026-07-17 | Confirmed that score beyond a completed accumulate-score task target carries into the next accumulate-score task. |
| 2026-07-17 | Confirmed that scoring time starts on the first successfully placed Piece and stops when completed-puzzle settlement begins. |
| 2026-07-17 | Confirmed additive bonus stacking, `Ceil(BaseScore * (1 + TotalBonusRate))`, and sequential settlement score-roll presentation with synchronized progress/score updates. |
| 2026-07-17 | Confirmed that exact threshold values belong to the faster tier: <=A, (A,B], and (B,C]. Time above C has no time bonus. |
| 2026-07-17 | Confirmed three configurable completion-time bonus tiers; initial thresholds are A=15s, B=30s, C=60s. |
| 2026-07-17 | Confirmed an additional 5% settlement bonus when the sticker outline is not enabled during the current game. |
| 2026-07-17 | Confirmed an additional 2% settlement bonus when the level outline is not enabled during the current game. |
| 2026-07-17 | Confirmed a 5% base-score bonus when the current game is completed without using an in-game hint. |
| 2026-07-17 | Confirmed that score is calculated during GameScene settlement after puzzle completion. |
| 2026-07-17 | Recorded the confirmed XS through 3XL card-pack base-score table. |
