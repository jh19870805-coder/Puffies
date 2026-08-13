# 当前任务

- 任务：拼图单亮光持久滑动动画
- 状态：代码与编译验证已完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-13

## 用户意图

- 每一片拼图只从 `PieceLight1.png` 到 `PieceLight4.png` 中选择一张亮光图片，只显示一个光。
- 光完成一次滑动后停留在终点，不淡出、不销毁。
- 该碎片后续被相邻传播再次触发时，从上次停留位置继续滑动。
- 当前正确落位 Piece 原有的绿色斜向 ADD 光带继续保留。

## 工作记录

- 删除每片创建两个常驻亮斑以及落位时额外创建四个或两个临时亮斑的逻辑。
- 以完整 Piece 编号保存确定性的亮光图片、初始位置、旋转、缩放和当前归一化位置；同一 Piece 从托盘切到棋盘、切组重建后仍使用同一份状态。
- 托盘 `SpriteRenderer` 和棋盘 UGUI 各只创建一个亮光，继续分别使用 `SpriteMask` 和 Alpha Mask 裁切到贴纸轮廓。
- 正确落位传播改为移动当前块和相邻已拼块各自已有的单个亮光；动画结束保持白色和终点坐标，不再淡出或销毁。
- 每次传播结束将棋盘亮光终点写回运行时状态，下一次从该位置继续。
- 原有 `PuzzlePlacementShine.shader` 绿色斜向光带未修改，仍只在刚正确落位的当前 Piece 上播放。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 四张光图按 Piece 编号确定性选择，避免托盘转棋盘或切组后随机换图。
- 持久位置使用贴纸 Rect 的归一化坐标保存，使棋盘缩放或布局变化后仍能恢复相对位置。
- 相邻块收集范围和约 `0.07~0.23s` 的错峰时机保持不变，只改变每片光的数量与生命周期。
- 不再播放常驻呼吸、缩放脉冲或传播淡入淡出；单光保持可见，只执行位移。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- 当前 Unity `Editor.log` 尾部未发现 C#、Shader、PieceLight 或空引用错误，但日志时间早于本次脚本修改，不计作本次编辑器导入验证。
- 待在 Unity Play Mode 目视确认单光数量、停留位置和二次传播的连续移动。

## 下一步

1. 在 GameScene 连续正确放置相邻 Piece，确认旧 Piece 的亮光从上一次终点继续滑动，且当前 Piece 的绿色斜向光带仍正常播放。

## 恢复提示

继续 Puffies 当前任务。先阅读 AGENTS.md、Documents/WORKFLOW.md 和 Documents/CURRENT_TASK.md；按“每片一个持久 PieceLight，后续传播从上次终点继续”的规则完成 Play Mode 视觉确认。
