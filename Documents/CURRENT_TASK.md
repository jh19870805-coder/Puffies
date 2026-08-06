# 当前任务

- 任务：修复 CardBag 图片匹配粗定位漏检
- 状态：实现完成，等待 Unity Editor 批量生成验收
- 更新时间：2026-08-06

## 用户意图

- 优化自动制作关卡的 Prefab 生成器。
- 解决同批新导入的 Preview、GameBoard 和 Piece 内容实际对应，但个别 Piece 被错误判定为无法定位的问题。
- 保持批量生成速度与匹配可靠性，不能通过降低置信度阈值掩盖错误资源。

## 工作记录

- 复现 `CardBag003/piece_017.png` 报错，确认资源属于同一批且原尺寸、原方向能够在 Preview 中对应。
- Unity 原算法使用固定 `6px` 粗网格和 12 个高辨识度采样点；真实位置不在网格点上时，细线图案偏移 1 至 2 像素即可让正确区域在保留 48 个候选前被淘汰。
- 将透明像素轮次和不透明像素轮次中唯一精确 RGB 锚点得到的位置作为感知匹配候选种子。
- `CardBag004/piece_010.png` 包含多根相似骨头，唯一 RGB 锚点会因偶然同色指向错误位置；现改为锚点位置本身达到 `78%` 感知匹配率后才允许作为候选种子。
- 首轮感知匹配不通过或候选不唯一时，增加 `1px` 逐像素网格回退；正常卡包仍使用原 `6px` 快速路径。
- 保留感知匹配至少 `78%`、最佳候选领先其他位置至少 `1.5%` 的既有安全校验。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不降低精确匹配或感知匹配阈值。
- 唯一精确锚点只负责提供候选坐标，最终仍由完整感知采样和候选唯一性决定是否接受。
- 唯一精确锚点若未先达到最低感知匹配率，不能进入候选集。
- 逐像素扫描只作为失败回退，避免所有 Piece 都承担更高的扫描耗时。

## 验证

- `dotnet build Puffies.sln --no-restore`：成功，0 个警告、0 个错误。
- 使用与 Unity 相同的底部坐标、采样顺序和候选规则回放 `CardBag003/piece_017.png`：定位为 `(890,521)`，感知匹配 `93.50%`，无远端竞争候选，满足接受条件。
- 修改前算法、带种子的 `6px` 算法和 `3px` 回退均会漏掉 `CardBag004/piece_010.png`；确认不是前一轮修改造成的回归。
- 对 `CardBag004/piece_010.png` 执行最终 `1px` 回退：唯一定位为 `(595,680)`，感知匹配 `93.70%`，无远端竞争候选，满足接受条件。
- `git diff --check`：代码修改通过；工作区已有 `Assets/UI/CardBags/CardBag003/GameBoard.png.meta` 修改未触碰。
- 尚未在当前已打开的 Unity Editor 中执行完整 CardBag003 Prefab 覆盖生成。

## 下一步

1. 等待 Unity 完成脚本重编译，在 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images** 中重新生成 CardBag004。
2. 确认 `piece_010.png` 日志显示 Preview 感知匹配约 `93%`，并继续完成其余 Piece。
3. 再选择 CardBag003 与多个已有卡包做批量覆盖测试，确认快速路径结果和生成耗时没有回归。

## 恢复提示

CardBag 生成器会先验证唯一精确锚点的感知分数，并在常规扫描失败时使用 `1px` 逐像素回退。命令行编译及 CardBag003/piece_017、CardBag004/piece_010 算法回放均通过；下一步在 Unity Editor 重新生成 CardBag004，并检查完整批次日志。
