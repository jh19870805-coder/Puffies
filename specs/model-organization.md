# Model 目录代码整理

- 状态：已完成
- 范围：`Assets/Scripts/Model` 的纯 C# 类型共置与文件数量控制

## 需求

1. WHEN 整理 Model 目录 THEN 工程 SHALL 保持单层目录，不新增领域子目录。
2. WHEN 合并脚本文件 THEN 所有公开类型名、方法签名和调用点 SHALL 保持不变，不改变运行时行为或持久化结构。
3. IF 脚本 Meta GUID 被场景、Prefab 或其他资源引用 THEN 该脚本 SHALL 保持独立文件，不得直接删除。
4. WHEN 多个小型纯 C# 类型属于同一职责范围 THEN 工程 MAY 将它们共置于一个文件；大型或所有权独立模块 SHALL 继续独立。
5. 动画、卡包、任务、配置等已达到数百行以上的独立模块 SHALL NOT 为减少文件数量继续互相合并。

## 设计

- `GameFontUtility` 保持独立公开类型，移动到包含 UI、Sprite、坐标和通用运行时辅助的 `GameCommonUtility.cs`。
- `GameSettingsData` 和 `GameSettingsUtility` 保持独立公开类型，移动到统一管理 JSON、SQLite 和持久化设置的 `LocalDataStore.cs`。
- 删除不再承载类型且没有 Unity 资源引用的 `GameFontUtility.cs/.meta` 与 `GameSettingsUtility.cs/.meta`。
- 不修改 Controller/View 调用点，不改序列化字段、SQLite 集合键或默认值。
- `GameAnimationUtility`、`CardPackDataUtility`、`GameTaskUtility`、`GameConfigRepository`、`CardFxRuntimeUtility` 和 `GameDefine` 保持独立。

## 任务

- [x] 将字体工具类型移动到 `GameCommonUtility.cs`。
- [x] 将设置类型移动到 `LocalDataStore.cs`。
- [x] 删除原脚本和 Meta。
- [x] 编译三个程序集并检查重复类型、丢失脚本引用与差异格式。
- [x] 更新工程上下文和当前任务记录。

## 验收

- `Assets/Scripts/Model` 从10个 `.cs` 减少到8个。
- 公开类型和调用点保持不变。
- `dotnet build Puffies.sln --no-restore` 为0警告、0错误。
- `git diff --check` 通过。

## 验证结果

- Model 目录现有8个 `.cs`；`GameFontUtility.cs` 和 `GameSettingsUtility.cs` 及其 Meta 已删除。
- 两个旧脚本 GUID 在场景、Prefab 和其他非 Meta 资源中的引用数均为0。
- `GameFontUtility`、`GameSettingsData` 和 `GameSettingsUtility` 均只保留一个定义，所有调用点无需修改。
- 使用临时空脚本兼容尚未刷新的 Unity 生成项目清单完成编译，随后立即删除临时文件；三个程序集为0警告、0错误。
- `git diff --check` 通过。
