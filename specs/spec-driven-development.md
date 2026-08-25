# Spec Driven Development

## 2026-08-20 - 缩短开包滑光结束后的切场景等待

### 需求

1. WHEN 开包模型动画与滑光主要可见画面结束 THEN 系统 SHALL 立即请求激活预加载的 GameScene，不得继续等待 Timeline 控制轨道的无画面尾段。
2. WHEN 缩短切换等待 THEN 系统 SHALL 保留 `fx_chai_w_001` 的 `0.5s` 启动延迟、整组原生播放、固定随机种子、场景 Transform、材质和渲染顺序。
3. IF 模型 Clip 包含主要动作结束后的静止尾段 THEN 系统 SHALL 允许在主要可见动作完成后切换；IF GameScene 尚未预加载完成 THEN 系统 SHALL 沿用异步加载完成后激活的兜底路径。

### 设计与任务

- [x] 核对制作方模型动画长度为约 `1.833s`，Timeline 光效控制轨道为 `0.5s` 延迟后持续约 `3.033s`。
- [x] 核对光效主要子粒子的启动延迟不超过约 `0.1s`、可见寿命约 `0.1~1.0s`；根 ParticleSystem 为循环模式，因此不使用 `IsAlive()` 作为结束条件。
- [x] 首次将场景切换等待改为 `max(模型动画长度, 0.5s + 1.2s 主要滑光窗口)`，从原 `3.533s` 缩短到约 `1.833s`。
- [x] 根据 Play Mode 仍有轻微停顿的反馈，将模型主要可见动作上限设为 `1.6s`，滑光主要窗口设为启动后的 `1.1s`；最终切换点为 `1.6s`，比上一版再缩短约 `0.23s`。
- [x] 保留粒子资源、Timeline、MainScene 场景实例和播放入口不变。
- [ ] 在 Play Mode 目视确认切换点没有截断明显光效，并确认 GameScene 入场动画完整播放。

### 验证

- 现有 Unity 日志中 CardBag 与 GameScene 在开包播放前已经到达预加载激活点；14 次 GameScene 初始化平均约 `141ms`，固定动画等待是本次主要优化目标。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。

## 2026-08-13 - 开包滑光保持原生整组播放

### 需求

1. WHEN `fx_chai_w_001` 在开包阶段启动 THEN 系统 SHALL 保持与 MainScene Inspector 中整组预览一致的粒子时序和组合效果。
2. WHEN 运行时代码播放或停止该特效 THEN 系统 SHALL 只从根 `ParticleSystem` 递归控制整组，不得再次逐个重启全部子粒子。
3. WHEN 修复播放行为 THEN 系统 SHALL 不修改场景层级、Transform、材质、发射参数、随机种子或渲染排序。

### 设计与任务

- [x] 核对 `fx_chai_w_001` 根节点本身包含 `ParticleSystem`，七个表现节点为其子节点。
- [x] 将启动逻辑改为根粒子一次 `Stop(withChildren)` 后一次 `Play(withChildren)`，等价于对整套粒子重新播放。
- [x] 将初始化和结束清理改为根粒子一次 `Stop(withChildren)`，避免父节点递归后又逐个对子节点重复操作。
- [ ] 在 MainScene Play Mode 对比 Inspector 手动预览和实际拆包阶段的光带、星星及渐变层时序。

### 验证

- [x] `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- [x] `git diff --check` 通过。
- [ ] Unity Play Mode 目视验证。

## 2026-08-13 - 首次拆包进入 GameScene 预加载

### 需求

1. WHEN 玩家第一次完成拆包动画 THEN 系统 SHALL 避免在动画结束后才同步读取 GameScene 与当前 CardBag 资源造成明显停顿。
2. WHEN 玩家进入等待划开状态 THEN 系统 SHALL 在不阻塞拆包表现的前提下预加载当前 CardBag Prefab 和 GameScene。
3. WHEN 拆包动画结束 THEN 系统 SHALL 激活已经预加载的 GameScene；IF 预加载失败 THEN 系统 SHALL 回退现有同步进入路径。
4. WHEN GameScene 激活 THEN 系统 SHALL 保持原棋盘、托盘、Piece 入场动画和玩法初始化结果。

### 设计

- `GameManager.PreloadGameScene` 先使用低优先级 `Resources.LoadAsync<GameObject>` 读取当前 `CardBagNNN`，再使用低优先级 `SceneManager.LoadSceneAsync` 将 GameScene 加载到 `progress>=0.9`，并通过 `allowSceneActivation=false` 保持 MainScene 可见。
- `EnterGameScene` 对 PackId 匹配的预加载任务只设置激活请求；玩家过早划开时由静态状态保存请求，Prefab 完成后继续加载并激活场景。
- GameScene 的 `EnsureCardBagLoaded` 优先使用预加载 Prefab，没有命中时回退 `Resources.Load`；场景初始化完成后清理静态预加载引用。
- 不提前实例化 GameScene 对象，不修改 Collider、描边、Sprite 或入场动画；新增日志记录预加载阶段、Prefab 来源和初始化总耗时。

### 验证

- [x] 两个 C# 程序集顺序编译通过，均为 `0` 警告、`0` 错误。
- [x] `git diff --check` 通过。
- [ ] 冷 Play Mode 首次拆包验证动画结束后的切场景停顿与入场动画完整性。
- [ ] 根据 `GameScene bootstrap completed in Nms` 确认是否仍需分帧初始化 Collider 和拖拽 Piece。

## 2026-08-12 - 首页卡包常驻呼吸特效

### 需求

**用户故事：** 作为玩家，我希望首页卡包列表保持柔和的常驻呼吸和高光效果，使可选择卡包具有持续的动态反馈。

1. WHEN 首页卡包处于列表可见范围 THEN 系统 SHALL 循环播放卡包整体呼吸，并显示 Prefab 配置的 ADD 高光。
2. WHEN 卡包呼吸 THEN 封面、高光和尺寸标识 SHALL 以共同中心同步缩放，列表位置与卡包间距不得变化。
3. WHEN 卡包被选中放大、移出 ScrollRect 可见范围或被设置面板遮挡 THEN 系统 SHALL 同步隐藏高光并暂停该列表项呼吸。
4. WHEN 卡包重新回到列表可见状态 THEN 系统 SHALL 恢复高光和呼吸。
5. WHEN 美术或开发打开 `PackItem.prefab` THEN Hierarchy SHALL 显示 `CardPackEffect` 常驻表现容器，呼吸范围和周期 SHALL 可在 Inspector 调整，并可在 Prefab Mode 预览。
6. WHEN 首页播放常驻效果 THEN 系统 SHALL 不加载 3D 卡包模型、撕包粒子、独立摄像机或 RenderTexture。

### 设计

- 复用 `PackageInteractionHandler` 承担卡包列表项的输入和轻量表现职责，避免新增单一用途脚本文件。
- 在 `PackItem.prefab` 新增可见的 `CardPackEffect` 子容器，将封面、高光和尺寸标识放入其中，并序列化呼吸目标、最小缩放 `0.98`、最大缩放 `1.02`、周期 `2.4s` 和编辑模式预览开关。
- 使用 `ExecuteAlways` 和不受 `Time.timeScale` 影响的时间推进；运行时呼吸不依赖 Animator，Prefab Mode 选中对象时持续刷新预览。
- `PackHighlight` 保持 Prefab 自身 UGUI ADD 材质与贴片配置，只由 `MainScene` 现有可见性判断控制父节点显隐。
- `PackageEntry` 保存高光父节点与交互组件引用，`SetPackageCoverVisible` 同步处理封面、高光和呼吸状态。
- 选择页和选中卡包继续使用 Main Camera；独立 Canvas 显式开启 `overrideSorting`。所有 Camera Canvas 坐标换算和屏幕范围计算使用所属 Canvas 的 `worldCamera`，避免沿用 Overlay 模式的 `null` 相机造成选中卡包落到屏幕外或点击范围失效。

### 任务

- [x] 1. 将呼吸参数和逻辑并入卡包列表交互组件。
- [x] 2. 在 `PackItem.prefab` 启用 ADD 高光并序列化呼吸配置。
- [x] 3. 将高光与呼吸接入列表裁切、面板遮挡和选中态显隐流程。
- [x] 4. 更新稳定项目事实和当前任务记录。
- [x] 5. 使用 Unity 和两个 C# 工程完成编译验证。
- [ ] 6. 在 Prefab Mode 和 MainScene Play Mode 目视确认幅度、层级、裁切及选择/返回流程。

### 当前验证

- Unity `2022.3.62f2c1` 批处理刷新成功；运行时与 Editor 程序集均编译成功，未报告 Prefab 丢失脚本或反序列化错误。
- `Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj` 最终顺序编译通过，均为 `0` 警告、`0` 错误。
- 静态确认 `PackHighlight` 默认启用，四张高光贴片继续引用既有 ADD Material；列表代码同步控制封面、高光和呼吸状态。
- 修复选择页空白回归：选中卡包坐标换算、Canvas 排序、面板 CanvasGroup 恢复、撕包输入范围和退出位置已适配 Camera Canvas。
- 修复确认开包后的动画空白：`BgGame` 从高排序 UGUI Image 改为主摄像机世界背景，使用早于卡包模型的渲染队列，避免全屏 Canvas 覆盖 3D 模型和粒子。
- `PackHighlightAdditive.shader` 已补齐 UGUI `_ColorMask` 属性，消除新增高光产生的材质兼容警告。
- Unity 当前实例已重新导入带 `CardPackEffect` 层级的 `PackItem.prefab`，未报告 Missing Script 或 Prefab 导入错误。
- 尚未在 Unity 可视界面检查呼吸观感、高光亮度和完整交互流程。

## 2026-08-12 - 开包动画与 MainScene 共用主摄像机

### 需求

**用户故事：** 作为玩家，我希望静态卡包切换到开包动画时没有透明合成产生的黑边，并保持现有位置、尺寸和撕包节奏。

1. WHEN 播放 3D 开包动画 THEN 卡包模型、撕口粒子、开包背景和 MainScene SHALL 由同一台 `Main Camera` 完成最终画面渲染。
2. WHEN 开包动画开始 THEN 系统 SHALL 不再创建独立特效摄像机、全屏 RenderTexture 或 RawImage 二次合成层。
3. WHEN 静态卡包切换为 3D 模型 THEN 系统 SHALL 继续按选中卡包实际屏幕中心和高度定位并等比缩放，避免位置或尺寸跳动。
4. WHEN 播放撕口粒子 THEN 系统 SHALL 保持制作方模型与粒子的固定相对坐标，不得通过屏幕蒙版识别单独移动粒子。
5. WHEN 开包结束或中断 THEN 系统 SHALL 恢复 Main Camera 的 Culling Mask，并清理模型、粒子和运行时材质。
6. WHEN 静态封面切换为 3D 模型 THEN 系统 SHALL 先准备并渲染模型第 `0` 帧，再用短时交叠淡变隐藏静态封面；Animator 和光效计时 SHALL 在交接完成后开始。

### 设计

- 将 `CardPackOpeningEffect` 收敛为普通运行时控制组件，不再创建 `RawImage`、`CardPackOpeningEffectCamera` 和 `CardPackOpeningEffectRT`。
- `Begin` 获取 `Camera.main`，将 EffectLayer 加入其 Culling Mask；Stage 直接位于主摄像机视野中，中心通过选中卡包屏幕 Rect 转换得到，缩放按主摄像机正交尺寸计算。
- 开包背景 Canvas 继续由 Main Camera 渲染，但播放阶段将其排序降到 EffectLayer 之后；卡包前后材质使用 UI 背景之后的运行时 Render Queue，粒子 Renderer 使用更高 Sorting Order。
- 模型使用制作方 `EffectScene001` 的基准 `Scale=2.63 / localZ=0`；划开光效直接复用 MainScene `Canvas/PackObject/fx_chai_w_001` 场景 Prefab 实例，并始终保留用户配置的父链和完整 Transform。主 Canvas 为 `Screen Space - Camera` 且绑定 Main Camera，已经满足同摄像机渲染要求。运行时不得加载或实例化第二份光效，不得换父节点、设置位置/旋转/缩放或覆盖 Particle Start Size、发射参数、材质和排序。
- 初始尺寸和中心只使用正面 `mesh_skin_cardPack_NNN` 包围盒，避免背面网格影响静态封面切换。
- `Begin` 只准备模型并将 Animator 固定在第 `0` 帧；等待一个渲染帧后调用 `StartPlayback`，但静态封面先全不透明保持 `0.06s`，再用 `0.12s SmoothStep` 淡出，以遮住动画开头的蒙皮预备变化。光效 `0.5s` 延迟和总播放时间从 `StartPlayback` 计算。

### 任务

- [x] 1. 明确黑边来源与同摄像机目标。
- [x] 2. 移除开包最终画面的独立相机和 RenderTexture 合成。
- [x] 3. 改造主摄像机下的模型定位、缩放和渲染顺序。
- [x] 4. 恢复制作方模型与撕口粒子的固定相对关系，删除失准的蒙版定位。
- [x] 5. 更新长期规则和当前任务记录。
- [x] 6. 编译运行时和 Editor 程序集。
- [ ] 7. 在 Play Mode 验证场景光效实例的人工尺寸、单层卡包、粒子和进场时序。
- [ ] 8. 在 Play Mode 验证静态封面到模型的首帧交叠淡变没有明显切换、闪帧或输入后额外卡顿。

### 当前验证

- 搜索确认运行时代码中不再存在 `CardPackOpeningEffectCamera`、`CardPackOpeningEffectRT`、最终画面 RawImage 及对应字段。
- 选中卡包、选择面板、开包背景、3D 模型和撕口粒子的最终画面统一通过 `Main Camera`；模型按 RectTransform 的真实屏幕中心与四角屏幕高度定位。
- 对照制作方 `EffectScene001` 确认模型基准为 `Scale=2.63 / localZ=0`；正式撕口粒子基准是 Timeline 绑定的主 Canvas 下 `fx_chai_w_001` 实例（Timeline 延迟 `0.5s`、轨道约 `3.033s`），不是场景中 `(0,1,-1.5)` 的世界空间演示实例。运行时复用 MainScene 中已人工调好的同一场景实例，不再根据演示实例坐标换算或覆盖 Transform。
- 制作方 Timeline 的正式滑光 Control Track 使用 `particleRandomSeed=1`。运行时直接播放场景粒子时，对启用 `Auto Random Seed` 的实例复现该固定种子，避免重新进入 MainScene 后随机形态变化；固定种子仅应用于运行时实例，不写回 Prefab。
- Play Mode 截图确认长名称背面网格的 `Bg01.png` 是中央灰块来源；将其替换为当前封面后出现第二层完整卡包，因此当前禁用该 Renderer，只为短名称正面网格创建动态封面材质。
- 制作方材质资源本体、FBX、Animator 和粒子 Prefab 均未修改；运行时与示例场景引用同一 GUID 的完整光效 Prefab，7 个粒子子节点没有丢失。模型保持屏幕适配尺寸，运行时只放大四个光带节点，三个星形节点保持 Prefab 原始 Transform。旧黑色矩形属于透明 RenderTexture/RawImage 二次合成异常，Main Camera 直绘后不应保留。
- 后续按资源优先原则收敛：粒子相对排序继续由 `fx_chai_w_001.prefab` 的 `0/5/10` 控制，不再由代码统一覆盖；卡包正反面 Render Queue `2001` 保存到 `test.mat` 与 `test01.mat`，运行时只替换动态贴图。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误。
- 最终修改再次编译两个工程，均为 `0` 警告、`0` 错误；`git diff --check` 通过。
- 尚未在 Unity Play Mode 目视确认划开光效的独立四倍大小是否合适、撕包时是否只剩一层完整卡包，以及禁用背面 Renderer 后上半包动画是否完整。

## 2026-08-12 - PackItem 与 MainScene 共用主摄像机

### 需求

**用户故事：** 作为开发者，我希望首页 `PackItem` 的卡包封面和高光特效与当前 MainScene 通过同一台主摄像机渲染，以便统一检查渲染层级和画面表现。

1. WHEN MainScene 加载 THEN 系统 SHALL 将承载 `PackItem` 的主 Canvas 设置为 `Screen Space - Camera`。
2. WHEN 配置 MainScene 主 Canvas THEN 系统 SHALL 将其 `World Camera` 明确绑定为场景 `Main Camera`。
3. WHEN 运行时场景配置被误改或缺失 THEN `MainScene` SHALL 在初始化时重新校正主 Canvas 的渲染模式、摄像机、设计分辨率和 Plane Distance。
4. WHEN 播放 3D 撕包动画 THEN 系统 SHALL 由同一台 `Main Camera` 直接渲染模型、粒子和开包背景，不使用独立特效相机和最终画面 RenderTexture 合成。

### 设计

- 直接修改 `MainScene.unity/Canvas`：`m_RenderMode=1`、`m_Camera=Main Camera`、`m_PlaneDistance=10`。
- 在 `MainScene.Start()` 开始阶段调用 `ConfigureMainCanvas()`，复用 `GameCommonUtility.ConfigureCanvasForGameplay` 绑定 `Camera.main`，并保持 `2560 x 1440`、`Match=0.5`、`PPU=100`。
- `PackItem/PackCover`、`PackHighlight` 和 `PackSize` 都是该主 Canvas 下的 UGUI Graphic，因此修改后统一经过 Main Camera；后续同摄像机改造同时将选中弹窗和 3D 开包最终画面接入 Main Camera，拍照闪屏继续保持独立职责。

### 任务

- [x] 1. 记录主 Canvas 与 PackItem 共用摄像机规格。
- [x] 2. 修改 MainScene 场景和运行时校正逻辑。
- [x] 3. 更新长期项目事实与当前任务记录。
- [x] 4. 编译并验证场景序列化配置。
- [ ] 5. 在 Unity Play Mode 检查首页布局、点击、裁切和高光显示。

### 当前验证

- `MainScene.unity/Canvas` 已保存为 `m_RenderMode: 1`，`m_Camera` 指向 `Main Camera` 的 Camera 组件，`m_PlaneDistance: 10`。
- `MainScene.Start()` 在解析和实例化卡包列表前调用 `ConfigureMainCanvas()`，运行时复用统一配置绑定 `Camera.main`。
- `PackItem` 的 `PackCover`、`PackHighlight` 和 `PackSize` 都继续作为主 Canvas 下的 UGUI Graphic，不增加额外相机或 RenderTexture。
- 后续开包改造已移除 `CardPackOpeningEffectCamera`、最终画面 RenderTexture 和 RawImage；EffectLayer 改由 Main Camera 直接渲染。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- 待在 Unity Play Mode 检查布局、ScrollRect 裁切、点击命中、高光材质和完整开包流程。

## 2026-08-11 - 错误 Piece 回弹反馈时序

### 需求

**用户故事：** 作为玩家，我希望错误 Piece 先自然回弹，并在重新进入黑色托盘时才得到较柔和的红色提示，避免松手瞬间的强烈闪红打断操作节奏。

1. WHEN 来自黑色托盘的 Piece 被判定为错误放置 THEN 系统 SHALL 立即开始回弹，不在松手位置先变红或停顿。
2. WHEN 回弹中的错误 Piece 首次进入可见黑色托盘区域 THEN 系统 SHALL 才显示红色反馈。
3. WHEN 显示错误红色反馈 THEN 系统 SHALL 将现有红色强度降低 `30%`，同时保持 Piece 原有透明度。
4. WHEN 正确 Piece 达到吸附标准 AND 正确槽位被外部错误 Piece 占用 THEN 系统 SHALL 正常吸附正确 Piece，并将占位错误 Piece 顶回黑色托盘。
5. WHEN 任意 Piece 从桌面或棋盘外部返回黑色托盘 THEN 系统 SHALL 在其进入托盘区域后播放同一红色错误提示，包括被正确 Piece 顶回和玩家手动拖回两种情况。
6. IF 错误 Piece 的回弹目标仍是桌面或棋盘外部位置 THEN 系统 SHALL 不因本次回弹显示托盘红色反馈。

### 设计

- 删除错误回弹开始前的红色赋值和 `0.08s` 停顿，保持现有 `0.3s` 三次方减速回弹。
- 回弹每帧使用 Piece 的世界渲染边界与当前黑色托盘世界边界做二维相交检测；首次相交时开始染红。
- 红色由 Piece 原始颜色向现有 `InvalidDropTintColor` 混合 `70%` 得到，Alpha 始终沿用原图，回弹到位后继续使用现有 `0.1s` 恢复动画。
- 正确吸附前收集与目标凹槽探针实际轮廓重叠的全部外部 Piece，将它们标记回托盘并按编号重算托盘位置；这些 Piece 分别从当前位置回弹到新托盘位置，正确 Piece 同时继续现有吸附流程。
- 外部 Piece 手动放回托盘时不再直接参与普通托盘重排，而是由同一错误回弹动画移动到重排后的目标位置并触发红色提示。
- Piece 回弹和正确吸附可能并行，使用计数式交互锁，最后一个动画结束后才允许下一次拖拽。

### 任务

- [x] 1. 记录错误回弹和红色反馈的新时序。
- [x] 2. 调整回弹动画的红色触发与强度。
- [x] 3. 更新长期规则与当前任务记录。
- [x] 4. 正确 Piece 顶回占位错误 Piece，并统一外部 Piece 回托盘反馈。
- [ ] 5. 编译并在 Unity Play Mode 验证正确顶回、手动回收和外部原位回弹。

### 当前验证

- 静态确认错误回弹开始前不再设置红色，也不再等待原 `0.08s` 停顿。
- 回弹过程只在 `state.IsOnTray` 且 Piece 渲染边界首次与可见托盘边界相交时设置红色。
- 错误红色使用 `Color.LerpUnclamped(originalColor, InvalidDropTintColor, 0.7f)`，并显式恢复原始 Alpha。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- 正确吸附分支会收集目标凹槽探针覆盖的全部外部 Piece，将其加入托盘布局并分别播放回弹；正确 Piece 同时继续吸附和持久化。
- 外部 Piece 手动进入仍有内容的托盘时，改为排除自身的托盘重排加错误回弹，不再瞬间缩回托盘尺寸。
- Piece 回弹与正确吸附共用计数式交互锁，任一并行动画未结束时保持拖拽锁定。
- 修改后再次编译 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`，均为 `0` 警告、`0` 错误。
- 待在 Unity Play Mode 目视确认正确顶回、手动回收、进入托盘触发边界和红色强度。

## 2026-08-11 - Piece 正确吸附与绿色叠加滑光优化

### 需求

**用户故事：** 作为玩家，我希望 Piece 放对后的吸附更干脆，并通过清晰但不遮盖原图的绿色滑动确认效果得到即时反馈。

1. WHEN Piece 达到正确吸附标准 THEN 系统 SHALL 将现有吸附位移动画时间缩短 `1/3`。
2. WHEN Piece 完成吸附 THEN 系统 SHALL 只在当前刚吸附的 Piece 内播放绿色滑动光带，不扩散到任何相邻已放置 Piece，也不使用整块静态染色或缩放闪色。
3. WHEN 绘制绿色确认光带 THEN 系统 SHALL 使用 ADD 加法叠加模式，并继续按 Piece Alpha 裁切，不覆盖原图细节或棋盘空白。

### 设计

- 将 `PieceSnapDuration` 从 `0.18s` 调整为 `0.12s`，即保留原时长的 `2/3`。
- 沿用现有屏幕空间滑光路径和 `0.52s` 光带时长，只为当前 `grooveImage` 创建滑光覆盖层，范围直接使用当前 Piece 的屏幕 Rect；确认颜色使用项目标准绿 `(112,151,75)`。
- `PieceLight1.png` 到 `PieceLight4.png` 使用加法材质显示不规则贴纸微光；UGUI 与 SpriteRenderer 分别使用适配自身渲染路径的材质，并由贴纸 Alpha/ SpriteMask 裁切。正确落位时按实际相邻关系从当前块向已拼块错峰传播光点回弹，同时保留 `PuzzlePlacementShine.shader` 在当前块内播放原有绿色斜向光带。

### 任务

- [x] 1. 记录吸附时长和绿色 ADD 滑光规格。
- [x] 2. 缩短正确吸附动画并切换确认光带颜色。
- [x] 3. 显式固定滑光 Shader 的 ADD 混合操作。
- [x] 4. 更新长期规则与当前任务记录。
- [x] 5. 将滑光范围限制为当前刚吸附 Piece，并删除相邻连通扫描。
- [ ] 6. 在 Unity Play Mode 验证吸附节奏、当前块范围、滑光方向、颜色和叠加效果。

### 当前验证

- `PieceSnapDuration` 已由 `0.18f` 调整为 `0.12f`，数学上等于缩短 `1/3`。
- 运行时 `_ShineColor` 已改为 `(112,151,75,230)`；Shader 默认值同步为对应归一化绿色。
- 滑光仍通过 `_SweepCenter` 在约 `0.52s` 内从当前 Piece 起点移动到终点，并按当前 Sprite Alpha 裁切。
- Shader 显式使用 `BlendOp Add` 和 `Blend SrcAlpha One`，未增加整块缩放或静态染色分支。
- 运行时只为当前 `grooveImage` 创建一个滑光覆盖层；相邻 Piece 收集、Rect 接触判断和连通队列代码已删除。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 无界面完整导入成功退出，日志无 Shader 或 C# 编译错误。
- 待在 Play Mode 目视确认 `0.12s` 吸附节奏和绿色 ADD 滑光亮度。

## 2026-08-11 - Piece 自由放置、防重叠与空托盘提醒

### 需求

**用户故事：** 作为玩家，我希望未正确吸附的 Piece 可以临时整理在桌面或棋盘空位，同时错误靠近自身凹槽时得到明确反馈，并在托盘清空后能持续识别散落 Piece。

1. WHEN 松手点不在托盘原始区域 AND Piece 达到自身凹槽吸附标准 AND 吸附目标未被其他错误 Piece 占用 THEN 系统 SHALL 正确吸附 Piece。
2. WHEN Piece 完整渲染边界位于棋盘内 AND 实际轮廓未与 Alpha 大于 0 的已拼 Piece 相交 AND 未与自己的凹槽相交 AND 未同时跨在灰色拼图区与 GameBoard 非灰色区域两侧 THEN 系统 SHALL 允许其停留；Piece 完整位于灰色区域内部或完整位于灰色区域外部均属于合法棋盘空位。IF Piece 与自己的凹槽相交但未达到正确吸附标准，或同时与灰色拼图区和非灰色区域存在实际轮廓重叠 THEN 系统 SHALL 判定放错并回归托盘。
3. WHEN Piece 完整渲染边界位于棋盘左侧或右侧的背景范围内，或在棋盘左右边界之间完整位于棋盘底边与托盘原始顶部之间 THEN 系统 SHALL 允许其停留在桌面；横跨棋盘边框或侵入托盘原始高度时 SHALL 拒绝放置。
4. WHEN Piece 与已拼 Piece 或另一块外部 Piece 的实际轮廓相交 THEN 系统 SHALL 拒绝本次放置并回弹；托盘 Piece 不参与该阻挡判定。
5. WHEN 黑色托盘中没有 Piece THEN 系统 SHALL 自动将托盘收下去，不因桌面或错误棋盘位置仍有未完成 Piece 而重新显示。
6. WHEN 鼠标或触点在托盘原始区域松手 THEN 系统 SHALL 优先恢复托盘并将当前 Piece 自动排回托盘，不受棋盘是否与托盘重叠、托盘当前隐藏状态或剩余 Piece 数量限制。
7. WHEN 黑色托盘已完全收下 AND 当前组仍有未正确吸附的外部 Piece THEN 系统 SHALL 从托盘收起完成后开始计时，每隔 `5s` 让这些错误 Piece 播放一次短暂抖动。
8. WHEN 玩家开始拖拽、Piece 正在回弹、切组、结算或托盘重新出现 THEN 系统 SHALL 暂停抖动提醒并在再次满足条件后重新计时。

### 设计

- 运行时为 Piece 创建基于 `Sprite.GetPhysicsShape` 的 `Collider2D`；没有可用轮廓时回退 Sprite 本地边界 Box。
- 松手时先用 Piece 完整 `SpriteRenderer.bounds` 判断其是否完整处于棋盘内、棋盘左右桌面，或在棋盘左右边界之间处于棋盘底边与托盘原始顶部之间；下方区域的下边界使用缓存的托盘原始归一化屏幕矩形，即使托盘收起也不扩大可放置区。随后复用 Groove Sprite Physics Shape 判断已拼占用和自身凹槽相交；棋盘灰色边缘只在 Piece 同时与 Alpha 为 0/未激活凹槽区域及 GameBoard 不透明 Physics Shape 相交时成立，单独处于灰色区域内部不阻挡。
- 松手优先级固定为：鼠标或触点进入托盘原始区域回收 -> 正确吸附目标且未被占用 -> 与其他错误 Piece 或已拼内容重叠 -> 棋盘内空位、左右桌面或棋盘下方安全桌面自由放置 -> 其他位置回弹。托盘命中不附加“必须低于棋盘底边”条件。
- 从托盘拿起最后一块时继续执行托盘下收动画，但托盘原始屏幕区域保持为回收热区；在该区域松手会立即恢复并启用托盘，刷新布局后将 Piece 动画送到按编号重新计算的托盘位置。
- 使用不受 `TimeScale` 影响的统一提醒计时和短时旋转抖动；任何交互或生命周期切换都恢复原旋转并停止旧动画。

### 任务

- [x] 1. 记录放置判定、防重叠、空托盘和提醒动画规格。
- [x] 2. 为运行时 Piece 和自身凹槽建立轮廓碰撞数据。
- [x] 3. 重构松手判定与托盘显示规则。
- [x] 4. 实现空托盘后每 `5s` 的错误 Piece 抖动提醒。
- [x] 5. 更新长期规则与当前任务记录。
- [x] 6. 增加未吸附 Piece 横跨灰色拼图区边缘的双侧真实轮廓重叠判定，并恢复自身凹槽相交但未吸附时的错误回弹；完整位于灰区内部继续允许。
- [ ] 7. 在 Play Mode 验证正确吸附、灰色区域错误相交、棋盘装饰空位自由放置、防重叠和提醒分支。

### 当前验证

- `EndDragging` 已移除整个棋盘相交判定，并按正确吸附、可见托盘回收、外部重叠、自身凹槽错误相交、自由放置的顺序处理。
- 棋盘内自由放置新增灰色拼图区与 GameBoard 非灰色区域的双侧 Physics Shape 重叠检查；正确吸附仍先于该检查，完整处于任一侧均允许，横跨边缘才复用现有错误回弹到托盘流程。
- Piece 与凹槽探针都使用同一 Sprite 的自动 Physics Shape；抽查 CardBag001 Piece 已启用 Tight Mesh、Fallback Physics Shape 和透明 Alpha。
- 外部重叠检测只遍历当前组中 `!IsPlaced && !IsOnTray` 的其他 Piece；吸附前使用凹槽探针检查目标是否被占用。
- 拿起外部 Piece 不再恢复空托盘；托盘清空后自由放置与正确吸附都保持收起，只有来自托盘的错误回弹恢复托盘。
- 提醒条件只在托盘完全收起、没有托盘 Piece 且存在外部未吸附 Piece 时成立；从条件成立开始等待 `5s`，之后每 `5s` 播放一次短暂抖动。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 无界面完整导入和脚本编译成功退出，日志无 C# 编译错误。
- 待在 Play Mode 实际拖放确认 Physics Shape 接触边界、托盘最终位置和提醒动画节奏。

## 2026-08-11 - 黑色托盘 Piece 居中与补位缓动

### 需求

**用户故事：** 作为玩家，我希望当前分组的 Piece 在黑色托盘中整齐、稳定地排列，并在拿起 Piece 后平滑补位，以便不同卡包的托盘表现一致且易于追踪。

1. WHEN 创建当前分组的托盘 Piece THEN 系统 SHALL 将 Piece 的实际渲染边界在黑色托盘内上下居中。
2. WHEN 排列任意卡包的托盘 Piece THEN 系统 SHALL 使用统一的固定水平间距，不因卡包或 Piece 尺寸改变间距值。
3. WHEN 玩家从托盘拿起一个 Piece AND 该 Piece 后方没有仍在托盘的同组 Piece THEN 系统 SHALL 不刷新其他 Piece 的位置。
4. WHEN 玩家从托盘拿起一个 Piece AND 其后方存在仍在托盘的同组 Piece THEN 系统 SHALL 仅将这些后序 Piece 向前补位，前序 Piece、桌面 Piece、Y 坐标和缩放保持不变。
5. WHEN 托盘 Piece 需要补位或回收重排 THEN 系统 SHALL 使用 `0.5s` 缓动移动到目标位置，不得瞬移。
6. WHEN Piece 因错误放置返回原托盘位置 THEN 系统 SHALL 同步恢复其他托盘 Piece 的固定间距布局。
7. WHEN 创建托盘 Piece THEN 系统 SHALL 以配置缩放后的凹槽实际显示比例作为 `DragScale`，并将 `TrayScale` 设为该比例与托盘高度 `90%` 容纳上限中的等比较小值；任何托盘 Piece 的 Scale 均不得超过 `1`。
8. WHEN Piece 从托盘拿起进入拖拽 THEN 系统 SHALL 按当前凹槽屏幕矩形刷新 `DragScale/BoardScale` 并立即恢复该目标比例；该比例包含 `CardPacks.csv/BoardScale` 且允许超过 `1`，不得继续使用托盘缩小比例。
9. WHEN 拿起后的 Piece 因直接命中托盘、错误回弹、被其他 Piece 顶回或窗口失焦而回归托盘 THEN 系统 SHALL 恢复本次拿起前保存的同一个 `TrayScale`；更新 `DragScale` 不得覆盖 `TrayScale`，回托盘也不得保留或写回 `DragScale`。
10. WHEN 托盘 Piece 播放首次入场或切组入场动画 THEN 系统 SHALL 全程保持最终 `TrayScale`，不得通过缩放过冲临时超过目标比例或 `1`。
11. WHEN 玩家松开 Piece AND Piece 的屏幕渲染边界仍与托盘原始区域相交 THEN 系统 SHALL 优先将 Piece 自动放回托盘布局位置并恢复其他 Piece 排列，即使鼠标没有移动或松手点未落入托盘矩形。
12. WHEN 当前托盘 Piece 横向范围超出托盘可视宽度 AND 玩家从托盘内未命中 Piece 的空白区域开始横向拖动 THEN 系统 SHALL 平移全部仍在托盘上的 Piece，以浏览屏幕外碎片。
13. WHEN 横向滑动托盘 Piece THEN 系统 SHALL 固定黑色托盘、棋盘和桌面 Piece，并将内容限制在首块左边缘与末块右边缘形成的有效滚动范围内，不允许整组越过托盘左右内边界。
14. WHEN 指针起点命中 Piece THEN 系统 SHALL 优先执行现有 Piece 拿取，不得启动托盘滑动；IF 托盘内容未溢出 THEN 空白区域拖动 SHALL 不移动 Piece。
15. WHEN 玩家正在拖拽托盘 Piece AND 游戏窗口失去焦点或应用暂停 THEN 系统 SHALL 立即取消拖拽，将 Piece 恢复为 `TrayScale` 并重新排回托盘，不得停在屏幕边缘或与其他 Piece 重叠。
16. WHEN 玩家正在拖拽桌面或错误棋盘 Piece AND 游戏窗口失去焦点或应用暂停 THEN 系统 SHALL 将 Piece 恢复到本次拿起前的位置；WHEN 玩家正在横向滑动托盘 THEN 系统 SHALL 结束手势并保留当前合法滑动位置。
17. WHEN 玩家拖动选中的 Piece THEN 系统 SHALL 按 Piece 当前完整渲染边界将其限制在游戏可视区域内；即使指针移出窗口，Piece 的任一边缘也不得移出可视边界。
18. WHEN 系统排列托盘 Piece THEN 最左 Piece 左边缘 SHALL 与托盘左边界保持固定 `0.6` 世界单位间隙；WHEN 托盘内容横向溢出 THEN 左右滚动安全边距 SHALL 使用相同固定值。

### 设计

- `DraggableHorizontalSpacingPixels` 从 `20` 调整为 `40`，作为所有卡包共用的设计像素间距；初始布局、拿起补位和回收重排统一使用该值。设计像素通过 `PieceBoard` 在根 Canvas 中的宽度与其当前世界宽度实时换算，不能固定按 `40 / PPU` 使用，否则正交相机自适配后屏幕间距会被缩小。
- `DraggableLeftPadding` 从固定 `0.2` 世界单位调整为固定 `0.6`，初始左边距和托盘滚动左右限制继续共用该值。
- `DragScale/BoardScale` 使用 SpriteRenderer 与凹槽的屏幕矩形直接校准；`TrayScale` 再按同一目标比例与 `PieceBoard` 高度 `90%` 上限等比取小，并硬限制为 `<=1`。
- PieceBoard 的托盘中心从根 Canvas 设计坐标直接换算到屏幕和游戏世界坐标，避免相机适配后首帧 Canvas 世界角点尚未刷新；Piece 继续使用实际 `SpriteRenderer.bounds.center` 校正最终世界坐标。
- 在 `TryBeginDrag` 中触发后序 Piece 补位；队尾没有移动目标时不启动协程。
- 新增单一托盘 Piece 重排协程，使用 `Time.unscaledDeltaTime` 和 `Mathf.SmoothStep` 在 `0.5s` 内只插值世界 X 坐标。
- 重排期间暂时禁止开始下一次拖拽；当前已拿起的 Piece 仍可继续移动和松手。
- Piece 放回托盘时按编号重新计算固定间距目标；初始建组仍即时布局，不播放补位动画。
- 松手回收同时检查指针位置和 Piece 的屏幕渲染矩形；Piece 与缓存的托盘原始屏幕矩形只要存在正面积相交，就沿用 `ReturnPieceToTray` 和现有 `0.5s` 重排流程。
- 托盘滑动使用独立输入状态和起始位置快照。开始时合并全部 `IsOnTray` Piece 的 `SpriteRenderer.bounds`，按托盘世界边界及既有内边距计算左右最大位移；移动时只更新这些 Piece 的 X 和 `StartPosition`，不改变 Y、Scale、旋转或 Piece 状态。
- 输入起始阶段先调用现有 Piece 命中检测，未命中时才尝试托盘滑动。切组、结算和对象销毁统一清理滑动状态；不增加惯性或回弹。
- `OnApplicationFocus(false)` 与 `OnApplicationPause(true)` 共用取消入口。取消逻辑不调用普通 `EndDragging`，避免失焦前最后一个屏幕边缘坐标进入吸附或自由放置判定；托盘恢复使用即时布局，不依赖失焦后的协程帧更新；重复收到失焦和暂停事件必须保持幂等。
- `UpdateDragging` 先保留现有鼠标世界坐标与抓取偏移，再复用松手阶段的 `ClampPieceToTableBounds`；该方法按当前 `SpriteRenderer.bounds` 与游戏背景世界边界计算四边偏移，因此限制完整 Piece 而不是中心点。

### 任务

- [x] 1. 修正初始托盘 Piece 的实际渲染边界垂直居中。
- [x] 2. 将拿起后的后序补位改为统一 `0.5s` 缓动，并保证队尾不刷新。
- [x] 3. 将托盘回收重排接入相同固定间距与缓动逻辑。
- [x] 4. 更新长期规则和当前任务记录。
- [x] 5. 将托盘缩放改为配置目标比例与托盘高度 `90%` 容纳上限中的等比较小值。
- [ ] 6. 编译并验证初始布局、队尾、非队尾和回收分支。

### 当前验证

- 静态检查确认所有布局入口共用 `DraggableHorizontalSpacingPixels=40`，并通过 PieceBoard 设计空间到当前相机世界空间的统一换算应用。
- 静态检查确认托盘比例以 `DragScale` 为目标，并且只在目标超过 `1` 或 Piece 高度超过托盘 `90%` 时继续等比缩小。
- 拿起补位只收集编号大于当前 Piece、仍在托盘且不是当前拖拽对象的状态；队尾目标列表为空时不启动协程。
- 初始布局与点击后的重排共用由 Canvas 设计坐标换算的托盘中心，并使用 `SpriteRenderer.bounds.center` 计算 Piece 实际渲染中心偏移；托盘重排目标继续保持同一 Y 和缩放。
- 运行时与编辑器 `dotnet build` 顺序通过，均为 `0` 警告、`0` 错误。

### 2026-08-20 回归修正

- [x] 明确拿起后的“原尺寸”是配置缩放后与凹槽实际显示一致的 `DragScale/BoardScale`，该值允许超过 `1`；托盘内仍硬限制为 `<=1`。
- [x] 改用配置目标比例和托盘设计高度共同计算 `TrayScale`：先按 Sprite 原始设计高度限制到托盘 `90%`，再与 `DragScale` 等比取小。
- [x] 移除首次入场 `1.12` 和切组入场 `1.08` 的临时放大，保留位移、旋转和淡入。
- [x] 增加 Piece 渲染边界与托盘原始区域的松手相交判定，修复原地拿起再松手后停留重叠。
- [x] 增加溢出托盘 Piece 的空白区域横向滑动、首尾边界限制及位置同步。
- [x] 增加窗口失焦与应用暂停时的拖拽取消和合法位置恢复。
- [x] 增加拖拽过程中的 Piece 完整可视边界限制。
- [x] 撤销误加到棋盘目标 Scale 的最大轴 `<=1` 钳制，恢复托盘 Piece 拿起后的原始游戏尺寸；托盘 `TrayScale<=1` 与 `90%` 上限保持不变。
- [x] 在托盘 Scale 创建、拿起前快照、直接回收、错误回弹和布局入口统一增加等比 `<=1` 校验，保证 `DragScale` 与 `TrayScale` 不会互相覆盖。
- [x] 让 `DragScale/BoardScale` 共用凹槽屏幕矩形校准结果：拿起、桌面、凹槽探针和正确吸附使用同一目标尺寸，回托盘使用 `TrayScale`。
- [ ] 在 Play Mode 使用实际横向溢出分组验证空白起手、Piece 起手、首尾边界、拿取补位和错误回收。
- [ ] 在 Play Mode 分别验证托盘 Piece、外部 Piece 和托盘滑动手势的失焦恢复。
- [ ] 在 Play Mode 使用宽、高和不规则 Piece 验证窗口四边限制，并验证仅 Piece 边缘与托盘相交时仍会回到原位。
- [x] 运行时与 Editor 程序集编译通过，并完成基础比例与相机拉远比例的公式样例验证。
- [x] 确认棋盘目标 Scale 的所有返回路径使用未钳制的屏幕校准结果；占用探针、正确吸附和桌面 Piece 使用同一目标尺寸，回托盘使用 `TrayScale`。
- [ ] 在 `CardBag021` 目视验证配置 `BoardScale=1.1` 时拿起尺寸与凹槽一致、托盘 Scale 不超过 `1`，并验证配置小于 `1` 的卡包不会在拿起时反向放大。
- Unity `2022.3.62f2c1` 无界面编译和完整资源导入通过，返回码为 `0`，日志中无 C# 错误或警告。
- `git diff --check` 通过；Unity 未修改场景、Prefab 或资源。
- 待在 Play Mode 分别拿起队首、中间、队尾 Piece，并测试错误返回和桌面回收的实际视觉节奏。

## 2026-08-07 - 卡包尺寸分档更新

### 需求

**用户故事：** 作为策划，我希望卡包尺寸按新的贴纸数量区间自动确定，以便卡包难度、尺寸展示、基础分和任务尺寸筛选使用统一定义。

1. WHEN 贴纸数量小于 `20` THEN 系统 SHALL 将卡包定义为 `XS`。
2. WHEN 贴纸数量介于 `20..30`（含边界）THEN 系统 SHALL 将卡包定义为 `S`。
3. WHEN 贴纸数量介于 `31..55`（含边界）THEN 系统 SHALL 将卡包定义为 `M`。
4. WHEN 贴纸数量介于 `56..85`（含边界）THEN 系统 SHALL 将卡包定义为 `L`。
5. WHEN 贴纸数量介于 `86..125`（含边界）THEN 系统 SHALL 将卡包定义为 `XL`。
6. WHEN 贴纸数量介于 `126..170`（含边界）THEN 系统 SHALL 将卡包定义为 `XXL`。
7. WHEN 贴纸数量大于 `170` THEN 系统 SHALL 将卡包定义为 `XXXL`。
8. WHEN 配置更新工具处理有标准 Piece 源资源的现有卡包 THEN 系统 SHALL 始终按实际片数重写 `StickerCount` 和 `PackSize`；IF `AutoUpdate=1` THEN 系统 SHALL 继续使用既有尺寸到 `BoardScale` 的映射更新棋盘缩放，ELSE 系统 SHALL 保留该行手工填写的 `BoardScale`。
9. WHEN 验证尺寸边界 THEN 系统 SHALL 覆盖 `19/20/30/31/55/56/85/86/125/126/170/171`，确保区间无断档或重叠。

### 设计

- 修改唯一尺寸判定入口 `CardBagPrefabGeneratorEditor.ResolvePackSize`，依次使用 `<20`、`<31`、`<56`、`<86`、`<126`、`<171` 判断七档尺寸。
- 不修改 `CardPackSize` 枚举值、尺寸图标映射、基础分表或既有 `ResolveBoardScale` 映射。
- 根据标准 Piece 源资源更新所有配置行的 `StickerCount` 和 `PackSize`；只对 `AutoUpdate=1` 行同步 `BoardScale`，`AutoUpdate=0` 行保留手工棋盘缩放。
- 本次不修改 SQLite 结构或 JSON 结构；已持久化卡包记录的运行时尺寸由配置读取，不需要删除本地数据。

### 任务

- [x] 1. 更新尺寸判定函数并覆盖全部新边界。
- [x] 2. 按新规则重算现有 `CardPacks.csv` 的尺寸和棋盘缩放。
- [x] 3. 更新稳定项目规则、策划记录和当前任务记录。
- [x] 4. 静态核对边界映射并使用 Unity 编译验证。

### 当前验证

- 22 个 `CardBagNNN` 源目录的标准 Piece 数量与 `CardPacks.csv/StickerCount` 全部一致。
- 22 行 `AutoUpdate=1` 配置按新分档和既有 `BoardScale` 映射检查，零不一致。
- `AutoUpdate=0` 的 CardBag001 与 CardBag018 已核对实际 Piece 数量、`StickerCount` 和 `PackSize`；工具逻辑继续更新后两者，但分别保留手工 `BoardScale=1.3` 与 `0.7`。
- 边界映射结果为：`19=XS`、`20/30=S`、`31/55=M`、`56/85=L`、`86/125=XL`、`126/170=XXL`、`171=XXXL`。
- `git diff --check` 通过。
- 当前 Unity `2022.3.62f2c1` 实例刷新后重新生成 `Assembly-CSharp-Editor.dll`；Editor.log 最近记录中 C# 错误和警告均为 `0`，无配置导入异常。
- Unity 保持打开，未修改场景、Prefab 或其他资源。

## 2026-08-07 - 三类任务配置与生成规则重构

### 需求

**用户故事：** 作为策划，我希望任务系统只使用三类明确任务，并按配置控制目标参数的递进或随机方式，以便任务节奏稳定且相邻任务不重复同一类型。

1. WHEN 生成任务类型 1 THEN 系统 SHALL 使用“完成任意拼图包，收集 N 分”，并从 `150|200|250|300` 按顺序取值，取完后循环回 `150`。
2. WHEN 生成任务类型 2 THEN 系统 SHALL 使用“从任意拼图包中收集 N 个贴纸”，并从 `45|60|80` 按顺序取值，取完后循环回 `45`。
3. WHEN 生成任务类型 3 THEN 系统 SHALL 使用“完成 N 个 S/M 尺寸的拼图包”，数量从 `2|3` 随机，尺寸从当前可玩卡包与 `S|M` 的交集中随机，两项选择互相独立。
4. WHEN 生成下一任务 THEN 系统 SHALL 排除与当前任务相同的 `TaskType`，而不是只排除相同 `TemplateId`。
5. IF 任务类型 3 当前不存在可玩的 `S` 或 `M` 卡包 THEN 系统 SHALL 将该模板视为不可用，并从另外两类任务中选择。
6. WHEN 任务 1 或任务 2 的目标被选用 THEN 系统 SHALL 分别持久化各自的递进游标，两个序列互不影响。
7. WHEN 任务配置更新 THEN 系统 SHALL 保持既有奖励、章节范围、重玩计数和结算贡献规则不变。
8. WHEN 积分任务产生超额分数 THEN 系统 SHALL 持久化超额值，并在经过其他类型任务后生成下一个积分任务时恢复为初始进度。

### 设计

- `TaskConfig.csv` 收敛为三个启用模板：类型 1 使用任意尺寸和目标池 `150|200|250|300`；类型 2 使用任意尺寸和目标池 `45|60|80`；类型 3 使用指定尺寸池 `2|3`（`S|M`）和目标池 `2|3`。
- `TaskProgressData.ScoreTargetCycleIndex` 继续保存任务 1 游标，新增 `StickerTargetCycleIndex` 保存任务 2 游标；旧 JSON 缺少新字段时由 `JsonUtility` 初始化为 `0`。
- `TaskProgressData.PendingScoreCarryOver` 保存积分任务超额值；生成非积分任务时继续保留，生成下一个积分任务时转入 `CurrentCompleteValue` 并清零。
- `TryCreateTaskInstance` 根据当前任务的 `TaskType` 排除同类型候选。首个任务的当前类型为 `None`，不执行排除。
- `ChooseTargetValue` 对任务 1、2调用同一个顺序取值函数，对任务 3继续使用随机取值。
- 任务 3 继续通过 `TryChooseEligiblePackSize` 与当前可玩卡包求交集，避免生成无法完成的尺寸任务。
- 旧配置产生的当前任务实例不会自动改写；验证新规则前删除 `persistentDataPath/LocalData.json`，SQLite 数据无需删除。

### 任务

- [x] 1. 更新 `TaskConfig.csv` 为三条新任务模板。
- [x] 2. 为贴纸任务增加独立持久化递进游标，并实现通用循环取值。
- [x] 3. 将下一任务过滤规则从相同模板改为相同任务类型。
- [x] 4. 更新任务 UI 文案和稳定项目规则文档。
- [x] 5. 使用 Unity 编译并验证配置加载、任务候选和递进逻辑。

### 当前验证

- `TaskConfig.csv` 可按 14 列结构读取，共 3 行，`TaskType` 恰好为 `1、2、3`。
- 已确认 `CardPackSize.S=2`、`CardPackSize.M=3`，任务 3 的 `SizePool=2|3` 映射正确。
- 搜索确认运行代码和稳定规则中不再存在 `LastTemplateId`、旧积分目标池或按旧模板过滤的逻辑。
- `git diff --check` 通过。
- Unity `2022.3.62f2c1` 无界面编译通过，返回码为 `0`，日志中无 C# 错误或警告。
- Unity 未修改场景、Prefab 或其他资源文件。

## 2026-07-29 - 剩余关卡切图标准化

### 需求

**用户故事：** 作为关卡资源制作者，我希望美术交付的剩余关卡切图统一为 CardBag 生成器可识别的目录和文件名，以便后续直接导入工程并批量生成 Prefab。

1. WHEN 扫描到名称为关卡数字的一级目录 THEN 系统 SHALL 将其重命名为 `CardBagXXX`，其中 `XXX` 为三位十进制编号，不足三位时左侧补 `0`。
2. WHEN 处理 `CardBagXXX/preview.png` THEN 系统 SHALL 将其重命名为 `CardBagXXX.png`。
3. WHEN 处理 `CardBagXXX/smooth/` THEN 系统 SHALL 将其中全部图片移动到 `CardBagXXX/` 一级目录。
4. WHEN 移动 `smooth/background_base.png` THEN 系统 SHALL 将其目标名称设置为 `GameBoard.png`。
5. WHEN 根目录存在 `PackTitleXXX.png` THEN 系统 SHALL 将其移动到编号相同的 `CardBagXXX/` 并重命名为 `BoardTitle.png`。
6. IF 任一源文件缺失、编号无法对应或目标路径已存在 THEN 系统 SHALL 在修改前终止整批操作，不覆盖现有文件。
7. WHEN 迁移成功 THEN 下载根目录 SHALL 保存各包的 `CardBagXXX.png`；每个 `CardBagXXX` 目录 SHALL 只保留 `GameBoard.png`、`BoardTitle.png` 和原 `smooth` 中的 `piece_###.png`；空 `smooth` 目录 SHALL 被删除。

### 设计

- 输入根目录固定为 `D:\360极速浏览器X下载\剩下关卡的切图`。
- 本批编号为 `015、016、018、019、020、021`。
- 使用 PowerShell 原生 `Move-Item` 和 `Remove-Item` 在同一文件系统内处理，不跨 Shell 传递路径。
- 写入前构造每个源到目标的完整映射，并检查源存在、目标不存在、标题编号唯一且完整。
- 先整理每个数字目录内部，再将一级目录重命名为 `CardBagXXX`；任一异常立即停止并保留错误信息。
- 不修改图片内容，不导入 Unity，不生成 Prefab。

### 任务

- [x] 1. 扫描输入目录并确认六个关卡的关键文件和标题一一对应。
- [x] 2. 完成整批源/目标路径无覆盖预检。
- [x] 3. 移动 `smooth` 图片，重命名背景、预览和标题，并标准化目录名。
- [x] 4. 验证六个目录的最终结构、文件数量和残留项。
- [x] 5. 更新 `Documents/CURRENT_TASK.md`，记录执行与验证结果。
- [x] 6. 将六张 `CardBagXXX.png` 从各自卡包目录移动到下载根目录并验证无覆盖、无残留。

### 当前验证

- 已确认六个数字目录均包含 `preview.png` 和 `smooth/background_base.png`。
- 已确认根目录存在六张一一对应的 `PackTitleXXX.png`。
- Piece 数量：`015=41`、`016=28`、`018=35`、`019=26`、`020=31`、`021=34`。
- 无覆盖预检通过：六个目录、`201` 张 smooth 图片和 `6` 张标题图均无目标冲突。
- 迁移完成：生成 `CardBag015`、`CardBag016`、`CardBag018`、`CardBag019`、`CardBag020`、`CardBag021`。
- 最终结构验证通过：下载根目录包含六张 `CardBagXXX.png`；六包均包含 `GameBoard.png`、`BoardTitle.png` 和预期 Piece，总 Piece 数为 `195`。
- 根目录没有残留数字目录或 `PackTitleXXX.png`；子目录没有残留 `smooth`、`preview.png` 或 `background_base.png`。
- 本次只整理下载目录，尚未将资源复制到 `Assets/UI/CardBags/`，也未生成或覆盖 Unity Prefab。
## 2026-08-13 - 工程冗余代码保守清理

### 需求

**用户故事：** 作为项目维护者，我希望删除已经失去调用路径的冗余代码，以降低后续维护成本，同时不改变当前已经验证的游戏行为和资源配置。

1. WHEN 某段代码在全仓库中只有定义且不存在 C# 调用、Unity 序列化引用、编辑器菜单入口或反射入口 THEN 系统 SHALL 允许删除该代码。
2. IF 无法证明代码未被 Unity 场景、Prefab、动画事件、构建回调或菜单机制使用 THEN 本轮 SHALL 保留该代码。
3. WHEN 清理完成 THEN 卡包排序、开包动画、拼图交互、任务结算和编辑器工具菜单 SHALL 保持现有行为。
4. WHEN 工作区已有已确认修改 THEN 清理 SHALL 保留该修改，不得回滚或顺带调整其参数。
5. WHEN 删除脚本或公开类型 THEN 系统 SHALL 同时确认对应 Meta GUID 未被场景、Prefab 或资源引用；存在引用时不得删除。

### 设计

- 使用编译警告、标识符全仓库引用计数、Unity YAML GUID 引用和 Git 历史共同筛选候选。
- 优先删除类内未引用私有字段、方法和已经被现行流程完全替代的兼容分支；不因文件较大而拆分或重构。
- Unity 生命周期函数、序列化字段、`MenuItem`、构建回调和可能由动画事件调用的方法不按普通 C# 引用计数直接删除。
- 修改后分别编译运行时与 Editor 程序集，并执行 `git diff --check`；最终 diff 必须只包含有证据的清理、任务记录和本节 spec。

### 任务

- [x] 1. 建立候选清单并排除 Unity 隐式入口。
- [x] 2. 删除确认无调用路径的冗余代码。
- [x] 3. 编译运行时与 Editor 程序集并检查 diff。
- [x] 4. 更新当前任务记录，写明删除内容和剩余风险。

### 当前验证

- MainScene 当前场景只存在 `PackageScrollView/Content/Page_1` 分页列表，全工程不存在 `Package001` 场景或 Prefab 对象；已删除旧单卡包列表解析、模板字段、横向手工布局和相关条件分支。
- 卡包顺序仍由 `CardPackDataUtility.TakeMainSceneOrderedPackIds()` 提供，并按原索引调用 `CreatePagedPackageSlot`，6 x 3 分页与排序逻辑未修改。
- 已确认 `BuildSync` 中列出的 11 个旧资源目录和 4 个旧 StreamingAssets 根目录均不存在；已删除每次编辑器启动重复执行的一次性迁移清理，正式 UI 同步目录、菜单入口和构建前回调保持不变。
- 全仓库孤立公开类型扫描只剩 `PackCoverShadowEffect`；该类型由 Prefab 通过 Meta GUID 引用，因此保留。私有单次引用候选均为 Unity 初始化特性回调，因此保留。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。
## 2026-08-13 - Piece 单亮光持久滑动

### 需求

1. WHEN 创建 Piece 光点 THEN 系统 SHALL 根据该 Piece 的实际宽高，从 `PieceLight1.png` 到 `PieceLight4.png` 中选择比例最合适的一张，并且每个 Piece 只显示一个光点。
2. WHEN Piece 显示在托盘或棋盘 THEN 系统 SHALL 将光点稳定放在 Piece 可见轮廓的左上区域，以匹配左上方向的全局入射光；不允许继续随机分布在中间或右下区域。
3. IF 现有四张光点的原始比例不能直接匹配 Piece THEN 系统 SHALL 允许对选中的光点做受控 X/Y 拉伸，但不得超出 Piece Alpha 遮罩。
4. WHEN Piece 正确落位 THEN 系统 SHALL 让当前块及既有相邻块的光点产生受挤压回弹：光点两端保持固定，中段沿受力方向拉伸弯曲，随后经过小幅反向回弹并恢复初始形状和位置。
5. WHEN 光点回弹完成 THEN 系统 SHALL 保留光点，不淡出、不销毁、不累计位置漂移；后续再次受力时从当前可见形状平滑接管。
6. 当前正确落位 Piece 原有的绿色斜向 ADD 光带必须保持现有 Shader、范围、颜色、时长和播放时序。
7. Piece 抵达正确凹槽并提交到棋盘后，即使绿色光带或光点回弹仍在播放，也必须允许玩家拿取托盘上的下一块 Piece。
8. 多个 Piece 的落位反馈并发时，切组或结算必须等待全部落位流程完成，并且只能触发一次。

### 设计与验证

- 使用 Piece 的 `RectTransform` 原生宽高确定目标光点尺寸和目标宽高比，在四张资源中选择比例误差最小的一张；目标尺寸限制在 Piece 尺寸的一定比例内，仅用受控非等比缩放补足差异。
- 优先从 Sprite Physics Shape 选取左上方向的可见极值点并向轮廓中心收回，得到稳定归一化位置；读取不到轮廓时回退固定左上位置。托盘 SpriteRenderer 与棋盘 UGUI 共用同一状态。
- 棋盘 UGUI 光点增加 8 段横向网格变形：两端权重为 `0`，中段使用正弦权重叠加二维弯曲位移和厚度拉伸；动画先推出，再以衰减余弦做反向回弹，最终强制归零。
- 保留当前块及最多六块实际相邻已拼 Piece 的筛选和错峰时序，但传播只驱动光点中段形变，不再永久平移光点。新传播使用递增版本从当前形变接管，旧传播停止写入。
- 保留 SpriteMask/UGUI Alpha Mask 裁切和 `PuzzlePlacementShine.shader` 当前块绿色光带；绿色滑光代码与资源不修改。
- 2026-08-20 将落位锁拆为拖拽阻塞计数和完整流程计数：`0.12s` 吸附及错误回弹继续阻塞拖拽，正确 Piece 提交棋盘后只释放对应拖拽阻塞；完整流程计数继续覆盖滑光，并由最后完成的流程统一检查切组或结算。
- 并发绿色光带各自实例化运行时 Material；持久亮光使用递增动画版本，新传播接管后旧传播不再写网格形变，避免同一亮光被两个协程争抢。
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 串行编译通过，均为 `0` 警告、`0` 错误；待 Play Mode 目视确认左上位置、形状选择和回弹幅度。

### 任务

- [x] 1. 按 Piece 实际尺寸和轮廓重写光点资源选择、缩放和左上定位。
- [x] 2. 为棋盘光点增加两端固定的分段网格形变组件。
- [x] 3. 将原永久平移传播替换为推出、衰减回弹和归零动画。
- [x] 4. 保持绿色滑光与并发落位锁逻辑不变并完成编译检查。
- [ ] 5. 在 Play Mode 验证小/宽/高 Piece、连续快速落位和切组结算。

## 2026-08-19 - CardBag022 背板视觉异常调查

### 需求

1. CardBag022 的卡包背板必须与灰色棋盘底层视觉一致，不能在右下角呈现为另一块缩小的矩形。
2. 修复不得误改底部黑色 `PieceBoard` 托盘。

### 设计

- Prefab 中 `CardBag022` 根与 `GameBoard` 均为 `2600 x 3920`，但 Unity Play Mode 实测初始化后的 `GameBoard` 被改为 `1358 x 2048`；这是 022 源图受 `maxTextureSize=2048` 降采样后的 Sprite 尺寸。
- `SyncEditorLayoutToSprites()` 原本同时同步 GameBoard 和 Piece 槽位，错误地用 `sprite.rect.size` 覆盖 GameBoard 的原始画布 RectTransform。初始化改为只同步 Piece 槽位，GameBoard 始终保留 Prefab 设计尺寸。
- 不修改 `PieceBoard`、022 源图、Prefab 或 TextureImporter；源图 Alpha 检查未发现会在运行时显示的内部矩形。

### 验证

- [x] 通过运行时 Rect 记录复现根 `2600 x 3920`、GameBoard `1358 x 2048` 的尺寸分离。
- [x] 保留 GameBoard 的 Prefab 设计尺寸，只同步 Piece 槽位。
- [x] Unity 2022.3.62f2c1 Play Mode 复验：CardBag022 根与 GameBoard 均为 `2600 x 3920`，运行时矩形一致。

## 2026-08-20 - 本地工程卡顿与缓存清理

### 目标

1. 清理不参与版本控制、可以安全重建的 Unity 编译缓存、临时文件和 IDE 缓存。
2. 压缩 Git 松散对象，减少 SourceTree 和 Git 刷新的本地负担。
3. 不删除项目资源、用户设置或能显著缩短 Unity 资源导入时间的有效缓存。
4. 将清理规则固化为仓库脚本，并在每台 Windows 开发设备注册相同的每周本地计划任务。

### 长期任务需求

1. WHEN 执行脚本但未指定操作 THEN 脚本 SHALL 只审计容量并输出报告，不删除任何内容。
2. WHEN `Library >= 10 GiB`、`Library/Bee >= 4 GiB`、临时目录合计 `>= 500 MiB` 或工程磁盘可用空间 `< 25 GiB` THEN `-Clean` SHALL 只删除白名单中的可重建缓存。
3. WHEN Git 松散对象数量 `>= 500` 或松散对象体积 `>= 256 MiB` THEN `-Clean` SHALL 执行使用默认安全保留期的 `git gc`，不得执行 `--prune=now` 或改写历史。
4. IF Unity、Bee、脚本编译或 Git 操作正在运行 THEN 脚本 SHALL 跳过可能冲突的维护并记录原因。
5. WHEN 在新设备拉取仓库 THEN Codex SHALL 通过 `AGENTS.md` 和工作流发现该维护规则，先执行审计，并在本机计划任务缺失时请求必要权限后注册。
6. WHEN 注册计划任务 THEN 系统 SHALL 每周日 `03:00` 执行阈值清理，并在错过运行时间后尽快补跑。

### 长期任务设计

- 根目录 `ProjectMaintenance.ps1` 是唯一执行入口，保持目录扁平；阈值、白名单、审计、清理、任务安装和卸载集中在同一文件。
- 支持 `-Audit`、`-Clean`、`-InstallScheduledTask`、`-UninstallScheduledTask`，无参数等同于 `-Audit`；多个操作参数同时出现时报错。
- 自动清理白名单固定为 `Library/Bee`、`BurstCache`、`ShaderCache`、`ScriptAssemblies`、`PlayerDataCache`、`BuildPlayerData`、`TempArtifacts` 以及根目录 `Temp`、`Logs`、`obj`、`.vs` 和 `debug.log`。
- `Library/Artifacts`、`Library/PackageCache`、`Assets`、`Packages`、`ProjectSettings`、`UserSettings`、`特效资源` 和 `美术切图` 永不自动删除；完整 `Library` 清理不提供自动入口。
- 删除前将每个绝对路径限制在仓库根目录并拒绝目录连接或符号链接；任务日志保存到被 Git 忽略的 `UserSettings/ProjectMaintenance.log`。
- Windows 计划任务名固定为 `Puffies Project Maintenance`，按当前设备的脚本绝对路径注册，因此脚本和规则可通过 Git 跨设备同步，但每台设备仍需本地注册一次。

### 实施任务

- [x] 实现容量审计、阈值判定、安全删除和 Git 维护。
- [x] 实现 Windows 每周计划任务安装、状态校验和卸载。
- [x] 更新 `AGENTS.md`、工作流、项目上下文和当前任务记录。
- [x] 验证默认审计无写入、低于阈值不清理、计划任务动作与本机路径正确。

### 诊断与边界

- 上次无窗口验证卡住的直接原因是受限环境中的 Unity 进程未完成项目初始化，并遇到遗留 `Temp/UnityLockfile`；授权启动后 Play Mode 验证正常完成，不是游戏代码死循环。
- 清理前 `Library` 约 `9.33 GiB`，其中 `Bee` 约 `4.99 GiB`、`Artifacts` 约 `2.64 GiB`、`PackageCache` 约 `1.65 GiB`、`BurstCache` 约 `114 MiB`。
- 删除 `Library/Bee`、`BurstCache`、`ShaderCache`、`ScriptAssemblies`、`PlayerDataCache`、`BuildPlayerData`、`TempArtifacts`；这些属于可重建编译或构建缓存。
- 保留 `Library/Artifacts` 和 `Library/PackageCache`，避免下次打开 Unity 触发完整资源和 Package 重导入。
- 清理根目录 `Temp`、`Logs`、`obj`、`.vs`，并使用 Git 自带维护命令压缩松散对象；不改写 Git 历史。
- `Assets`、`Packages`、`ProjectSettings`、`UserSettings`、`特效资源` 和 `美术切图` 不在清理范围。

### 验证

- [x] `Library` 从约 `9.33 GiB` 降至 `4.28 GiB`，释放约 `5.05 GiB`；保留的主要内容为 `Artifacts` 和 `PackageCache`。
- [x] Git 松散对象从 `635` 个降为 `0`，`git fsck --full` 和 `git diff --check` 均通过；`git status` 实测约 `42 ms`。
- [x] SourceTree、Unity、Bee 和本轮遗留编译服务均已退出；工作区只保留 `.gitignore` 与本规格记录两项预期修改。
- [ ] 下次手动打开 Unity 时等待一次脚本与构建缓存重建，并确认编辑器正常进入工程；不在本轮预生成已主动清理的缓存。
- [x] `ProjectMaintenance.ps1` 在 Windows PowerShell 5.1 中通过语法解析；默认审计报告 `Library=4.28 GiB`、`Bee=0`、临时目录 `0`、磁盘可用空间约 `589.84 GiB`，未达到清理阈值。
- [x] 使用逻辑大小 `501 MiB` 的 NTFS 稀疏文件触发临时目录阈值：脚本只删除 `Temp` 并报告释放 `501 MiB`；`Library/Artifacts`、`Library/PackageCache`、`Assets`、`Packages`、`ProjectSettings` 和 `UserSettings` 均保留。
- [x] 本机计划任务 `Puffies Project Maintenance` 已注册为每周日 `03:00`、`StartWhenAvailable=True`，动作固定为当前仓库脚本绝对路径与 `-Clean`。
- [x] 计划任务已手工触发试跑，返回结果 `0`；低于阈值时只记录“未达到清理阈值”，没有删除文件。

## 2026-08-19 - CardBag010 同位置候选误判修复

### 需求

1. 自动生成 CardBag010 时必须正确定位 `piece_011.png`，不能因同一位置附近的采样抖动报“候选不唯一”。
2. 修复不得降低颜色、结构或轮廓的全局匹配门槛，也不能把真正位于远处的相似图案合并。
3. 后续 Piece 的远端相似候选如果覆盖已经定位的面积相近 Piece 主体，必须排除该候选，不能因此拒绝正确位置。

### 设计与验证

- 离线按 Unity 像素顺序复算：最佳位置为 `(523,1022)`，颜色 `91.16%`、结构 `91.88%`；原次候选 `(515,1021)` 只相距 `8px/1px`，实际属于同一个位置簇。
- 保留 `7px` 搜索细化半径；候选簇半径按 Piece 短边 `15%` 计算并限制为 `14~48px`，统一用于感知颜色、结构和轮廓的独立候选判断。
- 离线复算 `piece_022.png`：正确位置 `(1023,767)` 的颜色匹配为 `93.74%`、结构匹配为 `93.49%`；远端次候选 `(1024,981)` 与已定位的 `piece_021.png` 主体大面积重叠。
- 生成期间维护已定位 Piece 的高 Alpha 占用，共享边缘像素归属最近定位 Piece；面积相近且重叠达到 `65%` 的最终候选会被排除，正常边缘接触和小配件覆盖继续允许。
- 最新候选复算中，正确位置重叠 `1.92%`，两个错误位置分别重叠 `84.56%`、`65.08%`；同槽位 `34px` 偏移由动态候选簇合并。处理后下一个独立候选约为 `91.66%`，与最佳点相差 `2.08%`，满足原有 `1.5%` 分差。
- 原有最低匹配率和远端候选分差全部保持不变。
- 运行时与 Editor 程序集编译通过，均为 `0` 警告、`0` 错误；待 Unity 生成窗口重试 CardBag010。

## 2026-08-20 - 棋盘底部与托盘顶部间距限制

### 需求

1. WHEN 当前拼图组根据可见槽位自动适配棋盘位置 THEN 系统 SHALL 将 `GameBoard` 底部到可见托盘顶部的屏幕间距限制在游戏可视高度的 `10%` 以内。
2. IF 自动居中后的间距已经不超过上限 THEN 系统 SHALL 保持现有棋盘位置，不额外移动。
3. WHEN 间距超过上限 THEN 系统 SHALL 只向下平移整个 CardBag 根节点，不修改卡包配置的 `BoardScale`、相机缩放、托盘位置或 Piece 在棋盘内的相对布局。
4. IF 托盘已收起、棋盘或托盘边界不可用 THEN 系统 SHALL 跳过间距限制并保留现有适配结果。
5. WHEN 切换拼图组并重新适配页面 THEN 系统 SHALL 对新组重复应用同一间距上限，并继续沿用现有切组缓动。

### 设计

- 截图中的红框约占 Game 视口高度的 `9%~10%`；使用背景可视矩形高度的 `10%` 作为分辨率无关上限，而不是写死某个窗口像素值。
- 继续先执行现有的相机适配、托盘贴底和活动组居中，再读取 `GameBoard` 的屏幕底边与托盘的屏幕顶边；只对超出部分计算向下屏幕位移。
- 通过 CardBag 父级 `RectTransform` 将屏幕位移转换为本地位移，确保 Screen Space - Camera、CanvasScaler 和不同窗口尺寸下结果一致。

### 任务

- [x] 1. 增加棋盘到托盘的最大屏幕间距常量和超限位移计算。
- [x] 2. 将限制接入首次建组及后续切组共用的棋盘自动适配流程。
- [x] 3. 编译运行时和 Editor 程序集。
- [ ] 4. 在 Play Mode 对高、宽棋盘目视验证 `10%` 上限并按实际画面微调。
## 2026-08-21 - CardBag010 正确吸附 Piece 投影

### 需求

- CardBag010 的全不透明方形 Piece 正确吸附后必须显示与其他卡包一致的 `IngameCoverShadow03` 轻投影。
- 修复不得改变 Piece 的 RectTransform、棋盘位置、缩放、吸附坐标或 Physics Shape。

### 实现与验证

- CardBag010 的全部正式 Piece 已确认存在 03 材质与 `PackCoverShadowEffect`；根因是其 `249 x 249` 源图没有透明像素，而 03 原先没有 Render Padding，2px Alpha 投影被 UGUI 网格边界裁掉。
- 将 `IngameCoverShadow03` 的 `PaddingX/Y` 从 `0` 调整为与 `BlurX/Y` 一致的 `2`；其他美术参数保持不变。
- 静态验证确认该参数只驱动渲染网格留白和 Shader UV，不参与 Piece RectTransform、Scale、吸附或碰撞计算；仍需 Play Mode 目视验收投影清晰度与拼接缝。

## 2026-08-21 - Piece 光点柔边与样式区分

### 需求

1. WHEN 常驻光点发生推出、弯曲和回弹 THEN 系统 SHALL 在整个过程中保持柔化透明边缘，不得露出矩形硬边或明显切口。
2. WHEN 同一组中存在多个 Piece THEN 系统 SHALL 稳定使用不同的光点轮廓，圆环、斜光、长弧和圆角框四种现有资源应具有明确可见的样式差异。
3. WHEN Piece 在托盘、棋盘及重进关卡之间切换 THEN 系统 SHALL 保持该 Piece 的光点资源、比例、旋转和位置一致。
4. 本次修改不得改变当前 Piece 的绿色斜向 ADD 滑光 Shader、颜色、范围、`0.52s` 时长和播放时序。

### 设计与任务

- [x] 1. 将光点资源选择改为按 Piece 正式编号稳定轮换四种样式；资源缺失时才顺序回退。
- [x] 2. 移除把四张光点强制压成同一目标宽高比的缩放，改为保持各自原始比例并只按 Piece 可用宽高做整体缩小。
- [x] 3. 为 UGUI 和 SpriteRenderer 两条 ADD 光点 Shader 路径增加小范围预乘 Alpha 柔化和纹理边界渐隐。
- [x] 4. 编译运行时与 Editor 程序集，均为 `0` 警告、`0` 错误；Unity 刷新后日志未发现 Shader 编译错误。
- [ ] 5. 在 Play Mode 目视验证回弹全过程柔边和同组四种样式差异。

## 2026-08-21 - 描边烘焙全局质量修复

### 背景与问题

- 默认连接描边由 `GameBoard` 最终外边界和已完成 Piece Alpha 接触边组合而成，两套栅格在真实交点附近可能相差数个像素并形成断口。
- 仅凭最近距离进行边界归属会把切线邻近或错误侧的边界分给当前组，产生端点尾巴和多余线段。
- 跨独立组件强制补线会产生长斜线或阶梯线；当前最多 `4px` 的受限桥接是必要保护，但尚未完成全卡包验证。
- 烘焙器当前只记录输出像素总数，没有拓扑质量门禁，无法自动发现孤立短线、异常分支、断裂组件和错误桥接。
- 搜索半径、法线采样、桥接长度和线宽均为固定源像素值，需要用不同分辨率棋盘验证其尺度适用性。

### 需求

1. WHEN 烘焙正式 CardBag THEN 系统 SHALL 对默认连接图执行连通组件、端点、分支点和桥接来源诊断，并输出可定位到 CardBag、Group 和像素区域的结果。
2. WHEN 当前最终外轮廓和已完成组接触边存在真实交点 THEN 系统 SHALL 只修补栅格化微小断口，不得新增跨区域线段。
3. IF 两个边界组件没有真实交点 THEN 系统 SHALL 保持组件独立，不得强制连接。
4. WHEN 判断线段归属 THEN 系统 SHALL 同时验证距离、局部法线方向和组身份，排除切线方向延长和错误侧边界。
5. WHEN 生成 `GroupNN.png` THEN 系统 SHALL 排除同组 Piece 接缝、未来组边界、已完成组无关边界和上一阶段整图。
6. WHEN GameBoard 分辨率变化 THEN 系统 SHALL 使用经过低、中、高分辨率样本验证并带合理上下限的尺度规则。
7. WHEN 拓扑质量超出门槛 THEN 系统 SHALL 给出明确警告或使对应组烘焙失败，不得仅以非透明像素总数作为成功依据。

### 设计约束

- 保持离线烘焙和运行时只显示 PNG 的架构，不引入运行时轮廓识别或第三方描边插件。
- 将当前最终外轮廓、已完成组接触边和桥接补点保留为可独立检查的中间蒙版，先确定错误来源再调整算法。
- 桥接必须受真实交点邻域、组身份、最大路径长度和边界走廊共同限制；禁止通过增大搜索半径或全局膨胀掩盖断口。
- 自动诊断至少记录 8 邻域组件数量、组件像素数、端点/分支点、包围盒、组件间最短距离和桥接像素数量。
- 回归样本至少覆盖低分辨率、普通分辨率和 CardBag022 高分辨率棋盘。

### 任务

- [x] 1. 记录当前已知问题、已有保护、验收标准和跨设备恢复入口。
- [x] 2. 为烘焙器增加只读拓扑诊断并生成全 CardBag 异常组清单。
- [x] 3. 分离并检查三类中间蒙版，定位断裂与错误线段的实际阶段。
- [x] 4. 收紧真实交点桥接和边界归属逻辑，验证不同分辨率的尺度规则。
- [x] 5. 增加自动质量门槛和可定位日志。
- [ ] 6. 重新烘焙全部正式 CardBag，编译程序集并完成 Play Mode 三种描边回归。

### 当前实现与验证

- 最终外轮廓和已完成区域接触边都采用跨组唯一归属；评分同时使用法线方向、最近距离、法线侧支撑量和朝向。
- 桥接只允许端点到端点，并保留约 `4px` 最大路径和真实边界走廊限制；桥接像素单独计数。
- 判定参数按 GameBoard 宽度相对 `1300px` 缩放，缩放范围限制为 `0.9~1.1`。
- 默认连接图自动移除极小孤立噪声，纹理画布边缘使用更严格的小组件阈值；关卡和贴纸独立描边不受影响。
- 已全量烘焙 23 个 CardBag Prefab，有效生成 108 个分组。旧输出扫描中的 9 组 `9~18px` 孤立块已归零；唯一保留的小组件经贴纸边界叠加确认是真实组间接触边。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 已通过，`0` 警告、`0` 错误；任务 6 仍等待 Play Mode 三种描边逐组视觉回归。

## 2026-08-25 - PackItem 完成态美术配置

### 需求

1. WHEN 首页卡包处于已完成且没有活动拼图会话的状态 THEN 系统 SHALL 使用美术在 `PackItem` 配置的完成态封面材质。
2. WHEN 首页卡包处于未开始或进行中状态 THEN 系统 SHALL 使用美术在 `PackItem` 配置的正常封面材质。
3. WHEN 卡包需要显示撕口 THEN 程序 SHALL 只向所选材质的运行时副本写入随机撕口蒙版和启用状态，不得写入灰度强度、灰色颜色或灰色遮罩参数。
4. WHEN 美术调整完成态效果 THEN 美术 SHALL 能在材质中直接调整灰度强度、灰色颜色并选择是否使用灰色蒙版贴图，无需修改 C#。
5. `PackSize` 不再由程序创建或切换置灰材质；其颜色和材质完全保留 Prefab 美术配置。

### 设计与任务

- [x] 1. 在 `PackItem` 添加 `PackCoverVisualSettings`，引用 `PackCover`、正常封面材质与完成态封面材质，并提供非运行时完成态预览开关。
- [x] 2. 新增完成态材质模板，并为 `PackCoverShadow.shader` 增加美术可调灰色颜色与可选灰色蒙版，不改变默认正常材质效果。
- [x] 3. 删除 `MainScene` 中写死的完成态灰度值、Shader 灰度属性写入及 `PackSize` 运行时置灰材质，只保留撕口蒙版注入与资源释放。
- [x] 4. Runtime/Editor 程序集编译通过，均为 `0` 警告、`0` 错误；Prefab 脚本与材质 GUID 引用完整，`git diff --check` 通过。Unity 当前占用工程且尚未自动刷新，仍需回到编辑器触发资源刷新，并在 MainScene 目视验收三种卡包状态及灰色蒙版效果。

## 2026-08-25 - 等待撕包页循环滑光提示

### 需求

1. WHEN 玩家点击“玩”或确认重玩并完成开包舞台转场 THEN 系统 SHALL 在居中卡包上循环播放 `PackItem.prefab/PackNode/ImgLight` 的现有 `PackAni` 滑动动画。
2. WHILE 系统等待玩家操作卡包 THEN 滑光 SHALL 持续循环，不得在一次动画结束后停止。
3. WHEN 玩家在卡包上完成有效轻点或达到现有横划判定 THEN 系统 SHALL 立即停止并隐藏循环滑光，然后播放现有卡包撕开模型和粒子动画。
4. IF 点击或滑动没有通过现有开包输入判定 THEN 系统 SHALL 继续播放循环滑光，不得提前停止。
5. 列表中的 `ImgLight` 默认隐藏规则、`PackAni` 美术曲线、现有撕包输入门槛、3D 模型、`fx_chai_w_001` 和切场景时序不得改变。

### 设计与任务

- [x] 1. 进入等待撕包状态后，从 `PackItem` 模板克隆 `PackNode` 动画层到 `SelectedCardPackImage`，隐藏 `PackCover/PackSize`，只启用 `ImgLight` 和现有 Animator。
- [x] 2. 在有效轻点/横划统一入口、清除选择和场景销毁时停止并释放提示动画层。
- [x] 3. Runtime/Editor 程序集编译通过，均为 `0` 警告、`0` 错误；`PackAni.anim`、`PackNode.controller`、输入阈值和正式撕包资源无修改，`git diff --check` 通过。仍需 MainScene Play Mode 目视验收提示位置、循环和交接时机。

## 2026-08-25 - 首页卡包美术呼吸动画

### 需求

1. WHEN 首页创建可见卡包列表项 THEN 系统 SHALL 默认循环播放美术提供的 `PackAniBreath` 呼吸动画。
2. IF 卡包使用完成态灰色材质 THEN 系统 SHALL 以正常卡包 `1/5` 的速度播放同一呼吸动画，不得修改美术曲线或缩放幅度。
3. IF 已完成卡包存在活动重玩会话并恢复为彩色进行中状态 THEN 系统 SHALL 恢复正常呼吸速度。
4. `BgGame` 等待撕包页的 ImgLight 提示 SHALL 继续显式播放 `PackAni` 且保持正常速度，不得继承灰色列表项速度。

### 设计与任务

- [x] 1. 将 `PackAniBreath.anim` 添加到 `PackNode.controller` 并设为首页默认状态，保留 `PackAni` 供等待撕包页显式调用。
- [x] 2. 在卡包状态刷新时设置列表项 Animator 速度：彩色 `1`、灰色 `0.2`；等待撕包提示克隆固定恢复为 `1`。
- [x] 3. Runtime/Editor 程序集编译通过，均为 `0` 警告、`0` 错误；Controller 状态、Clip GUID、Prefab Controller GUID 和 Git 差异检查通过，`PackAni.anim`、`PackAniBreath.anim` 本体均未修改。
- [ ] 4. 当前 `PackAniBreath.anim` 虽已启用循环，但文件中没有任何曲线；需要美术在 Unity 中把实际呼吸曲线保存进该 Clip 后再做 Play Mode 视觉验收。
