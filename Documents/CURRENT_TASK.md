# 当前任务

- 任务：微调 `CardBag002.prefab` 中更新切图对应的拼图位置
- 状态：已完成可确认切图的精确微调，发现 `piece_025.png` 资源内容异常待确认
- 更新时间：2026-08-09

## 用户意图

- 不重新生成整个 `CardBag002.prefab`。
- 对照现有效果图，只微调本次更新切图对应 Piece 的位置和原生尺寸。
- 不修改图片中的影子，不修改或重新烘焙描边。

## 工作记录

- 使用 Git 中原来已经对齐的旧切图与当前新切图做内存特征比对，没有运行关卡生成器。
- `piece_001.png` 相对旧图裁切偏移为 `(3, 7)`，将 `Piece11` 调整为位置 `(-437, 375)`、尺寸 `(404, 358)`。
- `piece_002.png` 相对旧图裁切偏移为 `(0, 6)`，将 `Piece21` 调整为位置 `(315, 347)`、尺寸 `(274, 416)`。
- `piece_003.png` 内容、尺寸和裁切位置均未变化，`Piece22` 保持不动。
- `piece_004.png` 相对旧图裁切偏移为 `(0, 0)`，将 `Piece23` 调整为位置 `(69, 365)`、尺寸 `(322, 360)`。
- `piece_024.png` 相对旧图裁切偏移为 `(0, 0)`，将 `Piece37` 调整为位置 `(344, -399)`、尺寸 `(194, 294)`。
- `piece_025.png` 已从原来的底部小鸭内容变成顶部橡皮鸭，并与未修改的 `piece_011.png` 内容重复；为避免将 `Piece38` 错误叠到 `Piece26` 上，暂未修改 `Piece38`。
- 保留了用户在 Prefab 中已有的 Piece Alpha 修改，没有触碰 Shadow、描边资源或其他节点。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag002.prefab`
- `Documents/CURRENT_TASK.md`

## 验证

- 新旧切图特征比对确认 `piece_001/002/003/004/024` 的像素内容可 100% 对齐，并据此换算 Prefab 坐标。
- `git diff --check -- Assets/Resources/CardBagPrefabs/CardBag002.prefab` 通过；仅有工作区换行符提示。
- 尚未在 Unity Prefab Mode 或 Play Mode 中目视验收。

## 下一步

1. 确认 `piece_025.png` 是否误覆盖；如果是，恢复正确的底部小鸭切图后，再按同样方式只微调 `Piece38`。
2. 在 Unity 中打开 `CardBag002.prefab`，目视检查五个已调整 Piece 与效果图的接缝。

## 数据说明

- 本次没有修改 JSON、SQLite 或业务数据结构，不需要删除本地存储。
