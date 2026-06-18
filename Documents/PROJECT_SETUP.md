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
4. 若需 3D 开包：在 `Resources/Effect/CardPack/Prefabs/` 添加 `mesh_skin_cardPack_XXX.prefab`，FBX 放 `CardPack/Fbx/`
5. 执行 **Puffies → Sync Build Resources**（仅同步 2D 到 StreamingAssets）

## 5. 测试

1. Play MainScene → 点击卡包 → 开包动画 → GameScene
2. 拖拽碎片完成全部组 → Console 输出「游戏结束」

## 6. 已知限制

- Package002 需有 `Configs/Package002.json` 才能正常进入游戏
- 3D 资源统一在 `Assets/Resources/` 直接维护（Effect、PlaneGroup）

## 7. Resources 目录规范

`Assets/Resources/` 放**运行时动态加载**的 3D 资源：

```
Resources/
  Effect/
    CardPack/
      Prefabs/     mesh_skin_cardPack_001 …       # 开包皮肤模型
      Fbx/         mesh_ani_cardPack_001.FBX 等    # 动画与源模型
      Materials/   CardPackLit.mat
    Scene/         fx_chai_w_001.prefab 等
    Shader/
      TwoSided_01.shader
      EffectPacket.shader
      Particle/
        ParticleFire_AdditiveClip.shader
        ParticleFire_AlphaClip.shader
    Texture/       特效通用贴图（Trail、Glow、Particle 等）
  PlaneGroup/
    Prefabs/       PlaneGroup_001.prefab
    Fbx/           PlaneGroup_001.FBX
    Materials/     PlaneGroupLit.mat
    Textures/      PlaneGroup_Albedo、Normal、AmbientOcclusion …
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

**卡包命名（暂未改，与动画解析逻辑绑定）**

| 类型 | 规则 | 示例 |
|------|------|------|
| 皮肤 Prefab | `mesh_skin_cardPack_` + 编号 | `mesh_skin_cardPack_001` |
| 动画 FBX | `mesh_ani_cardPack_` + 编号 | `mesh_ani_cardPack_001.FBX` |
| 材质 | `CardPackLit` | `Effect/CardPack/Materials/CardPackLit` |

`mesh_cardPack_*` 为静态壳体，仅参考；运行时加载 `mesh_skin_*`。
