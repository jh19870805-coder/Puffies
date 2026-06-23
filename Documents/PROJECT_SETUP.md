# 项目设置与资源规范

## 1. 目录结构

```
Assets/
  Scenes/       LoadingScene（启动）、MainScene、GameScene、RankScene、AchieveScene
  Prefabs/      自定义预制体（预留）
  UI/           2D PNG 源文件
  Scripts/      MVC（Model / View / Controller / Editor）
  Resources/
    Effects/        # 3D 特效
      CardPack/
      PlaneGroup/
  StreamingAssets/  构建产物（UI）
```

## 2. 新增卡包流程

**MainScene 卡包**：复制 `Package001` 对象，改名为 `Package003`，换封面贴图即可。

1. 在 `UI/PackImages/` 添加封面
2. 若需 3D 开包：在 `Resources/Effects/CardPack/` 添加 FBX、材质贴图与 `CardPackSkin_XXX.prefab`

**GameScene 拼图**：在场景中编辑 `GameBoard` 与 `Piece01`、`Piece02`…（Image 组件，命名 `Piece` + 两位数字），摆好位置并指定碎片贴图；运行时自动按编号排序生成凹槽与可拖拽碎片。

## 3. 构建前同步

Unity 菜单：**Puffies → Sync Build Resources**

会将 UI 写入 StreamingAssets（Effects 已在 Resources 内，无需再同步）。

## 4. 注意事项

- `Resources` 文件夹名不可改（代码使用 `Resources.Load`）
- GameScene 依赖编辑器对象：`GameBoard`、`Piece01`…（无需 JSON 配置）
- 3D 特效资源统一在 `Resources/Effects/`；编辑器与运行时共用同一路径
- `Prefabs/` 预留给后期自定义预制体，与特效资源分开

## 5. 命名规范

| 类型 | 命名 | 路径示例 |
|------|------|----------|
| 卡包皮肤预制体 | `CardPackSkin_001` | `Resources/Effects/CardPack/` |
| 开包动画 FBX | `CardPackAni_001.FBX` | `Resources/Effects/CardPack/` |
| 材质 | `CardPackLit` | `Resources/Effects/CardPack/` |
| 平面组 | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| 获得新卡特效 | `CardObtain_001` | `Resources/Effects/CardFx/` |
| 卡片拖尾特效 | `CardTrail_001` | `Resources/Effects/CardFx/` |
| 卡片特效依赖 | Materials / Textures / Meshes / Shaders | `Resources/Effects/CardFx/` 子目录 |
| 拼图凹槽/碎片 | `Piece01`…`PieceNN` | GameScene 编辑器 Image |

## 6. 场景跳转

| 场景/按钮 | 目标 | 说明 |
|-----------|------|------|
| **LoadingScene**（启动页） | MainScene | 约 5 秒，`TextLoading` 0%→100% 后自动进入 |
| BtnRank | RankScene | 排行榜 |
| BtnAchieve | AchieveScene | 成就 |
| BtnReturn | MainScene | 从 Game/Rank/Achieve 返回首页 |
| **effect**（调试） | 仅 CardFx 预览 | 菜单 **Puffies → Preview CardFx Effects** |

**Build Settings 顺序**：`LoadingScene` 必须为 Index 0（启动场景）。

加载页对象名：`TextLoading`（`UnityEngine.UI.Text`），文案格式 `Loading... {0}%`。

## 7. 设计分辨率

**2560×1440**，PPU = 100（见 `GameDefine.DesignWidth` / `DesignHeight`）。

### 统一修改已有 Canvas

编辑器菜单：

| 菜单 | 作用 |
|------|------|
| **Puffies → Canvas → Apply Design Resolution (Current Scene)** | 只改当前打开场景里所有 `CanvasScaler` |
| **Puffies → Canvas → Apply Design Resolution (All Scenes & Prefabs)** | 批量改 `Assets/Scenes` 下场景 + `Assets` 下预制体（跳过 TextMesh Pro 示例） |

在 Inspector 里改的是 **Canvas Scaler → Reference Resolution**，不是 RectTransform 的 Width/Height。

### 新建 Canvas 自动默认 2560×1440

`Assets/Scripts/Editor/CanvasDesignResolutionEditor.cs` 会在编辑器中 **自动监听新加的 `CanvasScaler`**（含 GameObject → UI → Canvas），并套用：

- Reference Resolution：**2560 × 1440**
- UI Scale Mode：Scale With Screen Size
- Reference Pixels Per Unit：100
- Match：0.5

Unity 没有全局 Project Settings 项可改 Canvas 默认值，因此用上述 Editor 脚本实现同等效果。

### 手工检查（可选）

选中 Canvas → **Canvas Scaler** → Reference Resolution 应为 **2560 × 1440**。

| 项 | 值 |
|----|-----|
| 代码引用 | `GameDefine.DesignWidth` / `DesignHeight`，勿写死数值 |
| Game 窗口默认尺寸 | Project Settings → 2560×1440 |

运行时各场景 Controller 已通过 `GameDefine` 配置正交相机与 Canvas，无需在脚本里改分辨率常量。
