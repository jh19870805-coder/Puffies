# 整理清单

## 已完成（2026-06-01）

- [x] `Assets/Textures/` 合并入 `Assets/ArtRes/`（PackImages、Game001、BasicUI、MainBg）
- [x] `GameDefine.TexturesRoot` → `ArtResRoot`，增加设计分辨率常量
- [x] `Assets/Models/` 重命名为 `Assets/Core/`
- [x] 三个 Editor 同步脚本合并为 `BuildSync.cs`（菜单 **Puffies → Sync Build Resources**）
- [x] 删除 `WindowAspectController`
- [x] 删除 `U3DMake/` 孤立资源
- [x] 删除 `PackageConfigModel.json` 重复模板
- [x] 删除 `GameManager.LoadBagPieces`、`GetGameBoard`
- [x] CardPack Resources 同步仅保留 `mesh_skin_*` prefab
- [x] 文档：`ARCHITECTURE.md`、`PROJECT_SETUP.md`

## 待验证（Unity Editor）

- [ ] 重新打开工程，确认无编译错误
- [ ] **Puffies → Sync Build Resources**
- [ ] Play MainScene：Package001/002 可见、可点击、开包动画
- [ ] Play GameScene：拼图全流程
- [ ] Build 后验证 StreamingAssets/ArtRes

## 可选后续

- [ ] 添加 `Configs/Package002.json`（场景已有 Package002）
- [ ] 清理 `Resources/CardPack/` 中多余的 `mesh_cardPack_*`（旧同步残留）
- [ ] MainScene 多页卡包翻页 UI
- [ ] 构建版 3D 卡包销毁策略
