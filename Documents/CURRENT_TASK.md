# 当前任务

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
