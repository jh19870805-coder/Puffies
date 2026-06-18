# 架构说明

## 资源组织

```
Assets/
  Scenes/
  Prefabs/          # 自定义预制体（预留）
  UI/               # 2D 贴图源文件
  Scripts/          # MVC（Model / View / Controller / Editor）
  Resources/
    Config/
    Effects/
      CardPack/
      PlaneGroup/
  StreamingAssets/
    UI/
    Config/
```

## 加载策略

| 阶段 | 2D 贴图 / 配置 | 3D 卡包 / 平面组 |
|------|----------------|------------------|
| Editor Play | `Assets/UI`、`Assets/Resources/Config` | `Assets/Resources/Effects`（AssetDatabase） |
| Build | `StreamingAssets/UI`、`StreamingAssets/Config` | `Resources.Load("Effects/CardPack/...")`、`Resources.Load("Effects/PlaneGroup/...")` |

## BuildSync

菜单 **Puffies → Sync Build Resources** 会：

1. 将 `Assets/UI` 同步到 `StreamingAssets/UI`
2. 将 `Assets/Resources/Config` 同步到 `StreamingAssets/Config`
3. 清理遗留的 `Assets/Effects`、`Resources/CardPack`、`Resources/PlaneGroup` 等目录

3D 特效已在 `Resources/Effects`，无需再复制同步。
