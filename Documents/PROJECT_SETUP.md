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
- 构建版 3D 卡包依赖 `Resources/CardPack`（由 BuildSync 同步）
