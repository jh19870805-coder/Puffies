# 当前任务

- 任务：修正首页卡包姿态与尺寸图标层级
- 状态：代码与 Prefab 已修改，待 MainScene Play Mode 画面确认
- 更新时间：2026-08-02

## 用户意图

- 首页列表卡包保持 `EffectScene001` 中制作方 Prefab 的原始静态朝向。
- 卡包尺寸图片显示在对应卡包上方，但不能覆盖其他卡包或弹窗。

## 工作记录

- 确认 `EffectScene001` 和正式卡包 Prefab 的根节点朝向均为 `Y=180`，不是场景灯光或 Prefab Override 导致角度差异。
- 列表此前会强制执行 `Animator.Rebind()` 并采样 `CardPackOpening` 第 0 帧，动画曲线可能覆盖制作方保存的静态姿态。
- 正式 authored 卡包列表状态现改为禁用 Animator 并保留 Prefab 静态姿态；点击“玩”后才重置并播放开包动画。
- `PackItem.prefab/PackSize` 保留为编辑器配置的 Image；创建列表项后将其迁入共享根级 `CardPackSizeCanvas`，每帧根据对应卡包屏幕中心同步位置、呼吸缩放和可见范围。
- 画面验证确认仅提高 Canvas 排序或把尺寸层放在世界 `Z=-0.05` 仍可能被有厚度的 3D 卡包前表面遮挡。尺寸图标现在使用独立的 `UI/Default` 材质实例，并将 `unity_GUIZTestMode` 固定为 `Always`；尺寸 Canvas 排序为 `15000`，因此覆盖普通卡包，同时仍低于排序为 `20000` 的选择弹窗。
- 后置相机直出屏幕的首次方案仍出现单个历史卡包越过虚化层清晰显示。根因包括屏幕直出相机仍参与最终合成，以及 Layer 恢复通过全局“当前特效”引用、可能恢复到错误实例。现将 Layer 设置与恢复改为直接绑定对应 `CardPackIdleDisplay`，每次选中前先清理所有列表实例的残留 Layer。
- `PanelBagSelectCanvas` 现为排序 `20000` 的 Screen Space Overlay，确保无条件覆盖所有列表 UI 与 3D 卡包；当前选中卡包及子粒子临时切到 Layer 29，由隔离相机输出全屏透明 RenderTexture，再通过排序 `30000` 的 `SelectedCardPackCanvas` Overlay 合成，不再由后置相机直接画到屏幕。
- 选包背景改为半分辨率输出的三级降采样与回采样虚化，替代单次四分之一分辨率拉伸；拍照闪光 Canvas 改为 Screen Space Overlay，确保仍覆盖后置卡包相机。
- 尝试启动 Unity 批处理姿态诊断时发现工程已由用户编辑器打开，已终止本次批处理实例，没有关闭用户编辑器，也没有保留诊断脚本。

## 修改文件

- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `ProjectSettings/TagManager.asset`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 不通过额外欧拉角猜测修正卡包，而是保持制作方 Prefab 的静态 Transform 和蒙皮姿态。
- 不修改制作方材质、Shader、纹理、灯光和粒子参数。
- 尺寸图片继续使用 `PackItem.prefab` 中的编辑器尺寸和偏移配置；运行时共享 Canvas 只负责跟随定位。专用 UI 材质跳过 3D 深度测试，但不修改制作方卡包的任何材质或 Shader。
- 选中卡包使用独立 Layer、透明 RenderTexture 与 Overlay Canvas 解决跨 3D/UI 排序，不创建第二份卡包，不修改制作方特效参数；返回列表时按具体实例恢复原 Layer 和主相机 Culling Mask。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过，仅有仓库既有的 LF/CRLF 转换提示。
- Unity `Editor.log`：资源刷新后未发现新增 C# 编译、Prefab 导入或断言错误。
- Overlay 虚化层、透明选中卡包 RenderTexture 和具体实例 Layer 恢复尚未在 MainScene Play Mode 人工确认最终画面。

## 下一步

1. 在 MainScene Play Mode 查看首页列表，确认卡包正面朝向与 `EffectScene001` 一致。
2. 确认每个 `PackSize` 位于自己的卡包上方；点开卡包后，所有列表内容都只能存在于 Overlay 虚化背景内，透明 RenderTexture 合成的中央卡包位于其上方。
3. 点击卡包再返回，确认静态姿态不跳变；点击“玩”后确认开包动画仍从闭合状态正常开始。

## 恢复提示

继续验证 MainScene 卡包静态姿态和 `PackSize` 层级。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；不要修改制作方材质、Shader、灯光或粒子参数。
