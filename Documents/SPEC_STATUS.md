# SPEC 状态面板

- Task: 主流程交互完善、GameScene 布局改造与工具类重构
- Status: In Progress
- Updated At: 2026-04-23 13:50
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
- 新需求：整理整个工程，能统一接口的代码尽量合并，删除重复冗余实现。
- 新需求：添加一个播放游戏动画的工具类。
- 新需求：将 GameManager 从单例组件模式改为 Unity 常用工具类模式。
- 新需求：后续资源更新统一放置到源目录并由构建流程同步。
- 新需求：整理目录结构，功能型类归档到 `Tools`。
- 新需求：MainScene 卡包布局改为 30% 缩放、左上起排、5 列 x 3 行分页均分。
- 新需求：MainScene 卡包交互改为“点击（抬起判定）聚焦放大居中，再执行滑动进入游戏”。
- 新需求：GameScene 新增 `PieceBg`（九宫拉伸），并按规则承载待拖拽贴片。
- 新需求：贴片层级高于 `PieceBg`，托盘中自适应缩放，拖拽时还原原始尺寸，未吸附回托盘后恢复托盘缩放。
- 新需求：持续整理冗余代码，通用方法下沉到 `GameCommonUtility`，并收敛状态结构。
- 新需求：当 `PieceBg` 上仅剩一个待拖拽贴图时，拿起后 `PieceBg` 向下滑出屏幕；回托盘或创建下一组前上滑回原位。
- 新需求：`GameBoard` 移动对齐时 `PieceBg` 不跟随，且 `PieceBg` 底部固定对齐游戏页面底部。
- 新需求：Win32 平台取消游戏窗口 `16:9` 锁定，允许自由拖拽调整窗口尺寸。
- 新需求：Win32 平台增加自定义鼠标（`BasicUi/ImgHand.png`），仅在背景范围内替换系统鼠标，超出范围切回系统鼠标并保留自定义鼠标最后位置。
- 问题反馈：自定义鼠标与系统鼠标未对齐，且背景范围判定偏小。

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
  - 已将图片加载与屏幕坐标转换收敛到 `GameCommonUtility` 统一接口。
  - 已删除 `MainScene` / `GameScene` 内重复的 `CreateSpriteByPath` 与 `ScreenToWorld` 实现。
  - 已将跨场景的“创建 Sprite 对象”逻辑收敛到 `GameCommonUtility.CreateSpriteRendererObject(...)`。
  - 已将鼠标/触屏输入分发模板收敛到 `GameCommonUtility.ProcessPointerInput(...)`。
  - 已删除 MainScene / GameScene 内重复的 `SetupMainCamera` 包装方法，改为直接调用公共接口。
  - 已删除 MainScene / GameScene 内重复相机初始化中间层，全部直接使用 `GameCommonUtility.SetupOrthographicCamera(...)`。
  - 已将 `GameManager` 重构为 `static` 工具类，并清理所有 `CreateInstance/Instance` 调用点。
  - 已新增 `GameAnimationUtility`，支持位移、缩放、透明度动画与缓动。
  - 已将 `GameAnimationUtility` 与 `GameCommonUtility` 移入 `Assets/Tools`，并迁移 `WindowAspectController` 到 `Tools` 目录。
  - 已将构建同步脚本重命名为 `StreamingBuildSync`。
  - 已完成工作区显示优化：常见构建产物与非日常目录在 `.vscode/settings.json` 中隐藏显示（仅显示层，不影响文件本体）。
  - 已实现 MainScene 卡包网格化布局：30% 缩放、左上起排、每页 5 列 x 3 行，按背景范围均分坐标。
  - 已实现 MainScene 两段交互：抬起确认点击后卡包缓动放大并居中；聚焦后滑动进入对应 bagId 的 GameScene。
  - 已加严点击判定：移动超阈值判为滑动，不触发点击聚焦。
  - 已实现 GameScene `PieceBg` 创建流程：宽度对齐 `GameBoard`、高度为 `GameBoard` 的 1/4、底部与 `GameBoard` 底部对齐。
  - 已完成 `PieceBg` 可见性修复方案：九宫边框 + 填充层双层结构，避免仅显示四角点。
  - 已完成贴片托盘规则：贴片层级高于 `PieceBg`；托盘中自适应（最大高度 90%）；拖拽还原原始尺寸；未吸附回托盘自动恢复托盘缩放。
  - 已完成第二轮通用方法下沉：`GameScene` 中坐标换算/透明度/托盘缩放/宽度计算已迁移到 `GameCommonUtility`。
  - 已完成 `GameScene` 状态聚合重构（resources / board / drag）并将状态类型定义迁移到 `GameDefine`。
  - 已完成吸附阈值第三轮微调：`GameScene` 吸附半径由固定值改为“基于碎片尺寸的自适应阈值（含上下限）”。
  - 已完成 `PieceBg` 单贴图交互动画：单贴图拿起时 `PieceBg` 下滑出屏，回托盘或切下一组前上滑回原位。
  - 已完成 `GameScene` 布局约束调整：`GameBoard` 对齐移动时不再带动 `PieceBg`；`PieceBg` 常态固定锚定页面底部并与填充层同步。
  - 已完成 Win32 窗口策略调整：`WindowAspectController` 关闭 `16:9` 强制锁定，窗口支持任意尺寸拖拽。
  - 已完成 Win32 自定义鼠标控制：背景范围内显示 `ImgHand`，范围外恢复系统鼠标并保持自定义鼠标停留在最后位置。
  - 已完成鼠标体验修复：自定义鼠标改为像素坐标直跟，命中范围改为屏幕矩形判定并增加边界补偿。
- 进行中：
  - 已尝试在当前环境执行 Unity 自动化回归，但未检测到 Unity Editor 可执行文件，无法直接启动 MainScene/GameScene 进行交互验证。
  - 等待你在本机运行回归，确认 MainScene 新交互、GameScene 托盘/拖拽体验与 Win32 鼠标/窗口行为符合预期。
- 未完成：
  - 如需继续调优：卡包聚焦动画时长、拖拽吸附阈值、托盘视觉样式与分页切换交互。

## Resume Prompt

- 在另一台设备对 Codex 说：`继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

- MainScene 与 GameScene 运行后都能看到页面完整内容，不出现内容裁切。
- 在保持现有布局逻辑前提下，增加相机自动框选机制。
- GameScene 需按配置生成全部碎片并正确对齐到棋盘相对坐标。
- 增加分组拖拽玩法流程：吸附/回弹/自动下一组/全部完成输出游戏结束。
- 统一重复接口并删除冗余实现，保持功能行为不变。
- MainScene 需支持网格化卡包布局与“点击聚焦 + 滑动进入”两段交互。
- GameScene 需提供可视化托盘背景（PieceBg）与托盘内贴片自适应缩放逻辑。
- 验收标准：可完成全部分组并输出“游戏结束”，且无新增 lint 报错。

## P - Plan

- 在 `GameCommonUtility` 增加通用相机框选方法。
- MainScene：按背景+卡包精灵联合边界自适配相机。
- GameScene：按棋盘+第一组碎片联合边界自适配相机。
- GameScene：将“第一组左列展示”替换为“全量碎片按配置坐标展示”。
- 新增拖拽状态机：命中检测、拖拽更新、吸附判定、组完成推进。
- 将跨场景共用能力（Sprite 加载、屏幕坐标转换）统一抽到 `GameCommonUtility`。
- MainScene：实现卡包网格布局与点击聚焦后滑动进入的输入状态机。
- GameScene：新增 PieceBg 双层可视化结构（九宫边框 + 填充层）并承载贴片托盘布局。
- 重构：将 `GameScene` 离散状态字段聚合为 `resources / board / drag`，并将状态类型定义迁移到 `GameDefine`。

## E - Execute

- 已完成：`GameCommonUtility` 新增 `FitOrthographicCameraToRenderers(...)`。
- 已完成：MainScene 在创建背景与包图后自动框选可视范围。
- 已完成：GameScene 在创建棋盘与第一组碎片后自动框选可视范围。
- 已完成：GameScene 读取全部分组碎片，按 `x/y` 相对棋盘坐标创建并定位。
- 已完成：GameScene 左侧分组拖拽与吸附流程。
- 已完成：重复接口统一与冗余实现清理。
- 已完成：MainScene 网格化卡包布局与点击聚焦/滑动进入两段交互。
- 已完成：GameScene `PieceBg` 创建、对齐、尺寸与托盘承载规则。
- 已完成：贴片托盘自适应缩放与拖拽缩放还原机制。
- 已完成：`GameScene` 状态聚合与状态类型迁移至 `GameDefine`。
- 当前状态：核心开发已完成，等待你做一轮完整功能回归。

## C - Check

- 检查方式：针对 `GameCommonUtility.cs`、`MainScene.cs`、`GameScene.cs` 执行 lints。
- 检查结果：无新增 linter 报错。
- 自动化回归补充：当前会话环境缺少 Unity Editor 可执行程序，无法通过 batchmode 或编辑器命令完成场景级交互回归。
- 已知风险：
  - 吸附半径已改为自适应策略；若后续引入极端尺寸碎片，可能仍需微调 `SnapDistanceSizeRatio/Min/Max`。
  - 贴片托盘横向布局在未来引入极端宽图时可能需要增加换行或分页策略。
  - 自定义鼠标当前命中基于背景屏幕矩形，若后续背景改为非矩形可交互区域，需升级为像素级或多边形命中。

## Next Action

- 在本机 Unity Editor 运行 MainScene -> GameScene 做完整回归，重点确认：卡包点击聚焦、滑动进入、`PieceBg` 底部锚定与单贴图下滑/回位、贴片拖拽吸附/回弹与组推进；并在 Win32 验证窗口可自由缩放与自定义鼠标范围切换。回归后反馈异常点（含复现步骤），我继续按问题逐项修复。
