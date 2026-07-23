# 项目协作说明

本仓库使用 Codex 优先的项目记录方式维护。

修改代码、Unity 场景、资源、配置或已记录行为前：

1. 阅读 `Documents/WORKFLOW.md`。
2. 阅读 `Documents/CURRENT_TASK.md`。
3. 阅读 `Documents/PROJECT_CONTEXT.md` 中与任务相关的稳定项目事实。
4. 记录的下一步与用户最新指令不一致时，以用户最新指令为准。

完成影响行为的修改后，在同一轮更新 `Documents/CURRENT_TASK.md`，记录修改内容、验证方式和下一项有效行动。稳定项目事实发生变化时，同时更新 `Documents/PROJECT_CONTEXT.md`。

对于只读调查、简单问题或工作区整理，除非用户要求，否则不要更新任务记录。

## 新设备环境准备

在另一台设备首次打开本仓库时，排查 C# 项目加载错误前应主动确认开发环境：

1. 使用 `dotnet --list-sdks` 检查是否安装 .NET 8 SDK。
2. 检查 VS Code 扩展 `ms-dotnettools.csharp`、`ms-dotnettools.csdevkit` 和 `visualstudiotoolsforunity.vstuc`。
3. 缺少任何项目时，请求必要的系统或网络授权并为用户安装。WinGet 可用时，SDK 优先使用 `winget install --id Microsoft.DotNet.SDK.8 --exact`；VS Code 扩展使用 `code --install-extension <extension-id>`。
4. 安装或调整 C# 工具后，重新加载 VS Code 窗口。

不要为了绕过编辑器项目加载错误而修改 Unity 生成的 `Assembly-CSharp*.csproj`。Unity 2022.3 生成的是旧格式项目文件，需要 Microsoft Unity VS Code 扩展，或使用现有的 `dotnet.preferCSharpExtension` 兼容设置。

## 代码目录偏好

- 源码目录优先保持浅层、扁平，只使用少量有明确含义的文件夹。
- 除非用户明确要求，不要引入深层目录或大量单一用途文件夹。
- 在同一个扁平目录内，属于同一领域的相关纯 C# 类型可以共用一个文件，但文件仍须清晰易懂。
- 大型模块或所有权独立的模块继续使用独立文件；减少文件数量不能产生新的万能工具文件。
