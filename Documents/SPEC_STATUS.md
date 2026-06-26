# SPEC 状态面板

- Task: Puffies 新阶段开发
- Status: In Progress
- Updated At: 2026-05-29
- Previous Phase: 工程目录重组（已完成）

---

## 工程基线

### 目录结构

```
Assets/
  Scenes/           LoadingScene、MainScene、GameScene、RankScene、AchieveScene、effect
  UI/               2D 贴图源（PackImages、Game001、BasicUI…）
  Resources/
    Effects/
      CardPack/     3D 卡包
      PlaneGroup/
      CardFx/       卡片获得/拖尾特效（预制体 + Materials/Textures/Meshes/Shaders）
    Configs/        CardPacks.csv 等
  Scripts/          MVC
    Model/          GameDefine、GameManager、工具类、本地存储
    View/           PackageInteractionHandler
    Controller/     各场景脚本
    Editor/         BuildSync、Canvas 分辨率、中文字体、CardFx 预览菜单
  Prefabs/          预留
  StreamingAssets/  UI（BuildSync 同步）
```

### 设计分辨率

| 项 | 值 |
|----|-----|
| 设计分辨率 | **2560 × 1440** |
| PPU | 100 |
| 常量 | `GameDefine.DesignWidth` / `DesignHeight` |
| Canvas 工具 | **Puffies → Canvas → Apply Design Resolution** |
| 中文字体 | **Puffies → Fonts → Setup Default Chinese Font**（Noto Sans SC TMP） |

### Build Settings（顺序）

| Index | 场景 |
|-------|------|
| 0 | **LoadingScene**（启动场景） |
| 1 | MainScene |
| 2 | GameScene |
| 3 | effect |
| 4 | RankScene |
| 5 | AchieveScene |

构建前菜单：**Puffies → Sync Build Resources**（`UI` → `StreamingAssets/UI`）

---

## 场景与跳转（已实现）

| 场景 | 脚本 | 说明 |
|------|------|------|
| **LoadingScene** | `LoadingScene.cs` | 进度 `TextLoading` 0%→100%，**2.5s** 后进 MainScene；初始化 `JsonLocalStore` + `SqliteLocalStore` |
| **MainScene** | `MainScene.cs` | 卡包 Package001/002/003；`BtnRank`→Rank；`BtnAchieve`→Achieve |
| **GameScene** | `GameScene.cs` | 编辑器拼图（`Piece01`…）；`BtnReturn`→Main；拼完 `RewardPanel` + `BtnFinish`→Main |
| **RankScene** | `RankScene.cs` | `BtnReturn`→Main |
| **AchieveScene** | `AchieveScene.cs` | `BtnReturn`→Main |
| **effect** | `CardFxPreviewScene.cs` | CardFx 预览；菜单 **Puffies → Preview CardFx Effects** |

### 按钮对象名约定

| 对象名 | 作用 |
|--------|------|
| `BtnRank` | MainScene → RankScene |
| `BtnAchieve` | MainScene → AchieveScene |
| `BtnReturn` | Rank / Achieve / Game → MainScene |
| `BtnFinish` | GameScene RewardPanel → MainScene |
| `TextLoading` | LoadingScene 进度文字 |
| `Package001`… | MainScene 卡包（可点击开包） |

---

## 脚本清单（19 个）

**Controller:** LoadingScene, MainScene, GameScene, RankScene, AchieveScene, CardFxPreviewScene  
**Model:** GameDefine, GameManager, GameCommonUtility, GameAnimationUtility, GameFontUtility, CardFxRuntimeUtility, JsonLocalStore, SqliteLocalStore  
**View:** PackageInteractionHandler  
**Editor:** BuildSync, CanvasDesignResolutionEditor, DefaultChineseFontEditor, CardFxPreviewMenu  

---

## 本地存储（已定方案）

| 用途 | 方案 | 运行时路径 |
|------|------|------------|
| 轻量 KV、设置快照 | **JSON** | `persistentDataPath/LocalData.json` |
| 成就、进度等多条查询 | **SQLite** | `persistentDataPath/LocalData.db` |

- 插件：sqlite-net（`Assets/Plugins/SQLite/`）
- 初始化：**LoadingScene** `Start` 中调用（懒加载兜底保留）
- **不使用 PlayerPrefs**

---

## 拼图 / 卡包逻辑要点

- **GameScene**：不再读 Package JSON；凹槽与碎片来自场景内 `Piece` 开头 Image；支持 `PieceGroup` 分组；组间切换、RewardPanel 已实现
- **MainScene**：3D 开包 `CardPackAni_001` 有；002/003 无则 2D fallback
- **CardFx**：`CardObtain_001`、`CardTrail_001` 在 `Resources/Effects/CardFx/`

---

## 已完成（近期）

- [x] 工程目录重组 + MVC 脚本分类
- [x] `Resources/Effects/{CardPack,PlaneGroup,CardFx}` 统一
- [x] LoadingScene 启动链 + 本地存储初始化
- [x] MainScene ↔ RankScene / AchieveScene 跳转
- [x] GameScene 返回首页 + 拼图完成 RewardPanel
- [x] 设计分辨率 2560×1440 + Canvas 批量工具
- [x] CardFx 特效目录与 effect 预览场景
- [x] 默认中文字体工具链

## 待办

- [ ] 业务数据接入本地存储（设置、成就、拼图进度）
- [ ] RankScene / AchieveScene 页面内容与数据展示
- [ ] `CardPackAni_002+` 或接受 fallback
- [ ] Steam 成就 / Steamworks（物料未齐可后做）
- [ ] 打包构建回归（StreamingAssets/UI）

## Next Action

1. Play 全链路：Loading → Main → Rank/Achieve/Game → 返回
2. 拼图多组切换 + RewardPanel 回归
3. 业务层写入 `JsonLocalStore` / `SqliteLocalStore`

## Resume Prompt

`继续 Puffies 新阶段开发，请先读取 Documents/SPEC_STATUS.md`
