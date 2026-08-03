# 当前任务

- 任务：导入并重新接入新卡包特效
- 状态：已完成代码调整，等待 MainScene Play Mode 视觉验收
- 更新时间：2026-08-03

## 用户意图

- 从清空旧特效后的干净状态导入 `特效资源` 中的新资源包。
- 保持制作方 Prefab、材质、Shader、粒子、Animator 和灯光参数原样。
- 首页列表继续常驻卡包特效，选中后放大，确认后播放原开包动画并进入游戏。
- 首页直接使用 `EffectScene001` 对应的制作方 Prefab，除列表定位和暂停自动开包外，不修改制作方配置。
- UI 配置保留在 Prefab/场景中，不覆盖资源包重复携带的项目 UI、字体或 `TaskItem`。

## 工作记录

- 解析 `特效资源/effect文件夹.unitypackage`：441 项全部属于 `Assets/Resources/Effects`，已按原始路径完整导入。
- 解析 `特效资源/场景卡包和特效展示.unitypackage`：导入 `Assets/Scenes/EffectScene001.unity`；其中重复的 152 项 Effects 与第一个包逐项一致，没有重复覆盖；31 项项目 UI、字体和 `TaskItem` 未导入。
- `PackItem.prefab` 恢复 `CardPackEffect` 编辑器节点，并嵌套制作方 `CardBag_tutorial/CardPackOpening_springOuting_001` Prefab；只保留既有尺寸容器配置，没有修改制作方资源参数。
- MainScene 精确恢复 `EffectScene001` 的环境光、Skybox、Sun Source 和强度 `1.3` 的 Directional Light。
- 修正默认卡包和 PackId 8 的资源路径，由新包不存在的 `caPiBaLa` 改为 `springOuting`。`CardPackOpeningMaterial_caPiBaLa` 仍保留，因为制作方 `springOuting` Prefab 原生引用该材质。
- 2026-08-03 特效提交 `a467d66` 更新了 22 张 `Packaging` 贴图、`littleKittens`/`puppy`/`oldGadgets` 材质，并新增 `CardBag_littleKittens01` 的 6 套开包与静态 Prefab；这些制作方参数保持原样，不做代码补偿。
- 新 `CardBag_littleKittens01` 材质引用 `Packaging_022.png`，且 `EffectScene001` 新增其 `_001` 实例，已确认它是 PackId 22 的专属特效。运行时映射已扩展为 PackId 1-22，PackId 22 不再使用共享 `springOuting` 回退。
- 特效提交遗漏了 4 个 `NotoSansSC-Regular SDF*.mat.meta`。已按提交前内容和 GUID 原样恢复，避免新设备重新生成 GUID 后导致字体材质引用失效。
- 未对制作方材质、Shader、纹理、粒子、Animator、颜色、亮度或灯光做运行时参数补偿。
- 复核到 MainScene 原先会强制重绑并采样 Animator，这会改写制作方 Prefab 保存的骨骼姿态；制作方专属 Prefab 不再覆盖 Renderer 参数，也不叠加代码呼吸，只通过外层布局节点按 UI 槽位做等比缩放和定位。
- 制作方 `Puffies/2_Sided` Shader 使用 `unity_ObjectToWorld` 参与反射与高光计算，根节点缩放会直接改变亮度，是场景环境已经对齐后列表仍偏暗的主要差异。
- 桌面卡包改为统一初始化流程：直接保留 `CardPackOpening` Prefab 保存的原始姿态并将 Animator 组件 `enabled=false`，不再执行 `Rebind`、`Play(0)` 或 `Update(0)`；列表按 `240 x 272` 槽位等比适配，不叠加代码呼吸。只有手指滑动拆包成立后才重新启用 Animator，并从第 0 帧正式播放。
- 删除 MainScene 的 `ApplyPackageLifecycleVisual` 调用和方法；`Completed`、`InProgress` 与 `Unlocked` 卡包不再经过任何生命周期颜色处理，统一显示 EffectScene 制作方 Prefab 的原始样式。
- 修正 MainScene 与 `EffectScene001` 的视角差异：MainScene 主相机运行时使用制作方示例的正交尺寸 `2.66`。制作方 `Puffies/2_Sided` Shader 会根据相机世界位置计算正反面、反射和高光，原先 MainScene 的正交尺寸 `5` 会让列表边缘卡包获得不同的视线方向，表现为整体偏暗且个别卡包过曝。
- 继续逐项对齐 MainScene 与 `EffectScene001` 的渲染链路：主相机的 Clear Flags、背景色、Depth 和 Volume Layer Mask 已同步；Directional Light 补齐制作方场景携带的 Additional Light Data；主 Canvas 改为绑定主相机的 Screen Space Camera、`Plane Distance=100`、`Vertex Color Always Gamma Space=true`。原先运行时 Canvas 距离只有 `11`，与 `Z≈0` 的卡包几乎共面，会让背景 UI 与 3D 卡包发生错误覆盖并压暗列表卡包。
- 清理临时解包目录 `Temp/CodexNewFxA` 和 `Temp/CodexNewFxB`。

## 修改文件

- 新增 `Assets/Resources/Effects/`
- 新增 `Assets/Scenes/EffectScene001.unity`
- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansSC-Regular SDF*.mat.meta`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 新资源包中的原始路径和 GUID 是正式来源，不执行重命名或目录重组。
- 当前首页和开包流程只使用制作方开包 Prefab；不把示例包中重复携带的项目 UI 覆盖回工程。
- 首页卡包加载时直接禁用 Animator 并保留制作方 Prefab 的序列化姿态；桌面停留、选中移动和等待滑动期间 Animator 均保持禁用，不执行重绑或首帧采样。
- 首页卡包按现有 `240 x 272` UI 槽位统一等比缩放，选中后从该实际尺寸平滑放大到 `600 x 680`，避免列表宽度变化或点击瞬间跳变；正式拆包时再启用 Animator。
- MainScene 直接匹配 `EffectScene001` 的相机正交尺寸，不对制作方材质、颜色、亮度或灯光增加补偿。首页 Canvas 和卡包定位均按相机实时换算，列表的 UI 像素布局保持不变。
- 渲染环境对齐只修改场景相机、灯光附加数据和 Canvas 前后层级；不复制 `EffectScene001` 的演示桌面、演示卡包实例或其他演示内容，也不改变 MainScene 的业务 UI。
- 源包内部有 6 个未随包导出的旧序列化 GUID。`EffectScene001`、MainScene、`PackItem` 和 22 个当前开包 Prefab 的直接依赖完整，因此不使用相似纹理猜测补齐，也不改写制作方材质。
- `CardBag_littleKittens01` 对应 PackId 22；名称与 PackId 3 的 `CardBag_littleKittens` 相近，但两者是独立卡包，不互相替换。

## 验证

- 第一个包 441 项与导入结果逐项哈希一致，0 项不匹配。
- `EffectScene001`、MainScene 和 `PackItem` 的资源引用均能在 Assets、Packages 或 PackageCache 中解析。
- PackId 1-21 的制作方 Prefab 路径逐项存在；工程代码中不再引用不存在的 `CardBag_caPiBaLa` Prefab。
- Unity 2022.3 当前会话成功导入 `PackItem.prefab` 和 MainScene；本次会话日志未发现 C#、Shader、Prefab、Missing Reference 或资源导入错误。
- `dotnet build Puffies.sln --no-restore` 通过，0 警告、0 错误。
- Animator 初始姿态和 MainScene 相机视角修正后再次执行 `dotnet build Puffies.sln --no-restore`，仍为 0 警告、0 错误。
- 完整相机、灯光和主 Canvas 对齐后再次执行 `dotnet build Puffies.sln --no-restore`，仍为 0 警告、0 错误；结构对比确认 Camera、Camera Additional Data 和主 Canvas 无字段差异，Light 与 Additional Light Data 仅场景对象 fileID 不同。
- 移除制作方专属 Prefab 的 Animator 重绑采样、Renderer 覆盖和列表 Transform 缩放后执行 `dotnet build Puffies.sln --no-restore`，通过，0 警告、0 错误；`git diff --check` 通过。
- 特效提交同步后确认 PackId 22 Resources 路径存在；新目录与材质共 31 个 GUID 依赖全部可解析；`EffectScene001` 未修改相机、灯光、环境光或 Skybox；4 个字体材质 `.meta` 与提交前内容一致。
- 追加 PackId 22 映射后执行 `dotnet build Puffies.sln --no-restore`，通过，0 警告、0 错误。
- 修复桌面卡包宽度、首包歪斜和 Animator 启用时机后执行 `dotnet build Puffies.sln --no-restore`，通过，0 警告、0 错误；相关代码差异检查通过。
- 移除桌面卡包的 Animator 重绑和首帧采样后执行 `dotnet build Puffies.sln --no-restore`，通过，0 警告、0 错误；选中隔离层、背景虚化和原开包流程未修改。
- 删除卡包生命周期置灰入口后执行 `dotnet build Puffies.sln --no-restore`，通过，0 警告、0 错误。
- Unity BatchMode 导入验证因项目已在另一个 Unity Editor 实例中打开而未执行；未强制关闭用户编辑器。退出当前 Play Mode 后重新进入 LoadingScene，才能加载本轮 MainScene 场景序列化修改并做视觉验收。
- `git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。

## 下一步

1. 退出当前 Play Mode 后从 LoadingScene 重新进入 MainScene，检查列表卡包 `240 x 272` 槽位宽度、首包静止姿态、亮度、材质和粒子表现。
2. 检查选中放大、返回复位、拆包光效、原 Animator 开包动画及进入 GameScene。
3. 重点检查 PackId 8 的 `springOuting` 和新增 PackId 22 的 `littleKittens01`。

## 恢复提示

新特效已按原始路径导入并接回现有业务流程。继续前读取 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；不要对制作方特效参数做额外补偿，视觉问题优先对照 `Assets/Scenes/EffectScene001.unity` 排查场景环境、Prefab 选择和层级。
