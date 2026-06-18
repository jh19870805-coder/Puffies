# SPEC 状态面板

- Task: Puffies 工程整理（目录、资源、构建同步）
- Status: In Progress
- Updated At: 2026-06-01 23:00
- Auto-Update Mode: Enabled (Maintained by Codex)

## Requirement Log

- （历史需求见下方 Progress Snapshot）
- 新需求：重新整理整个工程（目录结构、冗余清理、统一资源根与构建同步）。
- 问题反馈：整理后编译失败——`GameCommonUtility.BuildResourcePathCandidates` 使用 `List<>` 但缺少 `using System.Collections.Generic`；已修复。

## Progress Snapshot

- 已完成：
  - 历史主流程：编辑器 UI 场景 + Bootstrap 脚本、3D 开包动画、拼图玩法（见 E - Execute 历史项）。
  - **本轮整理**：
    - `Textures` → `ArtRes` 合并，路径常量改为 `ArtResRoot`
    - `Models` → `Core` 重命名
    - `BuildSync.cs` 统一 Configs / ArtRes / CardPack / PlaneGroup 同步
    - 删除 `WindowAspectController`、三个旧 Editor 同步脚本、`U3DMake`、`PackageConfigModel.json`
    - 删除 `LoadBagPieces`、`GetGameBoard`
    - 新增 `ARCHITECTURE.md`、`PROJECT_SETUP.md`、`CLEANUP_CHECKLIST.md`
- 进行中：
  - 等待 Unity Editor 打开验证编译与 Play 回归
- 未完成：
  - `Package002.json` 配置
  - 多页卡包翻页、构建版 3D 动画策略

## Resume Prompt

- `继续当前任务，请先读取 Documents/SPEC_STATUS.md，然后按 Next Action 执行`

## S - Scope

## 当前状态（2026-05）

- 工程已按标准 Unity 目录重组：`Models/`、`Materials/`、`Prefabs/`、`UI/`、`Scripts/`、`Resources/`、`StreamingAssets/`
- `ArtRes/` 已移除；2D 资源在 `UI/`，配置在 `Resources/Config/`
- `BuildSync` 负责 UI/Config → StreamingAssets，Prefabs/Materials → Resources
- 脚本统一在 `Assets/Scripts/`（含 Core、Tools、Editor 子目录）

## 待办

1. Unity 内验证三个场景 Play 模式
2. 补 `Resources/Config/Package002.json`
3. 补 CardPackAni_002+ 动画资源
- 保持现有 Bootstrap + 编辑器 UI 架构不变
- 验收：工程可编译、Play 主流程正常

## P - Plan

1. 目录与资源合并（已完成）
2. 代码路径与 BuildSync 更新（已完成）
3. Unity 打开 → Sync → Play 回归

## E - Execute

- 已完成：`Textures` 物理合并到 `ArtRes`
- 已完成：`Core/` 替代 `Models/`
- 已完成：`BuildSync.cs` + 删除旧同步脚本
- 已完成：删除 `WindowAspectController`、`U3DMake`、死代码
- 已完成：架构文档三份
- 待完成：Editor 验证
- 已修复：CS0246 `List<>` 缺少 using（`GameCommonUtility.cs`）

## C - Check

- 代码：`TexturesRoot` 已移除；`ToDiskPath` 保留 ArtRes/Textures 回退
- 场景：MainScene/GameScene 使用 Bootstrap，无需改 .unity
- 风险：场景 Image 若仍引用旧 Textures 路径 GUID 不受影响（Unity 按 GUID 引用）

## Next Action

1. 打开 Unity → 确认编译通过
2. **Puffies → Sync Build Resources**
3. Play 测试 MainScene → GameScene
4. 若 Package002 需 playable，补 `Configs/Package002.json`
