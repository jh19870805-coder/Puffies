# 当前任务

- 任务：整理 Model 目录脚本
- 状态：已完成
- 更新时间：2026-07-23

## 用户意图

- 减少 `Assets/Scripts/Model` 下过多的工具脚本文件。
- 保持目录扁平，不为了合并制造新的深层目录或万能工具类。
- 在不改变现有功能、公开 API 和持久化数据的前提下共置相关类型。

## 已完成

- 将独立公开类型 `GameFontUtility` 从 `GameFontUtility.cs` 移入 `GameCommonUtility.cs`；Controller 和 View 调用点保持不变。
- 将 `GameSettingsData` 和独立公开类型 `GameSettingsUtility` 从 `GameSettingsUtility.cs` 移入 `LocalDataStore.cs`；SQLite 集合键、字段默认值和运行时应用逻辑保持不变。
- 删除 `GameFontUtility.cs/.meta` 和 `GameSettingsUtility.cs/.meta`。
- Model 目录由10个 `.cs` 减少为8个。
- 保留动画、卡包、任务、配置、CardFx 和核心定义为独立文件，避免继续扩大现有大文件。
- 新增并完成 `specs/model-organization.md`，记录长期合并边界。

## 修改文件

- `Assets/Scripts/Model/GameCommonUtility.cs`
- `Assets/Scripts/Model/LocalDataStore.cs`
- 删除 `Assets/Scripts/Model/GameFontUtility.cs` 及 `.meta`
- 删除 `Assets/Scripts/Model/GameSettingsUtility.cs` 及 `.meta`
- `specs/model-organization.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 本次只合并文件归属，不合并公开类，也不修改调用点。
- 字体辅助属于通用 UI/运行时辅助，与 `GameCommonUtility` 共置。
- 设置数据与设置持久化依赖本地 SQLite，与 `LocalDataStore` 共置；类职责继续独立。
- `GameAnimationUtility` 和 `CardPackDataUtility` 已超过1000行，其他独立模块也有明确所有权，不再为减少文件数强行合并。

## 验证

- 两个被删除脚本的 Meta GUID 在场景、Prefab 和其他非 Meta 资源中的引用均为0。
- `GameFontUtility`、`GameSettingsData` 和 `GameSettingsUtility` 均只保留一个定义。
- `dotnet build Puffies.sln --no-restore` 完成，runtime、first-pass 和 Editor 程序集均为0警告、0错误。
- Unity 生成的 `.csproj` 尚未自动刷新删除路径；编译时临时使用两个无类型空脚本兼容旧清单，编译后已删除，未修改生成的 `.csproj`。
- `git diff --check` 通过。

## 下一步

1. Unity 编辑器获得焦点后等待资源刷新，确认 Project 窗口的 Model 目录只剩8个脚本且 Console 无 Missing Script。
2. 回到 CardBag 批量生成流程：打开 **Puffies -> Puzzles -> Generate CardBag Prefabs From Images**，验证资源列表和自动 `GameBoard.png` 迁移。

## 恢复提示

继续 Puffies 开发。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；Model 文件整理已完成，下一项有效行动是回到 Unity 确认脚本刷新，然后继续 CardBag 批量生成与 Piece 分组验证。
