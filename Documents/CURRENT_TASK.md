# 当前任务

- 任务：托盘拼图间距与缩放规则调整
- 状态：代码与编译验证完成，等待 Play Mode 目视确认
- 更新时间：2026-08-20

## 用户意图

- 将游戏中托盘拼图碎片之间的固定 X 间距从 `20` 设计像素调整为 `40` 设计像素。
- 托盘 Piece 先按原始 Sprite 尺寸判断高度；超过托盘高度 `90%` 时等比缩小到 `90%`，否则保持原尺寸。

## 工作记录

- `GameScene.DraggableHorizontalSpacingPixels` 已从 `20f` 修改为 `40f`。
- 初始托盘布局、拿起后的后序 Piece 补位，以及 Piece 返回托盘后的重排继续共用同一个间距常量。
- 托盘比例改为从 `Vector3.one` 的原尺寸开始计算，不再继承棋盘 `BoardScale`，也不再被棋盘吸附目标比例限制。
- 原尺寸高度不超过托盘高度 `90%` 时使用 `Vector3.one`；超过时按高度比例统一缩放 X/Y/Z，保持宽高比。
- 托盘垂直居中和 `0.5s` 补位缓动未修改。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- `40` 表示相邻 Piece 实际可见渲染边缘之间的设计像素距离，不是中心点距离。
- 保持 `PixelsPerUnit=100`，运行时对应 `0.4` 世界单位。
- “原尺寸”定义为 Sprite 在 `localScale = (1,1,1)` 时的尺寸；棋盘吸附仍单独使用既有 `DragScale`。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅提示工作区行尾将在 Git 后续处理时转换为 CRLF。
- 静态确认拖拽 Piece 与其运行时父节点初始缩放均为 `Vector3.one`，原尺寸判断不受父节点缩放影响。
- 待在 Unity Play Mode 目视确认较矮 Piece 保持原尺寸、较高 Piece 缩放到托盘高度 `90%`，且固定间距和垂直居中正常。

## 下一步

1. 在同时具有较矮和较高碎片的卡包中检查初始布局、拿起补位和返回托盘重排。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；验证托盘 Piece 使用 `40` 设计像素固定水平间距，并且只在原尺寸高度超过托盘 `90%` 时等比缩小。
