# 当前任务

- 任务：为关卡生成工具增加现有 Piece 布局更新功能
- 状态：代码完成，等待 Unity 窗口内操作验证
- 更新时间：2026-08-09

## 用户意图

- 更新少量卡包切图后，可使用效果图重新校准现有 Prefab 中对应 Piece 的位置。
- 不重新生成整个 Prefab，不丢失手工分组和编辑器配置。
- 不修改影子，不重烘焙或修改描边。

## 工作记录

- 在 `Puffies -> Generate CardBag Prefabs From Images` 窗口增加 `Select Existing` 和 `Update Existing Piece Layouts`。
- 提取并复用现有 Preview/GameBoard 图像匹配流程，生成和更新使用同一套定位标准。
- 更新操作通过现有 Image 的 Sprite 资源路径映射源 PNG，因此 `piece_001.png` 可以继续对应手工改名后的 `Piece11`。
- 只写入已有 Piece 的 `RectTransform.anchoredPosition` 和 `sizeDelta`；保留层级、对象名、手工分组、Image 参数、颜色、影子、旋转、缩放和描边资源。
- 整包先计算和校验，全部通过后才保存。源文件数量或引用不一致、定位失败、重复 Sprite 引用、拉伸锚点，或目标高 Alpha 区域重叠达到 `65%` 时，本包不会保存。
- 批量更新逐包隔离失败，结果弹窗显示成功包数、实际变化 Piece 数和失败原因。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 验证

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `git diff --check` 通过，仅有工作区换行符提示。
- 当前已有 Unity 进程运行，因此未启动第二个 Batch Mode 实例；尚未在工具窗口实际点击更新。

## 下一步

1. 等 Unity 完成脚本重载后，打开 `Puffies -> Generate CardBag Prefabs From Images`。
2. 点击 `Select Existing`，也可以只勾选目标卡包，再点击 `Update Existing Piece Layouts`。
3. 先用当前 `CardBag002` 验证重复的 `piece_025.png` 会报告重叠且不保存；修正该切图后重新执行并目视检查接缝。

## 数据说明

- 本次没有修改 JSON、SQLite 或业务数据结构，不需要删除本地存储。
