# 项目设置指南

## 1. 打开工程

1. Unity **2022.3.62f2c1** 打开 `Puffies`
2. 等待编译完成（Console 无红色错误）
3. 菜单 **Puffies → Sync Build Resources**

## 2. MainScene

场景内已有 Canvas + Package001/002。运行时 `MainScene` 会自动：

- 创建 `MainSceneBootstrap` 并挂脚本
- 扫描名为 `Package001`、`Package002` 的 Image
- 自动添加 `PackageInteractionHandler`

**新增卡包**：复制 Package001，改名为 `Package003`，换贴图即可（需对应 `Configs/Package003.json`）。

## 3. GameScene

场景内已有 GameBoard、Background、Piece01–09。运行时 `GameScene` 会：

- 创建 `GameSceneBootstrap`
- 按 JSON 加载棋盘贴图、创建凹槽与可拖拽碎片
- 运行时创建 `PieceBg`（世界空间 Sprite，底部托盘）

## 4. 新增关卡资源

1. 在 `ArtRes/Game00X/` 放棋盘与碎片贴图
2. 在 `Configs/` 添加 `Package00X.json`
3. 在 `ArtRes/PackImages/` 添加封面
4. 若需 3D 开包：在 `ArtRes/Effect/Prefab/CardPack/` 添加 `mesh_skin_cardPack_XXX.prefab`
5. 执行 **Puffies → Sync Build Resources**

## 5. 测试

1. Play MainScene → 点击卡包 → 开包动画 → GameScene
2. 拖拽碎片完成全部组 → Console 输出「游戏结束」

## 6. 已知限制

- Package002 需有 `Configs/Package002.json` 才能正常进入游戏
- 构建版 3D 资源由 **Puffies → Sync Build Resources** 同步到 `Assets/Resources/`（勿手改，见下方目录规范）

## 7. Resources 目录规范

`Assets/Resources/` 仅放**构建后运行时**需要的 3D 资源，由 `BuildSync` 从 `ArtRes` 自动同步：

```
Resources/
  CardPack/
    Prefabs/     mesh_skin_cardPack_001.prefab … 006   # 与 ArtRes 同名，供开包动画加载
    Materials/   CardPackLit.mat                      # 来源 ArtRes/Effect/Texture/Materials/001.mat
  PlaneGroup/
    Prefabs/     mesh_PlaneGroup_001.prefab
    Materials/   PlaneGroupLit.mat                    # 来源 ArtRes/PlaneGroup/Materials/002.mat
```

**命名约定**

| 类型 | 规则 | 示例 |
|------|------|------|
| 卡包皮肤 Prefab | 与 `ArtRes` 源文件同名，前缀 `mesh_skin_cardPack_` | `mesh_skin_cardPack_001` |
| 卡包材质 | 固定 `CardPackLit` | `CardPack/Materials/CardPackLit` |
| 平面组 Prefab | 与 ArtRes 同名 | `mesh_PlaneGroup_001` |
| 平面组材质 | 固定 `PlaneGroupLit` | `PlaneGroup/Materials/PlaneGroupLit` |

**不要**在 `Resources/CardPack/` 根目录堆放 `mesh_cardPack_*`（静态壳体，运行时不用）或 `001.mat` 等旧名文件；同步脚本会自动清理。
