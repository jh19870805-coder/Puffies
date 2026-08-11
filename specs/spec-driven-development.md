# Spec Driven Development

## 2026-08-11 - 黑色托盘 Piece 居中与补位缓动

### 需求

**用户故事：** 作为玩家，我希望当前分组的 Piece 在黑色托盘中整齐、稳定地排列，并在拿起 Piece 后平滑补位，以便不同卡包的托盘表现一致且易于追踪。

1. WHEN 创建当前分组的托盘 Piece THEN 系统 SHALL 将 Piece 的实际渲染边界在黑色托盘内上下居中。
2. WHEN 排列任意卡包的托盘 Piece THEN 系统 SHALL 使用统一的固定水平间距，不因卡包或 Piece 尺寸改变间距值。
3. WHEN 玩家从托盘拿起一个 Piece AND 该 Piece 后方没有仍在托盘的同组 Piece THEN 系统 SHALL 不刷新其他 Piece 的位置。
4. WHEN 玩家从托盘拿起一个 Piece AND 其后方存在仍在托盘的同组 Piece THEN 系统 SHALL 仅将这些后序 Piece 向前补位，前序 Piece、桌面 Piece、Y 坐标和缩放保持不变。
5. WHEN 托盘 Piece 需要补位或回收重排 THEN 系统 SHALL 使用 `0.5s` 缓动移动到目标位置，不得瞬移。
6. WHEN Piece 因错误放置返回原托盘位置 THEN 系统 SHALL 同步恢复其他托盘 Piece 的固定间距布局。

### 设计

- 保留现有 `DraggableHorizontalSpacingPixels=20`，将其作为所有卡包共用的设计像素间距。
- 初始布局使用 `SpriteRenderer.bounds.center` 校正最终世界坐标，直接对齐实际渲染边界中心与托盘中心。
- 在 `TryBeginDrag` 中触发后序 Piece 补位；队尾没有移动目标时不启动协程。
- 新增单一托盘 Piece 重排协程，使用 `Time.unscaledDeltaTime` 和 `Mathf.SmoothStep` 在 `0.5s` 内只插值世界 X 坐标。
- 重排期间暂时禁止开始下一次拖拽；当前已拿起的 Piece 仍可继续移动和松手。
- Piece 放回托盘时按编号重新计算固定间距目标；初始建组仍即时布局，不播放补位动画。

### 任务

- [x] 1. 修正初始托盘 Piece 的实际渲染边界垂直居中。
- [x] 2. 将拿起后的后序补位改为统一 `0.5s` 缓动，并保证队尾不刷新。
- [x] 3. 将托盘回收重排接入相同固定间距与缓动逻辑。
- [x] 4. 更新长期规则和当前任务记录。
- [ ] 5. 编译并验证初始布局、队尾、非队尾和回收分支。

### 当前验证

- 静态检查确认所有布局入口共用 `DraggableHorizontalSpacingPixels=20`。
- 拿起补位只收集编号大于当前 Piece、仍在托盘且不是当前拖拽对象的状态；队尾目标列表为空时不启动协程。
- 初始布局使用 `SpriteRenderer.bounds.center` 计算实际渲染中心偏移，托盘重排目标继续保持同一 Y 和缩放。
- 运行时与编辑器 `dotnet build` 顺序通过，均为 `0` 警告、`0` 错误。
- Unity `2022.3.62f2c1` 无界面编译和完整资源导入通过，返回码为 `0`，日志中无 C# 错误或警告。
- `git diff --check` 通过；Unity 未修改场景、Prefab 或资源。
- 待在 Play Mode 分别拿起队首、中间、队尾 Piece，并测试错误返回和桌面回收的实际视觉节奏。

## 2026-08-07 - 卡包尺寸分档更新

### 需求

**用户故事：** 作为策划，我希望卡包尺寸按新的贴纸数量区间自动确定，以便卡包难度、尺寸展示、基础分和任务尺寸筛选使用统一定义。

1. WHEN 贴纸数量小于 `20` THEN 系统 SHALL 将卡包定义为 `XS`。
2. WHEN 贴纸数量介于 `20..30`（含边界）THEN 系统 SHALL 将卡包定义为 `S`。
3. WHEN 贴纸数量介于 `31..55`（含边界）THEN 系统 SHALL 将卡包定义为 `M`。
4. WHEN 贴纸数量介于 `56..85`（含边界）THEN 系统 SHALL 将卡包定义为 `L`。
5. WHEN 贴纸数量介于 `86..125`（含边界）THEN 系统 SHALL 将卡包定义为 `XL`。
6. WHEN 贴纸数量介于 `126..170`（含边界）THEN 系统 SHALL 将卡包定义为 `XXL`。
7. WHEN 贴纸数量大于 `170` THEN 系统 SHALL 将卡包定义为 `XXXL`。
8. WHEN 配置更新工具处理 `AutoUpdate=1` 的现有卡包 THEN 系统 SHALL 按新尺寸重写 `PackSize`，并继续使用既有尺寸到 `BoardScale` 的映射更新棋盘缩放。
9. WHEN 验证尺寸边界 THEN 系统 SHALL 覆盖 `19/20/30/31/55/56/85/86/125/126/170/171`，确保区间无断档或重叠。

### 设计

- 修改唯一尺寸判定入口 `CardBagPrefabGeneratorEditor.ResolvePackSize`，依次使用 `<20`、`<31`、`<56`、`<86`、`<126`、`<171` 判断七档尺寸。
- 不修改 `CardPackSize` 枚举值、尺寸图标映射、基础分表或既有 `ResolveBoardScale` 映射。
- 根据 `CardPacks.csv/StickerCount` 立即更新当前 `AutoUpdate=1` 行，使现有配置与后续工具生成结果一致。
- 本次不修改 SQLite 结构或 JSON 结构；已持久化卡包记录的运行时尺寸由配置读取，不需要删除本地数据。

### 任务

- [x] 1. 更新尺寸判定函数并覆盖全部新边界。
- [x] 2. 按新规则重算现有 `CardPacks.csv` 的尺寸和棋盘缩放。
- [x] 3. 更新稳定项目规则、策划记录和当前任务记录。
- [x] 4. 静态核对边界映射并使用 Unity 编译验证。

### 当前验证

- 22 个 `CardBagNNN` 源目录的标准 Piece 数量与 `CardPacks.csv/StickerCount` 全部一致。
- 22 行 `AutoUpdate=1` 配置按新分档和既有 `BoardScale` 映射检查，零不一致。
- 边界映射结果为：`19=XS`、`20/30=S`、`31/55=M`、`56/85=L`、`86/125=XL`、`126/170=XXL`、`171=XXXL`。
- `git diff --check` 通过。
- 当前 Unity `2022.3.62f2c1` 实例刷新后重新生成 `Assembly-CSharp-Editor.dll`；Editor.log 最近记录中 C# 错误和警告均为 `0`，无配置导入异常。
- Unity 保持打开，未修改场景、Prefab 或其他资源。

## 2026-08-07 - 三类任务配置与生成规则重构

### 需求

**用户故事：** 作为策划，我希望任务系统只使用三类明确任务，并按配置控制目标参数的递进或随机方式，以便任务节奏稳定且相邻任务不重复同一类型。

1. WHEN 生成任务类型 1 THEN 系统 SHALL 使用“完成任意拼图包，收集 N 分”，并从 `150|200|250|300` 按顺序取值，取完后循环回 `150`。
2. WHEN 生成任务类型 2 THEN 系统 SHALL 使用“从任意拼图包中收集 N 个贴纸”，并从 `45|60|80` 按顺序取值，取完后循环回 `45`。
3. WHEN 生成任务类型 3 THEN 系统 SHALL 使用“完成 N 个 S/M 尺寸的拼图包”，数量从 `2|3` 随机，尺寸从当前可玩卡包与 `S|M` 的交集中随机，两项选择互相独立。
4. WHEN 生成下一任务 THEN 系统 SHALL 排除与当前任务相同的 `TaskType`，而不是只排除相同 `TemplateId`。
5. IF 任务类型 3 当前不存在可玩的 `S` 或 `M` 卡包 THEN 系统 SHALL 将该模板视为不可用，并从另外两类任务中选择。
6. WHEN 任务 1 或任务 2 的目标被选用 THEN 系统 SHALL 分别持久化各自的递进游标，两个序列互不影响。
7. WHEN 任务配置更新 THEN 系统 SHALL 保持既有奖励、章节范围、重玩计数和结算贡献规则不变。
8. WHEN 积分任务产生超额分数 THEN 系统 SHALL 持久化超额值，并在经过其他类型任务后生成下一个积分任务时恢复为初始进度。

### 设计

- `TaskConfig.csv` 收敛为三个启用模板：类型 1 使用任意尺寸和目标池 `150|200|250|300`；类型 2 使用任意尺寸和目标池 `45|60|80`；类型 3 使用指定尺寸池 `2|3`（`S|M`）和目标池 `2|3`。
- `TaskProgressData.ScoreTargetCycleIndex` 继续保存任务 1 游标，新增 `StickerTargetCycleIndex` 保存任务 2 游标；旧 JSON 缺少新字段时由 `JsonUtility` 初始化为 `0`。
- `TaskProgressData.PendingScoreCarryOver` 保存积分任务超额值；生成非积分任务时继续保留，生成下一个积分任务时转入 `CurrentCompleteValue` 并清零。
- `TryCreateTaskInstance` 根据当前任务的 `TaskType` 排除同类型候选。首个任务的当前类型为 `None`，不执行排除。
- `ChooseTargetValue` 对任务 1、2调用同一个顺序取值函数，对任务 3继续使用随机取值。
- 任务 3 继续通过 `TryChooseEligiblePackSize` 与当前可玩卡包求交集，避免生成无法完成的尺寸任务。
- 旧配置产生的当前任务实例不会自动改写；验证新规则前删除 `persistentDataPath/LocalData.json`，SQLite 数据无需删除。

### 任务

- [x] 1. 更新 `TaskConfig.csv` 为三条新任务模板。
- [x] 2. 为贴纸任务增加独立持久化递进游标，并实现通用循环取值。
- [x] 3. 将下一任务过滤规则从相同模板改为相同任务类型。
- [x] 4. 更新任务 UI 文案和稳定项目规则文档。
- [x] 5. 使用 Unity 编译并验证配置加载、任务候选和递进逻辑。

### 当前验证

- `TaskConfig.csv` 可按 14 列结构读取，共 3 行，`TaskType` 恰好为 `1、2、3`。
- 已确认 `CardPackSize.S=2`、`CardPackSize.M=3`，任务 3 的 `SizePool=2|3` 映射正确。
- 搜索确认运行代码和稳定规则中不再存在 `LastTemplateId`、旧积分目标池或按旧模板过滤的逻辑。
- `git diff --check` 通过。
- Unity `2022.3.62f2c1` 无界面编译通过，返回码为 `0`，日志中无 C# 错误或警告。
- Unity 未修改场景、Prefab 或其他资源文件。

## 2026-07-29 - 剩余关卡切图标准化

### 需求

**用户故事：** 作为关卡资源制作者，我希望美术交付的剩余关卡切图统一为 CardBag 生成器可识别的目录和文件名，以便后续直接导入工程并批量生成 Prefab。

1. WHEN 扫描到名称为关卡数字的一级目录 THEN 系统 SHALL 将其重命名为 `CardBagXXX`，其中 `XXX` 为三位十进制编号，不足三位时左侧补 `0`。
2. WHEN 处理 `CardBagXXX/preview.png` THEN 系统 SHALL 将其重命名为 `CardBagXXX.png`。
3. WHEN 处理 `CardBagXXX/smooth/` THEN 系统 SHALL 将其中全部图片移动到 `CardBagXXX/` 一级目录。
4. WHEN 移动 `smooth/background_base.png` THEN 系统 SHALL 将其目标名称设置为 `GameBoard.png`。
5. WHEN 根目录存在 `PackTitleXXX.png` THEN 系统 SHALL 将其移动到编号相同的 `CardBagXXX/` 并重命名为 `BoardTitle.png`。
6. IF 任一源文件缺失、编号无法对应或目标路径已存在 THEN 系统 SHALL 在修改前终止整批操作，不覆盖现有文件。
7. WHEN 迁移成功 THEN 下载根目录 SHALL 保存各包的 `CardBagXXX.png`；每个 `CardBagXXX` 目录 SHALL 只保留 `GameBoard.png`、`BoardTitle.png` 和原 `smooth` 中的 `piece_###.png`；空 `smooth` 目录 SHALL 被删除。

### 设计

- 输入根目录固定为 `D:\360极速浏览器X下载\剩下关卡的切图`。
- 本批编号为 `015、016、018、019、020、021`。
- 使用 PowerShell 原生 `Move-Item` 和 `Remove-Item` 在同一文件系统内处理，不跨 Shell 传递路径。
- 写入前构造每个源到目标的完整映射，并检查源存在、目标不存在、标题编号唯一且完整。
- 先整理每个数字目录内部，再将一级目录重命名为 `CardBagXXX`；任一异常立即停止并保留错误信息。
- 不修改图片内容，不导入 Unity，不生成 Prefab。

### 任务

- [x] 1. 扫描输入目录并确认六个关卡的关键文件和标题一一对应。
- [x] 2. 完成整批源/目标路径无覆盖预检。
- [x] 3. 移动 `smooth` 图片，重命名背景、预览和标题，并标准化目录名。
- [x] 4. 验证六个目录的最终结构、文件数量和残留项。
- [x] 5. 更新 `Documents/CURRENT_TASK.md`，记录执行与验证结果。
- [x] 6. 将六张 `CardBagXXX.png` 从各自卡包目录移动到下载根目录并验证无覆盖、无残留。

### 当前验证

- 已确认六个数字目录均包含 `preview.png` 和 `smooth/background_base.png`。
- 已确认根目录存在六张一一对应的 `PackTitleXXX.png`。
- Piece 数量：`015=41`、`016=28`、`018=35`、`019=26`、`020=31`、`021=34`。
- 无覆盖预检通过：六个目录、`201` 张 smooth 图片和 `6` 张标题图均无目标冲突。
- 迁移完成：生成 `CardBag015`、`CardBag016`、`CardBag018`、`CardBag019`、`CardBag020`、`CardBag021`。
- 最终结构验证通过：下载根目录包含六张 `CardBagXXX.png`；六包均包含 `GameBoard.png`、`BoardTitle.png` 和预期 Piece，总 Piece 数为 `195`。
- 根目录没有残留数字目录或 `PackTitleXXX.png`；子目录没有残留 `smooth`、`preview.png` 或 `background_base.png`。
- 本次只整理下载目录，尚未将资源复制到 `Assets/UI/CardBags/`，也未生成或覆盖 Unity Prefab。
