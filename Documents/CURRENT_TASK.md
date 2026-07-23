# 当前任务

- 任务：使用常驻特效替换 MainScene 静态卡包封面
- 状态：已完成
- 更新时间：2026-07-23

## 用户意图

- 将 MainScene 列表中的每个静态卡包封面替换为使用该卡包真实封面的轻量常驻特效。
- MainScene 打开后自动播放轻微呼吸动画。
- 卡包尺寸图标保持编辑器中设置的位置和显示层级。
- 点击后停止呼吸，将选中卡包从列表尺寸放大至原始设计尺寸 `600 x 680`，播放完整六层开包动画，然后携带对应 PackId 进入 GameScene。

## 已完成

- `GameAnimationUtility` 根据六个 `CardPackOpening` 原始层生成一份共享的动画第零帧 Mesh，避免每个列表项创建六个 SkinnedMeshRenderer 和 Animator。
- MainScene 中每个可见卡包只使用一个轻量 MeshRenderer，显示对应真实封面和完成状态颜色。
- MainScene 在 `LateUpdate` 中将特效对齐到 `PackCover` 锚点，按错峰方式在 `2.4s` 内进行 `0.98..1.02` 呼吸缩放，关闭页面外 Renderer，并按 ScrollRect 视口裁剪可见片段。
- 现有 `PackSize` Image 继续作为位置、Sprite 和颜色的数据源。额外的裁剪世界层在不修改 RectTransform 或层级结构的前提下，使尺寸图标显示在世界卡包前方。
- 点击卡包后，从常驻显示切换到相同姿态的可复用六层开包特效，在 `0.3s` 内放大到 `600 x 680`，播放原动画并等待最长动画片段结束，然后进入 GameScene。
- 特效创建失败时仍可使用静态封面和阴影回退。等待奖励飞行动画的卡包继续隐藏，直到原有显示回调触发。
- 列表重建或 MainScene 销毁时，会释放按卡包生成的对象及共享运行时 Mesh 和材质。
- 删除了会在每次脚本重载后错误强制进入 Play Mode 的 `TemporaryOpenGameView`。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Resources/Effects/CardPack/CardPackOpening.shader`
- `Assets/Scripts/View/PackageInteractionHandler.cs`
- 删除 `Assets/Scripts/Editor/TemporaryOpenGameView.cs` 及其 `.meta`。
- 更新 `specs/card-pack-effects.md`。
- 更新 `Documents/PROJECT_CONTEXT.md`。

## 决策

- 使用一份共享的烘焙静态 Mesh 和一个可复用六层动画开包器，不为每个列表项常驻六个动画层。
- 列表特效使用当前卡包的真实封面，并保留现有 `PackSize` 编辑器配置。
- 点击流程固定为：先放大，再播放原始开包动画，最后切换到 GameScene。
- 导入的拆包粒子 Prefab 暂不加入已确认的 MainScene 流程，等待其播放时机获得明确批准。

## 验证

- MainScene 自动化 Play Mode 验证成功创建四个常驻卡包特效，并选中 PackId 1。
- 选中卡包的呼吸缩放从 `2.449215` 变化到 `2.52881527`；PackSize 的位置、同级顺序、Sprite 和颜色保持不变。
- 点击流程将可复用开包器放大到 `6.246120`，启动全部六个 Animator 和六个 Renderer，然后进入 GameScene。
- 已目视检查 `2560 x 1440` 的常驻和开包截图。动态封面、前景尺寸图标、列表对齐和撕开动画均正常，没有重叠错乱。
- Unity 完成一次干净的批处理刷新，没有 C# 或 Shader 编译错误。
- `dotnet build Puffies.sln --no-restore` 完成，runtime、first-pass 和 Editor 程序集均为 0 警告、0 错误。
- 不需要重置本地 JSON 或 SQLite 数据。

## 下一步

1. 用户在正常 Unity Editor 流程中体验呼吸幅度和 `0.3s` 放大节奏。
2. 拆包粒子的播放时机继续作为独立后续项；未经明确批准，不加入已确认流程。

## 恢复提示

继续 Puffies 开发。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；MainScene 常驻卡包特效流程已经实现并验证，直接遵循用户的最新指令，不要擅自改变该流程。
