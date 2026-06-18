# SPEC 状态面板

- Task: Puffies 新阶段开发
- Status: In Progress
- Updated At: 2026-06-02 16:00
- Previous Phase: 工程目录重组（已完成并验证）

## Requirement Log

- 用户：准备开始新阶段工作。
- 用户：`.cursor` 目录不要在 Cursor 文件树中显示（已写入 `.vscode/settings.json` files.exclude）。
- 新需求：成就页 AchieveScene——MainScene `BtnAchieve` 跳转成就页，成就页 `BtnReturn` 返回 MainScene。
- 新需求：LoadingScene 为启动页，停留约 5 秒，`TextLoading` 显示 0%→100% 后自动进入 MainScene。
- 新需求：GameScene `BtnReturn` 点击返回 MainScene（首页）。

## 基线快照（当前仓库实测）

### 工程状态

- 编译：无 `error CS`（Editor.log）
- 主流程：MainScene → GameScene Play 已验证（用户确认）
- 结构：MVC（`Scripts/Model|View|Controller|Editor`），2D 资源在 `Assets/UI`

### 场景

| 场景 | 状态 |
|------|------|
| **LoadingScene** | 启动页，进度条文字后进 MainScene |
| MainScene | Package001/002/003 卡包 UI |
| GameScene | 编辑器拼图页 + 返回首页 |
| RankScene | 排行榜 + 返回 |
| AchieveScene | 成就页 + 返回（已实现场景跳转） |
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

1. Play 验证：GameScene `BtnReturn` → MainScene；LoadingScene 启动流程
2. 成就列表 / Package002 等见上方待办

## Resume Prompt

`继续 Puffies 新阶段开发，请先读取 Documents/SPEC_STATUS.md`
