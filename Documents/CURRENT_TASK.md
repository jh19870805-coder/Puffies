# 当前任务

- 任务：修正托盘 Piece 原尺寸与 90% 高度缩放
- 状态：根因已修正并完成编译，等待 Play Mode 目视验证
- 更新时间：2026-08-20

## 用户意图

- Piece 原始高度不超过托盘高度 `90%` 时，在托盘上保持原始显示高度。
- Piece 原始高度超过托盘高度 `90%` 时，按宽高比等比缩小到托盘高度 `90%`。
- Piece 在托盘生成和回收后的 `TrayScale` 最大只能为 `1`，不得通过相机补偿放大。
- 托盘缩放不能随活动组相机适配或棋盘 `BoardScale` 改变。
- Piece 离开托盘进入拖拽、桌面或棋盘状态后，棋盘目标缩放最大轴不能超过 `1`，不得放大。

## 工作记录

- 原实现只在 Piece 较小时返回 `localScale=1`，但活动组相机会自动改变正交尺寸；相机拉远后，`localScale=1` 的 Sprite 在屏幕上仍会缩小，因此实际没有保持原尺寸。
- 新实现使用 Sprite 原始设计高度与 `PieceBoard.rect.height * 90%` 比较：未超过时直接使用 `TrayScale=1`，超过时按高度比等比缩小。
- 托盘比例不再补偿相机缩放，计算结果统一限制为 `<=1`；活动组相机适配可以改变其屏幕显示大小，但不能把 Piece 自身放大。
- 首次入场与切组入场不再对托盘 Piece 临时乘 `1.12` 或 `1.08`；位移、旋转和淡入保留，整个入场过程均使用最终 `TrayScale`。
- 棋盘吸附继续使用独立 `DragScale`，托盘缩放不继承 `BoardScale`，现有 X 间距、上下居中和重排动画不变。
- `CalculatePieceScaleOnBoard` 的屏幕匹配、棋盘比例回退和异常尺寸回退现已统一经过最大轴 `<=1` 的等比钳制；初始化、拿起刷新、占用探针、错误回弹和吸附动画共用该结果。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- “原尺寸”定义为设计分辨率下的显示高度，不定义为固定 `localScale=1`。
- 90% 判断和缩放比例均使用稳定的设计尺寸，托盘 `TrayScale` 不使用当前相机下的世界高度补偿。
- 保留没有 `PieceBoard` 时的旧世界高度回退，避免影响兼容的 `PieceBgRenderer` 路径。
- 棋盘 Scale 超过 `1` 时按最大轴统一缩小，不分别裁切 X/Y，避免改变 Piece 宽高比；`BoardScale` 和已放置 UGUI 层级不在本次调整范围内。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- 公式样例：托盘设计高度 `350`、上限 `315`；原高 `285` 的 Piece 目标仍为 `285`，原高 `395` 的 Piece 目标为 `315`。
- 公式结果始终不大于 `1`：原高 `285` 的 Piece 为 `1`，原高 `395` 的 Piece 为 `315/395≈0.797`。
- 棋盘 Scale 样例：`(1.25,1.25)` 变为 `(1,1)`，`(1.2,0.8)` 等比变为 `(1,0.667)`，`(0.75,0.8)` 保持不变。
- 当前机器未运行 Unity，仍需 Play Mode 目视确认实际 Sprite 和托盘画面。

## 关联待办：棋盘与托盘间距

- 棋盘自动适配后，`GameBoard` 底部到可见托盘顶部的间距已限制为背景可视高度 `10%`；代码和编译验证通过，仍需在截图对应关卡目视确认比例。

## 关联待办：卡包编号迁移

- 已核对资源迁移主体：旧 007 -> 新 005、旧 005 -> 新 006、旧 006 -> 新 007、旧 010 -> 新 023；新 010 为重新制作内容。
- 当前源碎片数和 Prefab 节点数一致：005=19、006=29、007=35、010=25、023=28；Prefab 分组数分别为 4、4、5、2、5，均使用正式 `PieceGGII` 命名。
- `CardPacks.csv` 当前仍只有 22 行，需补齐 005=`1/19/0.75`、006=`2/29/0.78`、007=`3/35/1.10`、010=`2/25/0.78`，并新增 `23,23,2,28,2,0.78,,1`。
- CardBag006 描边目录比 Prefab 多一套无效 `Group05`，需要清理；其他本轮新烘焙描边保留并在 Unity 中检查。
- `CardBag005/BoardTitle.png` 与 `美术切图/游戏内包头/PackTitle005.png` 内容不同，修改前需确认采用哪一份。
- 配置修改后，测试前删除 `LocalData.db` 与 `LocalData.json`，按开发阶段规则重新初始化本地数据。

## 本机维护同步

- 当前设备已经安装每周日 `03:00` 运行的 `Puffies Project Maintenance`，脚本路径为 `E:\MyWork\UnityProjects\Puffies\ProjectMaintenance.ps1`。
- 最近审计发现 Git 松散对象达到维护阈值；检查时 Unity 正在运行，因此未执行即时清理。

## 下一步

1. 在 CardBag022 或其他相机明显拉远的组中，同时观察一个原高小于 `315` 和一个大于 `315` 的 Piece。
2. 确认小 Piece 的 `TrayScale=1`，大 Piece 只会缩小到托盘 `90%`，所有托盘 Piece 的 Scale 均不超过 `1` 且上下居中。
3. 拿起 Piece 后确认棋盘最大缩放轴也不超过 `1`；放到桌面、错误棋盘位置并回托盘，确认棋盘 Scale 与托盘 Scale 正确切换。
4. 顺带确认棋盘与托盘的 `10%` 间距。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；先在 Play Mode 验证托盘上小 Piece 保持原始设计高度、大 Piece 限制为托盘高度 `90%`，再检查棋盘与托盘的 `10%` 最大间距。随后处理本文件记录的卡包编号迁移待办，不要回滚现有描边资源。
