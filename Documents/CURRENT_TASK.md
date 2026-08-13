# 当前任务

- 任务：工程冗余代码保守清理
- 状态：已完成代码审计、清理和编译验证
- 更新时间：2026-08-13

## 用户意图

- 整理工程并删除能够确认不再使用的代码。
- 不改变已经验证的卡包排序、开包动画、拼图交互、任务结算和编辑器工具行为。

## 工作记录

- 全仓库审计 21 个 C# 脚本，结合编译结果、标识符引用、Unity 特性入口、脚本 Meta GUID 资源引用和当前场景层级筛选删除候选。
- MainScene 当前只使用 `PackageScrollView/Content/Page_1` 与 `PackItem` 的 6 x 3 分页列表，全工程不存在旧 `Package001` 对象；删除旧列表解析、模板字段、手工横向布局和分页/旧版双分支。
- 卡包排序入口 `CardPackDataUtility.TakeMainSceneOrderedPackIds()`、按顺序创建分页条目和现有分页布局均保留。
- 删除 `BuildSync` 中针对已经不存在的 11 个旧资源目录和 4 个旧 StreamingAssets 根目录的一次性迁移清理；保留正式 UI 同步、编辑器菜单和构建前同步回调。
- 孤立公开类型扫描只发现 `PackCoverShadowEffect`，但其脚本 GUID 被 Prefab 引用，因此未删除；私有单次引用候选均为 Unity 初始化特性回调，也全部保留。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Editor/BuildSync.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 只有能够证明不存在 C#、Unity 序列化、菜单、构建回调或反射入口的代码才删除。
- 不按文件大小拆分 `MainScene`、`GameScene` 或编辑器生成器；本轮目标是净删除，不做结构性重构。
- `PackCoverShadowEffect` 等由 Unity 资源引用的组件即使没有普通 C# 调用也必须保留。

## 验证

- `Assembly-CSharp.csproj` 编译通过，`0` 警告、`0` 错误。
- `Assembly-CSharp-Editor.csproj` 编译通过，`0` 警告、`0` 错误。
- 搜索确认已删除的旧列表字段、方法和 BuildSync 迁移字段、方法无残留引用。
- 静态核对确认卡包排序仍使用 `TakeMainSceneOrderedPackIds()`，并按原索引创建分页条目。
- `git diff --check` 通过。

## 下一步

1. 在 Unity 进入 MainScene，确认首页卡包按原顺序显示并可正常翻页、点击。
2. 后续发现新的废弃功能时，继续按 Unity 资源引用与运行入口双重核对后清理。

## 恢复提示

本轮已删除旧 `Package001` 列表兼容代码和 BuildSync 一次性旧目录清理；当前首页只支持正式分页 PackItem 列表，卡包排序逻辑完整保留。
