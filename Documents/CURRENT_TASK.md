# 当前任务

- 任务：CardBag022 背板视觉异常
- 状态：已完成并通过 Unity Play Mode 复验
- 更新时间：2026-08-20

## 用户意图

- 修复 CardBag022 游戏页面中棋盘内容明显缩小、无法与灰色棋盘底层对齐的问题。
- 修复完成后提交全部改动并推送到当前分支。

## 工作记录

- Unity Play Mode 修复前实测：`CardBag022` 根为 `2600 x 3920`，`GameBoard` 被运行时改为 `1358 x 2048`。
- 根因是 `GameBoard.png` 受 TextureImporter `maxTextureSize=2048` 降采样后，`SyncEditorLayoutToSprites()` 又用 `sprite.rect.size` 覆盖了 Prefab 中正确的设计尺寸。
- 初始化现改为 `SyncGrooveLayoutToSprites()`：只同步 Piece 槽位尺寸，不再修改 `GameBoard` RectTransform。
- Unity 重新导入合并后的资源时发现 CardBag007 与 CardBag006 存在三个重复 GUID，已保留 Unity 为 CardBag007 自动生成的新 GUID，并将 CardBag007 Prefab 的标题图引用同步指向新 GUID，避免其他设备重复发生冲突或显示成 CardBag006 标题。
- 已删除本次自动 Play Mode 复验使用的临时 Editor 脚本。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Resources/CardBagPrefabs/CardBag007.prefab`
- `Assets/Resources/CardBagPrefabs/CardBag007.prefab.meta`
- `Assets/UI/CardBags/CardBag007.meta`
- `Assets/UI/CardBags/CardBag007/BoardTitle.png.meta`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 不修改 `PieceBoard`、CardBag022 源图、Prefab 或 TextureImporter。
- GameBoard 始终保留 Prefab 生成时记录的原始画布尺寸；Piece 槽位继续按 Sprite 尺寸同步。
- CardBag007 的新 GUID 是对仓库既有重复 GUID 的确定性修复，不属于 CardBag022 视觉逻辑改动。

## 验证

- Unity 2022.3.62f2c1 无窗口 Play Mode：`CardBag022 root=(2600.00, 3920.00), gameBoard=(2600.00, 3920.00)`，通过。
- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `git diff --check`：通过。

## 下一步

1. 在正常游戏画面中目视确认 CardBag022 灰色棋盘与内容边界完全贴合。
2. 当前记录完成后，可切换到下一项需求。

## 恢复提示

继续 Puffies 工程。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；CardBag022 尺寸修复已完成，除非用户反馈目视异常，否则开始处理最新需求。
