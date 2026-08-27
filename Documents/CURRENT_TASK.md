# 当前任务

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
