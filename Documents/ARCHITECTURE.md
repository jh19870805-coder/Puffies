# 架构说明

## 资源组织

```
Assets/
  Scenes/
  UI/                    # 2D 贴图源
  Scripts/               # MVC
    Model/
    View/
    Controller/
    Editor/
  Resources/
    Configs/             # CardPacks.csv 等
    Effects/
      CardPack/
      PlaneGroup/
      CardFx/            # 预制体 + Materials/Textures/Meshes/Shaders
  Prefabs/               # 预留
  StreamingAssets/
    UI/                  # BuildSync 同步产物
  Plugins/SQLite/        # sqlite-net
```

## 加载策略

| 阶段 | 2D UI | 3D / 特效 |
|------|-------|-----------|
| Editor | `Assets/UI`（场景 Image 直接引用） | `Assets/Resources/Effects`（AssetDatabase） |
| Build | `StreamingAssets/UI`（`ToDiskPath` 读文件） | `Resources.Load("Effects/...")` |

GameScene 拼图贴图以**场景内 Image 引用**为主；`UI/Game001/` 为贴图源文件。

## 场景流

```
LoadingScene (2.5s)
    → MainScene
        → BtnRank → RankScene → BtnReturn → MainScene
        → BtnAchieve → AchieveScene → BtnReturn → MainScene
        → Package00X 开包 → GameScene
            → BtnReturn → MainScene
            → 拼完 RewardPanel / BtnFinish → MainScene
```

## 本地存储

- `LocalData.json` — `JsonLocalStore`（KV）
- `LocalData.db` — `SqliteLocalStore`（collection + key）
- 路径：`Application.persistentDataPath`
- 初始化：`LoadingScene`

## Editor 菜单

| 菜单 | 作用 |
|------|------|
| Puffies → Sync Build Resources | UI → StreamingAssets |
| Puffies → Canvas → Apply Design Resolution | 统一 2560×1440 |
| Puffies → Fonts → Setup Default Chinese Font | Noto Sans SC TMP |
| Puffies → Preview CardFx Effects | 打开 effect 场景预览 CardFx |

## BuildSync

1. 同步 `Assets/UI` → `StreamingAssets/UI`
2. 清理遗留 `ArtRes`、`Configs`、`Resources/CardPack` 等旧目录

3D 特效已在 `Resources/Effects`，无需再复制。
