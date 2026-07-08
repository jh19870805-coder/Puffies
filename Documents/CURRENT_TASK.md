# Current Task

- Task: Fix missing SQLite native plugin after pulling repository
- Status: In Progress
- Updated At: 2026-07-08

## User Intent

- A freshly pulled copy of the repository on another Windows computer should run in Unity without `DllNotFoundException: sqlite3`.
- SQLite initialization should find the native `sqlite3` library required by `Assets/Plugins/SQLite/SQLite.cs`.

## Working Notes

- `SQLite.cs` imports native functions with `DllImport("sqlite3")`.
- The native files exist locally at `Assets/Plugins/x86/sqlite3.dll` and `Assets/Plugins/x86_64/sqlite3.dll`.
- Git was tracking the `.meta` files but not the actual `.dll` files.
- The DLL files were ignored by the user's global Git ignore rule `*.dll`, not by the project `.gitignore`.

## Files Changed

- `.gitignore`
- `Documents/CURRENT_TASK.md`

## Decisions

- Keep the existing `sqlite-net` C# wrapper.
- Keep the existing Windows native plugin layout under `Assets/Plugins/x86` and `Assets/Plugins/x86_64`.
- Add explicit project-level `.gitignore` exceptions so these two SQLite DLLs can be committed even when a developer has a global `*.dll` ignore rule.

## Validation

- Confirmed `git ls-files Assets/Plugins` only listed the SQLite `.meta` files and C# wrapper before the fix.
- Confirmed `git check-ignore -v` reported the DLL files were ignored by `C:\Users\Administrator\Documents\gitignore_global.txt`.
- Unity play mode was not run in this turn.

## Next Action

1. Add and commit `Assets/Plugins/x86/sqlite3.dll`, `Assets/Plugins/x86_64/sqlite3.dll`, their `.meta` files, `.gitignore`, and this task note.
2. On the other Windows computer, pull the commit and confirm both DLL files exist.
3. Reopen Unity or reimport `Assets/Plugins`, then verify `SqliteLocalStore.Initialize` succeeds.

## Resume Prompt

Continue Puffies SQLite native plugin tracking work. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then verify the SQLite DLL files are tracked and Unity can load `sqlite3` on Windows Editor.
