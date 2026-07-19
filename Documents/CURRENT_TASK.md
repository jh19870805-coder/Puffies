# Current Task

- Task: Default level outline on and distinguish completed packs
- Status: Completed
- Updated At: 2026-07-19

## User Intent

- Make MainScene `Toggle1` enabled by default so the active puzzle-region outline is shown for new users.
- Lightly gray completed card packs in the MainScene list.

## Working Notes

- `GameSettingsData.UsableOption1` and newly created runtime settings now default to `true`.
- Persisted settings continue to win over defaults, so an existing saved `false` remains off until the user changes it.
- MainScene checks each displayed pack's lifecycle state and applies a light neutral tint to completed covers and size icons.
- Completed packs remain interactive and retain their normal shadow.
- The previous card-pack opening visibility and model-size alignment changes remain in the same working tree.

## Files Changed

- `Assets/Scripts/Model/GameSettingsUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs` (previous opening-animation task, still uncommitted)
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## Decisions

- Defaults must not overwrite an explicit persisted toggle choice.
- Use a `0.78` neutral Image tint for a subtle completed state without introducing a new shader or making replayable packs look disabled.
- Tint `PackCover` and `PackSize`; leave `PackShadow` unchanged.

## Validation

- `dotnet build Puffies.sln --no-restore` completed with 0 warnings and 0 errors.
- `git diff --check` completed without whitespace errors; Git only reported LF-to-CRLF working-copy notices.
- Unity Play Mode visual verification is still required.

## Next Action

1. For the current local profile, enable `PanelUsable/Toggle1` once because its existing saved value is `false`.
2. Re-enter GameScene and verify the active puzzle-region outline is visible.
3. Return to MainScene and confirm completed packs are lightly gray while unlocked and in-progress packs remain full color.
4. Confirm completed packs can still be clicked and replayed.

## Resume Prompt

Continue Puffies outline-default and completed-pack visual verification. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then run the listed Unity Play Mode checks or follow the user's latest instruction.
