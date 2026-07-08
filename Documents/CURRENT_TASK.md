# Current Task

- Task: Rebuild project workflow for Codex
- Status: Done
- Updated At: 2026-07-08

## User Intent

- The project was previously developed with another AI.
- The user allowed one thorough rewrite of workflow-related files, including adding files, deleting files, and redefining workflow paths.
- The workflow should now match Codex preferences for future maintenance.

## Working Notes

- Replaced the old `SPEC_*` workflow naming with a Codex-first structure.
- Stable project facts now live in `Documents/PROJECT_CONTEXT.md`.
- Current task state now lives in `Documents/CURRENT_TASK.md`.
- Workflow rules now live in `Documents/WORKFLOW.md`.
- `AGENTS.md` is the repository-level AI entry point.

## Files Changed

- `AGENTS.md`
- `README.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/WORKFLOW.md`
- `Documents/CURRENT_TASK.md`
- Removed old workflow files after migration:
  - `Documents/PROJECT_SETUP.md`
  - `Documents/SPEC_STATUS.md`
  - `Documents/SPEC_WORKFLOW.md`
  - `.cursor/rules/spec-workflow.mdc`

## Decisions

- Use `PROJECT_CONTEXT.md` instead of `PROJECT_SETUP.md` because the file contains both requirements and engineering facts, not just setup instructions.
- Use `CURRENT_TASK.md` instead of `SPEC_STATUS.md` to make the status file's role obvious without inherited SPEC terminology.
- Use one repository-level `AGENTS.md` for AI instructions instead of keeping duplicate Cursor-specific rules.
- Keep `README.md` as a thin index only.

## Validation

- Re-read the migrated files after editing.
- Confirmed the remaining Markdown set is intentionally small:
  - `README.md`
  - `AGENTS.md`
  - `Documents/PROJECT_CONTEXT.md`
  - `Documents/WORKFLOW.md`
  - `Documents/CURRENT_TASK.md`
- Did not run Unity tests; this task changed documentation/workflow files only.

## Next Action

1. For future feature work, start by reading `AGENTS.md`, `Documents/WORKFLOW.md`, and this file.
2. If a new task starts, replace this file's top task block using the template in `Documents/WORKFLOW.md`.

## Resume Prompt

Continue Puffies development. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then follow Next Action unless the user gives a newer instruction.
