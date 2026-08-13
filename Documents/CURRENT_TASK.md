# 当前任务

- 任务：开包滑光改为复用 MainScene 场景粒子
- 状态：已对齐制作方正式 Timeline 播放窗口，待目视验证
- 更新时间：2026-08-13

## 用户意图

- 删除此前代码中对开包滑光粒子的加载、创建和尺寸参数覆盖。
- 使用 MainScene `PackObject` 下人工调好的 `fx_chai_w_001` 场景实例。
- 保留场景中人工配置的原层级和完整 Transform，沿用 `0.5s` 延迟，代码只负责播放与停止。
- 光效完全使用 MainScene 中人工配置的原 Transform；运行时不再进行任何位置换算或赋值。

## 工作记录

- 确认场景已新增 `fx_chai_w_001` Prefab 实例，并保存了根与内部粒子节点的人工 Scale 调整。
- 删除运行时 `Resources.Load/Instantiate` 光效流程，以及光带 `7x`、星星 Start Size `3x`、根 Scale 重置等覆盖。
- MainScene 初始化时停止、清空并隐藏场景粒子，避免 `Play On Awake` 在首页提前显示。
- 场景父节点正式命名为 `PackObject`，`fx_chai_w_001` 实例默认设为隐藏；未修改其人工 Scale 或内部粒子配置。
- `PackObject` 容器改为常开，内部仅作编辑器参照的 `PackItem` 实例单独保持关闭；否则父节点关闭会阻止场景粒子激活和渲染。
- 已定位视觉不一致原因：上一版仍把场景实例临时挂到运行时 `CardPackOpeningStage`，并覆盖了 Local Position/Rotation，导致它受到 Stage 的屏幕适配缩放和位移。
- 删除父节点、Sibling、Local Transform 缓存及换父节点/坐标覆盖逻辑；`fx_chai_w_001` 始终留在 `PackObject` 原层级，根与全部子节点参数均不由代码修改。
- 播放结束或中断时只停止、清空并隐藏场景粒子，不随运行时 Stage 销毁。
- 用户确认位置修改前的动画版本正确；后续完整 Stage 坐标换算和仅 X/Y 换算两版均未达到预期，现已全部删除。
- 当前不设置光效根节点 Position、Local Position、Rotation 或 Scale，只控制停止、清空、隐藏和播放。
- 曾将 `PackObject` 从 MainScene 主 Canvas 移到场景根级以排除父链影响，但该操作改变了用户在编辑器中调好的父级坐标系，且“同一摄像机”不要求同一父节点；用户指出后已完整撤销。
- 当前恢复用户原始层级 `Canvas/PackObject/fx_chai_w_001`；主 Canvas 本身为 `Screen Space - Camera` 并绑定 Main Camera，因此无需移动对象来满足同摄像机要求。
- 对比确认运行时原先先从根节点递归播放整组，随后又逐个对子粒子执行 `Stop/Play(withChildren)`；这会重复重置子粒子，与 Inspector 对整组点击 Play 的行为不一致。
- 已改为只对根 `ParticleSystem` 执行一次递归停止和播放；初始化及结束清理也只从根节点递归停止，不再逐个操作七个子粒子。
- 重新核对制作方 `EffectScene001/test.playable`：正式绑定对象是主 Canvas 下的 `fx_chai_w_001`，模型启动后延迟 `0.5s` 播放，光效轨道持续约 `3.033s`；此前记录的 `(0,1,-1.5)` 属于另一份世界空间演示实例，不是正式 Timeline 基准。
- 运行时完成条件改为等待模型动画和 `0.5s + 3.033s` 光效窗口中较晚结束的一项，避免原先约 `1.333s` 后切换场景导致左右渐变尚未完整展开；完成后立即停止并隐藏场景粒子。
- 舞台环境核对确认：MainScene 与制作方场景均由 Main Camera 直接渲染，HDR 开启、MSAA 关闭、后处理关闭、背景色和 Canvas 设计分辨率/Match/PPU 一致。制作方的全屏 `blur` 是单独的背景合成层，不属于粒子 Prefab；其高采样材质会改变当前木纹开包背景，本轮不将其误作为粒子参数强制接入。
- 定位“首次正确、从 GameScene 返回后第二次滑光形态错误”的原因：制作方 `test.playable` 的 `ControlPlayableAsset` 固定使用 `particleRandomSeed=1`，而手写播放此前绕过 Timeline，七个开启 `Auto Random Seed` 的子粒子会在每次重新加载 MainScene 时生成不同形态。
- 播放前按 Unity Timeline 的语义，仅对运行时实例中启用自动随机种子的 ParticleSystem 设置确定性种子 `1`，随后清空并从零播放；不修改粒子 Prefab，也不覆盖 Transform、材质、发射、Start Size 或排序。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scenes/MainScene.unity`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 运行时按精确场景路径 `PackObject/fx_chai_w_001` 查找实例；对象始终保留在 `PackObject` 下，代码不改变父节点和 Transform。
- 保留用户当前未提交的 `fx_chai_w_001.prefab`、`PackItem.prefab` 和 `QualitySettings.asset` 参数；MainScene 仅补正 `PackObject` 名称与粒子默认隐藏状态。
- `CardPackOpeningStage` 只负责 3D 卡包模型的屏幕适配；滑光保持用户在 MainScene 中建立的 `Canvas/PackObject/fx_chai_w_001` 层级，不挂入运行时 Stage。
- 粒子播放语义以根 `ParticleSystem.Play(withChildren: true)` 为准，保持制作方整套层级的延迟、随机和组合关系。

## 验证

- 本次移除 Transform 干预后的 `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- 最终增加精确场景路径绑定后再次编译通过，`0` 警告、`0` 错误。
- 恢复旧 Stage 播放点后再次编译通过，`0` 警告、`0` 错误；静态搜索确认未恢复换父节点、Local Transform 或粒子尺寸覆盖逻辑。
- 修正为仅对齐旧播放点 X/Y 并保留场景世界 Z 后再次编译通过，`0` 警告、`0` 错误。
- 完全删除后续位置换算与赋值代码、还原到位置修改前版本后再次编译通过，`0` 警告、`0` 错误。
- 核对确认 `fx_chai_w_001.prefab` 工作区哈希与 Git 索引完全一致，MainScene 中人工 Scale 覆盖仍在，Unity 加载的 `Assembly-CSharp.dll` 也晚于还原后的源码。
- 排查发现重启期间同时存在两个指向 Puffies 的 Unity 主进程；已正常关闭旧编辑器、清理无窗口重复实例，并只保留由 Unity Hub 干净打开且正常响应的一个编辑器窗口。
- 当前编辑器从 `LoadingScene` 打开，下一次正常运行会重新从磁盘创建 MainScene 场景实例，不再沿用此前热重载期间的运行态对象。
- 场景 YAML 检查确认 `PackObject` 只在 `SceneRoots` 出现一次，主 Canvas 的 `m_Children` 不再包含它；`PackItem` 与唯一 `fx_chai_w_001` 实例仍正确挂在 `PackObject` 下。
- 父链修复后 `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误；当前仅一个 Unity 主编辑器进程，另外两个 Unity 进程是其 Asset Import Worker。
- 本次代码和文档文件 `git diff --check` 通过；全工作区检查仍会报告用户现有 `PackItem.prefab` 两行 Unity 序列化尾随空格，本次未格式化该 Prefab。
- 搜索确认旧 `LightEffectPath`、`Resources.Load/Instantiate` 光效、光带缩放和星星 Start Size 缩放代码均已删除。
- Unity 已完成 `MainScene.cs` 强制同步编译、程序集重载和场景资源刷新，Editor.log 未出现 C# 编译错误、Missing Prefab 或导入失败。
- 本次根粒子单次递归播放修复的 `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。
- 完整光效轨道等待逻辑的 `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。
- 固定随机种子修复的 `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误；`git diff --check` 通过。待在 Unity 连续验证“首次开包 -> GameScene -> 返回 MainScene -> 同一卡包再次开包”的两次滑光一致性。
- 待在 MainScene 完整播放一次开包动画，确认只出现一份滑光，尺寸与场景人工配置一致。
- 待连续返回 MainScene 后再次开包，确认场景粒子可重复播放且没有被销毁。

## 下一步

1. 退出并重新进入 Play Mode，从 MainScene 播放一次开包动画，对比 Inspector 手动预览效果。
2. 返回 MainScene 再开一次包，确认场景粒子仍可重复播放。

## 恢复提示

开包滑光已恢复用户原始场景层级 `Canvas/PackObject/fx_chai_w_001`；Canvas 与开包内容均由 Main Camera 渲染，运行时不换父节点、不设置 Transform，只控制播放和停止。
