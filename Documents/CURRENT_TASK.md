# 当前任务

- 任务：CardBag 棋盘与贴纸状态投影规则
- 状态：已修复拿起尺寸恢复与投影状态切换，等待 Play Mode 目视验证
- 更新时间：2026-08-20

## 用户意图

- 所有 `CardBagXXX.prefab` 的 `BoardTitle` 和 `GameBoard` 使用 `IngameCoverShadow01`。
- `CardBagXXX.prefab` 内的 `PieceGGII` 是棋盘凹槽，只在正确拼入后显示，因此固定使用 `IngameCoverShadow03`。
- GameScene 从凹槽纹理创建的运行时初始 Piece 使用 `IngameCoverShadow04`。
- Piece 松手后未正确吸附、放到桌面或错误回弹时使用 `IngameCoverShadow02`。
- Piece 正确吸附、恢复历史进度或完整显示为已完成时使用 `IngameCoverShadow03`。
- Piece 的 `DragScale` 必须按当前凹槽的实际屏幕显示尺寸计算，包含 `CardPacks.csv/BoardScale`；每次拿起时刷新该比例并切回 `IngameCoverShadow04`。

## 工作记录

- 已通过 Unity Editor 批量处理 `Assets/Resources/CardBagPrefabs/CardBag001-023.prefab`。
- 23 个 Prefab 共处理 886 个 Image：46 个 `GameBoard/BoardTitle` 绑定 01，840 个凹槽 `PieceGGII` 绑定 03，并全部添加 `PackCoverShadowEffect`。
- Unity 批处理日志：`prefabs=23, changedPrefabs=23, changes=1772, failed=0`。
- `GameScene` 在创建托盘 SpriteRenderer 后使用 04；松手失败或桌面放置切换 02；确认正确吸附时切换 03，并在提交棋盘后让对应 UGUI Image 保持 03。
- 所有凹槽 Image 始终保持 03；未完成凹槽只通过隐藏或 Alpha 0 控制，不再切换成 04。已正确放置的历史 Piece、已完成分组和结算完整棋盘显示时同样使用 03。
- 凹槽 Image 的代码入口已收紧为 `ApplyPlacedPieceImageShadow`，只能应用 03；`Initial/Loose/Placed` 三态只用于运行时 SpriteRenderer，防止后续再次把凹槽误当作初始碎片。
- 截图确认问题出现在 `CardBag021`：配置 `BoardScale=1.1`，但错误实现将 `DragScale` 固定保存为 `1`，因此拿起后稳定小约 10%。现已恢复按凹槽实际屏幕矩形计算 `DragScale`，并让 `BoardScale` 使用同一目标比例，避免正确吸附时二次缩放。
- `TrayScale` 改为 `Min(DragScale, 托盘90%容纳上限)` 的等比结果：配置目标较小时直接在托盘使用目标尺寸，配置目标较大或 Piece 过高时只在托盘缩小，拿起后恢复完整目标尺寸。
- `TryBeginDrag` 每次刷新实际 `DragScale/BoardScale` 并切换到 04；桌面态 02 再次拿起会同步恢复尺寸和材质。
- 创建运行时 Piece 时先用原始凹槽 Sprite 创建精确 Piece/凹槽碰撞体，再切换 FullRect 投影显示 Sprite，最后统一按 FullRect 与凹槽屏幕矩形计算 Scale；消除创建时 Tight Mesh、拿起时 FullRect 导致的尺寸基准差异，同时避免投影 Sprite 把碰撞探针退化为矩形。
- `PackCoverShadow.shader` 增加 SpriteRenderer 专用变体。运行时使用 FullRect Sprite 和按 PPU 计算的顶点留白，复用 02/03/04 的原始美术参数，避免直接使用 UI 材质时贴图缩小或投影被裁切。
- 新增菜单 `Puffies -> Apply CardBag Shadow Materials`；关卡生成器创建新 CardBag Prefab 时也会自动为凹槽应用 03。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag001.prefab` 至 `CardBag023.prefab`
- `Assets/Resources/PackCoverShadow.shader`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 四个 `IngameCoverShadow01-04.mat` 的美术参数保持不变；运行时只克隆材质并启用 SpriteRenderer Shader 变体。
- Piece 的碰撞体仍按原 Sprite 创建，投影用 FullRect 运行时 Sprite 只影响渲染，不改变凹槽、碰撞、吸附和持久化编号。
- 窗口失焦取消不视为玩家松手，保持 Piece 当前投影状态；真正执行松手判定时才切换 02 或 03。
- `BagShadow.prefab` 是用户当前独立修改，本轮不覆盖、不回滚。

## 验证

- Unity Editor 批量处理：23 个 Prefab 全部成功，失败 0。
- 序列化审计：23 个 Prefab 当前包含 46 个 01、840 个 03、0 个 04；Prefab 中的 04 错误绑定已全部清除。
- 配置与代码审计：截图对应 `CardBag021`，`CardPacks.csv/BoardScale=1.1`；创建和拿起路径均不再把 `DragScale` 固定为 `1`，而是使用凹槽屏幕矩形校准结果。托盘路径仍通过 `LimitPieceScaleToTray` 保证 `TrayScale<=1`。
- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- Unity Editor 日志未发现 C# 或 Shader 编译错误。
- 尚未进行 GameScene Play Mode 目视验证。

## 关联待办

- 继续验证托盘 `TrayScale <= 1`、拿起恢复 `DragScale`、桌面放置、回托盘、托盘滑动、失焦恢复和棋盘到托盘 `10%` 间距。
- 卡包编号迁移仍需补齐 `CardPacks.csv` 的 005/006/007/010/023，并确认 CardBag005 包头来源。

## 下一步

1. 在 `CardBag021` 从托盘拿起截图中的 Piece，确认 Scale 从托盘限制值恢复为包含配置 `1.1` 的凹槽实际显示尺寸，且材质为 04。
2. 将 Piece 放到桌面确认切换为 02，再次拿起确认恢复原始尺寸并切回 04；错误回弹和红色反馈仍应正常。
3. 正确吸附并重新进入有历史进度的卡包，确认棋盘 Piece 使用 03。
4. 检查 GameBoard、BoardTitle 使用 01 时投影没有被父级 Canvas 或棋盘边界裁切。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；在 GameScene Play Mode 验证托盘 Piece 拿起恢复 `DragScale` 和 04、桌面/错误松手切换 02、再次拿起切回 04，以及正确吸附和凹槽使用 03。不要覆盖用户对 `BagShadow.prefab` 的独立修改。
