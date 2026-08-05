# 当前任务

- 任务：卡包配置增加贴纸数量字段
- 状态：已完成
- 更新时间：2026-08-03

## 用户意图

- 在 `CardPacks.csv` 的 `PackSize` 旁边增加贴纸数量列，不放在最后一列。
- 数量来自 `Assets/UI/CardBags/CardBagNNN` 中的贴纸图片，不计算 `GameBoard.png` 和 `BoardTitle.png`。
- 后续使用现有一键工具更新卡包尺寸时，同时刷新贴纸数量。

## 工作记录

- 新增 `StickerCount` 列，列顺序为 `Index,PackId,PackSize,StickerCount,ChapterId,BoardScale,AutoUpdate`。
- 配置更新工具在缺少 `StickerCount` 时自动将其插入 `PackSize` 后面，并用顶层标准资源 `piece_NNN.png` 的数量更新该字段。
- `AutoUpdate=1` 同步更新 `PackSize`、`StickerCount` 和 `BoardScale`；`AutoUpdate=0` 时三项均保持手工配置。
- `CardPackConfigData` 增加 `StickerCount`，运行时加载配置时要求该值为正整数。
- 当前 22 个卡包的 `StickerCount` 已按资源目录实际数量写入配置表。

## 修改文件

- `Assets/Resources/Configs/CardPacks.csv`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 验证

- 逐包比较 CSV 与 `Assets/UI/CardBags/CardBagNNN/piece_NNN.png`，22 行全部一致，0 项不匹配。
- `dotnet build Puffies.sln --no-restore` 通过，0 警告、0 错误。
- `git diff --check` 通过，仅有仓库既有的 LF/CRLF 转换提示。

## 下一步

1. 后续导入新卡包并增加配置行后，执行 `Puffies -> Card Packs -> Update Pack Sizes From Piece Counts`，自动同步尺寸、贴纸数量和棋盘缩放比。

## 恢复提示

卡包配置已增加 `StickerCount`，固定放在 `PackSize` 后面。更新工具只统计 CardBag 顶层的标准 `piece_NNN.png`，并同时维护 `PackSize`、`StickerCount` 和 `BoardScale`。
