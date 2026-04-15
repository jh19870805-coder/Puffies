# SPEC 状态面板

- Task: MainScene 包图滑动交互切换到 GameScene
- Status: In Progress
- Updated At: 2026-04-15
- Auto-Update Mode: Enabled (Maintained by Codex)

## Requirement Log

- 用户希望建立跨设备同步进度的 SPEC 工作流。
- 用户希望不手动填写，由 Codex 自动维护状态面板。
- 用户希望在另一台设备打开后，Codex 能继续工作并知悉已提需求与当前开发进度。
- 新需求：在 MainScene 创建并居中放置一个精灵，纹理为 `PackImages/Package001.png`，复用现有方法并减少重复代码。
- 新需求：在 MainScene 上给包图添加交互，要求在精灵范围内完成从左到右滑动后切换到 `GameScene`。

## Progress Snapshot

- 已完成：
  - 已建立 `SPEC_WORKFLOW.md` / `SPEC_STATUS.md` / `SPEC_TEMPLATE.md`。
  - 已开启 Auto-Update Mode。
  - 已完成一轮工程重复代码合并（公共工具与场景引导模板抽取）。
  - MainScene 已新增 `Package001` 精灵创建流程，启动后自动居中显示。
  - 通过抽取 `CreateCenteredSpriteObject(...)` 合并了背景与包图创建逻辑，减少重复代码。
  - 已实现包图内滑动交互：起点与终点都在包图区域内，左滑到右且位移达阈值时切换到 `GameScene`。
- 进行中：
  - 等待你验证鼠标/触屏交互手势在目标设备上的实际体验。
- 未完成：
  - 如需进一步调阈值、增加滑动反馈动画或音效，进入下一轮调整。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- 在 `MainScene` 启动流程中创建 `Package001` 精灵并居中显示。
- 复用 `CreateSpriteByPath` 与现有相机/对象创建逻辑，避免新增重复实现。
- 为 `Package001` 增加“仅精灵内有效”的左到右滑动交互，并切换到 `GameScene`。
- 验收标准：运行主场景可看到包图，且在包图区域内右滑可切场景，脚本无新增 linter 报错。

## P - Plan

- 在 `MainScene` 增加 `Package001` 对象名和资源路径常量。
- 抽取通用创建方法 `CreateCenteredSpriteObject(...)`。
- 让 `CreateCenteredBackground(...)` 与 `CreateCenteredPackageSprite(...)` 共同复用该方法。
- 缓存包图 `SpriteRenderer`，基于其 bounds 判断交互范围。
- 实现鼠标/触屏输入下的开始与结束点检测，并校验左到右滑动方向后切场景。

## E - Execute

- 已完成：新增 `MainPackageObjectName`、`MainPackagePath` 常量。
- 已完成：`Start()` 中新增 `CreateCenteredPackageSprite()` 调用。
- 已完成：抽取 `CreateCenteredSpriteObject(...)`，统一背景图与包图的创建流程。
- 已完成：新增滑动交互逻辑（`TryBeginSwipe` / `TryCompleteSwipe`），并接入鼠标与触屏输入。
- 当前状态：功能已完成，等待你在场景中确认交互体验。

## C - Check

- 检查方式：针对 `MainScene.cs` 执行 lints。
- 检查结果：无新增 linter 报错。
- 已知风险：滑动阈值当前固定为 `0.5` 世界单位，不同分辨率下可能需要调整。

## Next Action

- 在 MainScene 运行验证包图内右滑切场景手势；如需调整阈值或方向判定，直接给我目标规则。
