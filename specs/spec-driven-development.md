# Spec Driven Development

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

### 设计

- 将 `CardPackOpeningEffect` 收敛为普通运行时控制组件，不再创建 `RawImage`、`CardPackOpeningEffectCamera` 和 `CardPackOpeningEffectRT`。
- `Begin` 获取 `Camera.main`，将 EffectLayer 加入其 Culling Mask；Stage 直接位于主摄像机视野中，中心通过选中卡包屏幕 Rect 转换得到，缩放按主摄像机正交尺寸计算。
- 开包背景 Canvas 继续由 Main Camera 渲染，但播放阶段将其排序降到 EffectLayer 之后；卡包前后材质使用 UI 背景之后的运行时 Render Queue，粒子 Renderer 使用更高 Sorting Order。
- 模型使用制作方 `EffectScene001` 的基准 `Scale=2.63 / localZ=0`；划开光效 `fx_chai_w_001` 使用 `(0,1,-1.5)` 和独立 `Scale=4`。屏幕尺寸适配只缩放和移动共同 Stage，四倍不得作用于卡包模型或 Stage。
- 初始尺寸和中心只使用正面 `mesh_skin_cardPack_NNN` 包围盒，避免背面网格影响静态封面切换。

### 任务

- [x] 1. 明确黑边来源与同摄像机目标。
- [x] 2. 移除开包最终画面的独立相机和 RenderTexture 合成。
- [x] 3. 改造主摄像机下的模型定位、缩放和渲染顺序。
- [x] 4. 恢复制作方模型与撕口粒子的固定相对关系，删除失准的蒙版定位。
- [x] 5. 更新长期规则和当前任务记录。
- [x] 6. 编译运行时和 Editor 程序集。
- [ ] 7. 在 Play Mode 验证黑边、光效独立四倍缩放、单层卡包、粒子和进场时序。

### 当前验证

- 搜索确认运行时代码中不再存在 `CardPackOpeningEffectCamera`、`CardPackOpeningEffectRT`、最终画面 RawImage 及对应字段。
- 选中卡包、选择面板、开包背景、3D 模型和撕口粒子的最终画面统一通过 `Main Camera`；模型按 RectTransform 的真实屏幕中心与四角屏幕高度定位。
- 对照制作方 `EffectScene001` 确认模型基准为 `Scale=2.63 / localZ=0`、撕口粒子为 `(0,1,-1.5)`；代码已按该关系实例化，并删除曾产生 `localY≈302` 的蒙版定位。
- Play Mode 截图确认长名称背面网格的 `Bg01.png` 是中央灰块来源；将其替换为当前封面后出现第二层完整卡包，因此当前禁用该 Renderer，只为短名称正面网格创建动态封面材质。
- 制作方材质资源本体、FBX、Animator 和粒子 Prefab 均未修改；模型保持屏幕适配尺寸，仅运行时的 `fx_chai_w_001` 光效实例独立放大四倍。
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
- `PuzzlePlacementShine.shader` 显式使用 `BlendOp Add` 与 `Blend SrcAlpha One`，保持带 Alpha 强度控制的加法叠加。

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

1. WHEN Piece 达到自身凹槽吸附标准 AND 吸附目标未被其他错误 Piece 占用 THEN 系统 SHALL 正确吸附 Piece。
2. WHEN Piece 完整渲染边界位于棋盘内 AND 实际轮廓未与 Alpha 大于 0 的已拼 Piece 相交 THEN 系统 SHALL 允许其停留在该棋盘空位。
3. WHEN Piece 完整渲染边界位于棋盘左侧或右侧的背景范围内 THEN 系统 SHALL 允许其停留在桌面；横跨棋盘边框时 SHALL 拒绝放置。
4. WHEN Piece 与已拼 Piece 或另一块外部 Piece 的实际轮廓相交 THEN 系统 SHALL 拒绝本次放置并回弹；托盘 Piece 不参与该阻挡判定。
5. WHEN 黑色托盘中没有 Piece THEN 系统 SHALL 自动将托盘收下去，不因桌面或错误棋盘位置仍有未完成 Piece 而重新显示。
6. WHEN 鼠标在棋盘下方的托盘原始区域松手 THEN 系统 SHALL 恢复托盘并将当前 Piece 自动排回托盘，不受托盘当前隐藏状态或剩余 Piece 数量限制。
7. WHEN 黑色托盘已完全收下 AND 当前组仍有未正确吸附的外部 Piece THEN 系统 SHALL 从托盘收起完成后开始计时，每隔 `5s` 让这些错误 Piece 播放一次短暂抖动。
8. WHEN 玩家开始拖拽、Piece 正在回弹、切组、结算或托盘重新出现 THEN 系统 SHALL 暂停抖动提醒并在再次满足条件后重新计时。

### 设计

- 运行时为 Piece 创建基于 `Sprite.GetPhysicsShape` 的 `Collider2D`；没有可用轮廓时回退 Sprite 本地边界 Box。
- 松手时先用 Piece 完整 `SpriteRenderer.bounds` 判断其是否完整处于棋盘内或棋盘左右桌面，再为 Alpha 大于 0 的已拼 Image 建立并复用不渲染的 Physics Shape 探针，用 `Collider2D.Distance().isOverlapped` 判断真实轮廓相交；Alpha 为 0 的凹槽不参与阻挡。
- 松手优先级固定为：正确吸附目标且未被占用 -> 鼠标进入托盘原始区域回收 -> 与其他错误 Piece 或已拼内容重叠 -> 棋盘内空位或左右桌面自由放置 -> 其他位置回弹。
- 从托盘拿起最后一块时继续执行托盘下收动画，但托盘原始屏幕区域保持为回收热区；在该区域松手会立即恢复并启用托盘，刷新布局后将 Piece 动画送到按编号重新计算的托盘位置。
- 使用不受 `TimeScale` 影响的统一提醒计时和短时旋转抖动；任何交互或生命周期切换都恢复原旋转并停止旧动画。

### 任务

- [x] 1. 记录放置判定、防重叠、空托盘和提醒动画规格。
- [x] 2. 为运行时 Piece 和自身凹槽建立轮廓碰撞数据。
- [x] 3. 重构松手判定与托盘显示规则。
- [x] 4. 实现空托盘后每 `5s` 的错误 Piece 抖动提醒。
- [x] 5. 更新长期规则与当前任务记录。
- [ ] 6. 编译并验证正确吸附、错误相交、自由放置、防重叠和提醒分支。

### 当前验证

- `EndDragging` 已移除整个棋盘相交判定，并按正确吸附、可见托盘回收、外部重叠、自身凹槽错误相交、自由放置的顺序处理。
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

### 设计

- 保留现有 `DraggableHorizontalSpacingPixels=20`，将其作为所有卡包共用的设计像素间距。
- 初始布局使用 `SpriteRenderer.bounds.center` 校正最终世界坐标，直接对齐实际渲染边界中心与托盘中心。
- 在 `TryBeginDrag` 中触发后序 Piece 补位；队尾没有移动目标时不启动协程。
- 新增单一托盘 Piece 重排协程，使用 `Time.unscaledDeltaTime` 和 `Mathf.SmoothStep` 在 `0.5s` 内只插值世界 X 坐标。
- 重排期间暂时禁止开始下一次拖拽；当前已拿起的 Piece 仍可继续移动和松手。
- Piece 放回托盘时按编号重新计算固定间距目标；初始建组仍即时布局，不播放补位动画。

### 任务

- [x] 1. 修正初始托盘 Piece 的实际渲染边界垂直居中。
- [x] 2. 将拿起后的后序补位改为统一 `0.5s` 缓动，并保证队尾不刷新。
- [x] 3. 将托盘回收重排接入相同固定间距与缓动逻辑。
- [x] 4. 更新长期规则和当前任务记录。
- [ ] 5. 编译并验证初始布局、队尾、非队尾和回收分支。

### 当前验证

- 静态检查确认所有布局入口共用 `DraggableHorizontalSpacingPixels=20`。
- 拿起补位只收集编号大于当前 Piece、仍在托盘且不是当前拖拽对象的状态；队尾目标列表为空时不启动协程。
- 初始布局使用 `SpriteRenderer.bounds.center` 计算实际渲染中心偏移，托盘重排目标继续保持同一 Y 和缩放。
- 运行时与编辑器 `dotnet build` 顺序通过，均为 `0` 警告、`0` 错误。
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
8. WHEN 配置更新工具处理 `AutoUpdate=1` 的现有卡包 THEN 系统 SHALL 按新尺寸重写 `PackSize`，并继续使用既有尺寸到 `BoardScale` 的映射更新棋盘缩放。
9. WHEN 验证尺寸边界 THEN 系统 SHALL 覆盖 `19/20/30/31/55/56/85/86/125/126/170/171`，确保区间无断档或重叠。

### 设计

- 修改唯一尺寸判定入口 `CardBagPrefabGeneratorEditor.ResolvePackSize`，依次使用 `<20`、`<31`、`<56`、`<86`、`<126`、`<171` 判断七档尺寸。
- 不修改 `CardPackSize` 枚举值、尺寸图标映射、基础分表或既有 `ResolveBoardScale` 映射。
- 根据 `CardPacks.csv/StickerCount` 立即更新当前 `AutoUpdate=1` 行，使现有配置与后续工具生成结果一致。
- 本次不修改 SQLite 结构或 JSON 结构；已持久化卡包记录的运行时尺寸由配置读取，不需要删除本地数据。

### 任务

- [x] 1. 更新尺寸判定函数并覆盖全部新边界。
- [x] 2. 按新规则重算现有 `CardPacks.csv` 的尺寸和棋盘缩放。
- [x] 3. 更新稳定项目规则、策划记录和当前任务记录。
- [x] 4. 静态核对边界映射并使用 Unity 编译验证。

### 当前验证

- 22 个 `CardBagNNN` 源目录的标准 Piece 数量与 `CardPacks.csv/StickerCount` 全部一致。
- 22 行 `AutoUpdate=1` 配置按新分档和既有 `BoardScale` 映射检查，零不一致。
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
