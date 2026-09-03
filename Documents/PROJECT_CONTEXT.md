# 项目上下文

Unity **2022.3** / Built-in Render Pipeline 项目，使用 Linear 色彩空间和 Built-in Forward 渲染。核心循环：打开卡包 -> 拖放拼图 -> 任务奖励。本文档是需求、场景、数据、资源、构建规则和命名的稳定项目参考。

`GraphicsSettings.m_CustomRenderPipeline` 必须为空，各 Quality 档位不指定 SRP Asset。URP `14.0.12` Package 与 `Assets/Settings` 中的旧 URP Asset 暂时保留为迁移回退资源，但不是当前激活管线；新增运行时代码和 Shader 不得依赖 URP API。

当前工作状态记录在 [CURRENT_TASK.md](CURRENT_TASK.md)，工作流规则记录在 [WORKFLOW.md](WORKFLOW.md)，已确认的长期游戏设计规则记录在 [GAME_DESIGN_REQUIREMENTS.md](GAME_DESIGN_REQUIREMENTS.md)。

---

## 1. 功能需求

### 核心循环

1. `LoadingScene` 初始化本地数据、任务配置、卡包配置和持久化存储。
2. `MainScene` 根据解锁状态显示可玩的卡包。
3. 点击已解锁卡包进入选择页；完整彩色卡包继续进入开包舞台，彩色撕开进行中和确认重玩的灰色撕开卡包则直接进入 `GameScene`。
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
| MainScene | 根据 `CardPacks.csv` 与 SQLite 状态刷新卡包列表；每页固定 6 列 x 3 行显示 18 个卡包。列表和选择页都使用绑定 `Main Camera` 的 `Screen Space - Camera` Canvas。列表显示分为完整彩色、第一组完成后的彩色撕开加本关碎片、无活动会话的灰色撕开完成态；选择页复制当前完整 `PackNode` 并从列表位置放大到 `600 x 680`。只有完整彩色卡包进入 `BgGame` 开包舞台并播放 `CardPackOpeningModel_001-006` 与 `fx_chai_w_001`；彩色撕开点击“玩”直接继续游戏，灰色撕开确认重玩后清空进度并直接进入游戏。保留拍照、重玩确认、Rank、Achieve、Menu、Steam 愿望单、Discord 和 QQ 群入口。 |
| GameScene | 根据选中 PackId 加载 `CardBagNNN` Prefab，并读取 `CardPacks.csv/BoardScale` 缩放棋盘；按照 `PieceGGII` 四位数字命名组织拼图分组；从正常开包流程进入时播放棋盘、托盘和当前组 Piece 入场；每次正确放置 Piece 后立即持久化，重新进入时恢复已放置 Piece 并从首个未完成分组继续；全部完成后显示 RewardPanel；Editor 和 Development Build 在 `BtnTips` 左侧提供“一键完成”测试按钮 |
| RankScene | 仅占位；首个 Demo 不包含排行榜后端功能。当前模拟列表前三名的 `RankBg` 分别使用原生 `1646 x 148` 的 `RankCellBg_1.png`、`RankCellBg_2.png`、`RankCellBg_3.png`，第四名以后使用 `1636 x 136` 的 `RankCellBg.png`；`RankItem` 根高度为 `148`，列表纵向间距为 `5`，条目中心步距为 `153` |
| AchieveScene | 当前显示 20 条模拟成就，前 5 条已达成、后 15 条未达成；接入 Steam 后替换数据源。成就网格固定为 6 列，单元尺寸 `240 x 332`，横纵间距均为 `40` |

MainScene 卡包选中页与 GameScene 结算页共用 `Assets/Prefabs/PackPhotoItem.prefab` 和 `CardPackPhoto`。两个场景都必须保持一个名为 `PackPhotoItem` 的根级 Prefab 实例；拍照统一生成 `1024x1024` PNG，保存到桌面并命名为 `游戏名-YYYY-MM-DD-BagId.png`，随后在面板 `Photo` 中预览，点击 `BtnOK` 关闭。通用组件负责闪光、离屏渲染、保存、预览、根 Animator 时序和临时纹理释放，场景控制器只传入当前 PackId 并控制各自按钮状态，不得复制第二套拍照实现。离屏照片中的完整拼图不得显示 `GameBoard/BoardTitle` 的游戏内投影，拍照临时副本必须清除其投影材质、UI Shadow 和网格扩边组件，但不得修改关卡 Prefab、GameScene 棋盘或 Piece 投影。全屏白色闪光使用当前主相机下独立的固定 `16:9` Canvas，排序值不得超过 Unity 支持的 `32767`；每次播放前重新绑定当前相机，激活并强制刷新后至少等待一帧再开始透明度动画，确保 MainScene 和 GameScene 首次拍照均可见。闪光与图片生成期间根 Animator 必须停止；预览显示时从第 0 帧单次播放 `PackPhoto`，由美术动画控制 `TaskContent` 显示后消失以及 `BtnOK` 出现，代码只负责在动画完整结束前禁用按钮交互。

所有场景常规鼠标图标为 `UI/BasicUI/ImgHand_1.png`。GameScene 悬停当前可拖 Piece 时切换 `ImgHand_2.png`，按住左键拖拽 Piece 时切换 `ImgHand_3.png`；松开、结算或离开 GameScene 后恢复常规图标。三张资源随 `BasicUI` 同步到 Player 的 `StreamingAssets/UI/BasicUI`。运行时使用 `CursorMode.ForceSoftware`，以 `2560x1440` 设计分辨率的宽高缩放较小值匹配固定 `16:9` 有效视口，分别等比重建三张光标纹理并同步缩放热点；窗口尺寸变化时自动刷新，不能把三种不同比例的源图压入固定画布。

### 数据与奖励需求

- 任务配置来自 `Resources/Configs/TaskConfig.csv`。
- 卡包配置来自 `Resources/Configs/CardPacks.csv`。
- `CardPacks.csv/StickerCount` 紧跟在 `PackSize` 后面，记录卡包贴纸数量。`PackSize` 按该数量确定：`<20=XS`、`20..30=S`、`31..55=M`、`56..85=L`、`86..125=XL`、`126..170=XXL`、`>170=XXXL`。配置更新工具始终按实际片数更新 `StickerCount` 和 `PackSize`；`AutoUpdate=1` 时同时将 `BoardScale` 更新为：`XS=0.75`、`S=0.78`、`M=1.10`、`L=1.30`、`XL=1.00`、`XXL=1.15`、`XXXL=1.30`，`AutoUpdate=0` 时保留手工 `BoardScale`。工具只统计 `Assets/UI/CardBags/CardBagNNN` 顶层的标准碎片名 `piece_NNN.png`，不统计 `BoardTitle.png`、`GameBoard.png` 或其他 PNG。
- `CardPacks.csv` 的字符串列 `Series` 位于 `BoardScale` 与 `AutoUpdate` 之间，默认留空并且只能手工维护。某行填写 `15|18` 时，以该行 `PackId` 为链首建立 `当前包 -> 15 -> 18`；后续包只有在完整前置链都为 `Completed` 后才进入现有发包候选池，仍继续受章节、持有数量和其他常规发包规则限制。系列同时限制任务奖励、首次完成奖励和直接解锁 API，不会自动发包。不存在的 PackId、默认首包作为后续包、冲突前置、重复或循环链会使卡包配置加载失败。
- `CardPacks.csv` 最后一列 `AutoUpdate` 只允许 `0` 或 `1`，默认值为 `1`。配置更新工具遇到空值会补为 `1`；该字段只控制 `BoardScale`，设为 `0` 时保留手工棋盘缩放，但仍按实际碎片数更新 `PackSize` 和 `StickerCount`。无论 `AutoUpdate` 取值如何，工具都不修改已有 `Series` 内容；缺少该列时只在 `AutoUpdate` 前补一列空值。
- 任务配置固定为三个模板：`TaskType=1` 完成任意拼图包并累计结算分数，`TaskType=2` 从任意拼图包中收集贴纸数量，`TaskType=3` 完成指定尺寸的卡包数量。三类任务都只在完整完成一个符合尺寸要求的卡包后结算一次；贴纸任务按该卡包的全部 Piece 数量累计。
- `SizeMode=0` 表示任意尺寸；`SizeMode=1` 从模板 `SizePool` 与玩家当前可玩卡包尺寸的交集中随机指定一个尺寸。三个模板按 `Weight` 加权随机，生成下一任务时严格排除与当前任务相同的 `TaskType`；任务 3没有可玩的 S/M 卡包时不进入候选池。
- 积分任务目标按 `150 -> 200 -> 250 -> 300` 顺序循环，贴纸任务目标按 `45 -> 60 -> 80` 顺序循环，两类任务使用互不影响的持久化游标。完成卡包任务的目标数量从 `2|3` 随机，指定尺寸从 `S|M` 随机，两项独立选择。
- 正常首次完成以卡包原始基础分开始（XS 60、S 80、M 100、L 120、XL 140、XXL 160、XXXL 200），将所有符合条件的百分比加成相加后统一相乘，并向上取整。进入 GameScene 时已经为 `Completed` 的卡包按重玩结算：可得基础分仅为原始基础分的 `10%`，但全部加成仍按原始基础分计算，即 `ReplayFinalScore = Ceil(OriginalBaseScore * 10%) + Ceil(OriginalBaseScore * TotalBonusRate)`；进行中会话的继续游戏不属于重玩。
- 分数加成为：未点击 `BtnTips` +5%；关闭 MainScene `Toggle1` 关卡描边 +2%；关闭 `Toggle2` 贴纸描边 +5%；完成时间 <=15 / <=30 / <=60 秒分别 +3% / +2% / +1%。
- 完成任务后发放奖励并随机生成下一个任务实例。每个任务实例使用独立递增的 `TaskInstanceId`，同一个模板可以在后续再次出现。
- 完成任务必定创建一条持久化的新卡包权益。任务奖励不受 `R/H` 章节阶段门槛限制，只在全局可玩卡包数 `Unlocked + InProgress` 已达到最大值 `6` 时保持待发；出现空位后的任意一次成功结算都会继续重试。卡包首次完成时执行一次确定性的阶段门槛自然发包尝试。重玩已经 `Completed` 的卡包不执行首次完成发包；任务奖励与该限制独立，任务是否累计由 `CountReplay` 控制，重玩中达成任务时保底权益写入待发队列并正常推进任务，随后在同一结算按任务奖励上限尝试兑现。
- 卡包发放使用 8 个玩家不可见的内部章节，总量约 150 个卡包，平均每章 18.75 个。章节限制可选的锁定卡包奖励池，但不显示在 MainScene 或其他玩家界面。准确 PackId 分配和章节推进规则仍待确认。
- 内部章节阶段使用 `R` 表示当前章节仍为 `Locked` 的卡包数：初期 `17..9`、中期后段 `8..3`、末期 `2..1`。持有可玩数量为 `Unlocked + InProgress`，各阶段目标约为 `5-6`、`2-3` 和 `1`。章节超过 18 个卡包时，`R>17` 的额外范围同样属于初期。
- 自然结算发包门槛：`R>=9` 时允许 `H<=5`；`R=8` 时允许 `H<=3`；`R=7..3` 时允许 `H<=2`；`R=2..1` 时允许 `H<=1`。该阶段门槛不用于任务奖励。任务奖励在全局 `H<6` 时直接发放，`H>=6` 时保持待发；章节候选和系列前置规则继续生效。待发任务奖励先处理，自然结算随后读取更新后的状态继续判定，两个来源可在同一轮结算中同时发包。RewardPanel 入场时 `TaskBg2` 与当前 `TaskItem` 从屏幕上方同步落到编辑器位置；初始红色卡包数使用保存当前完成状态前的 `Completed` 数量。积分、加成和任务进度结束后，新包首次完成才播放红色 `+1` 上飘及数字滚动，重玩不播放。实际发到首次完成奖励，或本轮任务奖励权益已经成功入队时，`ImgBagBg` 从屏幕下方滑入；展示槽位固定为首次完成奖励在前、任务奖励在后，前者由默认 `ImgBag` 从 `0` 放大到 `1.2` 后回弹，任务奖励无论位于居中第一槽还是双奖励右侧第二槽都从 `TaskItem/BagIcon` 飞入对应槽位。任务奖励结算展示以“本轮权益成功入队”为准，不要求本轮已经分配真实 PackId；达到全局上限、没有符合条件的锁定候选或系列前置未满足时，权益保持待发但动画仍播放。只有已经分配真实 PackId 的奖励参与点击完成后的跨场景列表飞入。单奖励居中，双奖励左右排列，默认 `ImgBag` Sprite 不替换为发放卡包封面。
- 积分任务超过目标的分数保存为待结转值；即使中间出现贴纸或完成卡包任务，也会在下一个积分任务生成时恢复为该任务的初始进度。
- 卡包生命周期保存在 SQLite `CardPacks` 表中，状态为 `Locked`、`Unlocked`、`InProgress` 或 `Completed`。
- 当前拼图会话保存在 SQLite `CardPackPuzzleProgress` 表中；记录存在表示该卡包有一局可继续，已正确放置的 Piece 编号即时保存，整包完成后删除记录。
- 已完成卡包确认重玩时，MainScene 清除上一局进度并创建新的空 `CardPackPuzzleProgress`，确保本次重玩从空棋盘开始。进入游戏后，每片正确拼入都会立即保存；无论第一组只完成部分、第一组已经完成，还是已进入后续组，返回首页都保留当前会话，再次进入时恢复全部已拼 Piece。首页表现只按第一组是否完整完成区分：未完成第一组显示完整彩色卡包，完成第一组显示彩色撕开和本关碎片。
- MainScene 卡包排序分四层：第一层为本游戏进程新发放的卡包，按解锁时间倒序；新包从 `Unlocked` 进入 `InProgress` 但第一波未完成时仍保留第一层位置。第二层为非本次新发放、第一波已完整完成且整包未完成的 `InProgress`，按解锁时间升序。第三层为其他普通 `Unlocked` 与第一波未完成的旧 `InProgress`，统一按解锁时间升序，因此旧卡包开始游戏但未打完第一波不会改变位置。第四层为 `Completed`，按首次完成时间倒序，最新完成靠前、越早完成越靠后。PackId 是时间并列或无效时的确定性依据。本进程新发放标记不持久化且不会被列表读取消费，只在进程初始化时清空；整包完成后立即进入第四层。`Completed` 重玩不改变生命周期和首次完成时间，因此位置不变。系列折叠发生在真实 PackId 排序之后，新解锁 A02 会以 A02 的第一层优先级带动 A01+A02 整组置顶；每日挑战优先级暂缓实现。
- MainScene 对 `Series` 链执行系列槽位折叠：同系列全部已解锁成员只占一个网格位置，当前最高已解锁 Vol 是前层，上一已解锁 Vol 是后层。前后层必须分别实例化完整 `PackItem.prefab`，并分别按自己的 PackId 和进度刷新封面、`PackBg`、撕口蒙版、完成态材质、`PackSize`、`PackVol` 与进行中碎片；不得再把后层实现成前层内部的单张 `PackCover2` 图片。两张卡包统一使用普通列表的标准尺寸，根节点不允许额外缩放；后层只允许相对前层改变位置、Z 轴 `+7°/-7°` 旋转和更低层级，中心对齐时应由旋转自然露出上下左右各角，不读取 Shader `_PaddingY`，不修改 Pivot，不做尺寸补偿。系列槽的前后卡包不得各自播放呼吸动画：运行时必须关闭两个内部 Animator，并将两套完整视觉挂到唯一的 `SeriesAnimationRoot` 下，由父节点共用一个 Animator 播放 `PackAniBreath`，保证相对位置和旋转不变、整个槽位同步运动。Vol2 起显示对应 `PackVolN.png`。点击系列槽打开编辑器搭建的 `PanelBagVol`，按链顺序展示全部已解锁 Vol，初始居中最高已解锁 Vol；进场时主卡包先单独放大，底部操作按钮在放大后半段上滑，主卡包到位并短暂停顿后相邻 Vol 才从其背后展开，分页圆点随侧卡展开延迟淡入。拖动或左右按钮切换时使用 `PackLeft/PackCenter/PackRight` 的编辑器位置和缩放插值，松手在 `0.25s` 内吸附，分页圆点和操作按钮随当前 Vol 更新。展开后的所有轮播卡包继续播放 `PackItem.prefab` 自带的 `PackAniBreath`；程序不覆盖动画位移、缩放、速度或相位，只在末帧把卡包本体、`PackNode` 和封面的 Z 轴旋转归零。居中卡包继续复用现有开包、继续游戏、重玩确认、拍照和返回逻辑，普通卡包仍进入 `PanelBagSelect`。分页网格按完整六列宽度计算固定左右边距并从 `UpperLeft` 排列，因此满页居中且末页从相同第一列开始。
- 系列槽进入 `PanelBagVol` 时，主卡继续使用现有 `0.4s` 弹起放大。点击瞬间必须隐藏列表后层对应的真实 Vol 卡，并直接设置 Z 轴 `0°`、左侧卡位最终缩放和主卡最终中心位置；主卡展开期间不得显示后层卡的旋转、缩放或移动。主卡完全展开并经过现有 `0.15s` 停顿后，后层卡才从主卡背后显示，保持尺寸不变，只沿 X 轴从中心滑向左卡位；这是后层卡唯一的进场动画。
- `PanelBagVol` 第一次使用前必须完成一次不可见预布局：临时激活面板，在同一帧强制重建 Panel 与 `PackCarousel` 的 Layout 后恢复隐藏；每次动态创建系列卡后再次执行，再读取 `PackLeft/PackCenter/PackRight` 世界矩形。用于复制选中卡包并执行展开/关闭插值的独立 `SelectedCardPackCanvas` 根节点必须持续激活，使 `CanvasScaler` 从初始化开始维持稳定坐标系；不可通过禁用整个 Canvas 隐藏选中卡包，只切换其 `SelectedCardPackImage` 子节点。不得依赖面板或选中 Canvas 曾经显示过一次才获得正确动画坐标。
- MainScene 所有生命周期状态均使用 `UI/PackImages/PackIconNNN.png` 静态封面。活动会话只有在对应 `CardBagNNN.prefab` 的全部 `Piece01II` 均已正确拼入后，才显示彩色撕开状态和最多 3 片本关贴纸；第一组完成前仍显示完整彩色卡包。没有活动会话的 `Completed` 显示撕开，并切换为美术配置的完成态封面及标签材质；`PackSize` 与 `PackVol` 共用尺寸标签的普通/完成态材质，完成后同步置灰，其余状态保持彩色。所有撕开状态都显示 `PackBg`，进行中贴纸位于 `PackBg` 上方、`PackCover` 下方。程序只切换材质引用，不覆盖美术材质中的灰度、颜色、亮度或对比度参数。选中放大页复制列表当前完整 `PackNode`，必须继承撕口、`PackBg`、完成态材质、尺寸标签和进行中贴纸；复制后还必须按真实 PackId 重新确认 `PackSize`，并按真实系列序号显示 Vol2 及以上的 `PackVolN`，不能因列表裁切或临时显隐状态漏掉完整新包的标签。
- `PackItem/CardPackEffect/PackNode` 的列表视觉顺序为 `PackBg`、运行时可选的 `ProgressPieces`、`PackCover`、`PackSize`、`ImgLight`。`PackBg` 默认关闭，仅在撕开状态启用；运行时按封面从 Prefab 原始尺寸到列表尺寸的比例同步缩放，并与封面、尺寸图标和进行中贴纸统一执行可见区域及面板显隐。
- 任务实例、当前进度、下一个实例号、积分目标循环游标、贴纸目标循环游标和待结转积分保存在 JSON 根对象 `TaskProgressData`。
- 业务进度不得使用 `PlayerPrefs`。

### 内容扩展需求

- 新卡包沿用唯一 `Package001` 模板；`MainScene` 在运行时动态创建列表项。
- 新拼图通过在 `Resources/CardBagPrefabs/` 下新增 `CardBagNNN` Prefab 实现；每个已分组 Prefab 包含 `GameBoard` 和 `PieceGGII` 节点，不创建 Package JSON。
- 编辑器批量生成器可扫描 `CardBagNNN` 资源目录，使用完整的 `Previews/CardBagNNN.png` 与透明 Piece PNG 进行像素匹配，并以 `GameBoard.png` 作为运行时棋盘底图批量创建 Prefab，不依赖 Package JSON 或 `unity_layout.json`。生成器优先使用精确 RGB 锚点；切图与预览几何一致但存在导出色差时，回退到分阶段感知颜色匹配，并且只有最低相似度和远距离第二候选分差同时达标才接受，避免相似贴纸误定位。每个源目录除 `BoardTitle.png`、`GameBoard.png` 外的碎片统一命名为小写三位编号 `piece_001.png`、`piece_002.png`……；定位完成后自动按空间生成正式 `PieceGGII` 分组。已有 `.meta` 必须随 PNG 一起移动以保持 Prefab Sprite GUID 引用。
- 新卡包撕包特效位于 `Resources/Effects`：六套 `CardPackOpeningModel_001-006` 共用 `CardPackAnimation.controller`，`fx_chai_w_001` 提供撕口横向光效。列表和选中放大仍使用静态图，只有 `BgGame` 内收到轻点或有效横划后才加载并播放这些资源。
- `BgGame` 等待撕包输入必须从居中卡包的屏幕矩形内开始。短距离松手按点击处理；指针保持在卡包矩形内并向任意方向移动至少 `18` 屏幕像素或卡包短边 `6%` 时记录为有效滑动，但只有鼠标左键或触摸抬起且结束点仍在卡包内时才触发撕包。卡包外起手或滑动过程中离开卡包范围均不触发；点击与滑动在抬起时共用同一个撕包完成入口。
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
- 当前卡包列表和选中放大阶段使用静态封面；`BgGame` 等待输入阶段在静态封面上叠加从 `PackItem/PackNode` 克隆的 `ImgLight` 循环滑光提示，但不创建 3D 撕裂模型或正式撕包粒子。收到有效开包输入后立即移除提示层，再创建 3D 撕裂模型和粒子资源。

---

## 3. 场景与导航

```text
LoadingScene（2.5s，TextLoading 0% -> 100%）
  -> MainScene
      -> BtnRank     -> RankScene     -> BtnReturn -> Main
      -> BtnAchieve  -> AchieveScene  -> CloseBtn  -> Main
      -> BtnWishList -> Steam 商店愿望单页面
      -> BtnDiscord  -> Discord 邀请链接
      -> BtnQQ       -> QQ 群 `1079431440`
      -> BtnMenu     -> PanelMenu     -> BtnClose / BtnReturn -> 关闭菜单
                    -> BtnSet        -> PanelSet -> BtnClose / BtnReturn -> 关闭设置
                    -> BtnUsable     -> PanelUsable -> BtnClose / BtnReturn -> 关闭辅助选项
                    -> BtnData       -> PanelSave -> BtnClose / BtnReturn -> 关闭存档面板
      -> 已解锁卡包运行时列表项 -> 居中放大 + 无染色高斯模糊背景 + PanelBagSelect
                                      -> BtnPlay/重玩 -> BgGame 开包舞台
                                                         -> ImgLight 循环滑光 -> 轻点卡包 / 横划 -> 真实卡包撕裂 + 横向光效 -> GameScene 入场
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
| `BtnWishList` | 使用系统浏览器打开 `https://store.steampowered.com/app/4906510/?utm_source=InGame` |
| `BtnDiscord` | 使用系统浏览器打开 `https://discord.gg/sfmNFEF5ec` |
| `BtnQQ` | 使用系统浏览器打开配置的 QQ 群链接，目标群号 `1079431440` |
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
| `PanelSave` | MainScene 三档本地进度选择弹窗；每次运行只使用当前活动档位 |
| `PanelSave/BtnSave1~3` | 选择待使用或待删除的存档；点击后立即刷新三档选中/未选中文字样式。空档仍显示左侧 `1/2/3`，右侧居中显示“新游戏” |
| `PanelSave/BtnContinue` | 保存活动档位并重新进入 `LoadingScene`，按所选档位重新加载 |
| `PanelSave/BtnDelete` | 删除当前选中档位；空档时隐藏 |
| `PanelSave/BtnClose` / `PanelSave/BtnReturn` | 关闭存档弹窗，不改变当前活动档位 |
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
| `PackItem/ImgLight` | 卡包气泡/亮光 Image；是 `PackItem` 直属子节点 |
| `PackItem/PackSize` | 卡包尺寸图标；运行时根据 `CardPackSize` 配置选择 `PackSize_1.png` 到 `PackSize_7.png` |

- `SliderMusic` 和 `SliderEffect` 使用 `FakeSettingsSliderInput` 驱动现有 `Bar01/Progress/ProgressDot` 美术。圆点中心轨道由 `SliderFill` 完整左右范围各内收圆点实际半宽得到，最小/最大值时圆点外边缘必须分别与凹槽左右端对齐，不得超出凹槽。绿色在圆点下方继续铺到圆点右边缘，使上层圆点不会把绿色视觉终点遮短；满值时绿色必须完整填满凹槽。禁止使用固定 padding 或场景初始圆点坐标计算轨道。
- Windows Player 默认使用当前显示器原生分辨率的 `FullScreenWindow` 无边框全屏启动；首次没有本地设置时 `IsWindowed=false`，因此 `PanelSet/ToggleFrame` 默认关闭。打开开关后立即切换为 `FullScreenMode.Windowed`；关闭开关后必须读取 `Screen.currentResolution` 并以原生宽高和刷新率明确调用 `Screen.SetResolution(..., FullScreenWindow)`，不得保留窗口客户区尺寸或顶部窗口边框。音乐和音效更新只应用音频，不得重复切换显示模式。窗口模式允许自由拉伸；已有本地设置继续按上次选择启动。
- Windows 窗口客户区尺寸变化后，所有页面必须按 `2560x1440`、`16:9` 设计区域整体等比缩放并立即刷新 Canvas、RectTransform 和 Layout；不得裁切 UI，也不得分别拉伸 X/Y。超宽窗口在左右显示黑边，偏窄或偏高窗口在上下显示黑边。MainScene 重新计算卡包分页并保持当前页；GameScene 不得在检测到尺寸变化的同一帧使用尚未稳定的 Canvas 边界，必须等待最新尺寸稳定两帧，并在没有拖拽或互斥动画的安全时机再次配置视口、刷新布局、重算当前组相机、棋盘、托盘及托盘 Piece；活动中的新手引导提示框、箭头和焦点层随后按新坐标重建。MainScene、GameScene、LoadingScene、RankScene、AchieveScene 和运行时根 Canvas 统一使用居中的固定宽高比相机视口与 `CanvasScaler Expand`；独立的全屏底层相机将视口外区域清为纯黑，避免软件鼠标历史帧残影。

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
| 任务进度 | `GameTaskUtility` | `persistentDataPath/SaveSlotN/LocalData.json` 根对象 `TaskProgressData` |
| 卡包配置（`PackId`、`PackSize`、`StickerCount`、`ChapterId`、`BoardScale`、`Series`、`AutoUpdate`） | `GameConfigRepository` 读取 `Resources/Configs/CardPacks.csv` | 只读 |
| 卡包生命周期 | `CardPackDataUtility` | `SaveSlotN/LocalData.db` 的 `CardPacks` 表 |
| 卡包当前拼图会话 | `CardPackDataUtility` | `SaveSlotN/LocalData.db` 的 `CardPackPuzzleProgress` 表 |
| 通用集合与键值存储 | `SqliteLocalStore` API | `SaveSlotN/LocalData.db` 的 `AppRecords` 表 |

- `GameConfigRepository` 加载并缓存任务和卡包配置。当前数据源为 `ResourcesGameConfigTextSource`，优先使用 `Resources.Load<TextAsset>`，失败时回退到编辑器磁盘路径。
- `CsvTable` 是统一 CSV 解析器，支持表头访问、引号字段和空行过滤；业务代码不得直接 `Split(',')`。
- `CardPacks.csv/BoardScale` 使用 invariant-culture 浮点数且必须大于零。GameScene 将其乘到当前 CardBag 根节点，使棋盘、槽位、描边和吸附坐标统一缩放。Piece 使用两类互不覆盖的 Scale：`DragScale/BoardScale` 通过运行时 SpriteRenderer 屏幕包围盒与对应凹槽屏幕矩形直接校准，包含配置后的棋盘比例，并在每次拿起时刷新；两者使用同一目标值，使拿起后立即恢复凹槽实际显示尺寸，正确吸附时只做位置缓动、不二次缩放。`TrayScale` 默认直接等于 `DragScale`；只有 Piece 在该 Scale 下的实际屏幕高度超过托盘实际屏幕高度 `90%` 时，才统一等比缩小到 `90%`。创建、拿起时重算、回收、错误回弹和托盘布局均使用该值。任何直接回收、错误回弹、被其他 Piece 顶回或失焦回收只要最终回到托盘，都必须恢复当前规则计算出的 `TrayScale`。未正确吸附的 Piece 使用 `DragScale`，并只允许完整渲染边界落在棋盘内且实际轮廓不与 Alpha 大于 0 的已拼 Piece 或自身凹槽相交、也未同时横跨灰色拼图区与 GameBoard 非灰色区域的位置，或完整落在棋盘左右两侧的桌面，或在棋盘左右边界之间完整落入棋盘底边与托盘原始顶部之间的桌面空间；Piece 完整位于灰色拼图区内部或完整位于灰区外的棋盘空位均允许。新增下方区域使用托盘缓存的原始屏幕顶部作为下边界，托盘收起后也不得侵入其原始高度。正确吸附判定继续优先；未达到吸附标准但与自身凹槽相交、横跨灰色拼图区边缘、横跨棋盘外框、位于棋盘正上方、侵入托盘原始区域或与已拼内容实际轮廓重叠的位置均回弹。成功吸附后立即用 Prefab 对应原始 `Image` 替代 SpriteRenderer，确保已放置 Piece 与棋盘在同一 Canvas 层级共同缩放。
- 上述自由放置规则中，Piece 与“自身凹槽相交”不再属于非法条件：正确吸附判定仍优先；未达到吸附标准时，只要 Piece 完整位于棋盘范围内、未与已拼区域或其他外部 Piece 冲突且没有触犯棋盘外框或托盘区域限制，即使实际轮廓与自身凹槽部分相交，也允许按未完成 Piece 留在当前位置。未与自身凹槽相交时，其他未填凹槽边界继续使用原判定。
- `JsonLocalStore` 读写整个文件的单一根对象，目前用于任务进度。
- `SqliteLocalStore` 在 `AppRecords` 中使用集合/键记录；卡包业务状态使用专用 `CardPacks` 表。
- 本地进度固定支持三档。`persistentDataPath/SaveSlots.json` 只记录活动档位 Id，实际 SQLite 与 JSON 数据分别隔离在 `SaveSlot1`、`SaveSlot2`、`SaveSlot3` 目录。选择按钮只改变面板中的待选档位；点击“继续”才写入活动档位、重置各存储和业务静态缓存并返回 `LoadingScene`。存档摘要统计 `LifecycleState != Locked` 的卡包数量，更新时间取该档数据文件的最新本地修改时间并显示为 `dd/MM/yyyy HH:mm`。删除档位时直接删除对应目录，不迁移或合并数据。
- 从 `PanelSave` 点击“继续”切档时必须先取消仍在收尾的 `CardPackRewardFlyTransition`。该对象会跨场景存活，若保留到下一次 MainScene，会让新档位列表进入只为结算飞入准备的预隐藏状态且无人恢复。
- `CardPackLifecycleState` 为 `Locked=0`、`Unlocked=1`、`InProgress=2`、`Completed=3`。首次进入 GameScene 时将未完成卡包标记为 `InProgress`，完成最后一组后标记为 `Completed`；重玩期间保持 `Completed`，不降级。
- SQLite `CardPacks` 表包含 `PackId`、`PackSize`、`LifecycleState`、`UnlockTime` 和 `CompletionTime`，不保留旧 `IsUnlocked`、`IsPlayed` 字段。解锁和完成时间使用固定格式的本地时间 `yyyy-MM-dd HH:mm:ss.fff`。`CompletionTime` 仅在首次进入 `Completed` 时写入，重玩不修改。
- SQLite `CardPackPuzzleProgress` 表包含 `PackId`、`PlacedPieceNumbersJson` 和 `UpdatedTime`。进入 GameScene 即创建会话，即使尚未放置 Piece 也保留空记录；正确吸附后按 `PieceGGII` 的 `组号 * 100 + 组内索引` 完整编号去重、排序并立即保存。桌面 Piece 的位置不持久化。完成整包并成功保存 `Completed` 后清除该会话。
- `CardPackDistributionUtility` 与 `CardPackDataUtility` 放在一起，负责章节选择、`R` / 持有数量判断、确定性锁定候选选择和首次完成发包。重玩根据 GameScene 启动时记录的生命周期快照跳过该尝试。
- 待发任务卡包权益保存在 SQLite `AppRecords` 的 `CardPackDistribution/Progress` 下，并按唯一 `TaskInstanceId` 去重。
- GameScene 在推进任务前先持久化任务权益，且仅在任务推进保存成功后尝试发放，避免任务进度保存失败时重复发包。
- MainScene 设置以集合/键 `GameSettings/Runtime` 保存在 `AppRecords`：音乐音量、音效音量和窗口模式。
- MainScene 辅助选项开关同样保存在 `GameSettings/Runtime`，字段为 `UsableOption1`、`UsableOption2` 和 `UsableOption3`。
- `UsableOption1` 是关卡描边开关，`UsableOption2` 是贴纸描边开关，两者新建设置时都默认关闭；`UsableOption3` 是高对比度并默认关闭。已持久化的用户选择优先。关卡描边关闭时 GameScene 保留现有当前阶段连接区域，打开时改为显示当前待拼组的完整合并外边界；贴纸描边关闭时不显示单块轮廓，打开时叠加当前组每块凹槽的独立轮廓。PanelUsable 的 `ImgContentBg` 按高对比度状态显示 `MainSetHigh1/2.png`；`ImgContentLine` 在描边全关、仅关卡描边、贴纸描边打开时分别显示 `MainSetLine1/2/3.png`，两项同时打开使用信息更完整的 `MainSetLine3.png`。GameScene 的 `BoardBgXX RawImage` 在高对比度关闭时统一使用 `UI/BasicUI/BgCardBoard1.png`，打开时统一替换为 `BgCardBoard2.png`；CardBag 根 `Image.sprite` 始终为空，运行时不改变背景块布局或 UV。烘焙棋盘描边通过 Alpha-only UGUI Shader 固定输出 `#3f423e`，不随高对比度切换颜色；提示按钮的绿色滚动虚线在高对比度时改用 `#b1d702`，新手引导专用蓝色虚线不变。
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
- MainScene 的卡包选择、居中放大、`PanelBagSelect`、开包输入和 2D 缺失资源回退逻辑保持不变。选中态在隐藏选中槽位后临时隐藏软件鼠标，生成不包含鼠标的原分辨率背景截图，再使用横向与纵向两遍可分离高斯采样实现 Photoshop 半径约 `8px` 的模糊效果；完整 17 点高斯核通过线性采样合并为每方向 9 次纹理读取。Linear 色彩空间下在最终高斯通道纠正屏幕截图的 sRGB/Gamma 转换，保证虚化前后亮度与饱和度一致。虚化截图按原色、全不透明显示，不与原首页透明混合；`PanelBagSelect` 根 Image 的黑色视觉层完全透明，只继续负责 Raycast，不引入灰调。选中卡包使用更高排序层保持清晰。运行时保持动态封面、`600 x 680` 设计尺寸、选中复原和进入 GameScene 的现有交互节奏。
- 从 MainScene 正常导航进入 GameScene 时播放一次参考视频节奏的入场：PieceBoard 先从下方快速进入；CardBag/棋盘延迟后从右侧滑入，返回和提示按钮与棋盘阶段同步淡入；Piece 最后错峰落到托盘目标位置。完整拆包入口由 GameScene 从 MainScene 传入位置执行向上扇出后再落牌；彩色或灰色撕开卡包已经在 MainScene 完成碎片分开，因此传递“已扇出”状态，GameScene 首帧直接显示扇出 Piece，并在约 `0.04s` 后连续落牌，不得重复扇出或再等待 `0.3s`。入场完成前屏蔽拖拽。托盘 Piece 在首次入场和切组入场全过程保持最终 `TrayScale`，不做临时放大，位移、旋转与淡入动画继续保留。对象在起始姿态保留两个渲染帧后才推进动画；已扇出 Piece 在预热帧保持可见。单帧动画时间最多推进 `1/30s`，场景加载或首帧资源初始化卡顿不得吞掉入场过程。直接在编辑器启动 GameScene 保持即时初始化。
- 完整彩包的制作方动画 `Take 001` 在 `0.800s` 到达纵向最高点并开始下落，MainScene 以该关键帧作为 GameScene 交接点，不再等到原 `1.600s` 模型可见窗口结束。交接前滑光在 `0.5s` 启动；MainScene 按 SQLite 已拼编号选择首个未完成组，只创建该组尚未拼上的真实 Piece，并让它们使用 `IngameCoverShadow04`、从卡包实际撕口后层用 `0.32s` EaseOut 短跳到既定散点。完整拆包舞台的背景、临时 Piece、卡包正面分别使用 Render Queue `1999/2000/2001`，不能仅依赖 Sorting Order 或通用 Shader 深度测试。GameScene 接管 Piece 只在撕口初始两帧使用 `2000`，正式飞向托盘前恢复初始阴影材质原始队列，避免游戏 UI 遮挡；不得改动滑光和星星的透明渲染队列。交接时 MainScene 临时 Piece 隐藏，由 GameScene 同数量的真实 Piece 在同一散点中心接管；三种卡包入口的 GameScene 发牌 Piece 均使用初始碎片阴影。3D 卡包模型和 `fx_chai_w_001` 跨场景继续完成原始下落与滑光收尾后自行清理，棋盘、托盘和按钮同步入场；不得恢复静态撕开包、创建第二份卡包下落、重复已拼 Piece 或改变发牌参数。
- 每组完成后等待最后一片的绿色 ADD 滑光和本轮光点回弹全部结束，再将 CardBag/棋盘位置、相机正交尺寸和托盘平滑切换到下一组布局；棋盘主体动画约 `0.72s`，新组 Piece 从棋盘区域用约 `0.38s` 错峰进入托盘。切组动画期间锁定拖拽、提示和“一键完成”，普通卡包同样使用该流程。
- 首次建组和后续切组自动适配棋盘时，继续先按活动槽位适配相机并居中，再限制 `GameBoard` 屏幕底边到可见托盘屏幕顶边的间距不超过游戏背景可视高度的 `10%`。只有超限时才向下平移整个 CardBag 根节点；不修改 `BoardScale`、相机缩放、托盘位置或 Piece 相对布局，托盘隐藏时不执行该限制。
- GameScene 的 `BtnCompleteAllTest` 仅在 Unity Editor 和 Development Build 中运行时创建。点击后批量持久化当前 CardBag 全部 Piece 编号、显示完整棋盘并调用正式 `ShowRewardPanel()`；因此卡包生命周期、任务积分、奖励发放和完成数量都会产生真实本地测试数据。正式非 Development Build 不显示该按钮。
- `GameScene/BtnTips` 从当前组运行时托盘排列中选择第一个仍在托盘的未完成 Piece，不再按 Piece 编号固定选择首个凹槽；新手引导目标继续使用独立的固定编号规则。目标碎片在托盘原位置左右抖动约 `0.8s` 后停止，棋盘对应 `GrooveRect` 使用 `HintDashedOutlineGraphic` 从 GPU 读取 Piece Sprite 的实际 Alpha 像素边界，沿真实累计轮廓长度生成固定 `20` 像素实线、基础间隔 `15` 像素、滚动速度 `60` 像素/秒的绿色滚动虚线；普通模式使用 `(112,151,75)`，高对比度使用 `#b1d702`。轮廓在当前 GameScene 内按 Sprite 缓存并在离场时清空，Physics Shape 只作为读取失败回退。再次点击按钮取消当前提示，成功放置、切组或结算时同样清理。一旦有效提示显示过，本局持续记为已使用提示。
- `CardBag001` 引导按实际三组拆成三个阶段，首次流程已有部分进度时从当前未完成组继续；活动拼图会话优先于历史卡包完成状态和教程完成记录。已完成卡包重玩中途退出后保留所有已拼 Piece，再次进入时从当前未完成位置继续引导；只有重新确认一局重玩时才清除上一局并从第1组开始。只有整包完成并进入结算时才将 `Tutorial/CardBag001TutorialCompleted` 写入 SQLite `AppRecords`，中途退出不会提前完成教程；没有活动会话的已完成卡包普通进入不自动引导，通过 MainScene“重玩”确认进入时则从第1组重新播放完整引导。第1组 `Piece0101` 为强引导：保留游戏原有暗色托盘且不额外叠加教程黑色遮罩，突出目标 Piece，显示蓝色滚动虚线和等比缩小至原尺寸 `70%` 的 `GuideArrow1.png`；箭头从碎片中心出现并循环移动指向目标凹槽，不做从小到大的缩放，并只允许拖动目标；拿起后文字和虚线保留，放错恢复焦点。第2组 `Piece0201/0202` 同时高亮、允许任意顺序放置，并从本阶段开始显示当前组烘焙关卡描边；不显示箭头或目标虚线，第一片放对后只刷新剩余 Piece 焦点。第3组 `Piece0301-0305` 恢复正常交互和 `BtnTips`，并介绍提示功能；`BtnTips` 在前两步保持隐藏。第一步提示框以屏幕归一化位置 `(0.5, 0.7)` 为基础向上增加一个提示框自身高度，再追加设计坐标偏移 `(-30, -50)` 并限制在屏幕安全范围内；第二步位于当前待拼凹槽整体边界上方，位置随棋盘布局动态计算并限制在屏幕安全范围内。第三步运行时读取 `BtnTips` 的真实屏幕中心，以场景 `GuideTip/Arrow` 的右侧尖端反算基准位置，再将提示框整体向左移动 `48` 个、向下移动 `20` 个设计像素并限制在屏幕安全范围内；箭头独立读取按钮实际矩形，推进终点停在按钮左边缘外 `16` 个、按钮垂直中心下方 `20` 个设计像素处，不覆盖按钮图标。三者从对应方向淡入。三步文字统一克隆场景 `GuideTip/TextTips`，保留编辑器设置的 TMP 字体、材质、颜色、字重、对齐和 RectTransform；运行时只替换文案，并将内容平衡为最多两行，排除标点出现在第二行开头的断点，超过单行宽度时启用 Auto Size 缩小字号。第三步从场景 `GuideTip/Arrow` 读取 `GuideArrow2.png` 及编辑器布局，提示框入场后箭头淡入并循环向右上推进。该引导本身不算使用提示。
- 每片可见贴纸只显示一个不规则暖白光点。托盘 Piece 在 `CreateDraggableGroup` 创建 SpriteRenderer 时不提前生成光点；首次入场和切组入场均在该 Piece 完成落入托盘后单独创建，避免多个 Piece 聚集或飞行时光点重叠。没有入场动画时由运行时幂等校验在首个 Update 创建；历史已拼 Piece 在棋盘初始化时同样生成，不依赖首次正确吸附。`UI/GameScene/PieceLight1.png` 到 `PieceLight4.png` 按 Piece 正式编号稳定轮换圆环、斜光、长弧和圆角框，资源缺失时才顺序回退；四种 Sprite 保持原始宽高比，只按 Piece 可用宽高整体缩小，并使用各自的稳定旋转区间，因此相近尺寸 Piece 也能保持明显轮廓差异。位置优先读取 Sprite Physics Shape 的左上极值并向轮廓内部收回，缺少轮廓时回退固定左上位置；同一 Piece 在托盘 SpriteRenderer 与棋盘 UGUI 间共用同一形状、旋转、缩放和归一化位置，并分别通过 SpriteMask 和 Alpha Mask 裁切在 Piece 真实轮廓内。两条 ADD 材质路径均使用 9 点预乘 Alpha 柔化及纹理边界渐隐，确保回弹形变时不露出矩形硬边。正确放置 Piece 后仍先用固定 `0.12s` 吸附到凹槽中心；抵达并提交棋盘后立即允许拿取托盘下一块，同时当前块继续播放原有约 `0.52s` 的绿色斜向 ADD 光带。当前块和最多六块实际相邻的已拼 Piece 按原 `0.07~0.23s` 错峰触发光点回弹：光点横向 8 段网格的两端固定，中段沿受力方向推出并加厚拉伸，再小幅反向回弹并恢复初始形状和固定位置，不累计位移。光点推出距离为原规则的 3 倍，即按光点尺寸限制在 `18~42px`；当前块运动 `0.96s`，相邻块运动 `0.84s`，均为原时长的 2 倍。新动画从当前形变接管同一光点，旧动画停止写入。绿色光带的 Shader、范围、颜色和时长不变；多个落位反馈可以并发，实际等待时间按最后一个光点的错峰延迟与运动时长计算，切组或结算必须等待全部反馈结束后只触发一次。
- 高光点位置以内部安全区域规则为准，替代上一条中的“Physics Shape 左上极值”定位：运行时在归一化 Physics Shape 内部采样，先保留轮廓余量达到该 Piece 最大内部余量 `72%` 的候选，再选择最接近原左上视觉区域的位置；没有足够左上空间的异形或狭长 Piece 使用自身最深内部点。位置不再叠加随机偏移，也不再强制夹到固定左上范围；缺少有效 Physics Shape 时回退 Piece 中心。高光样式、尺寸、旋转、裁切与动画规则保持不变。
- RewardPanel 使用三个独立标题文本：`TaskBg2/TaskTitle2` 固定保留编辑器配置的“卡包数”，`TaskTitle21` 依次显示“未使用提示”“关闭关卡描边”“关闭贴纸描边”和“快速完成”等实际生效的加成名称，`TaskTitle22` 同时显示该阶段的实际 `+N分`。`N` 使用该阶段累计分数与上一阶段分数的差值，确保全部加分之和与最终分一致。结算初始化和基础分滚动期间三者全部隐藏；每条加成只显示 `TaskTitle21/22`；全部加成结束后隐藏二者并显示 `TaskTitle2`，无加成时在基础分滚动结束后直接显示 `TaskTitle2`。基础分滚动 `1.2s`；每条加成先展示文案 `0.16s`，再滚分 `1.08s`；最终分稳定停留 `0.45s`。非积分任务进度从最后一次有效滚分的 `25%` 位置开始同步推进，没有加成时并入基础分滚动，不再单独追加一段任务等待。加成名称和分数强制单行并以编辑器字号为上限自动缩小；代码不覆盖三个文本的字体、材质、颜色、对齐和 RectTransform，积分计算、任务进度和发包结果不变。
- 进入 RewardPanel 时，完成棋盘使用完整 `CardBag` 根 RectTransform 的实际屏幕边界，计算等比缩小到“当前屏幕与游戏背景可见区域交集”`90%` 以内的居中目标；小于该范围的棋盘不放大。托盘和运行时 Piece 移除、全部完成凹槽显示后，棋盘以约 `0.46s` SmoothStep 同时缩放和移动，并与结算顶部 UI 入场并行播放，因此整体包含 `BoardTitle`、全部 `BoardBg`、`GameBoard` 和已完成 Piece，且不会瞬间跳变。`TaskBg2` 与当前 `TaskItem` 的顶部入场仍为 `0.52s`，末段统一下沉约 `14px` 后回到编辑器终点，形成轻微纸张落地回弹。
- RewardPanel 的完成按钮在顶部入场、积分与任务进度、首次完成卡包数 `+1`、`ImgBagBg` 入场以及本轮奖励图标动画全部结束前保持不可交互，避免跳过中间结算表现。卡包数右上角飘出的 `+1` 使用编辑器 `TaskBg2/TaskTitle2` 的字号，不跟随红色数字 `TaskBagNum` 的字号。
- RewardPanel 的 `ImgBagBg` 开始上滑 `0.26s` 后启动首张奖励卡，双奖励再错峰 `0.14s` 启动任务奖励飞入。任务奖励无论是唯一奖励还是双奖励中的第二个，都复用对应 `BagRewardItem/Canvas/BagCover`：在任务条 `BagBg/BagIcon` 的实际位置瞬时替换原图标，再用 `0.52s` 直线下移并放大到对应槽位，且逐帧读取移动中的槽位终点；原 `BagIcon` 立即隐藏，`BagBg` 绿圈、`BagAddBg` 和 `TextAddNum` 留在原位用 `0.2s` 渐隐。不得复制临时问号图。`BtnFinish` 和 `BtnCamera` 在结算首帧只沿 Y 轴放到屏幕下方，不修改编辑器透明度、缩放、旋转或样式；底板和全部奖励卡动画结束后，两按钮同步复用 `ImgBagBg` 的 `0.42s` 位移缓动滑回编辑器终点。无奖励卡包时则在积分和卡包数流程结束后进入。两按钮到位前不可交互。
- `PanelBagSelect` 每次打开时直接复制当前列表项的完整 `PackNode`，按同一状态放大到 `600 x 680`，因此撕口、`PackBg`、进行中装饰碎片、封面、尺寸标签和状态材质必须与列表一致。完整彩色卡包显示“玩”，点击后进入现有拆包舞台、等待轻点/滑动并播放完整拆包动画。完成第一组后的彩色撕开卡包显示“玩”，点击后跳过拆包特效；`Completed` 且没有活动会话的灰色撕开卡包显示“重玩”，先弹出 `PanelReplay`，确认后清除旧会话、创建空会话，但卡包继续保持灰色完成态。两种撕开卡包随后执行同一套无额外停顿的跨场景下落和真实 Piece 发牌节奏；彩色撕开额外将其已有的首页展示碎片收进撕口并隐藏，灰色撕开不创建或处理假碎片。`BtnReturn` 和 `BtnClose` 取消。相机按钮只对历史上至少完整完成过一次、生命周期为 `Completed` 的卡包显示；首次拼图尚未完成的 `InProgress` 卡包不显示。弹窗显示期间隐藏选中卡包、其他列表卡包 Renderer 和尺寸图标，并锁定选择页按钮；取消时全部恢复。
- 点击 `BtnCamera` 后播放一次全屏白色闪光，并离屏生成 `1024 x 1024` PNG。图片由 `MainPhotoBg` 木纹底图、当前 `CardBagNNN` Prefab 还原的完整拼图和左下角 `MainGameIcon` 组成；拼图等比适配并轻微旋转。文件以 `Application.productName-YYYY-MM-DD-BagId.png` 保存到桌面，BagId 使用三位编号，同日同一卡包重复拍照覆盖旧文件。保存成功后通过独立顶层 `PackPhotoItem` 将 `Photo` 替换为生成图；预览出现时单次播放 `PackPhoto`，下方保存提示先显示再消失，随后 OK 按钮出现，完整动画结束后按钮才可点击。预览期间隐藏选中卡包，点击 `BtnOK` 关闭预览并恢复卡包。拍照不写业务持久化数据。

- 首次入场和切组动画期间暂停托盘光点通用补建；每块 Piece 落稳时显式创建，动画结束后才恢复对“活动、未放置且缺少光点”的 Piece 幂等补建。`SpriteMask` 排序范围覆盖 Piece 本体到其上方两级，光点固定处于中间一级。

### 开发期持久化策略

- 开发阶段的本地持久化不保证向后兼容。数据结构和 SQLite 字段类型可直接改为当前需求，不增加迁移或旧数据回退，除非用户明确要求。
- SQLite 表结构发生不兼容修改后，关闭 Unity，并在测试前删除受影响的 `%USERPROFILE%/AppData/LocalLow/MainTown/Ducky Stickers/SaveSlotN/LocalData.db`。
- JSON 任务进度或跨两个存储的行为发生变化时，同时删除对应 `SaveSlotN/LocalData.json`。从旧单档结构切换到三档结构时，旧根目录 `LocalData.db` 和 `LocalData.json` 不迁移，应手动删除；完整重置三档时同时删除 `SaveSlots.json` 和 `SaveSlot1/2/3`。每次不兼容修改后，助手必须指出需要删除的文件；未经明确要求不得自动删除。

---

## 6. 添加内容

### 卡包

`MainScene.RefreshPackageList` 根据数据库动态创建已解锁卡包槽位。不要在场景中手工复制 `Package002`、`Package003` 等对象。

共享尺寸图标为 `UI/PackImages/PackSize_1.png` 到 `PackSize_7.png`，对应 `CardPackSize` 数值（`XS=1` 到 `XXXL=7`）。`PackItem` 当前层级为 `PackItem/CardPackEffect/PackNode/PackCover|PackSize|ImgLight`；旧 `PackHighlight` 与 `PackHighlight02~05.png` 已删除。`PackItem` 根节点的 `PackCoverVisualSettings` 引用 `PackCover`、`PackSize` 及各自普通/完成态材质；封面完成态使用 `PackCoverCompleted.mat`，尺寸标签完成态使用独立的 `PackSizeCompleted.mat`。后者基于 `PackSizeState.shader`，只暴露 `Tint`、`Grayscale Amount`、`Grayscale Color`、`Brightness` 和 `Contrast`，不包含撕口或投影。`Preview Completed In Editor` 同时预览封面和尺寸标签完成态。`PackNode` 绑定 `Assets/Animation/PackNode.controller`，默认状态为循环的美术 `PackAniBreath`；该 Clip 当前包含 6 秒根节点位置与旋转循环曲线。MainScene 不写动画曲线或 Transform，仅根据实际显示状态设置 Animator 播放速度：彩色 `1`、完成且无活动会话的灰色卡包 `1/3`，活动重玩会话恢复为彩色和正常速度；每个列表卡包再按 PackId 与黄金分割步长生成稳定起始相位，使同屏呼吸错开且刷新不跳变。原 `PackAni` 状态保留给等待撕包页的 `ImgLight` 滑动提示，并继续从归一化时间 `0` 开始。MainScene 在运行时设置封面和尺寸 Sprite，并根据编辑器封面尺寸缩放尺寸图标；程序仅根据完成状态切换尺寸标签材质，不覆盖美术参数。需显示撕开状态的卡包列表封面随机使用 `Assets/UI/PackImages/PackMask01~06.png` 的 Alpha 数据裁剪，透明区域撕除、不透明区域保留；`PackCoverShadow.shader` 同时裁剪封面与投影，默认关闭蒙版以保证其他共用材质不受影响。活动会话会在 `PackCover` 后方运行时创建 `ProgressPieces`，优先从对应 `CardBagNNN.prefab` 选择 3 片尚未拼上的 `PieceGGII`，不足时用本关其他 Piece 补齐；三片在原显示尺寸基础上放大 `40%`，并按增加高度的一半向下修正中心，以基本保持原顶部露出位置。每片按 PackId 和碎片序号使用稳定错峰相位，以 `6` 设计像素振幅、与 `PackAniBreath` 相同的 `6s` 周期持续上下浮动；横向中心根据放大尺寸和倾角计算的包围宽度限制在卡包左右边界内，不能漏出卡包外侧。原有倾角、阴影和完整矩形 Image 网格保持不变。选中时直接复制包含该容器在内的当前 `PackNode`，完整保留列表视觉并统一放大；原列表实例则暂时隐藏。`PackageInteractionHandler` 仅负责卡包点击、滑动与 ScrollRect 事件转发，不再包含呼吸参数、编辑器预览或运行时缩放写入。首页卡包后续动效由美术直接在 Prefab/Animator 中配置，程序不得覆盖其 Transform 动画。`Assets/Resources/PackHighlightAdditive.mat/.shader` 名称继续保留，但只供 GameScene 的 `PieceLight1~4` 拼图高光点使用，不属于 PackItem。选择页与选中卡包 Canvas 都使用 Main Camera，并明确覆盖 Sorting Order；Camera Canvas 之间的屏幕/本地坐标转换、撕包输入范围和卡包退出位置必须传入各自 `worldCamera`，不能沿用 Overlay 模式的 `null` 相机。Shader 和 Material 不得放进 `Assets/UI`，否则 BuildSync 会将其复制到 `StreamingAssets/UI` 并触发重复材质导入。

`PackItem` 不再包含 `PackShadow` Image，也不再读取封面像素或在 CPU 生成阴影 Texture/Sprite。`PackCover` 默认引用 `Assets/Resources/PackCoverShadow.mat`；完成态模板为 `Assets/Resources/PackCoverCompleted.mat`。对应 UGUI Shader 根据当前封面 Alpha 在同一次绘制中合成投影和原封面，并按源贴图像素提供颜色/透明度、X/Y 偏移、X/Y 模糊、扩散及 X/Y 渲染留白参数；完成态还提供灰度强度、灰色颜色和可选黑白蒙版，蒙版白色区域置灰、黑色区域保留原色。`PackCoverShadowEffect` 只在 UGUI 网格生成阶段围绕 `PackCover` 自身矩形中心提供 Shader 所需留白，并在 Material 留白参数变化时刷新网格；不能在 Shader 顶点阶段围绕 Canvas 原点直接缩放。MainScene 根据状态选择 `PackCoverVisualSettings` 中的美术材质模板；创建撕口副本时只写 `_TornMaskTex` 与 `_UseTornMask`，不得覆盖任何完成态美术参数。美术统一在材质中调整投影和完成态效果；阴影被裁切时先增大 `Render Padding X/Y`。

`PackSize` 的尺寸、位置、锚点和 Pivot 完全由 `PackItem.prefab` 的美术配置决定。MainScene 只根据 `CardPacks.csv/PackSize` 替换 `PackSize_1~7.png`，并按卡包状态选择普通或完成态材质；不得直接修改 `PackSize` 的 RectTransform。首页列表根据 `PackCover` 原始尺寸与目标 `240 x 272` 计算统一比例，只设置共同父节点 `PackNode.localScale`，让 `PackCover`、`PackBg`、`PackSize`、`ImgLight` 和其他美术子节点整体缩放并保持 Prefab 内的相对位置。

所有 `Assets/Resources/CardBagPrefabs/CardBagNNN.prefab` 采用统一投影状态规则：`GameBoard` 和 `BoardTitle` 在 Prefab 中绑定 `IngameCoverShadow01`；Prefab 内的正式 `PieceGGII` 是棋盘凹槽，只在正确拼入后显示，固定绑定 `IngameCoverShadow03`；这些 Image 都必须带 `PackCoverShadowEffect`。GameScene 从凹槽纹理创建世界空间 SpriteRenderer 作为玩家操作的实际碎片：初始托盘为 04，每次拿起时刷新包含配置棋盘比例的实际 `DragScale/BoardScale` 并切回 04；松手后未正确吸附、桌面放置或错误回弹为 02，正确吸附时为 03；提交后销毁运行时碎片并显示同样使用 03 的凹槽 Image。未完成凹槽只通过隐藏或 Alpha 0 控制，不能切回 04。SpriteRenderer 使用运行时 FullRect Sprite、材质克隆和 `PACK_SHADOW_SPRITE_RENDERER` 变体按 Sprite PPU 扩展渲染顶点；Scale 统一在切换 FullRect 后计算，Piece Collider 和 Groove Probe 则始终从原始凹槽 Sprite 创建，投影网格不得改变精确碰撞轮廓。`IngameCoverShadow03` 使用 `2px` X/Y Render Padding 承载其现有 `2px` Alpha 模糊，确保全不透明矩形 Piece 的投影不会被自身 UGUI 网格裁掉；该留白只扩展渲染网格，不改变凹槽 RectTransform、吸附尺寸或碰撞轮廓。新建 CardBag Prefab 时生成器自动应用该规则；现有资源可通过 `Puffies -> Apply CardBag Shadows` 重建绑定。

所有 `CardBagNNN.prefab` 使用扁平制作层级：根 `Image.sprite` 必须为 `None`，直属子节点依次为可选 `BoardTitle`、`BoardBgXX`、`GameBoard`、按正式编号升序的 `PieceGGII`。`BoardBgXX` 是不接收射线的 `RawImage`，默认引用 `BgCardBoard1.png`，按从上到下、每行从左到右从 GameBoard 左上角二维平铺；最后一行和最后一列同时缩小 Rect 并调整 UV，只显示 GameBoard 范围内的纹理且不得拉伸。高对比度运行时统一将这些 RawImage 的纹理替换为 `BgCardBoard2.png`，不再给 CardBag 根 Image 设置背景 Sprite。`Generate CardBag Prefabs` 在生成时自动建立并校验该结构；独立的手工层级迁移菜单已移除。

1. 场景中只保留一个模板对象：`Package001`。
2. 在 `CardPacks.csv` 增加一行（`PackId`、临时 `PackSize`、临时 `StickerCount`、`ChapterId`、正数 `BoardScale`、手工 `Series`、`AutoUpdate=1`），随后执行配置更新工具按实际片数同步 `PackSize`、`StickerCount` 和 `BoardScale`。需要手工固定棋盘缩放时将该行 `AutoUpdate` 改为 `0`；`PackSize` 和 `StickerCount` 仍会自动更新，工具始终保留 `Series` 原值。
3. 在 `UI/PackImages/` 下按 `PackIconNNN.png` 命名增加对应封面。`GameDefine.FormatPackImagePath` 将 PackId `1` 映射到 `UI/PackImages/PackIcon001.png`。
4. 通过 `CardPackDataUtility` 将生命周期写入 SQLite `CardPacks` 表。
5. 卡包列表和选中态直接使用对应静态封面；不为每个卡包创建 3D 展示资源。

### 拼图

1. 在 `Assets/Resources/CardBagPrefabs/` 下创建 `CardBagNNN` Prefab，`NNN` 与 `PackId` 一致。
2. Prefab 根 Image 不设置 Source Image；根下放置直属的 `BoardTitle`、自动平铺的 `BoardBgXX`、`GameBoard` 和全部 Piece，顺序不得改变。
3. 分组凹槽使用直属根节点的 Image，名称严格使用 `PieceGGII`：`GG` 和 `II` 都是两位数字且范围为 `01..99`。例如第 1 组使用 `Piece0101`、`Piece0102`...，第 2 组使用 `Piece0201`、`Piece0202`...。
4. 源贴图放在 `Assets/UI/CardBags/CardBagNNN/`，标准切图名继续使用 `piece_001.png`、`piece_002.png`...；需要在源文件名中显式携带正式分组时使用 `Piece0101.png` 或 `Pieces0101.png` 格式。
5. 不使用 `PieceGroup` 父节点；分组严格读取 `PieceGGII` 的前两位 `GG`，组内排序读取后两位 `II`。
6. 不创建 Package JSON；运行时数据来自已加载 Prefab 的 Image。
7. 新增或修改 CardBag 后，执行 **Puffies -> Bake CardBag Outlines**。烘焙器优先使用 `GameBoard.png` 的透明挖空 Alpha 作为最终拼图外边界，并使用已完成 Piece 的 Alpha 作为后续组接触边；GameBoard 没有有效挖空时回退到全部 Piece Alpha 并集。每组生成三张同尺寸资源：`GroupNN.png` 是默认连接区域；`GroupNN_Level.png` 是当前组 Piece Alpha 并集的完整外边界；`GroupNN_Stickers.png` 是当前组每块 Piece Alpha 边界的并集。默认连接图第 1 组只包含自身最终拼图外边界，后续图只包含当前组最终外边界及其与低编号已完成组的接触边。接触边和最终外轮廓均使用圆形最近距离与局部边界法线判定归属，切线方向的邻近不得延长端点；相邻分组可在真实交点共享少量边界像素，不能为避免重叠而删除交点。
8. `GameScene` 将烘焙 Sprite 的 Alpha 作为不可交互的 `GameBoard` 子 Image 显示，并通过专用 UGUI Shader 固定输出 `#3f423e`。Shader 在源纹理像素坐标上生成稳定的细粒和少量小空点，形成轻微铅笔断墨质感；纹理不随棋盘移动或缩放逐帧变化。关卡描边关闭时加载 `GroupNN.png`，打开时替换为 `GroupNN_Level.png`；贴纸描边打开时额外叠加 `GroupNN_Stickers.png`。不要在 Prefab 中手工制作描边对象，也不要为两种底板重复烘焙资源。
9. 缺少生成 Sprite 时，运行时记录制作警告，并在无描边情况下继续游戏。交付前重新运行烘焙器。
- 创建一组碎片时，将当前尚未完成的 Piece 随机打乱一次后从左向右排列；两片以上必须与原凹槽编号顺序不同，同一组存续期间保持这份顺序，不因拿起、放回或重排再次随机。排列继续使用所有卡包共用的 `40` 设计像素固定间距，并以每块 Piece 的实际 `SpriteRenderer.bounds` 将渲染内容上下居中到黑色托盘。托盘缩放使用第一版屏幕尺寸适配：根据 `PieceBoard` 实际屏幕高度换算设计像素比例，将 Sprite 原生设计高度与托盘 `90%` 容纳上限取小后反算当前 `SpriteRenderer.localScale`；拿起后切换到对应凹槽的 `DragScale`。不得改为只按托盘稳定世界边界主动放大到目标设计高度的第二版实现。PieceBoard 的世界边界从根 Canvas 设计坐标直接映射到当前屏幕和游戏相机，不依赖 Screen Space - Camera Canvas 在相机适配后的首个渲染帧更新世界角点，因此首次初始化与点击后的重排使用相同托盘中心。Piece 从托盘拿起时，仅随机顺序中位于其后且仍在托盘的 Piece 沿 X 轴用 `0.5s SmoothStep` 向前补位；不得刷新前序 Piece、外部 Piece 或任何剩余 Piece 的 Y/缩放，拿起队尾时不启动位置刷新。拖拽 Piece 每帧跟随指针后，必须按当前完整 `SpriteRenderer.bounds` 限制在游戏背景可视边界内，不能只限制中心点，也不能改变抓取偏移。松手时首先检查鼠标或触点是否位于托盘原始区域，或 Piece 自身屏幕边界是否与该区域实际相交；任一条件满足都立即自动回托盘并恢复布局，不再检查正确吸附，因此原地拿起再松手不能停留并与其他 Piece 重叠，棋盘与托盘重叠部分也始终由托盘优先处理。两项都未命中托盘才继续正确吸附和自由放置判定。未吸附 Piece 只允许完整渲染边界停在棋盘内 Alpha 为 0、没有已拼内容占用的位置，完整落在棋盘左右两侧的桌面，或在棋盘左右边界之间完整落入棋盘底边与托盘原始顶部之间的桌面空间，并且不得与另一块外部 Piece 重叠；任何横跨棋盘边框或侵入托盘原始高度的状态均不允许停放。棋盘正上方、已拼内容轮廓上或其他外部 Piece 上松手时缓动返回本次拖拽起点。来自托盘的错误 Piece 松手后立即回弹，不预先变红或停顿；所有从桌面或棋盘外部返回托盘的 Piece（包括玩家手动拖回和被正确 Piece 顶回）都在渲染边界首次进入可见托盘区域后，以原错误红色 `70%` 强度显示反馈，到位后淡回原色。已经放在棋盘透明区或桌面的外部 Piece 再次拖到非法位置时，判定失败后立即显示同样的错误红色，随 `0.3s` 回弹到本次拖拽起点，再用 `0.1s` 淡回原色。若正确目标被外部错误 Piece 占用，全部实际轮廓重叠的错误块会按本组随机顺序重排并回弹到托盘，不能阻止正确 Piece。托盘即使正在隐藏或已经收下，原始屏幕区域仍作为回收热区；命中后立即恢复并启用托盘、刷新布局，再按本组随机顺序计算 Piece 的托盘目标位置并回弹到托盘内部。托盘收起完成并仍有外部错误 Piece 时，每隔 `5s` 让这些 Piece 短暂抖动一次；拖拽、回弹、切组、结算或托盘重新出现时停止并重新计时。最后一块正确吸附后仍进入切组或结算。

- 托盘收起后的外部错误 Piece 周期提醒仅在桌面或棋盘上严格剩余 `1` 块时执行；`0` 块或大于 `1` 块时不抖动。临时组合按成员 Piece 数量统计，数量或其他运行条件变化时立即停止当前抖动并重新开始提醒计时。
- 当前活动分组内未正确放置的 Piece 支持桌面临时组合。只有两个 Piece 对应 Groove 在最终棋盘坐标下的 `GrooveProbeCollider` 真实相邻，并且玩家把它们拖到接近正确相对位置时才自动吸附；吸附位置固定使用两个 Groove 目标坐标之差，不按当前桌面外框或编号猜测。单块可加入组合，组合之间也可继续合并；拿起任一成员时整组保持内部相对位置跟随指针，并按组合渲染边界并集限制在桌面内。组合松手仍保持托盘回收最高优先级：任一成员或指针命中托盘原始区域时自动拆散并重新排回托盘。未命中托盘时，只有统一平移后所有成员都进入各自吸附距离才整组正确放置、逐块保存并沿用现有落位光效；否则先尝试与外部相邻 Piece/组合连接，再逐块校验合法空位和外部重叠，同时禁止组合总边界横跨棋盘边框或侵入托盘原始区域。非法放置时所有成员同时显示错误红色并回到各自拖拽起点。桌面组合只存在于当前 GameScene 内存，退出场景不恢复；回托盘、被正确 Piece 顶回和切组时清除组合关系；新手引导期间禁用组合形成。
- 临时组合按单个整体显示投影：运行时将全部成员 Sprite Alpha 按桌面实际位置合并成一张并集蒙版，成员自身不再分别投影，整体投影直接复用 `IngameCoverShadow02/04` 的既有参数。拿起组合使用 `04`，留在桌面使用 `02`；拖动、错误回弹时整体跟随，合并、拆散、正确入槽、回托盘和切组时重建或释放。组合进入托盘拆散后，每个成员必须在回弹动画开始前恢复托盘默认投影 `04`，包括被正确 Piece 顶回托盘的路径。不得为组合重新猜测或覆盖材质的颜色、偏移、模糊、扩散参数。
- 桌面临时组合的 `0.12s` 自动吸附缓动完成后也播放一次既有 `PuzzlePlacementShine` 滑光。滑光按组合当前桌面位置覆盖全部成员，整组共享同一道屏幕空间光带及现有 `0.52s`、颜色和宽度参数；不得错误地在最终棋盘凹槽位置播放，也不得修改成员常驻材质或组合投影参数。
- 普通提示继续优先当前托盘排列第一块；托盘为空后先选择最早形成的桌面组合，没有组合时选择最早放到桌面的单块。组合提示会让所有成员一起抖动，并把各成员 Groove Sprite 的 Alpha 映射到共同 Rect 后合并为一张临时蒙版，只沿 Alpha 并集绘制一次滚动虚线外轮廓，因此不显示组合内部接缝；虚线参数和普通/高对比颜色规则保持不变。
- 组合提示的抖动必须围绕成员共同中心执行：所有成员的位置、旋转和整体投影应用同一个旋转增量，视觉上作为一块完整 Piece 抖动；不能让各成员围绕自身中心分别旋转。
- 托盘 Piece 的缩放只使用一条规则：先以对应凹槽最终屏幕矩形计算出的 `DragScale` 作为 `TrayScale`，该值已经包含 `CardPacks.csv/BoardScale`、Canvas 和 CardBag 根节点缩放；再计算 Piece 在该 Scale 下的实际屏幕高度，只有高度超过托盘实际屏幕高度的 `90%` 时，才按高度比例统一等比缩小到 `90%`。首次创建、拿起快照、回收、错误回弹、托盘重排和拿起补位都必须复用该算法。拿起后仍使用 `DragScale`，因此只能保持尺寸或放大，不允许缩小。
- `DragScale` 必须用 Piece Sprite 原始本地尺寸经父节点和游戏相机投影后的屏幕尺寸，与凹槽最终屏幕矩形直接匹配；不能读取依赖首帧渲染状态的 `SpriteRenderer.bounds`。初始化创建与第一次点击重算必须得到相同结果。
- 首次入场在 Piece 淡入前，必须先以棋盘和托盘最终位置统一刷新当前组的 `DragScale/BoardScale/TrayScale` 和托盘排布，再缓存 Piece 动画目标；切组时在棋盘与相机移动结束后、Piece 淡入前执行同一刷新。托盘高度限制按 Sprite 原始尺寸投影计算，不能受 Piece 当前旋转或动画起点影响。
- 托盘布局的 Piece 宽度和渲染中心偏移必须使用 Sprite 原始 bounds、最终 `TrayScale` 与未旋转姿态计算；入场和切组动画的起始旋转不得通过 `SpriteRenderer.bounds` 改变最终横向间距或上下居中。
- 提示按钮与新手引导复用的滚动虚线描边宽度为 `3px`；实线长度、间距和滚动速度继续使用既有参数。
- CardBag001 新手引导第一、二步提示框统一动态定位在当前要拼凹槽区域上方：第一步使用指定 Piece 的凹槽，第二步使用当前阶段全部尚未拼好的凹槽并集；Screen Space - Camera 坐标换算必须使用教程 Canvas 的实际相机，失败时先回退到棋盘区域上方，再回退到画面顶部居中。第三步按 `BtnTips` 实际矩形使用独立位置与箭头偏移，提示框右边缘和按钮左边缘至少间隔 `32` 设计像素，箭头尖端位于按钮左边缘外 `16` 设计像素；按钮、提示框和箭头的屏幕坐标换算同样必须使用教程 Canvas 的实际相机。
- 烘焙关卡描边使用约 `3px` 的全 Alpha 核心，并在外侧增加一像素 `115/255` Alpha 边缘，视觉宽度约为 `3.9px`；默认连接、关卡完整边界和贴纸独立边界共用该输出规则。
- 默认连接描边中与低编号已完成组相邻的接触边采用单侧输出：清除 `completedMask` 内全部线像素，只在当前待拼组 Mask 邻域内向外绘制完整线宽，避免描边的一半被已完成 Piece 遮挡。最终棋盘外围、`_Level` 和 `_Stickers` 描边继续以原边界为中心生成。
- 当前组托盘 Piece 的合并渲染边界超出托盘左右可视范围时，托盘空白区域支持鼠标或触摸横向拖动。起点命中任一 Piece 时仍优先拿取 Piece；只有起点位于可见托盘内且未命中 Piece 时才滑动。手势只平移 `IsOnTray` Piece，不移动黑色托盘、棋盘或桌面 Piece；首块左边缘和末块右边缘受托盘内边距限制，不允许整组越界。滑动后的世界位置同步为各 Piece 的托盘返回位置，供后续拿取、回弹和重排继续使用。从已滚动托盘拿起 Piece 前必须保存整排当前世界位置；拼错回弹、主动拖回托盘或窗口失焦取消时恢复这份快照，不能调用从托盘左边界重新排布的全量布局，也不能清除当前滚动偏移。成功拼入或合法放到桌面后丢弃快照并保留正常前移补位。
- GameScene 在窗口失焦或应用暂停时立即取消当前指针手势，不等待可能丢失的松手事件。正在拖拽的托盘 Piece 恢复 `TrayScale` 并按当前编号重新排回托盘，正在拖拽的桌面或错误棋盘 Piece 恢复到本次拿起前的合法世界位置；托盘横向滑动只结束手势并保留已经滑到的位置。取消后恢复默认鼠标图标，Piece 不得停留在屏幕边缘。

- 托盘 `40` 设计像素水平间距必须通过 `PieceBoard` 在根 Canvas 中的设计宽度与当前游戏相机世界宽度换算，不能固定使用 `40 / PPU` 世界单位；这样不同卡包触发正交相机自适配后，首次排列、拿起补位和回收重排的屏幕设计间距仍保持一致。
- 托盘最左 Piece 到托盘左边界使用固定 `0.6` 世界单位间隙，即原 `0.2` 的 3 倍；托盘内容溢出后的横向滑动左右安全边距共用该固定值，不随相机缩放。

#### 无 JSON Prefab 批量生成

- 菜单 **Puffies -> Generate CardBag Prefabs** 打开批量窗口，扫描 `UI/CardBags/` 下严格匹配 `CardBagNNN` 的一级目录。
- CardBag 源 PNG 与对应 `.meta` 必须作为一个整体提交和同步，Prefab 内的 Sprite 引用以 `.meta` GUID 为准。Git 不会通过贴图文件名自动修复另一台设备产生的本地 GUID；禁止只提交重新保存的 `CardBagNNN.prefab` 而遗漏同批资源 `.meta` 变化。
- CardBag 生成、位置更新、层级更新和阴影更新只使用各工具原有的源资源、定位、层级和材质校验；不得注册 `OnWillSaveAssets` CardBag 保存守卫，也不得因目标 Prefab 尚未存在、Missing、跨包引用或 GUID 诊断结果阻止新建或覆盖保存。
- Git 拉取、合并或资源导入影响 CardBag Prefab/源目录后，导入监视器可直接解析磁盘 Prefab YAML 的 `m_Sprite` GUID 并与源 PNG `.meta` 对比，在 Console 输出只读诊断。命令行可用 `-executeMethod CardBagPrefabGeneratorEditor.ValidateCardBagReferencesFromCommandLine -cardBagId 19` 校验单包；省略 `-cardBagId` 时校验全部 CardBag。该诊断不参与 Unity 菜单工具的保存流程，跨设备提交正确性最终由 Prefab 与同批 `.meta` 一起提交保证。
- 每个卡包硬性需要 `CardBagNNN/GameBoard.png`、`Previews/CardBagNNN.png` 和至少一张合法 Piece PNG；缺失项会显示在列表中并禁止选择。
- 旧 `background_base.png` 仅用于兼容迁移：当 `GameBoard.png` 不存在时，扫描器通过 `AssetDatabase.MoveAsset` 自动改名并保留 Meta/GUID；两者同时存在时不覆盖目标文件。
- `BoardTitle.png` 是标准资源但采用软校验。缺失时列表显示警告，仍允许生成不含 `BoardTitle` 节点的 Prefab。
- `UI/CardBags/Previews/CardBagNNN.png` 是完整拼图和 Piece 定位参考图，不作为运行时 Prefab Sprite；它必须与 `GameBoard.png` 画布尺寸一致。
- 生成器利用 Piece PNG 保留的原始裁切 RGB，默认在 Preview 完整图中做像素匹配，Piece Alpha 继续作为运行时形状。Preview 因调色板量化、分割线或其他处理无法通过置信度校验时，再尝试 `GameBoard.png`；GameBoard 若已挖空或未保留完整 RGB，回退可以继续失败，不要求所有卡包都维护第二套完整定位图。
- 第二轮不透明像素匹配排除 Alpha 轮廓内侧 `1px`，避免 Preview 分割线或相邻 Piece 覆盖导致正确位置被边缘像素否决。Preview 和 GameBoard 各自沿用同一校验：常规最低匹配率为 `98%`；低于该值时，只有匹配率至少 `90%` 且所选精确 RGB 锚点在当前参考图中唯一才允许生成并记录警告，重复锚点或低于 `90%` 继续报错。两张参考图都失败时错误同时包含两边原因。
- 感知颜色匹配会验证透明像素轮次与不透明像素轮次中“精确 RGB 锚点唯一”的定位结果；该位置本身达到 `78%` 感知匹配率时才加入候选种子，避免重复图案中的偶然同色像素误导定位。首轮 `6px` 感知匹配仍不通过时才使用 `1px` 逐像素网格回退，覆盖细线和高对比图案偏移一个像素即失配的情况。逐像素回退优先执行最低 `78%`、候选差值至少 `1.5%` 的颜色校验；颜色受整批调色影响时，只有颜色仍达到 `65%`、原坐标的 RGB 边缘梯度达到 `85%`，且该结构区域领先远端候选至少 `3%` 才允许生成。搜索细化半径保持 `7px`；颜色、结构和轮廓的独立候选比较按 Piece 短边 `15%` 计算位置簇半径，并限制为 `14~48px`，避免同一槽位的宽匹配峰值被误判成远端重复位置，同时不合并相邻槽位。整包生成会记录已定位 Piece 的高 Alpha 占用，共享边缘像素归属最近定位的 Piece；面积相近的候选若与任一已定位 Piece 主体重叠达到 `65%`，会在精确、颜色、结构和轮廓最终候选阶段被排除，正常边缘接触和面积差异明显的小配件覆盖不受影响。结构校验只验证颜色定位得到的原坐标，不使用结构最高点移动 Piece。颜色与结构均失败且 Preview 包含青色分割边界时，最后使用 Piece Alpha 外边界匹配 Preview 边界邻域；该回退不用于 GameBoard，要求轮廓匹配至少 `75%` 且领先独立远端候选至少 `8%`，不能通过最佳点附近候选占满列表来绕过唯一性校验。
- GameBoard 回退的首个 Piece 达到至少 `99.5%` 且位置唯一后，本卡包后续 Piece 统一使用 GameBoard，日志明确记录参考图。参考图颜色索引保存颜色次数和首次像素下标；唯一颜色锚点直接计算候选位置，不遍历整张画布。
- `PieceGGII.png` 或 `PiecesGGII.png` 中的四位 `GGII` 直接成为正式对象编号并作为人工覆盖规则。全部使用标准 `piece_###.png` 时，生成器按位置从上到下分带、每行最多两个空间组，每组最多 14 片；偶数行从左到右编号，奇数行从右到左编号，形成蛇形组序，组内始终按中心点从左到右编号。最终 Hierarchy 按 `PieceGGII` 升序创建。标准名与显式正式名不得在同一卡包内混用。
- 生成结构为扁平的 `CardBagNNN/BoardTitle`、`CardBagNNN/BoardBgXX`、`CardBagNNN/GameBoard` 和 `CardBagNNN/PieceGGII`。根 Image 不设置 Sprite；生成器按 GameBoard 尺寸自动创建、平铺并裁切默认使用 `BgCardBoard1.png` 的 `BoardBgXX RawImage`，保存前校验直属父级、顺序、数量、位置、尺寸和 UV。
- 窗口默认只选择资源完整且尚无 Prefab 的卡包。选择已有 Prefab 时显示 `Overwrite`，执行前必须确认；覆盖会替换已有层级和手工 Piece 分组。
- 批量生成逐个隔离失败并汇总结果，负责创建带正式自动分组的 Prefab，但不自动烘焙描边；生成时会删除该包可能残留的旧描边。完成生成后执行 **Bake CardBag Outlines**。
- 同一窗口的 **Update Existing Piece Layouts** 用于切图更新后的局部校准。它复用 Preview/GameBoard 定位算法，通过现有 Piece 的 Sprite 资源路径映射节点，只更新 `RectTransform.anchoredPosition` 与 `sizeDelta`；不重建层级、不改变手工分组、Image 参数、影子、旋转缩放或描边资源，也不会自动烘焙描边。更新采用整包事务：源 PNG 与现有 Piece 数量或引用不一致、定位不唯一，或两张有效面积相近的切图在目标位置的高 Alpha 区域重叠达到 `65%` 时，该 Prefab 在保存前失败，避免重复切图覆盖正确布局；面积明显较小且位于大切图内部的独立配件允许更新。
- `Piece001` 到 `Piece999` 的三位顺序名仅作为旧 Prefab 的制作中间状态，不属于正式命名；当前生成器不会再从标准切图创建这类名称。Prefab 中只要仍有任一三位节点，描边烘焙器仍会跳过整包，避免 `Piece100` 等顺序节点被误判为正式分组；卡包没有正式分组时删除对应旧描边目录。
- 当前 CardBag017 为 `1316 x 1316`、38 片，已完成正式 `PieceGGII` 命名并分为 6 组；对应 6 组描边资源已重新生成。CardBag022 当前为 `1300 x 1231`、34 片，按当前 Prefab 的正式命名分为 3 组；旧 `Group04~14` 输出已作为过期资源清理。运行时只按 Sprite 原生尺寸同步 Piece 槽位，不得用受 TextureImporter 降采样影响的 `sprite.rect.size` 覆盖 GameBoard 的 Prefab 设计尺寸。

### 拼图描边渲染

- 拼图描边由 `PuzzleOutlineBakerEditor` 离线生成，并通过 Unity UGUI `Image` 渲染。
- 关卡描边与贴纸描边默认都关闭；默认状态仍显示现有连接区域，不等于完全隐藏全部描边。
- 每个 `GroupNN.png` 独立生成当前阶段需要的线段；不同阶段可以共享交点像素，因为运行时只显示当前阶段蒙版。
- 边界归属除距离外还校验目标组位于边界的正确法线方向；最终外轮廓要求目标组位于轮廓内侧，已完成组接触边要求当前组位于旧组边界外侧。
- 最终外轮廓由所有正式组共同竞争并唯一归属；已完成区域接触边会同时比较当前组和未来组，只在当前组确实位于旧边界法线外侧且是最近合法候选时绘制。
- 已完成组接触边与当前组最终外轮廓只在真实交点附近修补栅格化断口；桥接路径最多 `4px`，且只能在真实边界外 `1px` 的走廊内移动。内部独立接触边不要求连接到最终外轮廓，禁止用长斜线或梯状短线强行连接不同边界组件。
- 烘焙参数按 GameBoard 宽度相对 `1300px` 缩放，并限制在 `0.9~1.1`；不按高度或最大边缩放，避免纵向构图比例影响实际判定尺度。
- 默认连接图会清理小于约 `8px` 的孤立原始边界；贴到纹理画布边缘的孤立段使用约 `12px` 阈值。关卡和贴纸独立描边不参与该清理，真实独立接触边允许保留。
- 烘焙日志包含 8 邻域组件、端点、分支点、组件间最短距离、桥接像素、清理位置和最终边界 assigned/unassigned/ambiguous 统计。分组变化时会自动删除不存在组号的旧默认、关卡和贴纸输出。
- 项目没有运行时描边 Shader、Renderer Feature 或第三方描边包。
- 描边加载与拼图交互保持隔离；缺少描边不得阻止可拖拽碎片创建。
- 运行时 `ActiveGroupOutline` 根节点通过 `CanvasGroup` 控制显示。首组创建和切组创建时 Alpha 初始为 `0`；首次入场或切组的棋盘移动结束后，使用不受 TimeScale 影响的 `0.5s` 平滑淡入。新手引导第一阶段仍完全隐藏烘焙描边，切到第二阶段时按同一移动结束时机淡入。

### 卡包展示与开包表现

- 首页列表的卡包主体使用 `Assets/UI/PackImages/PackIconNNN.png` 静态图；`PackItem.prefab` 不嵌套 3D 卡包特效 Prefab，也不再包含旧 UGUI `PackHighlight` ADD 高光贴片。列表保留封面投影、尺寸图标、`ImgLight` 和美术 Animator。`PackNode.controller` 默认循环播放 `PackAniBreath`；程序不生成或覆盖呼吸曲线，只将完成且无活动会话的灰色卡包速度设为正常速度的 `1/3`，彩色及活动重玩卡包保持正常速度。每个列表项按 PackId 计算稳定的黄金分割归一化起始相位，避免同屏动画同步，并保证刷新与跨设备结果一致。
  - 卡包列表按每页 `18` 个组织为与 Viewport 等宽的横向 `Page_N`。每页固定六列，运行时 Grid 使用 `UpperCenter`，按六列实际总宽在 Viewport 内左右居中。拖拽松手后停止 ScrollRect 惯性，按当前活动页数选择最近页，并在 `0.26s` 内 EaseOut 吸附到完整页面；卡包和空白区域起手共用该逻辑，吸附期间锁定卡包点击但允许新拖拽接管。程序不按单个卡包位置计算 Alpha，也不创建卡包渐隐 CanvasGroup；列表左右柔边统一由 `PackageScrollView/Viewport` 上唯一的 `RectMask2D` 处理，`Softness=(83,0)`。Viewport 不得同时保留旧 `Mask`；其 Image 必须保持 Alpha `0`、RaycastTarget 开启，以隐藏矩形 Graphic 并继续承接空白拖拽。卡包自定义 UGUI Shader 必须读取 `_UIMaskSoftnessX/Y` 并使用标准像素软裁切计算。
- MainScene 主 Canvas 使用 `Screen Space - Camera`，`World Camera` 固定为场景 `Main Camera`，Plane Distance 为 `10`；`MainScene.ConfigureMainCanvas` 在运行时复用统一 Canvas 配置再次校正，确保 `PackItem` 封面、尺寸图标、`ImgLight` 和首页主 UI 经过同一摄像机渲染。
- LoadingScene 启动时并行异步预加载 MainScene 以及当前已解锁卡包的封面/尺寸图；图片使用 `UnityWebRequestTexture` 异步解码并缓存为 Sprite，只有最短 `2.5s`、MainScene 达到 `90%` 待激活点且列表图片预热完成后才显示 `100%` 并开放场景激活。MainScene 列表必须复用该缓存，并按每帧最多 4 个卡包分批创建，不能恢复为首帧逐张 `File.ReadAllBytes + Texture2D.LoadImage` 和集中 Instantiate。
- Unity 编辑器单独打开 Loading、Main、Game、Rank 或 Achieve 场景后，`CanvasDesignResolutionEditor` 会延迟一帧按根 `Canvas` 的世界边界，将 SceneView 恢复为正交正视并自动取景；这避免 Camera Canvas 与 Overlay Canvas 之间切换时继承歪斜视角，不修改场景 Selection、Canvas 配置或运行时行为。`EffectScene001` 不参与自动取景，以保留制作方的三维编辑视角。
- 选中态使用独立 `Screen Space - Camera` Canvas，并复制当前列表实例的完整 `PackNode` 显示，目标封面尺寸为 `600 x 680`；外层只负责统一移动、缩放和淡出，内层保持列表当前撕口、背景、装饰碎片、封面、尺寸标签、材质和 Animator 状态。该 Canvas 与选择面板同样绑定 `Main Camera`。背景使用原分辨率、约 `8px` 半径的两遍可分离高斯模糊，按原色、全不透明显示，不叠加黑色或白色蒙版；返回、拍照和重玩确认继续沿用现有流程。高斯 Shader 位于 `Assets/Resources`，运行时 Material 按需创建和释放，缺失时回退为未虚化原色背景。
  - 独立放大页显示时，`PanelBagSelect` 下的 `BtnBack`、`BtnPlay` 和当前可见的 `BtnCamera` 与卡包放大同时从屏幕下边界外 EaseOut 滑入，按钮时长固定为 `0.39s`，卡包自身仍为 `0.3s`。返回列表或确认进入游戏时，按钮按时间反向曲线滑回同一屏幕外位置；重玩确认和拍照临时覆盖不触发出场。运行时缓存场景原始 `anchoredPosition`，只统一插值 Y 并保持各自 X，不对按钮做 Alpha 渐变；动画期间 Disabled 颜色与 Normal 颜色一致，按钮从首次露出到动画结束始终保持完整不透明度。页面隐藏和再次打开前恢复缓存终点。相机按钮显隐和“玩/重玩”标签继续使用既有卡包状态规则。
  - 只有完整彩色卡包点击“玩”后切换到 `BgGame` 开包舞台；转场和卡包回弹结束后，从 `PackItem.prefab` 克隆 `PackNode` 到当前选中视觉下，关闭克隆中的 `PackCover/PackSize`，只启用 `ImgLight`，恢复 Animator 正常速度并循环播放现有 `PackAni`。提示层按当前选中卡包 Rect 等比缩放；只有有效轻点或达到原横划门槛时才立即停止并释放，无效操作继续循环。列表中的 `ImgLight` 由 `PackAniBreath` 的美术配置决定，程序不修改 `PackAni.anim` 或 `PackAniBreath.anim` 的曲线。
  - 有效操作随机选择 `CardPackOpeningModel_001-006`，共用制作方 `CardPackAnimation.controller`；三位编号正面网格的 `_MainTex` 在运行时替换为当前 `PackIconNNN`。制作方原始 `Model_002~006` 只有正面 Renderer，`Model_001` 额外包含五位编号背面 Renderer；背面存在时会显示制作方 `Bg01.png` 灰块，替换成封面又会形成第二层完整卡包，因此运行时仅在该背面存在时禁用，不能把背面作为模型有效性的必要条件。FBX、骨骼、UV 和动画资源本体不修改。
- 开包特效的混合模式和内部渲染层级归资源配置所有：`fx_chai_w_001.prefab` 保留各 ParticleSystemRenderer 自己的 `sortingOrder`，其 Material/Shader 决定 Additive 或 Alpha 混合；运行时代码不得把所有粒子 Renderer 强制改成同一排序值。卡包正反面 `test.mat`、`test01.mat` 的 Custom Render Queue 固定为 `2001` 并直接保存在 Material 中，运行时材质实例只替换动态贴图。
- 完整彩包使用制作方 `Assets/Resources/Effects/CardFx/Animations/test.playable` 驱动拆包、下落、滑光和完成回调，不得再用 Animator、粒子状态或固定时长手写替代。重新导入后的制作方原始 Timeline 总长约 `5.533s`，从 `0s` 直接播放；`Take 001` 为 `0~1.8333s`，`fx_chai_w_001` 为 `0.5333~5.5333s`，Activation 为 `0~5s`。原版不包含 Recorded 前置放大、`Image` 或 `blur` 轨道，运行时代码不得再以这些后期改造轨道作为启动条件。
- 原始 Activation Track 绑定当前卡包模型 GameObject，唯一的 Animation Track 绑定当前模型 Animator；`fx_chai_w_001` Control Track 绑定 MainScene `PackObject` 下现有实例，Timeline 自带 `particleRandomSeed=2`。运行时不得创建 `Image`、`blur` 代理、可见 Canvas、`blue.mat` 或任何额外蒙版。
- GameScene 交接点使用 Timeline 内 `Take 001` 起点加真实下落关键帧 `0.800s`，即约 `0.800s`。真正开放场景激活前暂停 `PlayableDirector`；跨场景对象加载完成后，把 EffectLayer 31 临时加入 GameScene 自身 MainCamera 的 Culling Mask，等待一帧再从同一 Timeline 时间恢复，场景加载阻塞不得推进动画。完整拆包是否跨场景存活只由 Timeline 是否成功启动决定，不得依赖当前组碎片或散点投影是否可用。
- 完整彩包的 `PlayableDirector` 使用 `DirectorWrapMode.Hold`；播放监控在末帧到达前把 Director 固定到完整时长、Evaluate 并 Pause，随后只标记播放完成并保留整套对象；`PlayableDirector.stopped` 仅作异常漏网兜底。自然完成不释放模型、滑光、临时 Piece、运行时材质或跨场景根对象，也不调用 `Destroy(gameObject)`。不得使用滑光结束、粒子 `IsAlive`、Animator `normalizedTime` 或固定延时提前隐藏、停止或销毁任一部分。准备失败、异常中断或对象被外部销毁时仍可强制清理，但必须先取消完成回调，不能误报自然结束。正式参考是 `EffectScene001` 的“拆包演示”节点及其原始 Timeline 绑定；开包流程继续复用 MainScene 场景 `Canvas/PackObject` 下已调好的光效实例，不通过 `Resources.Load/Instantiate` 创建第二份，也不覆盖根或子节点 Transform、Start Size、材质、粒子模块和相对排序。
- 撕包完整发生在 MainScene 的 `BgGame` 开包舞台，不能移到 GameScene。横向撕口光效与模型由 Main Camera 渲染；模型使用制作方 `EffectScene001` 保存的基准 `Scale=2.63 / localZ=0`。场景光效始终保留在 `PackObject` 原层级，完整使用编辑器中人工调整的根/子节点 Transform、Start Size、发射参数、材质和排序；运行时代码不得设置其位置、旋转或缩放，也不让其继承模型 Stage 的适配。模型尺寸和中心只按正面 `mesh_skin_cardPack_NNN` 计算，背面网格不参与初始对齐。
- 完整彩包开始播放拆包 Timeline 时，模型左下与右下分别显示当前展开卡包真实的 `PackSize` 和 `PackVol` 标签；Vol1 没有 Vol 资源时只显示尺寸标签。标签纹理、颜色、相对中心和显示尺寸直接读取展开态 `PackNode` 中的 `PackSize/PackVol`，因此继续由 `PackItem.prefab` 的美术 RectTransform 决定，不维护另一套固定坐标。运行时将可见标签转换为 EffectLayer 世界空间 SpriteRenderer，并分别绑定到模型对应位置最近的下半部骨骼，在 `LateUpdate` 跟随制作方动画的位移与旋转；模型 Activation 隐藏时同步隐藏。Timeline 在约 `0.8s` 暂停交接时必须记录卡包正面渲染器的真实 Viewport 中心和高度；GameScene 完成动态棋盘相机适配后、Timeline 恢复前，按记录值重新适配整个模型 Stage，并用同一倍率同步两张标签，独立的场景滑光不继承该 Stage 适配。GameScene 根 Canvas 运行时为 `Screen Space - Camera`，因此进入 GameScene 后只允许把当前运行时卡包材质从制作方不透明队列切到透明队列 `3000`，避免模型先于玩法 UI 绘制而被遮挡；不得修改材质资产、Renderer 原始排序、FBX、`test.playable` 或 `fx_chai_w_001` 参数。
- 点击卡包“玩”或确认“重玩”后，选中卡包保持当前位置与尺寸不动；首页根 Canvas 通过同一个 `CanvasGroup` 让卡包列表、`Background` 和其余首页内容保持原位置并同步渐隐，选中页虚化截图按相同进度渐隐，`BgGame` 固定在同一屏幕中心渐现。运行时不得创建额外首页移动容器、不得重排 `PackageScrollView`，也不得横向移动列表、首页背景、虚化截图或 `BgGame`；`PanelBagSelect` 继续向下滑出。完整彩包在 `BgGame` 上播放滑光提示并等待拆包；彩色撕开和灰色撕开跳过拆包。低优先级异步预加载当前 `CardBagNNN` Prefab 并将 GameScene 加载到 `90%` 待激活状态；两种撕开状态都不保留额外最短静止等待，但仍等待 GameScene 达到可激活状态，最长 `5s`，随后将选中卡包 Canvas 临时设为跨场景对象并开放 GameScene 激活。非发牌转场统一使用 `1.2` 时长倍率：首页与游戏背景交接为 `0.504s`，两种撕开卡包均使用 `0.414s` 线性连续下落，GameScene 棋盘入场为 `0.684s`，托盘和按钮入场为 `0.396s`。卡包下移约自身显示高度 `72%` 时，真实 Piece 仍按 `0.027s` 错峰和 `0.39s` 单片飞行时长发牌，不参与该倍率；只有彩色撕开处理其已有的首页展示碎片，灰色撕开没有假碎片。预加载超时、统一两帧稳定和单帧最大推进量不随上述节奏变化。`CardPackGameEntranceTransition` 只保存跨场景卡包、装饰碎片、材质和位置数据，不得在该 Canvas 自身 MonoBehaviour 上启动决定流程推进的协程。GameScene 完成初始化并绑定 Camera 后，先缓存托盘终点并创建当前组真实 Piece；统一稳定两帧后，卡包下落、棋盘与托盘入场、游戏按钮淡入立即在同一帧并行开始，不再为棋盘、按钮或卡包追加独立起步延迟。转场期间会同时存在 GameScene Canvas 和跨场景卡包 Canvas，因此 GameScene 配置 Canvas、实例化 `CardBagNNN` 或挂载场景 UI 时必须按当前激活场景的根对象查找，禁止使用无场景约束的 `FindObjectOfType<Canvas>()`；否则临时 Canvas 释放时会连带销毁玩法对象。每段视觉转场允许“动画时长 + `1s` 宽限”，异常或超时必须强制进入该段终点并继续玩法；MainScene 每次启动时清理残留实例。移交时必须克隆撕口蒙版 Material/Texture，避免 MainScene 卸载缓存资源破坏转场视觉。GameScene 实例化卡包时优先复用 PackId 匹配的预加载 Prefab，预加载失败或直接进入则回退同步 `Resources.Load`；玩法初始化、Collider 精度、托盘目标位置、顺序和 Piece 缩放规则不变。
- 彩色撕开的跨场景 Canvas 会带入仅用于首页展示的 `ProgressPieces`，它们不得参与实际发牌；本条覆盖上一条中的旧淡出和首帧隐藏描述。卡包开始下移后，展示碎片相对卡包向下收约其父容器高度 `28%`，低于撕口后隐藏。GameScene 创建的真实 Piece 同时在卡包中心附近的小范围散点以最终 `TrayScale` 和正常 Alpha 准备，并在统一两帧稳定及托盘目标缓存完成后才允许跨场景卡包开始下移；真实 Piece 由上层卡包遮挡，卡包移开时自然露出，不得等到卡包越过碎片位置后再补显。卡包下移约自身高度 `72%` 时，真实 Piece 以 `0.027s` 错峰和 `0.39s` 单片时长飞向各自托盘终点；展示碎片继续随卡包收进撕口并在该节点隐藏。
- 彩色撕开和灰色撕开使用相同的跨场景动画节奏：背景交接后不增加额外静止时间；真实 Piece 在卡包移动前以最终 `TrayScale` 和正常 Alpha 创建在卡包中心附近的小范围散点，起始散点半径统一使用基础 `0.025~0.049` 世界单位的 `20` 倍，目标屏幕半径约 `50~100px`；起飞点为卡包下移约自身显示高度 `72%` 的位置，起飞点前后两段按距离占比分配同一个 `0.414s` 总时长并使用线性位移，最终下移距离按卡包实际顶边到 Canvas 底边动态补足；Piece 以 `0.027s` 错峰和 `0.39s` 单片时长飞向托盘。两者唯一动画差异是彩色撕开需要把首页展示用 `ProgressPieces` 下收至撕口并隐藏，灰色撕开没有假碎片，不执行该操作。
- 完整彩包的拆包阶段是上述通用跨场景规则的专用前置分支：从 `CardBagNNN.prefab` 选择首个未完成组且只读取尚未拼上的真实 `PieceGGII` Sprite，以场景 `PackObject/fx_chai_w_001` 的实际世界位置作为撕口锚点。撕开滑光在模型播放后约 `0.5333s` 启动，同一帧全部交接碎片从撕口下方、卡包模型后层以最终显示尺寸出现，并在 `0.32s` 内 EaseOut 向上短跳；跳跃中心高度约为卡包显示高度 `8%`，不做 Alpha 渐显、逐片延迟或 `40% -> 100%` 缩放。跳跃终点直接使用与撕开包真实 Piece 相同的黄金角散点公式及 `20` 倍半径。交接视觉仍沿用首页 `86px * 1.4` 最大边基准，并随卡包从 `240x272` 到 `600x680` 同比例放大，展开状态最大边约 `301px`。
- Timeline 到达约 `0.800s` 的下落交接点后，MainScene 保存交接碎片散点中心，隐藏并销毁选中静态卡包内容，不恢复第二个撕开静态包、不执行额外卡包下落，也不创建 `CardPackGameEntranceTransition`。GameScene 当前组真实 Piece 从相同中心用 `GameDefine.CalculatePieceDealScatterOffset` 重建同一套 `20` 倍散点，以最终 `TrayScale`、`0.027s` 错峰和 `0.39s` 单片时长飞向托盘；棋盘、托盘和按钮入场参数与彩色撕开一致。制作方模型和滑光继续跨场景播放到 Timeline 约 `5.533s`，播放监控把 Director 保持在末帧并只结束播放状态，不回零、不自然释放或销毁。完整彩包已经完成模型撕开，因此不重复等待卡包下落起飞点；彩色进行中包与灰色重玩包继续使用可见卡包下落到 `72%` 起飞点后的发牌规则。
- 灰色撕开完成包打开 `PanelReplay` 时只临时隐藏当前选中卡包 Canvas。确认重玩后必须先关闭弹窗并恢复同一份选中视觉，保留其灰色材质、撕口蒙版、尺寸和位置，再清空/创建会话并进入统一转场；禁止从已隐藏的首页槽位重新克隆，因为克隆会继承被关闭的 Image 状态而造成灰色卡包后续动作不可见。
  - 撕包模型和光效直接放入 MainScene 世界并由 `Main Camera` 渲染；运行时将 EffectLayer 加入主相机 Culling Mask，按居中静态卡包的真实屏幕中心和高度等比定位整个 Stage，结束或中断时恢复原 Culling Mask。`BgGame` 开包背景使用同一主摄像机下的世界 `SpriteRenderer`，以不透明几何队列先于卡包模型绘制，不能放在高 Sorting Order 的全屏 UGUI Canvas 中覆盖模型。最终画面不创建独立特效相机、RenderTexture、RawImage 或撕口蒙版采样；旧透明二次合成路径出现的粒子外围黑色矩形属于混合异常，不是制作方特效内容。
  - 静态封面切换到 3D 模型时，运行时在制作方原始 Timeline 的 `0s` 首帧计算模型正面边界，将模型与当前已经放大的 `600x680` 静态卡包尺寸和中心对齐，再隐藏静态视觉并从 `0s` 播放。原版没有 Recorded 前置放大轨，运行时不得自行补建、偏移或改写 Timeline 来重复选择页放大阶段。
- `Assets/Scenes/EffectScene001.unity` 仅用于核对制作方配置和 Timeline，不加入正常场景导航。列表不加载 3D 模型、撕包粒子、特效 Skybox、Directional Light 或 `CardPackListUnlit.shader`；列表动效由美术在 `PackItem.prefab` 的 Animator 中维护。
- 点击结算 `BtnFinish` 后，顶部 `TaskBg2/TaskItem` 先按入场的反向节奏收回屏幕上方，底部 `ImgBagBg/BtnFinish/BtnCamera` 同时收回屏幕下方；顶部 `0.52s`、底部 `0.42s`，不改变透明度、缩放或旋转。奖励始终以编辑器中组装好的完整 `BagRewardItem` 为单位：GameScene 的 `ImgBagBg` 在编辑器中预置 `BagRewardItem` 与 `BagRewardItemSecondary` 两个完整实例，单奖励启用一个、双奖励启用两个，运行时不得克隆奖励卡对象；`BagCover` 与该实例自身的 `FX_ui_jieSuo_w` 不拆分。特效可位于 `Canvas` 下的中间容器中，代码必须在每个 `BagRewardItem` 自身范围递归定位，禁止跨实例共享。双奖励左右分槽实际移动的是各实例的 `BagCover`，转场接管后必须将 Canvas 下对应的特效布局父节点对齐到该实例 `BagCover` 的真实中心，只允许调整这个外层布局父节点，不修改特效内部 Transform、粒子参数或 Renderer 排序。结算页不得单独复制 `BagCover` 创建临时飞行图或倒影；只有本局实际分配到正数 PackId 的奖励才显示，待发但未分配的任务奖励不显示问号占位。退场前整个实例从 `ImgBagBg` 提升到 `RewardPanel` 并保持原显示位置；退场结束后再整体交给 `CardPackRewardFlyTransition` 的持久化 Canvas。两次换父都只能移动和等比缩放完整根节点，并以 `BagCover` 的真实屏幕矩形恢复位置与尺寸，不得重置 Prefab 内部 Canvas 或 `BagCover` 的 Transform。持久化 Canvas 使用 `Screen Space - Camera`，场景切换后重新绑定当前 `Main Camera` 并全程使用 `sortingOrder=1`，同时保留美术粒子自身 `0~112` 的前后层次。结算到首页不创建 `SceneSnapshot`、`RenderTexture`、`RawImage`，不捕获 GameScene 最后一帧；现有完整 `BagRewardItem` 作为 `DontDestroyOnLoad` Canvas 的子节点直接跨场景，MainScene 就绪后由同一对象继续飞行。MainScene 卡包列表从分批创建开始到完成排序、目标槽缓存和离屏布置期间整体预隐藏，避免创建协程让帧时先闪出其他卡包。完整 `BagRewardItem` 以单张 `0.72s`、多张 `0.12s` 错峰弧线一边移动一边等比缩放到对应列表卡包的实际尺寸；每张卡落位后独立播放自身 `FX_ui_jieSuo_w`，不复制特效、不换父、不覆盖粒子参数。约 `0.3s` 到各自首闪时只隐藏该实例的问号 `BagCover`，同帧显示真实卡包，特效和完整奖励根节点必须继续保留；全部奖励完成真实卡揭晓后，其余卡包按单卡 `0.44s`、错峰 `0.055s` 从屏幕下方依次滑入并恢复首页输入。每份特效继续独立播放，有限粒子全部结束或到达按自身粒子参数计算的容错结束点后，才停止该实例的循环粒子并回收对应 `BagRewardItem`。无真实奖励时仍执行整列上滑；系列后层奖励映射到系列实际列表槽。动画取消时必须恢复 GridLayout、ScrollRect、列表位置和输入。
- 结算返回首页时，`ImgBagBg` 的最新时序覆盖上一条“随底部 UI 在 GameScene 退场”的旧描述：黑色半透明条不提前下移，而是与完整奖励卡一起进入持久化转场 Canvas；MainScene 目标槽准备完成且奖励卡开始飞行 `0.08s` 后，`ImgBagBg` 再沿用 `0.42s` 三次缓入节奏向下移出屏幕。奖励卡仍独立换父和飞向列表，背板不得跟随卡包一起飞向目标槽。
- `CardPackRewardFlyTransition` 在奖励卡揭晓、首页其余列表入场及输入恢复后，允许仅为 `FX_ui_jieSuo_w` 长尾粒子继续跨场景存活；进入 GameScene 时必须清理该上一轮实例，当前结算创建新奖励转场前也必须再次防御性清理，避免旧静态实例阻止新转场。无实际奖励的结算仍创建空奖励转场并接管 MainScene 列表上滑，不能因为旧实例冲突而直接加载一个永久预隐藏列表的 MainScene。
- GameScene 进入结算后必须停止分发玩法鼠标/触摸输入，并由 RewardPanel 根 `CanvasGroup` 在所有入场、积分和奖励动画完成前统一禁用子级交互；完成后才恢复 `BtnFinish` 与 `BtnCamera`。奖励底板、首次完成奖励和任务奖励的并行动画必须使用有界等待，异常未回调时恢复各对象最终可见状态并继续按钮入场，不得让结算主流程永久等待。
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
- 本地缓存由根目录 `ProjectMaintenance.ps1` 维护：默认只审计，`-Clean` 达到阈值才删除白名单缓存；每台 Windows 设备需单独执行一次 `-InstallScheduledTask`，注册每周日 `03:00` 的本地任务。Git 会同步脚本与规则，但不会同步 Windows 计划任务。

---

## 9. 编辑器菜单参考

| 菜单 | 用途 |
|------|------|
| Puffies -> Sync Build Resources | 将运行时磁盘加载的 UI 目录复制到 StreamingAssets |
| Puffies -> Apply Design Resolution (Current Scene) | 为当前场景应用 Canvas 设计分辨率 |
| Puffies -> Apply Design Resolution (All Scenes & Prefabs) | 为全部场景和 Prefab 应用 Canvas 设计分辨率 |
| Puffies -> Setup Default Chinese Font | 设置中文字体 |
| Puffies -> Bake CardBag Outlines | 为每个 CardBag Prefab 重建各分组外边界描边 |
| Puffies -> Generate CardBag Prefabs | 扫描完整背景和透明碎图；窗口可选择完整生成 CardBag Prefab，或仅按效果图更新现有 Piece 的位置与原生尺寸 |
| Puffies -> Update CardBag Configs | 扫描 CardBag 源资源的碎片 PNG 数量并更新 `CardPacks.csv/PackSize` 与 `StickerCount`；仅在 `AutoUpdate=1` 时同步 `BoardScale`，并始终保留手工 `Series` 内容 |
| Puffies -> Apply CardBag Shadows | 为全部 CardBag Prefab 的 GameBoard/BoardTitle 绑定 01、凹槽 Piece 绑定 03，并补齐投影网格组件 |

---

## 10. 已弃用

- `Assets/ArtRes/`、`Assets/Configs/`
- `Resources/Config/Package001.json` 及 JSON 拼图配置流程
- `Tools/*.ps1` 下的一次性迁移脚本
