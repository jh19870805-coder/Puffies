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
4. 若需 3D 开包：在 `ArtRes/Resources/CardPack/` 添加 `CardPackSkin_XXX.prefab` 与对应 FBX
5. 执行 **Puffies → Sync Build Resources**（仅同步 2D 到 StreamingAssets）

## 5. 测试

1. Play MainScene → 点击卡包 → 开包动画 → GameScene
2. 拖拽碎片完成全部组 → Console 输出「游戏结束」

## 6. 已知限制

- Package002 需有 `Configs/Package002.json` 才能正常进入游戏
- 3D 资源统一在 `Assets/ArtRes/Resources/` 维护（`CardPack`、`PlaneGroup` 两个扁平目录）

## 7. ArtRes 目录规范

`Assets/ArtRes/` 放**全部美术资源**：

```
ArtRes/
  PackImages/          # 2D 卡包封面
  Game001/             # 2D 棋盘与碎片
  BasicUI/             # 2D UI
  MainBg.png
  Resources/           # 3D（Resources.Load，扁平无深层子目录）
    CardPack/          # CardPackSkin_*.prefab、CardPackAni_*.FBX、CardPackLit.mat、贴图
    PlaneGroup/        # PlaneGroup_001.prefab/.FBX、PlaneGroupLit.mat、贴图
```

**命名对照（旧 → 新）**

| 旧名 | 新名 | 说明 |
|------|------|------|
| `mesh_PlaneGroup_001` | `PlaneGroup_001` | 特效场景平面组 |
| `002.mat` | `PlaneGroupLit.mat` | 平面组材质 |
| `dscsd.png` | `PlaneGroup_Albedo.png` | 漫反射贴图 |
| `Material_26_Normal_DirectX.png` | `PlaneGroup_Normal.png` | 法线贴图 |
| `Material_26_Mixed_AO.png` | `PlaneGroup_AmbientOcclusion.png` | AO 贴图 |
| `asr.jpg` | `PlaneGroup_Environment.jpg` | 环境贴图 |
| `2Sided_w_01.shader` | `TwoSided_01.shader` | 双面材质 |
| `BF_Effect_EffectPacket.shader` | `EffectPacket.shader` | 卡包特效 Shader |
| `AParticleFireClipAdd10.shader` | `ParticleFire_AdditiveClip.shader` | 粒子加法裁剪 |
| `AParticleFireClip10.shader` | `ParticleFire_AlphaClip.shader` | 粒子透明裁剪 |
| `mesh_skin_cardPack_001` | `CardPackSkin_001` | 开包皮肤 Prefab / FBX |
| `mesh_ani_cardPack_001` | `CardPackAni_001` | 开包动画 FBX |
| `mesh_cardPack_001` | `CardPackShell_001` | 静态壳体（已删除，非运行时资源） |

**卡包命名**

| 类型 | 规则 | 示例 |
|------|------|------|
| 皮肤 Prefab | `CardPackSkin_` + 三位编号 | `CardPackSkin_001` |
| 动画 FBX | `CardPackAni_` + 三位编号 | `CardPackAni_001.FBX` |
| 材质 | `CardPackLit` | `ArtRes/Resources/CardPack/CardPackLit` |

运行时仅加载 `CardPackSkin_*` 与 `CardPackAni_*`。
