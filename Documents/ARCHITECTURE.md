# Puffies 架构说明

## 设计原则

- **编辑器搭建 UI**：MainScene / GameScene 的页面在 Unity 编辑器中摆放（Canvas + Image），脚本通过 Bootstrap 自动挂载并收集场景对象。
- **单一资源根**：2D 与 3D 均在 `Assets/ArtRes`；3D 位于 `ArtRes/Resources/`（`CardPack`、`PlaneGroup`）；配置在 `Assets/Configs`。
- **统一构建同步**：菜单 **Puffies → Sync Build Resources**（`BuildSync.cs`）仅同步 2D StreamingAssets。

## 目录结构

```
Assets/
  ArtRes/                 # 全部美术资源
    PackImages/           # 卡包封面（2D）
    Game001/              # 棋盘与碎片贴图（2D）
    BasicUI/              # UI 素材（2D）
    MainBg.png
    Resources/            # 3D 运行时加载（Resources.Load）
      CardPack/           # 开包模型、动画、材质、贴图（扁平，无子目录）
      PlaneGroup/         # 平面组模型、材质、贴图（扁平）
  Configs/                # PackageXXX.json
  Core/                   # GameManager、GameDefine
  Scenes/
```

## 场景职责

| 场景 | 脚本 | 编辑器负责 | 运行时负责 |
|------|------|-----------|-----------|
| MainScene | `MainScene` (Bootstrap) | Canvas、Background、Package001/002 Image | 收集卡包、开包动画、切 GameScene |
| GameScene | `GameScene` (Bootstrap) | GameBoard、Background、Piece01–09 Image | 读 JSON、凹槽/拖拽、吸附流程 |
| effect | `EffectScene` (Bootstrap) | 可选 | PlaneGroup 拖拽调试 |

## 分辨率

- 设计分辨率 1920×1080（`GameDefine.DesignWidth/Height`）
- PPU 100 → `orthographicSize = 5.4`

## 资源加载

| 环境 | 2D / 配置 | 3D 卡包 / 特效 |
|------|----------|----------------|
| Editor Play | `Assets/ArtRes`（2D 根目录）、`Assets/Configs` | `Assets/ArtRes/Resources`（AssetDatabase + Resources.Load） |
| Build | `StreamingAssets/ArtRes`（2D）、`StreamingAssets/Configs` | `Resources/CardPack`、`Resources/PlaneGroup`（Unity 自动打包） |

`ToDiskPath` 支持 `ArtRes` ↔ `Textures` 双向回退（兼容旧 StreamingAssets）。

## 已清理项

- `Assets/Textures/` → 并入 `ArtRes`
- `Assets/Models/` → 重命名为 `Core`
- `StreamingBuildSync` / `CardPackResourcesSync` / `PlaneGroupResourcesSync` → `BuildSync`
- `WindowAspectController`（Win32 窗口锁定）
- `U3DMake/` 孤立资源
- `PackageConfigModel.json` 重复模板
- `GameManager.LoadBagPieces` / `GetGameBoard`

## 命名约定

- 卡包 UI 对象：`Package001`、`Package002` …
- 拼图碎片 UI：`Piece01` …（GameScene 编辑器预置，运行时按 JSON 覆盖）
- 棋盘：`GameBoard`；背景：`Background`
