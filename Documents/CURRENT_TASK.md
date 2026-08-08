# 当前任务

- 任务：批量替换 CardBag 游戏内包头
- 状态：已完成
- 更新时间：2026-08-08

## 用户意图

- 将 `美术切图/游戏内包头/PackTitleXXX.png` 复制到编号对应的 `Assets/UI/CardBags/CardBagXXX`。
- 删除并替换原有 `BoardTitle.png`，新图片统一使用 `BoardTitle.png` 文件名。

## 工作记录

- 已按 `001` 至 `022` 的编号一一对应，覆盖 22 个 CardBag 目录中的 `BoardTitle.png`。
- 保留每个目标文件原有的 `BoardTitle.png.meta`，确保 Unity Sprite GUID 和现有 Prefab 引用不变。
- 源目录中的 `PackTitleXXX.png` 保持不变。
- `CardBag001` 至 `CardBag006` 的原图与新图内容已经一致；Git 实际记录 `CardBag007` 至 `CardBag022` 的 16 张图片发生变化。
- 未修改或覆盖工作区中已有的游戏逻辑、描边算法及烘焙资源变更。

## 修改文件

- `Assets/UI/CardBags/CardBag007/BoardTitle.png` 至 `Assets/UI/CardBags/CardBag022/BoardTitle.png`
- `Documents/CURRENT_TASK.md`

## 验证

- 22 个目标 `BoardTitle.png` 与对应源 `PackTitleXXX.png` 的 SHA-256 哈希全部一致。
- 22 个 `BoardTitle.png.meta` 均存在且替换前后哈希一致。
- Git 未报告任何 `BoardTitle.png.meta` 修改。

## 下一步

1. 让 Unity 刷新资源，并在 CardBag Prefab 或游戏内抽查包头显示与裁切效果。

## 恢复提示

22 个 CardBag 包头已按编号完成替换，Unity GUID 未变化。下一步只需在 Unity 中刷新并目视确认显示效果。
