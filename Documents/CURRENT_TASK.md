# 当前任务

- 任务：无 JSON 自动生成 CardBag017 Prefab
- 状态：已生成顺序节点，等待用户手工分组
- 更新时间：2026-07-23

## 用户意图

- 使用 `Assets/UI/CardBags/Previews/CardBag017.png`、`CardBag017/background_base.png` 和透明碎图，在不依赖 `unity_layout.json` 的情况下还原拼图布局。
- 参考现有 `CardBagNNN.prefab` 的结构和命名，创建 `Assets/Resources/CardBagPrefabs/CardBag017.prefab`。
- 后续允许通过手工修改碎图名称表达 Piece 分组。

## 已完成

- 新增 `CardBagPrefabGeneratorEditor`，使用碎图保留的裁切 RGB 像素在 `background_base.png` 中定位，并使用 Alpha 作为运行时 Piece 形状。
- 生成前校验预览图与棋盘尺寸、Piece 文件、Sprite 导入设置、重复对象名和像素匹配置信度。
- 已生成 `CardBag017.prefab`：画布和 GameBoard 为 `1316 x 1316`，包含 `BoardTitle` 和 37 个透明槽位 Image。
- BoardTitle 和 Piece 均位于 `GameBoard` 下。本次未改名的 `piece_001` 到 `piece_037` 依次生成 `Piece001` 到 `Piece037`，不自动推断游戏分组。
- 删除017旧的 `Group01.png` 到 `Group05.png`，防止未分组节点误用旧蒙版；用户完成手工分组后再重新烘焙。
- 菜单 **Puffies -> Puzzles -> Generate CardBag017 From Images** 可重复生成当前试点 Prefab；以后使用 `PieceNN.png` 命名时会优先保留文件名中的游戏分组。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs` 及 `.meta`
- `Assets/Resources/CardBagPrefabs/CardBag017.prefab`
- 删除 `Assets/Resources/Generated/PuzzleOutlines/CardBag017/` 下不再匹配当前节点命名的旧蒙版。
- Unity 同时保存了编辑器内已有的 `CardBag002/009` Piece 层级调整，并重新烘焙了 CardBag009 描边。
- 更新 `specs/puzzle-outline.md` 和 `Documents/PROJECT_CONTEXT.md`。

## 决策

- 定位基准使用无青色切割线的 `background_base.png`；`Previews/CardBagNNN.png` 只负责尺寸和后续视觉校验，不进入 Prefab 运行时引用。
- Prefab 根 Image 继续使用通用 `BgCardBoard.png`，`GameBoard` 使用当前卡包完整成图。
- Piece 文件名为 `PieceNN.png` 或 `PiecesNN.png` 时，`NN` 直接成为运行时对象编号；`piece_###.png` 只生成从 `Piece001` 开始的顺序名称，不负责分组。
- 不恢复用户已经删除的旧017 GameBoard 和 `PiecesNN` 切图。

## 验证

- Unity 2022.3.62f2 隔离批处理成功生成 Prefab，37张碎图均为 `100.00%` 像素匹配。
- Prefab 包含1个根、1个 GameBoard、1个 BoardTitle、37个 Piece；BoardTitle 和37个 Piece 使用同一 GameBoard 父节点。
- Prefab 的40个 Sprite GUID 与根背景、GameBoard、BoardTitle 和37张碎图一一对应，无缺失或额外引用。
- 顺序命名状态不生成描边；旧的五张017描边已删除。
- `dotnet build Puffies.sln --no-restore` 完成，runtime、first-pass 和 Editor 程序集均为0警告、0错误。
- `git diff --check` 未发现空白错误。
- 当前顺序节点尚未形成有效游戏分组，不能进行正式 GameScene Play Mode 回归。

## 下一步

1. 用户在 Unity 中将 `Piece001` 到 `Piece037` 手工改为正式的分组编号。
2. 执行 **Puffies -> Puzzles -> Bake Outline Masks**，再从 MainScene 进入 PackId 17 验证拖放和描边切换。
3. 分组流程确认后，将试点菜单扩展为可选择 PackId 的批量生成窗口。

## 恢复提示

继续 Puffies 的无 JSON CardBag 生成工作。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；CardBag017 已生成 `Piece001` 到 `Piece037`，下一步等待用户手工改成正式游戏分组，然后重新烘焙描边并做 GameScene 回归。
