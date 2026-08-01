# 当前任务

- 任务：统一 CardBag 碎片资源命名
- 状态：资源重命名完成，Prefab GUID 引用已验证
- 更新时间：2026-08-02

## 用户意图

- 扫描 `Assets/UI/CardBags/CardBagXXX`，保留 `BoardTitle.png` 和 `GameBoard.png`。
- 其他拼图碎片如果不是 `piece_xxx.png` 格式，则从 `piece_001.png` 开始按原文件数字顺序重命名。
- 已有 Prefab 使用这些纹理时同步保证引用有效。

## 工作记录

- `CardBag001`：`Pieces001..008.png` 改为 `piece_001..008.png`。
- `CardBag007`：19 张 `图层N.png` 按数字自然顺序改为 `piece_001..019.png`。
- `CardBag009`：36 张 `PiecesNN.png` 按数字自然顺序改为 `piece_001..036.png`。
- `CardBag017`：37 张 `PiecesNN.png` 按数字自然顺序改为 `piece_001..037.png`。
- `CardBag002` 原文件均已符合目标格式，因此保持原编号不变，包括现有编号缺口。
- 001、009、017 的 PNG 与 `.meta` 成对移动，GUID 未改变；007 当前没有 `CardBag007.prefab` 且源 PNG 尚无 Meta，等待 Unity 导入时生成。
- PackSize 一键更新工具同步收紧为只统计标准名 `piece_NNN.png`。

## 修改文件

- `Assets/UI/CardBags/CardBag001/`
- `Assets/UI/CardBags/CardBag007/`
- `Assets/UI/CardBags/CardBag009/`
- `Assets/UI/CardBags/CardBag017/`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 每个 CardBag 独立从 1 编号；旧文件按文件名末尾数字做自然排序，避免 `1、10、11、2` 的字典序错误。
- Prefab 的 Sprite 引用由 GUID 确定，不修改 Prefab YAML；保留并移动原 `.meta` 即完成引用同步。
- 已符合 `piece_三位数字.png` 格式的文件不因编号缺口而重排，避免无必要地改变现有资源路径。

## 验证

- 已扫描全部 `CardBagXXX` 一级目录，除 `BoardTitle.png`、`GameBoard.png` 外没有不符合 `piece_三位数字.png` 的 PNG。
- `CardBag001` 的 8/8、`CardBag009` 的 36/36、`CardBag017` 的 37/37 个改名后 Meta GUID 均仍存在于对应 Prefab。
- 代码和序列化资源中没有按旧 PNG 文件名保存的字符串引用。
- Unity 当前进程没有可交互窗口，资源导入和 007 Meta 生成待编辑器重新聚焦或重启后确认。

## 下一步

1. 重新聚焦或重启 Unity，等待 AssetDatabase 完成重命名资源导入。
2. 检查 Console 无 Missing Sprite，并打开 CardBag001、009、017 Prefab 确认 Image Sprite 正常。
3. 生成 `CardBag007.prefab` 后验证新资源布局和纹理匹配。

## 恢复提示

继续 Puffies CardBag 碎片命名回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；资源已统一为 `piece_NNN.png`，先完成 Unity 导入和 Prefab 可视检查，不要回退用户已有的 CardBag007、022 或美术源文件改动。
