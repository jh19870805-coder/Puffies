# SPEC 状态面板

- Task: Puffies 新阶段开发
- Status: In Progress
- Updated At: 2026-06-02 12:15
- Previous Phase: 工程目录重组（已完成并验证）

## Requirement Log

- 用户：准备开始新阶段工作。
- 用户：`.cursor` 目录不要在 Cursor 文件树中显示（已写入 `.vscode/settings.json` files.exclude）。

## 基线快照（当前仓库实测）

### 工程状态

- 编译：无 `error CS`（Editor.log）
- 主流程：MainScene → GameScene Play 已验证（用户确认）
- 结构：MVC（`Scripts/Model|View|Controller|Editor`），2D 资源在 `Assets/UI`

### 场景

| 场景 | 状态 |
|------|------|
| MainScene | Package001/002/003 卡包 UI |
| GameScene | 编辑器拼图页 |
| RankScene | 已加控制器脚本 |
| effect | 特效调试（可选删） |

### 配置与资源缺口

| 项 | 现状 |
|----|------|
| `Package001.json` | 有，对应 `UI/Game001/` |
| `Package002.json` | **无**（MainScene 已有 Package002） |
| `Package003.json` | **无**（MainScene 已有 Package003） |
| `UI/Game002`、`Game003` | **无**（仅 Game001 拼图资源） |
| `CardPackAni_001.FBX` | 有；002/003 无，点击会走 2D fallback |

### 构建

菜单：**Puffies → Sync Build Resources**（同步 UI → StreamingAssets、Config → StreamingAssets）

## 新阶段待办（按优先级）

1. [ ] `Resources/Config/Package002.json`（需 `UI/Game002/` 或临时复用 Game001）
2. [ ] `Package003.json` + `UI/Game003/`（同上）
3. [ ] `CardPackAni_002+` FBX（或接受 fallback 到 002/003）
4. [ ] RankScene 功能接线与回归
5. [ ] 多页卡包翻页（如需要）
6. [ ] Steam 成就占位 / Steamworks 接入（物料未齐，可后做）
7. [ ] 打包构建回归

## Next Action

1. 确认本阶段第一个目标（建议：**Package002 可点击进入游戏**）
2. 若暂无 Game002 美术：先建 `Package002.json` 临时指向 `Game001` 打通流程，或等 `UI/Game002/` 资源到位再写配置
3. Unity → **Sync Build Resources** → Play 验证

## Resume Prompt

`继续 Puffies 新阶段开发，请先读取 Documents/SPEC_STATUS.md`
