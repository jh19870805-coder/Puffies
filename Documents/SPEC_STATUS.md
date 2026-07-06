# SPEC 状态面板

- Task: Puffies 新阶段开发
- Status: In Progress
- Updated At: 2026-06-30

> 工程目录、场景、构建等静态说明见 [PROJECT_SETUP.md](PROJECT_SETUP.md)。

## Requirement Log

- GameScene：按组拼图（`PieceGroup` 或默认分组）；组完成切组时移除上一组棋盘碎片、显示凹槽（`FinalizeCompletedGroup`）。
- GameScene：拼完 `RewardPanel`；`BtnFinish` 回 Main；拼图进度写入任务与卡包（`GameTaskUtility` / `CardPackDataUtility`）。
- MainScene：根据 `CardPacks.csv` 解锁状态动态刷新卡包列表（`RefreshPackageList`）。
- LoadingScene：初始化 JSON、SQLite、任务、卡包数据。
- 本地存储：JSON + SQLite；不用 PlayerPrefs。
- CardFx 预览（effect）；设计分辨率 2560×1440。
- **未合入代码**（曾讨论后回退或未提交）：棋盘滑动对准凹槽中心、凹槽区域描边。

## Progress Snapshot

- **已完成**：
  - MVC + 全场景跳转（Loading → Main → Game/Rank/Achieve）
  - 拼图分组、托盘滑入/滑出、RewardPanel
  - `TaskConfig.csv` + `CardPacks.csv` 通过 `GameConfigRepository` 统一读取；Loading 初始化
  - 收集拼图任务进度（每拼一块 +1）、完成结算与发奖、任务推进
  - 拼图完成后保存卡包状态；首页卡包列表按解锁刷新
  - 本地存储骨架；Canvas/字体工具；CardFx 预览
  - 文档合并：`PROJECT_SETUP` + `SPEC_WORKFLOW` + `SPEC_STATUS`
- **进行中**：Play 全链路回归（任务发奖 → 回 Main 见新卡包）
- **未完成**：Rank/Achieve 页面内容；Steam；正式打包回归；棋盘滑动/描边（若仍需）

## E - Execute

- 最新提交 `7fde54f`（2026-06-30 核对 `git log -1`）：完整游戏流程、任务测试数据、首页卡包自动刷新。
- 文档整理（2026-06-30）：删除 `ARCHITECTURE` / `CLEANUP_CHECKLIST` / `SPEC_TEMPLATE`，内容并入保留文件；修正卡包流程与存储描述。
- 数据层整理（2026-06-30）：新增 `CsvTable`、`GameConfigRepository`、`IGameConfigTextSource`，把任务/卡包 CSV 加载与解析从 `GameTaskUtility`、`CardPackDataUtility` 中抽离。
- AchieveScene（2026-07-01）：返回按钮对象名改为 `CloseBtn`，匹配当前场景编辑器对象。
- AchieveScene mock（2026-07-02）：进入成就页时使用 `AchieveItem.prefab` 生成 20 条模拟成就，随机解锁状态与未解锁进度，用于暂未接入 Steam 前的页面测试。

## C - Check

- 与代码一致：无 Package JSON 拼图配置；GameScene 依赖场景 `Piece*` Image。
- 任务类型 `CollectPuzzle`（TaskType=1）与 `TaskConfig.csv` 测试行已打通。
- 配置数据仍来自 `Resources/Configs/*.csv`，但业务工具类不再直接解析 CSV；后续可替换 `IGameConfigTextSource` 为 ScriptableObject / Addressables 数据源。
- AchieveScene 当前为页面 mock 数据展示，非正式 Steam 成就系统；正式接入时应替换 `CreateMockAchievements` 数据源。
- 待 Play：多组切组 + 任务结算 + Main 卡包刷新一条龙。

## Next Action

1. Play：Loading → Main → 开包 → Game 拼完全部 → RewardPanel 任务奖励 → Main 看卡包列表是否更新
2. 多组拼图：切组后上一组凹槽显示、碎片移除
3. 若需要棋盘滑动/描边：在稳定基线上单独小步实现

## Resume Prompt

继续 Puffies 开发，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行。
