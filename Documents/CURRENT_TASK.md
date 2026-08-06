# 当前任务

- 任务：增加卡包系列顺序获取规则
- 状态：实现完成
- 更新时间：2026-08-06

## 用户意图

- 在 `CardPacks.csv` 增加手工维护的字符串字段 `Series`，默认留空。
- 系列使用 `卡包Id|卡包Id` 表达顺序链；自动更新卡包尺寸时不得修改该列。
- 系列后续卡包只有在前置卡包完成后才能进入现有发包候选池，不能通过任务奖励或首次完成奖励提前获得。

## 工作记录

- `CardPacks.csv` 在 `BoardScale` 与最后一列 `AutoUpdate` 之间增加 `Series`，当前 22 行全部为空。
- 配置解析保留原始字符串，并生成运行时 PackId 序列。第 2 行填写 `15|18` 会建立 `2 -> 15 -> 18`。
- 系列会转换为直接前置关系，并验证引用 PackId 存在、默认首包没有前置、同一包没有冲突前置、链中没有重复或循环。
- 发包候选要求完整前置链全部为 `Completed`；任务必得奖励和首次完成奖励共用该过滤。
- `TryUnlockPack` 与 `TryUnlockPackFromTaskReward` 增加二次系列校验，防止未来新增调用入口绕过候选过滤。
- 自动更新工具缺少 `Series` 时会在 `AutoUpdate` 前补空列；列已存在时只原样写回，不读取或修改内容。

## 修改文件

- `Assets/Resources/Configs/CardPacks.csv`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- `Series` 只控制进入发包候选池的资格，不自动解锁后续卡包，也不绕过章节、持有数量和其他现有发包规则。
- 系列按配置行自身 PackId 开始。例如 PackId 2 的 `Series=15|18` 表示 `2 -> 15 -> 18`。
- 前置状态必须为 `Completed`；仅 `Unlocked` 或 `InProgress` 不满足条件。
- 不主动回收已经获得的卡包。配置系列前已经解锁的后续卡包继续保留。

## 验证

- `dotnet build Puffies.sln --no-restore`：成功，0 个警告、0 个错误。
- CSV 校验：22 行；字段顺序包含 `BoardScale|Series|AutoUpdate`；Series 非空行 0；`AutoUpdate` 非法行 0。
- 独立调用正式 `CardPackSeriesRules` 验证 `2 -> 15 -> 18`：初始 15/18 均不可获取；完成 2 后仅 15 可获取；完成 15 后 18 可获取。
- `git diff --check`：通过，仅有仓库既有的 LF/CRLF 提示。

## 下一步

1. 在 `CardPacks.csv/Series` 中填写正式链配置。
2. 若要用已有开发存档验证“后续包未提前获得”，测试前删除 `persistentDataPath/LocalData.db`；本次没有数据库结构变化，非测试场景无需删除。
3. 在 Unity 完成前置卡包并观察结算发包，确认后续包按顺序进入常规候选池。

## 恢复提示

卡包系列规则已实现。`CardPacks.csv/Series` 使用 `15|18` 这类字符串，并以当前行 PackId 为链首；自动更新工具保留该列，所有正常发包入口和直接解锁 API 都会检查完整前置链是否已完成。
