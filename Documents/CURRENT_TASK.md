# 当前任务

- 任务：首页卡包常驻呼吸与开包特效修复
- 状态：代码和编译验证完成，等待 Play Mode 视觉确认
- 更新时间：2026-08-12

## 用户意图

- 首页列表卡包继续常驻播放呼吸效果，并保留 ADD 高光。
- `PackItem.prefab` 的 Hierarchy 中要能直接看到常驻表现容器，呼吸参数可调并能在 Prefab Mode 预览。
- 卡包进入选中放大流程或列表不可见时，呼吸和高光必须一起隐藏；返回列表后恢复。

## 工作记录

- 确认 `PackItem/PackHighlight` 是 UGUI `Image + CanvasRenderer`，Prefab 自身没有 Camera、ParticleSystem、Animator 或独立 Canvas。
- 将 MainScene 主 Canvas 从 `Screen Space - Overlay` 改为 `Screen Space - Camera`。
- 在场景中将主 Canvas 的 `World Camera` 明确绑定到 `Main Camera`，Plane Distance 设置为 `10`。
- 新增 `MainScene.ConfigureMainCanvas()`，进入 MainScene 时再次绑定 `Camera.main`，并校正 `2560 x 1440`、`Match=0.5`、`PPU=100`。
- 将选择面板和选中卡包 Canvas 同样改为 `Screen Space - Camera` 并绑定 `Main Camera`。
- 移除 3D 撕包最终画面的 `CardPackOpeningEffectCamera + RenderTexture + RawImage` 合成链路；模型和撕口粒子直接放入 MainScene 世界，由 `Main Camera` 渲染。
- 模型按选中卡包的真实屏幕中心和四角屏幕高度等比对齐，避免静态封面切换到模型时横向偏移或尺寸跳动。
- 删除失准的运行时撕口蒙版识别。该逻辑在主摄像机方案下曾将光效根节点错误移动到 `localY≈302`；撕口光效恢复制作方场景保存的固定相对位置 `(0,1,-1.5)`。
- 运行时保存并恢复 Main Camera 的 Culling Mask；临时采样完整恢复 TargetTexture、ClearFlags、背景色和 Renderer 状态。
- 删除代码对全部粒子 Renderer 的统一 `sortingOrder=31000` 覆盖，恢复 `fx_chai_w_001.prefab` 自身保存的 `0/5/10` 相对层级。
- 将卡包正反面 `Render Queue=2001` 从运行时代码移入 `test.mat` 和 `test01.mat`，使编辑器 Inspector 成为表现配置来源。
- 在 `PackItem.prefab` 新增可见的 `CardPackEffect` 子容器，将 `PackCover / PackHighlight / PackSize` 统一放入其中；根节点现有 `PackageInteractionHandler` 驱动该容器 `0.98 ↔ 1.02 / 2.4s` 循环呼吸，列表布局根节点保持 `Scale=1`。
- 启用 `PackHighlight` 父节点，首页常驻显示四张使用 `PackHighlightAdditive.mat` 的 UGUI ADD 高光。
- 将高光和呼吸纳入 `MainScene` 现有卡包显隐逻辑；翻页裁切、打开设置面板、选择卡包时同步隐藏/暂停，返回后恢复。
- 呼吸职责并入已有卡包交互组件，没有新增单一用途脚本文件；首页仍不实例化 3D 模型或粒子。
- 修复 Main Camera 改造后的选择页空白回归：Camera Canvas 的屏幕坐标转本地坐标改为传入目标 Canvas Camera，并为选择面板和选中卡包 Canvas 显式启用 `overrideSorting`。
- 选择面板每次显示时显式恢复 `CanvasGroup.alpha/interactable/blocksRaycasts`；等待撕包输入范围和进入 GameScene 前的卡包下沿记录也统一使用所属 Canvas Camera。
- 修复确认开包后只剩背景、看不到动画的问题：`BgGame` 不再作为高排序 UGUI Image 覆盖 3D 模型，改为同一 `Main Camera` 下的世界 `SpriteRenderer` 背景，并以 `Geometry` 队列先于卡包模型绘制；模型、Animator 和制作方粒子 Prefab 参数保持不变。
- 补齐 `PackHighlightAdditive.shader` 的 UGUI `_ColorMask` 属性，避免首页每个高光贴片持续产生材质警告。
- 修复开包模型与撕口粒子比例失真：旧的 `Scale=550 / Z=164.02565` 属于独立特效相机演示参数，不适用于当前 Main Camera 世界承载。模型恢复制作方 `EffectScene001` 中的基准 `Scale=2.63 / localZ=0`，外层 Stage 再整体适配选中卡包屏幕高度，使模型和 `fx_chai_w_001` 保持制作方原始相对比例与深度。
- 模型对齐包围盒只读取正面 `mesh_skin_cardPack_NNN`，不再让动画背面网格改变静态封面切换时的尺寸和中心。
- 根据 Play Mode 截图确认中央灰块来自长名称背面网格引用的制作方占位贴图 `Bg01.png`；将该网格替换为当前封面后又出现第二层完整卡包，因此运行时暂时禁用长名称背面 Renderer，只保留短名称正面网格绑定当前 `PackIconNNN`。
- 增加开包 Stage 对齐诊断日志，输出 Game View 分辨率、选中卡包目标像素高度、首帧正面 Bounds、Stage 缩放和屏幕中心。模型继续按选中静态卡包尺寸适配；对比参考画面后，直接将 `fx_chai_w_001` 的 `line`、`line01`、`glow`、`glowC` 四个光带节点放大 `3.5` 倍，不放大卡包模型、共同 Stage 或光效根节点。
- 优化静态封面切换到 3D 撕包模型的衔接：模型先完成实例化、贴图替换、定位并固定在 Animator 第 `0` 帧；实际渲染一帧后启动 Animator，但静态封面继续全不透明遮住开头 `0.06s` 的蒙皮预备变化，随后用 `0.12s SmoothStep` 淡出。动画完成时间和划光延迟均从 Animator 真正启动时计算。
- 核对 `EffectScene001` 与运行时资源：两者引用同一 GUID 的完整 `fx_chai_w_001.prefab`，7 个粒子子节点都在，场景实例没有删除节点或组件，代码也没有禁用子特效。旧黑色矩形来自透明 RenderTexture 经 RawImage 二次合成时的透明/混合异常；统一 Main Camera 后直接把 ADD 粒子绘制到桌面，黑框消失是预期修复，不是特效丢失。
- 确认该 Prefab 的 ParticleSystem 使用 `scalingMode=Local`：父级光效根缩放不会按预期放大粒子，却会使对子节点的反向补偿真实缩小星星。已撤销光效根缩放和星星反向补偿；`dot`、`dot01`、`glow01` 完全保持 Prefab 原始 Transform，仅直接缩放四个光带节点。

## 修改文件

- `Assets/Scenes/MainScene.unity`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/View/PackageInteractionHandler.cs`
- `Assets/Prefabs/PackItem.prefab`
- `Assets/Resources/Effects/CardFx/Materials/test.mat`
- `Assets/Resources/Effects/CardFx/Materials/test01.mat`
- `Assets/Resources/PackHighlightAdditive.shader`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- “同一个摄像机”覆盖开包完整最终画面：MainScene UI、选择页静态卡包、开包背景、3D 卡包模型和撕口粒子统一使用场景 `Main Camera`。
- 开包流程不再创建或读取任何撕口蒙版 RenderTexture。
- 能在资源中稳定配置的表现参数放在 Prefab/Material：粒子相对排序、材质引用、Blend 和 Render Queue；代码不覆盖这些资源参数。
- 代码只保留运行时动态职责：共用 Main Camera、卡包封面替换、屏幕位置尺寸适配、播放时序和 EffectLayer。
- 场景序列化和运行时校正同时保留，避免 Inspector 或场景合并导致摄像机引用丢失。
- 本次不修改制作方 FBX、Animator、粒子 Prefab、材质资源本体、贴图、配置或持久化结构，不需要清理本地数据。
- 首页常驻效果定义为静态封面整体呼吸与 UGUI ADD 高光；点击后的 3D 卡包模型和撕包粒子仍只在 `BgGame` 开包舞台加载。
- 开包背景必须处于 3D 卡包之后；不能使用高 Sorting Order 的全屏 UGUI 背景覆盖世界模型。世界背景使用独立运行时 Sprite 材质并先于卡包正反面 `Render Queue=2001` 绘制。

## 验证

- 静态确认 `MainScene.unity/Canvas` 为 `m_RenderMode: 1`，`m_Camera` 指向 `Main Camera` 的 Camera 组件，`m_PlaneDistance: 10`。
- 静态确认 `MainScene.Start()` 在生成卡包列表前调用 `ConfigureMainCanvas()`。
- 搜索确认运行时代码不再创建 `CardPackOpeningEffectCamera`、`CardPackOpeningEffectRT` 或最终画面 RawImage。
- 静态确认模型使用选中卡包真实屏幕中心和四角屏幕高度定位，尺寸计算只使用正面网格。
- 静态确认 `fx_chai_w_001.prefab` 的粒子 Renderer 保留 `sortingOrder=0/5/10`，代码不再调用统一排序覆盖；`test.mat` 与 `test01.mat` 均保存 `m_CustomRenderQueue: 2001`。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 批处理刷新成功，运行时和 Editor 程序集均由 Unity 编译成功，Prefab 未报告脚本丢失或反序列化错误。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 在最终改动后再次顺序编译通过，均为 `0` 警告、`0` 错误。
- Unity 当前实例已自动重新导入 `PackItem.prefab`，Editor.log 未出现 Missing Script、Prefab 导入错误或 C# 编译错误。
- 点击空白修复后，`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 再次编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。
- 静态确认开包背景已脱离 `PanelBagSelectCanvas`，使用主摄像机世界 `SpriteRenderer`，不会再以 Canvas Sorting Order 覆盖 3D 模型。
- 对照 `EffectScene001` 确认开包模型基准为 `Scale=2.63 / localZ=0`，撕口粒子为 `(0,1,-1.5)`；运行时代码已使用相同相对关系。
- 最新修改后 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 再次编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。
- 重复背面网格禁用和划开光效独立四倍缩放后，两个 C# 工程再次编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。
- 静态封面到模型的首帧交叠淡变完成后，两个 C# 工程编译通过，均为 `0` 警告、`0` 错误。最新版本改为 `StartPlayback()` 后先由静态封面遮挡 `0.06s`，再淡出 `0.12s`；播放完成和划光延迟使用真实 Animator 启动时间，仍待 Play Mode 视觉确认。
- 根据最新 Play Mode 日志确认静态舞台与模型首帧宽高比均约为 `0.882`，比例差约 `0.03%`；PackIcon Alpha 可见宽度为纹理宽度的 `99%~100%`，排除矩形比例和透明留白导致的明显拉宽。问题收敛到 Animator 开头的蒙皮变化，已改为在静态封面下预播放极短时段。
- 尚未在 Unity Play Mode 目视确认黑边消失、模型对齐、粒子层级和进入 GameScene 时序。
- 待在 Play Mode 确认光带节点 `3.5x` 后宽度接近参考画面，星星恢复 Prefab 原始尺寸与分布，且卡包模型尺寸、Stage 和撕口位置没有变化。
- 滑光独立调整后，`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误。
- 已根据 Play Mode 结果否定光效根缩放方案：根缩放没有放大 Local 模式粒子，星形节点反向补偿反而导致星星过小。当前已改为直接缩放四个光带节点；两个 C# 工程重新编译通过，均为 `0` 警告、`0` 错误，等待视觉验证。

## 下一步

1. 重新打开 `PackItem.prefab`，确认 Hierarchy 显示 `PackItem/CardPackEffect/PackCover|PackHighlight|PackSize`，并在根节点 `PackageInteractionHandler / Breathing Effect` 中预览幅度与周期。
2. 进入 MainScene Play Mode，确认首页可见卡包持续呼吸且 ADD 高光正常，没有改变列表间距或位置。
3. 点击任一卡包，确认其他卡包、原位置高光和呼吸显示全部消失；返回时正常恢复。
4. 继续进入 `BgGame` 并轻点/横划卡包，确认卡包模型保持原适配尺寸、四个光带节点放大到 `3.5x`、星形粒子恢复原尺寸，撕包时只存在一层完整卡包，且关闭背面网格后上半包撕裂动画没有空白；随后确认进入 GameScene 时序。

## 恢复提示

首页 `PackItem` 已增加可见的 `CardPackEffect` 子容器并恢复常驻整体呼吸和 UGUI ADD 高光；Main Camera 改造遗留的选择页坐标、Canvas 排序和后续点击范围问题已修复。下一步重新进入 Play Mode 验证首页、选择页和完整开包流程。
