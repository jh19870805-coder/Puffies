# 项目指南

Unity **2022.3** / URP 2D 项目。玩法：卡包开包 → 拼图拖拽 → 任务奖励。本文档涵盖目录、资源、场景、构建与命名规范；**当前开发任务**见 [SPEC_STATUS.md](SPEC_STATUS.md)。

---

## 1. 目录与加载策略

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

## 2. 场景与跳转

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

## 3. 设计分辨率与字体

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

## 4. 数据与配置

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

## 5. 新增内容流程

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

## 6. 命名规范

| 类型 | 命名 | 路径 |
|------|------|------|
| 卡包皮肤 | `CardPackSkin_001` | `Resources/Effects/CardPack/` |
| 开包动画 | `CardPackAni_001.FBX` | 同上 |
| 材质 | `CardPackLit` | 同上 |
| 平面组 | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| 获得新卡 | `CardObtain_001` | `Resources/Effects/CardFx/` |
| 卡片拖尾 | `CardTrail_001` | 同上 |

---

## 7. 构建

构建前菜单：**Puffies → Sync Build Resources**（`UI` → `StreamingAssets/UI`）。

Build Settings 顺序建议：LoadingScene → MainScene → GameScene → effect → RankScene → AchieveScene。

---

## 8. Editor 菜单速查

| 菜单 | 作用 |
|------|------|
| Puffies → Sync Build Resources | UI → StreamingAssets |
| Puffies → Canvas → Apply Design Resolution | 统一 Canvas 分辨率 |
| Puffies → Fonts → Setup Default Chinese Font | 中文字体 |
| Puffies → Preview CardFx Effects | 打开 effect 场景 |

---

## 9. 已废弃（勿再创建）

- `Assets/ArtRes/`、`Assets/Configs/`（旧目录）
- `Resources/Config/Package001.json` 及 JSON 拼图配置流（已改为场景编辑器摆位）
- 一次性迁移脚本 `Tools/*.ps1`（已删除）
