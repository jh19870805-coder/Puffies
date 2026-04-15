# SPEC 状态面板

- Task: GameScene 分组拖拽吸附拼图流程
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
- 新需求：将这批按配置创建的碎片透明度设为 0，作为凹槽。
- 新需求：左侧创建第一组可拖动碎片，支持吸附/回弹，组完成后下一组，全部完成输出游戏结束。
- 新需求：创建新组贴图时，保留 gameboard 和已吸附到凹槽的贴图不动。
- 新需求：每次进游戏后，gameboard 和所有凹槽只创建一次；后续每组贴图创建时不改变它们。
- 问题反馈：创建第二组碎片时 gameboard 会闪动。

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
  - 已将按配置创建的碎片透明度统一设置为 `0`，用于凹槽展示。
  - 已实现左侧可拖拽分组碎片、吸附到凹槽、回弹到起始位置、分组推进与通关日志。
  - 已将“已吸附碎片”迁移到独立根节点，切换下一组时不会被清理。
  - 已增加棋盘/凹槽初始化保护，单次进场仅创建一次，后续只刷新左侧待拖拽组。
  - 已修复组切换闪动：移除“每组切换时相机重新适配”步骤，避免画面跳变。
- 进行中：
  - 等待你验证吸附半径与拖拽手感是否符合预期。
- 未完成：
  - 如需进一步调阈值、增加滑动反馈动画或音效，进入下一轮调整。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- MainScene 与 GameScene 运行后都能看到页面完整内容，不出现内容裁切。
- 在保持现有布局逻辑前提下，增加相机自动框选机制。
- GameScene 需按配置生成全部碎片并正确对齐到棋盘相对坐标。
- 增加分组拖拽玩法流程：吸附/回弹/自动下一组/全部完成输出游戏结束。
- 验收标准：可完成全部分组并输出“游戏结束”。

## P - Plan

- 在 `GameCommonUtility` 增加通用相机框选方法。
- MainScene：按背景+卡包精灵联合边界自适配相机。
- GameScene：按棋盘+第一组碎片联合边界自适配相机。
- GameScene：将“第一组左列展示”替换为“全量碎片按配置坐标展示”。
- 新增拖拽状态机：命中检测、拖拽更新、吸附判定、组完成推进。

## E - Execute

- 已完成：`GameCommonUtility` 新增 `FitOrthographicCameraToRenderers(...)`。
- 已完成：MainScene 在创建背景与包图后自动框选可视范围。
- 已完成：GameScene 在创建棋盘与第一组碎片后自动框选可视范围。
- 已完成：GameScene 读取全部分组碎片，按 `x/y` 相对棋盘坐标创建并定位。
- 已完成：GameScene 左侧分组拖拽与吸附流程。
- 当前状态：功能已完成，等待运行验收。

## C - Check

- 检查方式：针对 `GameCommonUtility.cs`、`MainScene.cs`、`GameScene.cs` 执行 lints。
- 检查结果：无新增 linter 报错。
- 已知风险：吸附半径目前为固定值 `0.35` 世界单位，不同资源尺度下可能需要微调。

## Next Action

- 运行 MainScene 进入 GameScene，完整体验拖拽吸附与分组推进；如需调整吸附半径或左侧布局间距可直接给目标值。
