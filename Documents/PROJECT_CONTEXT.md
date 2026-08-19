# 项目上下文

Unity **2022.3** / Built-in Render Pipeline 项目，使用 Linear 色彩空间和 Built-in Forward 渲染。核心循环：打开卡包 -> 拖放拼图 -> 任务奖励。本文档是需求、场景、数据、资源、构建规则和命名的稳定项目参考。

`GraphicsSettings.m_CustomRenderPipeline` 必须为空，各 Quality 档位不指定 SRP Asset。URP `14.0.12` Package 与 `Assets/Settings` 中的旧 URP Asset 暂时保留为迁移回退资源，但不是当前激活管线；新增运行时代码和 Shader 不得依赖 URP API。

当前工作状态记录在 [CURRENT_TASK.md](CURRENT_TASK.md)，工作流规则记录在 [WORKFLOW.md](WORKFLOW.md)，已确认的长期游戏设计规则记录在 [GAME_DESIGN_REQUIREMENTS.md](GAME_DESIGN_REQUIREMENTS.md)。

---

## 1. 功能需求

### 核心循环

1. `LoadingScene` 初始化本地数据、任务配置、卡包配置和持久化存储。
2. `MainScene` 根据解锁状态显示可玩的卡包。
3. 点击已解锁卡包进入选择页；确认后进入开包舞台，再次轻点卡包或横划后进入 `GameScene`。
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
| MainScene | 根据 `CardPacks.csv` 与 SQLite 解锁状态刷新卡包列表；正式列表固定使用 `PackageScrollView/Content/Page_1` 与 `PackItem` 分页结构，不再兼容旧 `Package001` 单图列表；每页按 6 列 x 3 行显示 18 个静态卡包封面。承载首页主 UI 的 Canvas 使用 `Screen Space - Camera` 并绑定场景 `Main Camera`，因此 `PackItem/PackCover`、`PackHighlight`、`PackSize` 与首页主 UI 统一经过该摄像机；运行时 `MainScene` 会重新校正绑定、`2560 x 1440` 设计分辨率、`Match=0.5` 和 `PPU=100`。`PackItem.prefab` 保留 `PackCover`、`PackHighlight` 和 `PackSize`；`PackCover` 使用编辑器可调的 `PackCoverShadow.mat` 在同一个 Graphic 中合成封面投影，运行时只替换为对应 `PackIconNNN.png`。首页卡包常驻播放 `0.98 ↔ 1.02 / 2.4s` 的整体呼吸，并显示 Prefab 内配置的 ADD 高光；列表不创建 3D 模型、粒子或 Animator。点击后通过同一 `Main Camera` 渲染的独立顶层 Canvas 使用同一静态封面，从列表实际位置等比放大到屏幕中心 `600 x 680`，并显示 `PanelBagSelect` 和既有背景虚化；Back 将静态封面返回原列表位置。点击玩/确认重玩后进入 `BgGame` 开包舞台，等待玩家再次轻点卡包或横划；有效操作随机加载一套 `CardPackOpeningModel_001-006`，使用当前 `PackIconNNN` 作为正面纹理，播放制作方骨骼撕裂动画及 `fx_chai_w_001` 横向光效，完成后进入 `GameScene`。3D 模型、撕口粒子、开包背景和 MainScene 最终画面统一由 `Main Camera` 直接渲染，不再经过独立特效相机、透明 RenderTexture 和 RawImage 合成。保留拍照、重玩确认、Rank、Achieve 和 Menu 入口。 |
| GameScene | 根据选中 PackId 加载 `CardBagNNN` Prefab，并读取 `CardPacks.csv/BoardScale` 缩放棋盘；按照 `PieceGGII` 四位数字命名组织拼图分组；从正常开包流程进入时播放棋盘、托盘和当前组 Piece 入场；每次正确放置 Piece 后立即持久化，重新进入时恢复已放置 Piece 并从首个未完成分组继续；全部完成后显示 RewardPanel；Editor 和 Development Build 在 `BtnTips` 左侧提供“一键完成”测试按钮 |
| RankScene | 仅占位；首个 Demo 不包含排行榜后端功能。当前模拟列表前三名的 `RankBg` 分别使用原生 `1646 x 148` 的 `RankCellBg_1.png`、`RankCellBg_2.png`、`RankCellBg_3.png`，第四名以后使用 `1636 x 136` 的 `RankCellBg.png`；`RankItem` 根高度为 `148`，列表纵向间距为 `5`，条目中心步距为 `153` |
| AchieveScene | 当前显示 20 条模拟成就，前 5 条已达成、后 15 条未达成；接入 Steam 后替换数据源。成就网格固定为 6 列，单元尺寸 `240 x 332`，横纵间距均为 `40` |

所有场景常规鼠标图标为 `UI/BasicUI/ImgHand_1.png`。GameScene 悬停当前可拖 Piece 时切换 `ImgHand_2.png`，按住左键拖拽 Piece 时切换 `ImgHand_3.png`；松开、结算或离开 GameScene 后恢复常规图标。三张资源随 `BasicUI` 同步到 Player 的 `StreamingAssets/UI/BasicUI`。运行时使用 `CursorMode.ForceSoftware`，以 `2560x1440` 设计分辨率和 CanvasScaler `Match=0.5` 计算统一缩放系数，分别等比重建三张光标纹理并同步缩放热点；窗口尺寸变化时自动刷新，不能把三种不同比例的源图压入固定画布。

### 数据与奖励需求

- 任务配置来自 `Resources/Configs/TaskConfig.csv`。
- 卡包配置来自 `Resources/Configs/CardPacks.csv`。
- `CardPacks.csv/StickerCount` 紧跟在 `PackSize` 后面，记录卡包贴纸数量。`PackSize` 按该数量确定：`<20=XS`、`20..30=S`、`31..55=M`、`56..85=L`、`86..125=XL`、`126..170=XXL`、`>170=XXXL`。配置更新工具同时将 `BoardScale` 更新为：`XS=0.75`、`S=0.78`、`M=1.10`、`L=1.30`、`XL=1.00`、`XXL=1.15`、`XXXL=1.30`。工具只统计 `Assets/UI/CardBags/CardBagNNN` 顶层的标准碎片名 `piece_NNN.png`，不统计 `BoardTitle.png`、`GameBoard.png` 或其他 PNG。
- `CardPacks.csv` 的字符串列 `Series` 位于 `BoardScale` 与 `AutoUpdate` 之间，默认留空并且只能手工维护。某行填写 `15|18` 时，以该行 `PackId` 为链首建立 `当前包 -> 15 -> 18`；后续包只有在完整前置链都为 `Completed` 后才进入现有发包候选池，仍继续受章节、持有数量和其他常规发包规则限制。系列同时限制任务奖励、首次完成奖励和直接解锁 API，不会自动发包。不存在的 PackId、默认首包作为后续包、冲突前置、重复或循环链会使卡包配置加载失败。
- `CardPacks.csv` 最后一列 `AutoUpdate` 只允许 `0` 或 `1`，默认值为 `1`。配置更新工具遇到空值会补为 `1`；设为 `0` 的配置行会保留手工填写的 `PackSize`、`StickerCount` 和 `BoardScale`，不进行自动更新。无论 `AutoUpdate` 取值如何，工具都不修改已有 `Series` 内容；缺少该列时只在 `AutoUpdate` 前补一列空值。
- 任务配置固定为三个模板：`TaskType=1` 完成任意拼图包并累计结算分数，`TaskType=2` 从任意拼图包中收集贴纸数量，`TaskType=3` 完成指定尺寸的卡包数量。三类任务都只在完整完成一个符合尺寸要求的卡包后结算一次；贴纸任务按该卡包的全部 Piece 数量累计。
- `SizeMode=0` 表示任意尺寸；`SizeMode=1` 从模板 `SizePool` 与玩家当前可玩卡包尺寸的交集中随机指定一个尺寸。三个模板按 `Weight` 加权随机，生成下一任务时严格排除与当前任务相同的 `TaskType`；任务 3没有可玩的 S/M 卡包时不进入候选池。
- 积分任务目标按 `150 -> 200 -> 250 -> 300` 顺序循环，贴纸任务目标按 `45 -> 60 -> 80` 顺序循环，两类任务使用互不影响的持久化游标。完成卡包任务的目标数量从 `2|3` 随机，指定尺寸从 `S|M` 随机，两项独立选择。
- 结算以卡包基础分开始（XS 60、S 80、M 100、L 120、XL 140、XXL 160、XXXL 200），将所有符合条件的百分比加成相加后统一相乘，并向上取整。
- 分数加成为：未点击 `BtnTips` +5%；关闭 MainScene `Toggle1` 关卡描边 +2%；关闭 `Toggle2` 贴纸描边 +5%；完成时间 <=15 / <=30 / <=60 秒分别 +3% / +2% / +1%。
- 完成任务后发放奖励并随机生成下一个任务实例。每个任务实例使用独立递增的 `TaskInstanceId`，同一个模板可以在后续再次出现。
- 完成任务必定创建一条持久化的新卡包权益。章节持有数量门槛关闭时，奖励保持待发并稍后重试。卡包首次完成时执行一次确定性的阶段门槛发包尝试；重玩已经 `Completed` 的卡包不执行该尝试，但仍可能通过完成任务创建任务权益。
- 卡包发放使用 8 个玩家不可见的内部章节，总量约 150 个卡包，平均每章 18.75 个。章节限制可选的锁定卡包奖励池，但不显示在 MainScene 或其他玩家界面。准确 PackId 分配和章节推进规则仍待确认。
- 内部章节阶段使用 `R` 表示当前章节仍为 `Locked` 的卡包数：初期 `17..9`、中期后段 `8..3`、末期 `2..1`。持有可玩数量为 `Unlocked + InProgress`，各阶段目标约为 `5-6`、`2-3` 和 `1`。章节超过 18 个卡包时，`R>17` 的额外范围同样属于初期。
- 当前发包门槛：`R>=9` 时允许 `H<=5`；`R=8` 时允许 `H<=3`；`R=7..3` 时允许 `H<=2`；`R=2..1` 时允许 `H<=1`。被拦截的首次完成发包直接跳过；被拦截的任务奖励保持待发。两个来源可在同一轮结算中同时发包。RewardPanel 保留默认 `ImgBag` Sprite；点击 `BtnFinish` 后，本次发放的全部卡包从 `ImgBag` 飞到屏幕居中行，停顿后跨越 MainScene 加载，再分别飞到对应列表位置。
- 积分任务超过目标的分数保存为待结转值；即使中间出现贴纸或完成卡包任务，也会在下一个积分任务生成时恢复为该任务的初始进度。
- 卡包生命周期保存在 SQLite `CardPacks` 表中，状态为 `Locked`、`Unlocked`、`InProgress` 或 `Completed`。
- 当前拼图会话保存在 SQLite `CardPackPuzzleProgress` 表中；记录存在表示该卡包有一局可继续，已正确放置的 Piece 编号即时保存，整包完成后删除记录。
- MainScene 卡包排序：上次列表展示后新发放的卡包优先展示一次，且最新发放的在前；随后依次为 `InProgress`、按解锁时间升序的 `Unlocked`、按首次完成时间升序的 `Completed`。PackId 是确定性并列排序依据；每日挑战优先级暂缓实现。
- MainScene 所有生命周期状态统一显示 `UI/PackImages/PackIconNNN.png` 静态封面，不执行颜色、材质或置灰处理。
- 任务实例、当前进度、下一个实例号、积分目标循环游标、贴纸目标循环游标和待结转积分保存在 JSON 根对象 `TaskProgressData`。
- 业务进度不得使用 `PlayerPrefs`。

### 内容扩展需求

- 新卡包沿用唯一 `Package001` 模板；`MainScene` 在运行时动态创建列表项。
- 新拼图通过在 `Resources/CardBagPrefabs/` 下新增 `CardBagNNN` Prefab 实现；每个已分组 Prefab 包含 `GameBoard` 和 `PieceGGII` 节点，不创建 Package JSON。
- 编辑器批量生成器可扫描 `CardBagNNN` 资源目录，使用完整的 `Previews/CardBagNNN.png` 与透明 Piece PNG 进行像素匹配，并以 `GameBoard.png` 作为运行时棋盘底图批量创建 Prefab，不依赖 Package JSON 或 `unity_layout.json`。生成器优先使用精确 RGB 锚点；切图与预览几何一致但存在导出色差时，回退到分阶段感知颜色匹配，并且只有最低相似度和远距离第二候选分差同时达标才接受，避免相似贴纸误定位。每个源目录除 `BoardTitle.png`、`GameBoard.png` 外的碎片统一命名为小写三位编号 `piece_001.png`、`piece_002.png`……；定位完成后自动按空间生成正式 `PieceGGII` 分组。已有 `.meta` 必须随 PNG 一起移动以保持 Prefab Sprite GUID 引用。
- 新卡包撕包特效位于 `Resources/Effects`：六套 `CardPackOpeningModel_001-006` 共用 `CardPackAnimation.controller`，`fx_chai_w_001` 提供撕口横向光效。列表和选中放大仍使用静态图，只有 `BgGame` 内收到轻点或有效横划后才加载并播放这些资源。
- 构建前执行 `Puffies -> Sync Build Resources`，将运行时磁盘加载的 UI 目录同步到 `StreamingAssets/UI`。

### 待完成需求

- 正式排行榜页面内容。
- Steam 成就接入，用真实数据替换 AchieveScene 模拟数据。
- 最终章节 PackId 分配、章节推进规则、空卡包池处理和最终卡包选择策略。
- 卡包生命周期和发放、奖励飞行、列表排序和分页、碎片托盘固定位置、分阶段描边的完整 Play Mode 回归。
- 正式构建回归。

---

## 2. 目录与加载策略

```text
Assets/
  Scenes/           LoadingScene（启动）、MainScene、GameScene、RankScene、AchieveScene
  UI/               2D 源贴图（PackImages、CardBags/CardBagNNN、BasicUI...）
  Scripts/          MVC
    Model/          有意保持扁平：核心、配置、持久化、任务/卡包数据和运行时工具
    View/           PackageInteractionHandler
    Controller/     场景脚本
    Editor/         构建同步、Canvas 分辨率、中文字体
  Resources/
    Configs/        TaskConfig.csv、CardPacks.csv
    CardBagPrefabs/ GameScene 加载的 CardBagNNN 游戏 Prefab
  Prefabs/          共享 UI Prefab
  StreamingAssets/  UI 构建同步输出
  Plugins/SQLite/   sqlite-net
```

| 阶段 | 2D UI |
|------|-------|
| Editor | `Assets/UI`，场景 Image 直接引用 |
| Build | `StreamingAssets/UI` 中运行时磁盘加载的目录（`ToDiskPath`） |

- 不要重命名 `Resources`；代码中存在硬编码资源路径。
- GameScene 根据选中 PackId 动态加载 `Resources/CardBagPrefabs/CardBagNNN.prefab`。源贴图位于 `UI/CardBags/CardBagNNN/`，通过 Prefab 的 Sprite 引用进入构建，不放入 StreamingAssets。
- `Assets/Resources/Effects/` 保存制作方的新撕包模型、Animator、材质、Shader 和粒子 Prefab；`Assets/Scenes/EffectScene001.unity` 是制作方效果与时间轴参考场景，不作为游戏入口。
- 当前卡包列表、选中放大与 `BgGame` 等待输入阶段只使用静态封面；收到开包输入后才创建 3D 撕裂模型和粒子资源。

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
      -> 已解锁卡包运行时列表项 -> 居中放大 + 半透明压暗背景 + PanelBagSelect
                                      -> BtnPlay/重玩 -> BgGame 开包舞台
                                                         -> 轻点卡包 / 横划 -> 真实卡包撕裂 + 横向光效 -> GameScene 入场
                                      -> BtnBack -> 卡包返回列表并关闭面板
          -> BtnReturn -> Main
          -> RewardPanel / BtnFinish -> Main
```

| 场景 | 脚本 | 说明 |
|------|------|------|
| LoadingScene | `LoadingScene.cs` | 初始化 JSON / SQLite / `GameTaskUtility` / `CardPackDataUtility` |
| MainScene | `MainScene.cs` | 卡包 UI；按解锁状态刷新；3D 开包或 2D 回退 |
| GameScene | `GameScene.cs` | 拼图分组和 RewardPanel；保存卡包、累计结算积分任务进度并结算任务奖励 |
| RankScene / AchieveScene | 场景脚本 | 返回 Main |

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
| `GameBoard` / `PieceGGII` | `CardBagNNN` Prefab 内的棋盘和槽位；`GG` 是两位组号，`II` 是两位组内索引 |
| `ActiveGroupOutline` | `GameBoard` 下运行时显示烘焙描边的根节点，按设置组合 `LevelOutline` 与 `StickerOutlines` UGUI Image |
| `PieceBoard` | 拼图碎片托盘 |
| `RewardPanel` | 拼图完成奖励面板 |
| `TaskItem` | MainScene 任务进度和 GameScene RewardPanel 结算共用的 `Assets/Prefabs/TaskItem.prefab` 实例 |
| `Package001` | MainScene 卡包列表项模板，隐藏并在运行时克隆 |
| `PackItem/PackCover` | MainScene 卡包封面 Image；运行时设置 `PackIconNNN.png` |
| `PackItem/PackHighlight` | 卡包封面上方的可调 ADD 高光父节点；四个子 Image 的位置、尺寸、颜色和透明度由 Prefab 配置，运行时只随封面整体缩放 |
| `PackItem/PackSize` | 卡包尺寸图标；运行时根据 `CardPackSize` 配置选择 `PackSize_1.png` 到 `PackSize_7.png` |

---

## 4. 设计分辨率与字体

| 项目 | 值 |
|------|----|
| 设计分辨率 | **2560 x 1440** |
| PPU | 100（`GameDefine.PixelsPerUnit`） |

| 菜单 | 用途 |
|------|------|
| **Puffies -> Apply Design Resolution** | 批量应用 2560 x 1440 |
| **Puffies -> Setup Default Chinese Font** | Noto Sans SC TMP + UI Text |

新的 `CanvasScaler` 值由 `CanvasDesignResolutionEditor.cs` 写入。代码中使用 `GameFontUtility`，不要硬编码字体路径。

---

## 5. 数据与配置

| 数据 | 来源 | 运行时持久化 |
|------|------|-------------|
| 任务配置 | `GameConfigRepository` 读取 `Resources/Configs/TaskConfig.csv` | 只读 |
| 任务进度 | `GameTaskUtility` | `persistentDataPath/LocalData.json` 根对象 `TaskProgressData` |
| 卡包配置（`PackId`、`PackSize`、`StickerCount`、`ChapterId`、`BoardScale`、`Series`、`AutoUpdate`） | `GameConfigRepository` 读取 `Resources/Configs/CardPacks.csv` | 只读 |
| 卡包生命周期 | `CardPackDataUtility` | `LocalData.db` 的 `CardPacks` 表 |
| 卡包当前拼图会话 | `CardPackDataUtility` | `LocalData.db` 的 `CardPackPuzzleProgress` 表 |
| 通用集合与键值存储 | `SqliteLocalStore` API | `LocalData.db` 的 `AppRecords` 表 |

- `GameConfigRepository` 加载并缓存任务和卡包配置。当前数据源为 `ResourcesGameConfigTextSource`，优先使用 `Resources.Load<TextAsset>`，失败时回退到编辑器磁盘路径。
- `CsvTable` 是统一 CSV 解析器，支持表头访问、引号字段和空行过滤；业务代码不得直接 `Split(',')`。
- `CardPacks.csv/BoardScale` 使用 invariant-culture 浮点数且必须大于零。GameScene 将其乘到当前 CardBag 根节点，使棋盘、槽位、描边和吸附坐标统一缩放。每个 Piece 的托盘生成比例为 `Min(配置后的棋盘目标比例, 原黑色托盘规则比例)`，不按整组宽高分支；拖拽目标比例使用当前 `SpriteRenderer` 屏幕包围盒与目标凹槽屏幕矩形直接校准，并在每次拿起时刷新，所以按下时只会保持尺寸或放大，正确落位切换为 Canvas `Image` 时不得再次缩放。未正确吸附的 Piece 保持棋盘目标比例，并只允许完整渲染边界落在棋盘内未被 Alpha 大于 0 的已拼 Piece 占用的位置、完整落在棋盘左右两侧的桌面，或在棋盘左右边界之间完整落入棋盘底边与托盘原始顶部之间的桌面空间；新增下方区域使用托盘缓存的原始屏幕顶部作为下边界，托盘收起后也不得侵入其原始高度。任何横跨棋盘边框、位于棋盘正上方、侵入托盘原始区域或与已拼内容实际轮廓重叠的位置均回弹。成功吸附后立即用 Prefab 对应原始 `Image` 替代拖拽 `SpriteRenderer`，确保已放置 Piece 与棋盘在同一 Canvas 层级共同缩放，接缝不随 `BoardScale` 放大。
- `JsonLocalStore` 读写整个文件的单一根对象，目前用于任务进度。
- `SqliteLocalStore` 在 `AppRecords` 中使用集合/键记录；卡包业务状态使用专用 `CardPacks` 表。
- `CardPackLifecycleState` 为 `Locked=0`、`Unlocked=1`、`InProgress=2`、`Completed=3`。首次进入 GameScene 时将未完成卡包标记为 `InProgress`，完成最后一组后标记为 `Completed`；重玩期间保持 `Completed`，不降级。
- SQLite `CardPacks` 表包含 `PackId`、`PackSize`、`LifecycleState`、`UnlockTime` 和 `CompletionTime`，不保留旧 `IsUnlocked`、`IsPlayed` 字段。解锁和完成时间使用固定格式的本地时间 `yyyy-MM-dd HH:mm:ss.fff`。`CompletionTime` 仅在首次进入 `Completed` 时写入，重玩不修改。
- SQLite `CardPackPuzzleProgress` 表包含 `PackId`、`PlacedPieceNumbersJson` 和 `UpdatedTime`。进入 GameScene 即创建会话，即使尚未放置 Piece 也保留空记录；正确吸附后按 `PieceGGII` 的 `组号 * 100 + 组内索引` 完整编号去重、排序并立即保存。桌面 Piece 的位置不持久化。完成整包并成功保存 `Completed` 后清除该会话。
- `CardPackDistributionUtility` 与 `CardPackDataUtility` 放在一起，负责章节选择、`R` / 持有数量判断、确定性锁定候选选择和首次完成发包。重玩根据 GameScene 启动时记录的生命周期快照跳过该尝试。
- 待发任务卡包权益保存在 SQLite `AppRecords` 的 `CardPackDistribution/Progress` 下，并按唯一 `TaskInstanceId` 去重。
- GameScene 在推进任务前先持久化任务权益，且仅在任务推进保存成功后尝试发放，避免任务进度保存失败时重复发包。
- MainScene 设置以集合/键 `GameSettings/Runtime` 保存在 `AppRecords`：音乐音量、音效音量和窗口模式。
- MainScene 辅助选项开关同样保存在 `GameSettings/Runtime`，字段为 `UsableOption1`、`UsableOption2` 和 `UsableOption3`。
- `UsableOption1` 是关卡描边开关，`UsableOption2` 是贴纸描边开关，两者新建设置时都默认关闭；`UsableOption3` 是高对比度并默认关闭。已持久化的用户选择优先。关卡描边关闭时 GameScene 保留现有当前阶段连接区域，打开时改为显示当前待拼组的完整合并外边界；贴纸描边关闭时不显示单块轮廓，打开时叠加当前组每块凹槽的独立轮廓。PanelUsable 的 `ImgContentBg` 按高对比度状态显示 `MainSetHigh1/2.png`；`ImgContentLine` 在描边全关、仅关卡描边、贴纸描边打开时分别显示 `MainSetLine1/2/3.png`，两项同时打开使用信息更完整的 `MainSetLine3.png`。GameScene 的 CardBag 根背景在高对比度关闭时使用 `UI/BasicUI/BgCardBoard1.png`，打开时使用 `BgCardBoard2.png`；运行时只替换根 `Image.sprite`，不改变 Prefab 布局。烘焙棋盘描边通过 Alpha-only UGUI Shader 固定输出 `#3f423e`，不随高对比度切换颜色；提示按钮的绿色滚动虚线在高对比度时改用 `#b1d702`，新手引导专用蓝色虚线不变。
- MainScene 和 GameScene 引用相同 `TaskItem.prefab` GUID。场景 Override 只定位根节点（`MainScene`：`10,508`；`GameScene`：`-6,455`）；子节点布局和视觉必须在共享 Prefab 中修改。
- 共享 TaskItem 子节点名称为 `TaskContent`、`TextProgress`、`ProgressMask`、`BagIcon` 和 `BagBg`。任务 UI 绑定代码应相对 TaskItem 实例解析这些名称，不得使用场景专属后缀。
- `TaskProgressUIUtility` 是两个 TaskItem 实例共用的运行时绑定。三类任务文案分别为“完成任意拼图包，收集 N 分”“从任意拼图包中收集 N 个贴纸”和“完成 N 个 S/M 尺寸的拼图包”；`TextProgress` 显示当前值与任务实例目标值，可见 `ProgressMask` 宽度使用两者比值并限制在有效范围。`BagIcon` 始终使用共享 Prefab 中配置的固定 Sprite，运行时不得按任务奖励或卡包编号替换。
- MainScene 在 `Start` 时从持久化任务实例刷新 TaskItem。GameScene 结算使用不受 TimeScale 影响的时间：积分任务与结算分数同步滚动，贴纸和完成卡包任务在最终得分后单独滚动进度；任务奖励和下一任务生成在动画前持久化。
- GameScene 结算摘要将 `TaskBg2/TaskScore` 绑定到当局结算分数，将 `TaskBg2/TaskBagNum` 绑定到 SQLite 中生命周期为 `Completed` 的卡包数量；未完成的已解锁卡包和进行中卡包不计入，重玩不会重复计数。
- GameScene 进入时记录描边设置快照，点击 `BtnTips` 时记录提示使用，首个 Piece 成功放置时开始不受 TimeScale 影响的积分计时，RewardPanel 结算开始时冻结。
- MainScene `PanelSet/SliderMusic` 和 `PanelSet/SliderEffect` 是手工拼装的仿 Slider：根 Image 背景加 `SliderFill`、`SliderHandle` 子节点。运行时使用 `FakeSettingsSliderInput` 处理指针拖动、刷新视觉并保存数值。
- 不使用 `PlayerPrefs`。
- `LoadingScene.Start` 初始化 `JsonLocalStore`、`SqliteLocalStore`、`GameTaskUtility` 和 `CardPackDataUtility`。
- `Assets/Scripts/Model` 有意保持单层扁平目录。相关纯 C# 类型按以下方式合并：`GameManager` 位于 `GameDefine.cs`，CSV 解析类型位于 `GameConfigRepository.cs`，`JsonLocalStore`、`SqliteLocalStore`、`GameSettingsData` 和 `GameSettingsUtility` 位于 `LocalDataStore.cs`，积分类型和 `GameScoreUtility` 位于 `GameTaskUtility.cs`，`GameFontUtility` 位于 `GameCommonUtility.cs`。公开类型名和调用点保持不变。
- Model 当前保留 7 个脚本。`CardPackDataUtility`、`GameTaskUtility`、`GameConfigRepository`、`CardPackRewardFlyTransition` 和 `GameDefine` 属于大型或独立模块，不为减少文件数量继续互相合并。旧 `GameAnimationUtility` 与 `CardFxRuntimeUtility` 已随开包特效一起删除。
- MainScene 的卡包选择、居中放大、`PanelBagSelect`、开包输入和 2D 缺失资源回退逻辑保持不变。选中态在隐藏选中槽位后临时隐藏软件鼠标，生成不包含鼠标的四分之一分辨率背景截图并放大虚化，再立即恢复鼠标并叠加半透明 Panel 压暗；选中卡包使用更高排序层保持清晰。运行时保持动态封面、`600 x 680` 设计尺寸、选中复原和进入 GameScene 的现有交互节奏。
- 只有通过正常拆包进入 GameScene 时才播放一次入场：CardBag/棋盘从上方进入，PieceBoard 从下方进入，当前组 Piece 从棋盘附近错峰落入托盘，返回和提示按钮淡入；入场完成前屏蔽拖拽。对象在起始姿态保留两个渲染帧后才推进动画，单帧动画时间最多推进 `1/30s`，场景加载或首帧资源初始化卡顿不得吞掉入场过程。直接在编辑器启动 GameScene 保持即时初始化。
- 每组完成后等待最后一片当前块内的绿色 ADD 滑光结束，再将 CardBag/棋盘位置、相机正交尺寸和托盘平滑切换到下一组布局；棋盘主体动画约 `0.72s`，新组 Piece 从棋盘区域用约 `0.38s` 错峰进入托盘。动画期间锁定拖拽、提示和“一键完成”，普通卡包同样使用该切组流程。
- GameScene 的 `BtnCompleteAllTest` 仅在 Unity Editor 和 Development Build 中运行时创建。点击后批量持久化当前 CardBag 全部 Piece 编号、显示完整棋盘并调用正式 `ShowRewardPanel()`；因此卡包生命周期、任务积分、奖励发放和完成数量都会产生真实本地测试数据。正式非 Development Build 不显示该按钮。
- `GameScene/BtnTips` 从当前组选择 Piece 编号最小的未完成碎片。目标碎片在托盘原位置左右抖动约 `0.8s` 后停止，棋盘对应 `GrooveRect` 使用 `HintDashedOutlineGraphic` 从 GPU 读取 Piece Sprite 的实际 Alpha 像素边界，沿真实累计轮廓长度生成固定 `20` 像素实线、基础间隔 `15` 像素、滚动速度 `60` 像素/秒的绿色滚动虚线；普通模式使用 `(112,151,75)`，高对比度使用 `#b1d702`。轮廓在当前 GameScene 内按 Sprite 缓存并在离场时清空，Physics Shape 只作为读取失败回退。再次点击按钮取消当前提示，成功放置、切组或结算时同样清理。一旦有效提示显示过，本局持续记为已使用提示。
- `CardBag001` 引导按实际三组拆成三个阶段，首次流程已有部分进度时从当前未完成组继续；活动拼图会话优先于历史卡包完成状态和教程完成记录，因此已完成卡包“重玩”中途退出后再次进入，也会从当前未完成组继续引导。只有整包完成并进入结算时才将 `Tutorial/CardBag001TutorialCompleted` 写入 SQLite `AppRecords`，中途退出不会提前完成教程；没有活动会话的已完成卡包普通进入不自动引导，通过 MainScene“重玩”确认进入时则从第1组重新播放完整引导。第1组 `Piece0101` 为强引导：保留游戏原有暗色托盘且不额外叠加教程黑色遮罩，突出目标 Piece，显示蓝色滚动虚线和等比缩小至原尺寸 `70%` 的 `GuideArrow1.png`；箭头从碎片中心出现并循环移动指向目标凹槽，不做从小到大的缩放，并只允许拖动目标；拿起后文字和虚线保留，放错恢复焦点。第2组 `Piece0201/0202` 同时高亮、允许任意顺序放置，并从本阶段开始显示当前组烘焙关卡描边；不显示箭头或目标虚线，第一片放对后只刷新剩余 Piece 焦点。第3组 `Piece0301-0305` 恢复正常交互和 `BtnTips`，并介绍提示功能；`BtnTips` 在前两步保持隐藏。第一步提示框以屏幕归一化位置 `(0.5, 0.7)` 为基础向上增加一个提示框自身高度，再追加设计坐标偏移 `(-30, -50)` 并限制在屏幕安全范围内；第二、三步分别位于当前待拼凹槽整体边界上方和右上，三者从对应方向淡入；第二步位置随棋盘布局动态计算并限制在屏幕安全范围内。三步文字统一克隆场景 `GuideTip/TextTips`，保留编辑器设置的 TMP 字体、材质、颜色、字重、对齐和 RectTransform；运行时只替换文案，并将内容平衡为最多两行，排除标点出现在第二行开头的断点，超过单行宽度时启用 Auto Size 缩小字号。第三步从场景 `GuideTip/Arrow` 读取 `GuideArrow2.png` 及编辑器布局，提示框入场后箭头淡入并循环向右上推进。该引导本身不算使用提示。
- 每片可见贴纸只从 `UI/GameScene/PieceLight1.png` 到 `PieceLight4.png` 中按 Piece 编号确定性选择一张不规则暖白亮光，并且只创建一个光；托盘 SpriteRenderer 与棋盘 UGUI 分别通过 SpriteMask 和 Alpha Mask 将它裁切在贴纸真实轮廓内。同一 Piece 从托盘切到棋盘或切组重建时保持所选图片、旋转、缩放和相对位置。正确放置 Piece 后仍先从松手位置以三次方减速曲线吸附到凹槽中心，固定 `0.12s`；抵达后当前块播放原有约 `0.52s` 的绿色斜向 ADD 光带，同时当前块和最多六块实际相邻的已拼贴纸按距离延迟约 `0.07~0.23s` 移动各自已有的单个亮光，完整传播约 `0.72s`。亮光滑动结束后不淡出、不销毁，停在终点；后续再次被传播触发时从该终点继续移动。绿色光带仍只作用于当前刚吸附块，两层动画都结束后才解除落位锁定并继续切组或结算。
- RewardPanel 的 `TaskBg2/TaskScoreTitle` 初始为空，基础分滚动期间不显示文字；首条可见文字是本局第一个实际生效的加分项，之后依次显示“未使用提示 +N分”“关闭关卡描边 +N分”“关闭贴纸描边 +N分”和“快速完成 +N分”。`N` 使用该阶段累计分数与上一阶段分数的实际差值，确保全部加分之和与最终分一致。全部加成展示完成后标题显示“卡包数”；本局没有加成时直接在基础分滚动结束后显示“卡包数”。标题强制单行，超出宽度时保持编辑器字号上限并自动缩小；积分计算、任务进度和发包结果不变。
- `PanelBagSelect` 每次打开时同时读取历史生命周期和当前拼图会话。只有 `Completed` 且没有活动会话的卡包显示 `重玩` 并弹出 `PanelReplay`；未完成卡包以及重玩中途退出、仍有活动会话的 `Completed` 卡包都显示 `玩` 并直接进入现有流程。`BtnReplay` 确认时清除旧会话，新会话在进入 GameScene 时创建；`BtnReturn` 和 `BtnClose` 取消。相机按钮只对历史上至少完整完成过一次、生命周期为 `Completed` 的卡包显示；首次拼图尚未完成的 `InProgress` 卡包不显示。弹窗显示期间隐藏选中卡包、其他列表卡包 Renderer 和尺寸图标，并锁定选择页按钮；取消时全部恢复，确认时保持隐藏并衔接开包舞台。
- 点击 `BtnCamera` 后播放一次全屏白色闪光，并离屏生成 `1024 x 1024` PNG。图片由 `MainPhotoBg` 木纹底图、当前 `CardBagNNN` Prefab 还原的完整拼图和左下角 `MainGameIcon` 组成；拼图等比适配并轻微旋转。文件以 `Application.productName-YYYY-MM-DD-BagId.png` 保存到桌面，BagId 使用三位编号，同日同一卡包重复拍照覆盖旧文件。保存成功后通过独立顶层 `PanelPhotoCanvas` 显示 `PanelPhoto` 并将 `Photo` 替换为生成图；预览期间隐藏选中卡包，点击 `BtnOK` 关闭预览并恢复卡包。拍照不写业务持久化数据。

### 开发期持久化策略

- 开发阶段的本地持久化不保证向后兼容。数据结构和 SQLite 字段类型可直接改为当前需求，不增加迁移或旧数据回退，除非用户明确要求。
- SQLite 表结构发生不兼容修改后，关闭 Unity，并在测试前删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`。
- JSON 任务进度或跨两个存储的行为发生变化时，同时删除 `LocalData.json`。每次不兼容修改后，助手必须指出需要删除的文件；未经明确要求不得自动删除。

---

## 6. 添加内容

### 卡包

`MainScene.RefreshPackageList` 根据数据库动态创建已解锁卡包槽位。不要在场景中手工复制 `Package002`、`Package003` 等对象。

共享尺寸图标为 `UI/PackImages/PackSize_1.png` 到 `PackSize_7.png`，对应 `CardPackSize` 数值（`XS=1` 到 `XXXL=7`）。`PackItem` 的可见结构为 `PackItem/CardPackEffect/PackCover|PackHighlight|PackSize`；MainScene 在运行时设置封面和尺寸 Sprite，并根据编辑器封面尺寸缩放尺寸图标。可调高光层级顺序为 `PackCover`、`PackHighlight`、`PackSize`。`PackHighlight` 父节点初始化启用，四张高光贴片使用 `Assets/Resources/PackHighlightAdditive.mat`；MainScene 只控制父节点显隐，不改其材质、颜色、Alpha 或子节点布局。`PackageInteractionHandler` 在 `PackItem` 根节点驱动 `CardPackEffect` 容器整体呼吸，默认范围为 `0.98..1.02`、周期 `2.4s`，并允许在 Prefab Mode 选中 `PackItem` 时预览和调整；`PackItem` 布局根节点本身不缩放。卡包移出 ScrollRect 可视范围、被其他面板遮挡或进入选中放大流程时，高光与呼吸随列表卡包一起隐藏/暂停；返回列表后恢复。选择页与选中卡包 Canvas 都使用 Main Camera，并明确覆盖 Sorting Order；Camera Canvas 之间的屏幕/本地坐标转换、撕包输入范围和卡包退出位置必须传入各自 `worldCamera`，不能沿用 Overlay 模式的 `null` 相机。Shader 和 Material 不得放进 `Assets/UI`，否则 BuildSync 会将其复制到 `StreamingAssets/UI` 并触发重复材质导入。

`PackItem` 不再包含 `PackShadow` Image，也不再读取封面像素或在 CPU 生成阴影 Texture/Sprite。`PackCover` 直接引用 `Assets/Resources/PackCoverShadow.mat`；对应 UGUI Shader 根据当前封面 Alpha 在同一次绘制中合成投影和原封面，并按源贴图像素提供颜色/透明度、X/Y 偏移、X/Y 模糊、扩散及 X/Y 渲染留白参数。`PackCoverShadowEffect` 只在 UGUI 网格生成阶段围绕 `PackCover` 自身矩形中心提供 Shader 所需留白，并在 Material 留白参数变化时刷新网格；不能在 Shader 顶点阶段围绕 Canvas 原点直接缩放。MainScene 只替换封面 Sprite，不覆盖 Material。美术统一在该 Material 中调整投影；阴影被裁切时先增大 `Render Padding X/Y`。

1. 场景中只保留一个模板对象：`Package001`。
2. 在 `CardPacks.csv` 增加一行（`PackId`、临时 `PackSize`、临时 `StickerCount`、`ChapterId`、正数 `BoardScale`、手工 `Series`、`AutoUpdate=1`），随后执行配置更新工具按实际片数同步 `PackSize`、`StickerCount` 和 `BoardScale`。需要手工固定这三个值时将该行 `AutoUpdate` 改为 `0`；工具始终保留 `Series` 原值。
3. 在 `UI/PackImages/` 下按 `PackIconNNN.png` 命名增加对应封面。`GameDefine.FormatPackImagePath` 将 PackId `1` 映射到 `UI/PackImages/PackIcon001.png`。
4. 通过 `CardPackDataUtility` 将生命周期写入 SQLite `CardPacks` 表。
5. 卡包列表和选中态直接使用对应静态封面；不为每个卡包创建 3D 展示资源。

### 拼图

1. 在 `Assets/Resources/CardBagPrefabs/` 下创建 `CardBagNNN` Prefab，`NNN` 与 `PackId` 一致。
2. Prefab 内放置一个名为 `GameBoard` 的子对象。
3. 在 `GameBoard` 下用 Image 对象添加分组碎片，名称严格使用 `PieceGGII`：`GG` 和 `II` 都是两位数字且范围为 `01..99`。例如第 1 组使用 `Piece0101`、`Piece0102`...，第 2 组使用 `Piece0201`、`Piece0202`...。
4. 源贴图放在 `Assets/UI/CardBags/CardBagNNN/`，标准切图名继续使用 `piece_001.png`、`piece_002.png`...；需要在源文件名中显式携带正式分组时使用 `Piece0101.png` 或 `Pieces0101.png` 格式。
5. 不使用 `PieceGroup` 父节点；分组严格读取 `PieceGGII` 的前两位 `GG`，组内排序读取后两位 `II`。
6. 不创建 Package JSON；运行时数据来自已加载 Prefab 的 Image。
7. 新增或修改 CardBag 后，执行 **Puffies -> Bake Outline Masks**。烘焙器优先使用 `GameBoard.png` 的透明挖空 Alpha 作为最终拼图外边界，并使用已完成 Piece 的 Alpha 作为后续组接触边；GameBoard 没有有效挖空时回退到全部 Piece Alpha 并集。每组生成三张同尺寸资源：`GroupNN.png` 是默认连接区域；`GroupNN_Level.png` 是当前组 Piece Alpha 并集的完整外边界；`GroupNN_Stickers.png` 是当前组每块 Piece Alpha 边界的并集。默认连接图第 1 组只包含自身最终拼图外边界，后续图只包含当前组最终外边界及其与低编号已完成组的接触边。接触边和最终外轮廓均使用圆形最近距离与局部边界法线判定归属，切线方向的邻近不得延长端点；相邻分组可在真实交点共享少量边界像素，不能为避免重叠而删除交点。
8. `GameScene` 将烘焙 Sprite 的 Alpha 作为不可交互的 `GameBoard` 子 Image 显示，并通过专用 UGUI Shader 固定输出 `#3f423e`。Shader 在源纹理像素坐标上生成稳定的细粒和少量小空点，形成轻微铅笔断墨质感；纹理不随棋盘移动或缩放逐帧变化。关卡描边关闭时加载 `GroupNN.png`，打开时替换为 `GroupNN_Level.png`；贴纸描边打开时额外叠加 `GroupNN_Stickers.png`。不要在 Prefab 中手工制作描边对象，也不要为两种底板重复烘焙资源。
9. 缺少生成 Sprite 时，运行时记录制作警告，并在无描边情况下继续游戏。交付前重新运行烘焙器。
- 创建一组碎片时按编号从左向右排列，使用所有卡包共用的 `20` 设计像素固定间距，并以每块 Piece 的实际 `SpriteRenderer.bounds` 将渲染内容上下居中到黑色托盘。PieceBoard 的世界边界从根 Canvas 设计坐标直接映射到当前屏幕和游戏相机，不依赖 Screen Space - Camera Canvas 在相机适配后的首个渲染帧更新世界角点，因此首次初始化与点击后的重排使用相同托盘中心。Piece 从托盘拿起时，仅仍在托盘且编号靠后的 Piece 沿 X 轴用 `0.5s SmoothStep` 向前补位；不得刷新前序 Piece、外部 Piece 或任何剩余 Piece 的 Y/缩放，拿起队尾时不启动位置刷新。松手时首先以鼠标或触点屏幕坐标检查托盘原始区域，命中后立即自动回托盘，不再检查正确吸附；因此棋盘与托盘重叠部分始终由托盘优先处理。未命中托盘才继续正确吸附和自由放置判定。未吸附 Piece 只允许完整渲染边界停在棋盘内 Alpha 为 0、没有已拼内容占用的位置，完整落在棋盘左右两侧的桌面，或在棋盘左右边界之间完整落入棋盘底边与托盘原始顶部之间的桌面空间，并且不得与另一块外部 Piece 重叠；任何横跨棋盘边框或侵入托盘原始高度的状态均不允许停放。棋盘正上方、已拼内容轮廓上或其他外部 Piece 上松手时缓动返回本次拖拽起点。来自托盘的错误 Piece 松手后立即回弹，不预先变红或停顿；所有从桌面或棋盘外部返回托盘的 Piece（包括玩家手动拖回和被正确 Piece 顶回）都在渲染边界首次进入可见托盘区域后，以原错误红色 `70%` 强度显示反馈，到位后淡回原色。外部位置之间的错误回弹不显示托盘红色反馈。若正确目标被外部错误 Piece 占用，全部实际轮廓重叠的错误块会按编号重排并回弹到托盘，不能阻止正确 Piece。托盘即使正在隐藏或已经收下，原始屏幕区域仍作为回收热区；命中后立即恢复并启用托盘、刷新布局，再按编号计算 Piece 的托盘目标位置并回弹到托盘内部。托盘收起完成并仍有外部错误 Piece 时，每隔 `5s` 让这些 Piece 短暂抖动一次；拖拽、回弹、切组、结算或托盘重新出现时停止并重新计时。最后一块正确吸附后仍进入切组或结算。

#### 无 JSON Prefab 批量生成

- 菜单 **Puffies -> Generate CardBag Prefabs From Images** 打开批量窗口，扫描 `UI/CardBags/` 下严格匹配 `CardBagNNN` 的一级目录。
- 每个卡包硬性需要 `CardBagNNN/GameBoard.png`、`Previews/CardBagNNN.png` 和至少一张合法 Piece PNG；缺失项会显示在列表中并禁止选择。
- 旧 `background_base.png` 仅用于兼容迁移：当 `GameBoard.png` 不存在时，扫描器通过 `AssetDatabase.MoveAsset` 自动改名并保留 Meta/GUID；两者同时存在时不覆盖目标文件。
- `BoardTitle.png` 是标准资源但采用软校验。缺失时列表显示警告，仍允许生成不含 `BoardTitle` 节点的 Prefab。
- `UI/CardBags/Previews/CardBagNNN.png` 是完整拼图和 Piece 定位参考图，不作为运行时 Prefab Sprite；它必须与 `GameBoard.png` 画布尺寸一致。
- 生成器利用 Piece PNG 保留的原始裁切 RGB，默认在 Preview 完整图中做像素匹配，Piece Alpha 继续作为运行时形状。Preview 因调色板量化、分割线或其他处理无法通过置信度校验时，再尝试 `GameBoard.png`；GameBoard 若已挖空或未保留完整 RGB，回退可以继续失败，不要求所有卡包都维护第二套完整定位图。
- 第二轮不透明像素匹配排除 Alpha 轮廓内侧 `1px`，避免 Preview 分割线或相邻 Piece 覆盖导致正确位置被边缘像素否决。Preview 和 GameBoard 各自沿用同一校验：常规最低匹配率为 `98%`；低于该值时，只有匹配率至少 `90%` 且所选精确 RGB 锚点在当前参考图中唯一才允许生成并记录警告，重复锚点或低于 `90%` 继续报错。两张参考图都失败时错误同时包含两边原因。
- 感知颜色匹配会验证透明像素轮次与不透明像素轮次中“精确 RGB 锚点唯一”的定位结果；该位置本身达到 `78%` 感知匹配率时才加入候选种子，避免重复图案中的偶然同色像素误导定位。首轮 `6px` 感知匹配仍不通过时才使用 `1px` 逐像素网格回退，覆盖细线和高对比图案偏移一个像素即失配的情况。逐像素回退优先执行最低 `78%`、候选差值至少 `1.5%` 的颜色校验；颜色受整批调色影响时，只有颜色仍达到 `65%`、原坐标的 RGB 边缘梯度达到 `85%`，且该结构区域领先远端候选至少 `3%` 才允许生成。搜索细化半径保持 `7px`；颜色、结构和轮廓的独立候选比较按 Piece 短边 `15%` 计算位置簇半径，并限制为 `14~48px`，避免同一槽位的宽匹配峰值被误判成远端重复位置，同时不合并相邻槽位。整包生成会记录已定位 Piece 的高 Alpha 占用，共享边缘像素归属最近定位的 Piece；面积相近的候选若与任一已定位 Piece 主体重叠达到 `65%`，会在精确、颜色、结构和轮廓最终候选阶段被排除，正常边缘接触和面积差异明显的小配件覆盖不受影响。结构校验只验证颜色定位得到的原坐标，不使用结构最高点移动 Piece。颜色与结构均失败且 Preview 包含青色分割边界时，最后使用 Piece Alpha 外边界匹配 Preview 边界邻域；该回退不用于 GameBoard，要求轮廓匹配至少 `75%` 且领先独立远端候选至少 `8%`，不能通过最佳点附近候选占满列表来绕过唯一性校验。
- GameBoard 回退的首个 Piece 达到至少 `99.5%` 且位置唯一后，本卡包后续 Piece 统一使用 GameBoard，日志明确记录参考图。参考图颜色索引保存颜色次数和首次像素下标；唯一颜色锚点直接计算候选位置，不遍历整张画布。
- `PieceGGII.png` 或 `PiecesGGII.png` 中的四位 `GGII` 直接成为正式对象编号并作为人工覆盖规则。全部使用标准 `piece_###.png` 时，生成器按位置从上到下分带、每行最多两个空间组，每组最多 14 片；偶数行从左到右编号，奇数行从右到左编号，形成蛇形组序，组内始终按中心点从左到右编号。最终 Hierarchy 按 `PieceGGII` 升序创建。标准名与显式正式名不得在同一卡包内混用。
- 生成结构为 `CardBagNNN/GameBoard/BoardTitle` 和 `CardBagNNN/GameBoard/PieceGGII`；棋盘标题与全部碎片统一归属 `GameBoard`。
- 窗口默认只选择资源完整且尚无 Prefab 的卡包。选择已有 Prefab 时显示 `Overwrite`，执行前必须确认；覆盖会替换已有层级和手工 Piece 分组。
- 批量生成逐个隔离失败并汇总结果，负责创建带正式自动分组的 Prefab，但不自动烘焙描边；生成时会删除该包可能残留的旧描边。完成生成后执行 **Bake Outline Masks**。
- 同一窗口的 **Update Existing Piece Layouts** 用于切图更新后的局部校准。它复用 Preview/GameBoard 定位算法，通过现有 Piece 的 Sprite 资源路径映射节点，只更新 `RectTransform.anchoredPosition` 与 `sizeDelta`；不重建层级、不改变手工分组、Image 参数、影子、旋转缩放或描边资源，也不会自动烘焙描边。更新采用整包事务：源 PNG 与现有 Piece 数量或引用不一致、定位不唯一，或两张有效面积相近的切图在目标位置的高 Alpha 区域重叠达到 `65%` 时，该 Prefab 在保存前失败，避免重复切图覆盖正确布局；面积明显较小且位于大切图内部的独立配件允许更新。
- `Piece001` 到 `Piece999` 的三位顺序名仅作为旧 Prefab 的制作中间状态，不属于正式命名；当前生成器不会再从标准切图创建这类名称。Prefab 中只要仍有任一三位节点，描边烘焙器仍会跳过整包，避免 `Piece100` 等顺序节点被误判为正式分组；卡包没有正式分组时删除对应旧描边目录。
- 当前 CardBag017 为 `1316 x 1316`、37 片，已完成正式分组并生成 5 组描边资源。CardBag022 为 `2600 x 4000`、196 片，已按相邻空间区域分为 14 组、每组 14 片并使用正式 `PieceGGII` 命名；14 组默认、关卡和贴纸描边资源均已生成。

### 拼图描边渲染

- 拼图描边由 `PuzzleOutlineBakerEditor` 离线生成，并通过 Unity UGUI `Image` 渲染。
- 关卡描边与贴纸描边默认都关闭；默认状态仍显示现有连接区域，不等于完全隐藏全部描边。
- 每个 `GroupNN.png` 独立生成当前阶段需要的线段；不同阶段可以共享交点像素，因为运行时只显示当前阶段蒙版。
- 边界归属除距离外还校验目标组位于边界的正确法线方向；最终外轮廓要求目标组位于轮廓内侧，已完成组接触边要求当前组位于旧组边界外侧。
- 已完成组接触边与当前组最终外轮廓只在真实交点附近修补栅格化断口；桥接路径最多 `4px`，且只能在真实边界外 `1px` 的走廊内移动。内部独立接触边不要求连接到最终外轮廓，禁止用长斜线或梯状短线强行连接不同边界组件。
- 项目没有运行时描边 Shader、Renderer Feature 或第三方描边包。
- 描边加载与拼图交互保持隔离；缺少描边不得阻止可拖拽碎片创建。
- 运行时 `ActiveGroupOutline` 根节点通过 `CanvasGroup` 控制显示。首组创建和切组创建时 Alpha 初始为 `0`；首次入场或切组的棋盘移动结束后，使用不受 TimeScale 影响的 `0.5s` 平滑淡入。新手引导第一阶段仍完全隐藏烘焙描边，切到第二阶段时按同一移动结束时机淡入。

### 卡包展示与开包表现

- 首页列表的卡包主体使用 `Assets/UI/PackImages/PackIconNNN.png` 静态图；`PackItem.prefab` 不嵌套 3D 卡包特效 Prefab，但包含编辑器可调的 UGUI ADD 高光贴片。
- MainScene 主 Canvas 使用 `Screen Space - Camera`，`World Camera` 固定为场景 `Main Camera`，Plane Distance 为 `10`；`MainScene.ConfigureMainCanvas` 在运行时复用统一 Canvas 配置再次校正，确保 `PackItem` 封面、高光、尺寸图标和首页主 UI 经过同一摄像机渲染。
- Unity 编辑器单独打开 Loading、Main、Game、Rank 或 Achieve 场景后，`CanvasDesignResolutionEditor` 会延迟一帧按根 `Canvas` 的世界边界，将 SceneView 恢复为正交正视并自动取景；这避免 Camera Canvas 与 Overlay Canvas 之间切换时继承歪斜视角，不修改场景 Selection、Canvas 配置或运行时行为。`EffectScene001` 不参与自动取景，以保留制作方的三维编辑视角。
- 选中态使用独立 `Screen Space - Camera` Canvas 的 `Image` 显示同一张静态图，目标尺寸为 `600 x 680`；该 Canvas 与选择面板同样绑定 `Main Camera`，背景虚化、返回、拍照和重玩确认继续沿用现有流程。
  - 点击玩后切换到 `BgGame` 开包舞台并等待玩家轻点或横划。有效操作随机选择 `CardPackOpeningModel_001-006`，共用制作方 `CardPackAnimation.controller`；短名称正面网格的 `_MainTex` 在运行时替换为当前 `PackIconNNN`。长名称背面网格会显示制作方 `Bg01.png` 灰块，替换成封面又会形成第二层完整卡包，因此当前运行时禁用该 Renderer；FBX、骨骼、UV 和动画资源本体不修改。
- 开包特效的混合模式和内部渲染层级归资源配置所有：`fx_chai_w_001.prefab` 保留各 ParticleSystemRenderer 自己的 `sortingOrder`，其 Material/Shader 决定 Additive 或 Alpha 混合；运行时代码不得把所有粒子 Renderer 强制改成同一排序值。卡包正反面 `test.mat`、`test01.mat` 的 Custom Render Queue 固定为 `2001` 并直接保存在 Material 中，运行时材质实例只替换动态贴图。
- 制作方 Timeline 中骨骼动画先启动，`fx_chai_w_001` 在 `0.5s` 后启动并占用约 `3.033s` 的正式轨道；运行时沿用这一相对时序，并等待模型动画与完整光效轨道中较晚结束的一项后再进入 `GameScene`。正式参考是 `EffectScene001` Timeline 绑定的主 Canvas 下实例，不是场景中另一份世界空间演示实例。开包流程直接复用 MainScene 场景 `Canvas/PackObject` 下已调好的 `fx_chai_w_001` Prefab 实例，不再通过 `Resources.Load/Instantiate` 创建光效，也不覆盖根或子节点 Scale、Start Size、材质、粒子模块和相对排序。主 Canvas 使用 `Screen Space - Camera` 并绑定 Main Camera，与开包模型由同一摄像机渲染；不得以“同一摄像机”为由移动用户配置的父节点。`PackObject` 容器保持激活，内部编辑器参照 `PackItem` 与光效实例默认关闭，播放时只激活光效实例。
- `test.playable` 为正式滑光轨道保存了 `particleRandomSeed=1`。运行时手写播放必须复现该确定性：播放前只把启用 `Auto Random Seed` 的场景 ParticleSystem 实例设为种子 `1`，保证跨场景返回后重复开包仍与首次一致；这是 Timeline 播放状态，不得写回或修改粒子 Prefab 的美术参数。
- 撕包完整发生在 MainScene 的 `BgGame` 开包舞台，不能移到 GameScene。横向撕口光效与模型由 Main Camera 渲染；模型使用制作方 `EffectScene001` 保存的基准 `Scale=2.63 / localZ=0`。场景光效始终保留在 `PackObject` 原层级，完整使用编辑器中人工调整的根/子节点 Transform、Start Size、发射参数、材质和排序；运行时代码不得设置其位置、旋转或缩放，也不让其继承模型 Stage 的适配。模型尺寸和中心只按正面 `mesh_skin_cardPack_NNN` 计算，背面网格不参与初始对齐。
- MainScene 进入开包舞台并开始等待玩家轻点或横划时，低优先级异步预加载当前 `CardBagNNN` Prefab，随后将 GameScene 加载到 `90%` 待激活状态；拆包动画结束后只开放场景激活。GameScene 实例化卡包时优先复用 PackId 匹配的预加载 Prefab，预加载失败或直接进入则回退同步 `Resources.Load`。该流程只前移首次资源读取和场景反序列化，不改变玩法初始化、Collider 精度或入场动画。MainScene 撕包结束并进入 GameScene 后，不在游戏页重播卡包模型。切场景前由 `SelectedCardPackImage` 的实际 RectTransform 记录卡包下沿归一化屏幕坐标；当前未完成组的 Piece 从该坐标依次出现，再进入下方暗色托盘现有布局。托盘目标位置、顺序和 Piece 缩放继续使用 GameScene 已计算结果，不按固定分辨率估算起点。
  - 撕包模型和光效直接放入 MainScene 世界并由 `Main Camera` 渲染；运行时将 EffectLayer 加入主相机 Culling Mask，按居中静态卡包的真实屏幕中心和高度等比定位整个 Stage，结束或中断时恢复原 Culling Mask。`BgGame` 开包背景使用同一主摄像机下的世界 `SpriteRenderer`，以不透明几何队列先于卡包模型绘制，不能放在高 Sorting Order 的全屏 UGUI Canvas 中覆盖模型。最终画面不创建独立特效相机、RenderTexture、RawImage 或撕口蒙版采样；旧透明二次合成路径出现的粒子外围黑色矩形属于混合异常，不是制作方特效内容。
  - 静态封面切换到 3D 模型时，模型先在 Animator 第 `0` 帧完成贴图、定位并实际渲染一帧；随后启动 Animator，静态封面继续全不透明保持 `0.06s` 以遮住动画开头的蒙皮预备变化，再以 `0.12s` 淡出。动画完成时间与光效延迟从 Animator 启动时计算，避免空白帧、硬切或开头横向展开被直接看见。
- `Assets/Scenes/EffectScene001.unity` 仅用于核对制作方配置和 Timeline，不加入正常场景导航。列表保留静态封面整体呼吸与 UGUI ADD 高光，但不加载 3D 模型、撕包粒子、特效 Skybox、Directional Light 或 `CardPackListUnlit.shader`。
- `CardPackRewardFlyTransition` 仅负责结算后新卡包从 RewardPanel 飞到屏幕中央，再飞回 MainScene 对应列表位置，不属于开包特效，继续保留。
- `Assets/Resources/CardBagPrefabs/` 是 GameScene 拼图关卡资源，不属于卡包展示特效，必须保留。

---

## 7. 命名

卡包静态封面继续使用 `PackIconNNN.png`；GameScene 拼图 Prefab 继续使用 `CardBagNNN.prefab`。

---

## 8. 构建

构建前执行 **Puffies -> Sync Build Resources**。该命令将 `PackImages`、`BasicUI`、`MainScene`、`GameScene`、`AchieveScene` 和 `RankScene` 复制到 `StreamingAssets/UI`；CardBag 源贴图通过游戏 Prefab 的 Sprite 引用进入构建，因此不复制。

建议 Build Settings 顺序：LoadingScene -> MainScene -> GameScene -> RankScene -> AchieveScene。

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
| Puffies -> Apply Design Resolution (Current Scene) | 为当前场景应用 Canvas 设计分辨率 |
| Puffies -> Apply Design Resolution (All Scenes & Prefabs) | 为全部场景和 Prefab 应用 Canvas 设计分辨率 |
| Puffies -> Setup Default Chinese Font | 设置中文字体 |
| Puffies -> Bake Outline Masks | 为每个 CardBag Prefab 重建各分组外边界描边 |
| Puffies -> Generate CardBag Prefabs From Images | 扫描完整背景和透明碎图；窗口可选择完整生成 CardBag Prefab，或仅按效果图更新现有 Piece 的位置与原生尺寸 |
| Puffies -> Update Pack Sizes From Piece Counts | 扫描 CardBag 源资源的碎片 PNG 数量并同步更新 `CardPacks.csv/PackSize`、`StickerCount` 与 `BoardScale`；跳过 `AutoUpdate=0` 的行，并始终保留手工 `Series` 内容 |

---

## 10. 已弃用

- `Assets/ArtRes/`、`Assets/Configs/`
- `Resources/Config/Package001.json` 及 JSON 拼图配置流程
- `Tools/*.ps1` 下的一次性迁移脚本
