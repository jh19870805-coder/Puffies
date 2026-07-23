# 当前任务

- 任务：无 JSON 批量生成 CardBag Prefab
- 状态：批量工具已完成，等待 Unity 界面确认和 CardBag017 手工分组
- 更新时间：2026-07-23

## 用户意图

- 使用 `Assets/UI/CardBags/Previews/CardBag017.png`、`CardBag017/background_base.png` 和透明碎图，在不依赖 `unity_layout.json` 的情况下还原拼图布局。
- 参考现有 `CardBagNNN.prefab` 的结构和命名，创建 `Assets/Resources/CardBagPrefabs/CardBag017.prefab`。
- 后续允许通过手工修改碎图名称表达 Piece 分组。
- 将已验证的017试点扩展为可自动发现新资源、选择多个 PackId 并批量生成的正式编辑器工具。

## 已完成

- 新增 `CardBagPrefabGeneratorEditor`，使用碎图保留的裁切 RGB 像素在 `background_base.png` 中定位，并使用 Alpha 作为运行时 Piece 形状。
- 生成前校验预览图与棋盘尺寸、Piece 文件、Sprite 导入设置、重复对象名和像素匹配置信度。
- 已生成 `CardBag017.prefab`：画布和 GameBoard 为 `1316 x 1316`，包含 `BoardTitle` 和 37 个透明槽位 Image。
- BoardTitle 和 Piece 均位于 `GameBoard` 下。本次未改名的 `piece_001` 到 `piece_037` 依次生成 `Piece001` 到 `Piece037`，不自动推断游戏分组。
- 删除017旧的 `Group01.png` 到 `Group05.png`，防止未分组节点误用旧蒙版；用户完成手工分组后再重新烘焙。
- 菜单 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images** 打开批量窗口，扫描一级 `CardBagNNN` 资源目录并显示 Piece 数、Prefab 状态和缺失资源。
- 窗口默认只选择尚无 Prefab 的有效新资源，支持选择全部有效资源；覆盖已有 Prefab 前会明确警告手工层级和 Piece 分组将被替换。
- 批量执行按 PackId 逐个生成，一个卡包失败不会阻断其他卡包；结束后汇总成功和失败项。
- 批量生成不自动烘焙描边。`Piece001` 这类三位顺序名称会被描边烘焙器跳过，避免 `Piece010` 被误判成正式第1组；没有正式分组时同步删除该卡包的旧描边输出。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs` 及 `.meta`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/CardBagPrefabs/CardBag017.prefab`
- 删除 `Assets/Resources/Generated/PuzzleOutlines/CardBag017/` 下不再匹配当前节点命名的旧蒙版。
- Unity 同时保存了编辑器内已有的 `CardBag002/009` Piece 层级调整，并重新烘焙了 CardBag009 描边。
- 更新 `specs/puzzle-outline.md` 和 `Documents/PROJECT_CONTEXT.md`。

## 决策

- 定位基准使用无青色切割线的 `background_base.png`；`Previews/CardBagNNN.png` 只负责尺寸和后续视觉校验，不进入 Prefab 运行时引用。
- Prefab 根 Image 继续使用通用 `BgCardBoard.png`，`GameBoard` 使用当前卡包完整成图。
- Piece 文件名为 `PieceNN.png` 或 `PiecesNN.png` 时，`NN` 直接成为运行时对象编号；`piece_###.png` 只生成从 `Piece001` 开始的顺序名称，不负责分组。
- 批量窗口只扫描严格匹配 `CardBagNNN` 的一级目录，并要求 `background_base.png`、`Previews/CardBagNNN.png` 和至少一张合法 Piece PNG。
- 已存在的 Prefab 默认不勾选，防止批量操作意外覆盖手工分组；需要重建时由用户主动选择并二次确认。
- 不恢复用户已经删除的旧017 GameBoard 和 `PiecesNN` 切图。

## 验证

- Unity 2022.3.62f2 隔离批处理成功生成 Prefab，37张碎图均为 `100.00%` 像素匹配。
- Prefab 包含1个根、1个 GameBoard、1个 BoardTitle、37个 Piece；BoardTitle 和37个 Piece 使用同一 GameBoard 父节点。
- Prefab 的40个 Sprite GUID 与根背景、GameBoard、BoardTitle 和37张碎图一一对应，无缺失或额外引用。
- 顺序命名状态不生成描边；旧的五张017描边已删除。
- `dotnet build Puffies.sln --no-restore` 完成，runtime、first-pass 和 Editor 程序集均为0警告、0错误。
- `git diff --check` 未发现空白错误。
- 只读扫描识别到 6 个 `CardBagNNN` 目录：017资源完整且 Prefab 已存在，默认不选中；001/002/003/008/009 缺少新流程要求的背景或预览图，显示缺失并禁止选择。
- 检查时发现旧烘焙器曾将017顺序节点误生成 `Group01` 到 `Group03`；确认这些不是正式分组结果后已删除，017当前无残留描边目录。
- Unity 当前持有工程锁，本轮未启动第二个 Unity 批处理实例；需在现有编辑器完成菜单窗口的视觉确认。
- 当前顺序节点尚未形成有效游戏分组，不能进行正式 GameScene Play Mode 回归。

## 下一步

1. 在现有 Unity 编辑器打开 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images**，确认列表、默认选择和缺失状态展示。
2. 用户将 CardBag017 的 `Piece001` 到 `Piece037` 手工改为正式的分组编号。
3. 执行 **Puffies -> Puzzles -> Bake Outline Masks**，再从 MainScene 进入 PackId 17 验证拖放和描边切换。

## 恢复提示

继续 Puffies 的无 JSON CardBag 批量生成工作。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；批量窗口已经完成，CardBag017 已生成 `Piece001` 到 `Piece037`，下一步先确认窗口展示，再等待用户手工改成正式游戏分组并重新烘焙描边。
