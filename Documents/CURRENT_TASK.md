# 当前任务

- 任务：结算页显示已完成卡包数量
- 状态：已完成
- 更新时间：2026-07-23

## 用户意图

- GameScene 结算页面的 `TaskBagNum` 显示已完成卡包数量。
- 未完成的已解锁卡包和进行中卡包不计入。

## 已完成

- 在 `CardPackDataUtility` 增加 `GetCompletedPackCount()`，直接统计 SQLite 中生命周期为 `Completed` 的卡包记录。
- 将 GameScene 的 `TaskBagNum` 从已解锁卡包数量改为已完成卡包数量。
- 保持结算顺序不变：当前卡包完成状态保存后刷新数量，因此首次完成立即计入；重玩不会重复计数。

## 修改文件

- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 完成数量以 SQLite `CardPacks.LifecycleState = Completed` 为唯一口径。
- 不根据解锁时间、完成时间或当前结算奖励数量推算。

## 验证

- `dotnet build Puffies.sln --no-restore`：0 警告、0 错误。
- `git diff --check`：通过，仅显示仓库现有的 LF/CRLF 转换提示。
- 搜索确认 `TaskBagNum` 已调用 `GetCompletedPackCount()`，不再调用已解锁卡包数量接口。
- 不需要重置 JSON 或 SQLite 本地数据。

## 下一步

1. 在 Unity Play Mode 完成一个新卡包，确认结算页数字增加 1；重玩同一卡包，确认数字不再增加。

## 恢复提示

继续 Puffies 开发。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；GameScene 结算页 `TaskBagNum` 已改为显示 SQLite 中已完成卡包数量，下一步是在 Play Mode 验证首次完成和重玩两种情况。
