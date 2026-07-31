# 当前任务

- 任务：增加开包白色划线拖尾
- 状态：实现与编译验证完成，等待 Play Mode 视觉确认
- 更新时间：2026-07-31

## 用户意图

- 点击卡包开始开包时，参考视频增加一条从左向右滑过的白色粗动画线。
- 优先使用已经导入工程的制作方特效，不重新制作替代视觉。

## 工作记录

- 确认现有开包流程只调用完整卡包动画和 `fx_chai_w_001` 拆包粒子，没有调用最新拖尾资源。
- 确认 `Resources/Effects/CardFx/Profabs/FX_ui_tuowei_w_001.prefab` 是制作方最新拖尾，包含 4 个循环粒子系统，使用工程内 Built-in 加法粒子 Shader。
- 点击卡包或完成顶部横划后，运行时按当前卡包 Renderer 世界边界计算顶部封口线；拖尾在 `0.42s` 内从宽度 `4%` 处滑至 `96%` 处。
- 白色拖尾完成横扫后停止继续发射，保留 `1.2s` 让已有粒子自然消散；随后衔接现有开包动画和 `fx_chai_w_001` 拆包粒子。
- 拖尾使用当前卡包宽度计算缩放、沿用卡包 Sorting Layer 并高于拆包粒子；资源缺失或卡包边界无效时跳过拖尾，不阻断原开包流程。
- 本次未修改数据结构或持久化内容，无需删除本地数据。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Model/CardFxRuntimeUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 使用最新的 `FX_ui_tuowei_w_001`，不使用旧 `CardTrail_001`，也不修改制作方 Prefab。
- 白线先完成横扫，再启动卡包主体开包；尾迹消散阶段与开包主体重叠，保持“划开封口后展开”的动作关系。
- 位置和缩放以运行时卡包世界边界为准，不绑定固定分辨率或 UI 坐标。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- 已确认拖尾 Resources 路径存在、Prefab 包含 4 个 ParticleSystem，引用的 `AParticleFireClipAdd10.shader` 存在。
- `git diff --check`：通过，仅有仓库现有的 LF/CRLF 转换提示。
- 尚未在 Unity Play Mode 中确认白线粗细、封口高度和 `0.42s` 节奏。

## 下一步

1. 在 MainScene 选择任意卡包，点击“玩”后轻点居中卡包，确认白线从左向右贴着顶部封口滑动。
2. 确认白线结束后立即衔接开包主体和拆包粒子，尾迹自然消散且不残留循环粒子。
3. 分别测试轻点和手动横划，两种输入都只能触发一次白线与一次开包。
4. 根据 Play Mode 观感微调粗细、Y 位置或 `0.42s` 时长。

## 恢复提示

继续 Puffies 当前任务。最新 `FX_ui_tuowei_w_001` 已接入开包前的 `0.42s` 左到右白色拖尾；下一步在 MainScene Play Mode 确认线条粗细、封口位置和开包衔接节奏。
