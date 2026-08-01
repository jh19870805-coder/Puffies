# 当前任务

- 任务：按卡包碎片数量一键更新 PackSize
- 状态：实现完成，已更新当前配置并通过静态验证
- 更新时间：2026-08-01

## 用户意图

- 扫描 `Assets/UI/CardBags/CardBagNNN` 中的卡包源资源，按实际碎片 PNG 数量更新 `CardPacks.csv/PackSize`。
- `BoardTitle.png`、`GameBoard.png` 和其他非碎片图片不得计入。
- 后续导入新卡包资源后，可以从 Unity 菜单一键重复更新。

## 工作记录

- 在现有 `CardBagPrefabGeneratorEditor` 中新增菜单 `Puffies -> Card Packs -> Update Pack Sizes From Piece Counts`。
- 工具只统计卡包目录顶层的正式分组名 `PiecesNNN.png` 和新导入阶段名 `piece_NNN.png`；其他 PNG 自动排除。
- 尺寸规则为：`0..29=XS(1)`、`30..37=S(2)`、`38..49=M(3)`、`50..69=L(4)`、`70..84=XL(5)`、`85..99=XXL(6)`、`>=100=XXXL(7)`。
- 工具使用项目统一 `CsvTable` 读取 CSV，按 `PackId` 更新 `PackSize`，保留其他列、行顺序、换行格式和 CSV 引号规则。
- 没有合法碎片的资源目录不会被写成 XS；资源目录没有配置行、配置行没有资源目录或 PackId 重复时会报告。
- 已按相同规则直接更新当前 `CardPacks.csv`。21 个现有资源目录全部匹配；`CardBag007` 没有源目录，因此 PackId 7 保持原配置值。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Resources/Configs/CardPacks.csv`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 将工具放入现有 CardBag 编辑器模块，不新增单一用途编辑器文件。
- 同时支持 `PiecesNNN.png` 和 `piece_NNN.png`，保证资源刚导入、尚未人工分组时也能正确统计。
- 工具只更新已有 CSV 行，不根据文件夹自动创建章节、BoardScale 等其他业务配置。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- 使用独立统计脚本逐项核对 21 个 CardBag 目录，资源片数映射结果与更新后的 CSV 全部一致。
- 边界实现严格使用 `<30`、`<38`、`<50`、`<70`、`<85`、`<100` 和其余 `>=100`。
- `git diff --check`：通过，仅有工作区现有的行尾转换提示。
- 当前 Unity 进程尚未刷新最新 Editor 程序集，因此菜单点击流程待编辑器重新聚焦或重启后验证。

## 本地数据重置

- `CardPacks` SQLite 表和当前任务 JSON 可能仍保留旧尺寸。测试新尺寸规则前，关闭 Play Mode，删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db` 和 `LocalData.json`。
- 未自动删除用户本地存档。

## 下一步

1. 重新聚焦或重启 Unity，执行 `Puffies -> Card Packs -> Update Pack Sizes From Piece Counts`，确认结果弹窗和 Console 变更摘要。
2. 清理本地开发存档后进入 MainScene，验证尺寸图标和尺寸限定任务。

## 恢复提示

继续 Puffies 卡包尺寸更新工具回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；当前 CSV 已按资源片数更新，下一步验证 Unity 菜单执行和重置存档后的尺寸业务表现。
