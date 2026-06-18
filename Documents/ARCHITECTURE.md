# Puffies 架构说明

## 设计原则

- **编辑器搭建 UI**：MainScene / GameScene 的页面在 Unity 编辑器中摆放（Canvas + Image），脚本通过 Bootstrap 自动挂载并收集场景对象。
- **单一资源根**：2D 在 `Assets/ArtRes`；3D 在 `Assets/Resources`（`Effect`、`PlaneGroup`）；配置在 `Assets/Configs`。
- **统一构建同步**：菜单 **Puffies → Sync Build Resources**（`BuildSync.cs`）仅同步 2D StreamingAssets。

## 目录结构

```
Assets/
  ArtRes/                 # 全部美术源文件
    PackImages/           # 卡包封面
    Game001/              # 棋盘与碎片贴图
    BasicUI/              # UI 素材
    MainBg.png
    MainBg.png
  Configs/                # PackageXXX.json
  Core/                   # GameManager、GameDefine、状态类型
  Resources/              # 运行时动态加载的 3D 资源（直接维护，不经 BuildSync 复制）
    Effect/
      CardPack/
        Prefabs/          # CardPackSkin_*.prefab
        Fbx/              # CardPackAni_*.FBX、CardPackSkin_*.FBX
        Materials/        # CardPackLit.mat
      Texture/            # CardPackLit 用贴图（001.png、Material__25_*）
    PlaneGroup/
      Prefabs/            # PlaneGroup_001.prefab
      Fbx/                # PlaneGroup_001.FBX
      Materials/          # PlaneGroupLit.mat
      Textures/           # PlaneGroup_Albedo、Normal、AmbientOcclusion
  StreamingAssets/        # 构建同步的 2D/配置（Editor 下可不存在）
    ArtRes/
    Configs/
  Scripts/                # 场景控制器
    MainScene.cs
    GameScene.cs
    EffectScene.cs
    PackageInteractionHandler.cs
  Tools/                  # 静态工具类
    GameCommonUtility.cs
    GameAnimationUtility.cs
  Editor/
    BuildSync.cs
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
| Editor Play | `Assets/ArtRes`、`Assets/Configs` | `Assets/Resources/Effect`（AssetDatabase + Resources） |
| Build | `StreamingAssets/ArtRes`、`StreamingAssets/Configs` | `Resources/Effect`、`Resources/PlaneGroup` |

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
