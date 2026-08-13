# 当前任务

- 任务：放大拆包特效的星星粒子
- 状态：实现完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-13

## 用户意图

- 拆包特效的光带保持当前效果。
- 星星粒子大小等比放大 3 倍，其他表现不变。

## 工作记录

- 确认拆包滑光 Prefab 中星星相关节点为 `dot`、`dot01`、`glow01`。
- 光效实例化后仅将上述三个 Particle System 的 Start Size 统一乘以 `3`。
- 保持 `line`、`line01`、`glow`、`glowC` 当前的光带长度、厚度和位置缩放不变。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/CURRENT_TASK.md`

## 决策

- “粒子大小放大 3 倍”按 Particle System 的 Start Size 处理；兼容统一尺寸与 3D 分轴尺寸，不改变节点缩放、发射形状、位置和运动参数。
- 不直接改第三方 Prefab 的粒子参数，继续沿用现有的运行时适配方式，避免影响原始特效资源。

## 验证

- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `git diff --check` 通过。
- 待在 MainScene 实际播放一次拆包动画，确认星星粒子为原来的 3 倍且光带未变化。

## 下一步

1. 在 MainScene 播放一次拆包动画，视觉确认星星粒子大小和光带效果。
2. 继续冷启动验证首次拆包进入 GameScene 的预加载优化。

## 恢复提示

当前拆包光带保持 `7x1` 适配，星星节点 `dot`、`dot01`、`glow01` 的粒子 Start Size 已统一放大 3 倍。下一步在 MainScene 播放拆包动画做视觉确认，同时继续验证首次进入 GameScene 的预加载效果。
