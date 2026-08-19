# 当前任务

- 任务：CardBag022 背板视觉异常
- 状态：已修复，等待编译与 Play Mode 复验
- 更新时间：2026-08-19

## 用户意图

- 修复 CardBag022 游戏页面右下角出现一块明显缩小的矩形边界、未与灰色棋盘底层形成一致视觉的问题。
- 用户所说的背板是卡包棋盘内容，不是底部黑色 `PieceBoard` 托盘。

## 工作记录

- 已确认前一次将问题识别为 `PieceBoard` 宽度错误、随后识别为 `GameBoard.png` 源图透明区，均属于误判；未保留这两类修改。
- 通过 Unity Play Mode 自动截图与运行时 Rect 记录确认：`CardBag022` 根保持 Prefab 设计尺寸 `2600 x 3920`，但初始化逻辑将 `GameBoard` 改成了导入 Sprite 的 `1358 x 2048`，屏幕宽高均缩小到约 `52.25%`。
- 根因是 `SyncEditorLayoutToSprites()` 对 `GameBoard` 调用了通用 Sprite 原生尺寸同步；022 的大图受 TextureImporter `maxTextureSize=2048` 降采样，因此运行时错误覆盖了 Prefab 中正确的设计画布尺寸。
- 初始化现改为 `SyncGrooveLayoutToSprites()`：只同步 Piece 槽位尺寸，不再修改 `GameBoard` RectTransform；所有卡包继续保留 Prefab 生成时记录的原始画布尺寸。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- 不再修改 `PieceBoard` 或托盘逻辑。
- 不修改 022 源图、Prefab 或 TextureImporter；运行时必须保留 Prefab 的原始棋盘设计尺寸，不能用可能经过导入降采样的 `sprite.rect.size` 覆盖。
- Piece 槽位仍沿用现有 Sprite 尺寸同步，避免改变切图更新后的槽位行为。

## 验证

- 修改前 Unity Play Mode 实测：根 `CardBag022=2600 x 3920`，子 `GameBoard=1358 x 2048`，准确复现约一半尺寸的异常。
- 已确认源图 Alpha 没有运行时可见的内部矩形，资源本身无需修改。
- 待重新编译并运行同一自动化 Play Mode 捕获，确认根与 GameBoard 屏幕矩形完全一致。

## 下一步

1. 运行 C# 编译和 Unity Play Mode 视觉复验。
2. 确认无回归后提交并推送全部改动。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；验证 GameScene 不再用降采样后的 Sprite 原生尺寸覆盖 GameBoard 的 Prefab 设计尺寸。
