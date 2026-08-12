# 当前任务

- 任务：开包动画与 MainScene 共用主摄像机
- 状态：代码和编译验证完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-12

## 用户意图

- 让首页卡包、选中卡包、开包背景、3D 撕包模型和粒子最终都由 MainScene 的 `Main Camera` 渲染，消除透明 RenderTexture 合成产生的黑边。

## 工作记录

- 确认 `PackItem/PackHighlight` 是 UGUI `Image + CanvasRenderer`，Prefab 自身没有 Camera、ParticleSystem、Animator 或独立 Canvas。
- 将 MainScene 主 Canvas 从 `Screen Space - Overlay` 改为 `Screen Space - Camera`。
- 在场景中将主 Canvas 的 `World Camera` 明确绑定到 `Main Camera`，Plane Distance 设置为 `10`。
- 新增 `MainScene.ConfigureMainCanvas()`，进入 MainScene 时再次绑定 `Camera.main`，并校正 `2560 x 1440`、`Match=0.5`、`PPU=100`。
- 将选择面板和选中卡包 Canvas 同样改为 `Screen Space - Camera` 并绑定 `Main Camera`。
- 移除 3D 撕包最终画面的 `CardPackOpeningEffectCamera + RenderTexture + RawImage` 合成链路；模型和撕口粒子直接放入 MainScene 世界，由 `Main Camera` 渲染。
- 模型按选中卡包的真实屏幕中心和四角屏幕高度等比对齐，避免静态封面切换到模型时横向偏移或尺寸跳动。
- 撕口位置继续通过临时蒙版识别，但复用同一台 `Main Camera`，临时 RT 只读数据、不参与最终画面。
- 运行时保存并恢复 Main Camera 的 Culling Mask；临时采样完整恢复 TargetTexture、ClearFlags、背景色和 Renderer 状态。

## 修改文件

- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- “同一个摄像机”覆盖开包完整最终画面：MainScene UI、选择页静态卡包、开包背景、3D 卡包模型和撕口粒子统一使用场景 `Main Camera`。
- 临时 RenderTexture 只允许用于撕口蒙版数据读取，不得作为最终画面显示或合成。
- 场景序列化和运行时校正同时保留，避免 Inspector 或场景合并导致摄像机引用丢失。
- 本次不修改制作方 FBX、Animator、粒子 Prefab、材质资源本体、贴图、配置或持久化结构，不需要清理本地数据。

## 验证

- 静态确认 `MainScene.unity/Canvas` 为 `m_RenderMode: 1`，`m_Camera` 指向 `Main Camera` 的 Camera 组件，`m_PlaneDistance: 10`。
- 静态确认 `MainScene.Start()` 在生成卡包列表前调用 `ConfigureMainCanvas()`。
- 搜索确认运行时代码不再创建 `CardPackOpeningEffectCamera`、`CardPackOpeningEffectRT` 或最终画面 RawImage。
- 静态确认模型使用选中卡包真实屏幕中心和四角屏幕高度定位，临时蒙版采样的相机状态在 `finally` 中恢复。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- 尚未在 Unity Play Mode 目视确认黑边消失、模型对齐、粒子层级和进入 GameScene 时序。

## 下一步

1. 进入 MainScene Play Mode，打开任一卡包并进入 `BgGame` 开包舞台。
2. 轻点卡包或横划，确认静态封面切 3D 模型时无黑边、中心和尺寸不跳动。
3. 确认开包背景位于模型后方，撕口粒子完整可见，约 `1.833s` 后正常进入 GameScene。
4. 返回 MainScene 再次开包，确认 Main Camera 状态恢复且重复播放正常。

## 恢复提示

MainScene UI、选中卡包和 3D 开包最终画面已统一由 `Main Camera` 渲染，独立特效相机与最终画面 RT 合成已移除；两个 C# 项目编译通过，下一步进行 Play Mode 黑边与对齐回归。
