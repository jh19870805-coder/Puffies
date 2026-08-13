# 当前任务

- 任务：UI 场景在编辑器打开后自动正向显示页面
- 状态：代码与编译验证已完成，等待 Unity 编辑器交互验证
- 更新时间：2026-08-13

## 用户意图

- 在 Unity 编辑器双击切换各 UI 场景时，Scene 视图应直接正向显示页面。
- 从 MainScene 切换到 LoadingScene、GameScene、RankScene 或 AchieveScene 时，也不能继承歪斜的观察角度。
- 不再要求额外双击 Hierarchy 中的 Canvas 才能看到正常 UI。
- 不改变 MainScene 的运行时 Canvas 渲染模式和同摄像机开包特效方案。

## 工作记录

- 确认 MainScene 根 Canvas 当前为 `Screen Space - Camera` 并绑定 `Main Camera`，LoadingScene 根 Canvas 为 `Screen Space - Overlay`。
- 页面内容、Canvas 启用状态和相机引用均正常；问题来自 Unity 在切换场景后保留旧 Scene 视图观察位置。
- 在现有 `CanvasDesignResolutionEditor` 中监听 `EditorSceneManager.sceneOpened`。
- 首版仅处理 MainScene 且只 Frame 边界，导致从 MainScene 切到其他场景时仍保留歪斜角度；现已补全。
- 非播放模式单独打开 Loading、Main、Game、Rank 或 Achieve 场景后，延迟一帧读取根 Canvas 世界四角，将 SceneView 恢复为正交正视并立即 Frame 到该范围。
- 自动取景不修改 Selection、场景资源、Canvas RenderMode、Camera 或运行时逻辑。

## 修改文件

- `Assets/Scripts/Editor/CanvasDesignResolutionEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 保留各场景原有 Canvas RenderMode；MainScene 继续使用 `Screen Space - Camera`。
- 该行为只作用于 Unity 编辑器中单独打开五个正式 UI 场景，不进入 Player 构建，也不改变运行时页面导航或 Additive 场景工作流；`EffectScene001` 保留制作方的三维编辑视角。
- 使用 RectTransform 世界边界直接取景，不临时修改用户当前 Selection。

## 验证

- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `Assembly-CSharp-Editor.csproj` 编译通过，`0` 警告、`0` 错误。
- 待在 Unity 中双向切换 MainScene 与其他 UI 场景，确认页面均自动正向显示。

## 下一步

1. 在 Unity 编辑器双向切换 MainScene 与其他 UI 场景并确认自动正视取景。

## 恢复提示

页面没有丢失；SceneView 会在场景间保留观察位置和角度。编辑器工具正在补全所有正式 UI 场景打开后的正交正视取景。
