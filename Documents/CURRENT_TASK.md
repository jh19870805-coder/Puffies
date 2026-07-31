# 当前任务

- 任务：在编辑器 Prefab 中配置列表卡包特效
- 状态：已修复列表发黑逻辑，待 Unity Play Mode 最终验证
- 更新时间：2026-08-01

## 用户意图

- 卡包主体和列表项 UI 应直接配置在 `PackItem.prefab`，不要由运行时代码创建。
- 保持制作方原始特效材质、Shader、灯光和动画参数，代码只处理动态封面、尺寸、定位和业务控制。

## 工作记录

- 使用 Unity 编辑器 API 在 `PackItem.prefab/CardPackEffect` 下嵌套制作方原始 `CardPackOpening_caPiBaLa_001.prefab`。
- 在 `MainScene` 中新增并保存 `MainSceneController`，由其序列化引用 `PackItem.prefab`，确保 Player 构建直接包含该模板。
- 列表卡包改为绑定 Prefab 内已有的卡包实例；选中、返回和开包继续复用同一个实例。
- 删除列表卡包主体、尺寸图标和缺失模板时的运行时 UI 创建路径。
- `PackSize` 保留为 Prefab 内的 Image，并与本卡包一起执行呼吸缩放；普通 Canvas 层级继续受 `PanelBagSelect` 管理。
- 修复选中卡包仍被列表每帧可见性逻辑关闭的问题；选中后由选择/开包流程独占该实例，返回后再恢复列表控制。
- 修复隐藏状态下调用 Animator `Rebind/Play/Update` 导致闭合首帧初始化失败的问题；现在先激活卡包，再刷新制作方首帧，避免错误绑定姿势造成受光和亮度异常。
- `PackItem.prefab` 的卡包 Renderer 使用制作方已有的 `CardPackOpeningMaterial001.mat` Override；没有修改该材质的任何参数。
- 删除把 `Completed` 卡包运行时切换到 `CardPackOpeningMaterial_caPiBaLa.mat` 的逻辑。该材质是当前 Prefab 的暗版材质，并不是独立的完成态灰版资源，错误切换会让列表中的已完成卡包接近黑色。
- 列表不再通过代码覆盖 `_FrontFacesColor`；所有生命周期状态统一保留 `PackItem.prefab` 中配置材质的原始颜色。完成态置灰暂时停用，待制作方提供明确的灰版材质或状态资源后再接入。
- 修复 `GameScene` 开发测试“一键完成”按钮读取已不存在的 Unity 内置 `UI/Skin/UISprite.psd` 所产生的报错；按钮现在复用 `BtnTips` 的 Image Sprite、类型和材质，缺少来源时使用无 Sprite 的 Simple Image。该路径仅属于 Editor/Development Build，与 MainScene 卡包列表渲染无关。
- 一次性编辑器配置脚本执行后已删除，没有新增长期工具文件。
- 未修改持久化数据，无需删除本地存档。

## 修改文件

- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- `PackItem.prefab` 是列表卡包视觉层级的唯一编辑器配置入口。
- 不把整个 `EffectScene001` 嵌入列表项；场景环境光、Skybox 和 Directional Light 仍由 `MainScene` 全局保存一份。
- 保留开包、拖尾和拆包事件特效的按需播放逻辑；列表卡包主体不再运行时实例化制作方 Prefab。

## 验证

- Unity 2022.3.62f2 批处理执行 Prefab/场景配置成功，导入日志无 C#、Shader、YAML 或资源引用错误。
- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过，仅有工作区现有的行尾转换提示。
- 修改前的 Unity 程序集已从 `LoadingScene` 进入 `MainScene`，列表创建、点击居中和 `PanelBagSelect` 流程无 Animator inactive、卡包绑定或 Renderer 错误。
- 已在 Game 视图实际点击第一张卡包：同一特效实例正常移动并放大到屏幕中心，`PanelBagSelect`、背景虚化、普通卡包压暗、返回按钮和“玩”按钮均正常显示。
- 已逐项对比 `MainScene` 与 `EffectScene001`：Trilight 环境色、Skybox、Sun Source、Directional Light 旋转、颜色和强度 `1.3` 完全一致；未通过代码增加亮度或改写制作方材质。
- 最新列表亮度修复已通过 `dotnet build Puffies.sln --no-restore`，0 警告、0 错误。当前 Unity 进程没有可交互窗口且未刷新最新脚本，因此尚未完成本次修改后的 Play Mode 画面验证。
- GameScene 测试按钮资源修复已通过 `dotnet build Puffies.sln --no-restore`，0 警告、0 错误；工程代码中已无 `UI/Skin/UISprite.psd` 调用。

## 下一步

1. 重新聚焦或重启 Unity，等待脚本刷新后进入 MainScene，确认未完成和已完成卡包都不再发黑。
2. 回归点击居中、返回、重玩确认、轻点/横划开包以及进入 GameScene 的完整流程。

## 恢复提示

继续 Puffies 列表卡包亮度回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；完成态错误切换暗材质的逻辑已经移除，先用最新程序集验证列表亮度，再检查返回和完整开包流程。不要修改制作方材质、Shader、灯光、粒子或动画参数。
