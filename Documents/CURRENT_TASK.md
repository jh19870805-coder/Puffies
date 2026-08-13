# 当前任务

- 任务：MainScene 编辑器打开后自动显示页面
- 状态：代码与编译验证已完成，等待 Unity 编辑器交互验证
- 更新时间：2026-08-13

## 用户意图

- 在 Unity 编辑器从 LoadingScene 双击打开 MainScene 时，Scene 视图应直接显示页面。
- 不再要求额外双击 Hierarchy 中的 Canvas 才能看到 UI。
- 不改变 MainScene 的运行时 Canvas 渲染模式和同摄像机开包特效方案。

## 工作记录

- 确认 MainScene 根 Canvas 当前为 `Screen Space - Camera` 并绑定 `Main Camera`，LoadingScene 根 Canvas 为 `Screen Space - Overlay`。
- 页面内容、Canvas 启用状态和相机引用均正常；问题来自 Unity 在切换场景后保留旧 Scene 视图观察位置。
- 在现有 `CanvasDesignResolutionEditor` 中监听 `EditorSceneManager.sceneOpened`。
- 非播放模式打开 MainScene 后延迟一帧读取根 Canvas 世界四角，并让当前 SceneView 自动 Frame 到该范围。
- 自动取景不修改 Selection、场景资源、Canvas RenderMode、Camera 或运行时逻辑。

## 修改文件

- `Assets/Scripts/Editor/CanvasDesignResolutionEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 保留 MainScene 的 `Screen Space - Camera`，因为开包模型、粒子、背景和 UI 需要继续由同一台 Main Camera 渲染。
- 该行为只作用于 Unity 编辑器中打开 MainScene，不进入 Player 构建，也不改变运行时页面导航。
- 使用 RectTransform 世界边界直接取景，不临时修改用户当前 Selection。

## 验证

- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `Assembly-CSharp-Editor.csproj` 编译通过，`0` 警告、`0` 错误。
- 待在 Unity 中从 LoadingScene 双击打开 MainScene，确认无需再双击 Canvas。

## 下一步

1. 在 Unity 编辑器复现 LoadingScene -> MainScene 资源双击切换并确认自动取景。

## 恢复提示

MainScene 页面没有丢失；其 Camera Canvas 导致 SceneView 切场景后保留旧观察位置。编辑器工具已增加 MainScene 打开后的 Canvas 自动取景，下一步编译并在 Unity 中验证。
