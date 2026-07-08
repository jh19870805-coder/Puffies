# Codex Workflow

This project uses a compact Codex-first workflow. Keep durable facts separate from current task state.

## Single Sources

| File | Purpose |
|------|---------|
| [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) | Stable requirements, architecture, scenes, data, assets, build rules, naming |
| [CURRENT_TASK.md](CURRENT_TASK.md) | Current task, status, decisions, checks, and next action |
| [WORKFLOW.md](WORKFLOW.md) | This workflow |
| [../AGENTS.md](../AGENTS.md) | Repository-level instructions for AI agents |

## Start Of Work

For feature work, bug fixes, scene changes, asset/config changes, or behavior changes:

1. Read `AGENTS.md`.
2. Read `Documents/WORKFLOW.md`.
3. Read `Documents/CURRENT_TASK.md`.
4. Read the relevant sections of `Documents/PROJECT_CONTEXT.md`.
5. Inspect the code/assets before deciding implementation details.

For simple read-only questions, only read the files needed to answer accurately.

## During Work

- Prefer existing project patterns over new abstractions.
- Keep changes scoped to the user request.
- Do not rewrite project documentation unless the project facts or current task state changed.
- If implementation reveals a new stable fact, update `PROJECT_CONTEXT.md`.
- If code, scene, asset, config, or behavior changes are made, update `CURRENT_TASK.md` in the same turn.

## Current Task Update Rules

Update `CURRENT_TASK.md` with:

- task name and status
- user intent or requirement
- files changed
- important decisions
- validation performed or explicitly not performed
- next useful action

Do not update `CURRENT_TASK.md` for:

- simple explanations
- read-only investigation
- hidden/exclude list housekeeping
- formatting-only edits that do not affect project behavior, unless the user asks for documentation tracking

## Task Template

```markdown
# Current Task

- Task: <short name>
- Status: In Progress
- Updated At: <YYYY-MM-DD>

## User Intent

- 

## Working Notes

- 

## Files Changed

- 

## Decisions

- 

## Validation

- 

## Next Action

1. 

## Resume Prompt

Continue this Puffies task. Read AGENTS.md, Documents/WORKFLOW.md, and Documents/CURRENT_TASK.md first, then follow Next Action unless the user gives a newer instruction.
```
