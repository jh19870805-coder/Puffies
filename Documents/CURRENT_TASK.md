# 当前任务

- 任务：批量替换卡包棋盘标题图片
- 状态：已完成现有卡包资源替换，等待 Unity 导入后视觉验收
- 更新时间：2026-08-05

## 用户意图

- 将 `美术切图/游戏内包头/PackTitleXXX.png` 拷贝到对应的 `Assets/UI/CardBags/CardBagXXX`。
- 目标文件统一命名为 `BoardTitle.png`，替换原棋盘标题。

## 工作记录

- 按三位编号映射，将 `PackTitle001.png` 至 `PackTitle022.png` 分别覆盖到 `CardBag001` 至 `CardBag022` 的 `BoardTitle.png`。
- 只替换 PNG 内容，保留每个目标 `BoardTitle.png.meta`，现有 Sprite GUID 和 Prefab 引用不变。
- `PackTitle023.png`、`PackTitle024.png`、`PackTitle025.png` 暂未处理，因为工程中尚无对应的 `CardBag023`、`CardBag024`、`CardBag025` 目录；源文件保持不变。
- 未修改 Prefab、场景或代码。

## 修改文件

- `Assets/UI/CardBags/CardBag001/BoardTitle.png` 至 `Assets/UI/CardBags/CardBag022/BoardTitle.png`
- `Documents/CURRENT_TASK.md`

## 验证

- 22 组源文件与目标文件逐一执行 SHA-256 比较，0 项不匹配。
- 22 张目标 PNG 均可正常解码，尺寸统一为 `1300 x 164`。
- 所有目标 `BoardTitle.png.meta` 均未变化。
- 目标目录中没有残留 `PackTitleXXX.png` 文件。

## 下一步

1. Unity 完成自动导入后，抽查 GameScene 中各卡包 `BoardTitle` 的显示位置和清晰度。
2. 创建 `CardBag023` 至 `CardBag025` 后，再处理对应的三个包头资源。

## 恢复提示

现有 22 个 CardBag 的 `BoardTitle.png` 已按 `PackTitleXXX.png` 编号替换并保留原 `.meta`。023 至 025 因目标 CardBag 尚不存在而未处理。
