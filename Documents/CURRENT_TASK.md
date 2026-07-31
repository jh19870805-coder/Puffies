# 当前任务

- 任务：重做随机任务系统
- 状态：代码与配置实现完成，等待 Unity Play Mode 验证
- 更新时间：2026-07-31

## 用户意图

- 支持累计分数、收集贴纸、完成卡包三种任务类型。
- 每种任务支持任意尺寸和随机指定尺寸。
- 任务从配置表随机生成；积分目标不能纯随机，使用循环顺序。

## 工作记录

- `TaskConfig.csv` 改为 6 行任务模板池，配置启用状态、类型、尺寸模式、尺寸池、目标池、权重、章节范围、重玩规则和奖励。
- 当前任务改为持久化的独立实例，记录 `TaskInstanceId`、模板、实际尺寸、实际目标和当前进度。
- 模板按权重随机且避免连续使用同一模板；指定尺寸只从当前可玩尺寸中选择。
- 积分目标按 `200 -> 400 -> 600 -> 800 -> 1000 -> 1200` 循环；贴纸目标从 `60|80|100` 随机；完成卡包目标从 `1|2|3` 随机。
- GameScene 完整结算时按任务类型累计最终得分、卡包全部 Piece 数量或 1 个完成卡包；尺寸不匹配时进度不变，重玩按配置计入。
- 首页和 RewardPanel 的共享 `TaskItem` 已支持三类动态文案和进度动画。
- 待发任务奖励改为按唯一 `TaskInstanceId` 去重。

## 修改文件

- `Assets/Resources/Configs/TaskConfig.csv`
- `Assets/Scripts/Model/GameTaskUtility.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/View/TaskProgressUIUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`

## 决策

- 当前仍只同时维护一个任务，完成后随机生成下一任务。
- 三类任务都在完整完成卡包时原子结算；贴纸任务不会在中途放置单片时提前累计。
- `CountReplay=1`，保持已确认的重玩可推进任务规则。
- 积分溢出只在下一任务仍是积分任务且本局卡包符合新任务条件时结转。
- 本次任务 JSON 和待发奖励 SQLite 结构均不兼容，不增加旧数据迁移。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- 已静态确认旧顺序 TaskId API 和旧 `TaskConfigData` 引用均已清除。
- 尚未在 Unity Play Mode 验证首次随机任务、指定尺寸筛选、六级积分循环和三类结算表现。

## 下一步

1. 关闭 Unity，删除 `LocalData.json` 和 `LocalData.db` 后重新进入项目。
2. 验证首页随机任务文案和尺寸要求；连续完成任务，确认模板不连续重复且积分目标按固定顺序循环。
3. 分别验证分数、贴纸、完成卡包三类任务，以及尺寸匹配、不匹配和重玩场景。
4. 根据试玩结果调整模板权重与类型3的 `1|2|3` 目标池。

## 恢复提示

继续 Puffies 随机任务系统。配置、解析、任务实例持久化、三类结算和 UI 已完成；测试前删除 `LocalData.json` 与 `LocalData.db`，再在 Unity Play Mode 验证随机与循环规则。
