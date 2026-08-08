# 当前任务

- 任务：接入真实卡包撕开动画与横向光效
- 状态：代码完成，等待 Play Mode 目视验收
- 更新时间：2026-08-08

## 用户意图

- 不修改现有卡包列表点击和放大居中流程。
- 卡包进入 `BgGame` 并居中后，玩家轻点卡包或从左向右横划时播放新的撕包特效。
- 主要动画必须显示当前选中卡包的真实封面。
- 光效和卡包撕裂节奏参考 `拆卡包特效演示.mkv` 及制作方截图。

## 工作记录

- 现有输入状态机继续支持卡包内轻点，以及从撕口左侧开始的有效横划。
- 有效输入不再直接进入 `GameScene`，而是随机加载 `CardPackOpeningModel_001-006` 中的一套并播放 `CardPackAnimation.controller`。
- 六套模型的正面节点分别为 `mesh_skin_cardPack_001` 至 `006`，背面节点为对应编号后追加 `01`；运行时已按这一规则匹配全部模型。
- 正面材质从制作方 `test` 克隆，只将 `_MainTex` 替换为当前 `PackIconNNN`；背面从 `test01` 克隆并保留 `Bg01`。没有修改制作方原材质、Shader、骨骼或粒子参数。
- `fx_chai_w_001` 沿用原始 Prefab，在骨骼动画开始 `0.5s` 后播放；这是制作方 Timeline 中光效相对模型动画的原始时差。
- Animator Clip 时长由运行时读取，当前资源约为 `1.833s`；播放结束后进入 `GameScene`。
- 撕包画面使用隔离相机和透明 RenderTexture 合成到 `BgGame` 顶层，避免改动 MainScene 既有 Canvas；相机和模型基准参数来自 `EffectScene001`，只按当前居中卡包屏幕高度统一适配尺寸。
- 资源缺失或模型节点不符合约定时输出明确错误，并回退到直接进入 `GameScene`，避免阻断游戏流程。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 使用资源

- `Assets/Resources/Effects/CardPack/Models/CardPackOpeningModel_001-006.FBX`
- `Assets/Resources/Effects/CardPack/Models/CardPackOpeningAnimation.FBX`
- `Assets/Resources/Effects/CardPack/Animations/CardPackAnimation.controller`
- `Assets/Resources/Effects/CardFx/Materials/test.mat`
- `Assets/Resources/Effects/CardFx/Materials/test01.mat`
- `Assets/Resources/Effects/CardFx/Profabs/fx_chai_w_001.prefab`
- `Assets/UI/PackImages/Bg01.png`

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- Unity Editor 当前日志中 C# 编译错误为 0，Shader 错误为 0；六套 FBX、Animator Controller 和光效 Prefab 均已开始并完成资源导入。
- 静态扫描确认六套 FBX 都包含一对符合编号规则的正反面 SkinnedMeshRenderer，Animator Controller 正确引用 `CardPackOpeningAnimation.FBX/Take 001`，光效 Prefab 根节点为 `fx_chai_w_001`。
- 尚未在 Unity Play Mode 中目视确认透明合成、真实封面朝向、最终尺寸和横向光效是否精确贴合撕口。

## 数据说明

- 本次没有修改 JSON、SQLite 或业务数据结构，不需要删除本地存储。

## 下一步

1. 在 Unity 中从 MainScene 选择任一卡包，点击“玩”，再轻点或横划居中卡包，目视检查真实封面、撕裂方向、光效位置与约 `1.833s` 节奏。
2. 至少连续测试 6 次以覆盖随机模型；如有尺寸或撕口位置偏差，只调整统一尺寸和制作方光效根节点定位，不修改材质及粒子参数。

## 恢复提示

新撕包动画已接入 MainScene：输入后随机使用六套真实撕裂模型，正面替换为当前卡包纹理，`0.5s` 后播放原始横向光效，约 `1.833s` 后进入 GameScene。下一步在 Play Mode 中覆盖六套随机模型并目视验收。
