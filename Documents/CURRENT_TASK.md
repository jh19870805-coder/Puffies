# 当前任务

- 任务：修正黑色托盘 Piece 居中与补位动画
- 状态：代码与编译验证完成，等待 Unity Play Mode 视觉确认
- 更新时间：2026-08-11

## 用户意图

- 当前分组 Piece 初始显示时应在黑色托盘内上下居中。
- 块与块之间使用适用于所有卡包的固定间距。
- 拿起队尾 Piece 时不刷新其他 Piece 位置。
- 需要补位时，Piece 使用 `0.5s` 缓动滑到新位置。

## 工作记录

- 初始布局改为读取 `SpriteRenderer.bounds` 的实际渲染中心，将每块 Piece 上下居中到托盘中心。
- 保留并明确所有卡包共用 `20` 设计像素水平间距。
- 补位触发点从松手后前移到从托盘拿起时，只处理编号靠后的托盘 Piece。
- 新增单一托盘重排协程，使用不受 `TimeScale` 影响的 `0.5s SmoothStep` 缓动。
- 队尾 Piece 没有后序目标时不启动协程；桌面 Piece、前序 Piece、Y 和缩放不参与补位。
- 错误返回或放回托盘时取消旧补位，并从当前位置按固定间距缓动恢复。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- 固定间距沿用现有 `20` 设计像素，不新增卡包级配置。
- 补位期间暂时阻止开始下一次拖拽，但不影响当前已经拿起的 Piece 继续移动和松手。
- 初始建组即时布局，不播放补位动画；只有交互导致的补位或回收重排播放 `0.5s` 动画。
- 本次不修改场景、Prefab、资源或持久化结构，不需要清理本地数据。

## 验证

- `git diff --check` 通过。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 无界面编译和完整资源导入通过，返回码为 `0`，日志中无 C# 错误或警告。
- Unity 未修改场景、Prefab 或资源。
- 静态确认队尾目标为空时不启动补位；非队尾只移动编号靠后的托盘 Piece。
- 尚未在 Play Mode 目视确认初始垂直居中和 `0.5s` 缓动节奏。

## 下一步

1. 在 GameScene 分别测试小组和 CardBag022 的 14 片分组，确认初始上下居中及固定间距。
2. 依次拿起队首、中间和队尾 Piece，确认补位范围和 `0.5s` 节奏。
3. 测试错误放到棋盘和桌面 Piece 回收托盘，确认间距平滑恢复。

## 恢复提示

托盘居中与补位代码已经完成并通过编译。下一步进入 Unity Play Mode，验证初始布局、队首/中间/队尾以及回收分支的视觉表现。
