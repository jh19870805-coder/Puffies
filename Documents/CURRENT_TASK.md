# 当前任务

- 任务：重新导入并清理特效资源
- 状态：代码与 Unity 导入校验通过，等待 Play Mode 视觉确认
- 更新时间：2026-07-31

## 用户意图

- 重新导入 `特效资源/` 下的六个 `.unitypackage`。
- 使用特效制作方最新提供的卡包微调、拆包、拖尾和解锁资源。
- `CardFx/Shaders` 只保留 `2_Sided`、`AParticleFireClip10`、`AParticleFireClipAdd10`、`ReceiveShadow`，删除其余旧特效 Shader。

## 工作记录

- 按“资源管理 -> 桌面环境 -> 卡包微调 -> 拆包 -> 拖尾 -> 解锁”的顺序解析并覆盖导入包内资源，保留原始路径、Meta 和 GUID。
- 导入结果继续位于 `Assets/Resources/Effects/`，预览场景继续使用 `Assets/Scenes/EffectScene001.unity`；未导入包内重复的 UI、字体和 `TaskItem`。
- 更新了 `CardPackOpening_001`、`fx_chai_w_001`、`FX_ui_tuowei_w_001`、`FX_ui_jieSuo_w` 及其最新材质、贴图和模型依赖。
- 新增拆包/拖尾依赖：`FX_dot_w_012.mat`、`FX_dot_w_013.mat`、`FX_dot_w_027_01.mat` 和 `FX_dot_w_012.png`。
- 删除四个旧 Shader、七个旧 HLSL 及对应 Meta；`CardFx/Shaders` 现在只包含制作方指定的四个 Shader。
- 将遗留的 `default_unlit`、`FX_dot_w_002`、`FX_dot_w_003` 材质切换到保留 Shader，避免引用已删除的 URP Shader。
- `CardFxRuntimeUtility` 的 Trail 回退改用 `AParticleFireClipAdd10`，普通粒子回退使用 `AParticleFireClip10`，不再查找已删除的 URP/BF Packet Shader。
- 本次未修改 SQLite 或 JSON 数据结构，无需删除本地数据。

## 修改范围

- `Assets/Resources/Effects/CardFx/`
- `Assets/Resources/Effects/CardPack/`
- `Assets/Scripts/Model/CardFxRuntimeUtility.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过，仅有仓库现有的 LF/CRLF 转换提示。
- Unity 编辑器已自动刷新资源；`Editor.log` 未发现 Shader 编译、Missing Shader、GUID 冲突或 C# 错误。
- 四个正式特效 Prefab 的依赖闭包只使用制作方指定的四个 Shader。

## 下一步

1. 在 MainScene 检查列表卡包外观和呼吸动画。
2. 进入开包流程，确认最新卡包模型、材质和拆包特效正常。
3. 在 `EffectScene001` 或临时预览中检查 `FX_ui_tuowei_w_001` 与 `FX_ui_jieSuo_w` 的完整表现。
4. 完成一次正常进入 GameScene 的流程，确认新特效没有层级、缩放或紫色材质问题。

## 恢复提示

继续 Puffies 当前任务。特效六个资源包已重新覆盖导入，CardFx Shader 已精简为制作方指定的四个；下一步在 Unity Play Mode 回归卡包、拆包、拖尾和解锁视觉效果。
