# 分阶段拼图描边与碎片托盘布局

- 状态：已实现，待完成 Play Mode 回归
- 范围：CardBag Prefab 图片生成、编辑器烘焙的分组描边，以及运行时碎片托盘位置稳定性

## 需求

1. 创建一组碎片时只执行一次布局。成功放置一个 Piece 后，不得改变其他 Piece 的 X 或 Y 位置。
2. 第 1 组只显示该组对最终拼图外边界的贡献。
3. 后续每组显示自身的最终拼图外边界，以及与所有已完成低编号组的接触边。
4. 当前描边图必须排除已完成组的无关边界、当前组与未来组之间的边，以及同组 Piece 之间的接缝。
5. 不得将之前的 `GroupNN.png` 叠加到当前阶段。
6. 现有 `CardBagNNN` Prefab 继续作为 Piece 布局和数字分组的信息源。
7. 描边颜色为 `#3f423e`；烘焙不得修改场景、Canvas 尺寸和 Prefab 中已设置的 Transform。
8. 缺少烘焙 Sprite 时记录警告，但不得阻止可拖拽 Piece 创建。
9. 当 `Assets/UI/CardBags/` 下加入符合 `CardBagNNN` 命名的新资源目录时，编辑器工具应自动发现该卡包，不再为单个 PackId 增加专用菜单。
10. 每个卡包以 `GameBoard.png` 作为棋盘完整成图。当扫描到旧 `background_base.png` 且不存在 `GameBoard.png` 时，工具应先自动改名为 `GameBoard.png` 并保留 Unity Meta/GUID。
11. 当资源缺少 `GameBoard.png`、对应预览图或 Piece PNG 时，工具应显示缺失原因并禁止选择该卡包生成。
12. `BoardTitle.png` 是标准资源；缺失时工具应显示非阻断警告，并继续允许生成不含标题节点的关卡。
13. 批量窗口默认只选择尚无 Prefab 的资源；当选择已有 Prefab 时，执行前必须明确提示将覆盖已有层级和手工分组。
14. 当一批资源中的某个卡包生成失败时，工具应继续处理其他已选卡包，并在结束后汇总成功与失败结果。
15. 批量生成只创建 Prefab，不自动烘焙描边；使用者完成 Piece 分组后，再通过独立菜单统一烘焙。

## 设计

1. 在不改变导入设置的情况下，读取每个 Prefab 的 GameBoard 和 Piece 源贴图。
2. 将 Piece Alpha 蒙版转换到 GameBoard 像素坐标，生成每个数字分组的独立蒙版和 Piece 并集。
3. 最终拼图外边界优先来自 `GameBoard.png` 的透明挖空 Alpha，因此与运行时可见的浅色缝隙使用同一视觉来源；GameBoard 挖空无效或与 Piece 区域不重合时，回退到 Piece Alpha 并集。
4. 第 1 组只选择靠近该组 Piece 的最终外边界像素。`FinalBoundaryAssignmentRadius` 只扩大边界归属判断范围，不移动描边位置。
5. 后续每组从累计低编号已完成 Piece 的真实 Alpha 外边界中选择靠近当前组的接触边，不再把线画在当前组另一侧的 Alpha 边界上。`ContactSearchRadius` 只负责识别相邻关系。
6. 在推进已完成蒙版前写出当前图片，避免未来组边界泄漏到较早阶段。
7. 将全棋盘尺寸的透明 Sprite 写入 `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`，运行时只显示当前组对应的 Sprite。
8. `CardBag Prefab Generator` 编辑器窗口扫描一级 `CardBagNNN` 目录，提供刷新、选择新卡包、选择全部有效卡包、清空选择和批量生成操作。
9. 每个扫描项显示 Piece 数量、Prefab 是否已存在以及校验状态；默认勾选资源完整且尚无 Prefab 的卡包。
10. 批量执行逐个捕获异常，成功项正常保存，失败项写入 Console 并在结果对话框中汇总。
11. `Piece001` 这类三位顺序名称明确表示尚未分组。只要 Prefab 中仍有任一此类占位名，整包都不得烘焙，避免超过99片时把 `Piece100` 等后续顺序节点误判成正式游戏分组；卡包没有正式分组时应删除该卡包的旧描边输出。
12. 旧棋盘资源迁移使用 `AssetDatabase.MoveAsset`，使 `.meta` 随资源移动并保持现有 Prefab Sprite 引用有效；目标 `GameBoard.png` 已存在时不得覆盖或移动旧文件。
13. 扫描结果区分阻断生成的 Missing 项和不阻断生成的 Warning 项；缺少 `BoardTitle.png` 只进入 Warning。

## 内容制作流程

- 新增或修改 CardBag Prefab 或 Piece 贴图后，执行 **Puffies -> Bake Outline Masks**。
- 执行 **Puffies -> Generate CardBag Prefabs From Images** 打开批量窗口；窗口会发现所有符合命名规则且资源完整的 `CardBagNNN` 目录。
- 生成器要求 `GameBoard.png` 与 `Previews/CardBagNNN.png` 尺寸一致。效果图只用于校验，不进入运行时 Prefab 引用。
- 兼容旧资源：扫描到 `background_base.png` 且同目录没有 `GameBoard.png` 时自动改名；两者同时存在时以 `GameBoard.png` 为准，不覆盖已有文件。
- `BoardTitle.png` 应随卡包提供；缺失时窗口显示警告，但仍允许生成不含 `BoardTitle` 节点的 Prefab。
- 使用 `PieceNN.png` 或 `PiecesNN.png` 时，文件名直接定义游戏分组；使用 `piece_###.png` 时生成器只创建 `Piece001` 开始的顺序节点，不推断分组，也不烘焙该卡包描边。
- 默认使用 **Select New** 只选择尚未生成 Prefab 的卡包。使用 **Select All Ready** 并覆盖已有 Prefab 会丢失 Prefab 内手工修改的分组，执行前必须确认。
- 批量 Prefab 生成完成后，先手工完成顺序节点分组，再执行 **Puffies -> Bake Outline Masks**。
- 当前有效生成内容包括 CardBag001、002、003、008、009 和 017 的 24 张分组蒙版。CardBag022 等待手工分组后加入。

## 验证

- Unity 2022.3.62f2 在隔离临时工程中成功烘焙全部 24 张蒙版，没有编译错误或异常。
- CardBag002、003、008、009 和 017 的 GameBoard 透明挖空与 Piece 区域重合率为 `99.7%..100.0%`；CardBag001 不满足条件并正确回退到 Piece Alpha 并集。
- `GameBoard.png` 迁移、`BoardTitle.png` 软警告和超过99片的整包未分组保护通过 C# 编译；CardBag017 与 CardBag022 当前都满足旧棋盘文件自动迁移条件，CardBag022 的标题缺失不会阻断选择。
- CardBag003 Group04 包含 8,460 个描边像素；诊断合成确认右侧和底部最终外边界贴合 GameBoard 透明缝隙，左侧接触边来自已完成 Piece Alpha。
- 静态代码检查确认成功放置碎片后不再调用托盘布局路径。
- 仍需在 Play Mode 中验证固定 Piece 坐标、分组切换，以及描边与已完成组和未来组的关系。
