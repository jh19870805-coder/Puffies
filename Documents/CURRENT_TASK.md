# 当前任务

- 任务：跨设备本地缓存长期维护
- 状态：已实现并在当前设备注册每周计划任务
- 更新时间：2026-08-20

## 用户意图

- 定期检查 Puffies 工程本地缓存，达到阈值时安全清理，避免 `Library/Bee` 等可重建内容长期占用大量空间。
- 将规则固化为长期任务，并在拉取仓库后让其他设备能够发现和安装相同维护任务。

## 工作记录

- 新增根目录 `ProjectMaintenance.ps1`，默认或 `-Audit` 只输出容量报告；`-Clean` 只在达到固定阈值后清理白名单路径。
- 阈值为：`Library >= 10 GiB`、`Library/Bee >= 4 GiB`、临时目录合计 `>= 500 MiB`、磁盘可用空间 `< 25 GiB`，或 Git 松散对象 `>= 500` 个/`>= 256 MiB`。
- 自动清理不会删除 `Library/Artifacts`、`Library/PackageCache`、项目资源、用户设置或完整 `Library`；Git 维护只使用默认安全保留期的 `git gc`。
- 脚本支持安装和卸载 Windows 计划任务；任务名为 `Puffies Project Maintenance`，每周日 `03:00` 运行，错过后尽快补跑。
- 已将新设备审计和本机任务注册要求写入 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/PROJECT_CONTEXT.md`。
- Git 会同步脚本、阈值和工作流，但 Windows 计划任务必须在每台设备单独注册一次。

## 修改文件

- `ProjectMaintenance.ps1`
- `AGENTS.md`
- `Documents/WORKFLOW.md`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 使用确定性的本地 PowerShell 脚本和 Windows 任务计划程序执行清理，不依赖模型每周重新判断删除范围。
- 脚本保存在仓库根目录，避免增加深层工具目录；日志写入 Git 已忽略的 `UserSettings/ProjectMaintenance.log`。
- Codex Scheduled Task 不作为实际清理载体，因为本地项目任务要求对应电脑开机且桌面应用运行；仓库脚本和 Windows 计划任务更适合跨设备复用。

## 验证

- PowerShell 5.1 静态语法解析：通过。
- 默认无参数审计：通过；当前未达到任何清理阈值。
- `-Clean` 低阈值验证：通过，没有删除文件。
- `-Clean` 达阈值验证：使用 `501 MiB` 稀疏临时文件触发后，只删除 `Temp` 并正确报告释放空间；受保护目录均保留。
- 多操作参数保护：通过，同时指定 `-Audit -Clean` 时拒绝执行。
- 当前设备计划任务：`Ready`，脚本路径为 `E:\UnityProjects\Puffies\ProjectMaintenance.ps1`，下次运行时间 `2026-08-23 03:00`。
- 后台手工试跑计划任务：返回结果 `0`，低于阈值时未执行删除。

## 下一步

1. 下次在其他设备拉取仓库后，按 `AGENTS.md` 先运行 `ProjectMaintenance.ps1 -Audit`。
2. 确认该设备缺少 `Puffies Project Maintenance` 后，运行 `ProjectMaintenance.ps1 -InstallScheduledTask` 完成本机注册。
3. 下次 Unity 正常启动时等待一次脚本与构建缓存重建，并确认编辑器进入工程。

## 恢复提示

继续 Puffies 工程。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；本机缓存维护脚本已完成，其他设备首次打开时必须审计并单独注册 Windows 计划任务。
