# 项目上下文

Unity **2022.3** / URP 2D 项目。核心循环：打开卡包 -> 拖放拼图 -> 任务奖励。本文档是需求、场景、数据、资源、构建规则和命名的稳定项目参考。

当前工作状态记录在 [CURRENT_TASK.md](CURRENT_TASK.md)，工作流规则记录在 [WORKFLOW.md](WORKFLOW.md)，已确认的长期游戏设计规则记录在 [GAME_DESIGN_REQUIREMENTS.md](GAME_DESIGN_REQUIREMENTS.md)。

---

## 1. 功能需求

### 核心循环

1. `LoadingScene` 初始化本地数据、任务配置、卡包配置和持久化存储。
2. `MainScene` 根据解锁状态显示可玩的卡包。
3. 点击已解锁卡包，播放开包表现并进入 `GameScene`。
4. 玩家拖动拼图碎片，完成选中卡包对应的拼图。
5. 拼图完成后显示 `RewardPanel`，结算任务进度并保存卡包状态。
6. `BtnFinish` 返回 `MainScene`，卡包列表按最新解锁状态刷新。

### 首个 Demo 范围

- 首个 Demo 不实现排行榜功能。除非 Demo 范围发生变化，否则不要接入排行榜后端数据、替换模拟列表、增加排行榜持久化，或修复 Player Build 中 `RankItem.prefab` 的加载。
- 现有 `RankScene` 和 MainScene 排行榜入口仅为占位，不属于 Demo 验收范围。

### 场景需求

| 场景 | 需求 |
|------|------|
| LoadingScene | 初始化 JSON、SQLite、任务数据和卡包数据；加载结束后进入 MainScene |
| MainScene | 根据 `CardPacks.csv` 与 SQLite 解锁状态刷新卡包列表；每页按 6 列 x 3 行显示 18 个带呼吸动画的轻量常驻卡包特效；将选中特效放大到 `600 x 680`，使用真实封面播放完整通用 3D 开包模型，然后进入 GameScene；提供 Rank、Achieve 和 Menu 入口 |
| GameScene | 根据选中 PackId 加载 `CardBagNNN` Prefab；按照 `PieceNN` 数字命名组织拼图分组；一组完成后切换分组并清理上一组碎片；全部完成后显示 RewardPanel |
| RankScene | 仅占位；首个 Demo 不包含排行榜功能 |
| AchieveScene | 当前显示 20 条模拟成就，前 5 条已达成、后 15 条未达成；接入 Steam 后替换数据源 |
| effect | 预览和调试 CardFx |

### 数据与奖励需求

- 任务配置来自 `Resources/Configs/TaskConfig.csv`。
- 卡包配置来自 `Resources/Configs/CardPacks.csv`。
- 累计积分任务类型为 `AccumulateScore`（`TaskType=1`）；每次完成拼图只累计一次当局结算分数。
- 结算以卡包基础分开始（XS 60、S 80、M 100、L 120、XL 140、XXL 160、XXXL 200），将所有符合条件的百分比加成相加后统一相乘，并向上取整。
- 分数加成为：未点击 `BtnTips` +5%；关闭 MainScene `Toggle1` 关卡描边 +2%；关闭 `Toggle2` 贴纸描边 +5%；完成时间 <=15 / <=30 / <=60 秒分别 +3% / +2% / +1%。
- 完成任务后发放奖励并推进到下一个任务。
- 完成任务必定创建一条持久化的新卡包权益。章节持有数量门槛关闭时，奖励保持待发并稍后重试。卡包首次完成时执行一次确定性的阶段门槛发包尝试；重玩已经 `Completed` 的卡包不执行该尝试，但仍可能通过完成任务创建任务权益。
- 卡包发放使用 8 个玩家不可见的内部章节，总量约 150 个卡包，平均每章 18.75 个。章节限制可选的锁定卡包奖励池，但不显示在 MainScene 或其他玩家界面。准确 PackId 分配和章节推进规则仍待确认。
- 内部章节阶段使用 `R` 表示当前章节仍为 `Locked` 的卡包数：初期 `17..9`、中期后段 `8..3`、末期 `2..1`。持有可玩数量为 `Unlocked + InProgress`，各阶段目标约为 `5-6`、`2-3` 和 `1`。章节超过 18 个卡包时，`R>17` 的额外范围同样属于初期。
- 当前发包门槛：`R>=9` 时允许 `H<=5`；`R=8` 时允许 `H<=3`；`R=7..3` 时允许 `H<=2`；`R=2..1` 时允许 `H<=1`。被拦截的首次完成发包直接跳过；被拦截的任务奖励保持待发。两个来源可在同一轮结算中同时发包。RewardPanel 保留默认 `ImgBag` Sprite；点击 `BtnFinish` 后，本次发放的全部卡包从 `ImgBag` 飞到屏幕居中行，停顿后跨越 MainScene 加载，再分别飞到对应列表位置。
- 累计积分任务推进到另一个累计积分任务时，超过已完成目标的进度向后结转（`nextProgress = currentProgress - completedTarget`）。
- 卡包生命周期保存在 SQLite `CardPacks` 表中，状态为 `Locked`、`Unlocked`、`InProgress` 或 `Completed`。
- MainScene 卡包排序：上次列表展示后新发放的卡包优先展示一次，且最新发放的在前；随后依次为 `InProgress`、按解锁时间升序的 `Unlocked`、按首次完成时间升序的 `Completed`。PackId 是确定性并列排序依据；每日挑战优先级暂缓实现。
- MainScene 对 `Completed` 卡包封面和尺寸图标进行轻微置灰，但保持可重玩。
- 任务进度保存在 JSON 根对象 `TaskProgressData`。
- 业务进度不得使用 `PlayerPrefs`。

### 内容扩展需求

- 新卡包沿用唯一 `Package001` 模板；`MainScene` 在运行时动态创建列表项。
- 新拼图通过在 `Resources/CardBagPrefabs/` 下新增 `CardBagNNN` Prefab 实现；每个 Prefab 包含 `GameBoard` 和 `Piece01`...`PieceNN`，不创建 Package JSON。
- 通用 3D 开包模型和 CardFx 资源放在 `Resources/Effects/` 下，通过 `Resources.Load` 加载。
- 构建前执行 `Puffies -> Sync Build Resources`，将运行时磁盘加载的 UI 目录同步到 `StreamingAssets/UI`。

### 待完成需求

- 正式排行榜页面内容。
- Steam 成就接入，用真实数据替换 AchieveScene 模拟数据。
- 逐项显示符合条件的加成并滚动累计阶段分数；当前运行时只执行一次从 0 到最终分数的滚动。
- 最终章节 PackId 分配、章节推进规则、空卡包池处理和最终卡包选择策略。
- 卡包生命周期和发放、奖励飞行、列表排序和分页、碎片托盘固定位置、分阶段描边的完整 Play Mode 回归。
- 正式构建回归。
- 曾讨论让棋盘滑动到槽位中心，但未合并；仍需要时应作为独立小任务实现。

---

## 2. 目录与加载策略

```text
Assets/
  Scenes/           LoadingScene（启动）、MainScene、GameScene、RankScene、AchieveScene、effect
  UI/               2D 源贴图（PackImages、CardBags/CardBagNNN、BasicUI...）
  Scripts/          MVC
    Model/          有意保持扁平：核心、配置、持久化、任务/卡包数据和运行时工具
    View/           PackageInteractionHandler
    Controller/     场景脚本
    Editor/         构建同步、Canvas 分辨率、中文字体、CardFx 预览
  Resources/
    Configs/        TaskConfig.csv、CardPacks.csv
    Effects/
      CardPack/     3D 卡包
      CardPackDismantle/ 美术制作的五层卡包拆包粒子特效
      PlaneGroup/
      CardFx/       卡包获得/轨迹 Prefab 及 Materials/Textures/Meshes/Shaders
    CardBagPrefabs/ GameScene 加载的 CardBagNNN 游戏 Prefab
  Prefabs/          共享 UI Prefab
  StreamingAssets/  UI 构建同步输出
  Plugins/SQLite/   sqlite-net
```

| 阶段 | 2D UI | 3D / FX |
|------|-------|---------|
| Editor | `Assets/UI`，场景 Image 直接引用 | `Assets/Resources/Effects` |
| Build | `StreamingAssets/UI` 中运行时磁盘加载的目录（`ToDiskPath`） | `Resources.Load("Effects/...")` |

- 不要重命名 `Resources`；代码中存在硬编码资源路径。
- GameScene 根据选中 PackId 动态加载 `Resources/CardBagPrefabs/CardBagNNN.prefab`。源贴图位于 `UI/CardBags/CardBagNNN/`，通过 Prefab 的 Sprite 引用进入构建，不放入 StreamingAssets。
- 3D 特效保留在 `Resources/Effects/`，不要复制到 StreamingAssets。
- 导入的卡包拆包特效为 `Resources/Effects/CardPackDismantle/CardPackDismantle_001.prefab`，包含五个原始 ParticleSystem 层，目前未接入 MainScene。两个旧 Shader Forge Pass 为 Renderer2D 使用 `SRPDefaultUnlit`，不可用的自定义材质 Inspector 已移除，原始粒子层级未改变。
- 编辑器组合预览为 `Resources/Effects/CardPackDismantle/CardPackDismantlePreview.prefab`，将六个嵌套 `CardPackOpening` 动画层与嵌套 `CardPackDismantle_001` 粒子组合，并通过 Renderer PropertyBlock 应用 `PackIcon001`。使用 **Puffies -> Effects -> Preview Card Pack Dismantle**（`Ctrl+Shift+D`）打开；专用 SceneView 会同步循环六个 Animator 和五层粒子结构。

---

## 3. 场景与导航

```text
LoadingScene（2.5s，TextLoading 0% -> 100%）
  -> MainScene
      -> BtnRank     -> RankScene     -> BtnReturn -> Main
      -> BtnAchieve  -> AchieveScene  -> CloseBtn  -> Main
      -> BtnMenu     -> PanelMenu     -> BtnClose / BtnReturn -> 关闭菜单
                    -> BtnSet        -> PanelSet -> BtnClose / BtnReturn -> 关闭设置
                    -> BtnUsable     -> PanelUsable -> BtnClose / BtnReturn -> 关闭辅助选项
                    -> BtnData       -> PanelSave -> BtnClose / BtnReturn -> 关闭存档面板
      -> 已解锁卡包运行时列表项 -> 开包动画 -> GameScene
          -> BtnReturn -> Main
          -> RewardPanel / BtnFinish -> Main
effect（调试）：CardFx 预览；菜单 Puffies -> Preview CardFx Effects
```

| 场景 | 脚本 | 说明 |
|------|------|------|
| LoadingScene | `LoadingScene.cs` | 初始化 JSON / SQLite / `GameTaskUtility` / `CardPackDataUtility` |
| MainScene | `MainScene.cs` | 卡包 UI；按解锁状态刷新；3D 开包或 2D 回退 |
| GameScene | `GameScene.cs` | 拼图分组和 RewardPanel；保存卡包、累计结算积分任务进度并结算任务奖励 |
| RankScene / AchieveScene | 场景脚本 | 返回 Main |
| effect | `CardFxPreviewScene.cs` | CardObtain / CardTrail 预览 |

**Build Settings**：`LoadingScene` 必须为 Index **0**。

| 对象名称 | 用途 |
|---------|------|
| `BtnRank` / `BtnAchieve` | Main -> Rank / Achieve |
| `BtnMenu` | MainScene 打开 `PanelMenu` |
| `PanelMenu` | MainScene 菜单弹窗，启动时隐藏 |
| `PanelMenu/BtnClose` | 关闭 MainScene 菜单 |
| `PanelMenu/BtnSet` | 打开 MainScene `PanelSet` 设置弹窗 |
| `PanelMenu/BtnUsable` | 打开 MainScene `PanelUsable` 辅助选项弹窗 |
| `PanelMenu/BtnData` | 打开 MainScene `PanelSave` 弹窗 |
| `PanelSet` | 音乐、音效和窗口模式设置弹窗 |
| `PanelSet/SliderMusic` | 音乐音量 |
| `PanelSet/SliderEffect` | 音效音量 |
| `PanelSet/ToggleFrame` | 窗口模式 |
| `PanelSet/BtnClose` / `PanelSet/BtnReturn` | 关闭设置弹窗 |
| `PanelUsable` | MainScene 辅助选项弹窗 |
| `PanelUsable/Toggle1` / `Toggle2` / `Toggle3` | 持久化辅助选项开关 |
| `PanelUsable/BtnClose` / `PanelUsable/BtnReturn` | 关闭辅助选项弹窗 |
| `PanelSave` | MainScene 存档/数据弹窗，目前仅显示和隐藏 |
| `PanelSave/BtnClose` / `PanelSave/BtnReturn` | 关闭存档弹窗 |
| `BtnReturn` | Rank / Game -> Main；位于 `PanelMenu` 时关闭 MainScene 菜单 |
| `CloseBtn` | Achieve -> Main |
| `BtnFinish` | 将新发卡包从 RewardPanel 动画移动到 MainScene 列表位置，然后完成返回 |
| `TextLoading` | 加载进度文字，支持 TextMeshPro `TMP_Text` 和旧 `UnityEngine.UI.Text` |
| `CardBagNNN` | 从 `Resources/CardBagPrefabs/` 加载的运行时游戏 Prefab |
| `GameBoard` / `Piece01`... | `CardBagNNN` Prefab 内的棋盘和槽位 |
| `ActiveGroupOutline` | `GameBoard` 下运行时显示的烘焙描边 UGUI Image |
| `PieceBoard` | 拼图碎片托盘 |
| `RewardPanel` | 拼图完成奖励面板 |
| `TaskItem` | MainScene 任务进度和 GameScene RewardPanel 结算共用的 `Assets/Prefabs/TaskItem.prefab` 实例 |
| `Package001` | MainScene 卡包列表项模板，隐藏并在运行时克隆 |
| `PackItem/PackCover` | MainScene 卡包封面 Image；运行时设置 `PackIconNNN.png` |
| `PackItem/PackSize` | 卡包尺寸图标；运行时根据 `CardPackSize` 配置选择 `PackSize_1.png` 到 `PackSize_7.png` |

---

## 4. 设计分辨率与字体

| 项目 | 值 |
|------|----|
| 设计分辨率 | **2560 x 1440** |
| PPU | 100（`GameDefine.PixelsPerUnit`） |

| 菜单 | 用途 |
|------|------|
| **Puffies -> Canvas -> Apply Design Resolution** | 批量应用 2560 x 1440 |
| **Puffies -> Fonts -> Setup Default Chinese Font** | Noto Sans SC TMP + UI Text |

新的 `CanvasScaler` 值由 `CanvasDesignResolutionEditor.cs` 写入。代码中使用 `GameFontUtility`，不要硬编码字体路径。

---

## 5. 数据与配置

| 数据 | 来源 | 运行时持久化 |
|------|------|-------------|
| 任务配置 | `GameConfigRepository` 读取 `Resources/Configs/TaskConfig.csv` | 只读 |
| 任务进度 | `GameTaskUtility` | `persistentDataPath/LocalData.json` 根对象 `TaskProgressData` |
| 卡包配置（`PackId`、`PackSize`、`ChapterId`） | `GameConfigRepository` 读取 `Resources/Configs/CardPacks.csv` | 只读 |
| 卡包生命周期 | `CardPackDataUtility` | `LocalData.db` 的 `CardPacks` 表 |
| 通用集合与键值存储 | `SqliteLocalStore` API | `LocalData.db` 的 `AppRecords` 表 |

- `GameConfigRepository` 加载并缓存任务和卡包配置。当前数据源为 `ResourcesGameConfigTextSource`，优先使用 `Resources.Load<TextAsset>`，失败时回退到编辑器磁盘路径。
- `CsvTable` 是统一 CSV 解析器，支持表头访问、引号字段和空行过滤；业务代码不得直接 `Split(',')`。
- `JsonLocalStore` 读写整个文件的单一根对象，目前用于任务进度。
- `SqliteLocalStore` 在 `AppRecords` 中使用集合/键记录；卡包业务状态使用专用 `CardPacks` 表。
- `CardPackLifecycleState` 为 `Locked=0`、`Unlocked=1`、`InProgress=2`、`Completed=3`。完成多组卡包第一组后标记为 `InProgress`，完成最后一组后标记为 `Completed`。
- SQLite `CardPacks` 表包含 `PackId`、`PackSize`、`LifecycleState`、`UnlockTime` 和 `CompletionTime`，不保留旧 `IsUnlocked`、`IsPlayed` 字段。解锁和完成时间使用固定格式的本地时间 `yyyy-MM-dd HH:mm:ss.fff`。`CompletionTime` 仅在首次进入 `Completed` 时写入，重玩不修改。
- `CardPackDistributionUtility` 与 `CardPackDataUtility` 放在一起，负责章节选择、`R` / 持有数量判断、确定性锁定候选选择和首次完成发包。重玩根据 GameScene 启动时记录的生命周期快照跳过该尝试。
- 待发任务卡包权益保存在 SQLite `AppRecords` 的 `CardPackDistribution/Progress` 下，并按 TaskId 去重。
- GameScene 在推进任务前先持久化任务权益，且仅在任务推进保存成功后尝试发放，避免任务进度保存失败时重复发包。
- MainScene 设置以集合/键 `GameSettings/Runtime` 保存在 `AppRecords`：音乐音量、音效音量和窗口模式。
- MainScene 辅助选项开关同样保存在 `GameSettings/Runtime`，字段为 `UsableOption1`、`UsableOption2` 和 `UsableOption3`。
- `UsableOption1` 是关卡外框开关，新建设置时默认开启；`UsableOption2` 是贴纸完整轮廓开关；`UsableOption3` 是高对比度。已持久化的用户选择优先。
- MainScene 和 GameScene 引用相同 `TaskItem.prefab` GUID。场景 Override 只定位根节点（`MainScene`：`10,508`；`GameScene`：`-6,455`）；子节点布局和视觉必须在共享 Prefab 中修改。
- 共享 TaskItem 子节点名称为 `TaskContent`、`TextProgress`、`ProgressMask`、`BagIcon` 和 `BagBg`。任务 UI 绑定代码应相对 TaskItem 实例解析这些名称，不得使用场景专属后缀。
- `TaskProgressUIUtility` 是两个 TaskItem 实例共用的运行时绑定。`TextProgress` 显示 `CurrentCompleteValue / TaskConfig.CompleteValue`，可见 `ProgressMask` 宽度使用两者比值并限制在有效范围。
- MainScene 在 `Start` 时从持久化任务数据刷新 TaskItem。GameScene 结算使用不受 TimeScale 影响的时间，在 0.8 秒内同步滚动 `TaskScore`、`TextProgress` 和 `ProgressMask`；任务奖励和推进在动画前持久化。
- GameScene 结算摘要将 `TaskBg2/TaskScore` 绑定到当局结算分数，将 `TaskBg2/TaskBagNum` 绑定到 SQLite 当前已解锁卡包数；本次任务奖励解锁的卡包立即计入。
- GameScene 进入时记录描边设置快照，点击 `BtnTips` 时记录提示使用，首个 Piece 成功放置时开始不受 TimeScale 影响的积分计时，RewardPanel 结算开始时冻结。
- MainScene `PanelSet/SliderMusic` 和 `PanelSet/SliderEffect` 是手工拼装的仿 Slider：根 Image 背景加 `SliderFill`、`SliderHandle` 子节点。运行时使用 `FakeSettingsSliderInput` 处理指针拖动、刷新视觉并保存数值。
- 不使用 `PlayerPrefs`。
- `LoadingScene.Start` 初始化 `JsonLocalStore`、`SqliteLocalStore`、`GameTaskUtility` 和 `CardPackDataUtility`。
- `Assets/Scripts/Model` 有意保持单层扁平目录。相关纯 C# 类型按以下方式合并：`GameManager` 位于 `GameDefine.cs`，CSV 解析类型位于 `GameConfigRepository.cs`，`JsonLocalStore` 和 `SqliteLocalStore` 位于 `LocalDataStore.cs`，积分类型和 `GameScoreUtility` 位于 `GameTaskUtility.cs`。公开类型名和调用点保持不变。
- MainScene 开包时创建运行时根节点 `CardPackOpeningFull`，并实例化全部六个原始蒙皮层：`CardPackOpening.prefab` 及 `CardPackOpening_002` 到 `006`。六个 Animator 同时启动共享 `CardPackOpening` 状态。`GameAnimationUtility` 通过 `MaterialPropertyBlock` 将选中列表项的完整 `PackIconNNN` Sprite 贴图矩形应用到每一层，测量组合动画第零帧蒙皮边界，在启用 Renderer 前将完整特效等比适配到点击的 UI 边界。共享材质不被修改。URP Shader 在正面渲染选中封面、背面渲染原始卡背，并使用 `CardPackClipMask.png` 保留波浪形边缘；同时应用美术提供的正反面法线、HDR 环境反射、光照渐变、金属度/光滑度和 AO。选中封面颜色保持为未改变的基础色，渐变和反射贡献已中和，不会为整张卡包染色。项目使用 URP `Renderer2D`，因此 Mesh Shader Pass 必须使用 `LightMode=SRPDefaultUnlit` 或 `Universal2D`；交付的 Built-in/Amplify Surface Shader 必须移植，不能直接导入。资源包中的静态 `CardPackStatic_001`...`006`、`CardPackPlane` 和 `PlaneGroup_001` 是支持或参考资源，不是运行时开包层；Plane 资源没有动画且使用固定示例图，会遮挡动态卡包。
- MainScene 空闲卡包使用六个开包层第零帧烘焙的一份共享 Mesh。每个可见卡包只使用一个带真实封面和生命周期颜色的轻量 Renderer，在 `2.4s` 内进行 `0.98` 到 `1.02` 呼吸缩放，跟随 `PackCover` 锚点，并裁剪到卡包 ScrollRect 视口。点击后使用可复用六层开包器替换空闲 Renderer，在 `0.3s` 内放大到原始 `600 x 680` 设计尺寸，再播放开包动画并进入 GameScene。编辑器配置的 `PackSize` Image 继续作为位置、Sprite、颜色和前景视觉依据。

### 开发期持久化策略

- 开发阶段的本地持久化不保证向后兼容。数据结构和 SQLite 字段类型可直接改为当前需求，不增加迁移或旧数据回退，除非用户明确要求。
- SQLite 表结构发生不兼容修改后，关闭 Unity，并在测试前删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`。
- JSON 任务进度或跨两个存储的行为发生变化时，同时删除 `LocalData.json`。每次不兼容修改后，助手必须指出需要删除的文件；未经明确要求不得自动删除。

---

## 6. 添加内容

### 卡包

`MainScene.RefreshPackageList` 根据数据库动态创建已解锁卡包槽位。不要在场景中手工复制 `Package002`、`Package003` 等对象。

共享尺寸图标为 `UI/PackImages/PackSize_1.png` 到 `PackSize_7.png`，对应 `CardPackSize` 数值（`XS=1` 到 `XXXL=7`）。`PackItem` 必须包含名为 `PackCover` 和 `PackSize` 的 Image 子节点；MainScene 在运行时设置两者 Sprite，并根据编辑器封面尺寸缩放尺寸图标。

`PackItem/PackShadow` 是渲染在 `PackCover` 后方的同级 Image。MainScene 读取运行时可读封面贴图，将 Alpha 缩小到 `240 x 272` 显示尺寸，并执行三次可分离 Box Blur，水平半径 2、垂直半径 5。缓存阴影 Sprite 尺寸为 `256 x 344`、偏移为 `(0,-20)`，使投影只向下而不是向右。水平/垂直内边距为 `8/36` 像素，阴影颜色 `#1f292d`，最大 Alpha `0.52`。MainScene 销毁时释放生成的阴影 Sprite 和 Texture。`PackSize` 保持在两张图片上方。

1. 场景中只保留一个模板对象：`Package001`。
2. 在 `CardPacks.csv` 增加一行（`PackId`、`PackSize`、`ChapterId`）。
3. 在 `UI/PackImages/` 下按 `PackIconNNN.png` 命名增加对应封面。`GameDefine.FormatPackImagePath` 将 PackId `1` 映射到 `UI/PackImages/PackIcon001.png`。
4. 通过 `CardPackDataUtility` 将生命周期写入 SQLite `CardPacks` 表。
5. 不创建每个卡包专属的 3D 资源。运行时复用共享动画、Controller、材质和全部六个 `CardPackOpening` 蒙皮层；选中的 `PackIconNNN.png` 成为每个动画层的封面。共享资源缺失时，MainScene 使用 2D 回退。

### 拼图

1. 在 `Assets/Resources/CardBagPrefabs/` 下创建 `CardBagNNN` Prefab，`NNN` 与 `PackId` 一致。
2. Prefab 内放置一个名为 `GameBoard` 的子对象。
3. 在 `GameBoard` 下用 Image 对象添加分组碎片：第 1 组使用 `Piece11`、`Piece12`...；第 2 组使用 `Piece21`、`Piece22`...；第 3 组使用 `Piece31`...。分组号为 `PieceNN / 10`，按升序处理。
4. 源贴图放在 `Assets/UI/CardBags/CardBagNNN/`，按分组命名，例如 `Pieces11`...`Pieces14` 和 `Pieces21`...`Pieces25`。
5. 不使用 `PieceGroup` 父节点；分组只读取 `Piece` 后面的数字。
6. 不创建 Package JSON；运行时数据来自已加载 Prefab 的 Image。
7. 新增或修改 CardBag 后，执行 **Puffies -> Puzzles -> Bake Outline Masks**。烘焙器在 GameBoard 坐标中合并 Piece Alpha、闭合窄间隙，并写入 `Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`。第 1 组只包含自身最终拼图外边界；后续每张图只包含当前组最终外边界及其与低编号已完成组的接触边。
8. `GameScene` 将烘焙的 `#3f423e` 当前组 Sprite 作为不可交互的 `GameBoard` 子 Image 显示。蒙版排除已完成组的无关边界、当前组与未来组的边界以及同组各 Piece 之间的接缝。不要在 Prefab 中手工制作描边对象。
9. 缺少生成 Sprite 时，运行时记录制作警告，并在无描边情况下继续游戏。交付前重新运行烘焙器。
- 创建一组碎片时只定位一次。成功放置 Piece 后，托盘中其他 Piece 保持既定 X、Y 位置；空位直到下一组创建时才重新布局。

### 拼图描边渲染

- 拼图描边由 `PuzzleOutlineBakerEditor` 离线生成，并通过 Unity UGUI `Image` 渲染。
- 项目没有运行时描边 Shader、Renderer Feature 或第三方描边包。
- 描边加载与拼图交互保持隔离；缺少描边不得阻止可拖拽碎片创建。

### CardFx

Prefab 和依赖放在 `Resources/Effects/CardFx/`，例如 `CardObtain_001` 和 `CardTrail_001`。

---

## 7. 命名

| 类型 | 名称 | 路径 |
|------|------|------|
| 卡包动画层 | `CardPackOpening`、`CardPackOpening_002`...`006` | `Resources/Effects/CardPack/` |
| 通用卡包 Controller | `CardPackOpening.controller` | 同上 |
| 通用卡包动画 | `CardPackOpeningAnimation.FBX` | 同上 |
| 卡包动画模型 | `CardPackOpeningModel.FBX`、`CardPackOpeningModel_002`...`006` | 同上 |
| 静态卡包参考 | `CardPackStaticModel.FBX`、`CardPackStatic_001`...`006` | 同上 |
| 静态卡包示例 | `CardPackPlane` | 同上 |
| 默认卡包封面 | `CardPackDefaultCover.png` | 同上 |
| 材质 | `CardPackOpeningMaterial` | 同上 |
| URP Shader | `CardPackOpening.shader` | 同上 |
| 正面法线 | `CardPackFrontNormal.png` | 同上 |
| HDR 反射 | `CardPackReflection.hdr` | 同上 |
| 光照渐变 | `CardPackLightingRamp.png` | 同上 |
| AO 贴图 | `CardPackOcclusion.png` | 同上 |
| 背面贴图 | `CardPackBack.png` | 同上 |
| 波浪裁切蒙版 | `CardPackClipMask.png` | 同上 |
| Plane 组 | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| 新卡包获得 | `CardObtain_001` | `Resources/Effects/CardFx/` |
| 卡包轨迹 | `CardTrail_001` | 同上 |
| 卡包拆包特效 | `CardPackDismantle_001` | `Resources/Effects/CardPackDismantle/` |

---

## 8. 构建

构建前执行 **Puffies -> Sync Build Resources**。该命令将 `PackImages`、`BasicUI`、`AchieveScene` 和 `RankScene` 复制到 `StreamingAssets/UI`；CardBag 源贴图通过游戏 Prefab 的 Sprite 引用进入构建，因此不复制。

建议 Build Settings 顺序：LoadingScene -> MainScene -> GameScene -> effect -> RankScene -> AchieveScene。

### 开发工作站

- 必需命令行 SDK：.NET 8 SDK，不固定具体补丁版本。
- 必需 VS Code 扩展：C#（`ms-dotnettools.csharp`）、C# Dev Kit（`ms-dotnettools.csdevkit`）和 Microsoft Unity（`visualstudiotoolsforunity.vstuc`）。
- `.vscode/extensions.json` 提供编辑器推荐；扩展程序和 .NET SDK 需要在每台设备上单独安装。
- 在新设备上，Codex 应先检查这些前置条件；缺失时请求安装授权，然后再排查 Unity C# 项目加载错误。
- `Assembly-CSharp*.csproj` 由 Unity 生成，不得为了兼容 VS Code 手工转换或修改。

---

## 9. 编辑器菜单参考

| 菜单 | 用途 |
|------|------|
| Puffies -> Sync Build Resources | 将运行时磁盘加载的 UI 目录复制到 StreamingAssets |
| Puffies -> Canvas -> Apply Design Resolution | 应用 Canvas 设计分辨率 |
| Puffies -> Fonts -> Setup Default Chinese Font | 设置中文字体 |
| Puffies -> Preview CardFx Effects | 打开特效场景 |
| Puffies -> Puzzles -> Bake Outline Masks | 为每个 CardBag Prefab 重建各分组外边界描边 |

---

## 10. 已弃用

- `Assets/ArtRes/`、`Assets/Configs/`
- `Resources/Config/Package001.json` 及 JSON 拼图配置流程
- `Tools/*.ps1` 下的一次性迁移脚本
