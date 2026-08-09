# 当前任务

- 任务：将 CardBag Piece 正式命名升级为四位组号与索引
- 状态：代码、Prefab 迁移和编译完成，等待 Unity Play Mode 回归
- 更新时间：2026-08-09

## 用户意图

- CardBag Prefab 的正式 Piece 名称由两位 `PieceXY` 改为四位 `PieceGGII`。
- `GG` 是两位分组号，`II` 是两位组内索引，不足两位时补 `0`；例如 `Piece11` 改为 `Piece0101`。
- 运行时调用、关卡生成/更新工具和描边烘焙器同步使用新规则。

## 工作记录

- `GameDefine` 增加统一格式化和严格解析 API；组号、索引都只接受 `01..99`，完整持久化编号使用 `group * 100 + index`。
- GameScene 改为严格读取 `PieceGGII`，按前两位分组、后两位排序，并使用新完整编号保存拼图进度。
- 描边烘焙器改为读取四位正式名；三位 `Piece001..Piece999` 继续视为未分组中间态并跳过整包烘焙。
- 关卡生成器的显式分组源文件名改为 `PieceGGII.png` / `PiecesGGII.png`；标准 `piece_###.png` 仍生成三位未分组节点。现有布局更新同时接受四位正式名和三位中间名。
- 21 个已分组 CardBag Prefab 共 619 个正式 Piece 节点已从两位名迁移为四位名。CardBag022 的 196 个三位未分组节点保持不变。

## 修改文件

- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Resources/CardBagPrefabs/CardBag001.prefab` 到 `CardBag021.prefab`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- 全部 CardBag Prefab 共识别 619 个合法四位正式名、196 个合法三位中间名；旧两位名 0、非法名 0、同一 Prefab 内重复名 0。
- `git diff --check` 通过，仅有工作区换行符提示。
- 待在 Unity Play Mode 回归分组顺序、拖拽吸附、进度保存恢复、新手引导和描边加载。

## 下一步

1. 删除旧的 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`，避免旧 Piece 编号进度与新编号混用。
2. 从 LoadingScene 进入 CardBag001，验证三组顺序、新手引导、放置和退出恢复。
3. 随机进入其他已分组卡包，验证描边、提示和一键完成。

## 数据说明

- SQLite 表结构未变化，但 `CardPackPuzzleProgress.PlacedPieceNumbersJson` 中 Piece 完整编号已由旧两位规则改为 `group * 100 + index`。
- 项目处于开发阶段，本次不增加旧进度兼容；测试前需要删除 `LocalData.db`。
