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
| MainScene | 根据 `CardPacks.csv` 与 SQLite 解锁状态刷新卡包列表；每页按 6 列 x 3 行显示 18 个静态卡包封面。`PackItem.prefab` 保留 `PackCover`、`PackShadow` 和 `PackSize`，运行时将 `PackCover` 替换为对应 `PackIconNNN.png`；列表不创建 3D 模型、粒子或 Animator。点击后在顶层 Overlay 中使用同一静态封面，从列表实际位置等比放大到屏幕中心 `600 x 680`，并显示 `PanelBagSelect` 和既有背景虚化；Back 将静态封面返回原列表位置。点击玩/确认重玩后进入 `BgGame` 开包舞台，等待玩家再次轻点卡包或横划；正式撕包动画暂未接入，有效操作当前直接进入 `GameScene`。保留拍照、重玩确认、Rank、Achieve 和 Menu 入口。 |
| GameScene | 根据选中 PackId 加载 `CardBagNNN` Prefab，并读取 `CardPacks.csv/BoardScale` 缩放棋盘；按照 `PieceNN` 数字命名组织拼图分组；从正常开包流程进入时播放棋盘、托盘和当前组 Piece 入场；每次正确放置 Piece 后立即持久化，重新进入时恢复已放置 Piece 并从首个未完成分组继续；全部完成后显示 RewardPanel；Editor 和 Development Build 在 `BtnTips` 左侧提供“一键完成”测试按钮 |
| RankScene | 仅占位；首个 Demo 不包含排行榜后端功能。当前模拟列表前三名的 `RankBg` 分别使用原生 `1646 x 148` 的 `RankCellBg_1.png`、`RankCellBg_2.png`、`RankCellBg_3.png`，第四名以后使用 `1636 x 136` 的 `RankCellBg.png`；`RankItem` 根高度为 `148`，列表纵向间距为 `5`，条目中心步距为 `153` |
| AchieveScene | 当前显示 20 条模拟成就，前 5 条已达成、后 15 条未达成；接入 Steam 后替换数据源。成就网格固定为 6 列，单元尺寸 `240 x 332`，横纵间距均为 `40` |

所有场景常规鼠标图标为 `UI/BasicUI/ImgHand_1.png`。GameScene 悬停当前可拖 Piece 时切换 `ImgHand_2.png`，按住左键拖拽 Piece 时切换 `ImgHand_3.png`；松开、结算或离开 GameScene 后恢复常规图标。三张资源随 `BasicUI` 同步到 Player 的 `StreamingAssets/UI/BasicUI`。运行时使用 `CursorMode.ForceSoftware`，以 `2560x1440` 设计分辨率和 CanvasScaler `Match=0.5` 计算统一缩放系数，分别等比重建三张光标纹理并同步缩放热点；窗口尺寸变化时自动刷新，不能把三种不同比例的源图压入固定画布。

### 数据与奖励需求

- 任务配置来自 `Resources/Configs/TaskConfig.csv`。
- 卡包配置来自 `Resources/Configs/CardPacks.csv`。
- `CardPacks.csv/StickerCount` 紧跟在 `PackSize` 后面，记录卡包贴纸数量。`PackSize` 按该数量确定：`<30=XS`、`30..37=S`、`38..49=M`、`50..69=L`、`70..84=XL`、`85..99=XXL`、`>=100=XXXL`。配置更新工具同时将 `BoardScale` 更新为：`XS=0.75`、`S=0.78`、`M=1.10`、`L=1.30`、`XL=1.00`、`XXL=1.15`、`XXXL=1.30`。工具只统计 `Assets/UI/CardBags/CardBagNNN` 顶层的标准碎片名 `piece_NNN.png`，不统计 `BoardTitle.png`、`GameBoard.png` 或其他 PNG。
- `CardPacks.csv` 最后一列 `AutoUpdate` 只允许 `0` 或 `1`，默认值为 `1`。配置更新工具遇到空值会补为 `1`；设为 `0` 的配置行会保留手工填写的 `PackSize`、`StickerCount` 和 `BoardScale`，不进行自动更新。
- 任务使用随机模板池：`TaskType=1` 累计结算分数、`TaskType=2` 收集贴纸数量、`TaskType=3` 完成卡包数量。三类任务都只在完整完成一个符合尺寸要求的卡包后结算一次；贴纸任务按该卡包的全部 Piece 数量累计。
- `SizeMode=0` 表示任意尺寸；`SizeMode=1` 从模板 `SizePool` 与玩家当前可玩卡包尺寸的交集中随机指定一个尺寸。任务模板按 `Weight` 加权随机，并在存在其他候选时避免连续使用同一个 `TemplateId`。
- 积分任务目标不随机，按 `TargetPool` 的 `200 -> 400 -> 600 -> 800 -> 1000 -> 1200` 顺序循环并持久化循环游标。贴纸任务目标从 `60|80|100` 随机，完成卡包任务目标从 `1|2|3` 随机。
- 结算以卡包基础分开始（XS 60、S 80、M 100、L 120、XL 140、XXL 160、XXXL 200），将所有符合条件的百分比加成相加后统一相乘，并向上取整。
- 分数加成为：未点击 `BtnTips` +5%；关闭 MainScene `Toggle1` 关卡描边 +2%；关闭 `Toggle2` 贴纸描边 +5%；完成时间 <=15 / <=30 / <=60 秒分别 +3% / +2% / +1%。
- 完成任务后发放奖励并随机生成下一个任务实例。每个任务实例使用独立递增的 `TaskInstanceId`，同一个模板可以在后续再次出现。
- 完成任务必定创建一条持久化的新卡包权益。章节持有数量门槛关闭时，奖励保持待发并稍后重试。卡包首次完成时执行一次确定性的阶段门槛发包尝试；重玩已经 `Completed` 的卡包不执行该尝试，但仍可能通过完成任务创建任务权益。
- 卡包发放使用 8 个玩家不可见的内部章节，总量约 150 个卡包，平均每章 18.75 个。章节限制可选的锁定卡包奖励池，但不显示在 MainScene 或其他玩家界面。准确 PackId 分配和章节推进规则仍待确认。
- 内部章节阶段使用 `R` 表示当前章节仍为 `Locked` 的卡包数：初期 `17..9`、中期后段 `8..3`、末期 `2..1`。持有可玩数量为 `Unlocked + InProgress`，各阶段目标约为 `5-6`、`2-3` 和 `1`。章节超过 18 个卡包时，`R>17` 的额外范围同样属于初期。
- 当前发包门槛：`R>=9` 时允许 `H<=5`；`R=8` 时允许 `H<=3`；`R=7..3` 时允许 `H<=2`；`R=2..1` 时允许 `H<=1`。被拦截的首次完成发包直接跳过；被拦截的任务奖励保持待发。两个来源可在同一轮结算中同时发包。RewardPanel 保留默认 `ImgBag` Sprite；点击 `BtnFinish` 后，本次发放的全部卡包从 `ImgBag` 飞到屏幕居中行，停顿后跨越 MainScene 加载，再分别飞到对应列表位置。
- 当前任务和下一随机任务都是积分任务，并且本局卡包同时符合两者尺寸及重玩条件时，超过已完成目标的积分向后结转。
- 卡包生命周期保存在 SQLite `CardPacks` 表中，状态为 `Locked`、`Unlocked`、`InProgress` 或 `Completed`。
- 当前拼图会话保存在 SQLite `CardPackPuzzleProgress` 表中；记录存在表示该卡包有一局可继续，已正确放置的 Piece 编号即时保存，整包完成后删除记录。
- MainScene 卡包排序：上次列表展示后新发放的卡包优先展示一次，且最新发放的在前；随后依次为 `InProgress`、按解锁时间升序的 `Unlocked`、按首次完成时间升序的 `Completed`。PackId 是确定性并列排序依据；每日挑战优先级暂缓实现。
- MainScene 所有生命周期状态统一显示 `UI/PackImages/PackIconNNN.png` 静态封面，不执行颜色、材质或置灰处理。
- 任务实例、当前进度、下一个实例号、上个模板和积分目标循环游标保存在 JSON 根对象 `TaskProgressData`。
- 业务进度不得使用 `PlayerPrefs`。

### 内容扩展需求

- 新卡包沿用唯一 `Package001` 模板；`MainScene` 在运行时动态创建列表项。
- 新拼图通过在 `Resources/CardBagPrefabs/` 下新增 `CardBagNNN` Prefab 实现；每个 Prefab 包含 `GameBoard` 和 `Piece01`...`PieceNN`，不创建 Package JSON。
- 编辑器批量生成器可扫描 `CardBagNNN` 资源目录，使用完整的 `Previews/CardBagNNN.png` 与透明 Piece PNG 进行像素匹配，并以 `GameBoard.png` 作为运行时棋盘底图批量创建 Prefab，不依赖 Package JSON 或 `unity_layout.json`。生成器优先使用精确 RGB 锚点；切图与预览几何一致但存在导出色差时，回退到分阶段感知颜色匹配，并且只有最低相似度和远距离第二候选分差同时达标才接受，避免相似贴纸误定位。每个源目录除 `BoardTitle.png`、`GameBoard.png` 外的碎片统一命名为小写三位编号 `piece_001.png`、`piece_002.png`……；已有 `.meta` 必须随 PNG 一起移动以保持 Prefab Sprite GUID 引用。
- 卡包开包特效已移除；后续重新接入前不得恢复旧 `Resources/Effects` 运行时路径。
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
- `Assets/Resources/Effects/`、`Assets/Scenes/EffectScene001.unity` 和 `PackItem.prefab/CardPackEffect` 已删除。
- 当前卡包列表、选中放大与开包等待阶段只使用静态封面，不加载 3D 或粒子资源。

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
                                                         -> 轻点卡包 / 横划 -> 预留撕包动画（当前直接完成）-> GameScene 入场
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
| `GameBoard` / `Piece01`... | `CardBagNNN` Prefab 内的棋盘和槽位 |
| `ActiveGroupOutline` | `GameBoard` 下运行时显示烘焙描边的根节点，按设置组合 `LevelOutline` 与 `StickerOutlines` UGUI Image |
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
| 卡包配置（`PackId`、`PackSize`、`StickerCount`、`ChapterId`、`BoardScale`、`AutoUpdate`） | `GameConfigRepository` 读取 `Resources/Configs/CardPacks.csv` | 只读 |
| 卡包生命周期 | `CardPackDataUtility` | `LocalData.db` 的 `CardPacks` 表 |
| 卡包当前拼图会话 | `CardPackDataUtility` | `LocalData.db` 的 `CardPackPuzzleProgress` 表 |
| 通用集合与键值存储 | `SqliteLocalStore` API | `LocalData.db` 的 `AppRecords` 表 |

- `GameConfigRepository` 加载并缓存任务和卡包配置。当前数据源为 `ResourcesGameConfigTextSource`，优先使用 `Resources.Load<TextAsset>`，失败时回退到编辑器磁盘路径。
- `CsvTable` 是统一 CSV 解析器，支持表头访问、引号字段和空行过滤；业务代码不得直接 `Split(',')`。
- `CardPacks.csv/BoardScale` 使用 invariant-culture 浮点数且必须大于零。GameScene 将其乘到当前 CardBag 根节点，使棋盘、槽位、描边和吸附坐标统一缩放。每个 Piece 的托盘生成比例为 `Min(配置后的棋盘目标比例, 原黑色托盘规则比例)`，不按整组宽高分支；拿起使用棋盘目标比例，所以按下时只会保持尺寸或放大。未正确吸附且不与 CardBag 棋盘相交时，Piece 以棋盘目标比例停留在桌面并限制在背景可见范围内；若贴图边界与棋盘相交，则恢复到本次拖拽起点。成功吸附后立即用 Prefab 对应原始 `Image` 替代拖拽 `SpriteRenderer`，确保已放置 Piece 与棋盘在同一 Canvas 层级共同缩放，接缝不随 `BoardScale` 放大。
- `JsonLocalStore` 读写整个文件的单一根对象，目前用于任务进度。
- `SqliteLocalStore` 在 `AppRecords` 中使用集合/键记录；卡包业务状态使用专用 `CardPacks` 表。
- `CardPackLifecycleState` 为 `Locked=0`、`Unlocked=1`、`InProgress=2`、`Completed=3`。首次进入 GameScene 时将未完成卡包标记为 `InProgress`，完成最后一组后标记为 `Completed`；重玩期间保持 `Completed`，不降级。
- SQLite `CardPacks` 表包含 `PackId`、`PackSize`、`LifecycleState`、`UnlockTime` 和 `CompletionTime`，不保留旧 `IsUnlocked`、`IsPlayed` 字段。解锁和完成时间使用固定格式的本地时间 `yyyy-MM-dd HH:mm:ss.fff`。`CompletionTime` 仅在首次进入 `Completed` 时写入，重玩不修改。
- SQLite `CardPackPuzzleProgress` 表包含 `PackId`、`PlacedPieceNumbersJson` 和 `UpdatedTime`。进入 GameScene 即创建会话，即使尚未放置 Piece 也保留空记录；正确吸附后按 `PieceNN` 的完整数字编号去重、排序并立即保存。桌面 Piece 的位置不持久化。完成整包并成功保存 `Completed` 后清除该会话。
- `CardPackDistributionUtility` 与 `CardPackDataUtility` 放在一起，负责章节选择、`R` / 持有数量判断、确定性锁定候选选择和首次完成发包。重玩根据 GameScene 启动时记录的生命周期快照跳过该尝试。
- 待发任务卡包权益保存在 SQLite `AppRecords` 的 `CardPackDistribution/Progress` 下，并按唯一 `TaskInstanceId` 去重。
- GameScene 在推进任务前先持久化任务权益，且仅在任务推进保存成功后尝试发放，避免任务进度保存失败时重复发包。
- MainScene 设置以集合/键 `GameSettings/Runtime` 保存在 `AppRecords`：音乐音量、音效音量和窗口模式。
- MainScene 辅助选项开关同样保存在 `GameSettings/Runtime`，字段为 `UsableOption1`、`UsableOption2` 和 `UsableOption3`。
- `UsableOption1` 是关卡描边开关，`UsableOption2` 是贴纸描边开关，两者新建设置时都默认关闭；`UsableOption3` 是高对比度并默认关闭。已持久化的用户选择优先。关卡描边关闭时 GameScene 保留现有当前阶段连接区域，打开时改为显示当前待拼组的完整合并外边界；贴纸描边关闭时不显示单块轮廓，打开时叠加当前组每块凹槽的独立轮廓。PanelUsable 的 `ImgContentBg` 按高对比度状态显示 `MainSetHigh1/2.png`；`ImgContentLine` 在描边全关、仅关卡描边、贴纸描边打开时分别显示 `MainSetLine1/2/3.png`，两项同时打开使用信息更完整的 `MainSetLine3.png`。GameScene 的 CardBag 根背景在高对比度关闭时使用 `UI/BasicUI/BgCardBoard1.png`，打开时使用 `BgCardBoard2.png`；运行时只替换根 `Image.sprite`，不改变 Prefab 布局。
- MainScene 和 GameScene 引用相同 `TaskItem.prefab` GUID。场景 Override 只定位根节点（`MainScene`：`10,508`；`GameScene`：`-6,455`）；子节点布局和视觉必须在共享 Prefab 中修改。
- 共享 TaskItem 子节点名称为 `TaskContent`、`TextProgress`、`ProgressMask`、`BagIcon` 和 `BagBg`。任务 UI 绑定代码应相对 TaskItem 实例解析这些名称，不得使用场景专属后缀。
- `TaskProgressUIUtility` 是两个 TaskItem 实例共用的运行时绑定。任务文案按任务类型和实际指定尺寸生成；`TextProgress` 显示当前值与任务实例目标值，可见 `ProgressMask` 宽度使用两者比值并限制在有效范围。`BagIcon` 始终使用共享 Prefab 中配置的固定 Sprite，运行时不得按任务奖励或卡包编号替换。
- MainScene 在 `Start` 时从持久化任务实例刷新 TaskItem。GameScene 结算使用不受 TimeScale 影响的时间：积分任务与结算分数同步滚动，贴纸和完成卡包任务在最终得分后单独滚动进度；任务奖励和下一任务生成在动画前持久化。
- GameScene 结算摘要将 `TaskBg2/TaskScore` 绑定到当局结算分数，将 `TaskBg2/TaskBagNum` 绑定到 SQLite 中生命周期为 `Completed` 的卡包数量；未完成的已解锁卡包和进行中卡包不计入，重玩不会重复计数。
- GameScene 进入时记录描边设置快照，点击 `BtnTips` 时记录提示使用，首个 Piece 成功放置时开始不受 TimeScale 影响的积分计时，RewardPanel 结算开始时冻结。
- MainScene `PanelSet/SliderMusic` 和 `PanelSet/SliderEffect` 是手工拼装的仿 Slider：根 Image 背景加 `SliderFill`、`SliderHandle` 子节点。运行时使用 `FakeSettingsSliderInput` 处理指针拖动、刷新视觉并保存数值。
- 不使用 `PlayerPrefs`。
- `LoadingScene.Start` 初始化 `JsonLocalStore`、`SqliteLocalStore`、`GameTaskUtility` 和 `CardPackDataUtility`。
- `Assets/Scripts/Model` 有意保持单层扁平目录。相关纯 C# 类型按以下方式合并：`GameManager` 位于 `GameDefine.cs`，CSV 解析类型位于 `GameConfigRepository.cs`，`JsonLocalStore`、`SqliteLocalStore`、`GameSettingsData` 和 `GameSettingsUtility` 位于 `LocalDataStore.cs`，积分类型和 `GameScoreUtility` 位于 `GameTaskUtility.cs`，`GameFontUtility` 位于 `GameCommonUtility.cs`。公开类型名和调用点保持不变。
- Model 当前保留8个脚本。`GameAnimationUtility`、`CardPackDataUtility`、`GameTaskUtility`、`GameConfigRepository`、`CardFxRuntimeUtility` 和 `GameDefine` 属于大型或独立模块，不为减少文件数量继续互相合并。
- MainScene 的卡包选择、居中放大、`PanelBagSelect`、开包输入和 2D 缺失资源回退逻辑保持不变。选中态在隐藏选中槽位后临时隐藏软件鼠标，生成不包含鼠标的四分之一分辨率背景截图并放大虚化，再立即恢复鼠标并叠加半透明 Panel 压暗；选中卡包使用更高排序层保持清晰。运行时保持动态封面、`600 x 680` 设计尺寸、选中复原和进入 GameScene 的现有交互节奏。
- 只有通过正常拆包进入 GameScene 时才播放一次入场：CardBag/棋盘从上方进入，PieceBoard 从下方进入，当前组 Piece 从棋盘附近错峰落入托盘，返回和提示按钮淡入；入场完成前屏蔽拖拽。对象在起始姿态保留两个渲染帧后才推进动画，单帧动画时间最多推进 `1/30s`，场景加载或首帧资源初始化卡顿不得吞掉入场过程。直接在编辑器启动 GameScene 保持即时初始化。
- 每组完成后先播放 `0.3s` 绿色正确放置反馈，再将 CardBag/棋盘位置、相机正交尺寸和托盘平滑切换到下一组布局；棋盘主体动画约 `0.72s`，新组 Piece 从棋盘区域用约 `0.38s` 错峰进入托盘。动画期间锁定拖拽、提示和“一键完成”，普通卡包同样使用该切组流程。
- GameScene 的 `BtnCompleteAllTest` 仅在 Unity Editor 和 Development Build 中运行时创建。点击后批量持久化当前 CardBag 全部 Piece 编号、显示完整棋盘并调用正式 `ShowRewardPanel()`；因此卡包生命周期、任务积分、奖励发放和完成数量都会产生真实本地测试数据。正式非 Development Build 不显示该按钮。
- `GameScene/BtnTips` 从当前组选择 Piece 编号最小的未完成碎片。目标碎片在托盘原位置左右抖动约 `0.8s` 后停止，棋盘对应 `GrooveRect` 使用 `HintDashedOutlineGraphic` 从 GPU 读取 Piece Sprite 的实际 Alpha 像素边界，沿真实累计轮廓长度生成固定 `20` 像素实线、基础间隔 `15` 像素、滚动速度 `60` 像素/秒的绿色滚动虚线；轮廓在当前 GameScene 内按 Sprite 缓存并在离场时清空，Physics Shape 只作为读取失败回退。再次点击按钮取消当前提示，成功放置、切组或结算时同样清理。一旦有效提示显示过，本局持续记为已使用提示。
- `CardBag001` 引导按实际三组拆成三个阶段，首次流程已有部分进度时从当前未完成组继续；活动拼图会话优先于历史卡包完成状态和教程完成记录，因此已完成卡包“重玩”中途退出后再次进入，也会从当前未完成组继续引导。只有整包完成并进入结算时才将 `Tutorial/CardBag001TutorialCompleted` 写入 SQLite `AppRecords`，中途退出不会提前完成教程；没有活动会话的已完成卡包普通进入不自动引导，通过 MainScene“重玩”确认进入时则从第1组重新播放完整引导。第1组 `Piece11` 为强引导：托盘变暗，突出目标 Piece，显示蓝色滚动虚线和原始尺寸的 `GuideArrow1.png`；箭头从碎片中心出现并循环移动指向目标凹槽，不做从小到大的缩放，并只允许拖动目标；拿起后文字和虚线保留，放错恢复焦点。第2组 `Piece21/22` 同时高亮、允许任意顺序放置，并从本阶段开始显示当前组烘焙关卡描边；不显示箭头或目标虚线，第一片放对后只刷新剩余 Piece 焦点。第3组 `Piece31-35` 恢复正常交互和 `BtnTips`，并介绍提示功能；`BtnTips` 在前两步保持隐藏。三步提示框分别位于左上、当前待拼凹槽整体边界上方和右上，从对应方向淡入；第二步位置随棋盘布局动态计算并限制在屏幕安全范围内。三步文字统一克隆场景 `GuideTip/TextTips`，运行时只替换文案，不覆盖编辑器设置的 TMP 字体、材质、字号、颜色、粗体、对齐或 RectTransform。第三步从场景 `GuideTip/Arrow` 读取 `GuideArrow2.png` 及编辑器布局，提示框入场后箭头淡入并循环向右上推进。该引导本身不算使用提示。
- 正确放置 Piece 后先将运行时贴图从松手位置以三次方减速曲线吸附到凹槽中心，默认时长 `0.18s`；抵达后才切换为棋盘 Image、播放 `0.3s` 绿色成功闪光并继续切组或结算。吸附期间不得开始新的拖拽、提示或一键完成操作。
- RewardPanel 先从 `0` 滚动到基础分，再依次展示本局实际生效的“未使用提示”“关闭关卡描边”“关闭贴纸描边”和“快速完成”加成，最后显示最终得分；显示顺序和动画不改变项目既有加成比例、任务进度或发包结果。
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

共享尺寸图标为 `UI/PackImages/PackSize_1.png` 到 `PackSize_7.png`，对应 `CardPackSize` 数值（`XS=1` 到 `XXXL=7`）。`PackItem` 必须包含名为 `PackCover` 和 `PackSize` 的 Image 子节点；MainScene 在运行时设置两者 Sprite，并根据编辑器封面尺寸缩放尺寸图标。

`PackItem/PackShadow` 是渲染在 `PackCover` 后方的同级 Image。MainScene 读取运行时可读封面贴图，将 Alpha 缩小到 `240 x 272` 显示尺寸，并执行三次可分离 Box Blur，水平半径 2、垂直半径 5。缓存阴影 Sprite 尺寸为 `256 x 344`、偏移为 `(0,-20)`，使投影只向下而不是向右。水平/垂直内边距为 `8/36` 像素，阴影颜色 `#1f292d`，最大 Alpha `0.52`。MainScene 销毁时释放生成的阴影 Sprite 和 Texture。`PackSize` 保持在两张图片上方。

1. 场景中只保留一个模板对象：`Package001`。
2. 在 `CardPacks.csv` 增加一行（`PackId`、临时 `PackSize`、临时 `StickerCount`、`ChapterId`、正数 `BoardScale`、`AutoUpdate=1`），随后执行配置更新工具按实际片数同步 `PackSize`、`StickerCount` 和 `BoardScale`。需要手工固定这三个值时将该行 `AutoUpdate` 改为 `0`。
3. 在 `UI/PackImages/` 下按 `PackIconNNN.png` 命名增加对应封面。`GameDefine.FormatPackImagePath` 将 PackId `1` 映射到 `UI/PackImages/PackIcon001.png`。
4. 通过 `CardPackDataUtility` 将生命周期写入 SQLite `CardPacks` 表。
5. 卡包列表和选中态直接使用对应静态封面；不为每个卡包创建 3D 展示资源。

### 拼图

1. 在 `Assets/Resources/CardBagPrefabs/` 下创建 `CardBagNNN` Prefab，`NNN` 与 `PackId` 一致。
2. Prefab 内放置一个名为 `GameBoard` 的子对象。
3. 在 `GameBoard` 下用 Image 对象添加分组碎片：第 1 组使用 `Piece11`、`Piece12`...；第 2 组使用 `Piece21`、`Piece22`...；第 3 组使用 `Piece31`...。分组号为 `PieceNN / 10`，按升序处理。
4. 源贴图放在 `Assets/UI/CardBags/CardBagNNN/`，按分组命名，例如 `Pieces11`...`Pieces14` 和 `Pieces21`...`Pieces25`。
5. 不使用 `PieceGroup` 父节点；分组只读取 `Piece` 后面的数字。
6. 不创建 Package JSON；运行时数据来自已加载 Prefab 的 Image。
7. 新增或修改 CardBag 后，执行 **Puffies -> Puzzles -> Bake Outline Masks**。烘焙器优先使用 `GameBoard.png` 的透明挖空 Alpha 作为最终拼图外边界，并使用已完成 Piece 的 Alpha 作为后续组接触边；GameBoard 没有有效挖空时回退到全部 Piece Alpha 并集。每组生成三张同尺寸资源：`GroupNN.png` 是默认连接区域；`GroupNN_Level.png` 是当前组 Piece Alpha 并集的完整外边界；`GroupNN_Stickers.png` 是当前组每块 Piece Alpha 边界的并集。默认连接图第 1 组只包含自身最终拼图外边界，后续图只包含当前组最终外边界及其与低编号已完成组的接触边；同一描边像素按组顺序只由最早需要它的阶段认领。接触边和最终外轮廓均使用圆形最近距离与局部边界法线判定归属，切线方向的邻近不得延长端点；两类线在交汇处分别于对方边界 `24px` 范围外结束。
8. `GameScene` 将烘焙的 `#3f423e` Sprite 作为不可交互的 `GameBoard` 子 Image 显示。关卡描边关闭时加载 `GroupNN.png`，打开时替换为 `GroupNN_Level.png`；贴纸描边打开时额外叠加 `GroupNN_Stickers.png`。不要在 Prefab 中手工制作描边对象。
9. 缺少生成 Sprite 时，运行时记录制作警告，并在无描边情况下继续游戏。交付前重新运行烘焙器。
- 创建一组碎片时按编号从左向右排列。Piece 首次离开托盘时只允许仍在托盘且编号靠后的 Piece 沿 X 前移，不得刷新前序 Piece、桌面 Piece 或任何剩余 Piece 的 Y/缩放；先拿队尾 Piece 时其余 Piece 不移动。未吸附 Piece 仅可停留在 CardBag 棋盘范围外的桌面；与棋盘相交但未命中自身凹槽时恢复到本次拖拽起点。桌面 Piece 与黑色托盘水平相交且垂直重叠达到自身当前高度 `50%` 时，松手后恢复托盘尺寸并按编号自动重排。未吸附松手时空托盘恢复为回收目标，最后一块正确吸附后仍进入切组或结算。

#### 无 JSON Prefab 批量生成

- 菜单 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images** 打开批量窗口，扫描 `UI/CardBags/` 下严格匹配 `CardBagNNN` 的一级目录。
- 每个卡包硬性需要 `CardBagNNN/GameBoard.png`、`Previews/CardBagNNN.png` 和至少一张合法 Piece PNG；缺失项会显示在列表中并禁止选择。
- 旧 `background_base.png` 仅用于兼容迁移：当 `GameBoard.png` 不存在时，扫描器通过 `AssetDatabase.MoveAsset` 自动改名并保留 Meta/GUID；两者同时存在时不覆盖目标文件。
- `BoardTitle.png` 是标准资源但采用软校验。缺失时列表显示警告，仍允许生成不含 `BoardTitle` 节点的 Prefab。
- `UI/CardBags/Previews/CardBagNNN.png` 是完整拼图和 Piece 定位参考图，不作为运行时 Prefab Sprite；它必须与 `GameBoard.png` 画布尺寸一致。
- 生成器利用 Piece PNG 保留的原始裁切 RGB，默认在 Preview 完整图中做像素匹配，Piece Alpha 继续作为运行时形状。Preview 因调色板量化、分割线或其他处理无法通过置信度校验时，再尝试 `GameBoard.png`；GameBoard 若已挖空或未保留完整 RGB，回退可以继续失败，不要求所有卡包都维护第二套完整定位图。
- 第二轮不透明像素匹配排除 Alpha 轮廓内侧 `1px`，避免 Preview 分割线或相邻 Piece 覆盖导致正确位置被边缘像素否决。Preview 和 GameBoard 各自沿用同一校验：常规最低匹配率为 `98%`；低于该值时，只有匹配率至少 `90%` 且所选精确 RGB 锚点在当前参考图中唯一才允许生成并记录警告，重复锚点或低于 `90%` 继续报错。两张参考图都失败时错误同时包含两边原因。
- 感知颜色匹配会验证透明像素轮次与不透明像素轮次中“精确 RGB 锚点唯一”的定位结果；该位置本身达到 `78%` 感知匹配率时才加入候选种子，避免重复图案中的偶然同色像素误导定位。首轮 `6px` 感知匹配仍不通过时才使用 `1px` 逐像素网格回退，覆盖细线和高对比图案偏移一个像素即失配的情况。逐像素回退优先执行最低 `78%`、候选差值至少 `1.5%` 的颜色校验；颜色受整批调色影响时，只有颜色仍达到 `65%`、原坐标的 RGB 边缘梯度达到 `85%`，且该结构区域领先远端候选至少 `3%` 才允许生成。结构校验只验证颜色定位得到的原坐标，不使用结构最高点移动 Piece。
- GameBoard 回退的首个 Piece 达到至少 `99.5%` 且位置唯一后，本卡包后续 Piece 统一使用 GameBoard，日志明确记录参考图。参考图颜色索引保存颜色次数和首次像素下标；唯一颜色锚点直接计算候选位置，不遍历整张画布。
- `PieceNN.png` 或 `PiecesNN.png` 中的 `NN` 直接成为对象编号；未改名的 `piece_###.png` 依次生成 `Piece001`、`Piece002` 等未分组对象，不自动推断游戏分组。
- 生成结构为 `CardBagNNN/GameBoard/BoardTitle` 和 `CardBagNNN/GameBoard/PieceNN`；棋盘标题与全部碎片统一归属 `GameBoard`。
- 窗口默认只选择资源完整且尚无 Prefab 的卡包。选择已有 Prefab 时显示 `Overwrite`，执行前必须确认；覆盖会替换已有层级和手工 Piece 分组。
- 批量生成逐个隔离失败并汇总结果，只负责创建 Prefab，不自动烘焙描边。完成手工 Piece 分组后再执行 **Bake Outline Masks**。
- `Piece001` 这类带前导零的三位顺序名是制作中间状态。Prefab 中只要仍有任一此类节点，描边烘焙器将整包跳过，避免超过99片时把 `Piece100` 等后续顺序节点误判为正式分组；卡包没有正式分组时删除对应旧描边目录。
- 当前 CardBag017 为 `1316 x 1316`、37 片，已完成正式分组并生成 5 张描边蒙版。CardBag022 仍使用 `Piece001` 开始的顺序名称，完成手工分组并改为正式 `PieceNN` 后才能烘焙描边并进入 GameScene 测试。

### 拼图描边渲染

- 拼图描边由 `PuzzleOutlineBakerEditor` 离线生成，并通过 Unity UGUI `Image` 渲染。
- 关卡描边与贴纸描边默认都关闭；默认状态仍显示现有连接区域，不等于完全隐藏全部描边。
- 烘焙器在单个 CardBag 内累计已输出像素；后续组删除与前序组重叠的描边像素，防止接触边在阶段交界处沿旧外边界多画。
- 边界归属除距离外还校验目标组位于边界的正确法线方向；最终外轮廓要求目标组位于轮廓内侧，已完成组接触边要求当前组位于旧组边界外侧。
- 后续组外轮廓靠近已完成区域时提前截断，已完成组接触边靠近最终外轮廓时对称截断；交汇处允许保留小间隔，不能以连接线跨入贴纸空白区域。
- 项目没有运行时描边 Shader、Renderer Feature 或第三方描边包。
- 描边加载与拼图交互保持隔离；缺少描边不得阻止可拖拽碎片创建。

### 卡包展示与开包表现

- 首页列表的卡包主体使用 `Assets/UI/PackImages/PackIconNNN.png` 静态图，`PackItem.prefab` 不嵌套卡包特效 Prefab。
- 选中态使用独立 Screen Space Overlay `Image` 显示同一张静态图，目标尺寸为 `600 x 680`；选择面板、背景虚化、返回、拍照和重玩确认继续沿用现有流程。
- 点击玩后切换到 `BgGame` 开包舞台并等待玩家轻点或横划。撕开的动画接口保留为后续需求，当前有效操作不播放特效，下一帧直接进入 `GameScene`。
- `Assets/Resources/Effects/`、`Assets/Scenes/EffectScene001.unity`、专用 `CardPackListUnlit.shader` 以及 MainScene 的特效 Skybox/Directional Light 已删除。
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
| Puffies -> Canvas -> Apply Design Resolution | 应用 Canvas 设计分辨率 |
| Puffies -> Fonts -> Setup Default Chinese Font | 设置中文字体 |
| Puffies -> Puzzles -> Bake Outline Masks | 为每个 CardBag Prefab 重建各分组外边界描边 |
| Puffies -> Puzzles -> Generate CardBag Prefabs From Images | 扫描完整背景和透明碎图，选择并批量生成 CardBag Prefab |
| Puffies -> Card Packs -> Update Pack Sizes From Piece Counts | 扫描 CardBag 源资源的碎片 PNG 数量并同步更新 `CardPacks.csv/PackSize`、`StickerCount` 与 `BoardScale`；跳过 `AutoUpdate=0` 的行 |

---

## 10. 已弃用

- `Assets/ArtRes/`、`Assets/Configs/`
- `Resources/Config/Package001.json` 及 JSON 拼图配置流程
- `Tools/*.ps1` 下的一次性迁移脚本
