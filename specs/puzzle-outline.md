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
10. 当资源缺少 `background_base.png`、对应预览图或 Piece PNG 时，工具应显示缺失原因并禁止选择该卡包生成。
11. 批量窗口默认只选择尚无 Prefab 的资源；当选择已有 Prefab 时，执行前必须明确提示将覆盖已有层级和手工分组。
12. 当一批资源中的某个卡包生成失败时，工具应继续处理其他已选卡包，并在结束后汇总成功与失败结果。
13. 批量生成只创建 Prefab，不自动烘焙描边；使用者完成 Piece 分组后，再通过独立菜单统一烘焙。

## 设计

1. 在不改变导入设置的情况下，读取每个 Prefab 的 GameBoard 和 Piece 源贴图。
2. 将 Piece Alpha 蒙版转换到 GameBoard 像素坐标，生成完整拼图蒙版和每个数字分组的独立蒙版。
3. 完整拼图外边界只计算一次，同时分别计算当前组自身边界。
4. 第 1 组只选择归属于第 1 组的最终外边界像素。
5. 后续每组增加当前组边界中与累计低编号已完成蒙版相邻的像素。`ColorBridgeRadius` 用于跨越美术资源中较窄的抗锯齿间隙。
6. 在推进已完成蒙版前写出当前图片，避免未来组边界泄漏到较早阶段。
7. 将全棋盘尺寸的透明 Sprite 写入 `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`，运行时只显示当前组对应的 Sprite。
8. `CardBag Prefab Generator` 编辑器窗口扫描一级 `CardBagNNN` 目录，提供刷新、选择新卡包、选择全部有效卡包、清空选择和批量生成操作。
9. 每个扫描项显示 Piece 数量、Prefab 是否已存在以及校验状态；默认勾选资源完整且尚无 Prefab 的卡包。
10. 批量执行逐个捕获异常，成功项正常保存，失败项写入 Console 并在结果对话框中汇总。
11. `Piece001` 这类三位顺序名称明确表示尚未分组，描边烘焙器不得将 `Piece010` 到 `Piece099` 误判成正式游戏分组；卡包没有正式分组时应删除该卡包的旧描边输出。

## 内容制作流程

- 新增或修改 CardBag Prefab 或 Piece 贴图后，执行 **Puffies -> Puzzles -> Bake Outline Masks**。
- 执行 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images** 打开批量窗口；窗口会发现所有符合命名规则且资源完整的 `CardBagNNN` 目录。
- 生成器要求 `background_base.png` 与 `Previews/CardBagNNN.png` 尺寸一致。效果图只用于校验，不进入运行时 Prefab 引用。
- 使用 `PieceNN.png` 或 `PiecesNN.png` 时，文件名直接定义游戏分组；使用 `piece_###.png` 时生成器只创建 `Piece001` 开始的顺序节点，不推断分组，也不烘焙该卡包描边。
- 默认使用 **Select New** 只选择尚未生成 Prefab 的卡包。使用 **Select All Ready** 并覆盖已有 Prefab 会丢失 Prefab 内手工修改的分组，执行前必须确认。
- 批量 Prefab 生成完成后，先手工完成顺序节点分组，再执行 **Puffies -> Puzzles -> Bake Outline Masks**。
- 当前有效生成内容包括 CardBag001、002、003、008 和 009 的19张分组蒙版。CardBag017 等待手工分组后重新加入。

## 验证

- Unity 2022.3.62f2 成功烘焙全部24张蒙版，没有编译错误或异常。
- CardBag017 的37张新碎图均与完整背景达到 `100.00%` 像素匹配；旧分组蒙版已在切换为顺序命名后删除。
- 重新生成并受版本控制的 Group01 PNG 与此前正确版本的字节完全一致。
- CardBag001 Group01 包含 14,674 个描边像素；修正后的 Group02 包含 9,372 个，旧叠加版本为 24,018 个。
- 静态代码检查确认成功放置碎片后不再调用托盘布局路径。
- 仍需在 Play Mode 中验证固定 Piece 坐标、分组切换，以及描边与已完成组和未来组的关系。
