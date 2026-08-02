# 当前任务

- 任务：导入并重新接入新卡包特效
- 状态：已完成，等待 MainScene Play Mode 视觉验收
- 更新时间：2026-08-02

## 用户意图

- 从清空旧特效后的干净状态导入 `特效资源` 中的新资源包。
- 保持制作方 Prefab、材质、Shader、粒子、Animator 和灯光参数原样。
- 首页列表继续常驻卡包特效，选中后放大，确认后播放原开包动画并进入游戏。
- 首页卡包的初始骨骼姿态以 `EffectScene001` 中 Animator 默认状态的第 0 帧为准。
- UI 配置保留在 Prefab/场景中，不覆盖资源包重复携带的项目 UI、字体或 `TaskItem`。

## 工作记录

- 解析 `特效资源/effect文件夹.unitypackage`：441 项全部属于 `Assets/Resources/Effects`，已按原始路径完整导入。
- 解析 `特效资源/场景卡包和特效展示.unitypackage`：导入 `Assets/Scenes/EffectScene001.unity`；其中重复的 152 项 Effects 与第一个包逐项一致，没有重复覆盖；31 项项目 UI、字体和 `TaskItem` 未导入。
- `PackItem.prefab` 恢复 `CardPackEffect` 编辑器节点，并嵌套制作方 `CardBag_tutorial/CardPackOpening_springOuting_001` Prefab；只保留既有尺寸容器配置，没有修改制作方资源参数。
- MainScene 精确恢复 `EffectScene001` 的环境光、Skybox、Sun Source 和强度 `1.3` 的 Directional Light。
- 修正默认卡包和 PackId 8 的资源路径，由新包不存在的 `caPiBaLa` 改为 `springOuting`。`CardPackOpeningMaterial_caPiBaLa` 仍保留，因为制作方 `springOuting` Prefab 原生引用该材质。
- 21 个制作方 `_001` 开包 Prefab 已全部映射到 PackId 1-21。新包没有第 22 个制作方 Prefab，PackId 22 继续使用 `PackItem` 内的共享 `springOuting` 实例并替换卡包封面。
- 未对制作方材质、Shader、纹理、粒子、Animator、颜色、亮度或灯光做运行时参数补偿。
- 修正 MainScene 与 `EffectScene001` 的卡包姿态差异：列表卡包创建或重新显示时执行制作方 Animator 的 `Rebind -> Play(CardPackOpening, 0) -> Update(0)`，随后将速度暂停为 `0`；不再于采样前禁用 Animator。
- 修正 MainScene 与 `EffectScene001` 的视角差异：MainScene 主相机运行时使用制作方示例的正交尺寸 `2.66`。制作方 `Puffies/2_Sided` Shader 会根据相机世界位置计算正反面、反射和高光，原先 MainScene 的正交尺寸 `5` 会让列表边缘卡包获得不同的视线方向，表现为整体偏暗且个别卡包过曝。
- 清理临时解包目录 `Temp/CodexNewFxA` 和 `Temp/CodexNewFxB`。

## 修改文件

- 新增 `Assets/Resources/Effects/`
- 新增 `Assets/Scenes/EffectScene001.unity`
- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 新资源包中的原始路径和 GUID 是正式来源，不执行重命名或目录重组。
- 当前首页和开包流程只使用制作方开包 Prefab；不把示例包中重复携带的项目 UI 覆盖回工程。
- 首页只采样制作方开包动画第 0 帧，不让动画自动推进；点击确认后的正式开包仍从第 0 帧重置并正常播放。
- MainScene 直接匹配 `EffectScene001` 的相机正交尺寸，不对制作方材质、颜色、亮度或灯光增加补偿。首页 Canvas 和卡包定位均按相机实时换算，列表的 UI 像素布局保持不变。
- 源包内部有 6 个未随包导出的旧序列化 GUID。`EffectScene001`、MainScene、`PackItem` 和 21 个当前开包 Prefab 的直接依赖完整，因此不使用相似纹理猜测补齐，也不改写制作方材质。

## 验证

- 第一个包 441 项与导入结果逐项哈希一致，0 项不匹配。
- `EffectScene001`、MainScene 和 `PackItem` 的资源引用均能在 Assets、Packages 或 PackageCache 中解析。
- PackId 1-21 的制作方 Prefab 路径逐项存在；工程代码中不再引用不存在的 `CardBag_caPiBaLa` Prefab。
- Unity 2022.3 当前会话成功导入 `PackItem.prefab` 和 MainScene；本次会话日志未发现 C#、Shader、Prefab、Missing Reference 或资源导入错误。
- `dotnet build Puffies.sln --no-restore` 通过，0 警告、0 错误。
- Animator 初始姿态和 MainScene 相机视角修正后再次执行 `dotnet build Puffies.sln --no-restore`，仍为 0 警告、0 错误。
- `git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。

## 下一步

1. 在 MainScene Play Mode 依次检查首页列表常驻效果、尺寸图标呼吸和层级。
2. 检查选中放大、返回复位、拆包光效、原 Animator 开包动画及进入 GameScene。
3. 重点检查 PackId 8 的 `springOuting` 和缺少专属制作方 Prefab 的 PackId 22。

## 恢复提示

新特效已按原始路径导入并接回现有业务流程。继续前读取 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；不要对制作方特效参数做额外补偿，视觉问题优先对照 `Assets/Scenes/EffectScene001.unity` 排查场景环境、Prefab 选择和层级。
