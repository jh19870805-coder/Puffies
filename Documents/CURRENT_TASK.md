# 当前任务

- 任务：修复 CardBag013 贴图正确落位时二次放大
- 状态：代码与编译验证已完成，等待 Play Mode 验证
- 更新时间：2026-08-11

## 用户意图

- CardBag013 的贴图从托盘拿起后已经达到棋盘目标尺寸，正确放到棋盘时不能再次放大。
- 修复应沿用通用缩放规则，不为 CardBag013 配置单独倍率。

## 工作记录

- 核对 CardBag013：根节点、GameBoard 和全部 Piece 的本地缩放均为 `1`；Piece PPU 为 `100`、Pivot 为中心，Prefab Rect 与对应 PNG 像素尺寸一致。
- CardBag013 棋盘尺寸为 `1300 x 1513`、`BoardScale=1.1`；它比相邻卡包更高，运行时相机与 Canvas 换算误差更容易在 SpriteRenderer 切换为 Image 时暴露。
- 拖拽目标缩放改为使用当前 SpriteRenderer 的实际屏幕包围盒与目标凹槽屏幕矩形直接计算。
- 每次拿起 Piece 时重新校准 `DragScale`，不继续完全依赖分组创建时缓存的世界坐标换算结果。
- 原世界尺寸算法保留为屏幕测量不可用时的回退。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不修改 CardBag013 Prefab、源贴图、`BoardScale` 或托盘缩放规则。
- 不取消按下时从 `TrayScale` 到棋盘目标比例的首次放大，只消除正确落位切换渲染器时的第二次尺寸变化。
- 本次不修改持久化结构，不需要清理本地数据。

## 验证

- 修正校准结果 Z 比例后，`Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 再次顺序编译通过，均为 `0` 警告、`0` 错误。
- 待在 Play Mode 验证 CardBag013 多个分组，以及 CardBag012/015 的回归表现。

## 下一步

1. 在 Play Mode 拿起 CardBag013 的普通 Piece 并正确落位，确认落位前后尺寸连续。
2. 抽查 CardBag012 和 CardBag015，确认通用屏幕校准没有改变正常卡包。

## 恢复提示

CardBag013 资源缩放本身正常；拖拽目标尺寸已改为按 SpriteRenderer 与凹槽的实际屏幕尺寸校准，并在拿起时刷新。两个 C# 项目已编译通过，下一步 Play Mode 验证。
