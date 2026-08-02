# 当前任务

- 任务：扩展卡包尺寸与棋盘缩放自动更新工具
- 状态：代码与配置更新完成，待 Unity Editor 菜单回归
- 更新时间：2026-08-02

## 用户意图

- 更新卡包尺寸时，根据尺寸同步更新 `BoardScale`。
- `CardPacks.csv` 最后一列增加 `AutoUpdate`，默认填 `1`。
- 手工将某行 `AutoUpdate` 改为 `0` 后，工具不再修改该行的 `PackSize` 和 `BoardScale`。

## 工作记录

- 保留菜单 `Puffies -> Card Packs -> Update Pack Sizes From Piece Counts`，继续只统计 `Assets/UI/CardBags/CardBagNNN` 顶层的 `piece_NNN.png`。
- 工具根据现有片数区间确定 `PackSize`，并在同一轮按固定映射更新 `BoardScale`：
  - `XS=0.75`
  - `S=0.78`
  - `M=1.10`
  - `L=1.30`
  - `XL=1.00`
  - `XXL=1.15`
  - `XXXL=1.30`
- 工具缺少 `AutoUpdate` 列时会将它追加为最后一列，空值按默认值 `1` 补齐；字段只允许 `0` 或 `1`。
- `AutoUpdate=0` 的行会在结果中列为跳过，原有 `PackSize` 和 `BoardScale` 保持不变。
- `GameConfigRepository` 已将 `AutoUpdate` 解析到 `CardPackConfigData`，非法值会使该行配置加载失败并输出原有警告。
- 当前 `CardPacks.csv` 的 22 行已全部设为 `AutoUpdate=1`，并按实际碎片数同步尺寸和缩放比；CardBag007 的 19 片已从错误的 `XXXL` 修正为 `XS`。

## 修改文件

- `Assets/Resources/Configs/CardPacks.csv`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- `AutoUpdate` 必须位于 CSV 最后一列，避免工具行为随列位置产生歧义。
- `AutoUpdate=0` 同时保护尺寸和缩放比，而不是只保护其中一个字段。
- 缩放比使用固定两位小数写入，保证工具重复执行后 CSV 内容稳定。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过；仅有仓库既有的 LF/CRLF 转换提示。
- PowerShell CSV 结构检查：22 行，字段顺序为 `Index,PackId,PackSize,ChapterId,BoardScale,AutoUpdate`，无非法 `AutoUpdate` 或非正数 `BoardScale`。
- 已按 `piece_NNN.png` 实际数量核对 CardBag001 至 CardBag022 的配置结果。
- 尚未在 Unity Editor 中手工将一行设为 `AutoUpdate=0` 后执行菜单回归。

## 下一步

1. 在 Unity Editor 中执行 `Puffies -> Card Packs -> Update Pack Sizes From Piece Counts`，确认结果显示扫描 22 行且无重复改写。
2. 回归保护规则时，可临时将任意一行 `AutoUpdate` 设为 `0` 并手工修改尺寸与缩放比，执行工具后确认两个值都保持不变。

## 恢复提示

继续 Puffies 卡包尺寸工具回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；尺寸、缩放比与 `AutoUpdate` 已接入，下一步在 Unity Editor 验证菜单幂等性和 `AutoUpdate=0` 跳过行为，不要回退用户现有 CardBag、特效、场景或描边修改。
