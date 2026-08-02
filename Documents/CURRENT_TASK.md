# 当前任务

- 任务：清理此前导入的全部特效，准备重新整理和导入
- 状态：旧特效已清空，等待确认并导入新的资源包
- 更新时间：2026-08-02

## 用户意图

- 先彻底删除此前导入工程的特效资产和场景配置。
- 保留新的源 `.unitypackage`，后续从干净状态重新导入和接入。

## 工作记录

- 删除 `Assets/Resources/Effects/` 及其 Meta，清除了旧 CardFx、CardPack、PlaneGroup、材质、Shader、纹理、模型、动画和 Prefab。
- 删除旧预览场景 `Assets/Scenes/EffectScene001.unity` 及其 Meta。
- 从 `Assets/Prefabs/PackItem.prefab` 移除旧 `CardPackEffect` 容器和嵌套的制作方 Prefab，避免资源删除后出现 Missing Prefab。
- MainScene 恢复导入特效前的环境光、天空盒和 Sun Source 设置，并删除为旧卡包特效加入的 Directional Light。
- 保留 `特效资源/effect文件夹.unitypackage` 与 `特效资源/场景卡包和特效展示.unitypackage`，没有执行导入、重命名或内容修改。
- 保留现有特效接入业务代码；旧资源缺失期间，MainScene 按既有逻辑使用 2D 卡包回退。待新包导入后再按实际资源路径集中适配。
- 用户新增的 `特效资源/录制_2026_08_01_11_58_42_915.mkv` 未修改。

## 修改文件

- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- 删除 `Assets/Resources/Effects/`
- 删除 `Assets/Scenes/EffectScene001.unity`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 验证

- 已确认 `Assets/Resources/Effects`、对应 Meta、`EffectScene001.unity` 和对应 Meta 均不存在。
- 扫描两个保留资源包中的 441 个旧特效 GUID，工程外部悬空引用为 0。
- `PackItem.prefab` 不再包含 `CardPackEffect` 或旧嵌套 Prefab GUID。
- `dotnet build Puffies.sln --no-restore` 通过，0 警告、0 错误。
- `git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。
- 本轮未启动 Unity Editor；下次打开工程时由 Unity 正常刷新 983 个已删除资产。

## 下一步

1. 确认两个新 `.unitypackage` 的职责和覆盖关系。
2. 按 Unity 原始路径导入需要的特效内容，避免把包内重复的项目 UI 覆盖进工程。
3. 以新包实际 Prefab、场景、灯光和资源路径重新接入首页常驻卡包及开包流程。

## 恢复提示

旧特效已经清空。继续前先读取 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；不要恢复旧 `Effects` 目录或旧 `EffectScene001` 配置，下一步以 `特效资源` 中保留的新包为准。
