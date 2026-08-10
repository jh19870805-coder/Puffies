# 当前任务

- 任务：卡包投影改为编辑器可调 Shader
- 状态：旧 PackShadow 和运行时纹理生成已删除，新 Shader/Material 已配置并通过 C# 编译，等待 Unity 导入与视觉调参
- 更新时间：2026-08-10

## 用户意图

- 参考现有 `CardBagXXX.prefab` 的分组方式，将同一局部区域的贴纸划入一组。
- 先在 `CardBag022.prefab` 上尝试一版大概分组。
- 将 CardBag022 最终确认的排序和命名规则同步到 CardBag 自动生成工具。
- 所有棋盘背景都固定使用原描边颜色 `#3f423e`；撤销高对比度深色底板改用 `#b1d702` 的规则。
- 描边边界形状保持不变，线条改为轻微铅笔质感：不规则深浅、少量细小中空点和短断点，不做规则虚线，纹理不能随棋盘移动闪烁。
- 提示按钮生成的绿色滚动虚线在普通模式继续使用当前颜色，高对比度模式改用 `#b1d702`；该规则不应用于烘焙棋盘描边和新手引导蓝色虚线。
- 当前组的烘焙棋盘描边在棋盘开始移动时保持隐藏；首次入场和每次切组的棋盘移动结束后，用 `0.5s` 淡入显示。
- 不重建 Prefab，不修改贴图、位置、尺寸、Image 参数、层级或影子。
- 新手引导只保留游戏原有的暗色托盘，不再额外叠加教程黑色遮罩。
- 第一阶段从贴纸移动到凹槽的 `GuideArrow1.png` 保持宽高比并缩小 30%；第三阶段提示框内的箭头不变。
- 第一阶段提示框以屏幕归一化位置 `(0.5, 0.7)` 为基础，向上移动一个提示框背景板自身高度后，累计左移 `30`、下移 `50`；不跟随包含透明区域的 CardBag Rect。
- 引导提示文字最多显示两行；内容较长时保持两行并自动缩小字号，不改变编辑器模板的字体、材质、颜色、字重和对齐样式。
- 参考视频 `85562245f2decc4cc7e116bd1d06798f.mp4`：拼图块正确落位后播放暖白金色斜向滑光，并让与新块相邻连通的已拼贴纸一起出现连续滑光。
- 上一轮为拼图落位滑光增加外扩 ADD 光晕的方向不符合需求，需要恢复修改前行为。
- 将新提供的四张高光贴片直接配置到 `PackItem.prefab`，让位置、尺寸、颜色和透明度可在 Unity Inspector 中调整；高光必须使用真正的 ADD 混合，MainScene 不在运行时修改视觉参数。
- 删除 `PackItem/PackShadow` 和 MainScene 动态生成阴影贴图的逻辑；卡包投影改由直接配置在 `PackCover` 上的 Shader 绘制，颜色、透明度、偏移、横纵模糊、扩散和渲染留白都可由美术在 Material Inspector 中调整。

## 工作记录

- 解析 `CardBag022` 的 196 个 Piece 中心位置和原生尺寸，并与完整预览图核对空间分布。
- 初版按从上到下的 7 个水平区域、每个区域从左到右 4 组分为 28 组；根据体验反馈将相邻的 `01+02`、`03+04` 等两两合并，当前共 14 组、每组 14 片。
- 组号按棋盘从上到下、同一行从左到右排列；组内索引按中心位置从左到右稳定排列。
- 仅将三位中间名 `Piece001..Piece196` 替换为正式四位名 `PieceGGII`，没有调整任何序列化布局或渲染属性。
- 整理 `GameBoard` 子节点的同级顺序：保留 `BoardTitle` 第一位，其后严格按 `Piece0101..Piece1414` 升序排列，方便 Unity Hierarchy 查看并确保直接遍历顺序稳定。
- 按用户指定调整阶段顺序：完整交换分组 `03 ↔ 04`、`07 ↔ 08`、`11 ↔ 12` 的名称；每片组内索引保持不变，并在交换后重新按新名称整理同级顺序。
- 当前打开的 Unity Editor 在导入正式分组后将 196 个 Piece 占位 Image 的 Alpha 同步为 `0`；该配置与 CardBag020/021 的隐藏棋盘占位 Piece 一致，因此保留，不回退为初始未分组状态的 Alpha `1`。
- 自动生成器现在对标准 `piece_###.png` 按位置从上到下分带，每行最多两个组、每组最多 14 片，并按左右交替的蛇形顺序生成正式 `PieceGGII`；组内按中心点从左到右编号，Hierarchy 按正式名升序创建。
- 全部显式 `PieceGGII.png` 继续保留人工分组；显式名与标准名混用时生成失败并给出明确错误。“Update Existing Piece Layouts”仍只更新位置和尺寸，不重新命名或分组。
- 批量生成不自动执行耗时描边烘焙，但会删除该包旧描边并提示执行 `Bake Outline Masks`，避免新分组加载旧蒙版。
- 尝试以 Unity 批处理执行全量描边烘焙；由于项目已被当前打开的 Unity Editor 占用，批处理实例等待项目锁，已只关闭该等待实例。现有编辑器已成功导入更新后的 Prefab。
- 当前打开的 Unity Editor 随后完成全量描边烘焙；CardBag022 已生成 14 组对应的默认、关卡和贴纸描边资源，共 42 张 PNG。
- GameScene 初始化时读取并缓存 `IsHighContrastEnabled`；默认连接描边、完整关卡描边和贴纸描边共用同一颜色规则。
- Built-in UGUI 描边 Shader 只读取烘焙 PNG 的 Alpha并统一输出 `#3f423e`；高对比度开关继续更换棋盘背景，但不再改变描边颜色。
- `PuzzleOutlineTint` 使用固定在源纹理像素坐标上的两级稳定噪声，对线条做轻微透明度颗粒，并以约 `9%` 的细粒空点和约 `3%` 的局部两像素空点形成不规则断墨；不修改离线烘焙边界和现有描边资源。
- 描边 Shader 和运行时 Material 只服务于烘焙棋盘描边；提示按钮的滚动虚线单独根据高对比度选择当前绿色或 `#b1d702`，新手引导蓝色虚线保持不变。GameScene 销毁时释放运行时 Material。
- 删除教程焦点层创建 `TutorialTrayDim` 的逻辑，避免它与游戏现有 `PieceBoard/PieceBg` 暗色托盘叠加成两层遮罩；教程 Piece 高亮、文字、虚线和交互限制保持不变。
- 第一阶段移动箭头的原生 Sprite 尺寸统一乘以 `0.7`，并继续使用缩放后的实际箭头高度计算移动终点。
- 第一版尝试按 CardBag 运行时 Rect 定位，但该 Rect 包含透明区域，与画面中的可见棋盘边界不一致，实际位置仍偏离红框；现已删除该分支，第一步直接使用红框中心对应的屏幕归一化锚点 `(0.5, 0.7)`。
- 教程文字按模板当前字号测量内容宽度，超过一行时选择宽度最均衡的断点拆成两行，并排除逗号、句号等标点出现在第二行开头的断点；关闭进一步自动换行并启用 TMP Auto Size，以模板字号为上限、模板最小字号为下限自动适配。
- 按最新反馈，第一步在归一化锚点位置基础上增加 `promptSize.y`，完整向上移动一个提示框背景板高度，再执行现有屏幕安全边界限制。
- 第一阶段最终位置的设计坐标偏移更新为 `(-30, -50)`，即累计左移 `30`、下移 `50`，之后再执行屏幕安全边界限制。
- 使用 FFmpeg 对 `720 x 1280`、约 `6.17s` 的参考视频按 `15fps` 抽取落位前后高密度帧；滑光主亮峰约 `0.2s`，整体约 `0.45~0.55s`，颜色偏暖白黄，光从新落位区域延伸到接触的已拼贴纸。
- 新增 Built-in UGUI 滑光 Shader，按屏幕空间统一计算斜向光带并只读取 Sprite Alpha；同一次反馈中的所有贴纸共用一个运行时 Material，保证跨贴纸时光带方向和位置连续。
- 正确落位后从当前所有已显示、Alpha 大于零的 Piece 中，以屏幕 Rect 接触和 `10` 设计像素邻距构建新块所在的连通区域；光带范围覆盖整个连通区域，不包含未拼 Piece。
- 落位反馈时为连通区域创建临时 Image 叠层，复制各 Piece 的 RectTransform、Sprite 和 Image 显示方式；滑光约 `0.52s`，结束后销毁叠层，GameScene 销毁时释放运行时 Material。
- `PlayPieceSnapAnimation` 现在等待滑光结束后才解除落位锁定并执行切组或结算；吸附时长、持久化、分组和新手引导判断不变。原来的绿色缩放闪光已删除。
- 已撤销上一轮误加在 `PuzzlePlacementShine` 和 GameScene 临时叠层上的 `1.1x` 外扩光晕，恢复原有拼图滑光实现。
- 将 `高光.zip` 中的四张贴片以 `PackHighlight02..05.png` 导入 `Assets/UI/MainScene`，使用无压缩 Sprite 配置保留透明边缘。
- 新增 `PackHighlightAdditive` UGUI Shader 和 Material：使用 RGB 预乘后的 `Blend One One`，只写 RGB，不修改目标 Alpha。
- `PackItem.prefab` 新增 `PackHighlight` 父节点和四个独立 Image 子节点，父节点提供统一 Alpha，子节点可分别调整位置、尺寸、颜色、Sprite 和 Material；层级位于 `PackCover` 上方、`PackSize` 下方。
- MainScene 只在创建列表项时将整个 `PackHighlight` 按封面从 `600 x 680` 缩放到列表 `240 x 272`，不覆盖 Prefab 内高光参数。
- `ActiveGroupOutline` 根节点现在使用独立 `CanvasGroup` 控制透明度；创建首组或下一组时初始 Alpha 为 `0`，首次入场和切组的棋盘移动结束后使用不受 TimeScale 影响的 `0.5s` 平滑淡入。刷新或清理描边时会停止旧淡入协程，避免动画叠加。
- 新手引导第一阶段继续阻止烘焙描边创建；从第一阶段切到第二阶段时允许提前创建透明描边，在棋盘移动完成后淡入，进入第二阶段提示时不再重复销毁重建描边。
- 新增 `PackCoverShadow` UGUI Shader 和共享 Material；Shader 在同一个 `PackCover` Graphic 中先根据封面 Alpha 生成投影、再合成原封面，通过顶点外扩为偏移和模糊保留渲染空间，不需要额外阴影节点。
- MainScene 已删除封面可读像素读取、CPU Box Blur、运行时 Shadow Sprite/Texture 缓存及释放逻辑；列表只替换 `PackCover.sprite`，不会覆盖 Prefab 中配置的投影 Material 参数。
- 修复首版 Shader 直接缩放 UGUI 合批顶点导致 `PackCover` 围绕 Canvas 原点偏移的问题；新增 `PackCoverShadowEffect`，在 `Image` 网格生成阶段围绕自身 `RectTransform.rect.center` 外扩顶点，Shader 不再修改顶点位置。
- `PackCoverShadowEffect` 实时读取共享 Material 的 `Render Padding X/Y` 和当前封面纹理尺寸；美术修改 Material 后会自动标记封面网格刷新，视觉参数仍只维护在 Material 中。
- `PackHighlight` 父节点初始化改为关闭，MainScene 不主动启用；四张现有 ADD 高光贴片继续保留供后续特效时机和混合方式确认，但首页常驻列表不再显示散落光点。
- 投影首版默认值在蓝色桌布上过弱，现将颜色调整为近黑色、Alpha `0.7`、Y 偏移 `-55px`、横纵模糊 `10/32px`、扩散 `8px`，Y 留白增至 `140px`；X 偏移保持 `0`，投影方向仍只向下。

## 修改文件

- `Assets/Resources/CardBagPrefabs/CardBag022.prefab`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBag022/`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Resources/PuzzleOutlineTint.shader`
- `Assets/Resources/PuzzlePlacementShine.shader`
- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/View/PackCoverShadowEffect.cs`
- `Assets/UI/MainScene/PackHighlight02.png` 到 `PackHighlight05.png`
- `Assets/Resources/PackHighlightAdditive.shader`
- `Assets/Resources/PackHighlightAdditive.mat`
- `Assets/Resources/PackCoverShadow.shader`
- `Assets/Resources/PackCoverShadow.mat`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 验证

- `CardBag022` 共 196 个合法四位正式 Piece 名，三位中间名 0，重复名 0。
- 分组为 `Group01..Group14`，每组均为 14 片，索引完整为 `01..14`。
- `GameBoard` 共 197 个子节点，顺序精确等于 `BoardTitle` 加 `Piece0101..Piece1414`，顺序差异 0。
- 指定的三对分组交换后仍为 196 个唯一正式名，14 组均保持 14 片且索引完整为 `01..14`。
- CardBag022 当前 Piece 占位 Image 共 196 个 Alpha `0`；CardBag020/021 的 Piece 占位同样采用 Alpha `0`，配置方向一致。
- `git diff --unified=0` 检查确认手工 Prefab 操作仅修改 Piece 的 `m_Name` 与 `GameBoard.m_Children` 排序；Unity Editor 另将正式棋盘占位 Piece 的 Alpha 同步为 `0`。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- 使用 CardBag022 的 196 个实际中心位置回放自动分组算法，得到 14 组、每组 14 片，自动名称与当前人工确认结果完全一致，差异 0。
- CardBag022 描边目录包含 42 张 PNG 和 42 个对应 Meta；`Group01..Group14` 的默认、`_Level`、`_Stickers` 文件全部存在。
- Unity 日志显示 `Puzzle outline baker: baked 114 group mask(s) from 22 card bag(s).`，未发现脚本编译错误；尚未进行 Play Mode 视觉和流程验证。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 和 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 顺序执行通过，均为 0 警告、0 错误。
- 项目当前被已打开的 Unity Editor 占用，未启动第二个批处理实例；新增 Shader 的 Unity 导入和浅色/深色底板实际显示仍需 Play Mode 验证。
- 新手引导修改后再次顺序编译 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`，均为 0 警告、0 错误。
- 尚未在 Unity Play Mode 确认 CardBag001 第一阶段只剩一层托盘，以及箭头缩小后的最终视觉尺寸。
- 第一阶段提示框定位及两行文字适配修改后，顺序编译运行时与编辑器程序集，均为 0 警告、0 错误。
- 尚未在目标分辨率 Play Mode 对照红框确认提示框最终像素位置和自动字号结果。
- 移除错误的 CardBag Rect 定位并修正标点断行后，再次顺序编译两个程序集，均为 0 警告、0 错误。
- 第一阶段上移一个背景板高度后，再次顺序编译运行时和编辑器程序集，均为 0 警告、0 错误。
- 第一阶段追加 `(-20, -30)` 微调后，再次顺序编译两个程序集，均为 0 警告、0 错误。
- 第一阶段继续左移 `10`、下移 `20`，累计偏移更新为 `(-30, -50)`；顺序编译两个程序集，均为 0 警告、0 错误。
- 参考视频已完成 5fps 全流程和 15fps 落位局部逐帧检查，确认滑光作用于新块及其相邻已拼区域，而不是全棋盘闪光。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 与 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 在滑光实现及范围修正后顺序通过，均为 0 警告、0 错误。
- 当前 Unity Editor 日志未发现新的 `Shader error` 或 C# 编译错误，但日志中尚未出现新 Shader 的明确导入记录；仍需回到 Unity 触发资源刷新并做 Play Mode 视觉验证。
- `git diff 1d8ebd7 -- Assets/Resources/PuzzlePlacementShine.shader Assets/Scripts/Controller/GameScene.cs` 无差异，确认错误的外扩 ADD 方案已完整撤回。
- `PackItem.prefab` 的四个 Image 均引用同一个 `PackHighlightAdditive.mat`，并分别引用四张新高光 Sprite；`git diff --check` 通过。
- 固定颜色与铅笔质感修改后，顺序编译 `Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj`，均为 0 警告、0 错误。
- 使用真实 `CardBag001/Group01.png` 与深色 `BgCardBoard2.png` 离线模拟 Shader，确认边界位置不变，实线变为轻微颗粒、深浅和零星短空点，没有形成规则虚线或大段缺失。
- 修复 Unity 导入时的两条 `MaterialPostprocessor` 空引用：`PackHighlightAdditive.shader/.mat` 已从会被 BuildSync 复制的 `Assets/UI/MainScene` 移到 `Assets/Resources`，保留原 GUID 和 PackItem 引用；旧 `StreamingAssets` 副本已清理，Unity 刷新后的最新日志不再出现这两条错误。
- 提示按钮创建 `HintDashedOutlineGraphic` 时读取本局缓存的高对比度设置：关闭时保持 `(112,151,75)`，打开时使用 `#b1d702`；教程调用继续显式传入原蓝色。
- 提示虚线颜色分支修改后顺序编译运行时和编辑器程序集，均为 0 警告、0 错误。
- 棋盘描边淡入修改后顺序编译 `Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj`，均为 0 警告、0 错误；尚需在 Unity Play Mode 确认首次入场、普通切组和新手引导第1至第2阶段的实际节奏。
- `PackItem.prefab` 已无 `PackShadow` 节点，`PackCover` 正确引用 `PackCoverShadow.mat`；MainScene 已无 `PackShadow`、动态阴影 Sprite 或 Box Blur 引用。
- 投影改造后顺序编译 `Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj`，均为 0 警告、0 错误；当前 Unity Editor 日志未在本轮继续写入，Shader 导入和首页实际效果仍需回到编辑器确认。
- 使用独立临时 `netstandard2.1` 编译项目引用当前 Unity 2022.3 的 `UnityEngine`、`UnityEngine.UI` 和 `TextRenderingModule`，确认新 `PackCoverShadowEffect.cs` 为 0 警告、0 错误；临时工程文件已删除，未修改 Unity 生成的 csproj。
- Prefab YAML 检查确认 `PackHighlight.m_IsActive=0`；`PackCoverShadow.mat` 与 Shader 默认值已同步为加强后的下方投影参数，`git diff --check` 通过。现有 MainScene Play Mode 实例需重新进入场景后才会使用更新后的 Prefab 状态。

## 下一步

1. 回到 Unity 等待 `PackCoverShadow.shader` 导入，确认 Console 无 Shader 错误，并在 MainScene 检查列表封面与投影边界。
2. 由美术选中 `Assets/Resources/PackCoverShadow.mat` 调整投影参数，重点确认向下偏移、深浅、模糊高度和 `Padding` 不裁切。
3. 回到 Unity 等待 `PuzzleOutlineTint.shader` 导入，确认 Console 无 Shader 错误。
4. 分别用 `BgCardBoard1` 和 `BgCardBoard2` 进入关卡，确认两者描边均固定为 `#3f423e`，线条有轻微铅笔颗粒和小断点且仍清楚可读。
5. 分别在普通和高对比度模式点击提示按钮，确认滚动虚线从当前绿色切换为 `#b1d702`；新手引导蓝色虚线保持原色。
6. 在 Unity Play Mode 确认首次入场和每次切组时描边先隐藏，并从棋盘移动结束时开始用 `0.5s` 淡入；CardBag001 第一阶段保持无烘焙描边，第二阶段正常淡入。

## 数据说明

- SQLite 表结构未变化。
- `CardBag022` 现在使用正式 Piece 编号；如果本地已经产生过该包的三位中间编号进度，测试前删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`。
