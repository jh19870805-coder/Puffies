# 当前任务

- 任务：扩展 Piece 自由放置、防重叠与空托盘提醒
- 状态：代码与编译验证完成，等待 Unity Play Mode 交互确认
- 更新时间：2026-08-11

## 用户意图

- Piece 可以停留在桌面或棋盘空位；只有碰到自身凹槽但未达到吸附标准时才判定放错并回弹。
- 停留在桌面或错误棋盘位置的 Piece 不能互相叠加。
- 黑色托盘清空后自动收下，不因外部未正确放置 Piece 而重新显示。
- 托盘收起后仍有错误 Piece 时，每隔 `5s` 播放一次抖动提醒。

## 工作记录

- 移除“Piece 与整个棋盘相交就回弹”的旧判定，改为只检测 Piece 与自身凹槽的实际轮廓相交。
- 为每个运行时 Piece 创建 `Sprite.GetPhysicsShape` 对应的 `Collider2D`，同时创建不渲染的自身凹槽轮廓探针；无 Physics Shape 时回退 Sprite 本地边界 Box。
- 松手优先级改为：正确吸附且目标未被占用、仍有其他 Piece 的托盘回收、外部 Piece 重叠、自身凹槽错误相交、自由放置。
- 正确吸附目标已被外部错误 Piece 占用时拒绝吸附并回弹，避免正确 Piece 与错误 Piece 叠放。
- 移除拿起外部 Piece 时强制显示空托盘的逻辑；从托盘拿起最后一块后立即开始收起，托盘尚可见时允许快速放回，完全收起后外部 Piece 不会让它重显；来自托盘的错误回弹仍会恢复托盘。
- 新增空托盘提醒计时：托盘完全收起后等待 `5s`，让全部外部错误 Piece 短暂旋转抖动，此后每 `5s` 重复一次。
- 拖拽、提示抖动、错误回弹、切组、结算和对象销毁都会停止提醒并恢复 Piece 原旋转；再次满足条件后重新计时。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- 凹槽相交和 Piece 防重叠都使用 Sprite 自动 Physics Shape，不使用会误判不规则透明区域的矩形包围盒。
- 托盘 Piece 与已经正确吸附的棋盘内容不参与外部 Piece 防重叠判定。
- 空托盘不再作为回收目标；托盘仍有其他 Piece 时继续保留原来的 `50%` 垂直进入回收规则。
- 抖动提醒使用 `Time.unscaledTime`，不受 `TimeScale` 影响。
- 本次不修改场景、Prefab、资源或持久化结构，不需要清理本地数据。

## 验证

- `git diff --check` 通过。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 无界面完整导入和脚本编译成功退出；日志无 C# 编译错误。无界面许可证在线刷新失败属于会话环境信息，不影响本地编译结果。
- 抽查 CardBag001 Piece 导入配置为 `spriteMeshType=1`、`spriteGenerateFallbackPhysicsShape=1`、`alphaIsTransparency=1`，运行时可获得非矩形自动轮廓。
- 静态确认空托盘只有错误回弹分支会重新显示；外部自由放置和正确吸附不会重显托盘。
- 尚未在 Play Mode 实际拖放确认轮廓交点、防重叠边界和 `5s` 抖动视觉节奏。

## 下一步

1. 将 Piece 放到棋盘空位，确认不碰自身凹槽时可以停留，碰到自身凹槽但未达到吸附距离时回弹。
2. 将两个外部 Piece 尝试叠放，并用错误 Piece 占住另一个 Piece 的正确凹槽，确认两种情况都回弹。
3. 将托盘最后一块移到外部，确认托盘保持收起，并从收起完成约 `5s` 后观察全部错误 Piece 抖动。
4. 测试错误回弹到托盘、托盘仍有 Piece 时的外部 Piece 回收，以及正确吸附和切组流程。

## 恢复提示

自由放置、防重叠、空托盘和 `5s` 抖动提醒已经实现并通过编译。下一步进入 Unity Play Mode，按“下一步”四类分支做实际拖放确认。
