# 项目设置与资源规范

## 1. 目录结构

```
Assets/
  Scenes/       LoadingScene（启动）、MainScene、GameScene、RankScene、AchieveScene
  Prefabs/      自定义预制体（预留）
  UI/           2D PNG 源文件
  Scripts/      MVC（Model / View / Controller / Editor）
  Resources/
    Config/     PackageXXX.json
    Effects/        # 3D 特效
      CardPack/
      PlaneGroup/
  StreamingAssets/  构建产物（UI、Config）
```

## 2. 新增卡包流程

**新增卡包**：复制 Package001，改名为 `Package003`，换贴图即可（需对应 `Resources/Config/Package003.json`）。

1. 在 `UI/Game00X/` 放棋盘与碎片贴图
2. 在 `Resources/Config/` 添加 `Package00X.json`
3. 在 `UI/PackImages/` 添加封面
4. 若需 3D 开包：在 `Resources/Effects/CardPack/` 添加 FBX、材质贴图与 `CardPackSkin_XXX.prefab`

## 3. 构建前同步

Unity 菜单：**Puffies → Sync Build Resources**

会将 UI、Config 写入 StreamingAssets（Effects 已在 Resources 内，无需再同步）。

## 4. 注意事项

- `Resources` 文件夹名不可改（代码使用 `Resources.Load`）
- Package002 需有 `Resources/Config/Package002.json` 才能正常进入游戏
- 3D 特效资源统一在 `Resources/Effects/`；编辑器与运行时共用同一路径
- `Prefabs/` 预留给后期自定义预制体，与特效资源分开

## 5. 命名规范

| 类型 | 命名 | 路径示例 |
|------|------|----------|
| 卡包皮肤预制体 | `CardPackSkin_001` | `Resources/Effects/CardPack/` |
| 开包动画 FBX | `CardPackAni_001.FBX` | `Resources/Effects/CardPack/` |
| 材质 | `CardPackLit` | `Resources/Effects/CardPack/` |
| 平面组 | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |

## 6. 场景跳转

| 场景/按钮 | 目标 | 说明 |
|-----------|------|------|
| **LoadingScene**（启动页） | MainScene | 约 5 秒，`TextLoading` 0%→100% 后自动进入 |
| BtnRank | RankScene | 排行榜 |
| BtnAchieve | AchieveScene | 成就 |
| BtnReturn | MainScene | 从 Game/Rank/Achieve 返回首页 |

**Build Settings 顺序**：`LoadingScene` 必须为 Index 0（启动场景）。

加载页对象名：`TextLoading`（`UnityEngine.UI.Text`），文案格式 `Loading... {0}%`。

## 7. 设计分辨率

1920×1080，PPU = 100（见 `GameDefine.DesignWidth/Height`）
