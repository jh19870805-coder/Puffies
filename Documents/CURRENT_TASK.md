# 当前任务

- 任务：修复多关卡默认描边断裂
- 状态：已完成
- 更新时间：2026-08-08

## 用户意图

- 修复当前组外边界与已完成组接触边在交汇处断裂的问题。
- 问题存在于多个关卡，应修复通用烘焙算法并批量更新蒙版，而不是单独修改某一关。

## 工作记录

- 截图对应 `CardBag008/Group02.png`，断点位于已完成组接触边与当前组最终外边界的交汇处。
- 移除了交汇双方各自 `24px` 的双向裁剪，以及跨分组删除已认领描边像素的逻辑。
- 当前组外边界继续使用距离和局部法线方向过滤；相邻组允许在真实交点共享少量外边界像素。
- 增加交汇端点桥接：只沿最终外边界与已完成区域边界组成的走廊寻找真实路径，最大路径 `64px`，走廊容差 `8px`，不使用跨空白直线。
- 通过当前 Unity 编辑器全量重烘焙 22 个 CardBag Prefab，生成 96 个有效分组；其中 87 张默认 `GroupNN.png` 发生变化。
- `CardBag020` 和 `CardBag022` 仍因 Piece 尚未正式分组而跳过；`GroupNN_Level.png` 与 `GroupNN_Stickers.png` 未发生变化。
- 上一项错误放置红色回弹修改保持原样，没有回退或覆盖。

## 修改文件

- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag002` 至 `CardBag019`、`CardBag021` 中发生变化的默认 `GroupNN.png`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不再通过预留空隙避免交点多画；运行时只显示当前阶段蒙版，因此相邻阶段共享交点像素是正确行为。
- 保留法线方向过滤以阻止切线方向误判；桥接只修复存在真实边界路径的邻近端点。
- 本次没有修改 SQLite 或 JSON 数据结构，不需要重置本地持久化文件。

## 验证

- `CardBag008/Group02.png` 烘焙前为分离的两段线，烘焙后接触边与顶部外边界连续闭合。
- 抽查 `CardBag008/Group03` 至 `Group06`，不存在真实边界路径的独立线段未被强行连接。
- Unity 日志记录 `Puzzle outline baker: baked 96 group mask(s) from 22 card bag(s).`。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `git diff --check` 通过；尚需在 Unity Play Mode 中目视复查实际缩放和 Bilinear 过滤后的显示效果。

## 下一步

1. 在 Unity 中复测 `CardBag008` 第 2 组，并随机抽查其他卡包的后续分组交汇处。

## 恢复提示

多关卡默认描边断裂已从通用烘焙算法修复，96 个有效分组已全量重烘焙。下一步在 Play Mode 中复测 CardBag008 第 2 组并抽查其他卡包。
