# 当前任务

- 任务：恢复第一版托盘屏幕尺寸适配
- 状态：代码已恢复，等待 Play Mode 验收
- 更新时间：2026-08-22

## 用户意图

- 托盘上的拼图被点击拿起后，只允许保持尺寸或放大，不允许缩小。
- 保留第一轮基于实际屏幕尺寸的适配，只撤回第二轮按托盘稳定世界边界主动放大的实现。

## 工作记录

- 已恢复第一轮实现：通过 `PieceBoard` 和 Piece Renderer 的实际屏幕矩形，把 Sprite 原生设计高度与托盘 `90%` 容纳上限映射为 `TrayScale`。
- 已撤回第二轮实现：不再只依赖托盘稳定世界边界把 Piece 主动放大到对应设计高度。
- 拿起、回托盘、错误回弹和托盘重排恢复第一轮的 `SanitizeTrayPieceScale` 状态保存方式。
- 用户确认第一轮效果正确；继续验收点击拿起时只保持尺寸或放大、不缩小。

## 修改文件

- `Documents/CURRENT_TASK.md`

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告，`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告，`0` 错误。

## 下一步

1. 重新进入卡包003和卡包018，确认第一轮托盘尺寸效果已恢复。
2. 点击托盘 Piece，确认只保持尺寸或放大，不出现缩小。

## 恢复提示

继续 Puffies 的“第一版托盘屏幕尺寸适配”任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；保留第一轮屏幕矩形换算，不得恢复第二轮世界边界强制放大方案，并确认拿起时只能不变或放大。未经用户明确要求不要自动提交；用户要求提交时同时推送。
