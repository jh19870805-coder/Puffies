# SPEC 状态面板

- Task: MainScene / GameManager / GameDefine 重复代码合并与冗余清理
- Status: In Progress
- Updated At: 2026-04-15
- Auto-Update Mode: Enabled (Maintained by Codex)

## Requirement Log

- 用户希望建立跨设备同步进度的 SPEC 工作流。
- 用户希望不手动填写，由 Codex 自动维护状态面板。
- 用户希望在另一台设备打开后，Codex 能继续工作并知悉已提需求与当前开发进度。
- 新需求：在 MainScene 创建并居中放置一个精灵，纹理为 `PackImages/Package001.png`，复用现有方法并减少重复代码。
- 新需求：在 MainScene 上给包图添加交互，要求在精灵范围内完成从左到右滑动后切换到 `GameScene`。
- 新需求：检查 MainScene、GameManager、GameDefine 三个文件，尝试合并重复代码并删除冗余代码。

## Progress Snapshot

- 已完成：
  - 已建立 `SPEC_WORKFLOW.md` / `SPEC_STATUS.md` / `SPEC_TEMPLATE.md`。
  - 已开启 Auto-Update Mode。
  - 已完成一轮工程重复代码合并（公共工具与场景引导模板抽取）。
  - MainScene 已新增 `Package001` 精灵创建流程，启动后自动居中显示。
  - 通过抽取 `CreateCenteredSpriteObject(...)` 合并了背景与包图创建逻辑，减少重复代码。
  - 已实现包图内滑动交互：起点与终点都在包图区域内，左滑到右且位移达阈值时切换到 `GameScene`。
  - 已将包图路径来源统一复用 `GameManager.GetBagPackagePath()`，删除 MainScene 内硬编码包图路径常量。
  - 已合并鼠标/触屏滑动分发重复逻辑（新增 `HandleSwipeInput(...)`）。
  - 已在 GameManager 中收敛 bagId 格式化与目录命名重复逻辑（`GetBagIdText` / `GetBagFolderName`）。
  - 已删除 GameManager 仅转发的冗余方法 `ToDiskConfigPath(...)`，直接复用 `GameCommonUtility.ToDiskPath(...)`。
- 进行中：
  - 等待你验证重构后主场景展示与交互行为是否与预期一致。
- 未完成：
  - 如需进一步调阈值、增加滑动反馈动画或音效，进入下一轮调整。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- 检查并合并 `MainScene` / `GameManager` / `GameDefine` 中可安全收敛的重复代码。
- 删除不必要的中转方法和硬编码路径，统一常量来源。
- 验收标准：脚本行为不变，且无新增 linter 报错。

## P - Plan

- MainScene：清理包图路径重复定义并合并输入分发逻辑。
- GameManager：统一 bagId 文本格式化与目录命名逻辑，移除冗余中转方法。
- GameDefine：补充可复用命名前缀常量并消除硬编码。

## E - Execute

- 已完成：MainScene 包图路径来源统一改为 `GameManager`。
- 已完成：MainScene 鼠标与触屏滑动分发逻辑合并。
- 已完成：GameManager bag 路径构建重复逻辑收敛并删除冗余路径中转方法。
- 已完成：GameDefine 新增前缀常量并在业务代码中接入。
- 当前状态：重构完成，等待运行验收。

## C - Check

- 检查方式：针对 `MainScene.cs`、`GameManager.cs`、`GameDefine.cs` 执行 lints。
- 检查结果：无新增 linter 报错。
- 已知风险：本次为结构重构，建议在编辑器里回归验证主场景包图显示和右滑切场景流程。

## Next Action

- 运行 MainScene 做一次回归验证；若行为 OK，可继续下一轮功能开发。
