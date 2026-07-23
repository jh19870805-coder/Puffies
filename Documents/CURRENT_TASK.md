# 当前任务

- 任务：修正分阶段拼图描边与真实缝隙的偏差
- 状态：已完成，待用户 Play Mode 复测
- 更新时间：2026-07-23

## 用户意图

- 修正截图中 `CardBag003` 第 4 组描边与实际浅色缝隙不重合的问题。
- 保持现有分组规则、棋盘位置、拖拽和运行时显示逻辑不变。

## 已完成

- 确认截图对应 `CardBag003` 第 4 组：第 1 至第 3 组已完成，灰色区域为当前组。
- 最终拼图外边界改为优先读取 `GameBoard.png` 的透明挖空 Alpha，不再从 Piece Alpha 并集和灰度色差推测。
- 后续组与已完成区域的接触边改为直接读取已完成 Piece 的真实 Alpha 外边界，不再画在当前组另一侧的边界上。
- GameBoard 没有有效透明挖空或与 Piece 区域不重合时，保留 Piece Alpha 并集回退。
- `FinalBoundaryAssignmentRadius=12` 只用于判断最终边界属于哪个组，不移动边界坐标；`ContactSearchRadius=6` 只用于判断组间相邻关系。
- 在隔离的临时 Unity 工程中重新烘焙 6 个已分组 CardBag 的 24 张蒙版，并将 PNG 同步回正式资源，保留原 `.meta` 和 GUID。

## 修改文件

- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag001` 至 `CardBag017` 下共 24 张 `GroupNN.png`
- `specs/puzzle-outline.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 最终外边界以 GameBoard 可见透明缝隙为准。
- 组间接触边以已完成 Piece 的可见 Alpha 边缘为准。
- 搜索半径只决定边界归属和相邻关系，不用于平移、内收或外扩描边。
- 不修改 GameScene 的描边 RectTransform、棋盘移动逻辑或 Piece 放置坐标。

## 验证

- `dotnet build Puffies.sln --no-restore`：runtime、first-pass 和 Editor 程序集 0 警告、0 错误。
- Unity 2022.3.62f2 临时工程批处理烘焙完成：7 个 Prefab 中 6 个已分组卡包生成 24 张蒙版；未分组的 CardBag022 正确跳过。
- CardBag002、003、008、009、017 的 GameBoard 挖空与 Piece 区域重合率为 `99.7%..100.0%`；CardBag001 正确使用回退。
- `CardBag003/Group04.png` 包含 8,460 个描边像素。
- 将 Group04 与 GameBoard Alpha 合成检查后，右侧和底部黑线与透明挖空边缘重合；左侧接触边来自已完成 Piece Alpha。
- 不需要重置 JSON 或 SQLite 本地数据。

## 下一步

1. Unity 获得焦点并完成资源导入后，在 Play Mode 进入 CardBag003 第 4 组，确认截图位置的黑线与浅色缝隙重合。
2. 若仍有局部偏差，请提供同一位置的新截图；后续只调整 Alpha 阈值，不再修改棋盘或描边整体位置。

## 恢复提示

继续 Puffies 开发。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；描边烘焙已改为最终外边界读取 GameBoard Alpha、组间接触边读取已完成 Piece Alpha，下一步是在 Unity Play Mode 复测 CardBag003 第 4 组。
