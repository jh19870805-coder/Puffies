# SPEC 工作流

> 跨设备开发时，用 Git + 状态文件接力，避免只在聊天里口头同步。

## 单一事实源

| 文件 | 用途 |
|------|------|
| [SPEC_WORKFLOW.md](SPEC_WORKFLOW.md) | 流程规范（本文件） |
| [SPEC_STATUS.md](SPEC_STATUS.md) | **当前任务**实时状态 |
| [PROJECT_SETUP.md](PROJECT_SETUP.md) | 工程静态指南（目录、场景、构建） |

## 四阶段（S / P / E / C）

- **S - Scope**：需求范围与验收标准
- **P - Plan**：实现方案与改动清单
- **E - Execute**：执行进度、阻塞、变更说明
- **C - Check**：验证结果、风险、回滚点

## SPEC_STATUS 必填字段

- `Task` / `Status`（Draft | In Progress | Blocked | Done）/ `Updated At`
- **Requirement Log**：用户关键需求
- **Progress Snapshot**：已完成 / 进行中 / 未完成
- **Next Action**：下一步可执行动作
- **Resume Prompt**：新设备续做的一句话

任务进行中可另写 S/P/E/C 小节；**静态工程信息**（目录、场景表）放 `PROJECT_SETUP.md`，避免在 STATUS 里重复维护。

## 执行约束

1. 新任务开始：先读 `SPEC_STATUS.md`（久未同步则先 `git pull`）。
2. 有代码/场景/配置改动：同一轮内回写 STATUS（至少 E + Next Action）。
3. 架构、路径、命名结论：写入 C 或 `PROJECT_SETUP.md`。
4. 仅问答、未改仓库：可不更新 STATUS。
5. 用户明确要求「只解释不改代码」：不强制改 STATUS。

## 跨设备同步

- 阶段性完成后小步提交。
- 切换设备前更新 Progress Snapshot 与 Resume Prompt。
- 冲突时保留最新 `Updated At`，并注明冲突说明。

## 新任务模板

复制以下内容到 `SPEC_STATUS.md` 顶部并填写：

```markdown
# SPEC 状态面板

- Task: <任务名称>
- Status: In Progress
- Updated At: <YYYY-MM-DD>

## Requirement Log

- 

## Progress Snapshot

- 已完成：
- 进行中：
- 未完成：

## S - Scope

- 背景与目标：
- 本次范围：
- 验收标准：

## P - Plan

- 方案概述：
- 涉及文件：
- 步骤：
  - [ ] 

## E - Execute

- 

## C - Check

- 

## Next Action

1. 

## Resume Prompt

继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行。
```
