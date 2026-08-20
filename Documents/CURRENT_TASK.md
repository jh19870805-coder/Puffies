# 当前任务

- 任务：CardBag Prefab 扁平层级与棋盘背景平铺
- 状态：已完成代码、资源迁移和结构校验，等待 GameScene 目视验证
- 更新时间：2026-08-20

## 用户意图

- 所有 `CardBagXXX.prefab` 根 `Image` 的 Source Image 设为 `None`。
- 内容全部直属 CardBag 根节点，顺序为 `BoardTitle`、`BoardBgXX`、`GameBoard`、`PieceGGII`。
- `BoardBgXX` 默认使用 `BgCardBoard1.png`，从 GameBoard 左上角开始二维平铺，超出棋盘的末行和末列裁掉不显示。
- 高对比度继续按既有设置切换为 `BgCardBoard2.png`。
- Piece 分组、位置、吸附和投影逻辑保持不变。
- 卡包完整生成工具和已有 Piece 局部校准工具都兼容最新扁平结构。

## 工作记录

- 在 `CardBagPrefabGeneratorEditor.cs` 中新增 `CardBagHierarchyEditor`，菜单为 `Puffies -> Apply CardBag Hierarchy`。
- 背景使用直属根节点的 `RawImage`，按 `512 x 512` 纹理尺寸逐行平铺；末列和末行同时缩小 Rect 并调整 `uvRect`，保持 1:1 像素比例，不拉伸纹理，也不增加 Mask 父节点。
- 批量迁移保留原有 `BoardTitle`、`GameBoard` 和全部 `PieceGGII` 对象及其 Sprite、位置、材质、投影组件，只清空根 Sprite、重设父级与顺序并重建 `BoardBgXX`。
- 已通过 Unity Editor 迁移 `CardBag001-023.prefab`。背景数量按棋盘尺寸自动计算：普通卡包多数为 9 块，CardBag018/019 为 6 块，CardBag022 为 48 块。
- `GameScene.ApplyCardBoardBackground` 改为更新当前 CardBag 下全部 `BoardBgXX RawImage.texture`，根 Image 始终保持无 Sprite。
- GameScene 的 `GameBoard` 和 Piece 收集限定在当前加载的 CardBag 内；扁平结构下仍按 `PieceGGII` 编号分组。
- 完整生成器直接创建扁平节点并调用同一规范化器；`Update Existing Piece Layouts` 从 CardBag 根节点映射 Piece，继续只更新位置和尺寸。
- 规范化器保存前严格验证根 Sprite、直属父级、节点顺序、背景数量、默认纹理、位置、尺寸和 UV 裁切。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag001.prefab` 至 `CardBag023.prefab`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 使用 `RawImage.uvRect` 裁切最后一行和最后一列。普通 `Image` 在不增加 Mask 父节点的前提下无法同时裁切两个方向且保持纹理不拉伸。
- `BoardBgXX` 按从上到下、每行从左到右编号，位数至少两位。
- Prefab 中 `BoardTitle` 可按原生成规则缺省；存在时必须是第一个直属子节点。
- 运行时生成的描边仍可作为 `GameBoard` 子节点，不属于 Prefab 的制作层级约束。
- 本轮不改变 Piece 分组编号、RectTransform、吸附、描边烘焙和投影材质规则。

## 验证

- Unity 批量迁移与严格校验：`prefabs=23, changedPrefabs=23, failed=0`。
- 23 个 Prefab 均存在自动计算的 BoardBg；总数和每包棋盘尺寸匹配。
- CardBag002 抽查：根 Sprite 为 None，直属顺序为 `BoardTitle -> BoardBg01...09 -> GameBoard -> Piece`；末行高度为 `114px`，UV 高度为 `114/512 = 0.22265625`。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误，并包含运行时 `Assembly-CSharp` 编译。
- Unity Editor 日志未出现本轮 C# 编译错误。
- 尚未在 GameScene Play Mode 目视验证普通/高对比背景和完整一组拼图流程。

## 关联待办

- 上一任务的 Piece 拿起尺寸与 01/02/03/04 投影状态仍需在 Play Mode 做完整目视回归。
- 卡包编号迁移仍需补齐 `CardPacks.csv` 的 005/006/007/010/023，并确认 CardBag005 包头来源。

## 下一步

1. 在 GameScene 分别关闭和打开高对比度，确认背景块无接缝、无拉伸并完整裁在 GameBoard 范围内。
2. 用 CardBag002 和大棋盘 CardBag022 各完成一组，确认 Piece 分组、吸附、描边和投影状态不受扁平层级影响。
3. 在生成器创建或覆盖一个测试 CardBag，确认新资源直接生成相同扁平结构。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；在 GameScene 验证普通/高对比棋盘背景平铺，以及 CardBag002/CardBag022 的 Piece 分组、吸附、描边和投影回归。不要自动提交，用户明确要求提交时再提交并推送。
