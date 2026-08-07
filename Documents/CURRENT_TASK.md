# 当前任务

- 任务：扁平化 Unity 编辑器工具菜单
- 状态：已完成
- 更新时间：2026-08-07

## 用户意图

- 将项目自定义编辑器工具集中到一个 `Puffies` 菜单。
- 删除 `Canvas`、`Fonts`、`Puzzles`、`Card Packs` 等多层子菜单，减少查找路径。

## 工作记录

- 7 个有效编辑器工具现在全部直接显示在 Unity 顶层 `Puffies` 菜单中。
- 使用 `MenuItem` 优先级保持构建同步、卡包工具、Canvas 工具和字体工具的稳定显示顺序。
- 同步修改 GameScene 的描边缺失提示，以及项目上下文和拼图描边文档中的旧菜单路径。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/BuildSync.cs`
- `Assets/Scripts/Editor/CanvasDesignResolutionEditor.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Editor/DefaultChineseFontEditor.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/puzzle-outline.md`

## 决策

- 保留各工具原有英文名称和功能，只移除菜单路径中的中间层级。
- 不新增统一工具窗口；所有命令仍直接调用原实现。
- 历史上已删除的开包特效预览菜单不恢复。

## 验证

- 搜索确认 7 个有效 `MenuItem` 均为 `Puffies/<工具名>`，不存在 `Puffies/Canvas`、`Puffies/Fonts`、`Puffies/Puzzles` 或 `Puffies/Card Packs` 代码入口。
- `git diff --check` 通过。
- 使用 Unity `2022.3.62f2c1` 无界面模式完成脚本编译，返回码为 `0`，日志无 C# 错误或警告。
- Unity 未修改场景、Prefab、资源或配置文件。

## 下一步

1. 打开 Unity，确认顶层 `Puffies` 菜单直接显示 7 个工具入口且点击正常。

## 恢复提示

Unity 编辑器工具菜单已扁平化并通过编译。下一步仅需在编辑器中目视确认菜单顺序和入口显示。
