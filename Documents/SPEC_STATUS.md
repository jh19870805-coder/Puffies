# SPEC 状态面板

- Task: Puffies 新阶段开发
- Status: In Progress
- 新需求：卡片 UI 特效目录精简为 `Effects/CardFx/`（预制体 + Materials/Textures/Meshes/Shaders）。
- 新需求：effect 场景改为 CardFx 预览（菜单 Puffies → Preview CardFx Effects）。
- 问题：CardFx 按钮有效但无画面——Overlay Canvas 下粒子不在相机视野；改世界空间 + UI→世界材质 + maxParticleSize 解除。
- Updated At: 2026-06-01 19:00
- Previous Phase: 工程目录重组（已完成并验证）

## Requirement Log

- 用户：准备开始新阶段工作。
- 用户：`.cursor` 目录不要在 Cursor 文件树中显示（已写入 `.vscode/settings.json` files.exclude）。
- 新需求：成就页 AchieveScene——MainScene `BtnAchieve` 跳转成就页，成就页 `BtnReturn` 返回 MainScene。
- 新需求：LoadingScene 为启动页，停留约 5 秒，`TextLoading` 显示 0%→100% 后自动进入 MainScene。
- 新需求：GameScene `BtnReturn` 点击返回 MainScene（首页）。
- 新需求：GameScene 不再读取 Package JSON 配置；凹槽与可拖拽碎片均来自编辑器中 `Piece` 开头的 Image 对象。
- 架构决策：本地数据存储仅用 **JSON 文件 + SQLite**，不使用 PlayerPrefs。
- 架构决策：**除非用户特别指定**，JSON / SQLite 选型由开发侧全权决定。
- 初始化时机：**LoadingScene** `Start` 中调用两存储 `Initialize()`（懒加载仍保留作兜底）。
- 新需求：本地存储文件统一命名为 `LocalData.json` / `LocalData.db`。
- 新需求：获得新卡特效 → `CardObtain_001`；卡片拖尾 → `CardTrail_001`（均在 `Effects/CardFx/`）。
- 新需求：effect 预览仅展示 CardFx（CardObtain + CardTrail），不含卡包 3D。
- 问题：CardFx 预览仅见两点——多数层用 UI 粒子 shader；世界空间预览+材质转换+GUI 按钮（2026-06-01 修复）。

## 本地存储方案（已定）

| 用途 | 方案 | 路径/键示例 |
|------|------|-------------|
| 全局设置、轻量快照、整份读写的小对象 | **JSON** | `LocalData.json` → `settings`、`profile` |
| 成就、拼图进度、解锁、排行榜缓存等条数多/需查询 | **SQLite** | `LocalData.db` → collection 如 `achievements`、`package_progress` |

**选型原则（默认执行）**
- 单条、低频、结构简单 → JSON
- 多条、要筛选/排序/按 key 查 → SQLite
- 用户明确说「放 JSON」或「放 SQLite」时，以其为准

- 根目录：`Application.persistentDataPath`（Windows 约 `AppData/LocalLow/MainTown/Puffies/`）
- SQLite 插件：sqlite-net（`Assets/Plugins/SQLite/SQLite.cs`）+ `sqlite3.dll`
- 已实现：`JsonLocalStore`、`SqliteLocalStore`


### 工程状态

- 编译：无 `error CS`（Editor.log）
- 主流程：MainScene → GameScene Play 已验证（用户确认）
- 结构：MVC（`Scripts/Model|View|Controller|Editor`），2D 资源在 `Assets/UI`

### 场景

| 场景 | 状态 |
|------|------|
| **LoadingScene** | 启动页，进度条文字后进 MainScene |
| MainScene | Package001/002/003 卡包 UI |
| GameScene | 编辑器拼图页（Piece01… 凹槽 + 拖拽），返回首页 |
| RankScene | 排行榜 + 返回 |
| AchieveScene | 成就页 + 返回（已实现场景跳转） |
| effect | CardFx 特效预览（CardObtain / CardTrail） |

### 配置与资源

| 项 | 现状 |
|----|------|
| GameScene 拼图 | 场景内 `GameBoard` + `Piece01`…`PieceNN`（Image，编辑器摆位贴图） |
| `UI/Game001/` | 拼图贴图源文件（编辑器引用） |
| MainScene 卡包 | Package001/002/003 封面 UI |
| `CardPackAni_001.FBX` | 有；002/003 无，点击会走 2D fallback |

已删除：`Resources/Config/Package001.json` 及配置同步逻辑。

### 构建

菜单：**Puffies → Sync Build Resources**（同步 UI → StreamingAssets）

## 新阶段待办（按优先级）

1. [ ] RankScene 功能接线与回归
2. [ ] 多页卡包翻页（如需要）
3. [ ] `CardPackAni_002+` FBX（或接受 fallback 到 002/003）
4. [x] 本地存储骨架：`JsonLocalStore` + `SqliteLocalStore`
5. [ ] 业务接入：设置/成就/进度写入上述存储
6. [ ] Steam 成就占位 / Steamworks 接入（物料未齐，可后做）
7. [ ] 打包构建回归

## Next Action

1. Play 验证 CardFx 预览修复（正交相机 + Hierarchy 缩放，不再自动拉远相机）
2. 业务层接入本地存储（settings、成就、拼图进度）

## Resume Prompt

`继续 Puffies 新阶段开发，请先读取 Documents/SPEC_STATUS.md`
