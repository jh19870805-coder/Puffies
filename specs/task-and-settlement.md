# 进度与结算

- 状态：核心实现已完成；分阶段奖励展示和 Play Mode 回归待完成
- 范围：积分任务、结算、卡包生命周期、卡包发放、列表展示和持久化

## 已确认规则

### 积分与任务

1. `TaskType=1` 表示 `AccumulateScore`；放置 Piece 不直接增加任务进度。
2. 基础分为 XS 60、S 80、M 100、L 120、XL 140、XXL 160、XXXL 200；卡包尺寸读取自 `CardPacks.csv`。
3. 符合条件的加成相加：未使用提示 +5%、关闭关卡描边 +2%、关闭贴纸描边 +5%、完成时间 `<=15` / `<=30` / `<=60` 秒分别 +3% / +2% / +1%。
4. 首个 Piece 成功放置时开始计时，结算开始时冻结。点击 `BtnTips` 后不再获得未使用提示加成。
5. 最终分数为 `ceil(baseScore * (1 + totalBonusPercent / 100))`，并且只向当前积分任务累计一次。
6. 已完成任务只发放一次配置奖励并只推进一次。超额进度仅可结转到下一个 `AccumulateScore` 任务。
7. 结算分数、`TextProgress` 和 `ProgressMask` 使用同一个动画分数值。

### 卡包进度

1. 持久化生命周期状态为 `Locked`、`Unlocked`、`InProgress` 和 `Completed`。
2. 发放使 `Locked` 变为 `Unlocked`；完成多组卡包的第一组后变为 `InProgress`；完成最后一组后变为 `Completed`。
3. 重玩已完成卡包不再执行首次完成发包尝试，但仍可完成任务并创建任务必得权益。
4. 任务奖励创建可去重的待发权益，不受自然结算的章节阶段门槛限制；全局可玩卡包数 `Unlocked + InProgress` 小于 `6` 时立即发放，达到 `6` 时持久化保存，并在之后任意一次成功结算出现空位后继续重试。重玩只禁止首次完成发包，不禁止兑现已经赚到的任务权益。
5. 首次完成执行一次受阶段门槛控制的发包尝试。同一轮结算中，任务和首次完成来源可以发放两个不同的锁定卡包。
6. 内部章节限制可选锁定卡包池，但不向玩家展示。

### MainScene 与奖励表现

1. MainScene 每页使用 6 列 x 3 行显示 18 个卡包。
2. 单次展示的新发卡包优先，其后依次是 `InProgress`、`Unlocked` 和 `Completed`；时间戳和 PackId 确保排序确定。
3. 已完成卡包的封面和尺寸图标置灰，但仍可重玩。
4. RewardPanel 保持编辑器设置的 `ImgBag` Sprite。点击 `BtnFinish` 后，本次结算发放的全部卡包移动到居中行，停顿并跨越场景加载，然后分别飞到 MainScene 对应列表位置。
5. MainScene 为每个 PackId 复用同一套通用 3D 卡包模型及现有 Animator Controller。
6. 播放前，通过每个 Renderer 的材质属性为模型设置所选卡包的真实 `PackIconNNN.png`，不得修改共享材质资源。
7. 封面 UV 使用完整原始 Sprite 贴图矩形，不进行居中裁切或运行时宽高补偿。通用特效必须按照统一 `600 x 680`（`15:17`）封面格式制作，再等比适配并居中到点击的 UI 边界。
8. 缺少封面数据时可以回退到模型自带贴图，但缺少 PackId 专属 3D Prefab 不得阻止通用动画播放。
9. 渲染前，在蒙皮后测量动画时间零点的闭合 Mesh，等比适配并对齐点击封面。兼容的替换特效在交接时不得出现尺寸、比例、裁切或空白边变化。
10. 替换特效使用一套语义明确的 `CardPackOpening` 资源。运行时选择的封面渲染在模型正面，原始卡背贴图渲染在背面，原始裁切蒙版保留波浪形边缘。
11. 卡包材质必须使用兼容 URP 的 Shader。导入替换特效时，不得保留 Built-in/Amplify Shader 依赖或源包中无关的编号卡包资源。

### 卡包特效替换

- [x] 从交付包中只提取通用开包动画、模型、Prefab、Controller、材质、Shader、封面、背面贴图和裁切蒙版。
- [x] 删除过时的编号卡包外观及其未使用材质贴图。
- [x] 统一内部资源命名，并将交付的双面裁切 Shader 适配到 URP。
- [x] 将选中卡包 Sprite 绑定到 Shader 正面贴图，不修改共享材质。
- [x] 验证 C# 编译、Unity 导入、Shader 支持、资源引用和第零帧宽高比。
- [x] 修复项目 URP Renderer2D 的 Unlit Shader Pass，并确认离屏输出非空（`600 x 680` 下有 `320789` 个可见像素）。
- [ ] 在 MainScene Play Mode 中使用 PackId 1 和 17 目视验证波浪边缘及静态图到动画的交接。

Renderer2D 修复后，MainScene 复测确认输出可见。仍存在轻微位置跳变；最终对齐等待生产播放容器和屏幕位置确认后处理。

## 当前实现

- `GameScoreUtility` 与任务进度代码一起存放在 `GameTaskUtility.cs`，负责计算完整最终分数和各项加成百分比。
- GameScene 在播放结算表现前持久化任务进度和奖励状态。
- `TaskProgressUIUtility` 绑定 MainScene 和 GameScene 共用的 `TaskItem`。
- 当前积分表现只在 0.8 秒内从 0 滚动到最终分数，尚未逐项显示符合条件的加成及累计阶段分数。
- `CardPackDistributionUtility` 对自然结算应用确定性的 `R/H` 阶段门槛，对任务奖励只应用全局可玩卡包上限 `6`，并将达到上限时的待发任务权益存入 SQLite。
- 当前配置包含章节 1 和章节 2 的 21 个卡包；目前只有五个可玩 CardBag Prefab：001、002、003、008 和 017。
- 开包使用 `CardPackOpening.prefab` 和 `CardPackOpening.controller` 作为通用模型和 Animator Controller；PackId 只决定运行时封面，不选择编号 Prefab。

## 持久化

- 任务进度保存在 `LocalData.json` 的 `TaskProgressData` 下。
- 卡包生命周期和发放进度保存在 `LocalData.db`。
- `CardPacks` 表使用 `PackId`、`PackSize`、`LifecycleState`、`UnlockTime` 和 `CompletionTime`；不支持旧字段 `IsUnlocked` 和 `IsPlayed`。
- 从旧结构同步后，关闭 Unity 并重置两个本地数据文件，再进行完整跨存储回归。未经用户明确允许不得删除。

## 待确认事项

- 加成展示顺序、每一步的时长和缓动，以及中间值取整方式。
- 提示失败语义和描边开关对加成资格的精确定义。
- 最终候选选择方式，以及锁定卡包池为空时的行为。
- 章节准确成员、初始章节状态和章节推进规则。
- 特殊卡包统计方式，以及趋近约 150 个卡包的最终节奏。

## 验证

- 当前 HEAD `2236f9f` 的 runtime、first-pass 和 Editor 程序集构建均为 0 警告、0 错误。
- 已将静态实现和持久化结构与 `PROJECT_CONTEXT.md`、`GAME_DESIGN_REQUIREMENTS.md` 交叉核对。
- 完整 Play Mode 回归仍待完成。
