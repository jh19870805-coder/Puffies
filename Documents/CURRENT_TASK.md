# 当前任务

- 任务：剩余关卡美术切图标准化与 Prefab 生成
- 状态：CardBag022 生成故障已修复；下载目录中的六个卡包尚未导入 Unity
- 更新时间：2026-07-29

## 用户意图

- 将 `D:\360极速浏览器X下载\剩下关卡的切图` 中的数字目录统一为三位编号的 `CardBagXXX`。
- 将预览图统一放在下载根目录，将 GameBoard、BoardTitle 和 smooth Piece 整理成对应 CardBag 的扁平目录结构。
- 参考 `9696192e8e16c9aca4ab142437f24bb3.mp4` 实现首次拼图引导的路径、节奏和交互。
- 引导箭头使用 `Assets/UI/GameScene/GameImgArrow.png`，旋转朝向槽位，移动过程中保持宽高比等比放大到原生尺寸。
- 棋盘目标位置复用提示按钮的绿色滚动虚线；引导期间不显示烘焙的当前组描边。
- 托盘区域变暗，只突出指定 Piece；指定 Piece 放错后恢复引导，正确放置后继续提示下一个，整包完成后结束并持久化。
- CardBag 自动生成时，Piece 坐标优先按完整的 `Previews/CardBagXXX.png` 匹配；Preview 因量化或分割线无法可靠定位时，允许使用保留完整 RGB 的 `GameBoard.png` 受控回退。
- 新导入 CardBag 的 Preview 分割线或相邻碎片覆盖不得导致正确 Piece 因少量边缘像素不一致而生成失败；低分位置仍须具备唯一性证据。
- 卡包拆开或确认重玩后，只拼一部分退出，下次选择时显示“玩”而不是“重玩”。
- 正确拼到棋盘上的 Piece 要持久化，下次进入仍显示在棋盘上并继续剩余拼图。
- MainScene 卡包选择按钮的 `Play` 改为“玩”。
- 新拆开但一次都没有完整完成的卡包不能显示相机按钮。
- `Resources/Configs/CardPacks.csv` 新增 `BoardScale` 列。
- 进入 GameScene 后，根据当前 BagId 读取 `BoardScale`。
- 棋盘、槽位和创建出来的贴图碎片使用该缩放比。
- Piece 初始位于下方黑色托盘时，生成比例取“配置后的棋盘目标比例”和“修改前黑色托盘规则算出的比例”中的较小值。
- 拿起 Piece 后使用配置后的棋盘目标比例，因此尺寸只能保持不变或放大，不能在按下时缩小；放置失败后恢复生成时的托盘比例。
- 所有游戏页面使用 `ImgHand_1.png` 作为常规鼠标图标。
- GameScene 鼠标悬停在可拖贴纸上时切换为 `ImgHand_2.png`；按下左键并拿起贴纸后切换为 `ImgHand_3.png`。
- 分阶段描边不得在当前组与已完成组交界处重复绘制已由前序阶段显示过的线段。
- 拼图 Piece 从托盘拿起后可以放在桌面；未命中正确槽位时停在松手位置，不再返回托盘。
- CardBag 棋盘外层背景默认使用 `BgCardBoard1.png`；设置页高对比开关打开时使用 `BgCardBoard2.png`，关闭时恢复 `BgCardBoard1.png`，初始化默认关闭并持久化选择。

## 工作记录

- 已整理 `015、016、018、019、020、021` 六个关卡：一级目录分别为 `CardBag015`、`CardBag016`、`CardBag018`、`CardBag019`、`CardBag020`、`CardBag021`。
- 每包 `preview.png` 已重命名为 `CardBagXXX.png`，随后移到下载根目录；`smooth/background_base.png` 已移动并重命名为 `GameBoard.png`；其余 `piece_###.png` 已从 `smooth` 移到 CardBag 一级目录；空 `smooth` 已删除。
- 根目录 `PackTitleXXX.png` 已移动到对应 CardBag 并重命名为 `BoardTitle.png`。
- 本次没有复制资源到 Unity 工程，没有生成或覆盖 CardBag Prefab。
- 新手引导限定为从未完成过引导且历史未完成的 `CardBag001`；已有部分进度时从当前未完成 Piece 继续。正常拆包时等待 GameScene 入场动画结束，直接打开场景时立即启动。
- 目标固定为当前组编号最小的未完成 Piece。引导期间其他 Piece 不响应悬停和拖拽，提示按钮暂时禁用。
- 运行时创建顶层引导 Canvas：托盘半透明遮罩、目标 Piece 副本、中文操作文案和箭头都不修改场景 Prefab。
- 箭头使用 `TutorialArrowScale`：原生 `122 x 271` 直箭头从目标 Piece 上方开始，底部 Pivot 沿指向槽位的直线同步前移，整图保持宽高比从 `0` 等比放大到 `1`；终点按原生箭头长度反算，保证完整尺寸时箭头尖落在槽位中心，不做非等比拉伸或弯曲。
- 槽位继续使用 `HintDashedOutlineGraphic` 的绿色滚动虚线。错误松手后按新位置恢复完整引导视觉；正确吸附后切换到下一 Piece。
- 拿起目标 Piece 时只隐藏箭头、托盘遮罩、文案和 Piece 副本，目标槽位绿色虚线持续显示，直到正确放置或切换目标。
- 箭头节奏为 `0.9s` 平滑移动并等比放大、`0.12s` 在目标位置以原生尺寸停留、`0.2s` 整体隐藏后重新开始。
- 引导覆盖 `CardBag001` 的每一个 Piece；当前 Piece 正确吸附后立即提示下一个，跨分组持续到整包完成。
- 引导完成状态写入 SQLite `AppRecords` 的 `Tutorial/CardBag001AllPiecesCompleted` 记录，不新增数据表。
- `BuildSync` 新增同步 `Assets/UI/GameScene` 到 `StreamingAssets/UI/GameScene`，确保 Player 可加载箭头资源。
- CardBag 生成器默认加载 Preview 作为 Piece 像素匹配参考图；GameBoard 通常只提供运行时棋盘 Sprite 和画布尺寸，Preview 无法通过置信度校验时才作为定位回退。
- Preview 与 GameBoard 仍强制要求相同尺寸；GameBoard 透明洞区不再需要保留完成图 RGB。
- Piece 第二轮像素匹配忽略 Alpha 形状边缘内侧 `1px`，排除 Preview 分割线和相邻碎片覆盖造成的边缘干扰。低于 `98%` 时仅允许“分数至少 `90%` 且精确 RGB 锚点在 Preview 中只出现一次”的位置，并记录警告；其余情况继续拒绝生成。
- Unity 已自动批量生成 `CardBag002`、`003`、`004`、`005`、`006`、`008`、`010`、`011`、`012`、`013`、`014` Prefab；生成结果为 `11` 成功、`0` 失败。
- SQLite 新增 `CardPackPuzzleProgress` 表，按 `PackId` 保存去重、排序后的 Piece 数字编号 JSON 和更新时间；记录存在即表示有当前可继续的拼图会话。
- GameScene 进入时确保会话存在并加载已放置 Piece；已完成 Piece 直接恢复为 Prefab 原始 `Image`，从首个未完成分组创建剩余可拖 Piece。
- 每次正确吸附先即时保存 Piece 编号，再更新棋盘显示和切组；整包完成且 `Completed` 保存成功后清除会话。
- `Completed` 生命周期在重玩期间保持不变，避免破坏完成数量、置灰、排序、首次完成时间和首次完成发包判定。
- MainScene 仅在 `Completed` 且没有活动会话时显示“重玩”并弹确认；其余可玩状态统一显示“玩”。确认重玩时清除旧会话。
- 相机按钮改为只按历史完成状态显示：`Completed` 显示，首次拼图中的 `Unlocked` / `InProgress` 隐藏。
- `CardPackConfigData` 新增 `BoardScale` 浮点字段。
- `CsvRow` 新增使用 `InvariantCulture` 的浮点读取，避免系统小数点区域设置影响 CSV。
- `CardPacks.csv/BoardScale` 现在是必填正数；缺失、无法解析或小于等于零的行视为无效配置。
- GameScene 初始化时按 BagId 读取配置，并将 `BoardScale` 乘到 CardBag Prefab 原始根节点缩放；`GameBoard`、Piece 槽位、描边坐标和吸附位置随层级统一缩放。
- 拖拽期间的 SpriteRenderer Piece 比例从对应棋盘槽位的实际显示尺寸计算，因此自动包含 `BoardScale`。
- 托盘比例计算时除去 `BoardScale`，继续使用修改前的棋盘匹配比例和托盘最大高度限制。
- 创建 Piece 时一次性保存包含 `BoardScale` 的 `DragScale`；拿起时直接使用保存值，不再根据可能已变化的 Canvas/棋盘状态二次计算。放置失败恢复 `TrayScale`，成功则切换为 Prefab 原始 Image 显示。
- 每个 Piece 创建时直接执行 `TrayScale = Min(旧托盘规则比例, DragScale)`；不再使用整组宽高或是否超框作为分支条件。
- 棋盘先应用 `BoardScale`，再按照当前组缩放后的实际屏幕范围居中；入场动画以该居中结果作为终点，不再恢复 Prefab 原始锚点。
- 当前组拖拽使用的运行时 `SpriteRenderer` 不再按整张棋盘的单一比例推算尺寸；每个 Piece 分别读取对应槽位缩放后的实际世界宽高计算 X/Y `DragScale`。
- Piece 成功吸附后，同一帧立即显示对应 Prefab 原始 `Image`，并停用、销毁拖拽 `SpriteRenderer`。已放置 Piece 与棋盘处于同一 Canvas 层级并共同继承 `BoardScale`，避免世界空间 Renderer 与 UI Image 的采样、缩放误差产生放大后的接缝。
- 成功吸附 Piece 后不再完整刷新托盘布局；仅将编号位于其后的未放置 Piece 沿 X 轴前移“被取走 Piece 的托盘宽度 + 间距”，并更新失败回退 X。所有剩余 Piece 的 Y 和缩放保持不变；先拼队尾 Piece 时其他 Piece 完全不刷新。
- 最后一块吸附并切组时，上一组运行时 Piece 在调用延迟 `Destroy` 前先立即停用；下一组重新居中和恢复 Prefab 原始 Piece 时不再与旧 Renderer 重叠，避免已完成 Piece 在 Y 轴抖动一帧。结算清理使用同一规则。
- `GameCursorUtility` 在场景加载前从 `UI/BasicUI` 加载三张运行时可读 PNG 并设置系统光标；常规、贴纸悬停和贴纸抓取分别使用 `ImgHand_1/2/3.png`。GameScene 的悬停与按下复用同一个命中方法，离开 GameScene 时恢复常规图标。
- 自定义光标使用 `CursorMode.ForceSoftware` 绘制，切换时分别保留 `52x58`、`68x48`、`64x50` 的真实纹理宽高，避免 Windows 硬件光标固定画布把较宽的 `ImgHand_2/3` 横向压扁。
- `PuzzleOutlineBakerEditor` 在同一卡包内按组顺序记录已认领的描边像素；后续组生成后删除前序组已经认领的像素，只保留当前阶段真正新增的最终外边界和已完成组接触边。
- 组间接触边与最终外轮廓都改用圆形最近距离和局部边界法线判定归属；当前组位于边界切线方向时不再认领该线段，避免拐角端点沿已完成区域多画。
- 已重新烘焙 `CardBag017` 五组蒙版。最终 `Group02` 相对原蒙版删除 209 个像素且没有新增像素，其中左上 `400px` 范围删除 83 个，覆盖截图红框对应的外轮廓端点。
- 后续 Unity 日志确认用户最新截图实际测试的是 `CardBag009`（`BagId=9`），不是 `CardBag017`；已使用最终算法重新烘焙 `CardBag009` 五组蒙版。
- 分组交汇处增加双向端点截断：后续组最终外轮廓进入已完成区域 `24px` 范围时停止，已完成组接触边进入最终外轮廓 `24px` 范围时同样停止，避免两类线段在交汇拐角伸入贴纸空白区域。
- `DraggablePieceState` 增加 `IsOnTray`。Piece 首次离开托盘后保持棋盘目标缩放，未吸附时停在桌面并限制在背景可见范围内，后续可以从该位置再次拿起。
- Piece 首次离开托盘时继续触发现有后序 X 补位；桌面 Piece 不再参与托盘计数、布局或后续补位。未吸附松手时空托盘恢复为桌面 Piece 的回收目标。
- Piece 与托盘水平方向有交集且垂直重叠达到 Piece 当前高度的 `50%` 时，松手后切回 `TrayScale` 并与其他托盘 Piece 按编号自动重排。
- CardBag Prefab 生成器的默认外层背景路径从已删除的 `BgCardBoard.png` 更新为 `BgCardBoard1.png`；现有 Prefab 因资源改名保留原 Meta/GUID，无需重新生成。
- GameScene 实例化 CardBag 后读取 `GameSettings/Runtime` 的 `UsableOption3`：关闭时从 `UI/BasicUI/BgCardBoard1.png` 加载背景，打开时加载 `BgCardBoard2.png`。运行时创建的 Sprite 和 Texture 在离场时释放。
- `GameSettingsData.IsHighContrastEnabled` 明确表达 `UsableOption3` 的含义；新建设置显式将高对比设为 `false`。现有 `Toggle3` 绑定与 SQLite 保存链路保持不变。
- `CardBag022` 的 Preview 是加入青色分割线并经过调色板量化的图片，Piece 原始 RGB 在 Preview 中不存在，首片最佳匹配仅 `23.24%`；生成器现会改用保留完整 RGB 的 GameBoard 定位。
- GameBoard 回退首片达到至少 `99.5%` 且位置唯一后，同一卡包后续 Piece 统一使用 GameBoard；日志逐片标明 `Preview` 或 `GameBoard fallback`。颜色索引同时记录唯一颜色的像素下标，唯一锚点直接定位而不再扫描整张参考图。
- 新增通用 Unity 批处理入口 `GenerateCardBagFromCommandLine`，通过 `-cardBagId <number>` 生成指定卡包。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Model/LocalDataStore.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Resources/Configs/CardPacks.csv`（用户修改）
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameCommonUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Scripts/Editor/BuildSync.cs`
- `Assets/UI/GameScene/GameImgArrow.png`（用户提供）
- `Assets/Resources/Generated/PuzzleOutlines/CardBag017/Group02.png` 至 `Group05.png`
- `Assets/Resources/CardBagPrefabs/CardBag002.prefab`、`003`、`004`、`005`、`006`、`008`、`010`、`011`、`012`、`013`、`014`
- `Assets/Resources/CardBagPrefabs/CardBag022.prefab`
- `Assets/UI/BasicUI/ImgHand_1.png`、`ImgHand_2.png`、`ImgHand_3.png`（用户新增）
- `Assets/UI/BasicUI/BgCardBoard1.png`、`BgCardBoard2.png`（用户提供）
- `specs/spec-driven-development.md`
- `D:\360极速浏览器X下载\剩下关卡的切图\CardBag015`、`016`、`018`、`019`、`020`、`021`（外部美术资源整理）
- `Documents/CURRENT_TASK.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 新手引导不是提示按钮使用，不设置 `_wasHintUsed`，不会取消本局“未使用提示”积分加成。
- 引导期间只隐藏当前组烘焙描边；正确放置引导 Piece 后立即按玩家设置恢复。
- 已经放置过 Piece、历史已完成 `CardBag001` 或其他 BagId 均不补播该引导，避免打断已有进度。
- 引导完成记录复用 `AppRecords`，不新增表、不需要删除现有 `LocalData.db`。
- 当前拼图会话与 `CardPackLifecycleState` 分开：生命周期表达历史权益和完成状态，会话表达本局是否可继续及已放置 Piece。
- 空会话同样持久化，确保进入游戏后尚未放置 Piece 就退出时仍按继续状态处理。
- 只保存正确吸附 Piece 的编号；托盘和桌面 Piece 的位置不保存，下次进入按现有托盘规则重新生成。
- 拍照资格取决于是否历史完成，不取决于是否进入过游戏或是否存在当前拼图会话。
- `BoardScale` 是只读资源配置，不写入 SQLite、JSON 或 `PlayerPrefs`。
- 缺少整个卡包配置时 GameScene 记录警告并回退 `1`；CSV 中存在的卡包行必须提供合法正数。
- 缩放 CardBag 根节点而不是单独修改 GameBoard 图片，确保棋盘、Piece 槽位和描边保持同一坐标系。
- 不修改托盘布局、`20px` 间距和 `90%` 最大高度规则；这些规则先算出旧托盘比例，再与配置后的棋盘目标比例取较小值。
- 光标资源沿用 `BasicUI` 的 Editor/Player 磁盘加载和构建同步规则，不额外复制到 `Resources`；资源缺失时回退常规或系统光标并记录警告。
- 三张光标宽高比不同，固定使用软件光标模式，不使用可能受平台固定光标尺寸约束的 `CursorMode.Auto`。
- 同一描边像素只由最早需要它的分组认领；后续分组不得再次显示该像素，避免组交界处沿已完成区域多画一段。
- 接触边和最终外轮廓不能只按搜索半径判断归属；目标组还必须位于该边界的正确法线方向，切线方向的邻近不能生成描边。
- 最终外轮廓与已完成组接触边在交汇处分别保留 `24px` 截止范围，不要求两类烘焙线段直接相连。
- 未正确吸附不再属于“失败回托盘”；Piece 一旦离开托盘就成为桌面 Piece，保持 `DragScale` 和松手位置。桌面位置只约束在背景可见范围内。
- 托盘自动补位只移动 `IsOnTray=true` 的后序 Piece；已经放在桌面的 Piece 必须保持位置不变。
- 回收托盘判定使用运行时 Renderer Bounds：垂直重叠至少 `50%` 且水平重叠大于 `0`；正确槽位吸附优先于托盘回收。
- 自动生成器不全局降低匹配要求。每张参考图仍执行现有 `98%` 常规阈值与“至少 `90%` 且精确 RGB 锚点唯一”的受控低分规则；Preview 校验失败后才尝试 GameBoard，两边都失败时同时报告原因。
- 高对比背景在 CardBag 实例化后替换根节点 `Image.sprite`，不制作两套 Prefab、不改变棋盘和 Piece 坐标。设置字段已存在，因此不修改 SQLite 表结构，也不要求删除本地数据。

## 验证

- 剩余关卡切图迁移前无覆盖预检通过：六个目录共 `201` 张 smooth 图片和 `6` 张标题图，源文件完整且目标均不存在。
- 迁移后结构验证通过：六包均包含 `CardBagXXX.png`、`GameBoard.png`、`BoardTitle.png`；Piece 数量分别为 `41、28、35、26、31、34`，合计 `195`。
- 根目录没有残留数字目录、`PackTitleXXX.png` 或其他文件；六包没有残留 `smooth`、`preview.png`、`background_base.png` 或非预期文件。
- `dotnet build Puffies.sln --no-restore`（首次 Piece 放置新手引导）：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅有既有 LF/CRLF 转换提示。
- `BgCardBoard1.png` 与 `BgCardBoard2.png` 均为 `512 x 512`，可在同一 CardBag 根节点 RectTransform 上直接替换。
- `dotnet build Puffies.sln --no-restore`（高对比棋盘背景接入后）：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`（高对比棋盘背景接入后）：通过，仅有既有 LF/CRLF 转换提示。
- 已确认箭头源图为 `122 x 271`，有效像素范围基本覆盖整张图片，方向为向上。
- 尚未完成 Unity Play Mode 中箭头弯曲形态、目标副本层级、托盘遮罩和错误放置恢复的视觉验证。
- `dotnet build Puffies.sln --no-restore`（CardBag 改用 Preview 匹配 Piece）：三个程序集成功，`0` 警告、`0` 错误。
- 已确认 `CardBag001` 的 Preview 与 GameBoard 均为 `1300 x 1518`；Preview 是完整图，GameBoard 包含透明挖洞，符合新的定位与运行时职责划分。
- `dotnet build Puffies.sln --no-restore`（未历史完成卡包隐藏相机按钮）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（拼图进度持久化与玩/重玩判断）：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅有既有 LF/CRLF 转换提示。
- 本次新增 SQLite 表。测试前需关闭 Unity 并删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`；不影响任务 JSON，无需删除 `LocalData.json`。
- 当前 22 行 `CardPacks.csv` 均提供了正数 `BoardScale`。
- `dotnet build Puffies.sln --no-restore`：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：代码通过，仅有既有 LF/CRLF 转换提示。
- 已确认代码调用顺序为应用 `BoardScale` -> 创建当前组并计算居中位置 -> 播放入场动画；动画现在保留计算后的当前位置。
- `dotnet build Puffies.sln --no-restore`（逐 Piece 槽位尺寸修复后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（托盘自动补位后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（切组旧 Piece 帧内隐藏后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（托盘增量 X 补位后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（全局自定义鼠标接入后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（光标真实纹理尺寸修复后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（已放置 Piece 切换为 Prefab Image 后）：三个程序集成功，`0` 警告、`0` 错误。
- `CardBag017` 五张最终描边蒙版逐像素交叉统计：任意两组之间的非透明像素重叠均为 `0`。
- 最终 `Group02` 与仓库原蒙版逐像素对比：删除 209 个像素、新增 `0` 个；删除范围为 `(17,32)` 至 `(665,717)`，其中上方 `400px` 范围删除 83 个。
- `CardBag009` 五张最终描边蒙版逐像素交叉统计：任意两组之间的非透明像素最大重叠为 `0`。
- `dotnet build Puffies.sln --no-restore`（分阶段描边去重后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（Piece 桌面放置后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（桌面 Piece 50% 托盘回收后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（CardBag 唯一锚点匹配修复后）：三个程序集成功，`0` 警告、`0` 错误。
- Unity Editor 实际批量生成日志：`generated=11, failed=0`；原失败的 11 个卡包全部创建 Prefab。仅 `CardBag002/piece_013.png` 使用唯一锚点回退，边缘内缩后匹配率由 `95.25%` 提升到 `97.22%`。
- Unity `2022.3.62f2c1` 批处理实际生成 `CardBag022` 成功：115 张 Piece 均使用 `GameBoard fallback` 达到 `100.00%` 唯一匹配，输出 `created Assets/Resources/CardBagPrefabs/CardBag022.prefab with 115 pieces`，进程返回码为 `0`。
- `CardBag022/piece_001.png` 定位为纹理底部原点 `(5,3612)`；按 `2600 x 3920` 棋盘和 `395 x 303` Piece 换算后，设计图左上原点仍为 `(5,5)`。重新生成保留了原有 115 个 Piece 布局，并按当前源资源新增 `BoardTitle` 节点。
- `dotnet build Puffies.sln --no-restore`（CardBag022 Preview/GameBoard 回退与唯一锚点索引后）：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅有既有 LF/CRLF 转换提示。
- 尚未完成 BoardScale 大于 1、小于 1 和等于 1 三种卡包的 Play Mode 视觉验证。

## 下一步

### 本次优先

1. 用户确认后，将六个已标准化 CardBag 从下载目录导入 `Assets/UI/CardBags/`；导入前检查工程中同名目录，避免覆盖现有资源。
2. 导入完成后使用现有批量生成器创建 Prefab，再按实际玩法完成 Piece 分组命名和描边烘焙。
3. 在 MainScene 设置页确认 `Toggle3` 首次为关闭；关闭时进入任意 CardBag 验证外层背景为较浅的 `BgCardBoard1.png`，返回后打开高对比再进入，确认切换为较深的 `BgCardBoard2.png`；重启后确认选择保持。
4. 在 Prefab Mode 抽查 `CardBag002` 和最小碎片较多的 `CardBag006`，确认 Piece 槽位与 Preview 位置一致；随后按实际玩法对顺序节点完成分组命名并烘焙描边。
5. 在 Unity 执行 `Assets -> Refresh`，进入一个从未开始的 `CardBag001`，确认入场完成后才显示引导。
6. 确认托盘整体变暗、指定 Piece 保持正常亮度、棋盘槽位显示绿色跑马灯，箭头从 Piece 顶部沿弯曲路径逐步画向槽位。
7. 尝试点击其他 Piece，确认不能拿起；将目标 Piece 放错到托盘或桌面，确认引导按当前位置恢复。
8. 将目标 Piece 正确吸附，确认立即切换到下一 Piece；完成分组后继续提示下一组，整包完成时引导结束并恢复描边。
9. 整包完成后重新进入，确认不再显示引导；已有部分进度的 `CardBag001` 从当前未完成 Piece 继续，其他 BagId 和历史完成的 `CardBag001` 不触发。

### 既有回归

1. 测试 CardBag001（`BoardScale=1.48`），确认棋盘放大且入场结束后仍在可用棋盘区域居中，托盘 Piece 初始尺寸不变、拿起后放大并能准确吸附。
2. 测试 CardBag002（`BoardScale=0.72`），确认棋盘缩小且入场结束后仍在可用棋盘区域居中，拿起 Piece 同比例缩小且吸附准确。
3. 测试 `BoardScale=1` 的卡包，确认行为与修改前一致。
4. 完成第一组并创建第二组，确认棋盘居中、描边、已放置 Piece 和新组托盘布局没有偏移。
5. 验证每个 Piece 按下拿起时只会保持尺寸或放大，不会缩小。
6. 在当前组尚未完成时检查已吸附 Piece 的接缝，确认其与切换下一组后 Prefab 原始 Piece 的接缝宽度一致。
7. 从托盘中间取一块并成功吸附，确认后续 Piece 向前补位、前序 Piece 不跳动，且补位后的 Piece 放置失败会返回新位置。
8. 放置当前组最后一块，确认切换下一组时已完成 Piece 不再发生 Y 轴抖动或短暂重影。
9. 第一片先选择托盘队尾 Piece，确认吸附后前序 Piece 的 X、Y 均完全不刷新；再选择中间 Piece，确认只有后序 Piece 沿 X 前移且 Y 不抖动。
10. 依次打开 LoadingScene、MainScene、GameScene、AchieveScene 和 RankScene，确认常规图标均为 `ImgHand_1`；在 GameScene 验证悬停 `ImgHand_2`、按住拖拽 `ImgHand_3`、松开和离场恢复 `ImgHand_1`。
11. 重新进入 `CardBag009` 第二组，确认戴帽子贴纸顶部原截图位置不再多画，并检查左侧外轮廓与下方接触边在交汇处自然结束。
12. 从托盘拿起 Piece 并在桌面松手，确认其保持棋盘目标尺寸和松手位置、不会越出背景；再次拿起可以继续移动或正确吸附。
13. 依次将中间、最后一个托盘 Piece 放到桌面，确认只补位仍在托盘的后序 Piece，桌面 Piece 不移动，未吸附松手后空托盘恢复为回收目标。
14. 将桌面 Piece 拖回黑色底板，分别以低于和达到自身高度 `50%` 的重叠量松手；确认前者仍停在桌面，后者缩回托盘尺寸并按编号自动重排。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；下载目录中的六个剩余 CardBag 已完成标准化但尚未导入 Unity，下一步需先确认是否复制到 `Assets/UI/CardBags/`，再运行批量 Prefab 生成流程。
