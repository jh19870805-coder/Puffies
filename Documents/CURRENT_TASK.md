# 当前任务

- 任务：清除旧特效资源，准备重新导入
- 状态：已完成
- 更新时间：2026-07-30

## 用户意图

- 删除此前导入工程的全部特效，包括导入后的资源和原始 `.unitypackage`。
- 重点清空旧 `Assets/Resources/Effects`，后续从干净状态重新导入新特效包。

## 工作记录

- 删除 `Assets/Resources/Effects` 整个目录及 Meta，共 230 个文件、8 个子目录，约 57.6 MB。
- 删除原始包 `shader修改.unitypackage`、`卡包.unitypackage`、`拆卡包特效.unitypackage` 及各自 Meta。
- 删除仓库根目录 `特效资源/`，包含 6 个历史 `.unitypackage`、视频、参考图和压缩包，共 11 个文件、约 65.9 MB。
- 删除旧 `effect.unity` 特效预览场景及 Meta，并从 Build Settings 移除。
- 删除只服务旧资源的 `CardFxPreviewScene.cs`、`CardFxPreviewMenu.cs`、`CardPackDismantlePreviewEditor.cs` 及 Meta。
- 保留 MainScene/GameScene 的卡包交互、开包输入、2D 缺失资源回退及通用运行时接入代码，等待新包导入后重新绑定。
- 清理 `PROJECT_CONTEXT.md` 中旧资源结构、旧命名、旧预览入口和旧 Build Settings 记录。
- 本次不涉及持久化结构变化，不需要删除本地 SQLite 或 JSON 数据。

## 修改文件

- `Assets/Resources/Effects/`（整个目录删除）
- `Assets/Resources/*.unitypackage`（三个旧原始包及 Meta 删除）
- `特效资源/`（整个历史原始资源目录删除）
- `Assets/Scenes/effect.unity`（删除）
- `Assets/Scripts/Controller/CardFxPreviewScene.cs`（删除）
- `Assets/Scripts/Editor/CardFxPreviewMenu.cs`（删除）
- `Assets/Scripts/Editor/CardPackDismantlePreviewEditor.cs`（删除）
- `ProjectSettings/EditorBuildSettings.asset`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不保留空 `Effects` 目录或 `.gitkeep`；新包导入时由 Unity 重新建立目录和 Meta。
- 旧特效资源名不再作为新包命名标准；新包导入后先完整审计，再定义目录、命名和运行时映射。
- 暂不删除通用运行时特效代码，避免同时破坏卡包选择和场景切换流程。

## 验证

- Unity `2022.3.62f2c1` AssetDatabase 刷新成功，日志无脚本、Shader、缺失资源或导入错误。
- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- 仓库扫描确认不存在任何 `.unitypackage`、`.unitypackage.meta` 或旧 `Effect/Effects/CardFx/CardPackDismantle/PlaneGroup` 目录。
- Build Settings 已移除 `effect.unity`；`LoadingScene`、`MainScene`、`GameScene`、`RankScene`、`AchieveScene` 全部加载成功，缺失脚本数为 0。

## 下一步

1. 等待新的特效包；不要直接双击覆盖导入，先解包审计文件、GUID、Shader 管线、材质、动画、灯光和依赖。
2. 确认新资源结构后，再建立新的 `Resources/Effects` 目录、命名约定和运行时映射。

## 恢复提示

继续 Puffies 当前任务。先阅读 AGENTS.md、Documents/WORKFLOW.md 和 Documents/CURRENT_TASK.md；旧特效已经彻底清理，下一步从新特效包的导入前审计开始。
