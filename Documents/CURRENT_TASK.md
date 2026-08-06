# 当前任务

- 任务：修复 CardBag019 自动生成时 piece_014 定位失败
- 状态：代码修复完成，等待 Unity Editor 重新覆盖生成 CardBag019
- 更新时间：2026-08-06

## 用户意图

- 排查当前 Unity 报错并恢复 CardBag019 的自动关卡生成。
- 保留现有 CardBag019 图片、Meta 和美术源文件修改。
- 不能通过全面降低颜色或结构阈值掩盖错误资源和错误位置。

## 工作记录

- Unity `Editor.log` 确认报错不是 C# 编译错误，而是 `CardBag019/piece_014.png` 无法通过 Preview 和 GameBoard 的既有颜色、结构匹配。
- `piece_001` 至 `piece_013` 已正常定位；`piece_014` 与 Preview 中剪刀牌及左侧竖线实际对应，不是错图。
- Preview 的青色分割线和背景差异使正确位置只有 `40.42%` 感知颜色分、`64.18%` 结构分；GameBoard 回退最高为 `67.58%` 颜色分、`79.26%` 结构分，均不应通过原阈值。
- 增加最终 Alpha 轮廓回退：只对 Preview 启用，只在精确、感知颜色和结构匹配全部失败后执行；最低轮廓匹配 `75%`，并要求领先远端候选至少 `8%`。
- 回退使用 Piece 的 Alpha 外边界匹配 Preview 青色分割线邻域，不参与 GameBoard 回退，也不降低既有颜色和结构阈值。
- 唯一性校验会单独细化搜索最佳点以外的远端候选，避免最佳点附近候选占满列表后被误判为唯一。

## 修改文件

- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不修改或恢复用户当前 CardBag019 的 24 张 Piece、GameBoard、Preview、Meta 及美术源目录变更。
- Alpha 轮廓只作为 Preview 的最后安全回退，不替代既有颜色定位流程。
- 轮廓回退同时要求绝对匹配率和远端候选分差，避免相似外形被自动放到错误位置。

## 验证

- 离线核对 `piece_014` 正确 Preview 顶部坐标为 `(556,466)`，对应 Unity 底部坐标约 `(556,219)`。
- 正确位置 Alpha 轮廓匹配约 `89.16%`，最强远端候选约 `65.02%`，分差约 `24.14%`，满足统一安全规则。
- `dotnet build Puffies.sln --no-restore`：成功，0 个警告、0 个错误。
- `git diff --check -- Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`：通过，仅有仓库既有的 LF/CRLF 提示。
- 尚未在 Unity 中执行覆盖生成；当前旧 CardBag019 Prefab 仍可能引用已删除的 `piece_025/026`，必须重新生成成功后才能使用。

## 下一步

1. 切回 Unity 等待脚本刷新，在生成窗口选择 CardBag019 并确认覆盖。
2. 确认日志中 `piece_014` 通过 `Alpha boundary` 定位，坐标接近 `(556,219)`。
3. 确认批量结果为 `generated=1, failed=0`，并检查新 Prefab 只包含当前 24 张 Piece。

## 恢复提示

CardBag019 的 `piece_014` 因 Preview 青色分割线和背景差异无法通过颜色、结构匹配；生成器已增加仅用于 Preview 的 Alpha 轮廓安全回退。下一步在 Unity 生成窗口覆盖生成 CardBag019，并确认 `generated=1, failed=0`。
