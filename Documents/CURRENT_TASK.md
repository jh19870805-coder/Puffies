# 当前任务

## 2026-09-03 MainScene 通用确认弹窗

- 状态：代码实现和编译验证完成，等待 Play Mode 验收。
- 用户意图：退出游戏与保存页删除进度共用 `PanelConfirm`；弹窗按入口切换提示内容和确认行为，返回只关闭弹窗。
- 实现：`PanelMenu/BtnExit` 显示“确认退出游戏？”，确认后 Player 调用 `Application.Quit()`、Editor 停止 Play Mode；`PanelSave/BtnDelete` 显示“确认删除进度存储？”，并锁定点击时选中的槽位，确认后调用现有 `LocalSaveSlotUtility.DeleteSlot` 并刷新保存页。`BtnYes` 统一确认，`BtnNo`、`BtnClose` 及其他 Button 统一取消。
- 保留：原 `PanelReplay` 卡包重玩确认逻辑和用户当前 `MainScene.unity` 改动不变。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、统一 spec、任务记录与项目上下文。
- 验证：场景静态检查确认 `BtnExit`、`BtnDelete`、`PanelConfirm/TextContent`、`BtnYes`、`BtnNo` 和 `BtnClose` 结构正确；Runtime/Editor 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。
- 下一步：在 Play Mode 分别验证退出与删除入口的文案；验证 `BtnNo/BtnClose` 不产生业务操作、删除确认后对应槽位清空，以及退出确认停止 Play Mode；Windows 构建中确认退出按钮关闭进程。

## 2026-09-03 结算流程性能优化

- 状态：代码优化和编译验证完成，等待 Play Mode 体感与 Profiler 数据验收。
- 用户意图：优化最后一片完成到结算展示，以及结算返回首页的卡顿；保持现有动画节奏、分数、任务和奖励逻辑不变。
- 已确认根因：RewardPanel 激活前同帧执行棋盘清理、Canvas 重建、SQLite 完成数查询、卡包完成写入和拼图会话删除；任务结算阶段还有多次 JSON/SQLite 写入；奖励特效长尾等待每帧执行 `GetComponentsInChildren<ParticleSystem>` 并分配数组。
- 已完成：进入 GameScene 时缓存完成数；RewardPanel 首帧显示后再持久化；SQLite 集合 Upsert 合并为单条原子语句；会话删除不再额外确认查询；奖励粒子组件初始化时缓存；增加 Profiler 标记与仅 Editor/Development 的耗时日志。
- 修改范围：`GameScene.cs`、`CardPackRewardFlyTransition.cs`、`CardPackDataUtility.cs`、`LocalDataStore.cs`、统一 spec 和项目记录。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 连带 Runtime 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过，仅有既有换行提示。
- 下一步：在 Play Mode 各完成一次首次通关和重玩，观察 Console 的 `GameScene: settlement performance` 耗时，并比较最后一片到 RewardPanel、点击完成到首页飞行动画两段体感。

## 2026-09-03 GameScene 动态窗口尺寸适配修复

- 状态：已根据 Unity Editor 实际诊断日志完成第二版根因修复并删除临时诊断代码，编译通过，等待按“拉窄 -> 拉宽”复验。
- 根因一：UGUI 托盘矩形先归一化到 Canvas，但随后错误映射到整个 `Screen.width/height`；固定 `16:9` 相机产生左右或上下黑边时，黑边也被算进托盘世界边界，导致托盘 Piece 尺寸和位置偏离可见托盘。
- 根因二：棋盘每次居中都用 Prefab 初始 `anchoredPosition` 加“当前中心差”；第二次刷新时当前中心已含上次偏移，因此会反向抵消上次结果，造成棋盘在重复刷新后漂移或不居中。
- 修改：托盘 Canvas 边界统一映射到 `Camera.pixelRect`；托盘回收热区改为相对有效相机视口归一化并在最终布局后重建；棋盘按当前 `anchoredPosition` 增量应用本次中心差。托盘 Piece 随稳定帧继续统一重算缩放与排布。
- 保留：固定 `16:9` 与黑边、Piece 托盘缩放上限、拿起恢复原尺寸、吸附/回弹、固定间距、托盘滚动和动画参数均未修改。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、统一 spec 与项目上下文。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 成功并连带编译 Runtime，结果 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有 LF/CRLF 提示。
- 实际日志根因：错误帧的 `Screen=1086x611`、`cameraAspect=1.778` 正常，但根 Canvas 的 Rect 为 `0x0`；代码仍继续布局，导致托盘实际矩形被锚到屏幕中部 `y=305.5`。完整 `ConfigureGameplayCanvas` 在尺寸检测和最终刷新阶段重复设置根 RectTransform/Canvas 渲染状态，是该无效帧反复出现的直接原因。
- 第二版修复：窗口变化时只调用固定宽高比相机视口刷新，不再重复初始化 Canvas；最终布局增加根 Canvas 有效尺寸门槛，若仍为 `0x0` 则逐帧延后，直到 Canvas 恢复后才重算相机、棋盘、托盘和 Piece。
- 下一步：重新进入 Play Mode，按“拉窄 -> 拉宽”复现，确认托盘 Piece 不再进入棋盘中部且棋盘恢复居中；通过后再覆盖连续多次往返拉伸。

## 2026-09-03 正确 Piece 顶回错误 Piece 后托盘首槽空缺

- 状态：根因修复和编译验证完成，等待 Play Mode 按截图路径复验。
- 根因：正确 Piece 达到吸附标准后，代码先调用 `ReturnLoosePiecesToTray` 收回占位错误 Piece并刷新托盘，随后才把正确 Piece 标记为离开托盘。布局计算因此仍给即将吸附的正确 Piece 保留了一个槽位；它下一步进入棋盘后，托盘前方留下不可滚动消除的空槽。
- 修改：吸附目标确认后，在收回错误 Piece 之前先将本次全部正确吸附成员的 `IsOnTray` 设为 `false`。错误 Piece 的收集、拆组、回弹、进入托盘后闪红、`0.5s` 补位、随机顺序和滚动边界均保持原逻辑。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、统一 spec 与项目上下文。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 成功并连带编译 Runtime，结果 `0` 警告、`0` 错误；静态调用顺序已确认托盘重排不会再包含本次正确吸附 Piece。
- 下一步：在 Play Mode 将错误 Piece 放到另一 Piece 的正确凹槽，再用正确 Piece 将其顶回托盘；确认错误 Piece 回弹闪红、托盘全排连续补齐、最左侧没有空槽且可正常滑到左右边界。

## 2026-09-03 CardBag010 合并为单组

- 状态：已完成 Prefab 分组调整和单关描边重烘焙，等待 Play Mode 实际发牌验收。
- 修改：仅将 `CardBag010.prefab` 原第二组 `Piece0201~Piece0212` 顺延为第一组 `Piece0114~Piece0125`；原第一组 `Piece0101~Piece0113` 不变，全部 25 片现在统一属于 `01` 组。Sprite、坐标、尺寸、材质和层级引用均未改变。
- 描边：只重烘焙 `CardBag010`；`Group01` 默认、关卡和贴纸描边已更新，旧 `Group02` 三张输出及其 `.meta` 已自动删除。合并后的默认描边为完整棋盘外边框，符合该关方形 Piece 全部组成单组的结果。
- 验证：静态扫描为 `PieceCount=25`、`UniqueCount=25`、`Groups=01`，命名连续覆盖 `Piece0101~Piece0125`；输出目录只剩 `Group01.png`、`Group01_Level.png`、`Group01_Stickers.png` 及原有 `.meta`。一次性 Editor 执行器已自动删除，没有留下工具代码。
- 下一步：在 Play Mode 进入或重玩 `CardBag010`，确认首次发牌一次给出全部 25 片、托盘可横向滑动、完成后直接结算且描边显示正确。已有进行中存档仍含旧第二组 Piece 编号时，测试前应通过重玩流程建立空会话。

## 2026-09-03 Steam 与 GameAnalytics 运营统计接入

- 状态：已完成。两个 SDK、统一 Manager 和 GameScene 事件调用已接入，Runtime/Editor 编译、Editor 隔离及 Windows 非 Development Demo Player 实际到数均已验收。
- SDK：通过 OpenUPM 固定 `GameAnalytics 8.2.0` 和 `Steamworks.NET 2025.164.1`，`packages-lock.json` 已锁定版本。Steamworks.NET 自动要求的 `STEAMWORKS_NET` Standalone 编译符号已保留。
- Steam 环境：当前默认 Demo App ID 为 `5034540`，项目根 `steam_appid.txt` 已从 SDK 默认测试值 `480` 改为 Demo ID；定义 `PUFFIES_STEAM_RELEASE` 时切换到正式 App ID `4906510`。App ID 只在 `AnalyticsManager` 集中配置。
- 身份与隐私：Windows 非 Development Player 启动时初始化 Steam，读取 SteamID 后生成 SHA-256 匿名 ID，再初始化 GameAnalytics；不上传 Steam 昵称、邮箱或本地路径。Steam、网络或 Key 配置失败只告警，不阻塞游戏。
- 事件：进入关卡记录 `Start/CardBag/NNN`，成功保存且完成分数计算后记录 `Complete/CardBag/NNN/score`，未通关点击返回记录 `Fail/CardBag/NNN` 和 `LevelExit:CardBagNNN:ReturnButton`；重玩额外记录 `LevelReplay:CardBagNNN`，首次通关同步记录当前唯一完成卡包数。
- 隔离：Unity Editor 与 Development Build 不初始化 Steam、不初始化 GameAnalytics、不发送运营事件，因此“一键完成”测试不会污染正式数据。强制关闭由 GameAnalytics Session 中存在 Start 且没有 Complete/Fail 识别。
- 修改文件：`Assets/Scripts/Model/AnalyticsManager.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Resources/GameAnalytics/Settings.asset`、`Packages/manifest.json`、`Packages/packages-lock.json`、`ProjectSettings/PackageManagerSettings.asset`、`ProjectSettings/ProjectSettings.asset`、`steam_appid.txt`、统一 spec 与项目上下文。
- 验证：Unity Package Manager 已成功导入两个固定版本；`dotnet build Assembly-CSharp.csproj --no-restore -nologo` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；Editor Play Mode 未初始化 Steam 或 GA。Windows 非 Development 构建通过本地 Steam Demo App ID `5034540` 成功初始化 Steam 和 GameAnalytics，用户已在 GameAnalytics Live events 确认 Session、Start、Complete、Fail 与 Replay 数据全部到账；`git diff --check` 已通过，仅有仓库既有的 LF/CRLF 提示。
- 下一步：后续需要运营报表时，基于现有事件配置关卡漏斗、通关率、中途退出率和重玩次数面板；制作正式版构建时再启用 `PUFFIES_STEAM_RELEASE` 并通过正式 App ID 验证隔离。

## 2026-09-03 首播与场景切换敏感节点预热

- 状态：代码、音频导入设置与跨场景预加载链已优化，Runtime/Editor 编译及静态校验通过；等待 Unity Play Mode 体感与耗时复验。
- 启动音频：`AudioManager` 不再在首个 Scene 前同步加载 Catalog。LoadingScene 现在并行异步读取 Catalog，预热 `BGM_MainMenu` 和全部 19 个短音效，并在这些资源就绪后才进入 MainScene；等待上限为 `10s`，失败不会永久卡住加载页。
- 内存策略：6 首 BGM 合计约 `31 MB`，统一使用 `Streaming + loadInBackground`，不整体解压常驻；19 个 SFX 的 MP3 合计约 `562 KB`，使用 `DecompressOnLoad + preloadAudioData + loadInBackground`。Editor 自动同步器会为后续新增的 `BGM_`/`SFX_` 资源继续应用相同规则。
- 游戏转场：用户进入卡包放大等待页时，现有 CardBag Prefab 和 GameScene 预加载继续执行，同时后台准备该卡包/系列已固定的 BGM，以及 6 个开包模型、AnimatorController、Timeline 和相关材质。GameScene 激活时复用已确定的 BGM 文件名，不重复查询 SQLite。
- 开包节点：撕包动画的曲线、坐标、播放起点和场景交接时间未修改。开包所需碎片优先从已预载的 CardBag Prefab 读取，公共特效资源在玩家可以点击/滑动之前准备完成；开包资源等待上限 `5s`，失败时保留原同步降级。
- 审计结论：进入游戏链原本已经异步预载 CardBag Prefab 和 GameScene 到 `0.9`，该设计保留；本轮移除了音频首次解码、开包公共资源同步读取、开包阶段重复 CardBag 加载和 GameScene 激活帧重复 BGM SQLite 查询。GameScene 激活后的 Prefab 实例化、布局与 Piece 创建属于下一步需用 Profiler/现有 bootstrap 日志定量判断的部分，本轮未在无数据情况下改写玩法初始化。
- 修改文件：`Assets/Scripts/Model/AudioManager.cs`、`GameDefine.cs`、`Assets/Scripts/Controller/LoadingScene.cs`、`MainScene.cs`、`GameScene.cs`、`Assets/Scripts/Editor/AudioCatalogEditor.cs`、25 个 `Assets/Audios/*.mp3.meta`、统一 spec 与项目上下文。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 通过并连带生成 Runtime，结果 `0` 警告、`0` 错误；BGM 导入设置 `6/6`、SFX 导入设置 `19/19` 校验正确；`git diff --check` 通过，仅有仓库既有 LF/CRLF 提示。尚未完成 Play Mode 体感验收。
- 下一步：从 LoadingScene 启动，依次复验首次首页点击、首次撕包、首次碎片分发、首次 Piece 拿取/放置；记录 Console 中 `GameScene bootstrap completed in ...ms`。若仍有明显场景激活卡顿，再依据 Profiler 数据拆分 GameScene 的 Prefab 实例化、布局和 Piece 创建，不盲目调整动画时间。

## 2026-09-03 统一音乐音效播放系统

- 状态：代码、资源目录与持久化已接入，Runtime/Editor 编译及静态完整性验证通过；等待 Unity Play Mode 听感和完整事件时序复验。
- 单例接口：新增常驻 `AudioManager`，业务代码可直接调用 `AudioManager.Instance.PlayMusic("BGM_MainMenu.mp3")` 或 `PlaySfx("SFX_ButtonClick.mp3")`。BGM 单独循环播放，SFX 使用 `PlayOneShot` 支持并发；文件名大小写不敏感，也兼容不传 `.mp3` 扩展名。
- 资源目录：新增 `AudioCatalog` ScriptableObject 和 `Assets/Resources/AudioCatalog.asset`，引用 `Assets/Audios` 的全部 `25` 个 AudioClip。`Puffies/Update Audio Catalog`、音频资源变动回调和构建前处理会自动同步目录，不需要手工拖引用。
- 背景音乐：MainScene 固定播放 `BGM_MainMenu.mp3`；GameScene 首次进入卡包时随机 `BGM_Gameplay_01~05.mp3`。选择以稳定文件名写入当前 SaveSlot 的 SQLite `GameAudioPreferences`；普通卡包按 PackId 保存，系列卡包按系列链首 PackId 保存并由全系列复用。
- 音效接入：已在真实业务完成点接入通用按钮、卡包点击、弹窗切换、重玩确认、拆包、碎片分发、拿起、普通放置、错误回弹、正确吸附、组切换、普通提示抖动、最后散落块提醒、拼图完成、分数滚动、奖励出现、奖励落位、系列切换和设置开关音效。无效或被锁定输入不会播放对应业务音效。
- 音量：`SliderMusic` 与 `SliderEffect` 分别作用于 Manager 的 BGM/SFX AudioSource；`AudioListener.volume` 固定为 `1`，移除原先与 AudioSource 同时缩放造成的二次乘算。其他场景既有 AudioSource 仍按对象名中的 `music/bgm` 分类应用音量。
- 修改文件：`Assets/Scripts/Model/AudioManager.cs`、`AudioCatalog.cs`、`LocalDataStore.cs`、`GameConfigRepository.cs`、`CardPackPhoto.cs`、`CardPackRewardFlyTransition.cs`，`Assets/Scripts/Editor/AudioCatalogEditor.cs`，`Assets/Scripts/Controller/MainScene.cs`、`GameScene.cs`、`RankScene.cs`、`AchieveScene.cs`，`Assets/Resources/AudioCatalog.asset` 及对应 `.meta`，并更新统一 spec 与项目上下文。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 通过，结果 `0` 警告、`0` 错误；Unity 已成功导入独立 `AudioCatalog.cs` 并输出 `AudioCatalog updated: 25 clips.`；Catalog 与 `Assets/Audios` 的 GUID 对比为 `25/25`、无缺失、无额外、无重复；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 提示。自动 Play Mode 快捷键未被当前 Unity 窗口接受，尚未完成实际听感验证。
- 下一步：在 Unity 从 LoadingScene 进入 MainScene，确认首页 BGM；分别进入普通卡包和系列卡包，复验音乐固定、切档隔离、音量滑杆与 19 类事件音效的音量和时序。若某个美术音频需要提前或延后，只调整该事件的调用点，不修改玩法动画节奏。

## 2026-09-03 音频资源统一命名

- 状态：`Assets/Audios` 内全部 MP3 与 `.meta` 已完成统一重命名；播放逻辑已由上方“统一音乐音效播放系统”任务接入。
- 规则：背景音乐统一使用 `BGM_` 前缀，短音效统一使用 `SFX_` 前缀；文件名只使用 ASCII、PascalCase 功能名和必要的两位数字序号，目录继续保持单层扁平。
- 结果：`6` 个背景音乐重命名为 `BGM_MainMenu.mp3` 与 `BGM_Gameplay_01~05.mp3`；`19` 个音效按实际调用语义重命名为 `SFX_ButtonClick.mp3`、`SFX_CardPackOpen.mp3`、`SFX_PieceCorrect.mp3` 等稳定名称。完整旧名与新名映射记录在 `specs/spec-driven-development.md` 的“音频资源统一命名”章节。
- 资源完整性：重命名时 MP3 和对应 `.meta` 成对移动；`25/25` 个 MP3 的 SHA-256 保持不变，`25/25` 个 `.meta` GUID 保持不变，没有目标冲突、重复 GUID或遗留中文文件名。
- 修改范围：仅 `Assets/Audios` 文件名及工程记录；没有修改音频内容、Unity 导入参数、场景、Prefab 或播放代码。
- 后续：音效调用表和“卡包或系列首次游玩随机 `BGM_Gameplay_01~05`、本地持久化后永久复用”规则已经实现，当前只剩 Play Mode 听感与时序复验。

## 2026-09-03 结算动画连续点击卡死修复

- 状态：代码和规格记录已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 连点复验。
- 根因与风险：GameScene 进入结算后仍在 `Update()` 中分发玩法鼠标/触摸输入；同时奖励底板、首次完成奖励和任务奖励使用三个独立协程及布尔完成回调汇合，任一子协程未正常回调时，主结算协程会永久等待，表现为卡包消失且完成按钮无法出现。
- 修复：结算开始立即清除 EventSystem 旧选中态，通过 RewardPanel 根 `CanvasGroup` 锁住全部子级交互并继续承接全屏射线；`GameScene.Update()` 在 `_isGameFinished` 状态下不再分发玩法输入。全部结算动画结束后才统一恢复 `BtnFinish` 和 `BtnCamera`。
- 容错：奖励并行动画增加有界等待；若子动画未正常完成，停止残留协程并把 `ImgBagBg`、首次完成奖励和任务奖励恢复到各自最终位置、尺寸及可见状态，然后继续按钮入场，避免页面永久卡住。现有动画时长、路径、缩放曲线和视觉资源未修改。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`specs/spec-driven-development.md`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 通过，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在无奖励、单奖励、双奖励结算动画中分别连续点击空白区域，确认卡包不中断、按钮只在动画结束后启用且页面可正常返回首页。

## 2026-09-02 任务奖励独立发包上限

- 状态：代码与需求记录已修改，编译验证已完成，等待 Unity Play Mode 数据和动画复验。
- 规则：`R/H` 章节阶段门槛只用于首次完成后的自然结算发包。任务奖励只要存在符合章节、系列前置关系的锁定候选，并且全局可玩卡包数 `H=Unlocked+InProgress` 小于最大持有数 `6`，就立即发放；`H>=6` 时保留为待发权益，后续任意成功结算出现空位后重试。
- 顺序：待发任务奖励仍先于本轮自然结算发包处理；任务奖励发放后，自然结算使用更新后的卡包状态继续执行原阶段判定。同一轮满足条件时仍可获得两个卡包。
- 结算表现：本轮任务权益成功入队就从任务位置播放问号卡包飞入；真正分配到 PackId 后参与点击完成后的首页列表飞入。达到全局上限而待发时只播放结算占位动画，不提前解锁或飞入首页。
- 修改文件：`Assets/Scripts/Model/CardPackDataUtility.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`、`specs/task-and-settlement.md`。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore` 通过，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在 Play Mode 验证截图场景 `R=7、H=3` 会立即发放任务卡包并播放完整动画，同时验证 `H=6` 时权益保持待发，完成一个可玩卡包降到 `H=5` 后下一次结算会补发。

## 2026-09-02 无奖励重玩返回首页列表不可见

- 状态：根因已由运行日志确认，代码修改与编译验证已完成，等待 Unity Play Mode 复验。
- 根因：上一轮首页奖励卡已落位、列表也已恢复，但 `FX_ui_jieSuo_w` 的长尾粒子仍让旧 `CardPackRewardFlyTransition` 存活约 `16s`。玩家在此期间进入重玩后，无奖励结算创建新转场时被旧静态实例拒绝，只能直接加载 MainScene；新 MainScene 又因旧实例仍为 `IsActive` 把卡包列表预隐藏，而旧实例已经结束首页入场，不会再次恢复这份列表。
- 修复：进入 `GameScene` 时结束上一轮遗留的首页奖励长尾转场；点击结算完成并准备创建本轮转场前再次防御性清理旧实例。无奖励结算仍会创建本轮空奖励转场，继续执行首页卡包列表从屏幕下方依次上滑，不改为直接显示或生硬切换。
- 保留：重玩不产生首次完成奖励、任务奖励独立兑现、已有奖励飞入与解锁特效时序均不修改。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：`Editor.log` 已确认故障前存在上一轮 `CardPackRewardFlyTransition` 长尾、随后发生无奖励重玩结算及新 MainScene 列表刷新；`dotnet build Assembly-CSharp-Editor.csproj --no-restore` 通过，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在上一轮解锁特效尚未结束时立即进入重玩，完成后点击结算完成，确认首页列表正常上滑并恢复交互。

## 2026-09-02 重玩完成任务未发卡包修复

- 状态：根因已确认，代码与需求记录已修改，等待 Unity Play Mode 数据验收。
- 根因：`TaskConfig.csv/CountReplay=1` 允许重玩累计三类任务，所以重复完成 M 包能正常推进并完成任务；但结算代码随后以 `_wasSelectedPackCompletedOnEntry` 为条件跳过了整个待发任务奖励分配，导致任务已经推进到下一条、保底权益也已经写入 `CardPackDistribution/Progress`，却没有在本局获得卡包。
- 修复：重玩仍不执行首次完成/游戏结束发包；任务奖励改为独立处理。任何成功保存的结算都会尝试兑现已赚到的待发任务权益，因此重玩中刚完成任务时会立即按现有章节、系列和全局最大持有数量尝试发包，旧版本已留在队列中的任务权益也会在下一次成功结算重试。
- 数据：没有修改存储结构，不需要删除本地数据；本次未发出的任务奖励没有丢失，仍在待发队列中。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`、`specs/task-and-settlement.md`。
- 验证：Unity `Editor.log` 已确认任务完成前正常累计、完成时权益成功入队并推进下一任务；`dotnet build Assembly-CSharp-Editor.csproj --no-restore` 通过，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在 Play Mode 用“首次完成 1 个 M 包 + 重玩 2 次”复验任务奖励当局发放，并确认普通重玩仍不会触发首次完成奖励。

## 2026-09-02 结算任务奖励下飞动画

- 状态：代码修改与编译验证已完成，等待 Unity Play Mode 视觉复验。
- 待发奖励修复：运行日志确认任务从 `35/60` 推进到 `68/60` 并成功创建待发权益，但因阶段门槛为 `R=7、H=3、H<=2`，本轮没有分配真实 PackId。结算展示此前错误地只检查真实 PackId，导致整个任务奖励动画被跳过；现在增加本轮任务权益成功入队标记，只要任务奖励已经赚到，就会显示奖励底板并从任务位置飞入对应槽位。该标记只控制结算表现，不参与真实卡包解锁；点击完成时，尚未分配 PackId 的占位卡随 `ImgBagBg` 收回，只有真实任务卡包才提升为跨场景对象并飞入首页列表。
- 修改：结算奖励动画按奖励来源决定，不再按槽位序号决定。首次完成奖励继续使用下方槽位原有弹出动画；任务奖励无论是唯一奖励还是双奖励中的第二个，都直接复用对应 `BagRewardItem` 内的真实问号卡包，在任务条 `BagBg/BagIcon` 的实际屏幕位置瞬时接替原图标，再用 `0.52s` 直线下移并同步放大到目标槽。只有任务奖励时目标为居中第一槽，同时存在首次完成奖励时目标为右侧第二槽；飞行期间逐帧读取仍在上滑的目标槽终点，避免落点随背景条移动而偏移。
- 原地淡出：替换发生时立即隐藏原 `BagIcon`；`BagBg` 的绿色圆圈、`BagAddBg` 和 `TextAddNum` 保持原位置，用 `0.2s` 渐隐，不参与飞行。
- 对象约束：没有克隆临时问号图，也没有修改 `BagRewardItem.prefab` 或解锁特效；动画结束后任务奖励仍属于原完整 `BagRewardItem` 实例，可继续被结算退场和首页奖励转场接管。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：运行日志已确认本次未播放动画不是任务判定失败，而是任务权益入队后受到发包门槛拦截；`dotnet build Assembly-CSharp-Editor.csproj --no-restore` 通过，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在 Play Mode 分别触发“仅任务奖励且立即发包”“仅任务奖励但保持待发”和“首次完成奖励 + 任务奖励”，确认任务问号卡从任务区飞向对应槽位、绿圈与 `+1` 原地消失，且只有真实 PackId 参与完成按钮后的首页飞入。

## 2026-09-02 三档本地存档与 PanelSave 接入

- 状态：存储层、MainScene UI 逻辑及切档后列表不可见修复已完成，等待 Unity Play Mode 复验。
- 存储结构：新增 `LocalSaveSlotUtility`，当前活动档位记录在 `persistentDataPath/SaveSlots.json`；三份进度分别保存到 `SaveSlot1/2/3/LocalData.db` 和 `LocalData.json`。SQLite、JSON、任务、卡包与设置的静态状态会在点击“继续”正式切档时统一清空，再由 `LoadingScene` 按新档位重新初始化。
- 摘要与删除：存档面板不打开其他档位进行业务初始化，只读各档文件；有数据时统计 SQLite 中非 `Locked` 的卡包数量，并取档位目录内数据文件的最新修改时间，显示为“已解锁的拼图包：数量”和 `dd/MM/yyyy HH:mm`。删除当前活动档前先关闭连接、清理缓存，再删除该档目录；空档不显示删除按钮。
- UI：`PanelSave/BtnSave1~3`、`BtnContinue`、`BtnDelete` 已按实际层级绑定。第一档现有文字颜色作为选中模板，第二档作为未选中模板；点击档位立即刷新三档样式。空档也始终保留左侧 `1/2/3` 编号，并在编辑器原右侧内容区域居中显示“新游戏”；有数据时显示编号及原两行摘要布局。点击“继续”保存活动档位并进入 `LoadingScene`；关闭与返回逻辑不变。
- 列表修复：Unity 日志确认切回 `SaveSlot1` 后任务进度 `33/45` 和 `3` 个已解锁卡包均已正确读取；不可见是因为上一轮结算的 `CardPackRewardFlyTransition` 在解锁特效收尾期间仍为活动状态，新 MainScene 因此把列表预隐藏，但该过渡不会再接管第二次载入的 MainScene。存档“继续”成功后现在先取消旧奖励过渡、清除静态活动标记和输入层，再进入 `LoadingScene`。
- 修改文件：`Assets/Scripts/Model/LocalDataStore.cs`、`Assets/Scripts/Model/GameTaskUtility.cs`、`Assets/Scripts/Model/CardPackDataUtility.cs`、`Assets/Scripts/Model/GameDefine.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。`Assets/Scenes/MainScene.unity` 中 `PanelSave` 节点命名是用户已有未提交修改，本轮保留并直接使用。
- 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore` 通过并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在 Play Mode 验证三档新建、互相切换、退出重进、删除、摘要数量和更新时间刷新。
- 数据重置：这是不兼容的存储路径变更，不迁移旧根目录数据。当前工程实际 `persistentDataPath` 为 `%USERPROFILE%/AppData/LocalLow/MainTown/Ducky Stickers`；关闭 Unity 后删除该目录旧根级 `LocalData.db` 与 `LocalData.json`。若要把三档一起重新测试，同时删除 `SaveSlots.json` 和 `SaveSlot1/2/3` 目录。

## 2026-09-02 首页拍照白色闪光不可见

- 状态：代码修改和 Runtime/Editor 编译完成，等待 MainScene/GameScene 拍照实测。
- 根因：首页 `BtnCamera -> CardPackPhoto.TryCapture -> PlayPhotoFlash` 调用顺序正确，选中卡包也在预览真正出现后才隐藏；问题位于运行时 `PhotoFlashCanvas`：排序值使用 `33000`，超过 Unity Canvas 稳定支持的最高值 `32767`，且首次激活后同帧立即开始仅 `0.26s` 的闪光，没有等待 Screen Space - Camera Canvas 注册到当前相机渲染。
- 修改：闪光 Canvas 排序固定为 `short.MaxValue (32767)`；每次拍照开始时重新绑定当前 `Main Camera` 和固定 `16:9` Canvas 配置，激活后强制刷新 Canvas 并等待一帧，再沿用原 `0.06s` 淡入、`0.04s` 停留、`0.16s` 淡出。
- 保留：拍照按钮逻辑、选中卡包隐藏时机、离屏图片生成、去棋盘投影、保存路径和预览动画时序不变；MainScene 与 GameScene 继续共用同一组件。
- 修改文件：`Assets/Scripts/Model/CardPackPhoto.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp-Editor.csproj` 编译通过并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需分别在 MainScene 和 GameScene 点击拍照，确认先看到完整白色闪光，再生成照片并显示预览。

## 2026-09-02 桌面照片移除棋盘投影

- 状态：代码修改和 Runtime/Editor 编译完成，等待 MainScene/GameScene 拍照实测。
- 根因：`CardPackPhoto` 的离屏拍照副本既继承 `CardBagNNN.prefab` 中 `GameBoard/BoardTitle` 的 `IngameCoverShadow01` 投影材质和 `PackCoverShadowEffect` 扩边，又额外为 `GameBoard` 动态添加了偏移 `(18,-24)` 的 `UnityEngine.UI.Shadow`，因此导出的 PNG 在棋盘右侧和下侧出现明显双重投影。
- 修改：删除拍照流程主动创建右下 `Shadow` 的代码；离屏副本完整显示后，仅对副本中的 `GameBoard` 和 `BoardTitle` 清空投影材质，并禁用 UI Shadow 与 `PackCoverShadowEffect`。关卡 Prefab、游戏内棋盘和贴纸投影均不修改。
- 修改文件：`Assets/Scripts/Model/CardPackPhoto.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp-Editor.csproj` 编译通过并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需分别从 MainScene 历史卡包和 GameScene 结算页拍照，确认桌面 PNG 的棋盘右侧、下侧无投影且拼图内容、旋转、木纹背景和左下角图标保持不变。

## 2026-09-02 Win32 游戏中切换尺寸后棋盘未适配

- 状态：代码修改和 Runtime/Editor 编译完成，等待 Windows Player 实机验收。
- 根因：`GameScene` 检测到 `Screen.width/height` 变化后，在同一帧立即用刚更新视口和 CanvasScaler 的 RectTransform 世界边界计算当前组相机与棋盘；该帧 Canvas 边界仍可能是旧尺寸或半更新状态，但代码随即清除了待刷新标记，后续不会再纠正，导致棋盘和托盘停留在旧布局并被有效视口裁切。
- 修改：连续拉伸期间只记录最新客户区尺寸；最新尺寸稳定两帧且当前没有入场、切组、吸附、拖拽或托盘重排冲突时，再次配置固定 `16:9` 视口、强制刷新全部 Canvas/Layout，并完整重算当前组相机、棋盘、托盘及托盘 Piece。最终布局完成后，若新手引导正在显示，同步重建提示框、箭头和焦点层。
- 保留：现有棋盘缩放、托盘 Piece 缩放与间距、拖拽、吸附、错误回弹和互斥动画规则不变；重排仍只在安全时机执行，不中断玩家当前操作。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp-Editor.csproj` 编译通过并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。仍需在 Windows Player 游戏过程中验证窗口化、最大化、恢复及连续横向/纵向拉伸，确认棋盘、托盘和教程目标始终完整可操作。

## 2026-09-02 新手引导第三步提示框与箭头修复

- 状态：代码修改和 Runtime/Editor 编译完成，等待 CardBag001 Play Mode 视觉验收。
- 根因：第三步仍以 `null` 相机把 `BtnTips` 屏幕坐标换算到 Screen Space - Camera 教程 Canvas；Game 视图缩放后提示框与箭头得到错误的 Canvas 坐标，导致提示框遮挡按钮、箭头被移动到错误位置。场景 `GuideTip/Arrow` 模板、Sprite 和运行时创建逻辑均存在，不是资源丢失。
- 修改：第三步提示框中心、按钮矩形和箭头目标统一使用教程 Canvas 的实际相机换算；提示框右边缘与按钮左边缘强制保留至少 `32` 设计像素，箭头尖端继续停在按钮左边缘外 `16` 设计像素并使用原下移 `20`、脉冲和延迟出现动画。运行时 Arrow 显式激活。
- 保留：第一、二步继续位于对应待拼凹槽区域上方；第三步提示框原左移 `48`、下移 `20`、尺寸、文案和动画资源不变。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：当前场景 `BtnTips` 中心 `x=1112`、宽 `90`，按钮左边缘为 `1067`；第三步提示框宽 `568`，限制后右边缘为 `1035`，间隔 `32`；箭头脉冲终点为 `1051`，位于按钮左边缘外 `16`。`Assembly-CSharp-Editor.csproj` 编译通过并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误。仍需在 CardBag001 第三步确认提示框不遮挡 `BtnTips`、箭头延迟出现后持续可见并正确指向按钮左侧。

## 2026-09-02 新手引导前两步提示框定位修复

- 状态：代码修改和 Runtime/Editor 编译完成，等待 CardBag001 Play Mode 视觉验收。
- 根因：第一步提示框使用固定屏幕锚点，第二步才尝试读取当前组凹槽范围；第二步凹槽坐标换算失败时又会落入第三步使用的右上固定锚点，因此提示框可能不在要拼区域上方。
- 修改：第一步只读取当前指定 Piece 的凹槽范围，第二步读取当前阶段全部尚未拼好的凹槽范围，两者都以目标范围水平中心、上边缘外 `24` 设计像素定位提示框。目标凹槽换算改为使用教程 Screen Space - Camera Canvas 的实际相机；若目标范围暂时不可用，先回退到棋盘区域上方，棋盘也不可用时回退到画面顶部居中，绝不进入第三步定位分支。
- 保留：第三步继续根据 `BtnTips` 实际位置使用独立提示框、左移/下移和箭头定位规则；三步提示框尺寸、文案与进场动画不变。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp-Editor.csproj` 编译通过，并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库行尾转换提示。仍需在 CardBag001 依次检查第一步提示框位于指定凹槽上方、第二步提示框位于当前待拼区域上方、第三步保持提示按钮旁的独立位置。

## 2026-09-02 设置页音量条与 Windows 全屏修复

- 状态：代码修复、几何检查和 Runtime/Editor 编译完成，等待新 Windows Player 构建视觉验收。
- 音量条根因：`FakeSettingsSliderInput` 原先用 `SliderFill.sizeDelta.x - 8` 计算绿色宽度，同时直接把场景中圆点初始坐标 `101.1` 当作对称极限，填充和圆点使用了两套不同轨道。中间值时绿色终点与圆点中心不一致，满值时圆点右侧仍会露出深色底槽。
- 音量条修复：初始化时从 `SliderFill` 的完整世界矩形换算出凹槽左右边界，再按圆点实际半宽内收圆点中心轨道。圆点中心按真实进度移动，绿色在其下方继续铺到圆点右边缘，避免上层圆点遮住绿色后产生“绿色比圆点进度少一截”的观感；`0` 时圆点左边缘与凹槽左端对齐，`1` 时圆点右边缘与凹槽右端对齐且绿色完整填满凹槽。
- 全屏根因：原逻辑只设置 `Screen.fullScreenMode`，从窗口模式切回全屏时可能继续沿用窗口客户区尺寸或边框状态。Windows Player 现在读取 `Screen.currentResolution`，以当前显示器原生宽高和刷新率明确调用 `Screen.SetResolution(..., FullScreenWindow)`。
- 显示切换：音乐和音效更新现在只刷新音频，不再重复写全屏模式；显示模式仅在初始化、窗口化开关变化或实际模式不一致时应用。
- 修改文件：`Assets/Scripts/View/FakeSettingsSliderInput.cs`、`Assets/Scripts/Model/LocalDataStore.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：音量条按当前场景 `SliderFill=[-118.7,118.3]`、圆点半宽 `16` 检查 `0/0.4/0.5/1`；圆点中心轨道为 `[-102.7,102.3]`，两端的圆点外边缘分别为 `-118.7/118.3`，与凹槽左右端对齐。绿色铺到圆点右边缘，满值宽度为完整 `237`。仍需在 Unity 中目测两条音量条的绿紫衔接，并用新 Windows Player 检查窗口化切回全屏后顶部无黑条。

## 2026-09-02 拍照预览提示动画接入

- 状态：代码和动画资源接入完成，Runtime/Editor 编译通过，等待 MainScene 与 GameScene Play Mode 视觉验收。
- 顺序：点击拍照按钮后仍先播放原有全屏白色闪光并离屏生成、保存图片；生成成功后显示共享 `PackPhotoItem` 预览，同时从第 `0` 帧单次播放根 Animator 的 `PackPhoto` 状态。
- 美术动画：保留 `PackPhoto.anim` 的全部现有曲线和 `2.667s` 时长，由资源控制 `TaskContent` 先淡入、停留、淡出，再激活并淡入 `BtnOK`；仅关闭了错误的循环设置，代码不重做文字或按钮的透明度动画。
- 交互：Animator 在闪光和图片生成阶段保持停止，避免预览出现前提前播放；动画开始显示预览时立即执行原有预览就绪回调，使 MainScene 同帧隐藏原选中卡包。动画期间 `BtnOK` 即使已被曲线激活也不可点击，完整播放结束后才启用；关闭预览时停止 Animator 并恢复现有场景按钮与选中卡包状态。
- 共用：MainScene 选中卡包页和 GameScene 结算页继续使用同一个 `Assets/Prefabs/PackPhotoItem.prefab` 与 `CardPackPhoto`，没有复制第二套逻辑，也没有修改 Prefab 层级或动画曲线。
- 修改文件：`Assets/Scripts/Model/CardPackPhoto.cs`、`Assets/Animation/PackPhoto.anim`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过，仅有仓库行尾转换提示。仍需分别从 MainScene 历史卡包相机按钮和 GameScene 结算相机按钮验收“闪光 -> 预览出现 -> 文字显示/消失 -> OK 按钮出现并可点击 -> 关闭恢复”的完整顺序。

## 2026-09-02 Win32 窗口拉伸布局与鼠标残影

- 状态：已纠正前一版错误的 Cover 适配；代码和编译验证完成，等待 Windows Player 连续拉伸视觉验收。
- 固定画面：所有常规页面使用 `2560x1440`、`16:9` 的居中有效视口整体等比缩放。超宽窗口显示左右黑边，偏窄或偏高窗口显示上下黑边；背景和 UI 都不得裁切或分别拉伸 X/Y。
- Canvas：统一入口将根 Canvas 绑定当前 `Main Camera`，`CanvasScaler` 使用 `Scale With Screen Size + Expand`，缩放比例只取能完整容纳 `2560x1440` 设计区域的较小值。MainScene、GameScene、LoadingScene、RankScene、AchieveScene，以及运行时拍照面板、闪光层、新手引导和奖励转场均已接入。
- 自动刷新：五个场景都监听 `Screen.width/height` 变化并重算视口、Canvas 与 Layout。MainScene 保留卡包分页位置和系列列表刷新；GameScene 继续在没有拖拽或互斥动画的安全时机重算棋盘、托盘和 Piece，不中断当前操作。
- 黑边清理：统一入口创建不渲染任何 Layer 的全屏底层相机，每帧将视口外区域清为纯黑；内容相机只在居中 `16:9` 视口内渲染，避免软件鼠标在黑边留下历史帧残影。
- 鼠标：软件鼠标缩放改为取窗口宽高相对 `2560x1440` 的较小缩放比，与 `16:9` 有效视口保持一致；原图片、热点和三种状态切换不变。
- 删除：已删除 `FitRectTransformToParentCover` 及 MainScene/GameScene 两处背景 Cover 调用；不再使用完整 viewport 强制铺满窗口。
- 修改文件：`Assets/Scripts/Model/GameCommonUtility.cs`、`Assets/Scripts/Model/CardPackPhoto.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Controller/LoadingScene.cs`、`Assets/Scripts/Controller/RankScene.cs`、`Assets/Scripts/Controller/AchieveScene.cs`、`Assets/Scripts/Editor/CanvasDesignResolutionEditor.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译均通过，结果为 `0` 警告、`0` 错误。仍需在 Windows Player 验收 `16:9` 无黑边、超宽左右黑边、偏高上下黑边、拉伸后页面立即刷新、所有 UI 完整显示，以及鼠标经过黑边无残影。

## 2026-09-02 Windows 默认全屏与窗口化设置

- 状态：Windows Player 默认启动模式已改为无边框全屏；首次没有本地设置时，`PanelSet/ToggleFrame` 对应的 `IsWindowed` 固定为 `false`，因此窗口化开关默认关闭。用户打开开关后立即切换到 `FullScreenMode.Windowed` 并保存；关闭开关后立即恢复 `FullScreenMode.FullScreenWindow`。
- 保留：Windows 窗口仍允许用户自由拉伸，`2560x1440` 设计分辨率、Canvas 缩放规则、音乐/音效与辅助选项设置不变。已有本地 `GameSettings` 继续尊重用户上次保存的窗口化选择，不强制覆盖。
- 修改文件：`ProjectSettings/ProjectSettings.asset`、`Assets/Scripts/Model/LocalDataStore.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp-Editor.csproj` 编译通过，并连带生成 Runtime/firstpass，结果为 `0` 警告、`0` 错误；场景 `ToggleFrame` 的编辑器初始值确认是关闭。仍需 Windows Player 验收首次默认全屏、打开开关立即窗口化、关闭开关恢复全屏和重启保持；首次默认状态需要使用没有 `GameSettings/Runtime` 记录的本地数据验证，已有测试数据会继续显示上次保存值。

## 2026-09-01 跨场景共享拍照面板

- 状态：原拍照面板已由 Unity 编辑器转换并统一重命名为共享 `PackPhotoItem` Prefab，MainScene 与 GameScene 各引用同一个 Prefab 实例；运行时代码接入和编译验证完成，等待 Play Mode 功能验收。
- Prefab：共享资源命名为 `Assets/Prefabs/PackPhotoItem.prefab`，Prefab 根节点与两个场景实例也统一命名为 `PackPhotoItem`；完整保留 MainScene 原有 `Photo`、`GameIcon`、`BtnOK` 和美术布局。根节点使用独立 Canvas、CanvasScaler、GraphicRaycaster 与 CanvasGroup；运行时绑定当前 `Main Camera` 并使用固定 `16:9` 视口，排序层级仍高于首页选中卡包和结算 UI。
- 通用逻辑：脚本与组件类型命名为 `CardPackPhoto`，统一负责白色闪光、生成 `1024x1024` PNG、保存桌面、替换预览图、`BtnOK` 关闭及运行时纹理释放。文件名继续使用 `游戏名-YYYY-MM-DD-BagId.png`。
- MainScene：原内嵌拍照面板已替换为根级 `PackPhotoItem` Prefab 实例，原拍照生成代码从 `MainScene` 移入通用组件；卡包选中页相机按钮继续使用当前选中 PackId，预览关闭后恢复选中卡包和按钮交互。
- GameScene：结算页 `BtnCamera` 已绑定同一拍照功能，使用当前 `GameManager.GetBagId()`；拍照及预览期间完成按钮和相机按钮不可交互，关闭预览或拍照失败后恢复。
- 修改文件：`Assets/Prefabs/PackPhotoItem.prefab`、`Assets/Scripts/Model/CardPackPhoto.cs`、`Assets/Scenes/MainScene.unity`、`Assets/Scenes/GameScene.unity`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：Unity 批处理成功创建并保存 Prefab 和两个场景，日志明确输出创建成功且返回码为 `0`；两个场景各有且仅有一个相同 GUID 的 Prefab 实例，Prefab 内 `Photo`、`BtnOK` 和 `CardPackPhoto` 各一份；重命名保留原 `.meta` GUID，场景 Prefab 和组件引用不变。Runtime/Editor C# 串行编译均通过，`0` 警告、`0` 错误；`git diff --check` 仅有仓库既有 LF/CRLF 提示。仍需在 Play Mode 分别验证 MainScene 选中页与 GameScene 结算页的闪光、桌面文件、预览、OK 关闭和按钮恢复。

## 2026-09-01 结算奖励卡落位特效接入

- 状态：多奖励落位改为每个完整 `BagRewardItem` 独立播放自身 Prefab 内的 `FX_ui_jieSuo_w`；问号封面与特效生命周期已经拆开，约 `0.3s` 首闪时只隐藏该卡的 `BagCover` 并显示真实卡包，特效继续播放到自身结束后才回收完整奖励对象。双奖励左右分槽只移动 `BagCover` 导致两份特效挤在中间的问题已修复，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 新节点：结算奖励占位图为 `ImgBagBg/BagRewardItem/Canvas/BagCover`；`BagRewardItem` 内 `FX_ui_jieSuo_w` 在结算页保持停止和隐藏，不会提前播放。特效允许位于 `Canvas` 下的中间容器内，运行时在每个奖励实例自身范围递归查找，不依赖直属子节点路径。
- 跨场景播放：点击完成后，`CardPackRewardFlyTransition` 直接接管 GameScene 编辑器中现有的完整 `BagRewardItem` 实例；`BagCover` 与该实例自己的 `FX_ui_jieSuo_w` 保持原 Prefab 层级并一起跨场景移动。换父时只移动和等比缩放 `BagRewardItem` 根节点，并按 `BagCover` 换父前后的真实屏幕中心与尺寸校正；不得重置内部 Canvas 或 `BagCover` 的锚点、位置、尺寸、旋转和缩放。每张奖励卡落位时分别启动自身现成特效，不复制、不换父、不覆盖粒子参数，也不共享另一张奖励卡的特效实例。
- 渲染环境：持久化转场 Canvas 使用与 GameScene/MainScene 根 Canvas 相同的 `Screen Space - Camera` 路径，并在场景切换后重新绑定当前 `Main Camera`；完整卡包 Canvas 全程使用 `sortingOrder=1`，稳定显示在 MainScene 根 Canvas `0` 之上。美术背光粒子保持 Renderer 排序 `0`，前景闪光和粒子保持原生 `3~112`，不覆盖任何粒子排序参数。
- 结算页：任务奖励不再从 `BagCover` 单独复制 `TaskRewardFlyIcon`；只让完整 `BagRewardItem` 自身播放出现动画。只有真正分配到正数 PackId 的奖励才显示，已记入待发队列但本局未分配 PackId 的任务奖励不显示问号占位。
- 跨场景交接：不再创建 `SceneSnapshot`、`RenderTexture` 或 `RawImage`，也不再捕获 GameScene 最后一帧。结算页现有完整 `BagRewardItem` 先换父到 `DontDestroyOnLoad` 的转场 Canvas，切换 MainScene 后仍由同一对象继续飞行和缩放，不存在截图卡面副本。
- 双奖励对象：`GameScene/RewardPanel/ImgBagBg` 已在编辑器场景中预置 `BagRewardItem` 与 `BagRewardItemSecondary` 两个完整 Prefab 实例。单奖励启用一个，双奖励启用两个；两份对象分别绑定并播放各自层级内的 `FX_ui_jieSuo_w`，落位错峰、首闪揭晓和播放结束回收均独立计算。
- 双奖励特效位置：结算页的左右分槽位置保存在各自 `BagCover` 上，特效外层父节点默认仍位于内部 Canvas 中心。转场接管每个实例后，必须把该实例中位于 Canvas 下的特效布局父节点对齐到自身 `BagCover` 的真实中心；只调整新增的外层父节点，不修改 `FX_ui_jieSuo_w` 内部 Transform、粒子参数或 Renderer 排序。此后整包移动与缩放时，特效始终跟随自己的卡包。
- 奖励背板：结算页黑色半透明条 `ImgBagBg` 不再随完成按钮提前退出 GameScene；奖励卡从其层级提升后，`ImgBagBg` 与完整奖励卡一起换父到持久化 Canvas 并进入 MainScene。首页奖励卡开始飞行 `0.08s` 后，背板沿用原 `0.42s` 三次缓入节奏下移出屏幕，奖励卡飞行、落位和揭晓时序不变。
- 首页顺序：MainScene 列表在分批创建的全部让帧期间由 Content `CanvasGroup` 预隐藏，完成排序、目标缓存和全部卡包离屏布置后才恢复显示，因此不会先闪出其他卡包。完整 `BagRewardItem` 一边飞行一边等比缩放到首页目标卡包的实际尺寸并精确落位；实例内特效播放约 `0.3s` 到首闪时，只隐藏问号 `BagCover` 并同帧显示对应真实卡包，不能关闭特效或整个奖励根节点。所有奖励完成真实卡揭晓后，其余原有卡包从屏幕下方依次上移；各奖励特效继续独立播放，有限粒子全部结束后才分别停止循环粒子并回收自己的 `BagRewardItem`。列表入场完成后即解除输入拦截，不等待长尾特效结束。
- 容错：完整 `BagRewardItem`、`BagCover`、实例内特效或首页目标槽缺失时输出警告；没有特效时仍按原流程揭晓真实卡包并返回首页。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Assets/Scenes/GameScene.unity`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。`Assets/Prefabs/BagRewardItem.prefab` 与美术粒子参数未修改。
- 验证：已确认 `TaskRewardFlyIcon`、单独复制 `BagCover`、`SceneSnapshot`、`RenderTexture`、`RawImage` 和截图捕获逻辑均已移除；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误，`git diff --check` 仅有仓库既有 CRLF 转换提示。仍需在 Play Mode 用双奖励确认两张卡各自只播放自己的特效、问号封面首闪隐藏后特效不中断、真实卡保持显示、两份特效按各自实际播放结束时间分别回收，以及长尾播放期间首页输入在列表入场完成后正常恢复。

## 2026-09-01 重玩积分与卡包奖励限制

- 状态：运行时代码与稳定需求已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 数据验收。
- 重玩判定：进入 GameScene 时当前 PackId 已经是 `Completed`，即 `_wasSelectedPackCompletedOnEntry=true`，本局按重玩处理；进行中会话的继续游戏不属于重玩。
- 积分：正常局保持原规则。重玩的可得基础分为尺寸原始基础分的 `10%`；每项加成仍以原始基础分为计算基数。公式为 `ReplayFinalScore = Ceil(OriginalBaseScore * 10%) + Ceil(OriginalBaseScore * TotalBonusRate)`。结算动画先滚到折算基础分，再逐项显示按原始基础分换算的实际加分。
- 发包：重玩结算不执行首次完成发包。任务是否累计仍由 `TaskConfig.csv/CountReplay` 控制；重玩中达成任务时，保底卡包权益写入待发队列并推进任务，随后独立按常规门槛尝试兑现。该规则已由 2026-09-02 的“重玩完成任务未发卡包修复”更新。
- 数据：没有修改 SQLite 或 JSON 结构，不需要删除本地数据。
- 修改文件：`Assets/Scripts/Model/GameTaskUtility.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误。仍需在 Unity 验证正常首次完成、普通重玩、重玩达成任务、存在待发任务奖励时重玩四种情况。

## 2026-09-01 结算完成后奖励卡与首页列表入场

- 状态：运行时代码和需求记录已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 完成按钮退场：点击 `BtnFinish` 后先让顶部 `TaskBg2` 与当前 `TaskItem` 反向收回屏幕上方，底部 `ImgBagBg`、`BtnFinish` 与 `BtnCamera` 同时反向收回屏幕下方；顶部沿用入场 `0.52s` 的反向节奏，先向下回摆约 `14px` 再加速上收，底部沿用 `0.42s` 的反向加速曲线。真实奖励图标在退场前临时提升到 `RewardPanel` 顶层并保持原屏幕位置，不随 `ImgBagBg` 下沉。退场截图保留该奖励卡；首页交叉淡入期间，跨场景飞行副本在原位置从透明同步接管，结束后直接开始飞向列表，全程不得隐藏一帧或重新跳现。
- 场景切换：点击 `BtnFinish` 后截取结算页最后一帧并作为跨场景覆盖图保留；MainScene 卡包列表完成创建、排序和布局后，先缓存最终槽位并把全部列表卡包移到屏幕下方外侧，再用 `0.30s` 将结算覆盖图直接淡出到首页。中间不经过纯黑画面，截图失败时直接切换首页，也不显示黑色兜底层。
- 奖励卡：只有已经分配真实 PackId 的结算奖励参与飞行。飞行图标分别复制本局首次完成奖励槽和任务奖励槽中默认 `ImgBag` 的 Sprite、颜色、尺寸及实际起点，不提前切换为真实卡包纹理；单张飞行 `0.72s`，多张按 `0.12s` 错峰，一边移动一边缩放到对应列表卡包尺寸。
- 落位揭晓：奖励卡落位后完整播放 `BagRewardItem/Canvas/FX_ui_jieSuo_w`；有可见 Renderer 的非循环粒子结束后，同帧隐藏完整飞行对象并显示该槽真实卡包的完整状态、标签、系列叠加和进行中碎片。
- 列表入场：全部奖励卡揭晓后，其余卡包才按当前列表顺序从屏幕下方依次上滑；单卡时长 `0.44s`、错峰 `0.055s`。无真实奖励时仍执行场景淡入淡出和整列上滑。动画期间暂停分页布局、拖动和卡包输入，结束或异常取消时恢复位置、布局和交互。
- 系列卡包：奖励 PackId 若属于系列后层，目标解析到该系列实际占用的列表槽，不额外创建空槽。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；`git diff --check` 仅有仓库既有 CRLF 转换提示。Unity 中仍需验证结算覆盖图方向和清晰度、中间无纯黑帧，以及无奖励、单奖励、双奖励、普通卡包奖励、系列卡包奖励和动画结束后分页、拖动、点击恢复。

## 2026-09-01 结算动画节奏优化

- 状态：代码与需求记录已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 分数节奏：基础分滚动由 `1.0s` 调整为 `1.2s`；每条加成改为“文案提前 `0.16s` + 滚分 `1.08s`”，单条总时长约 `1.24s`；最终分稳定停留由 `0.24s` 调整为 `0.45s`。
- 任务进度：非积分任务不再在全部加成后额外串行滚动 `0.8s`，而是在最后一次有效滚分进行到 `25%` 后开始同步推进并同时结束；没有加成时并入基础分滚动。积分任务继续沿用分数与任务进度同步规则。
- 面板入场：`TaskBg2` 和当前 `TaskItem` 仍与棋盘适配并行从顶部进入，总时长仍为 `0.52s`；末段增加约 `14px` 的轻微下沉和回弹，未修改透明度、缩放或编辑器布局。
- 奖励节奏：`ImgBagBg` 开始上滑 `0.26s` 后即可启动首张奖励卡，双奖励再错峰 `0.14s` 启动任务奖励飞入；任务奖励飞行期间逐帧追踪仍在移动的目标槽，避免提前取坐标产生落点偏差。完成按钮和相机按钮仍等底板及全部奖励卡结束后才同步进入。
- 返回首页：继续使用项目现有的 GameScene/MainScene 淡入淡出，不增加整页横向退出。奖励卡移动到屏幕中央时增加轻微尺寸回弹，中央停留由 `0.55s` 收紧到 `0.42s`；进入 MainScene 后以 `0.08s` 错峰、单张 `0.64s` 弧线飞入各自列表位置。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；`git diff --check` 仅有仓库既有 CRLF 转换提示。Unity 中仍需验证无加成/四加成、积分/贴纸/完成卡包任务、无奖励/单奖励/双奖励，以及点击完成后的淡入淡出和奖励卡跨场景飞行动画。

## 2026-09-01 新手引导第三步提示框避让

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 修改：CardBag001 新手引导第三步提示框在现有 `BtnTips` 动态定位结果上整体向左移动 `48` 个设计像素、向下移动 `20` 个设计像素，避免提示背板遮挡提示按钮；屏幕安全边距限制继续生效。
- 箭头：仍按 `BtnTips` 的实际矩形独立定位，横向终点保持在按钮左边缘外 `16` 个设计像素，并在按钮垂直中心基础上向下移动 `20` 个设计像素，使视觉尖端不再偏高。
- 保留：提示框尺寸、文案、入场动画、按钮位置以及第一、二步引导均未修改。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。没有修改 `Assets/Scenes/GameScene.unity`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；仍需在 Unity Play Mode 确认提示背板不再遮挡 `BtnTips`，箭头仍准确指向按钮。

## 2026-08-31 结算面板入场、卡包数与奖励卡包动画

- 状态：运行时代码已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 入场顺序：`RewardPanel` 显示首帧先把 `TaskBg2` 和当前 `TaskItem` 放到屏幕上方，再用同一段 `0.52s` 缓动同时落到编辑器位置，避免先闪现在终点。
- 棋盘展示：进入结算时移除托盘运行时碎片并显示全部已完成凹槽后，以完整 `CardBag` 根节点的实际屏幕矩形为准，计算等比缩小到屏幕与游戏背景可见区域 `90%` 以内的居中目标；只缩小过大的棋盘，不主动放大小棋盘。棋盘用约 `0.46s` SmoothStep 同时完成缩放和位移，并与 `TaskBg2/TaskItem` 入场并行播放，不再瞬间切换。
- 新手引导第三步：已撤销后续错误增加的提示框 `48/96` 设计像素水平间隔，提示框恢复为第一次按 `BtnTips` 实时中心与模板箭头尖端反算的位置。箭头独立读取按钮实际屏幕矩形，循环推进终点停在按钮左边缘外 `16` 个设计像素处，只指向按钮而不覆盖按钮图标；调整箭头不再移动提示框整体。
- 卡包数：显示面板前先读取当前 `Completed` 卡包数，再保存本轮完成状态，因此初始红色卡包数不包含刚完成的当前卡包。只有新卡包首次完成时，全部积分、加成和任务进度动画结束后才在数字右上方播放红色 `+1` 上飘；`+1` 字号读取编辑器中 `TaskBg2/TaskTitle2` 的字号，并同步把数字滚动到新值，重玩不重复播放。
- 奖励顺序：任务奖励和首次完成概率奖励仍按既有数据规则判定，但展示时固定按“本局首次完成奖励、任务奖励”排列。实际发到首次完成奖励或本轮任务奖励权益成功入队时，`ImgBagBg` 从屏幕下方滑回编辑器位置；单个奖励居中，两个奖励左右预留槽位。
- 奖励表现：首次完成奖励使用结算页默认 `ImgBag`，在第一槽从 `0` 放大到 `1.2` 再回弹到 `1`；任务奖励只读取 `TaskItem/BagIcon` 的起点位置，飞行物和第二槽均使用默认 `ImgBag` 的纹理、颜色和尺寸，全程保持 Z 轴旋转为 `0`。默认 `ImgBag` 不替换成真实卡包封面，点击完成后的既有跨场景飞入首页列表逻辑继续保留。
- 底部按钮：结算首帧将 `BtnFinish` 和 `BtnCamera` 保持编辑器原透明度、缩放与旋转，仅移动到屏幕下方。积分、卡包数和本轮奖励卡包弹出/飞入全部完成后，两按钮同步使用 `ImgBagBg` 相同的 `0.42s` 位移缓动滑回编辑器位置；动画结束前按钮不可交互。
- 任务奖励耗尽修复：本地测试数据已经完成全部 `22` 个配置卡包时，任务奖励权益仍会成功进入待发队列，但暂时没有真实 PackId 可分配。结算页现在依据“本轮任务权益成功入队”播放 `ImgBagBg` 和 `BagIcon` 飞入，不再错误依赖 `grantedPackId > 0`；只有已经分配真实 PackId 的奖励才继续参与点击完成后的首页列表飞入。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`。本轮没有修改 `Assets/Scenes/GameScene.unity`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；日志已确认本次测试为 `chapter=0, R=0, granted=False, grantedPackId=0, pending=3`，即全部现有卡包完成后任务权益待发。`git diff --check` 仅提示文件将来由 Git 转换为仓库既有 CRLF，不存在空白错误。Unity 中仍需验证五种组合：新包无奖励、仅首次完成奖励、仅任务奖励、同时两个奖励、已完成卡包重玩；并额外验证“无可发 PackId 但本轮任务刚达成”仍播放任务图标飞入。

## 2026-08-31 拆包动画跨场景显隐、尺寸与 Vol 标签

- 状态：运行时代码已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 展示规则：完整彩包开始播放拆包 Timeline 时，左下显示当前卡包真实 `PackSize`；Vol2 及以上在右下显示真实 `PackVol`，Vol1 不额外显示 Vol 标签。
- 布局来源：直接读取展开态 `PackNode/PackSize` 和 `PackNode/PackVol` 的 Sprite、颜色、屏幕相对中心和尺寸，位置继续由 `PackItem.prefab` 的美术配置决定，不写死第二套标签坐标。
- 动画跟随：两个标签转换为 EffectLayer 世界空间 SpriteRenderer，分别锚定到卡包模型对应位置最近的下半部骨骼，在 `LateUpdate` 跟随制作方动画位移和旋转；模型 Activation 隐藏时同步隐藏。
- 跨场景姿态：Timeline 在约 `0.8s` 暂停时记录卡包正面渲染器的真实 Viewport 中心和高度；GameScene 完成棋盘相机适配后，恢复播放前重新适配整个模型 Stage，并用同一倍率同步 `PackSize/PackVol`，避免交接瞬间缩小或上移。独立的 `fx_chai_w_001` 不继承该适配。
- 跨场景显隐：GameScene 根 Canvas 运行时是 `Screen Space - Camera`，原卡包运行时材质队列 `2001` 会先于玩法 UI 绘制并被遮挡。只在进入 GameScene 后把当前运行时卡包材质切到透明队列 `3000`，保留原 Renderer 排序以及光效自身排序；没有修改制作方材质资产。
- 资源边界：没有修改 `PackItem.prefab`、`CardPackOpeningModel_001-006`、`test.playable`、模型材质或 `fx_chai_w_001` 粒子配置。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误，`git diff --check` 通过。Editor.log 已确认 Timeline 在场景交接前后保持约 `0.8s` 并完整播放到 `5.533s`，不是提前销毁。Unity 中仍需分别验证普通 Vol1 完整彩包和 Vol2+ 系列完整彩包，重点检查交接瞬间卡包不消失、标签不缩小上移、滑光遮挡关系和动画结束显隐。

## 2026-08-31 结算加成文本拆分

- 状态：代码与需求记录已修改，Runtime/Editor 编译通过，等待 Unity Play Mode 验收。
- UI 职责：`TaskTitle2` 固定显示编辑器中的“卡包数”并在结算初始化时隐藏；`TaskTitle21` 逐条显示加成名称；`TaskTitle22` 同时显示该条加成的实际 `+N分`。
- 显隐流程：基础分滚动期间三个标题全部隐藏；每条加成阶段只显示 `TaskTitle21/22`；所有加成结束后清空并隐藏 `TaskTitle21/22`，再显示 `TaskTitle2`。无加成时在基础分滚动结束后直接显示 `TaskTitle2`。
- 编辑器边界：代码只绑定、填写内容和控制显隐；保留用户在 `GameScene.unity` 中设置的字体、材质、颜色、对齐、位置和尺寸。加成名称与分数沿用单行自动缩小规则。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`。`Assets/Scenes/GameScene.unity` 及 `Assets/UI/TempImages` 当前改动来自用户，本次没有覆盖。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；本次代码和文档差异通过 `git diff --check`。全工作区检查仅报告用户新增 `GameScene.unity` 节点中 Unity 空字段的尾随空格，本次没有重写场景。Unity 中仍需分别验证“至少一条加成”和“无加成”两种结算。

## 2026-08-31 首页卡包排序调整

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 数据验收。
- 最新顺序：第一层是当前游戏进程新获得的卡包；新包从 `Unlocked` 进入 `InProgress` 但第一波未完成时继续保留原置顶位置。第二层是非本次新获得、但第一波已经完整完成且整包未完成的 `InProgress` 卡包。第三层合并普通 `Unlocked` 与第一波未完成的旧 `InProgress`，统一按原 `UnlockTime` 正序，因此打开旧卡包但未打完第一波不会改变位置。第四层是 `Completed`，按首次 `CompletionTime` 倒序，最新完成排在完成区最前、越早完成越靠后。
- 系列规则：排序仍先对真实 PackId 执行，再折叠系列槽。新解锁 A02 时，A02 的本次新获得优先级先出现，随后折叠成 A01+A02，所以整个堆叠槽位跟随 A02 到列表最前。已完成 A01 重玩时生命周期和首次完成时间均不修改，单包或系列槽位置保持不变。
- 数据边界：继续复用进程内 `sNewlyUnlockedPackIds`、SQLite `UnlockTime`、首次 `CompletionTime` 和第一波 Piece 进度；没有修改表结构，无需删除本地数据。
- 修改文件：`Assets/Scripts/Model/CardPackDataUtility.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误，`git diff --check` 通过。仍需在 MainScene Play Mode 依次验证：新获得未开始、新获得第一波未完成、旧包第一波未完成、旧包第一波完成、连续完成两个卡包、A01 完成后新解锁 A02、已完成旧包重玩。

## 2026-08-31 首页系列卡包独立叠加修复

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 用户规则：系列槽中的前后卡包都是完整、独立的卡包状态；两者均使用各自在普通列表中的标准尺寸，不得额外缩放。后层只允许改变位置、Z 轴 `+7°/-7°` 旋转和层级，旋转后应自然露出上下左右各角。
- 实现：废弃把后层压缩成 `PackCover2` 单张附属图片的旧路径。Vol2 及以上会从同一个 `PackItem.prefab` 再实例化上一已解锁 Vol，并与前层统一调用 `ApplyPackageVisualState`；因此后层会独立加载自己的封面、`PackBg`、撕口蒙版、完成态材质、`PackSize`、`PackVol` 和进行中碎片。
- 尺寸与层级：前后实例都由 `PreparePagedPackageItem` 使用同一套列表标准尺寸；根节点 `localScale` 固定为 `Vector3.one`。后层与前层中心对齐，作为列表槽根节点下的完整子实例置于前层视觉之前，只额外设置稳定的 `+7°/-7°` 旋转。已删除旧的底边 Pivot、Shader `_PaddingY` 补偿和后层专用缩放逻辑。
- 共享呼吸动画：系列槽创建唯一的 `SeriesAnimationRoot`，把前后两个完整卡包视觉共同放在该节点下；两个卡包自身的 Animator 会关闭并恢复静态局部姿态，只由父节点 Animator 播放一次 `PackAniBreath`。因此卡包状态仍独立，但呼吸时保持相对位置和角度，作为一个列表槽整体运动。
- 运行时修复：`SetPackageSizeImageVisible`、`SetPackageVolumeImageVisible` 和 `SetPackageProgressPiecesVisible` 原先在 `entry == null` 时仍递归调用自身，造成 `StackOverflowException`；现已增加明确终止条件。背景显隐递归也统一为先判断 Entry，再按可用组件处理。
- Vol 选中进场：保留主卡包现有 `0.4s` 弹起放大动画。点击瞬间将列表后层对应的真实 Vol 卡隐藏，并直接设置为 Z 轴 `0°`、左侧卡位最终缩放和主卡最终中心位置；主卡展开期间后层卡不播放、也不显示任何旋转、缩放或移动。主卡完全展开且经过现有 `0.15s` 停顿后，后层卡才从主卡背后显示，并保持尺寸不变，只沿 X 轴滑向左卡位。
- 首次展开和关闭修复：实际用于复制卡包并执行起终点插值的是运行时独立创建的 `SelectedCardPackCanvas`，不是 `PanelBagVol`。旧逻辑在创建后立即禁用整个 Canvas，第一次读取起终点时 `CanvasScaler` 尚未在激活状态下建立最终坐标系；首次显示又会触发缩放系数更新，因而出现“屏幕中心偏小后突然放大”，首次关闭也会先缩到屏幕中心。现在保持该 Canvas 根节点持续激活并完成一次强制刷新，只切换子节点 `SelectedCardPackImage` 的显隐；展开、关闭以及后续点击始终共用同一坐标系。`PanelBagVol/PackCarousel` 的不可见预布局仍保留，用于保证编辑器卡位矩形有效。
- 展开标签修复：完整新包、彩色撕开包和灰色完成包创建选中放大视觉后，都会重新按真实 PackId 刷新左侧 `PackSize`；`PackageEntry` 同时记录该卡包在系列中的真实 Vol 序号，Vol2 及以上加载对应 `PackVolN.png` 并显示右侧标签，Vol1 保持不显示。标签位置、尺寸、颜色和普通/完成态材质继续继承当前卡包与 `PackItem.prefab`，不增加代码视觉补偿。
- 兼容：Prefab 中旧 `PackCover2` 节点运行时强制禁用，仅保留资源兼容，不再参与系列叠加。列表显隐、进行中碎片浮动和选择页切换会递归处理后层完整实例。
- 数据边界：未修改 `Series` 配置、发包规则、数据库或进度结构，无需删除本地数据。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 均为 `0` 警告、`0` 错误；旧的 `SecondaryImage`、Pivot 和 `_PaddingY` 补偿调用扫描无残留，三个递归显隐入口均已具备空节点终止条件。静态调用扫描确认选中层显隐不再禁用 `SelectedCardPackCanvas` 根节点。仍需重启 Unity 后在 MainScene Play Mode 首次点击并首次关闭 `15 -> 2` 或 `18 -> 3` 系列槽，确认不再出现中心偏小、突然放大或关闭回位错误；展开完整新包时还需确认左侧尺寸标签使用该 PackId 的配置、右侧显示真实 Vol2 标签，Vol1 不显示 Vol 标签。

## 2026-08-30 首页系列卡包叠加与 Vol 轮播

- 状态：历史实现；列表后层展示已由 2026-08-31 的完整 `PackItem` 双实例方案替代。
- 行为：根据 `CardPacks.csv` 的 `Series` 链把已解锁的同系列卡包合并为一个列表槽位；当前最高已解锁 Vol 使用前层 `PackCover`，上一已解锁 Vol 使用后层 `PackCover2`，因此系列不会重复占用首页网格位置。
- Vol 标识：Vol1 不显示 `PackVol`；从 Vol2 开始加载 `Assets/UI/PackImages/PackVolN.png`。当前资源支持 `PackVol2.png` 至 `PackVol6.png`，资源缺失时隐藏标识，不显示 Prefab 默认占位图。`PackVol` 与 `PackSize` 共用同一套普通/完成态材质切换，卡包完成后两者同步置灰。
- 后层卡包：本段记录的是已废弃的 `PackCover2` 单图实现；当前后层已改为完整 `PackItem`，固定旋转角度以顶部 2026-08-31 规则的 `+7°/-7°` 为准。
- 现场修正：首轮 Play Mode 已确认 `22` 个已解锁卡包折叠为 `20` 个系列槽。末页左侧看似残留两个空格的最终根因是 `GridLayoutGroup` 使用 `UpperCenter`，两个末页卡包会在六列区域内居中；现改为按完整六列总宽计算固定左右边距，再使用 `UpperLeft` 从统一的第一列起点排列，满页仍相对 Viewport 居中。旧卡包和旧分页仍在 `Destroy` 前立即停用，避免延迟销毁参与布局。后层纹理明确加载上一 Vol 的真实 PackId，例如 `15 -> 2`、`18 -> 3`；当前完整实例、同尺寸和 `7°` 旋转规则以顶部 2026-08-31 记录为准。
- 交互：点击系列组合槽后打开编辑器现有 `PanelBagVol`，只生成该系列已解锁成员，初始居中最高已解锁 Vol。进场严格按参考视频分段：主卡包先用约 `0.4s` 单独从列表槽放大；底部返回/玩/相机在放大后半段延迟上滑；主卡包到位后保持约 `0.15s`，相邻 Vol 再用约 `0.2s` 从主卡包背后向两侧展开，分页圆点从侧卡展开约三分之一处开始淡入，不能提前出现。横向拖动时卡包同步移动，进入中心的卡包按 `PackCenter` 放大、离开中心的卡包按 `PackLeft/PackRight` 缩小；松手后以 `0.25s` EaseOut 吸附到最近 Vol，左右按钮使用同一吸附动画，分页圆点、`玩/重玩`、相机按钮随居中卡包刷新。展开后的中心卡与侧卡继续播放 `PackItem.prefab` 自带的 `PackAniBreath`，程序不覆盖动画位移、缩放、速度和相位；末帧只把轮播卡包本体、`PackNode` 与封面的 Z 轴旋转归零，避免继承首页叠放角度。
- 流程衔接：居中 Vol 的完整彩包、彩色撕开包和灰色完成包分别继续复用现有开包、直接继续和重玩确认流程；拍照使用居中 PackId。重玩取消返回 `PanelBagVol`，返回按钮把当前居中卡包缩回原系列列表槽。非系列卡包仍使用 `PanelBagSelect`。
- UI 边界：直接读取用户在 `MainScene` 中搭建的 `PackCarousel/PackLeft/PackCenter/PackRight`、五个按钮和 `PageIndicators/DotTemplate`；代码不覆盖编辑器卡位、按钮和圆点的美术参数，只把三张占位卡隐藏并按其位置与缩放生成真实卡包。
- 数据边界：未修改 `Series` 配置、发包规则、数据库或进度结构，无需删除本地数据。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/View/PackageInteractionHandler.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：现场日志确认 `unlocked=22, slots=20`，系列折叠数量正确；Play Mode 截图确认 `15 -> 2`、`18 -> 3` 两个系列槽的后层真实封面均已显示，尺寸与前层一致，旋转后侧边和底角可见。`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译均为 `0` 警告、`0` 错误，`git diff --check` 通过。系列初次进入动画、拖动/按钮切换、边界吸附、分页圆点、三种卡包状态的玩/重玩衔接、拍照、返回，以及普通卡包仍进入 `PanelBagSelect` 仍待完整交互回归。

## 2026-08-28 首页卡包排序分层

- 状态：排序代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 数据验收。
- 现有问题：原比较器把全部 `InProgress` 放在同一级，无法区分“第一波尚未完成的新卡包”和“第一波已经完成的已解锁未完成卡包”。
- 最新顺序：第一层为本次新发放卡包以及 `InProgress` 且第一波未完整完成的卡包；第二层为第一波已完整完成的 `InProgress`；第三层为 `Unlocked` 未开始卡包；第四层为 `Completed` 卡包。
- 层内顺序：第一层按解锁时间倒序，使最近获得的新卡包优先；第二、三层按解锁时间正序；第四层按首次完成时间正序；时间相同或无效时用 PackId 保证跨设备确定性。
- 会话规则：`sNewlyUnlockedPackIds` 只保存在内存中，列表读取和重复进入 MainScene 均不消费；同一游戏进程内仍为 `Unlocked` 的新发放卡包持续置顶，只有进程初始化时清空，因此重启游戏后未开始的新发放卡包才恢复到第三层解锁顺序。真实进度优先于新发放标记：进入 `InProgress` 后，第一波未完成继续视为新卡包，第一波完成立即进入第二层；整包完成立即进入第四层。生命周期为 `Completed` 的重玩卡包不改变首次完成顺序。
- 性能：排序前只对 `InProgress` 卡包各计算一次第一波完成状态，比较器只查询预计算 HashSet，不在比较过程中重复加载 CardBag Prefab 或读取 SQLite。
- 边界：不修改 SQLite 表、生命周期枚举、解锁时间、首次完成时间或拼图进度结构，不需要删除本地数据。
- 修改文件：`Assets/Scripts/Model/CardPackDataUtility.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`Documents/GAME_DESIGN_REQUIREMENTS.md`、`specs/spec-driven-development.md`。
- 验证：`git diff --check` 通过；旧方法名和旧的“一次性消费”语义扫描无残留；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译均为 `0` 警告、`0` 错误。静态核对确认所有正常发包入口通过 `TryUnlockPack` 记录本进程新包，第一波完成状态通过持久化 Piece 进度逐个核对 `Piece01XX`。Unity Play Mode 仍需准备四类卡包以及同一进程新发包，检查同一进程反复进入首页仍置顶，并确认重启后恢复正常排序。

## 2026-08-28 首页 QQ 按钮

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 验收。
- 行为：MainScene 初始化时查找现有 `BtnQQ` 并绑定点击事件；点击后使用 `Application.OpenURL` 打开用户提供的 QQ 群链接，目标群号为 `1079431440`。
- 整理：愿望单、Discord 和 QQ 三个外链按钮统一使用 `ConfigureExternalLinkButton` 查找 Button、输出缺失警告并先移除同一监听再绑定，三个入口仍保留各自独立的 URL 常量和点击回调。
- 边界：复用场景中现有 Button、位置和美术资源，不修改其他首页入口，不解码或重写 URL 中的 `%2B`、`authKey` 等查询参数。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：完整 URL 静态核对通过，`%2B` 与全部查询参数保持原样；`git diff --check` 通过；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译均为 `0` 警告、`0` 错误。Unity Play Mode 仍需点击 `BtnQQ`，确认系统打开指定 QQ 群链接并能识别群号 `1079431440`。

## 2026-08-28 首页 Discord 按钮

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 验收。
- 行为：MainScene 初始化时查找现有 `BtnDiscord` 并绑定点击事件；点击后使用 `Application.OpenURL` 打开 `https://discord.gg/sfmNFEF5ec`。
- 边界：复用场景中现有 Button、位置和美术资源，不修改其他首页入口；重复初始化时先移除同一运行时监听，避免重复打开。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`git diff --check` 通过；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译均为 `0` 警告、`0` 错误。Unity Play Mode 仍需点击 `BtnDiscord`，确认系统打开指定 Discord 邀请链接。

## 2026-08-28 首页愿望单按钮

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 验收。
- 行为：MainScene 初始化时查找现有 `BtnWishList` 并绑定点击事件；点击后使用 `Application.OpenURL` 打开 `https://store.steampowered.com/app/4906510/?utm_source=InGame`。
- 边界：复用场景中现有 Button、位置和美术资源，不添加 Steamworks SDK 依赖，不修改其他首页按钮或导航逻辑；重复初始化时先移除同一运行时监听，避免重复打开。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`git diff --check` 通过；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译均为 `0` 警告、`0` 错误。Unity Play Mode 仍需点击 `BtnWishList`，确认系统打开目标 Steam 商店 URL 且保留 `utm_source=InGame`。

## 2026-08-28 PackageScrollView 横向软裁切

- 状态：场景和 Shader 修改完成，Unity 已重新导入且 Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 根因：`PackageScrollView/Viewport` 同时存在旧 `Mask` 和新 `RectMask2D`，旧 Mask 会先执行硬裁切；同时 `PackCoverShadow.shader` 与 `PackSizeState.shader` 只使用 `UnityGet2DClipping` 做硬边裁切，没有读取 RectMask2D 写入的 `_UIMaskSoftnessX/Y`。
- 修改：保留用户设置的 `RectMask2D Softness=(83,0)` 并移除同节点旧 `Mask`；两个卡包 UI Shader 按 Unity UI 标准算法计算像素级软裁切，使封面、投影、撕口状态和尺寸标签统一响应横向柔边。
- 布局修正：运行时 `GridLayoutGroup.childAlignment` 从覆盖场景配置的 `UpperLeft` 改为 `UpperCenter`，六列卡包按实际总宽在每页 Viewport 内左右居中；Viewport Image 颜色 Alpha 设为 `0`，消除旧 Mask Graphic 重新显现造成的上下矩形底色，同时保留 `RaycastTarget` 以支持从列表空白区域起手拖拽。
- 边界：不恢复代码控制的列表渐隐，不创建卡包 CanvasGroup，不修改卡包材质参数、分页吸附、Viewport 尺寸或卡包状态逻辑。普通 UGUI Image 继续使用 Unity 自带 RectMask2D 支持。
- 修改文件：`Assets/Scenes/MainScene.unity`、`Assets/Resources/PackCoverShadow.shader`、`Assets/Resources/PackSizeState.shader`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：场景 YAML 确认 Viewport 只保留 `RectMask2D Softness=(83,0)`，Viewport Image 为透明且仍接收射线；Unity 自动重新导入场景并启动 Shader 编译器，Editor 日志无 Shader 编译错误；`git diff --check` 通过；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译均为 `0` 警告、`0` 错误。Unity Play Mode 仍需确认六列左右居中、上下无矩形底色、左右各约 `83px` 范围平滑淡入淡出，且空白区域拖拽分页仍可用。

## 2026-08-28 首页卡包列表整页吸附与边缘渐隐移除

- 状态：边缘渐隐已按最新要求移除，整页吸附保留，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉与手感验收。
- 列表显示：不再为运行时卡包根节点创建渐隐 CanvasGroup，也不再按 Viewport 左右剩余可见宽度计算 Alpha。进入或离开 Viewport 的卡包始终使用资源和现有状态逻辑决定的原始不透明度。
- 整页吸附：卡包列表拖拽松手后立即停止原 ScrollRect 惯性，按 Content 当前活动页数选择最近整数页，并在 `0.26s` 内 EaseOut 滑到完整页面；单页固定在第一页，刷新列表时取消旧吸附并归零。
- 输入覆盖：从卡包起手继续由 `PackageInteractionHandler` 转发 ScrollRect，同时通知 MainScene；从列表空白区域起手由 ScrollView 运行时 EventTrigger 通知 MainScene。两者共用同一吸附逻辑。吸附期间不接受卡包点击，新拖拽可立即中断吸附。
- 保留：每页 `18` 个、页面尺寸、卡包顺序、点击选择、状态显隐、呼吸动画和资源均未修改；未新增脚本文件，未修改 MainScene 场景或 PackItem Prefab。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；Unity Play Mode 仍需验证卡包边缘保持原始不透明度，以及卡包起手、空白起手、慢拖、快拖、第一页和末页吸附。

## 2026-08-28 卡包独立放大页底部按钮进出场

- 状态：代码修改完成，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 进场：点击首页列表卡包进入独立放大页时，`BtnBack`、`BtnPlay` 和当前状态允许显示的 `BtnCamera` 从 `PanelBagSelect` 下边界之外同步向上滑到场景原坐标；与卡包放大同时开始，按钮时长由原 `0.3s` 放慢 `30%` 为 `0.39s`，卡包自身 `0.3s` 节奏不变。
- 出场：返回首页列表或确认进入游戏流程时，按钮使用进场的时间反向曲线从原坐标向下滑回同一屏幕外位置。重玩确认和拍照面板只是临时覆盖，不重复触发按钮出场。
- 布局：三个按钮的场景层级、尺寸、最终 X/Y 坐标均未修改；运行时缓存各自终点，只统一插值 Y，因此横向间距保持不变。屏幕外起点按面板实际下边界、最大按钮半高和 `24px` 余量动态计算。
- 状态：按钮进出场均不修改 Alpha，不做渐现或渐隐；动画期间虽然不可交互，但 Disabled 颜色强制与 Normal 颜色一致，因此按钮从屏幕外出现时就是完整不透明度，结束时不会发生透明度切换。相机按钮继续只对已完成卡包显示；“玩/重玩”文字和既有状态判断不变。页面隐藏、失败或下次打开前统一恢复缓存终点，不累计位移。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；仍需分别目视验证三种卡包状态的进场完整不透明度、返回出场和进入游戏出场。

## 2026-08-28 开包代码适配制作方原始 Timeline

- 状态：运行时代码修改完成，Runtime/Editor 编译通过，等待 Unity 刷新与 Play Mode 视觉验收。
- 根因：重新导入的原始 `test.playable` 只有 Activation、单一 Animation 和 `fx_chai_w_001` Control 三条根轨；旧代码硬性要求后期改造版的第二条 Animation、Recorded、`Image` 和 `blur`，因此会在绑定检查阶段直接停止开包。
- 修改：`CardPackOpeningEffect` 按制作方 `EffectScene001` 原始绑定方式，将 Activation Track 绑定当前模型 GameObject、唯一 Animation Track 绑定模型 Animator、滑光 Control Track 绑定 MainScene `PackObject/fx_chai_w_001`；播放从原版 `0s` 开始。
- 保留：原始 Timeline、FBX、Prefab、材质和粒子参数均未修改；动态封面替换、模型与 `600x680` 静态卡包对齐、`0.800s` GameScene 交接、跨场景续播和末帧 Hold 行为保持不变。
- 原版时序：`Take 001=0~1.8333s`，`fx_chai_w_001=0.5333~5.5333s`，Activation=`0~5s`，Timeline 总长约 `5.533s`。
- 首次 Play Mode 实际日志确认动画仍不可见的直接原因发生在 Timeline 之前：随机到 `CardPackOpeningModel_002/003` 时，旧代码因没有找到五位编号背面 Renderer 而报 `expected card renderers were not found` 并立即清理。原始 FBX 中仅 `Model_001` 同时包含正面和背面，`Model_002~006` 只有正面；现已改为正面必须存在、背面可选且存在时仍禁用。
- 验证：Renderer 修复后 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 再次顺序编译通过，均为 `0` 警告、`0` 错误；原包 Timeline 哈希保持一致。仍需回到 Unity 触发脚本刷新后，从完整彩包验证撕开、下落、滑光、`0.800s` 跨场景交接和末帧保留。

## 2026-08-28 制作方两个特效包原样重新导入

- 状态：两个 `.unitypackage` 已按包内原始路径、GUID 和内容完整覆盖到工程，等待 Unity AssetDatabase 刷新及 Play Mode 视觉验收。
- 来源：`特效资源/effect文件夹.unitypackage`、`特效资源/场景卡包和特效展示.unitypackage`；同目录 MKV 未处理。
- 导入范围：第一个包 `441` 个资源条目，第二个包 `184` 个资源条目；两包有 `152` 个重叠路径，重叠资源的 GUID、资源内容和 `.meta` 完全一致。
- 实际操作：原样复制 `587` 个资源文件和 `625` 个 `.meta`；只创建或覆盖包内路径，不删除包外资源，不重命名，不调整尺寸、位置、材质、粒子、相机、Animator 或 Timeline 参数。
- 校验：重新逐项计算 SHA-256；两个包的全部资源和 `.meta` 与工程对应文件一致，差异数均为 `0`。导入前工作区干净；导入后 Git 差异均来自原包恢复及本任务记录。
- 注意：Unity 当前保持打开，`Library/ArtifactDB` 与 `SourceAssetDB` 尚未刷新到本次导入时间。回到 Unity 后先等待资源导入结束并检查 Console，再分别预览制作方特效场景和完整彩包拆包流程；验收前不得再用代码或 Prefab 覆盖制作方参数。

## 2026-08-28 完整彩色卡包拆包动画结束后保留

- 状态：代码修改完成，等待 Unity Play Mode 视觉验收。
- 用户要求：完整彩色卡包的拆包 Timeline 只播放，播放结束后不清理、不销毁整套特效对象；彩色撕开和灰色撕开流程不变。
- 根因：`PlayableDirector` 原使用 `DirectorWrapMode.None`；Timeline 到 `7s` 自然停止时自动回到 `0s`，而 Recorded Track 的 `0s` 曲线把卡包两个 Renderer 的 `m_Enabled` 设为 `0`，所以即使移除 `Destroy`，卡包仍会突然不可见。
- 修改：保持 `test.playable` 中所有 Clip 的起点和时长不变；`PlayableDirector` 改用 `DirectorWrapMode.Hold`，播放监控在末帧到达前把 Director 精确设为 Timeline 总时长、Evaluate 并 Pause，使画面保持最终状态且不回到 `0s`。自然结束只完成临时碎片状态、记录日志并停止播放监控，不调用 `CleanupPlaybackResources()`，也不调用 `Destroy(gameObject)`；`stopped` 回调只作为异常漏网兜底。
- 保留：准备失败、异常中断或对象被外部销毁时仍可进入原有资源清理逻辑，避免改变异常路径。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`dotnet build Assembly-CSharp.csproj --no-restore` 与 `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 均为 `0` 警告、`0` 错误，`git diff --check` 通过，`test.playable` 无工作区差异；Unity Play Mode 需确认完整彩包保持原动画节奏，Timeline 结束日志为 `time=7.000s`、`source=held final frame`，不得再出现结束时 `time=0.000s` 或卡包突然隐藏。
- 下一步：Unity Play Mode 验收本次“只播放、不销毁”行为，不调整尺寸、蒙版、相机、Timeline 起点或动画参数。

## 2026-08-28 CardBag 新建 Prefab 保存阻断移除

- 状态：保存限制已移除，Runtime/Editor 编译通过，等待 Unity 内重新生成 CardBag006/019/020。
- 根因：删除旧 Prefab 后，生成器创建的新 Prefab 尚未写入 AssetDatabase；`CardBagPrefabReferenceSaveGuard.OnWillSaveAssets` 却先按目标路径加载现有 Prefab，得到 `null` 后把保存路径从回调结果中移除。Unity 日志明确记录 `Prefab not saved due to OnWillSaveAssets callback`，因此生成器最后统一报 `failed to save`，与图片识别和布局生成无关。
- 修改：移除生成、位置更新、层级更新、阴影更新中的额外引用阻断，并删除全局 `OnWillSaveAssets` 保存守卫。生成器原有的源资源完整性、定位置信度、层级和阴影校验保持不变；导入后和命令行引用检查仅作诊断，不再阻止创建或保存。
- CardBag006/019/020 源 PNG 的 `.meta` 已由 Unity 重新生成并处于未跟踪状态；重新生成 Prefab 后应将这些 `.meta` 与对应 Prefab 一起提交，不能只提交 Prefab。
- 验证：日志中的三次失败均由旧保存回调阻断；移除后 Runtime/Editor 编译均为 `0` 警告、`0` 错误。仍需 Unity 完成脚本刷新后重新执行 **Generate CardBag Prefabs**，确认三包保存成功。

## 2026-08-28 完整彩色卡包拆包动画提前移除根因修复

- 状态：已从根源改为制作方完整 Timeline 驱动，Runtime/Editor 编译与静态检查通过，等待 Unity Play Mode 视觉验收。
- 最终根因：运行时代码从未播放 `Assets/Resources/Effects/CardFx/Animations/test.playable`，而是手动播放 `Take 001`、按 `0.5s` 启动滑光并用 Animator/ParticleSystem 状态决定清理，因此完全跳过 Timeline 的 `0~2.633s` Recorded 轨道。该轨道包含静态卡包交接、模型整体移动/缩放以及卡包撕开后的下落动作；继续调整滑光时长不可能恢复这些画面。
- 全工程扫描确认，完整开包的运行时播放、隐藏、停止和销毁入口只在 `Assets/Scripts/Controller/MainScene.cs` 的 `CardPackOpeningEffect`。已删除 5 类错误替代逻辑：手动淡出静态卡包、手动播放 Animator、手动启动滑光、按局部 `0.800s` 计时交接、按 Animator/粒子状态清理。
- 第一次修复回归：直接从 Timeline `0s` 播放会重复制作方原本用于 `240x272 -> 600x680` 的放大阶段；当前游戏点击拆包时卡包已经在选择页放大到 `600x680`，再次按 `2s` 的小卡包帧对齐会使 `2~2.633s` 又放大约 `2.28` 倍，造成尺寸错误。把制作方 `blur 2~7s` 还原成可见 `blue.mat` UI 也会在当前 `BgGame` 舞台上形成用户不需要的整屏蒙版。
- 最终修复：仍使用 `test.playable` 和 `PlayableDirector` 驱动拆包、下落、滑光及完成回调，但自动读取 Recorded Animation Track 的 `infiniteClip.length`，从其真实结束帧约 `2.633s` 开始播放。在该“大卡包完成帧”先将模型正面与当前 `600x680` 静态卡包中心和高度对齐，再隐藏静态卡包并继续 `Take 001`。`fx_chai_w_001` 继续绑定 MainScene `PackObject` 下美术调好的现有实例；`blur` Control Track 只绑定无 Renderer 的代理对象，不创建 Canvas、Image 或 `blue.mat`，因此不会显示额外蒙版。
- Timeline 资源总长仍为 `7s`：`Take 001` 为 `3.4667~5.3s`，滑光为 `3.9667~7s`。实际运行从约 `2.633s` 开始，GameScene 交接点仍按 `Take 001` 起点加真实下落关键帧 `0.800s` 计算为 `4.2667s`；交接前暂停 Director，GameScene 自身 MainCamera 绑定 EffectLayer 后等待一帧恢复。
- 正常释放的唯一入口是 `PlayableDirector.stopped` 完整结束回调。回调内统一释放模型、滑光、blur、运行时材质、临时 Piece 和跨场景根对象；不再使用滑光结束、粒子 `IsAlive`、Animator `normalizedTime` 或固定延时触发正常销毁。异常中断和对象销毁仍保留防泄漏强制清理，但会先取消完成回调，不能伪装成自然播放结束。
- 彩色撕开、灰色撕开的静态卡包下落、假碎片和真实 Piece 发牌参数未修改。
- 验证：回归修复后的 Runtime/Editor 编译均为 `0` 警告、`0` 错误，`git diff --check` 通过。当前 Unity `Library/ScriptAssemblies/Assembly-CSharp.dll` 是 `16:29:31`，本轮源码为 `16:32` 之后，仍需退出当前 Play Mode 并等待 Unity 刷新后视觉验收。刷新后准备日志应显示 `timelineDuration=7.000s, start=2.633s, handoff=4.267s`；画面不得出现 blur 蒙版，静态卡包切换模型前后都应保持 `600x680` 对齐，最后仍只在 `opening timeline completed callback` 后释放。

## CardBag Prefab 跨设备引用诊断

- 状态：2026-08-28 已完成工作区修复和磁盘级静态校验，等待用户提交并推送。
- 最终根因复核：`1a3be43` 写入的 26 个 CardBag019 GUID 与仓库 `.meta` 一致，是正确状态；后续 `9720caf` 在 `LIN-WORK` 被 Unity 本地 AssetDatabase 缓存误导，把 CardBag019 的 26 个引用反向恢复为仓库中不存在的 GUID，同时把 CardBag006/020 的 `BoardTitle` 改成跨包或不存在的 GUID。该提交只包含 Prefab，没有对应源 PNG `.meta`，因此其他设备必然丢失引用。此前工作流对两个提交的判断方向错误，现已纠正。
- 已按仓库当前 `.meta` 恢复 CardBag019 的 26 个 Sprite GUID，以及 CardBag006/020 各一个 `BoardTitle` GUID；没有重新生成 Prefab，也没有改变布局、分组、阴影或描边。
- 引用验证器保留导入后和命令行的磁盘 Prefab YAML/`.meta` 对照诊断，但不再注册保存守卫，也不参与生成器、位置更新、层级更新或阴影更新的保存流程。
- 验证：CardBag019 为 `Expected=31, Missing=0`；全部 23 个 CardBag Prefab 的磁盘 YAML/Meta 扫描为 `Prefabs=23, Failed=0`；Runtime/Editor 编译和 `git diff --check` 通过后才允许提交。
- 下一步：重新生成 CardBag006/019/020，检查 Prefab 内容后将新 Prefab 与 Unity 重新生成的源 PNG `.meta` 一起提交。

## 2026-08-28 拆包动画跨场景续播与碎片后层修复

- 状态：根因修复完成，Runtime/Editor 编译通过，等待 Unity Play Mode 视觉验收。
- 前一轮按固定总时长延后清理仍未解决实际消失：拆包对象虽然已转为 `DontDestroyOnLoad`，但 GameScene 的新 Main Camera 没有包含专用 EffectLayer 31，因此在 `0.800s` 场景交接时视觉上立即消失。本轮在新场景加载后只查找该场景自身的 MainCamera，将 EffectLayer 临时加入其 Culling Mask；特效自然结束并清理时恢复该相机原始 Mask。
- 光效在制作方 `0.5s + 3.033s` 控制时段结束后不再立即 `StopEmittingAndClear`。现在先对全部 ParticleSystem 执行 `StopEmitting`，等待现有粒子 `IsAlive` 自然结束后再清理；`10s` 仅作为异常循环粒子的泄漏兜底，不参与正常节奏。
- 前一轮只给碎片阴影 Shader 增加 `LessEqual` 深度测试没有改变碎片本体的绘制顺序，无法可靠把它压到卡包后面。本轮撤销该通用 Shader 改动；完整彩包拆包阶段明确使用背景 `1999`、临时碎片 `2000`、卡包正面 `2001` 的连续 Render Queue。GameScene 接管碎片只在撕口初始两帧保持 `2000`，正式飞向托盘前恢复 `IngameCoverShadow04` 原始队列，避免被游戏 UI 遮挡。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`dotnet build Assembly-CSharp.csproj --no-restore` 与 `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Unity Play Mode 确认模型和滑光跨入 GameScene 后不闪断、尾粒子自然消失，以及碎片从撕口出现时始终位于卡包实体后层。

## 2026-08-27 拆包碎片速度、数量、阴影与层级

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 完整彩包拆包时不再固定读取第一组全部 Piece；MainScene 读取 SQLite 已拼编号，按照和 GameScene 相同的规则选择首个仍有未完成 Piece 的组，只创建该组尚未拼上的真实碎片。已经拼在棋盘上的 Piece 不会在拆包阶段重复出现，临时视觉数量与 GameScene 接管后的真实可交互数量一致。
- 完整彩包碎片从撕口冒出的 EaseOut 时长由 `0.16s` 调整为 `0.32s`，启动点、最终尺寸、散点范围、`0.800s` 场景交接点和后续发牌参数不变。
- MainScene 临时碎片使用 `IngameCoverShadow04` 的 SpriteRenderer 兼容运行时材质和 FullRect Sprite。该节最初尝试的通用 Shader `LessEqual` 深度方案已被 2026-08-28 的修复废弃并撤销，实际层级改由背景 `1999`、碎片 `2000`、卡包 `2001` 的 Render Queue 保证。GameScene 在三种卡包入口发牌前仍统一应用 `IngameCoverShadow04` 初始阴影。
- 移除 `ModelVisibleDuration=1.6s` 对制作方动画的强制截断。GameScene 仍在 `Take 001` 的 `0.800s` 下落关键帧交接；跨场景特效继续保留，模型 Clip 完整播放约 `1.833s`，`fx_chai_w_001` 按 `0.5s` 延迟加约 `3.033s` 正式控制轨道完整播放，整体约 `3.533s` 后才停止粒子并清理对象。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；Unity 已刷新修改后的 Shader，最新 `Editor.log` 未出现 Shader error；`git diff --check` 通过。仍需在 Play Mode 分别验证三种状态的碎片阴影、完整彩包卡包实体/撕口遮挡关系、完整 `3.533s` 特效收尾，并用存在部分已拼进度的卡包确认拆包阶段不重复创建已拼 Piece。

## 2026-08-27 完整彩包下落与游戏入场同步

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 使用 Unity `AnimationUtility` 读取制作方 `CardPackOpeningAnimation.FBX/Take 001` 曲线，确认卡包骨骼在 `0.800s` 到达纵向最高点、下一帧开始下落；原流程等待到 `1.600s` 才激活 GameScene，导致碎片完成 `0.16s` 短跳后存在约 `0.77s` 的额外空档。
- `CardPackOpeningEffect` 改为自行驱动完整播放；MainScene 在 `0.800s` 真实下落节点保存散点中心并立即激活预加载完成的 GameScene。棋盘、托盘、按钮和真实 Piece 从该节点开始原有并行动画，不修改 `0.39s` 单片发牌、`0.027s` 错峰、Piece 缩放或棋盘/托盘自身时长。
- 场景交接时隐藏 MainScene 临时 Piece，开包 3D 模型与 `fx_chai_w_001` 转为短期跨场景对象；最新规则取消旧 `1.600s` 清理上限，模型完整播放约 `1.833s`，光效按制作方控制轨道播放到约 `3.533s` 后自动清理。不恢复静态撕开包，不创建第二份卡包下落视觉。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；Unity 批处理资源刷新成功。仍需在 MainScene Play Mode 验收滑光完成、卡包开始下落、棋盘/托盘进场三者的实际同拍效果，并确认场景交接没有重复 Piece、卡包闪断或粒子残留。

## 2026-08-27 非发牌转场整体延长 20%

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 使用共享常量 `GameDefine.NonDealTransitionDurationMultiplier = 1.2` 统一延长非发牌段：MainScene 首页与游戏背景交接由 `0.42s` 调整为 `0.504s`；彩色/灰色撕开卡包下落由 `0.345s` 调整为 `0.414s`；GameScene 棋盘入场由 `0.57s` 调整为 `0.684s`，托盘和按钮入场由 `0.33s` 调整为 `0.396s`。
- 完整彩包、彩色撕开和灰色撕开的棋盘/托盘/按钮参数继续共用，保持此前一致性。真实 Piece 发牌不参与本次延长，单片飞行仍为 `0.39s`，错峰仍为 `0.027s`。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/GameDefine.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需 Play Mode 对比三种卡包入口，确认非发牌段整体放慢且发牌速度不变。

## 2026-08-27 完整彩包撕开与发牌节奏统一

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 完整彩色卡包的第一组交接碎片继续读取 `CardBagNNN.prefab` 的真实 Piece Sprite。碎片启用时间由原动画启动后 `0.08s` 改为与撕开滑光完全相同的 `0.5s`；全部碎片在滑光启动同一帧从实际撕口下方、卡包模型后层显示，并用 `0.16s` EaseOut 快速向上短跳，跳跃中心高度保持卡包显示高度约 `8%`。
- 交接碎片不再从最终尺寸 `40%` 慢速放大，也不再逐片延迟启动；从第一帧直接使用最终显示尺寸并同时短跳。跳跃终点使用与彩色撕开相同的黄金角散点公式和 `20` 倍半径，MainScene 保存的是散点中心而非最终位置平均值。
- GameScene 接管完整彩包第一组后，真实可交互 Piece 从同一个散点中心按相同公式重建，因此不再聚拢。完整彩包和彩色撕开统一使用 `20` 倍起始散点、最终 `TrayScale`、`0.027s` 错峰、`0.39s` 单片飞行，以及相同的棋盘、托盘和按钮并行入场参数。完整彩包已经完成模型撕开，不恢复静态撕开包，也不重复播放卡包下落。
- 共享散点公式收敛到 `GameDefine.CalculatePieceDealScatterOffset`，MainScene 交接视觉和 GameScene 真实 Piece 使用同一数据规则。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/GameDefine.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 验收滑光和碎片同帧启动、碎片位于卡包后层、`0.16s` 短跳高度、场景交接前后散点不跳变，以及完整彩包与彩色撕开的棋盘/托盘/发牌节奏一致。

## 2026-08-27 撕开卡包真实碎片起点分散

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 根据 Play Mode 截图，将彩色撕开和灰色撕开进入 GameScene 时真实 Piece 的起始散点统一扩大：基础 `0.025~0.049` 世界单位半径使用 `20` 倍，目标屏幕散点半径约 `50~100px`；Piece 仍在卡包移动前以最终 `TrayScale` 和正常 Alpha 创建，并由卡包遮挡后自然露出。
- 彩色与灰色撕开的真实 Piece 起点、无额外停顿、`0.345s` 卡包下落、`72%` 起飞点、`0.027s` 错峰和 `0.39s` 单片飞行时长全部一致。唯一差异仍是彩色撕开收回并隐藏已有的首页假碎片，灰色撕开不执行该步骤。完整彩包拆包后的真实 Piece 接管流程不变。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 对照验收彩色和灰色撕开的真实 Piece 起点、卡包下落和发牌节奏完全一致，并确认仅彩色撕开执行假碎片隐藏。

## 2026-08-27 灰色与彩色撕开节奏统一

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 灰色撕开确认重玩后的转场改为和彩色撕开相同节奏：背景交接后不增加额外静止时间；卡包按 `0.345s` 总时长、动态离屏距离和线性速度下落；真实 Piece 在卡包移动前以最终 `TrayScale` 和正常 Alpha 创建在卡包中心附近的小范围散点，卡包下移约自身显示高度 `72%` 时开始以 `0.027s` 错峰、`0.39s` 单片时长飞向托盘。
- 两种撕开状态仅保留一个差异：彩色撕开继续把首页展示用 `ProgressPieces` 向撕口内下收并隐藏；灰色撕开没有这组假碎片，不执行任何假碎片收回或隐藏操作。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 对照验证灰色和彩色撕开卡包的停顿、下落、`72%` 起飞点和发牌速度一致，同时确认灰色流程没有假碎片显隐动作。

## 2026-08-27 彩色撕开真实碎片预创建

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 彩色撕开进入 GameScene 后，当前组真实 Piece 仍在跨场景卡包中心附近的小范围散点创建，但不再先设为透明并等到卡包下移 `72%` 后补显；真实 Piece 在卡包开始移动前就以最终 `TrayScale` 和正常 Alpha 准备完成，由上层跨场景卡包遮挡。
- GameScene 保留两帧稳定和最终托盘目标缓存，完成后才调用跨场景卡包的下移动画。卡包移开时会自然露出已经存在的真实 Piece；到 `72%` 位置只负责开始飞向托盘，不再负责临时显示，从而避免卡包先越过碎片位置后出现空档。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 验收彩色撕开卡包下移时真实 Piece 已在卡包后方、卡包越过碎片位置时无空帧或补显跳变，并确认 `72%` 起飞节奏保持不变。

## 2026-08-27 完整彩色卡包碎片蹦出节奏

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 本次只调整 `完整彩色卡包` 拆包阶段从撕口蹦出的第一组临时碎片；彩色撕开、灰色撕开和 GameScene 真实 Piece 发牌未修改。
- 临时碎片不再从 Alpha `0` 渐显；到各自原有启动时间时直接启用 Renderer，并始终保持 Alpha `1`。原逐片启动间隔保留。
- 临时碎片从最终尺寸的 `40%` 使用现有 SmoothStep 进度放大到 `100%`；全部放在卡包模型背面，使用同一个水平中心、约卡包高度 `8%` 的竖向终点和统一旋转，只从撕口向上露出少量距离，不再横向散开或使用不同高度。
- 临时碎片完成蹦出后保持重叠停在共同位置。进入 GameScene 后由真实 Piece 在同一屏幕位置接管，并继续使用完整彩色卡包原有的 `0.0135s` 错峰和 `0.39s` 单片时长飞向托盘；灰色撕开未修改。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 验收碎片位于卡包后方、直接显示、`40% -> 100%` 放大、共同位置停留及 GameScene 接管后的错峰飞行。

## 2026-08-27 彩色撕开卡包退场节奏

- 状态：代码修改完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 本次只调整 `彩色撕开`：卡包下落计时基准保持 `0.345s`；两段位移改为按实际距离分配时长的同一线速度，最终距离按卡包实际顶边与 Canvas 底边计算，确保卡包连续完整移出屏幕。`完整彩色卡包` 和 `灰色撕开` 保持原流程与曲线。
- 彩色撕开卡包内随跨场景 Canvas 带入的展示碎片不参与实际发牌；它们在卡包开始下移后相对卡包向下收约父容器高度 `28%`，低于撕口后才隐藏，不再首帧直接消失。
- 彩色撕开在首页背景交接后的最短静止等待由 `0.255s` 减少 `0.3s` 后归零；GameScene 预加载未完成时仍保留最长 `5s` 的安全等待。
- GameScene 的真实 Piece 全部同时创建在卡包中心附近的小范围散点并暂时隐藏；卡包下移约自身显示高度 `72%`、展示碎片收进撕口后，真实 Piece 直接显现并提前飞向托盘。单片时长保持 `0.39s`，错峰从 `0.0135s` 加倍为 `0.027s`；完整彩色卡包仍为 `0.0135s`，灰色撕开仍为 `0.027s`。GameScene 棋盘和托盘时长未修改。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 验收卡包连续完整移出屏幕、展示碎片向下收进撕口后隐藏、真实 Piece 小范围散点与提前发牌、`0.027s` 错峰，并回归完整彩色卡包与灰色撕开未发生变化。

## 2026-08-27 发牌动画托盘尺寸回归修复

- 状态：代码修复完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 修复 GameScene 首次入场发牌在首帧过早重算 Piece 尺寸的问题：发牌协程现在先等待两帧布局稳定，再临时把棋盘和托盘恢复到最终位置，统一刷新当前组的 `DragScale`、`TrayScale` 和托盘终点，缓存完成后立即恢复入场动画位置。
- 发牌起始、飞行过程和最终落点不再从动画中的 `Transform.localScale` 反推尺寸，统一直接使用每个 `DraggablePieceState.TrayScale`；CardBag018 等带非 1 `BoardScale` 的卡包继续使用此前的“`DragScale` 与托盘高度 90% 上限取小”规则。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Unity Play Mode 验证 CardBag018 第一组碎片飞入托盘后尺寸不跳变，并确认 CardBag003、CardBag010 的尺寸与点击拿起行为无回归。

## 2026-08-26 卡包进入游戏动画流程重排

- 状态：代码实现完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 视觉验收。
- 点击“玩”或确认“重玩”后，选中卡包保持当前位置和尺寸不动；首页根 Canvas 通过同一个 `CanvasGroup` 让卡包列表、`Background` 和其余首页内容保持原位置并同步渐隐，选中页虚化截图使用相同进度渐隐，`BgGame` 固定在同一屏幕中心渐现。运行时不再创建 `MainPageTransitionRoot`、不再重排 `PackageScrollView` 层级，列表和背景都不得横向移动；`PanelBagSelect` 继续向下滑出。
- 完整彩色卡包在背景交接后继续播放滑光提示并等待点击或横划；拆包特效完成后恢复同位置的撕开静态卡包，再进入统一的撕开包流程。彩色进行中和灰色重玩直接进入统一流程。灰色撕开卡包确认重玩时立即关闭 `PanelReplay`，直接恢复确认前隐藏的选中卡包 Canvas，保留原灰色材质、撕口蒙版、尺寸和位置，再重置会话并执行同一套切换、下落和发牌；不再从已隐藏的列表槽位重建选中视觉。
- 完整彩色卡包点击或滑动拆包时，从当前 `CardBagNNN.prefab` 按 `PieceGGII` 读取第一组全部真实 Sprite，并从场景 `PackObject/fx_chai_w_001` 的实际世界位置向上冒出；临时碎片使用卡包后方深度，不覆盖包装正面。碎片沿用首页 `86px * 1.4` 的展示基准，并与卡包从 `240x272` 到 `600x680` 使用相同 `2.5` 倍放大关系，最大边约为 `301px`；上冒改为覆盖主要拆包窗口的慢进慢出曲线，不再快速冲出。滑光主可见窗口结束后记录这些碎片最终中心的归一化屏幕坐标，立即隐藏并销毁选中静态卡包内容，不再恢复第二个撕开静态包或执行卡包下落；系统直接激活 GameScene，在同一点创建当前组真实可交互 Piece，棋盘、托盘和 Piece 依次直飞托盘同步开始。完整包不再经过额外 `0.255s` 静止等待，也不会进入 `CardPackGameEntranceTransition`；普通彩色进行中包与灰色重玩包仍等待可见卡包越过碎片后再发牌。
- GameScene 激活后，当前组真实 Piece 先以最终托盘缩放叠放在卡包初始中心；只保留统一的两帧起始姿态稳定，随后卡包下落与棋盘、托盘、返回和提示按钮入场立即在同一帧并行播放。棋盘与按钮原有的 `0.42s` 起步延迟已移除，卡包也不再重复等待额外两帧。碎片仍等待卡包完整越过后再直飞终点。
- 卡包慢速下落分界不再使用固定 `24%`，而是读取卡包 RectTransform 的实际显示高度。卡包完整落到初始碎片区域下方后，首页装饰碎片隐藏，卡包开始加速掉出屏幕；真实 Piece 不再预先散开，而是按托盘顺序从同一个初始位置依次直飞各自终点。
- GameScene Canvas 与 CardBag Prefab 仍只按当前激活场景查找和挂载，避免跨场景卡包 Canvas 销毁时带走棋盘。
- 本次流程的视觉动画继续按基础参数的 `1.5` 倍执行：首页交接 `0.42s`、卡包两段下坠合计 `0.69s`；卡包切入游戏背景后的最短静止等待再次减半，由 `0.51s` 改为 `0.255s`。GameScene 棋盘、托盘、按钮及 Piece 直飞的时长和错峰间隔不变；场景预加载 `5s` 超时、统一两帧稳定和单帧最大推进量不变。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。Unity Play Mode 需重点确认卡包列表与首页背景保持原位并使用完全相同的透明度同步渐隐、虚化截图同步淡出、`BgGame` 原地淡入；同时确认完整彩包第一组碎片从实际撕口上冒并在滑光结束后衔接棋盘/托盘入场，以及灰色重玩和彩色进行中包原流程。

## 2026-08-26 进行中卡包重复进入空白修复

- 状态：代码修复完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 流程验收。
- 最新失败日志确认 `GameScene.Start()`、CardBag Prefab 加载、5 片历史进度恢复、跨场景卡包释放和棋盘/托盘入场均已完整结束；因此本次“卡住”不是流程等待，而是最终画面对象挂载错误。
- 已确认根因：转场期间同时存在 GameScene Canvas 和 `DontDestroyOnLoad` 的卡包 Canvas，但 `GameScene` 原来使用全局 `FindObjectOfType<Canvas>()`。运行时可能把临时卡包 Canvas 当作游戏 Canvas 配置，并将 `CardBag002` 实例挂在临时 Canvas 下；转场释放该 Canvas 时棋盘随之销毁，真正的 GameScene Canvas 又保持场景序列化的零缩放，最终表现为空白。
- `ConfigureGameplayCanvas` 和 `EnsureCardBagLoaded` 现在都只通过当前激活场景的根对象 `Canvas` 获取 GameScene Canvas。找不到时直接报错并停止错误挂载，不再回退到任意全局 Canvas。
- `CardPackGameEntranceTransition` 现在只保存跨场景卡包、碎片、材质和位置信息，不再通过自身 `StartCoroutine` 决定流程推进。GameScene 完成初始化并绑定自己的 Camera 后，直接在 GameScene 的入场协程中执行该转场枚举器；即使 Canvas 曾被禁用，也会先恢复激活，再稳定两帧、完成原 `0.46s` 卡包下收和碎片分开，然后立即继续棋盘、托盘和发牌入场。
- 已移除上一版依赖 Canvas 自身协程和 `2s` 超时解锁的方案；MainScene 启动时仍清理残留实例，避免返回首页后的旧状态影响下一次进入。
- 转场枚举器由 GameScene 逐帧显式 `MoveNext`，完成边界为动画时长加 `1s` 宽限；超时或任一已卸载 UI 引用抛异常时，立即设置最终位置、记录发牌点并释放 Canvas，不能继续阻断玩法。`prepared -> camera bound -> playback started -> released -> board/tray entrance -> entrance completed` 均有阶段日志，后续可按实际最后一条日志精确定位。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需退出当前 Play Mode 等 Unity 完成脚本重编译，再次从带碎片的撕开卡包点击“玩”，确认 GameScene 棋盘、托盘、已恢复拼图和当前组碎片正常显示。

## 2026-08-26 Loading 与入场动画卡顿优化

- 状态：实现完成并通过 Runtime/Editor 编译，等待 Unity Play Mode 性能与视觉验收。
- Unity `Editor.log` 显示 MainScene/GameScene 的 Scene Integration 各约 `142~150ms`，`GameScene.Start()` 同步初始化约 `83~145ms`，场景切换后还会卸载约 `2500~2700` 个资源；此外 MainScene 原先在首帧同步创建 22 个卡包，并逐张执行 `File.ReadAllBytes + Texture2D.LoadImage`。这些工作集中在主线程单帧时会形成约 `150~300ms` 的停顿，不是调动画曲线能够消除的问题。
- LoadingScene 现在从开始阶段异步加载 MainScene，并把场景保持在 `90%` 待激活状态；卡包封面和尺寸图通过 `UnityWebRequestTexture` 在 Loading 期间异步读取/解码并写入静态 Sprite 缓存，Loading 至少播放 `2.5s`，且只有场景和列表图片均准备好后才显示 `100%` 并激活首页。
- MainScene 列表复用预热后的封面和尺寸 Sprite，不再对每个列表项重复从磁盘读取图片；22 个卡包按每帧最多 4 个分批创建，避免首帧一次完成全部 Instantiate、绑定和布局。选中卡包 Animator 改为在视觉节点激活后同步，消除了对 inactive Animator 调用 `Play/Update` 的警告。
- 撕开彩色进行中卡包和灰色重玩卡包进入 GameScene 时，选中卡包 Canvas 会临时 `DontDestroyOnLoad`。GameScene 的激活、反序列化、资源卸载和同步初始化发生在卡包居中静止期间；GameScene 初始化完成并稳定两帧后，跨场景卡包才按原 `0.46s` 参数下收、碎片分开并记录实际发牌起点，随后立即启动现有发牌动画。撕口蒙版 Material 和 Texture 在移交时独立复制，避免 MainScene 卸载资源导致转场卡包丢材质。
- 修改文件：`Assets/Scripts/Controller/LoadingScene.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Assets/Scripts/Model/CardPackRewardFlyTransition.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需从 LoadingScene 冷启动验收 Loading 文本/动画连续性、首页列表出现节奏，并分别用彩色撕开进行中卡包和灰色撕开重玩卡包验收场景交接、撕口材质、卡包下收与发牌是否连续。

## 2026-08-26 撕开卡包进入游戏转场

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene 到 GameScene 的完整视觉验收。
- 参考视频 `ad09e2d6b7628fc6e994704091f1f54e_raw.mp4` 为 `1170 x 2532`、约 `6.75s`、约 `59fps`。关键节奏是：选择 UI 与虚化背景先退场，只保留展开撕开卡包；卡包短暂停顿后向下收走，装饰碎片相对撕口向上脱出；游戏托盘先出现，当前组碎片从卡包碎片最后位置向上扇形散开；棋盘随后从右侧滑入，碎片最后落入托盘。
- 彩色撕开进行中卡包点击“玩”后不播放完整拆包特效。灰色撕开已完成卡包仍先显示重玩确认，确认后清除旧进度并创建空会话，再从对应 CardBag 临时选择本关碎片补入灰色卡包；两种撕开状态随后共用同一个转场协程，灰色卡包不会被切回彩色样式。
- MainScene 在 `0.22s` 内退掉选择 UI 并恢复 `BgGame`，居中停留 `0.68s`，再用 `0.46s` 将卡包向下收走；`ProgressPieces` 在此期间反向补偿位移并横向散开，形成从撕口脱出的连续动作。转场同时低优先级预加载当前 CardBag 与 GameScene。
- MainScene 在卡包退场结束时读取选中碎片实际屏幕位置的平均值并传给 GameScene，不再用固定坐标猜测发射点。GameScene 让当前组碎片从该位置先向上扇形发出，再落到已计算好的托盘位置；托盘先从下方快速进入，棋盘延迟后从右侧滑入，返回和提示按钮与棋盘阶段同步显示。
- 修复 MainScene 分开碎片后与 GameScene 发牌之间的明显停顿：撕开卡包入口现在向 GameScene 传递“碎片已扇出”状态，GameScene 首帧直接在扇出位置显示当前组碎片，不再重复执行 `0.3s` 扇出和后续 `0.3s` 等待；保留两帧起始姿态预热，但碎片在预热期间可见，并在约 `0.04s` 后直接错峰落入托盘。
- MainScene 在最短 `0.68s` 卡包停顿期间等待 GameScene 预加载达到 `90%` 可激活状态，最长等待 `5s`；加载等待不再出现在卡包下收和碎片分开之后。请求场景激活前不主动隐藏上一层碎片，由场景卸载自然清理，避免交接帧出现空画面。
- 入场期间 Piece 始终保持最终 `TrayScale`，不临时放大；目标位置、托盘顺序、光点创建、两帧起始姿态预热、单帧最大 `1/30s` 时间推进和交互锁定规则保持不变。完整彩色卡包的拆包流程不变；灰色完成态保留重玩确认和进度重置，但确认后的视觉转场与彩色撕开带碎片状态一致。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：FFmpeg 逐帧联系表确认参考视频的卡包下收、碎片发出和棋盘右侧进入顺序；`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。仍需在 Unity Play Mode 分别使用彩色撕开进行中卡包和灰色撕开已完成卡包验收背景交接、卡包颜色、退场距离、碎片分开到发牌的连续性、落牌范围和整体节奏。

## 2026-08-26 进行中卡包碎片显示恢复

- 状态：代码修复完成并通过 Runtime/Editor 编译，等待 MainScene 视觉验收。
- 第一组完成判定、彩色撕开状态和运行时碎片创建逻辑仍然存在；问题来自列表改为统一缩放 `PackNode` 后，动态创建的 `ProgressPieces` 仍使用旧列表像素尺寸，随后又被父节点缩小到 `0.4`，视觉上接近消失。
- MainScene 现在根据 `PackCover` 当前设计尺寸与列表目标 `240 x 272` 计算动态碎片的设计空间换算比例。当前封面为 `600 x 680`，换算比例为 `2.5`；碎片尺寸、位置、横向边界、阴影偏移和浮动距离统一乘以该比例，再随 `PackNode` 缩放到列表尺寸。
- 选中卡包继续复制完整 `PackNode`，因此进行中的撕口、`PackBg` 和碎片会一起从列表状态放大，不单独重建或丢失碎片。选中克隆里的 `ProgressPieceXX` 会复用列表碎片的基准位置、浮动距离和动画相位，并接入同一个逐帧动画更新，放大移动和中央停留期间不再静止；关闭选中页时同步清理动画引用。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误。仍需在 MainScene Play Mode 验收完成第一组后的彩色撕开带碎片状态，以及点击后选中层的碎片尺寸、位置和持续浮动动画。

## 2026-08-26 卡包选中视觉与流程按列表状态分流

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene 三态视觉/流程验收。
- 列表项统一记录 `完整彩色`、`彩色撕开进行中`、`灰色撕开已完成` 三种显示状态；选中层不再单独创建封面 Image，而是复制当前列表实例的完整 `PackNode`，保留撕口、`PackBg`、进行中碎片、封面、尺寸标签、状态材质和 Animator 状态，再由外层从列表尺寸统一放大到 `600 x 680`。
- 完整彩色点击“玩”继续进入现有拆包舞台并播放完整拆包流程；彩色撕开进行中点击“玩”跳过拆包舞台和特效，直接进入 GameScene 并播放棋盘、托盘和碎片入场；灰色撕开已完成点击“重玩”显示 `PanelReplay`，确认后清除并重建空会话，再直接进入 GameScene，不播放拆包动画。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。工作期间外部提交 `25e6b10` 已包含此前的 `PackItem.prefab` 完成态尺寸材质引用与父节点缩放修改，本任务在该最新提交上继续实现。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；旧的选中封面 Sprite 赋值和统一进入拆包舞台逻辑已移除，直接进入 GameScene 前仍记录选中卡包下沿并启用现有发碎片入场。仍需在 MainScene Play Mode 分别验收三种列表/选中视觉一致性、按钮文字、重玩确认和拆包跳过行为。

## 2026-08-26 首页卡包整体父节点缩放

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene 视觉验收。
- 美术资源更新时清空了 `PackItem.prefab` 的 `Completed Size Material` 和 `PackSize/Image` 编辑器预览材质引用，导致代码虽然执行状态切换，但完成态得到空材质。现已重新绑定 `PackSizeCompleted.mat`；普通态继续使用默认 UI 材质，标签 RectTransform 保持美术配置不变。
- 首页列表不再分别修改 `PackCover`、`PackBg` 或 `PackSize` 子节点的 RectTransform。MainScene 根据美术 `PackCover` 原始尺寸与列表目标 `240 x 272` 计算统一比例，并只缩放共同父节点 `PackNode`。
- 当前美术封面为 `600 x 680`，因此列表 `PackNode.localScale` 为 `0.4`。`PackSize` 的美术尺寸、位置、锚点和 Pivot 均保持 Prefab 原值，并随父节点同步缩小和移动；其他卡包子视觉也使用同一比例。
- `PackAniBreath` 没有 Scale 曲线，不会覆盖程序设置的父节点列表缩放。
- 修改文件：`Assets/Prefabs/PackItem.prefab`、`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；`PackItem.prefab` 的完成态配置和 `PackSize/Image` 编辑器预览均已引用 `PackSizeCompleted.mat` 的有效 GUID；代码中已无对 `PackSize` RectTransform 的缩放或定位写入。仍需在 MainScene Play Mode 检查普通、进行中和已完成三种尺寸标签状态。

## 2026-08-26 卡包尺寸标签使用美术 Prefab 布局

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene 视觉验收。
- 美术已更新卡包尺寸图片及 `PackItem.prefab/PackSize` 的尺寸、位置、锚点和 Pivot。MainScene 不再直接修改 `PackSize` 的 RectTransform；列表实例沿用 Prefab 配置并随共同父节点 `PackNode` 整体缩放。
- 保留按 `CardPacks.csv/PackSize` 动态替换对应 `PackSize_1~7.png` 的逻辑，以及普通/完成态材质选择；这些逻辑不修改尺寸和位置。
- 已删除 `PackageSizeListVisualScale`、`ScalePackageSizeWithCover` 及创建列表项时的调用。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；代码中已无 `PackSize` RectTransform 缩放或定位写入；`git diff --check` 通过。仍需在 MainScene Play Mode 确认各尺寸图片完全按最新 Prefab 布局显示。

## 2026-08-25 首页卡包尺寸标签等比缩放

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene 视觉验收。
- `PackItem.prefab/PackSize` 继续按卡包展开态 `600 x 680` 制作，不修改美术原始 RectTransform。
- MainScene 创建列表项时，先使用封面从展开态到列表态的统一比例计算布局位置，再对 `PackSize` 额外应用 `0.75` 的列表视觉缩放；全程保持宽高同倍率，不分别拉伸 X/Y。缩放后根据标签实际宽度与 Pivot 显式计算横向位置，使标签左边缘始终对齐列表卡包左边缘，不依赖 Prefab 的横坐标碰巧对齐。
- 当前卡包比例为 `0.4`，标签最终比例为 `0.4 x 0.75 = 0.3`；`PackSize` 从 `274 x 158` 显示为约 `82.2 x 47.4`，左边缘固定为列表卡包的 `x=-120`。纵向位置仍按卡包 `0.4` 比例定位，避免标签二次缩小时向卡包中心漂移。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；待在 MainScene Play Mode 检查不同尺寸标签的等比显示和左边缘对齐。

## 2026-08-25 卡包尺寸标签完成态材质

- 状态：实现完成并通过 Runtime/Editor 编译，等待 Unity 资源导入与 MainScene 视觉验收。
- 新增 `Assets/Resources/PackSizeCompleted.mat` 和专用 `PackSizeState.shader`。材质只处理尺寸标签本身，不包含卡包封面的撕口、投影或蒙版逻辑；美术可调整 `Tint`、`Grayscale Amount`、`Grayscale Color`、`Brightness` 和 `Contrast`。
- `PackCoverVisualSettings` 新增 `PackSize`、普通尺寸材质和完成态尺寸材质配置，并让 `Preview Completed In Editor` 同时预览封面与尺寸标签完成态。`PackItem.prefab` 已绑定新材质，普通材质留空表示使用 Unity 默认 UI 材质。
- MainScene 仅在卡包历史状态为 `Completed` 且没有活动拼图会话时切换 `PackSizeCompleted`；未完成、进行中和重玩中的卡包继续显示彩色尺寸标签。
- 修改文件：`Assets/Resources/PackSizeState.shader`、`Assets/Resources/PackSizeCompleted.mat`、`Assets/Scripts/View/PackCoverVisualSettings.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Prefabs/PackItem.prefab`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；材质、Shader 和 Prefab GUID 引用唯一且完整；`git diff --check` 通过。仍需回到 Unity 触发资源刷新，确认 Console 无 Shader error，并在 `PackItem.prefab` 和 MainScene 检查彩色/完成态切换效果。

## 2026-08-25 卡包进度与撕开状态

- 状态：实现完成并通过 Runtime/Editor 编译，等待 Play Mode 验收。
- 已完成卡包确认重玩时，MainScene 清除上一局会话并创建新的空会话，确保本次重玩从空棋盘开始。
- 首次游玩和重玩使用相同的持续记录规则：每片正确拼入后立即保存 Piece 编号，第一组无论完成多少片、后续任意组完成多少片，返回首页时都保留会话；再次进入时恢复全部已拼 Piece，并从当前进度继续。
- 首页撕开表现与是否存在进度分离。MainScene 按 Prefab 的实际 `Piece01II` 清单判定第一组：第一组未全部完成时显示完整彩色卡包；第一组完成后显示彩色撕开卡包和本关碎片；整包完成且没有活动会话时显示灰色撕开完成态。
- 第一组完成判定统一使用 `CardPackDataUtility.HasCompletedFirstPuzzleGroup`。
- 修改文件：`Assets/Scripts/Model/CardPackDataUtility.cs`、`Assets/Scripts/Controller/MainScene.cs`、`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需 Play Mode 分别验证重玩第一组部分完成返回后的完整卡包和进度恢复、第一组全部完成返回后的彩色撕开状态，以及后续组进度恢复。

## 2026-08-25 第一组完成后显示撕开状态

- 状态：实现完成并通过 Runtime/Editor 编译与全 CardBag 资源检查，等待 MainScene Play Mode 验收。
- 活动拼图会话不再直接代表卡包已撕开。MainScene 会读取当前会话已正确拼入的 Piece，并扫描对应 `CardBagNNN.prefab` 的全部 `Piece01II`；只有第一组全部完成，才显示彩色撕口、`PackBg` 和最多 3 片本关装饰碎片。
- 新进入游戏但尚未完成第一组、第一组只完成部分，以及已完成卡包开始重玩但当前第一组尚未完成时，首页均显示完整彩色卡包；完成且没有活动会话的卡包继续显示完成态撕开卡包。
- 判定使用 Prefab 实际第一组清单，不写死第一组片数；缺少 Prefab 或 `Piece01II` 时保持完整状态并输出警告。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；23 个 CardBag Prefab 均存在合法 `Piece01II`，第一组片数范围为 `1~14`；`git diff --check` 通过。仍需 MainScene Play Mode 验证第一组完成前、完成后、继续游戏、重玩和整包完成状态。

## 2026-08-25 撕包页面卡包范围内滑动

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene Play Mode 验收。
- 等待撕包页面继续支持卡包范围内点击；同时允许从卡包矩形内起手并在矩形内向任意方向滑动。移动达到至少 `18` 屏幕像素或卡包短边 `6%` 时只记录为有效滑动，必须等鼠标左键或触摸抬起后，才触发现有撕包动画与后续进入游戏流程。
- 滑动过程中离开卡包矩形会取消本次手势，卡包外起手不会触发；未抬起前不会停止循环滑光或播放撕包动画。点击与滑动在抬起时共用现有单次完成入口，不会重复启动动画。
- 移除了旧的撕口窄带、左侧起手、仅向右移动半个卡包宽度及纵向偏移限制；撕包资源、动画和转场时序保持不变。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；旧窄带和定向横划常量、方法已无残留，`git diff --check` 通过。仍需在 MainScene Play Mode 分别验证点击、横向滑动、纵向滑动、卡包外起手和滑出卡包五种输入。

## 2026-08-25 撕开卡包背景与碎片层级

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene Play Mode 视觉验收。
- 所有撕开状态，包括存在活动拼图会话的彩色撕开卡包和已完成无活动会话的完成态撕开卡包，都会显示 `PackItem/PackNode/PackBg`；未撕开的普通卡包保持隐藏。
- `PackBg` 沿用 Prefab 中的 `Bg01.png`、尺寸、颜色和材质，运行时只按封面从 `600x680` 到列表 `240x272` 的比例同步缩放，并关闭 Raycast，不覆盖美术参数。
- 层级固定为 `PackBg < ProgressPieces < PackCover`。没有进行中碎片时仍保持 `PackBg < PackCover`；列表滚出可见区域、打开遮挡面板或选中卡包时，背景与该卡包其他列表视觉同步隐藏。
- 修改文件：`Assets/Scripts/Controller/MainScene.cs`、`Assets/Prefabs/PackItem.prefab`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`、`specs/spec-driven-development.md`。
- 验证：`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 编译通过，均为 `0` 警告、`0` 错误；`git diff --check` 通过。仍需在 MainScene Play Mode 检查普通、进行中和完成三种状态以及 `PackBg < ProgressPieces < PackCover` 的实际视觉层级。

## 2026-08-25 进行中卡包碎片放大与浮动

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene Play Mode 视觉验收。
- 首页存在活动拼图会话的卡包继续在封面后方显示最多 3 片本关碎片；每片显示尺寸改为原来的 `140%`。
- 放大后按新增高度的一半向下修正碎片中心，使原来的顶部露出位置基本保持不变，新增尺寸主要向卡包内部延伸；倾角、封面层级和阴影不变。横向位置按旋转后的实际包围宽度限制在卡包左右边界内，避免左侧或右侧漏出。
- 每片以 `6` 设计像素振幅、与 `PackAniBreath` 相同的 `6s` 周期持续上下往返浮动，并按 PackId 和碎片序号使用稳定错峰相位；动画使用 `unscaledTime`，不累计修改基准位置。
- 进行中碎片继续沿用列表可见区域、面板遮挡、卡包选中与返回时的现有显隐规则，不会出现在新卡包、已完成无活动会话卡包或居中放大页。
- 验证：Runtime/Editor C# 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 MainScene Play Mode 验收碎片尺寸、露出范围、浮动节奏和多卡包错峰效果。

## 2026-08-25 选中卡包背景半径 8 高斯模糊

- 状态：实现完成并通过 Runtime/Editor 编译与 Shader 静态检查；Unity 当前尚未刷新新资源，等待编辑器导入和 MainScene Play Mode 视觉验收。
- 参考图明确为 Photoshop 高斯模糊半径 `8px` 的直接效果。将原有三级降采样/回放大的近似模糊替换为原分辨率横向、纵向两遍可分离高斯模糊；Shader 使用线性采样合并完整 17 点高斯核，每个方向实际执行中心和正负 4 组共 9 次采样，覆盖正负 `8px`。
- 新增 `Assets/Resources/BagSelectGaussianBlur.shader`，运行时按需加载并创建临时 Material；MainScene 销毁时释放 Material。Shader 缺失或平台不支持时只回退到未虚化的原分辨率背景，不阻断选择页。
- 高斯模糊结果按原色、全不透明显示，不叠加黑色或白色蒙版；前景排序、卡包放大、按钮和转场逻辑不变。
- 修正 Linear 色彩空间下 `CaptureScreenshotAsTexture` 的屏幕 sRGB 数据在 RenderTexture 链路中被再次 Gamma 编码而导致的偏白：只在纵向高斯通道最终输出时恢复为 Linear，保证 RawImage 再次显示后与模糊前首页的亮度、饱和度一致。
- 验证：Runtime/Editor C# 编译通过，`0` 警告、`0` 错误；17 点高斯核归一化权重和为 `1`，Shader GUID 唯一，`git diff --check` 通过。Unity 当前 AssetDatabase 时间早于新 Shader 文件，需回到编辑器触发导入并确认 Console 无 Shader error，再在 Play Mode 对比参考图的 PS 高斯模糊半径 `8` 观感和性能。

## 2026-08-25 首页卡包呼吸动画错峰

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene Play Mode 视觉验收。
- 首页列表卡包继续共用美术 `PackAniBreath` 曲线和既有速度：彩色 `1`、完成态灰色 `1/3`；不随机修改速度、周期或动画幅度。
- 每个卡包根据 PackId 与黄金分割步长 `0.61803398875` 计算稳定的归一化起始相位。相邻 PackId 的呼吸姿态自然分散，不再同一帧同步起伏；同一卡包刷新或跨设备运行时保持相同相位，不会随机跳变。
- 等待撕包页继续显式播放 `PackAni` 并从归一化时间 `0` 开始，不使用列表呼吸相位。
- 验证：Runtime/Editor C# 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 MainScene Play Mode 确认同屏卡包错落感和灰色慢速卡包观感。

## 2026-08-25 选中卡包背景无染色高斯模糊

- 状态：实现完成并通过 Runtime/Editor 编译，等待 MainScene Play Mode 视觉验收。
- 参考效果要求背景保留原本颜色，只呈现柔和虚化，不得形成明显灰层或白雾；前景选中卡包、按钮和交互流程不变。
- `PanelBagSelect` 根 Image 实际引用黑色 `ImgMaskBlack.png`。原 `0.34` Alpha 过灰，继续降至 `0.12/0.06` 仍会产生可见灰感，因此最终在场景和运行时完全关闭该黑色视觉层；Image 继续保留 Raycast 拦截能力。
- 运行时高斯模糊截图使用原色、全不透明显示，不与下方未模糊画面做透明混合。
- 进入开包舞台时，虚化截图按原转场进度从全不透明平滑淡出；高斯模糊半径、前景排序和选中卡包动画均未修改。
- 验证：`PanelBagSelect` 场景颜色与运行时代码一致；Runtime/Editor C# 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 与参考图对比背景亮度、颜色、模糊程度和转场衔接。

## 2026-08-25 首页卡包美术呼吸动画

- 状态：美术新版呼吸曲线、Controller 与运行时速度分流已合并，Runtime/Editor 编译通过；等待 MainScene Play Mode 视觉验收。
- `PackNode.controller` 新增 `PackAniBreath` 状态并设为默认状态，Motion 引用美术提供的 `Assets/Animation/PackAniBreath.anim`；原 `PackAni` 状态继续保留给等待撕包页的循环滑光提示。
- MainScene 创建卡包列表项时缓存 `PackNode` Animator。彩色未完成卡包和存在活动重玩会话的彩色卡包使用正常速度 `1`；只有 `Completed` 且无活动会话、实际显示完成态灰色材质的卡包使用 `1/3`，即动画节奏放慢 3 倍。
- 等待撕包页克隆 `PackNode` 后显式恢复 Animator 速度 `1` 并播放 `PackAni`，不会继承灰色列表卡包的慢速设置。
- 美术在 `develop` 的 `7c90569` 提交中重新保存了 `PackAniBreath.anim`，现在包含完整的 6 秒根节点位置与旋转循环曲线。合并冲突来自双方创建的同名 Animator 状态使用了不同内部 fileID；最终保留美术生成的状态 `5785946119623635755`，删除重复状态，并将其设为 Controller 默认状态。程序不写呼吸曲线、幅度或美术 Transform。
- 验证：美术 Clip 曲线、Controller 唯一状态、Clip GUID 与 Prefab Controller GUID 引用一致；合并冲突标记已清除；`Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj` 均编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。尚未进行 Play Mode 视觉验收。

## 2026-08-25 等待撕包页循环滑光提示

- 状态：代码接入完成并通过编译，等待 MainScene Play Mode 视觉验收。
- 点击“玩”或确认重玩并完成 `BgGame` 开包舞台转场后，从 `PackItem.prefab` 克隆 `PackNode` 到当前 `SelectedCardPackImage` 下；克隆层关闭 `PackCover` 与 `PackSize`，只启用 `ImgLight` 并播放现有循环状态 `PackAni`。
- 提示层以 Prefab 中 `PackCover` 的 `600 x 680` 为基准，按当前居中静态卡包的实际 Rect 等比缩放，因此继续使用美术在 `PackItem` 中配置的 ImgLight Sprite、位置、尺寸和动画曲线。
- 只有现有输入逻辑确认有效轻点或横划达到开包门槛时，才立即隐藏并销毁循环滑光，再进入现有 `PlaySelectedPackage` 撕包流程；无效点击和未达门槛的滑动不会停止提示。
- 清除卡包选择、重新进入开包舞台或销毁 MainScene 时统一释放提示层，避免返回首页后残留。
- `PackAni.anim`、`PackNode.controller`、轻点/横划阈值、3D 撕包模型、`fx_chai_w_001` 和切场景时序均未修改；`PackItem/ImgLight` 保持 Prefab 默认隐藏，只在等待撕包页的运行时克隆中启用。
- 验证：Runtime/Editor C# 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Play Mode 确认滑光位置、循环衔接、有效输入瞬间停止以及撕包动画正常开始。

## 2026-08-25 首页卡包撕口与完成态美术配置

- 状态：三种卡包状态逻辑、进行中贴纸展示及完成态美术材质入口已完成并通过编译，等待 Unity 刷新与 Play Mode 视觉验收。
- 用户将 `PackMask01~06.png` 从 `美术切图/卡包/撕开遮罩` 移入 `Assets/UI/PackImages`；这些蒙版尺寸为 `600x680`，透明区域表示撕掉，不透明区域表示保留，边界 Alpha 提供抗锯齿。
- 首页状态优先级以活动会话为最高：没玩过且没有活动会话的卡包显示完整彩色封面；只要存在 `CardPackPuzzleProgress`，无论生命周期是否已经是 `Completed`，都显示随机撕口彩色封面，并从对应 `CardBagNNN.prefab` 中展示最多 3 片本关贴纸；只有已完成且当前没有活动会话的卡包显示随机撕口并置灰。
- `PackItem` 根节点新增 `PackCoverVisualSettings`，引用 `PackCover`、正常材质 `PackCoverShadow.mat` 和完成态材质 `PackCoverCompleted.mat`。美术可用 `Preview Completed In Editor` 预览完成态，并直接在完成态材质中调整 `Grayscale Amount`、`Grayscale Color`、`Use Gray Mask` 和 `Gray Mask`；白色蒙版区域生效、黑色区域保持原色。
- `PackCoverShadow.shader` 使用可选 Alpha 蒙版同时裁剪封面和投影，并提供美术可调的真实灰度颜色与可选灰色蒙版；灰色蒙版使用独立 Shader 关键字，正常材质不会增加蒙版纹理采样。默认关闭撕口与灰度，因此不影响 GameScene 共用该 Shader 的投影材质。
- 完成且未重玩的卡包列表 `PackCover` 使用完成态材质；程序不再写灰度参数，也不再为 `PackSize` 创建或切换置灰材质。撕口只裁剪 `PackCover`。进行中贴纸创建在 `PackCover` 后方，优先选择会话中尚未拼上的 Piece，不足 3 片时再用本关其他 Piece 补齐，并随列表可视性、选中隐藏和面板遮挡统一显隐。三片贴纸中心高度为 `68 / 82 / 70`，保留参差高度和不同倾角，同时限制在列表单元顶部以内，并使用完整矩形 Image 网格避免 Sprite Mesh 裁切。点击后放大的完整封面、选择页及开包流程继续显示完整彩色封面。
- 每次进入或刷新首页时，每个需撕开的卡包随机选择一种蒙版，同一次首页停留期间不变化。正常与完成态分别从美术材质模板克隆撕口运行时材质；程序只写 `_TornMaskTex` 和 `_UseTornMask`，场景退出时统一释放运行时材质与蒙版资源。
- 验证：6 张蒙版均为 `600x680`，顶部 Alpha 为 `0`、主体 Alpha 为 `255`；使用 `CardBag001` 的 3 张实际 Piece 与撕口蒙版离线合成，确认贴纸从撕口后方露出且封面可遮挡下半部分；Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；Prefab GUID 引用和 `git diff --check` 通过。Unity 当前占用工程且未自动刷新，仍需回到编辑器刷新后确认 Shader 无报错，并在 MainScene 目视验收正常、进行中和完成三种状态。
- 首轮失败原因：错误使用 `LifecycleState == InProgress` 判断。完成卡包重玩中途退出时生命周期仍为 `Completed`，但会保留活动拼图会话；最终规则分别读取完成状态与活动会话，并让活动会话拥有最高显示优先级。

## 2026-08-24 移除首页卡包程序呼吸动画

- 状态：代码与 Prefab 设置已清理并通过静态检查及编译，等待 Unity Play Mode 验收。
- `PackageInteractionHandler` 保留点击、滑动和 ScrollRect 事件转发，删除 `[ExecuteAlways]`、呼吸缩放参数、编辑器预览、逐帧缩放及禁用时重置缩放逻辑。
- `MainScene` 删除仅为呼吸显隐服务的 Handler 缓存与 `SetBreathing` 调用；卡包正常显隐逻辑不变。
- `PackItem.prefab` 删除旧呼吸参数序列化数据，并把被编辑器呼吸预览停留在 `1.0166308` 的 `CardPackEffect.localScale` 恢复为 `1`；`CardPackEffect/PackNode`、空 Animator、封面、尺寸图标及 `ImgLight` 保留，供美术后续直接在 Prefab/Animator 中制作动效。
- 验证：旧呼吸字段、方法和序列化参数无残留；`PackAni.anim` 与 `PackNode.controller` 无缩放曲线；Prefab 本地 `fileID` 完整；Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需 Unity Play Mode 确认列表不再缩放且点击、滑动、显隐和进入选中页正常。

## 2026-08-24 删除 PackHighlight 残留

- 状态：Prefab、代码和专用贴片已清理并通过静态检查及编译，等待 Unity Play Mode 验收。
- 从 `Assets/Prefabs/PackItem.prefab` 删除 `PackHighlight` 与 `PackHighlight02~05`，保留 `CardPackEffect/PackNode/PackCover|PackSize`、直属 `ImgLight` 和空 Animator；程序呼吸逻辑已在后续任务中删除。
- 从 `MainScene` 删除旧节点查找、`PackageEntry.HighlightRoot`、显隐控制和列表尺寸适配逻辑。
- 删除仅被该节点引用的 `Assets/UI/MainScene/PackHighlight02~05.png` 及 `.meta`。
- 保留 `Assets/Resources/PackHighlightAdditive.mat/.shader`，因为 GameScene 的 `PieceLight1~4` 拼图高光点仍在使用。
- 验证：`PackItem.prefab` 本地 `fileID` 引用完整；Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Unity 刷新后确认首页封面、尺寸图标和 `ImgLight` 正常，Console 无 Missing Reference。

## 2026-08-24 仓库进度同步

- 当前分支：`develop`；与 `origin/develop` 完全同步，领先/落后均为 `0`，工作区在同步前无未提交修改。
- 当前 HEAD：`2bf3ade 呼吸动画层级+空动画对象`。
- 已同步外部提交：`168b289 左右箭头+划开气泡`、`af84c24 添加气泡`、`5e2a90b 美术前置资源补全`、`2bf3ade 呼吸动画层级+空动画对象`。
- `PackItem.prefab` 当前在 `CardPackEffect/PackNode` 下放置 `PackCover` 与 `PackSize`，`PackNode` 绑定循环空动画 `PackAni` 的 `PackNode.controller`；`ImgLight` 是 `PackItem` 直属子节点；旧 `PackHighlight` 与程序呼吸缩放均已在后续任务中删除。
- 美术资源新增/调整包含 `ImgBagLight.png`、`Back.png`、`BlackBg.png`、左右翻页素材、气泡素材、抖动提示音和卡包状态参考图；MainScene、GameScene、字体资源及 PackItem Prefab 已有对应提交调整。
- 程序侧已提交并保留：单块托盘提醒、取消拿起圆点投影、组合回托盘恢复 `04` 投影、自身凹槽相交允许临时放置、异形 Piece 高光点内部安全定位。
- 本轮只同步记录，没有修改运行时代码或美术配置。Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需在 Unity Play Mode 集中验收以下各项。

## 2026-08-23 异形 Piece 高光点内缩

- 状态：代码修改并通过编译，等待 Unity Play Mode 验收。
- 高光点不再使用 Physics Shape 左上边界顶点和固定左上夹值；改为在 Piece Physics Shape 内部采样。
- 优先选择距离轮廓边缘至少达到本 Piece 最大内部余量 `72%` 的候选，再从中选择最接近原左上视觉区域的位置；异形或狭长 Piece 会回退到自身最深内部区域。
- 取消位置随机偏移，保证选定安全点不会再次被推向透明边缘；高光样式、尺寸、旋转、ADD 材质、裁切和落位动画不变。
- 验证：Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。

## 2026-08-23 自身凹槽相交允许临时放置

- 状态：代码修改并通过编译，等待 Unity Play Mode 验收。
- Piece 未达到正确吸附条件时，与自身凹槽相交不再触发错误回弹，允许作为未完成 Piece 留在当前位置。
- 正确吸附仍优先执行；自身凹槽相交例外不放宽已拼区域、其他外部 Piece、棋盘边界或托盘区域的限制。未与自身凹槽相交时，其他未填凹槽边界继续沿用原判定。
- 单块和临时组合共用该规则。
- 验证：Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。

## 2026-08-23 组合回托盘投影恢复

- 状态：代码修改并通过编译，等待 Unity Play Mode 验收。
- 修复临时组合进入托盘并拆散后，成员仍保留桌面投影或组合成员无独立投影材质的问题。
- 单块回托盘、整组主动回托盘及被正确 Piece 顶回托盘统一恢复 `IngameCoverShadow04`。
- 验证：Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。

## 2026-08-23 取消拿起 Piece 圆点投影

- 状态：代码和专用资源已移除，Runtime/Editor C# 编译通过。
- 拿起单块或临时组合时不再创建、跟随或显示 `PieceShadow.png` 圆点投影。
- Piece 自身材质投影、临时组合整体投影和落位光效保持不变。
- 验证：Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。仍需 Unity Play Mode 确认拖拽时不再显示圆点且其他投影表现不变。

## 2026-08-23 托盘收起后的单块提醒

- 状态：代码修改完成，等待 Unity Play Mode 验收。
- 规则：托盘收起后，仅桌面或棋盘上严格剩余 `1` 块未正确放置的 Piece 时周期抖动；`0` 块或大于 `1` 块不抖动。
- 临时组合按成员 Piece 数统计，两块及以上组合不触发提醒。
- 数量或其他运行条件变化时立即停止当前抖动并重置提醒计时。
- 修改文件：`Assets/Scripts/Controller/GameScene.cs`、`Documents/CURRENT_TASK.md`、`Documents/PROJECT_CONTEXT.md`。
- 验证：Runtime/Editor C# 编译均通过，`0` 警告、`0` 错误；`git diff --check` 通过。Play Mode 仍需验证 1 块、2 块及数量动态变化场景。

- 任务：实现桌面临时拼图组合
- 状态：组合整体投影与整体提示抖动已完成并通过编译，等待 Unity Play Mode 验收
- 更新时间：2026-08-23

## 用户意图

- 未正确拼入凹槽的 Piece 可以继续放在棋盘合法空位或棋盘左右桌面。
- 第二块 Piece 若与桌面 Piece 在最终棋盘布局中真实相邻，靠近正确相对位置松手时自动吸附成临时组合。
- 单块可以加入组合，两个组合也可以继续合并；拖动任意成员时整组跟随指针。
- 组合正确靠近棋盘后整体拼入各自凹槽；非法放置时整组变红并回弹到本次拖拽前位置。
- 组合进入托盘回收区后自动拆散，并按当前托盘随机顺序重新排列。
- 托盘为空后，提示按钮提示最早形成的桌面组合；组合成员一起抖动，棋盘只显示组合凹槽 Alpha 并集的滚动虚线外轮廓，不显示内部接缝。
- 临时组合不再由每块 Piece 分别显示投影；组合需要按一块完整 Piece 的轮廓统一显示投影，提示抖动也必须围绕组合共同中心整体执行。
- 修复 `CardBag018` 托盘横向滑动后，从屏幕外带入的 Piece 拼错回弹时整排自动重置到最左侧、被选 Piece 落回错误槽位的问题。

## 工作记录

- 在 `GameScene` 内新增仅当前场景生命周期有效的 `LoosePieceCluster` 状态，不修改 Prefab、CSV、SQLite 或已保存拼图进度。
- 相邻关系使用两个 Piece 对应 Groove 在最终棋盘坐标下的 `GrooveProbeCollider.Distance` 判断，不按桌面当前矩形或 Piece 编号猜测。
- 桌面吸附目标使用 `stationaryCurrent + movingGrooveTarget - stationaryGrooveTarget`，确保临时组合内部采用最终棋盘的真实相对布局。
- 拿起组合任意成员时缓存全部成员起点，统一计算鼠标位移，并用组合渲染边界并集限制在桌面可视范围内。
- 松手优先级调整为：托盘回收、整组正确入槽、桌面组合吸附、整组自由放置校验、整组错误回弹。
- 整组正确入槽时逐块记录正式 Piece 编号并复用现有吸附、落位光效、切组和结算流程；目标区域若被另一桌面组合占用，该组合整体拆散并回托盘。
- 组合自由放置会逐块检查棋盘透明区、已拼区域、自身凹槽和外部 Piece，并额外用组合总边界禁止横跨棋盘边框或侵入托盘原始区域。
- 新手引导期间不形成桌面组合，继续沿用已确认的单 Piece 教程节奏。
- 提示选择继续优先当前托盘排列第一块；托盘为空后优先最早形成的组合，没有组合时选择最早放到桌面的单块。
- `HintDashedOutlineGraphic` 增加多 Sprite Alpha 合并模式：将组合 Groove 映射到共同 Rect，栅格化为一张布尔蒙版后只提取一次边界，复用现有 `20px` 实线、`15px` 间距、`3px` 线宽和 `60px/s` 滚动参数。
- 托盘 Piece 拿起前会保存当前所有托盘 Piece 的世界位置快照。拼错回弹、主动拖回托盘或窗口失焦取消时恢复该快照，不再调用从托盘左边界重新生成目标的全量布局；被选 Piece 单独回弹到原槽位，拿起后发生补位的其他 Piece 平滑回到滚动前位置。
- Piece 成功拼入棋盘或合法放到桌面时直接清除本次快照，保留拿起后既有的前移补位结果。
- 组合形成或继续合并后，将全部成员 Sprite 的 Alpha 按当前世界位置栅格化为一张并集蒙版；成员自身使用无投影材质，只由一个运行时 `SpriteRenderer` 使用现有 `IngameCoverShadow02/04` 参数绘制整体投影。
- 拿起组合时整体投影切换为 `IngameCoverShadow04` 规则，松手留在桌面时切回 `IngameCoverShadow02`；拖动、错误回弹和提示动画期间投影跟随整体，正确入槽、回托盘、组合合并及切组时销毁或重建对应运行时资源。
- 组合回托盘拆散时，每个成员在回弹动画开始前统一恢复托盘默认投影 `IngameCoverShadow04`；该规则同时覆盖被正确 Piece 顶回托盘的组合成员。
- 组合提示记录全部成员的世界位置、旋转和共同中心，成员与整体投影使用同一个旋转增量绕共同中心抖动，结束、取消或开始拖拽时统一恢复。
- 修复提示抖动期间立即拿起组合时整体投影留在提示起点的问题：开始拖拽会先恢复组合基准姿态、重新采集拖拽起点与鼠标偏移，再终止本轮抖动并解除提示起点的位置恢复绑定，但保留棋盘虚线；组合投影改在本帧鼠标拖拽位置更新完成后同步。
- 桌面 Piece 或已有组合吸附成新临时组合后，在 `0.12s` 吸附缓动结束位置复用现有 `PuzzlePlacementShine` 参数播放一次整组滑光；全部成员共享同一道屏幕空间光带，播放结束后销毁临时覆盖层，不修改常驻 Piece 材质和整体投影。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Resources/PackCoverShadow.shader`
- `Assets/UI/GameScene/PieceShadow.png`（已删除）
- `Assets/UI/GameScene/PieceShadow.png.meta`（已删除）
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 临时组合不持久化。退出 GameScene 后不恢复桌面组合；正确入槽仍按既有 Piece 编号立即保存。
- 组合只允许当前活动分组内的 Piece 参与，避免跨组引用尚未创建或已销毁的运行时对象。
- 组合形成采用 Collider 最终布局和自适应距离阈值；桌面摆放位置只用于判断玩家是否已经把两块靠近到足以吸附。
- 新组合的形成顺序单独记录；组合继续合并时保留两个原组合中更早的形成顺序。
- 回托盘、被正确 Piece 顶回以及切组清理都会移除组合关系，避免托盘 Piece 被隐式绑定。
- 组合投影继续直接复用制作方既有 `02/04` 材质参数；Shader 只增加 `PACK_SHADOW_ONLY` 变体，不重新定义投影颜色、偏移、模糊、扩散或留白参数。
- 组合 Alpha 并集纹理最大边限制为 `2048px`，并随组合生命周期释放；只有全部成员纹理均成功参与合并时才启用整体投影，避免生成缺块投影。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告，`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告，`0` 错误。
- `git diff --check`：通过，仅有 Git 的 LF/CRLF 工作区提示。
- 托盘滚动回弹修复后再次串行编译 Runtime 与 Editor C# 项目：均通过，`0` 警告，`0` 错误。
- 组合整体投影与整体提示抖动修改后再次编译 Runtime 与 Editor C# 项目：均通过，`0` 警告，`0` 错误。
- 提示后拿起组合的投影跟随修复后，Runtime 与 Editor C# 项目再次编译通过，`0` 警告，`0` 错误。
- 临时组合吸附滑光修改后，Runtime 与 Editor C# 项目再次编译通过，`0` 警告，`0` 错误。
- 尚未在 Unity Play Mode 中完成鼠标拖放和组合虚线视觉验收。

## 下一步

1. 在 `CardBag018` 横向滑动托盘，把原本在屏幕外的 Piece 滑入后拿起并故意拼错，确认托盘保持当前滚动偏移、其他 Piece 恢复拿起前位置、被选 Piece 回到原槽位。
2. 同一路径分别测试主动拖回托盘和拖拽时切出窗口，确认两条取消路径也恢复同一滚动布局。
3. 在包含至少三块相邻 Piece 的普通关卡中，把第一块放到桌面，再把真实相邻第二块靠近，确认 `0.12s` 自动吸附；非相邻块不得吸附。
4. 用第三块加入组合，并从任意成员拿起，确认全部成员保持最终棋盘相对位置一起移动。
5. 将组合拖到非法棋盘边界或已占用区域，确认全部成员同时变红并回到各自拖拽起点。
6. 将组合中任一成员靠近对应凹槽，确认整组只有在统一平移后全部满足吸附距离时才一起拼入，并记录全部 Piece。
7. 将组合拖入托盘原始回收区，确认托盘自动显示、组合拆散并按当前随机顺序重新排列。
8. 清空托盘但保留桌面组合，点击提示按钮，确认组合成员与投影围绕共同中心作为一块整体抖动，虚线只包围组合对应凹槽外轮廓且没有内部接缝。
9. 确认两块及三块组合只显示一份外围投影、内部连接边没有重复阴影；拿起时整体投影切换为 `04`，放回桌面切回 `02`，拖动和错误回弹期间持续跟随。
10. 回归 `CardBag001` 新手引导、单 Piece 错误回弹、托盘横向滚动、切组和结算。

## 恢复提示

继续 Puffies 的“桌面临时拼图组合”任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；优先执行下一步 Play Mode 验收，并根据实际手感微调桌面组合吸附距离。未经用户明确要求不要自动提交；用户要求提交时同时推送。
