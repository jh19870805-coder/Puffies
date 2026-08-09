# 当前任务

- 任务：同步 CardBag022 空间分组规则到自动生成工具
- 状态：Prefab 分组、生成器逻辑、编译和 CardBag022 描边完成，等待 Unity 工具与 Play Mode 回归
- 更新时间：2026-08-09

## 用户意图

- 参考现有 `CardBagXXX.prefab` 的分组方式，将同一局部区域的贴纸划入一组。
- 先在 `CardBag022.prefab` 上尝试一版大概分组。
- 将 CardBag022 最终确认的排序和命名规则同步到 CardBag 自动生成工具。
- 不重建 Prefab，不修改贴图、位置、尺寸、Image 参数、层级或影子。

## 工作记录

- 解析 `CardBag022` 的 196 个 Piece 中心位置和原生尺寸，并与完整预览图核对空间分布。
- 初版按从上到下的 7 个水平区域、每个区域从左到右 4 组分为 28 组；根据体验反馈将相邻的 `01+02`、`03+04` 等两两合并，当前共 14 组、每组 14 片。
- 组号按棋盘从上到下、同一行从左到右排列；组内索引按中心位置从左到右稳定排列。
- 仅将三位中间名 `Piece001..Piece196` 替换为正式四位名 `PieceGGII`，没有调整任何序列化布局或渲染属性。
- 整理 `GameBoard` 子节点的同级顺序：保留 `BoardTitle` 第一位，其后严格按 `Piece0101..Piece1414` 升序排列，方便 Unity Hierarchy 查看并确保直接遍历顺序稳定。
- 按用户指定调整阶段顺序：完整交换分组 `03 ↔ 04`、`07 ↔ 08`、`11 ↔ 12` 的名称；每片组内索引保持不变，并在交换后重新按新名称整理同级顺序。
- 当前打开的 Unity Editor 在导入正式分组后将 196 个 Piece 占位 Image 的 Alpha 同步为 `0`；该配置与 CardBag020/021 的隐藏棋盘占位 Piece 一致，因此保留，不回退为初始未分组状态的 Alpha `1`。
- 自动生成器现在对标准 `piece_###.png` 按位置从上到下分带，每行最多两个组、每组最多 14 片，并按左右交替的蛇形顺序生成正式 `PieceGGII`；组内按中心点从左到右编号，Hierarchy 按正式名升序创建。
- 全部显式 `PieceGGII.png` 继续保留人工分组；显式名与标准名混用时生成失败并给出明确错误。“Update Existing Piece Layouts”仍只更新位置和尺寸，不重新命名或分组。
- 批量生成不自动执行耗时描边烘焙，但会删除该包旧描边并提示执行 `Bake Outline Masks`，避免新分组加载旧蒙版。
- 尝试以 Unity 批处理执行全量描边烘焙；由于项目已被当前打开的 Unity Editor 占用，批处理实例等待项目锁，已只关闭该等待实例。现有编辑器已成功导入更新后的 Prefab。
- 当前打开的 Unity Editor 随后完成全量描边烘焙；CardBag022 已生成 14 组对应的默认、关卡和贴纸描边资源，共 42 张 PNG。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag022.prefab`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag022/`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 验证

- `CardBag022` 共 196 个合法四位正式 Piece 名，三位中间名 0，重复名 0。
- 分组为 `Group01..Group14`，每组均为 14 片，索引完整为 `01..14`。
- `GameBoard` 共 197 个子节点，顺序精确等于 `BoardTitle` 加 `Piece0101..Piece1414`，顺序差异 0。
- 指定的三对分组交换后仍为 196 个唯一正式名，14 组均保持 14 片且索引完整为 `01..14`。
- CardBag022 当前 Piece 占位 Image 共 196 个 Alpha `0`；CardBag020/021 的 Piece 占位同样采用 Alpha `0`，配置方向一致。
- `git diff --unified=0` 检查确认手工 Prefab 操作仅修改 Piece 的 `m_Name` 与 `GameBoard.m_Children` 排序；Unity Editor 另将正式棋盘占位 Piece 的 Alpha 同步为 `0`。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- 使用 CardBag022 的 196 个实际中心位置回放自动分组算法，得到 14 组、每组 14 片，自动名称与当前人工确认结果完全一致，差异 0。
- CardBag022 描边目录包含 42 张 PNG 和 42 个对应 Meta；`Group01..Group14` 的默认、`_Level`、`_Stickers` 文件全部存在。
- Unity 日志显示 `Puzzle outline baker: baked 114 group mask(s) from 22 card bag(s).`，未发现脚本编译错误；尚未进行 Play Mode 视觉和流程验证。

## 下一步

1. 用后续新卡包执行一次 `Generate Selected`，确认标准切图直接生成正式分组且 Hierarchy 顺序正确。
2. 进入 CardBag022，验证蛇形组序、每组 14 片、提示描边和一键完成。
3. 若实际体验中某一自动空间组边界不理想，调整通用分带参数或使用全量显式 `PieceGGII.png` 覆盖该包分组。

## 数据说明

- SQLite 表结构未变化。
- `CardBag022` 现在使用正式 Piece 编号；如果本地已经产生过该包的三位中间编号进度，测试前删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`。
