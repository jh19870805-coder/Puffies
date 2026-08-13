# 当前任务

- 任务：修复托盘 Piece 首次初始化未上下居中
- 状态：代码和编译验证完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-13

## 用户意图

- 进入 GameScene 后，托盘上首次初始化的 Piece 应立即上下居中。
- 不应依赖玩家先点击一块 Piece 才自动调整尺寸和位置。
- 保持固定水平间距、拿起后的 `0.5s` 补位和现有入场动画不变。

## 工作记录

- 第一版将问题误判为 SpriteRenderer Bounds 首帧未稳定；替换为 Sprite 网格 Bounds 后实测没有改善，已撤销该无效改动。
- 最新 Unity 日志确认复测关卡为 `CardBag013`，并已加载修改后的代码，不是热重载遗漏。
- 核对 CardBag013 第一组 6 张源 PNG，Alpha 均覆盖完整矩形，不存在导致统一垂直偏移的不对称透明留白。
- 确认真正不稳定的是 PieceBoard 托盘中心：关卡适配会在初始建组时改变正交相机尺寸，而 Screen Space - Camera Canvas 的世界角点要到渲染阶段才同步；首帧布局读到旧托盘中心，点击后的重排才读到新中心。
- PieceBoard Bounds 改为从根 Canvas 设计坐标直接映射到当前屏幕与游戏相机世界坐标，绕开首帧 Canvas 世界角点更新时序。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 修复所有卡包共用的运行时布局，不对 CardBag013 或 Prefab 写特殊偏移。
- 不增加等待一帧后二次跳位；首次创建即从 Canvas 设计坐标计算最终托盘中心。
- 设计坐标换算失败时保留原 Canvas 世界角点路径作为回退。

## 验证

- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `git diff --check` 通过。
- 静态检查确认只替换 PieceBoard Bounds 的首帧坐标来源；Piece 尺寸、拖放碰撞、回收、吸附与自由放置仍使用现有 Renderer Bounds 和 Collider 逻辑。
- Unity Editor.log 当前没有新的 C# 编译错误；编辑器仍处于运行状态，需退出 Play Mode 后重新进入以确保加载最新程序集。
- 待直接从编辑器进入截图关卡，确认 Piece 首帧上下居中且点击后不再发生尺寸/位置修正。

## 下一步

1. 退出当前 Play Mode，再由 Unity 重新进入 GameScene 测试截图关卡的首组初始化。
2. 确认 Piece 首帧上下居中，点击并放回后尺寸和 Y 位置不再二次修正。
3. 从正常拆包流程进入同一关，确认 Piece 入场目标位置仍正确。

## 恢复提示

上一版 Sprite 网格 Bounds 修复已撤销。当前改为从 Canvas 设计坐标直接计算 PieceBoard 最终世界中心，避免相机适配后的首帧 Canvas 世界角点滞后。下一步重新编译并进入 CardBag013，验证首帧居中、点击无跳变及正常入场动画。
