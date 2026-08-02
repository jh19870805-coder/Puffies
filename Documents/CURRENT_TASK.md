# 当前任务

- 任务：将 EffectScene 最新卡包特效接入首页列表与展开流程
- 状态：接入与静态验证完成，待 Play Mode 画面回归
- 更新时间：2026-08-02

## 用户意图

- 以 `EffectScene001` 中最新卡包特效为唯一视觉依据。
- 完整保留制作方设置，不调整原 Prefab 的材质、颜色、贴图或特效参数。
- 首页列表和点击展开使用同一套最新卡包特效。

## 工作记录

- 扫描 `EffectScene001/卡包` 下 21 个主题卡包，通过各材质 `_FrontFacesAlbedo` 反查 `Packaging_001.png` 到 `Packaging_021.png`，确认其依次对应 PackId 1 到 21。
- `GameAnimationUtility` 增加 PackId 到制作方原始 `_001` Prefab 的准确资源映射；21 个资源路径均已核实存在。运行时只在外部增加首页定位容器，制作方 Prefab 根节点自带的 `Y=180°` 朝向和内部 Transform 保持不变。
- 首页可见卡包不再统一使用旧 `caPiBaLa` 模型并动态替换封面，而是直接实例化对应主题 Prefab。
- 对这 21 个原始实例不再写入封面、正面颜色或 ScrollView 裁剪的 `MaterialPropertyBlock`，保留制作方材质中的金属、高光、色彩和贴图参数。
- 列表与展开继续复用同一个实例：列表暂停在 Animator 闭合首帧并执行原有尺寸呼吸；点击后接管该实例移动、放大、返回或播放其原 Animator 开包流程。
- `PackItem.prefab` 的编辑器模板已改为 EffectScene 中 PackId 1 对应的 `CardBag_tutorial/CardPackOpening_springOuting_001`，并移除原 Renderer 材质覆盖。
- MainScene 的环境光、Subtractive Shadow Color 和 Directional Light 方向已同步到最新 `EffectScene001`；Skybox、灯光强度、阴影等原本一致的参数保持不变。
- PackId 22 当前没有出现在 EffectScene，也没有 `Packaging_022` 对应主题材质，因此继续使用 PackItem 回退显示，不伪造制作方特效。

## 修改文件

- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 不复制或修改 21 套制作方资源本体；直接引用 Resources 中原始 Prefab，避免产生不同步副本。
- 允许首页集成必需的外层容器位置、统一缩放、可见性、Animator 播放状态和 Renderer 排序操作；不重置制作方 Prefab 根 Transform，不修改原材质及其运行时参数。
- PackId 22 在制作方补齐资源前明确回退，不错误复用其他主题卡包。

## 验证

- 21 个 PackId 映射的 Prefab 全部存在，缺失数为 0。
- `PackItem.prefab` 新嵌套 Prefab 的 GUID、根 GameObject fileID 和 Transform fileID 均与源 Prefab 一致，旧 `caPiBaLa` GUID 和材质 Override 已清除。
- MainScene 与 EffectScene 的 RenderSettings 逐行比较无差异（Sun Source fileID 按各场景归一化）；Light 公共参数逐行比较无差异，主灯 Transform 已同步。
- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过；仅有仓库既有的 LF/CRLF 转换提示。
- Unity Editor 已自动导入 3 个资源变更并完成域重载，Editor 日志无 C# 编译或 Prefab 导入错误。
- 尚未在 MainScene Play Mode 实际检查 21 个卡包的列表画面、展开亮度和开包动画。

## 下一步

1. 在 MainScene Play Mode 检查第一页 18 个卡包，确认主题、金属高光、亮度和尺寸与 EffectScene 一致。
2. 分别展开 PackId 1、8、18、21，验证列表到居中放大过程中材质表现不变，返回后恢复原位。
3. 点击“玩”，检查各主题原 Animator、白色拆包线和拆包粒子完整播放后进入 GameScene。
4. 制作方补充 PackId 22 特效后，将其加入现有资源映射并移除该卡包回退。

## 恢复提示

继续 Puffies 最新卡包特效 Play Mode 回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；PackId 1 到 21 已直接使用 EffectScene 原始主题 Prefab且不覆盖材质参数，下一步验证首页列表、展开、返回和开包画面，不要恢复旧通用封面替换逻辑。
