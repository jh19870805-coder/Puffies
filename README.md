# Puffies

Unity 2D / URP 卡包开包、拼图拖拽与任务奖励项目。

## 环境

- Unity **2022.3.62f2c1**
- URP **14.0.12**
- 设计分辨率 **2560×1440**，PPU **100**

## 快速结构

```text
Assets/
  Scenes/          LoadingScene、MainScene、GameScene、RankScene、AchieveScene、effect
  Scripts/         Model / View / Controller / Editor
  UI/              2D 贴图源
  Resources/       Configs、Effects（CardPack、CardFx…）
  StreamingAssets/ 构建同步的 UI
Documents/         项目与开发文档
```

## 场景流

`LoadingScene` → `MainScene` →（开包）→ `GameScene`（拼图 + 任务进度）→ `RewardPanel` → `MainScene`（卡包列表刷新）；Rank / Achieve / effect 从 Main 或菜单进入。

## 常用菜单

| 菜单 | 用途 |
|------|------|
| Puffies → Sync Build Resources | `UI` → StreamingAssets |
| Puffies → Canvas → Apply Design Resolution | 2560×1440 |
| Puffies → Fonts → Setup Default Chinese Font | Noto Sans SC |
| Puffies → Preview CardFx Effects | CardFx 预览场景 |

## 文档

| 文件 | 说明 |
|------|------|
| [Documents/PROJECT_SETUP.md](Documents/PROJECT_SETUP.md) | 目录、场景、资源、构建、命名（**主参考**） |
| [Documents/SPEC_STATUS.md](Documents/SPEC_STATUS.md) | 当前任务与进度 |
| [Documents/SPEC_WORKFLOW.md](Documents/SPEC_WORKFLOW.md) | SPEC 工作流与新任务模板 |

## 注意

- Build Settings 启动场景必须是 **LoadingScene**。
- 改名 `Resources` 路径或场景对象名（`GameBoard`、`Piece01`、`BtnReturn` 等）需同步改 `GameDefine` 与加载代码。
- 文本文件使用 UTF-8。
