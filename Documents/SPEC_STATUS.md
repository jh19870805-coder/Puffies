# SPEC 状态面板

- Task: Markdown 文档整理与需求合并
- Status: Done
- Updated At: 2026-07-08

> 工程目录、功能需求、场景、构建等静态说明统一维护在 [PROJECT_SETUP.md](PROJECT_SETUP.md)。

## Requirement Log

- 用户要求整理项目 Markdown 文件，合并功能需求，删除冗余 Markdown 文件。
- 用户要求以后按 `Documents/` 下的 SPEC 工作流维护项目。

## Progress Snapshot

- **已完成**：
  - 确认当前 Markdown 文件：`README.md`、`AGENTS.md`、`Documents/PROJECT_SETUP.md`、`Documents/SPEC_STATUS.md`、`Documents/SPEC_WORKFLOW.md`。
  - 将功能需求统一合并到 `Documents/PROJECT_SETUP.md` 的“功能需求总览”。
  - 将 `README.md` 精简为文档入口和快速约定，避免重复维护工程结构与场景流程。
  - 将 `SPEC_STATUS.md` 改为当前任务状态面板，不再重复保存完整功能需求。
  - 保留 `AGENTS.md` 与 `.cursor/rules/spec-workflow.mdc` 作为项目级代理工作流规则。
- **进行中**：无。
- **未完成**：无。

## S - Scope

- 背景与目标：减少 Markdown 文档重复内容，明确功能需求的单一维护位置。
- 本次范围：整理根目录与 `Documents/` 下 Markdown 文档；保留必要的代理规则文件。
- 验收标准：
  - 功能需求集中在 `Documents/PROJECT_SETUP.md`。
  - `README.md` 只作为入口，不复制大量工程事实。
  - `SPEC_STATUS.md` 只记录当前任务状态、检查与下一步。
  - 不删除仍承担明确用途的 Markdown 文件。

## P - Plan

- 方案概述：以 `PROJECT_SETUP.md` 作为功能需求与工程事实主参考，`SPEC_WORKFLOW.md` 保留流程规则，`SPEC_STATUS.md` 保留任务状态，`README.md` 保留入口信息。
- 涉及文件：
  - `README.md`
  - `Documents/PROJECT_SETUP.md`
  - `Documents/SPEC_STATUS.md`
  - `AGENTS.md`
  - `.cursor/rules/spec-workflow.mdc`
- 步骤：
  - [x] 盘点 Markdown 文件。
  - [x] 合并功能需求到 `PROJECT_SETUP.md`。
  - [x] 精简 `README.md`。
  - [x] 更新 `SPEC_STATUS.md`。
  - [x] 判断是否存在可删除的冗余 Markdown 文件。

## E - Execute

- `Documents/PROJECT_SETUP.md` 新增“功能需求总览”，覆盖核心循环、场景需求、数据与奖励需求、内容扩展需求、待定需求。
- `README.md` 改为文档入口，移除与 `PROJECT_SETUP.md` 重复的结构、场景流、菜单说明。
- `Documents/SPEC_STATUS.md` 更新为本轮文档整理任务状态。
- 未删除 Markdown 文件：当前每个 Markdown 文件仍有独立用途，没有发现纯冗余文件。

## C - Check

- 文档职责：
  - `PROJECT_SETUP.md`：功能需求与工程事实主参考。
  - `SPEC_WORKFLOW.md`：工作流规范与模板。
  - `SPEC_STATUS.md`：当前任务状态。
  - `README.md`：入口索引。
  - `AGENTS.md`：代理/AI 项目指令。
- 风险：`AGENTS.md` 与 `.cursor/rules/spec-workflow.mdc` 内容相近，但服务不同工具入口；暂不删除。

## Next Action

1. 后续功能开发前，先读 `Documents/SPEC_WORKFLOW.md` 与本文件。
2. 若开始新任务，按 `Documents/SPEC_WORKFLOW.md` 的模板替换本文件顶部状态内容。

## Resume Prompt

继续 Puffies 开发，请先读取 Documents/SPEC_WORKFLOW.md 和 Documents/SPEC_STATUS.md，然后按 Next Action 执行。
