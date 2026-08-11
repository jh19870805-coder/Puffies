# 当前任务

- 任务：调整错误 Piece 回弹与红色反馈时序
- 状态：代码、规则和编译验证完成，等待 Unity Play Mode 视觉确认
- 更新时间：2026-08-11

## 用户意图

- 错误 Piece 松手后不要立即变红，应先开始回弹。
- Piece 进入黑色托盘区域后才变红。
- 红色提示比现有效果淡 `30%`。

## 工作记录

- 删除错误回弹开始前的红色赋值和 `0.08s` 停顿，保持现有 `0.3s` 三次方减速回弹。
- 回弹期间逐帧检测 Piece 渲染边界与可见黑色托盘边界，首次相交时才显示红色。
- 红色由 Piece 原色向现有错误红混合 `70%`，Alpha 保持原值，避免通过半透明削弱 Piece 本身。
- 来自外部位置且回弹目标不在托盘的 Piece 不显示托盘红色反馈。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- “进入黑色托盘区域”按 Piece 和托盘的二维世界渲染边界首次相交计算，不等待进入面积达到 `50%`。
- “红色变淡 30%”按现有错误红着色强度保留 `70%` 处理，不降低 Piece Alpha。
- 不修改错误判定、回弹目标、回弹时长、托盘重排或正确吸附逻辑。
- 本次不修改场景、Prefab、资源或持久化结构，不需要清理本地数据。

## 验证

- 静态确认松手后不再立即赋红，也不再执行 `InvalidDropHoldDuration` 停顿。
- 静态确认只有 `state.IsOnTray` 且 Piece 首次进入可见托盘边界时才设置错误红色。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- 尚未在 Unity Play Mode 目视确认回弹时序和红色强度。

## 下一步

1. 在 GameScene 从托盘拖出 Piece 并错误放置，确认先回弹、触碰托盘后才以较淡红色提示。
2. 从桌面错误位置再次拖动并触发回弹，确认返回桌面原位置时不显示托盘红色。

## 恢复提示

错误 Piece 已改为先回弹、进入可见黑色托盘后再以 `70%` 强度显示红色，两个 C# 项目已编译通过。下一步进入 Unity Play Mode 做视觉验证。
