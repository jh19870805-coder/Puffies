# 当前任务

- 任务：按 EffectScene001 模板重做开包流程并修复首页常驻卡包
- 状态：等待运行时确认
- 更新时间：2026-07-30

## 用户意图

- 以 `Assets/Scenes/EffectScene001.unity` 为模板，替换 MainScene 的整套开包动画表现。
- 使用模板的环境光、方向光、卡包动画和拆包粒子，同时保留现有选包、返回、重玩确认和进入 GameScene 的交互流程。

## 工作记录

- 确认 `CardPackOpening_001` 是完整卡包，`_002...006` 是独立变体而非六个叠加层；列表闭合模型使用 `CardPackStatic_001`。
- 运行时资源路径切换到新包实际目录，并接入 `EffectScene001` 的 Trilight 环境光、方向光角度、强度 `1.3` 和柔和阴影。
- 修正卡包 Prefab 自带 Y=180 度与运行时外层重复旋转的问题；模板的 Y=178.718 度现在应用到 Prefab 实例，动态 `PackIconNNN` 正确显示在正面，背面保持原材质。
- 拆包粒子切换到 `fx_chai_w_001`。针对原包缺失 Shader 的四个材质，仅在运行时克隆并映射到现有 Built-in Shader，原始资源不修改。
- 保留选包居中、返回列表、重玩确认、进入开包舞台、轻点或横划触发、动画结束进入 GameScene 的调用链。
- 修复首页列表空白和封面裁切：`CardPackStatic_001` 的 Mesh 未开启 Read/Write，直接实例化后又发现其 UV 与开包模型不一致。首页现在从实际 `CardPackOpening_001` 的第 0 帧烘焙一份可读共享 Mesh，列表和选中开包使用同一模型、朝向与 UV。
- 首页卡包和尺寸图标继续使用现有 `2.4s` 呼吸循环，在 `0.98...1.02` 缩放之间往返。新包没有专门的首页常驻粒子；`CardTrail`、`CardObtain` 和 `fx_chai` 分别保留给移动拖尾、获得和拆包用途。
- 首页默认列表改用项目侧的双面 Unlit Shader，只保留动态封面、包装背面、模型裁切和置灰颜色，不受 `EffectScene001` 方向光影响；选中和开包模型继续使用特效原始受光材质。
- 选中卡包时恢复全屏背景虚化层。截图前先隐藏被选中卡包，避免其在原列表位置留下重复虚影；虚化纹理由原来的 `1/10` 分辨率提高到 `1/4`，降低块状低清感，选中卡包和 `PanelBagSelect` 继续位于虚化层上方。
- 本次不涉及持久化结构变化，不需要删除本地 SQLite 或 JSON 数据。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Resources/CardPackListUnlit.shader`
- `Assets/Scripts/Model/CardFxRuntimeUtility.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不修改、重命名或搬移特效制作者导入的原始资源；运行时代码适配原包路径和缺失 Shader。
- 列表无光照表现通过新增项目侧 Shader 实现，不修改特效包的 `Puffies/2_Sided` Shader；选中后仍沿用原材质的灯光、高光和环境反射。
- 只播放完整的 `CardPackOpening_001`，不叠加 `_002...006`。
- 开包动画节奏使用资源原始 `1.833333s` 片段，MainScene 等待该实际时长后进入 GameScene。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- Unity 独立离屏渲染：动态 `PackIcon001` 插画和文字完整显示；动画中间帧封面贴附正常；拆包粒子正常且无洋红材质。
- Unity 运行时校验：Bounds `Center (0, 0.06, 0.02)`、Extents `(1.59, 1.80, 0.29)`；动画时长 `1.833s`；环境光 `Trilight`；方向光强度 `1.3`，结果 `PASS`。
- 已清理一次性 Editor 审计脚本。
- 首页空白根因已由 Unity Editor 日志确认：`Cannot combine mesh that does not allow access: mesh_cardPack_001`；运行时不再读取 `CardPackStatic_001` 的不可读 Mesh。
- 用户截图确认直接静态 Prefab 的封面 UV 会产生顶部弧形缺口和错误裁切，已改为烘焙 `CardPackOpening_001` 第 0 帧。
- 用户重新确认需要选中态背景虚化。`PanelBagSelectBlurredBackdrop` 已恢复，但改为排除选中卡包后按四分之一分辨率捕获，避免原列表位置的重复虚影并减轻低清块状感；`PanelBagSelect` 半透明遮罩继续叠加压暗。
- 首页修复后 `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。当前 Unity 已由用户打开，独立离屏验证因项目锁定未执行。
- 列表 Unlit 材质接入后 `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误；`git diff --check` 通过。Shader 的最终画面等待当前 Unity 运行时确认。

## 下一步

1. 在当前 Unity 中重新进入 MainScene，确认默认列表卡包不再出现方向光高光，同时保留完整封面、包装边缘、尺寸图标和呼吸缩放。
2. 点击卡包确认背景恢复虚化，原列表位置没有选中卡包残影，选中卡包保持清晰并位于 `PanelBagSelect` 上方。
3. 再走一遍 Play、轻点或横划开包及进入 GameScene，确认开包流程未回归。

## 恢复提示

继续 Puffies 当前任务。先阅读 AGENTS.md、Documents/WORKFLOW.md 和 Documents/CURRENT_TASK.md；EffectScene001 开包流程已接入，首页不可读静态 Mesh 导致的列表空白已修复，下一步是在当前 Unity 中确认列表显示和呼吸循环。
