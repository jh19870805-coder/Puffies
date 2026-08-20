# 当前任务

- 任务：CardBag 棋盘与贴纸状态投影规则
- 状态：Prefab 批量设置与运行时切换已完成，等待 Play Mode 目视验证
- 更新时间：2026-08-20

## 用户意图

- 所有 `CardBagXXX.prefab` 的 `BoardTitle` 和 `GameBoard` 使用 `IngameCoverShadow01`。
- `CardBagXXX.prefab` 内的 `PieceGGII` 是棋盘凹槽，只在正确拼入后显示，因此固定使用 `IngameCoverShadow03`。
- GameScene 从凹槽纹理创建的运行时初始 Piece 使用 `IngameCoverShadow04`。
- Piece 松手后未正确吸附、放到桌面或错误回弹时使用 `IngameCoverShadow02`。
- Piece 正确吸附、恢复历史进度或完整显示为已完成时使用 `IngameCoverShadow03`。

## 工作记录

- 已通过 Unity Editor 批量处理 `Assets/Resources/CardBagPrefabs/CardBag001-023.prefab`。
- 23 个 Prefab 共处理 886 个 Image：46 个 `GameBoard/BoardTitle` 绑定 01，840 个凹槽 `PieceGGII` 绑定 03，并全部添加 `PackCoverShadowEffect`。
- Unity 批处理日志：`prefabs=23, changedPrefabs=23, changes=1772, failed=0`。
- `GameScene` 在创建托盘 SpriteRenderer 后使用 04；松手失败或桌面放置切换 02；确认正确吸附时切换 03，并在提交棋盘后让对应 UGUI Image 保持 03。
- 所有凹槽 Image 始终保持 03；未完成凹槽只通过隐藏或 Alpha 0 控制，不再切换成 04。已正确放置的历史 Piece、已完成分组和结算完整棋盘显示时同样使用 03。
- 凹槽 Image 的代码入口已收紧为 `ApplyPlacedPieceImageShadow`，只能应用 03；`Initial/Loose/Placed` 三态只用于运行时 SpriteRenderer，防止后续再次把凹槽误当作初始碎片。
- `PackCoverShadow.shader` 增加 SpriteRenderer 专用变体。运行时使用 FullRect Sprite 和按 PPU 计算的顶点留白，复用 02/03/04 的原始美术参数，避免直接使用 UI 材质时贴图缩小或投影被裁切。
- 新增菜单 `Puffies -> Apply CardBag Shadow Materials`；关卡生成器创建新 CardBag Prefab 时也会自动为凹槽应用 03。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag001.prefab` 至 `CardBag023.prefab`
- `Assets/Resources/PackCoverShadow.shader`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 四个 `IngameCoverShadow01-04.mat` 的美术参数保持不变；运行时只克隆材质并启用 SpriteRenderer Shader 变体。
- Piece 的碰撞体仍按原 Sprite 创建，投影用 FullRect 运行时 Sprite 只影响渲染，不改变凹槽、碰撞、吸附和持久化编号。
- 窗口失焦取消不视为玩家松手，保持 Piece 当前投影状态；真正执行松手判定时才切换 02 或 03。
- `BagShadow.prefab` 是用户当前独立修改，本轮不覆盖、不回滚。

## 验证

- Unity Editor 批量处理：23 个 Prefab 全部成功，失败 0。
- 序列化审计：23 个 Prefab 当前包含 46 个 01、840 个 03、0 个 04；Prefab 中的 04 错误绑定已全部清除。
- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- Unity Editor 日志未发现 C# 或 Shader 编译错误。
- 尚未进行 GameScene Play Mode 目视验证。

## 关联待办

- 继续验证托盘 `TrayScale <= 1`、拿起恢复 `DragScale`、桌面放置、回托盘、托盘滑动、失焦恢复和棋盘到托盘 `10%` 间距。
- 卡包编号迁移仍需补齐 `CardPacks.csv` 的 005/006/007/010/023，并确认 CardBag005 包头来源。

## 下一步

1. 在 GameScene 观察新组初始托盘 Piece，确认使用 04 且原图尺寸没有缩小。
2. 将 Piece 放到桌面及错误位置，确认松手后使用 02，回弹和红色反馈仍正常。
3. 正确吸附并重新进入有历史进度的卡包，确认棋盘 Piece 使用 03。
4. 检查 GameBoard、BoardTitle 使用 01 时投影没有被父级 Canvas 或棋盘边界裁切。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；在 GameScene Play Mode 依次验证运行时初始 Piece 使用 04、失败松手使用 02、Prefab 凹槽及正确吸附/恢复进度使用 03，以及 GameBoard/BoardTitle 使用 01。不要覆盖用户对 `BagShadow.prefab` 的独立修改。
