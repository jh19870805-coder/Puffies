# SPEC 状态面板

- Task: 所有页面相机自适配完整可见
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
- 新需求：修改 GameScene 创建逻辑，切场景时传入卡包 id（先使用默认值）。
- 新需求：进入 GameScene 后根据 bagId 创建对应 GameBoard，解析对应 JSON，并将第一组碎片在 GameBoard 左侧一列等距排列。
- 新需求：设置所有页面运行后都能看到整个页面。
- 新需求：GameScene 按 JSON 创建所有碎片，并按相对 GameBoard 的 x/y 坐标定位。
- 坐标系补充：贴图坐标以碎片中心为锚点，左下为原点。

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
  - 已打通切场景参数链路：MainScene 滑动成功后传入默认 `bagId`，GameScene 按传入 `bagId` 初始化资源。
  - 已实现 GameScene 进入后按 `bagId` 读取对应 `PackageXXX.json`，并创建对应 `GameBoard`。
  - 已实现按配置第一组碎片在 GameBoard 左侧一列等距排布。
  - 已新增通用相机框选方法 `FitOrthographicCameraToRenderers(...)`。
  - 已在 MainScene 与 GameScene 接入页面内容自动框选，保证页面整体可见。
  - 已将 GameScene 碎片创建改为“全量分组全量碎片”，并按 JSON 中 x/y 相对棋盘坐标落位。
  - 已将坐标换算改为“左下原点 + 中心锚点”规则。
- 进行中：
  - 等待你验证碎片坐标系方向（y 轴上下）与策划数据是否一致。
- 未完成：
  - 如需进一步调阈值、增加滑动反馈动画或音效，进入下一轮调整。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- MainScene 与 GameScene 运行后都能看到页面完整内容，不出现内容裁切。
- 在保持现有布局逻辑前提下，增加相机自动框选机制。
- GameScene 需按配置生成全部碎片并正确对齐到棋盘相对坐标。
- 验收标准：两页面完整可见，且 GameScene 中全部碎片按配置坐标准确显示。

## P - Plan

- 在 `GameCommonUtility` 增加通用相机框选方法。
- MainScene：按背景+卡包精灵联合边界自适配相机。
- GameScene：按棋盘+第一组碎片联合边界自适配相机。
- GameScene：将“第一组左列展示”替换为“全量碎片按配置坐标展示”。

## E - Execute

- 已完成：`GameCommonUtility` 新增 `FitOrthographicCameraToRenderers(...)`。
- 已完成：MainScene 在创建背景与包图后自动框选可视范围。
- 已完成：GameScene 在创建棋盘与第一组碎片后自动框选可视范围。
- 已完成：GameScene 读取全部分组碎片，按 `x/y` 相对棋盘坐标创建并定位。
- 当前状态：功能已完成，等待运行验收。

## C - Check

- 检查方式：针对 `GameCommonUtility.cs`、`MainScene.cs`、`GameScene.cs` 执行 lints。
- 检查结果：无新增 linter 报错。
- 已知风险：若配置坐标系与当前转换规则不一致（例如 y 轴方向定义不同），需要微调坐标换算。

## Next Action

- 运行 MainScene 右滑进入 GameScene，验证全部碎片坐标是否与棋盘期望位置一致；若 y 轴方向反了我可立即调整换算。
