# 项目指南

Unity **2022.3** / URP 2D 项目。玩法：卡包开包 → 拼图拖拽 → 任务奖励。本文档是工程事实与功能需求的主参考；**当前开发任务**见 [SPEC_STATUS.md](SPEC_STATUS.md)。

---

## 1. 功能需求总览

### 核心循环

1. 进入 `LoadingScene` 初始化本地数据、任务配置、卡包配置与持久化存储。
2. 进入 `MainScene`，根据卡包解锁状态动态显示可玩的卡包列表。
3. 点击已解锁卡包后播放开包表现，进入 `GameScene`。
4. 玩家拖拽拼图碎片完成当前卡包拼图。
5. 拼图完成后弹出 `RewardPanel`，结算任务进度与卡包状态。
6. 点击 `BtnFinish` 返回 `MainScene`，首页卡包列表按最新解锁状态刷新。

### 场景需求

| 场景 | 功能需求 |
|------|----------|
| LoadingScene | 初始化 JSON、SQLite、任务数据、卡包数据；加载结束进入 MainScene |
| MainScene | 按 `CardPacks.csv` 与 SQLite 解锁状态动态刷新卡包列表；支持 Rank / Achieve / 开包入口 |
| GameScene | 按 `PieceGroup` 或默认分组组织拼图；组完成后切组并清理上一组碎片；全部完成后显示 RewardPanel |
| RankScene | 从 Main 进入并可返回 Main |
| AchieveScene | 当前使用 mock 成就列表；后续 Steam 接入时替换数据源 |
| effect | 用于 CardFx 预览与调试 |

### 数据与奖励需求

- 任务配置来自 `Resources/Configs/TaskConfig.csv`。
- 卡包配置来自 `Resources/Configs/CardPacks.csv`。
- 收集拼图任务类型为 `CollectPuzzle`（`TaskType=1`），每拼上一块拼图进度 +1。
- 任务完成后发放奖励，并推进到下一任务。
- 卡包解锁、游玩状态写入 SQLite `CardPacks` 表。
- 任务进度写入 JSON `TaskProgressData`。
- 不使用 `PlayerPrefs` 存储业务进度。

### 内容扩展需求

- 新增卡包时，只需保留一个 `Package001` 模板，由 `MainScene` 运行时动态生成槽位。
- 新增拼图时，在 `GameBoard` 下添加 `Piece01`...`PieceNN`，不再创建 Package JSON。
- 3D 卡包与 CardFx 资源放在 `Resources/Effects/`，运行时通过 `Resources.Load` 加载。
- 构建前通过 `Puffies → Sync Build Resources` 同步 `Assets/UI` 到 `StreamingAssets/UI`。

### 待定或未完成需求

- Rank 页面正式内容。
- Steam 成就系统接入，替换 AchieveScene mock 数据。
- 正式打包回归。
- 棋盘滑动对准凹槽中心、凹槽区域描边：曾讨论但未合入，若仍需要应单独小步实现。

---

## 2. 目录与加载策略

```
Assets/
  Scenes/           LoadingScene（启动）、MainScene、GameScene、RankScene、AchieveScene、effect
  UI/               2D 贴图源（PackImages、Game001、BasicUI…）
  Scripts/          MVC
    Model/          GameDefine、GameManager、工具类、本地存储、任务/卡包数据
    View/           PackageInteractionHandler
    Controller/     各场景脚本
    Editor/         BuildSync、Canvas 分辨率、中文字体、CardFx 预览
  Resources/
    Configs/        TaskConfig.csv、CardPacks.csv
    Effects/
      CardPack/     3D 卡包
      PlaneGroup/
      CardFx/       卡片获得/拖尾（预制体 + Materials/Textures/Meshes/Shaders）
  Prefabs/          预留
  StreamingAssets/  UI（构建同步产物）
  Plugins/SQLite/   sqlite-net
```

| 阶段 | 2D UI | 3D / 特效 |
|------|-------|-----------|
| Editor | `Assets/UI`（场景 Image 直接引用） | `Assets/Resources/Effects` |
| Build | `StreamingAssets/UI`（`ToDiskPath`） | `Resources.Load("Effects/...")` |

- `Resources` 文件夹名不可改（代码硬编码路径）。
- GameScene 拼图以**场景内 Image** 为主；`UI/Game001/` 为贴图源。
- 3D 特效统一在 `Resources/Effects/`，无需再复制到 StreamingAssets。

---

## 3. 场景与跳转

```
LoadingScene (2.5s, TextLoading 0%→100%)
  → MainScene
      → BtnRank     → RankScene     → BtnReturn → Main
      → BtnAchieve  → AchieveScene  → BtnReturn → Main
      → 已解锁卡包（运行时动态槽位）→ 开包动画 → GameScene
          → BtnReturn → Main
          → 拼完 RewardPanel / BtnFinish → Main
effect（调试）: CardFx 预览，菜单 Puffies → Preview CardFx Effects
```

| 场景 | 脚本 | 要点 |
|------|------|------|
| LoadingScene | `LoadingScene.cs` | 初始化 JSON / SQLite / **GameTaskUtility** / **CardPackDataUtility** |
| MainScene | `MainScene.cs` | 卡包 UI；**按解锁状态动态刷新列表**；3D 开包或 2D fallback |
| GameScene | `GameScene.cs` | 拼图分组与 RewardPanel；**收集拼图任务进度**；完成后保存卡包并任务结算 |
| RankScene / AchieveScene | 各场景脚本 | 返回 Main |
| effect | `CardFxPreviewScene.cs` | CardObtain / CardTrail 预览 |

**Build Settings**：`LoadingScene` 必须为 Index **0**。

| 对象名 | 作用 |
|--------|------|
| `BtnRank` / `BtnAchieve` | Main → Rank / Achieve |
| `BtnReturn` | Rank / Game → Main |
| `CloseBtn` | Achieve → Main |
| `BtnFinish` | Game RewardPanel → Main |
| `TextLoading` | Loading 进度文案 |
| `GameBoard` / `Piece01`… | GameScene 棋盘与凹槽 |
| `PieceGroup01`… | 可选分组父节点 |
| `PieceBoard` | 碎片托盘 |
| `RewardPanel` | 拼图完成奖励页 |
| `Package001` | MainScene **卡包槽位模板**（隐藏；运行时 `Instantiate` 生成列表） |

---

## 4. 设计分辨率与字体

| 项 | 值 |
|----|-----|
| 设计分辨率 | **2560 × 1440** |
| PPU | 100（`GameDefine.PixelsPerUnit`） |

| 菜单 | 作用 |
|------|------|
| **Puffies → Canvas → Apply Design Resolution** | 批量套用 2560×1440 |
| **Puffies → Fonts → Setup Default Chinese Font** | Noto Sans SC TMP + UI Text |

新建 `CanvasScaler` 由 `CanvasDesignResolutionEditor.cs` 自动写入默认值。代码请用 `GameFontUtility`，勿写死字体路径。

---

## 5. 数据与配置

| 数据 | 来源 | 运行时持久化 |
|------|------|----------------|
| 任务配置 | `GameConfigRepository` 读取 `Resources/Configs/TaskConfig.csv` | —（只读配置） |
| 任务进度 | `GameTaskUtility` | `persistentDataPath/LocalData.json` 根对象 **`TaskProgressData`**（`CurrentTaskId`、`CurrentCompleteValue`） |
| 卡包配置 | `GameConfigRepository` 读取 `Resources/Configs/CardPacks.csv` | —（只读配置） |
| 卡包解锁/游玩状态 | `CardPackDataUtility` | `LocalData.db` 表 **`CardPacks`** |
| 通用 collection + key 存储 | `SqliteLocalStore` API | `LocalData.db` 表 **`AppRecords`**（成就等扩展用） |

- `GameConfigRepository`：统一加载和缓存任务/卡包配置，当前数据源是 `ResourcesGameConfigTextSource`（优先 `Resources.Load<TextAsset>`，编辑器兜底磁盘路径）。
- `CsvTable`：统一 CSV 解析，支持表头访问、引号字段和空行过滤；业务层不再直接 `Split(',')`。
- `JsonLocalStore`：整文件读写**单一根对象**（当前用于任务进度），不是通用 KV 字典。
- `SqliteLocalStore`：`AppRecords` 为 collection/key 模式；卡包业务使用独立 `CardPacks` 表。
- **不使用 PlayerPrefs**。
- 初始化时机：**LoadingScene** `Start`（`JsonLocalStore`、`SqliteLocalStore`、`GameTaskUtility`、`CardPackDataUtility`）。

---

## 6. 新增内容流程

### 卡包（MainScene）

MainScene 按数据库已解锁卡包**动态创建槽位**（`RefreshPackageList`），无需在场景里为每个卡包手动复制 `Package002`、`Package003`…

1. 场景中保留**一个**模板对象 **`Package001`**（`MainScene` 会将其隐藏，并 `Instantiate` 生成列表项）。
2. 在 `CardPacks.csv` 增加一行（`PackId`、`PackSize`）。
3. 在 `UI/PackImages/` 添加对应封面（路径规则见 `GameDefine.FormatPackImagePath`）。
4. 解锁/游玩状态由 `CardPackDataUtility` 写入 SQLite **`CardPacks`** 表（任务奖励解锁、拼图完成保存等）。
5. 可选 3D：`CardPackAni_00N.FBX`、`CardPackSkin_00N.prefab` → `Resources/Effects/CardPack/`（无则 2D fallback）。

### 拼图（GameScene）

1. 在 `GameBoard` 下添加 `Piece01`…`PieceNN`（Image，`Piece` + 两位数字）。
2. 分组：子节点 `PieceGroup01`… 或默认 `Piece01–04` / `Piece05+`。
3. 无需 Package JSON；运行时从场景 Image 生成凹槽与可拖拽碎片。

### CardFx

预制体与依赖放在 `Resources/Effects/CardFx/`（`CardObtain_001`、`CardTrail_001`）。

---

## 7. 命名规范

| 类型 | 命名 | 路径 |
|------|------|------|
| 卡包皮肤 | `CardPackSkin_001` | `Resources/Effects/CardPack/` |
| 开包动画 | `CardPackAni_001.FBX` | 同上 |
| 材质 | `CardPackLit` | 同上 |
| 平面组 | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| 获得新卡 | `CardObtain_001` | `Resources/Effects/CardFx/` |
| 卡片拖尾 | `CardTrail_001` | 同上 |

---

## 8. 构建

构建前菜单：**Puffies → Sync Build Resources**（`UI` → `StreamingAssets/UI`）。

Build Settings 顺序建议：LoadingScene → MainScene → GameScene → effect → RankScene → AchieveScene。

---

## 9. Editor 菜单速查

| 菜单 | 作用 |
|------|------|
| Puffies → Sync Build Resources | UI → StreamingAssets |
| Puffies → Canvas → Apply Design Resolution | 统一 Canvas 分辨率 |
| Puffies → Fonts → Setup Default Chinese Font | 中文字体 |
| Puffies → Preview CardFx Effects | 打开 effect 场景 |

---

## 10. 已废弃（勿再创建）

- `Assets/ArtRes/`、`Assets/Configs/`（旧目录）
- `Resources/Config/Package001.json` 及 JSON 拼图配置流（已改为场景编辑器摆位）
- 一次性迁移脚本 `Tools/*.ps1`（已删除）
