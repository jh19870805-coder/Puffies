# 当前任务

- 任务：拼图贴纸不规则亮光与落位传播微动画
- 状态：代码、资源导入和编译验证已完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-13

## 用户意图

- 使用 `Assets/UI/GameScene/PieceLight1.png` 到 `PieceLight4.png` 复现参考视频中的不规则贴纸亮光。
- 已拼棋盘贴纸和托盘待拼贴纸平时都有低强度、错峰变化的微光。
- 正确落位后，当前块先增强亮光，再向附近已经拼好的相邻块短暂传播。

## 工作记录

- 使用 FFmpeg 将 `85562245f2decc4cc7e116bd1d06798f.mp4` 按 `10fps` 与原分辨率关键帧拆解。
- 确认视频约 `6.17s / 30fps`：落位前贴纸已有微弱不规则亮光；落位后当前块出现短亮度峰值，并向相邻已拼块错峰传播，整体不到 `1s`。
- 常驻层为每块可见贴纸确定性创建两个不规则亮斑，低强度、慢速错峰呼吸并轻微移动。
- 落位层当前块创建四个亮斑，相邻已拼块各创建两个，按距离增加约 `0.07~0.23s` 延迟，完整传播约 `0.72s`。
- 棋盘 UGUI 贴纸使用自身 Sprite Alpha Mask；托盘 SpriteRenderer 使用 SpriteMask，亮光不会溢出透明轮廓。
- 移除旧的规则绿色斜向光带及 `PuzzlePlacementShine.shader`。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Resources/PuzzlePieceLightAdditive.shader`
- `Assets/UI/GameScene/PieceLight1.png.meta` 到 `PieceLight4.png.meta`
- 删除 `Assets/Resources/PuzzlePlacementShine.shader`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 只向真正相邻且已经放置的贴纸传播，不让整张棋盘同步闪烁。
- 四张亮斑保持原始宽高比，仅允许轻微缩放、旋转和移动。
- 常驻微光不锁定输入；只有现有正确落位动画继续保持输入锁定，组完成仍等待传播结束后切组。
- 资源继续走现有 `UI/GameScene` 的 StreamingAssets 构建同步，不新增独立资源目录。

## 验证

- 视频尺寸 `720 x 1280`，时长 `6.171667s`，约 `185` 帧。
- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `Assembly-CSharp-Editor.csproj` 编译通过，`0` 警告、`0` 错误。
- Unity 已完成脚本、四张 PieceLight 与新 Shader 的资源刷新；Editor.log 无 C# 或 Shader 编译错误。
- 四张图片已同步到 `Assets/StreamingAssets/UI/GameScene`，Player 可按同一路径加载。
- 待 Unity Play Mode 验证常驻微光、落位传播、切组和恢复进度的视觉强度与节奏。

## 下一步

1. 在 GameScene 正确放置一块与已有区域相邻的贴纸，核对当前块增强与邻块传播节奏。

## 恢复提示

已按参考视频实现常驻不规则微光和落位相邻传播；下一步在 Unity Play Mode 核对视觉强度和节奏。
