# SPEC 工作流（跨设备同步版）

> 目标：把开发过程沉淀为可追踪、可恢复、可在任意设备继续的记录。

## 1. 机制定义

SPEC 分为 4 个阶段：

- `S - Scope`：需求范围与验收标准
- `P - Plan`：实现方案与改动清单
- `E - Execute`：执行进度与已完成项
- `C - Check`：验证结果、风险与回滚点

所有阶段都写入仓库文件，随 Git 同步到不同设备。

## 2. 单一事实源

跨设备统一看这 2 个文件：

- `Documents/SPEC_WORKFLOW.md`：流程规范（本文件）
- `Documents/SPEC_STATUS.md`：当前任务实时状态

## 3. 跨设备接力字段（新增）

为保证“另一台设备打开即可续做”，`SPEC_STATUS.md` 必须包含以下交接信息：

- `Requirement Log`：记录用户关键需求原文或精简版
- `Progress Snapshot`：记录当前已完成、未完成、进行中
- `Resume Prompt`：记录下一设备可直接发送给 Codex 的一句话

> 这三项比 S/P/E/C 更偏“会话接力”，用于减少跨设备上下文损耗。

## 4. 使用规则（我会遵守）

每次开始新任务时：

1. 先更新 `SPEC_STATUS.md` 的 `S` 与 `P`
2. 实施过程中持续更新 `E`（完成项/阻塞项/下一步）
3. 完成后更新 `C`（验证方式、结果、已知风险）
4. 若任务关闭，打上 `Status: Done`

## 5. 状态字段约定

`SPEC_STATUS.md` 使用以下字段：

- `Task`：当前任务
- `Status`：`Draft | In Progress | Blocked | Done`
- `Updated At`：最后更新时间
- `S / P / E / C`：四阶段内容
- `Requirement Log`：需求记录（用于跨设备接力）
- `Progress Snapshot`：进度快照（用于跨设备接力）
- `Resume Prompt`：续接提示语（用于跨设备接力）
- `Next Action`：下一步动作

## 6. 跨设备同步建议

- 每次阶段性完成后提交一次（小步提交）
- 新设备开始前先 `pull`，优先读取 `SPEC_STATUS.md`
- 如果发生冲突，保留最新 `Updated At` 内容，并补充冲突说明
- 在切换设备前，确保 `Progress Snapshot` 与 `Resume Prompt` 已更新。

## 7. 执行约束

- 不在聊天里只口头同步，必须回写到 `SPEC_STATUS.md`
- 结论性变更（架构、路径、命名）必须写入 `C`
- 若中断，至少保证 `E` 和 `Next Action` 可直接接手
- 若预计跨设备续做，必须填写 `Requirement Log / Progress Snapshot / Resume Prompt`
