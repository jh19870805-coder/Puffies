# 卡包特效

本文档是卡包表现的稳定规格。当前需求放在前面，历史实现和未解决事项按日期记录在后面。

## 当前需求

### MainScene 卡包表现

**用户故事：** 作为玩家，我希望首页上每个可用卡包都以有生命感的卡包特效显示，使选择和打开卡包形成连续的视觉体验。

1. MainScene 创建完卡包列表时，每个可见卡包必须使用真实封面的卡包特效替代 `PackCover` Image。
2. 卡包处于空闲状态时，特效必须循环播放轻微呼吸缩放。
3. 点击卡包时，必须停止呼吸，并让同一个视觉对象从列表尺寸放大至原始设计尺寸 `600 x 680`。
4. 放大完成后，系统必须使用选中卡包的真实封面播放现有六层开包动画。
5. 开包动画结束后，系统必须携带选中 PackId 进入 GameScene。
6. 特效替换封面期间，`PackSize` 必须保留现有 RectTransform 位置、同级顺序、Sprite 和完成状态颜色。
7. 无法创建特效时，系统必须保留静态封面，并确保原有点击回退流程可用。
8. 卡包随 ScrollRect 移动时，特效必须持续对齐 `PackCover` RectTransform，且不得渲染到卡包列表视口之外。
9. MainScene 销毁或列表重建时，必须释放全部动态显示对象和 Mesh。
10. 卡包等待奖励飞行动画展示时，首页特效必须保持隐藏，直到现有显示回调执行。
11. 当玩家选择生命周期为 `Unlocked` 的未玩卡包时，`PanelBagSelect/BtnPlay` 必须显示 `Play`，`BtnCamera` 必须隐藏。
12. 当玩家选择生命周期为 `InProgress` 或 `Completed` 的已玩卡包时，`PanelBagSelect/BtnPlay` 必须显示 `重玩`，`BtnCamera` 必须显示。

### 开包交互与游戏进场

**用户故事：** 作为玩家，我希望从首页选中卡包、手势撕包到拼图进场是一段连续操作，使开包真正成为进入关卡的一部分，而不是按钮点击后的加载等待。

1. 当玩家点击列表卡包时，系统必须在约 `0.3s` 内将同一个卡包视觉对象移动并放大到选择位置，同时柔化并压暗当前首页，显示返回和确认操作。
2. 当玩家点击 `Play` 或 `重玩` 时，系统必须隐藏选择操作，将首页内容退场，并无闪屏地显示与 GameScene 一致的 `BgGame` 开包舞台；卡包在转场中必须保持封面、位置和比例连续。
3. 当卡包在开包舞台定场后，系统必须循环显示一个沿卡包顶部封口从左向右移动的无文字手势提示，并等待玩家操作，不得自动开包。
4. 当玩家从卡包顶部封口左侧开始并完成有效的向右横划时，系统必须隐藏提示并只触发一次开包；横向距离不足或明显偏离封口的操作不得开包，松手后提示恢复。
5. 当有效横划完成时，系统必须同步播放现有六层 `CardPackOpening` 动画和 `CardPackDismantle_001` 拆包粒子，保持选中 PackId 的真实封面。
6. 当开包动画结束时，系统必须携带选中 PackId 进入 GameScene；`BgGame` 背景必须保持连续，棋盘从上方进入、碎片托盘从下方进入，当前组 Piece 随后错峰落入托盘。
7. 开包与进场期间必须屏蔽重复点击、再次横划和拼图拖拽，动画完成后恢复 GameScene 输入。
8. 参考视频为竖屏录制；Puffies 保持现有 `2560 x 1440` 横屏布局，只复刻状态顺序、交互方式、相对运动和节奏，不复制竖屏坐标。

### 实现设计

- 选择背景在打开 `PanelBagSelect` 前截取一次当前屏幕，使用低分辨率双线性重采样生成静态柔化背景；退出选择或进入开包舞台时立即释放临时纹理。
- 开包舞台由 MainScene 运行时 Canvas 显示 `UI/BasicUI/BgGame.png`，与 GameScene 场景背景使用同一源图，避免场景加载时背景跳变。
- 横划手势使用屏幕坐标与当前六层卡包 Renderer 的实际 Bounds 判定，不按固定分辨率硬编码触发区域。
- `CardPackDismantle_001` 与六层 Animator 同步启动，并在场景切换后短暂保留，覆盖 MainScene 到 GameScene 的视觉交界。
- GameScene 只在 MainScene 正常开包进入时播放一次入场动画；编辑器直接启动 GameScene 或其他入口保持即时初始化，避免干扰关卡制作和调试。

### 编辑器拆包预览

**用户故事：** 作为开发者，我希望在 Unity Editor 中同时看到完整卡包动画、拆包粒子和项目真实封面，以判断交付内容是否符合特效参考。

1. 打开预览 Prefab 时，Unity 必须显示全部六个原始卡包动画层，并使用 `PackIcon001` 作为动态封面。
2. 播放组合预览时，五个原始 ParticleSystem 节点必须全部保留并使用其导入材质。
3. 重建预览时，编辑器工具不得修改任何导入的源 Prefab。
4. 项目使用 URP Renderer2D 时，导入粒子的 Pass 必须使用兼容 SRP 的 Unlit LightMode。
5. 此预览仅用于编辑器，不得改变 MainScene 播放逻辑。
6. 通过预览菜单打开时，六个 Animator 和拆包粒子必须在 Scene View 中同步循环。

## 当前实现

### 运行时

- `GameAnimationUtility` 根据六个动画层的第零帧创建一份共享空闲 Mesh。每个卡包只使用一个带独立封面和生命周期颜色的轻量 MeshRenderer，不常驻六个动画层。
- MainScene 在 `LateUpdate` 中将显示对象对齐 UI 锚点，按错峰方式在 `2.4s` 内进行 `0.98..1.02` 呼吸缩放，关闭页面外 Renderer，并将 ScrollRect 视口裁剪矩形传给 `CardPackOpening.shader`。
- 点击后，选中卡包从空闲 Renderer 切换到相同姿态的可复用六层开包器，在 `0.3s` 内放大到 `600 x 680`；Play/重玩后切到 `BgGame` 舞台并等待顶部向右横划，有效手势才同步播放六层原始动画和拆包粒子，然后进入 GameScene。
- MainScene 每次打开 `PanelBagSelect` 时根据 `CardPackRecord.IsPlayed` 刷新操作状态：`Unlocked` 显示 `Play` 并隐藏相机按钮，`InProgress` 和 `Completed` 显示 `重玩` 并显示相机按钮。
- `PackSize` 继续提供位置、Sprite、颜色和前景显示依据。静态封面与阴影回退、奖励飞行隐藏和动态资源清理均保留。
- 选择页使用截屏降采样的柔化首页背景和 `0.34` 遮罩；开包舞台使用与 GameScene 相同的 `BgGame`。GameScene 只在正常拆包入口播放一次棋盘、托盘、Piece 和操作按钮入场。

### 编辑器预览

- `CardPackDismantlePreview.prefab` 将全部六个嵌套 `CardPackOpening` 层与未修改的嵌套 `CardPackDismantle_001` 粒子特效组合。
- `PackIcon001` 通过 Renderer PropertyBlock 应用，与运行时动态封面方式一致。
- **Puffies -> Effects -> Preview Card Pack Dismantle** 打开预览，并同步循环六个 Animator 和五层粒子结构。
- 两个旧粒子 Shader Pass 使用 `SRPDefaultUnlit`，其余原始计算和混合模式保持不变。
- 与美术参考的最终视觉对比仍待在可见 Unity Play Mode 中确认。

## 验证

- MainScene Play Mode 验证创建了四个空闲特效。PackId 1 的呼吸缩放从 `2.449215` 变化到 `2.52881527`，放大至 `6.246120`，启动六个 Animator 和六个 Renderer 后进入 GameScene。
- `2560 x 1440` 的空闲和开包截图确认封面、前景尺寸图标、列表对齐、视口裁剪和撕开动画均正确。
- Unitypackage GUID 审计确认全部交付资源已存在：Shader 更新包 16/16、卡包动画包 44/44、拆包粒子包 13/13。
- Unity 输出 `Card-pack combined preview started. animators=6, particles=5`，没有异常。
- `dotnet build Puffies.sln --no-restore` 为 0 警告、0 错误；Unity 干净批处理刷新也没有 C# 或 Shader 编译错误。
- 2026-07-24 新增横划开包和 GameScene 入场后，`dotnet build Puffies.sln --no-restore` 再次为 0 警告、0 错误。隐藏 Unity 批处理不产生 `WaitForEndOfFrame`，普通 Editor 又无法在当前隐藏会话完成启动，因此新流程仍需在可见 Play Mode 做最终视觉与手感验收。

## 历史记录

### 2026-07-23 - MainScene 常驻特效

- 使用每个卡包的真实封面和共享 Mesh 常驻特效替换静态列表封面。
- 增加空闲呼吸、视口裁剪、前景尺寸图标映射、可复用开包器放大、完整六层开包播放、失败回退和资源清理。
- 删除了会在脚本重载后错误强制进入 Play Mode 的 `TemporaryOpenGameView`。
- 运行时流程已确认；拆包粒子时机保留为独立后续项。

### 2026-07-22 - 拆包预览

- 审计三个交付特效包，确认 Shader 更新包包含共享依赖和第六个开包层，并非第三套独立视觉序列。
- 使用包含全部六个开包层和五个原始 ParticleSystem 的组合预览替换旧静态封面粒子测试。四个系统渲染 Burst，根系统是不渲染的控制器。
- 保留导入源 Prefab，适配两个旧 Pass 以支持 Renderer2D，并重新生成仅供编辑器使用的预览 Prefab。
- 工作站窗口布局遮挡了自动截图，因此未完成与美术参考的最终无遮挡对比，该项保持待办。
