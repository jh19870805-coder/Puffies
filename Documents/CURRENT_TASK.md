# 当前任务

- 任务：GameScene 一键完成测试按钮
- 状态：等待运行时确认
- 更新时间：2026-07-30

## 用户意图

- 在游戏界面增加一个方便测试的“一键完成”按钮。
- 点击后直接显示全部拼图并进入正式结算，自动执行完整的卡包、分数、任务和发包数据计算。

## 工作记录

- `GameScene` 在 Unity Editor 和 Development Build 中运行时创建 `BtnCompleteAllTest`，位置在 `BtnTips` 左侧；正式非 Development Build 不包含该测试入口。
- 点击按钮后收集当前 CardBag 全部分组的合法 Piece 编号，并一次性写入当前拼图会话。
- 快捷完成会清理提示和进行中的新手引导，显示全部棋盘 Piece，然后复用现有 `ShowRewardPanel()` 正式结算链。
- 卡包生命周期、完成时间、分数加成、任务进度、完成卡包数量、任务奖励和首次完成概率发包均沿用正式逻辑。
- `CardPackDataUtility.TryRecordPlacedPiece` 改为复用新的批量保存接口，原单片拖拽保存行为不变。
- 本次没有数据表或 JSON 结构变化，不需要删除本地数据；但点击测试按钮会产生真实的本地完成、任务和发包记录。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 测试按钮仅在 `UNITY_EDITOR` 或 `DEVELOPMENT_BUILD` 条件下编译显示，避免进入正式发布版本。
- 快捷完成不维护第二套结算实现，只负责补齐全部 Piece 状态并调用正式完成流程。
- 如果玩家已经手工拼入部分 Piece，已有提示使用状态、当前计时和设置快照继续参与结算；尚未放置过 Piece 时，快捷完成按即时完成计算。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过。
- 当前 Unity Editor 日志未出现新的 `error CS`、`NullReferenceException` 或 `MissingReferenceException`。
- 尚未在当前已打开的 Unity 中实际点击按钮完成 Play Mode 全流程。

## 下一步

1. 重新进入 `GameScene`，确认右上角 `BtnTips` 左侧显示“一键完成”。
2. 分别在零进度和已有部分 Piece 进度时点击，确认棋盘完整显示、RewardPanel 正常结算且完成按钮最终可用。
3. 返回 MainScene，确认卡包完成状态、任务进度和可能发放的新卡包已刷新。

## 恢复提示

继续 Puffies 当前任务。先阅读 AGENTS.md、Documents/WORKFLOW.md 和 Documents/CURRENT_TASK.md；GameScene 一键完成测试按钮已实现，下一步是在当前 Unity 中验证零进度和部分进度两条结算路径。
