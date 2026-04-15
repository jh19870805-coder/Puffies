# SPEC 状态面板

- Task: 全工程重复代码合并与公共能力抽取
- Status: In Progress
- Updated At: 2026-04-15
- Auto-Update Mode: Enabled (Maintained by Codex)

## Requirement Log

- 用户希望建立跨设备同步进度的 SPEC 工作流。
- 用户希望不手动填写，由 Codex 自动维护状态面板。
- 用户希望在另一台设备打开后，Codex 能继续工作并知悉已提需求与当前开发进度。

## Progress Snapshot

- 已完成：
  - 已建立 `SPEC_WORKFLOW.md` / `SPEC_STATUS.md` / `SPEC_TEMPLATE.md`。
  - 已开启 Auto-Update Mode。
  - 已完成一轮工程重复代码合并（公共工具与场景引导模板抽取）。
- 进行中：
  - 将 SPEC 升级为“跨设备接力版”字段规范。
- 未完成：
  - 等待下一条具体功能需求并进入新一轮开发。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- 合并 `MainScene` / `GameScene` / `GameManager` 中可安全收敛的重复代码。
- 在不改变功能行为的前提下，降低路径处理、场景判断、相机配置的重复实现。
- 验收标准：脚本可编译、原有流程可运行、无新增 linter 报错。

## P - Plan

- 抽取 `GameCommonUtility` 统一处理：
  - 场景匹配
  - 正交相机配置
  - 资源路径转磁盘路径
  - 场景引导模板（泛型）
- 让 `MainScene` 和 `GameScene` 复用引导模板。
- 让 `GameManager` 复用公共路径与常量定义。

## E - Execute

- 已完成：新增 `GameCommonUtility`，并接入 `MainScene` / `GameScene` / `GameManager`。
- 已完成：合并 `Bootstrap -> OnSceneLoaded -> TryBootstrap` 模板逻辑。
- 已完成：统一字符串常量来源（`GameDefine`）。
- 当前状态：代码已重构完成，待进一步功能扩展时继续沿用该机制。

## C - Check

- 检查方式：针对变更文件执行 lints。
- 检查结果：无新增 linter 报错。
- 已知风险：当前是结构收敛，未增加自动化运行时测试；后续可补充 PlayMode 验证。

## Next Action

- 直接提出下一条功能需求；Codex 将自动更新 `Requirement Log / Progress Snapshot / S/P/E/C / Status` 并执行开发。
