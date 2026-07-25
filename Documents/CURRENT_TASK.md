# 当前任务

- 任务：GameScene 棋盘交互与全局自定义鼠标
- 状态：代码已实现，等待 Unity Play Mode 视觉验证
- 更新时间：2026-07-25

## 用户意图

- `Resources/Configs/CardPacks.csv` 新增 `BoardScale` 列。
- 进入 GameScene 后，根据当前 BagId 读取 `BoardScale`。
- 棋盘、槽位和创建出来的贴图碎片使用该缩放比。
- Piece 初始位于下方黑色托盘时，生成比例取“配置后的棋盘目标比例”和“修改前黑色托盘规则算出的比例”中的较小值。
- 拿起 Piece 后使用配置后的棋盘目标比例，因此尺寸只能保持不变或放大，不能在按下时缩小；放置失败后恢复生成时的托盘比例。
- 所有游戏页面使用 `ImgHand_1.png` 作为常规鼠标图标。
- GameScene 鼠标悬停在可拖贴纸上时切换为 `ImgHand_2.png`；按下左键并拿起贴纸后切换为 `ImgHand_3.png`。

## 工作记录

- `CardPackConfigData` 新增 `BoardScale` 浮点字段。
- `CsvRow` 新增使用 `InvariantCulture` 的浮点读取，避免系统小数点区域设置影响 CSV。
- `CardPacks.csv/BoardScale` 现在是必填正数；缺失、无法解析或小于等于零的行视为无效配置。
- GameScene 初始化时按 BagId 读取配置，并将 `BoardScale` 乘到 CardBag Prefab 原始根节点缩放；`GameBoard`、Piece 槽位、描边坐标和吸附位置随层级统一缩放。
- 棋盘上的 SpriteRenderer Piece 比例继续从棋盘实际显示尺寸计算，因此自动包含 `BoardScale`。
- 托盘比例计算时除去 `BoardScale`，继续使用修改前的棋盘匹配比例和托盘最大高度限制。
- 创建 Piece 时一次性保存包含 `BoardScale` 的 `DragScale`；拿起时直接使用保存值，不再根据可能已变化的 Canvas/棋盘状态二次计算。成功放置保持该比例，失败则恢复 `TrayScale`。
- 每个 Piece 创建时直接执行 `TrayScale = Min(旧托盘规则比例, DragScale)`；不再使用整组宽高或是否超框作为分支条件。
- 棋盘先应用 `BoardScale`，再按照当前组缩放后的实际屏幕范围居中；入场动画以该居中结果作为终点，不再恢复 Prefab 原始锚点。
- 当前组使用的运行时 `SpriteRenderer` 不再按整张棋盘的单一比例推算尺寸；每个 Piece 分别读取对应槽位缩放后的实际世界宽高，计算 X/Y `DragScale`，确保吸附后与 Prefab 槽位显示尺寸一致，避免切组前缝隙偏大、切组后才恢复正常。
- 成功吸附 Piece 后不再完整刷新托盘布局；仅将编号位于其后的未放置 Piece 沿 X 轴前移“被取走 Piece 的托盘宽度 + 间距”，并更新失败回退 X。所有剩余 Piece 的 Y 和缩放保持不变；先拼队尾 Piece 时其他 Piece 完全不刷新。
- 最后一块吸附并切组时，上一组运行时 Piece 在调用延迟 `Destroy` 前先立即停用；下一组重新居中和恢复 Prefab 原始 Piece 时不再与旧 Renderer 重叠，避免已完成 Piece 在 Y 轴抖动一帧。结算清理使用同一规则。
- `GameCursorUtility` 在场景加载前从 `UI/BasicUI` 加载三张运行时可读 PNG 并设置系统光标；常规、贴纸悬停和贴纸抓取分别使用 `ImgHand_1/2/3.png`。GameScene 的悬停与按下复用同一个命中方法，离开 GameScene 时恢复常规图标。

## 修改文件

- `Assets/Resources/Configs/CardPacks.csv`（用户修改）
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameCommonUtility.cs`
- `Assets/UI/BasicUI/ImgHand_1.png`、`ImgHand_2.png`、`ImgHand_3.png`（用户新增）
- `Documents/CURRENT_TASK.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- `BoardScale` 是只读资源配置，不写入 SQLite、JSON 或 `PlayerPrefs`。
- 缺少整个卡包配置时 GameScene 记录警告并回退 `1`；CSV 中存在的卡包行必须提供合法正数。
- 缩放 CardBag 根节点而不是单独修改 GameBoard 图片，确保棋盘、Piece 槽位和描边保持同一坐标系。
- 不修改托盘布局、`20px` 间距和 `90%` 最大高度规则；这些规则先算出旧托盘比例，再与配置后的棋盘目标比例取较小值。
- 光标资源沿用 `BasicUI` 的 Editor/Player 磁盘加载和构建同步规则，不额外复制到 `Resources`；资源缺失时回退常规或系统光标并记录警告。

## 验证

- 当前 22 行 `CardPacks.csv` 均提供了正数 `BoardScale`。
- `dotnet build Puffies.sln --no-restore`：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：代码通过，仅有既有 LF/CRLF 转换提示。
- 已确认代码调用顺序为应用 `BoardScale` -> 创建当前组并计算居中位置 -> 播放入场动画；动画现在保留计算后的当前位置。
- `dotnet build Puffies.sln --no-restore`（逐 Piece 槽位尺寸修复后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（托盘自动补位后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（切组旧 Piece 帧内隐藏后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（托盘增量 X 补位后）：三个程序集成功，`0` 警告、`0` 错误。
- `dotnet build Puffies.sln --no-restore`（全局自定义鼠标接入后）：三个程序集成功，`0` 警告、`0` 错误。
- 尚未完成 BoardScale 大于 1、小于 1 和等于 1 三种卡包的 Play Mode 视觉验证。
- 不涉及持久化结构变化，无需删除 `LocalData.db` 或 `LocalData.json`。

## 下一步

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

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；`BoardScale` 解析和运行时缩放已实现，下一步是在 Unity Play Mode 验证大于 1、小于 1 和等于 1 的卡包。
