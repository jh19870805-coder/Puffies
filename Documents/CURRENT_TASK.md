# 当前任务

- 任务：校准成就页与排行榜条目间距
- 状态：实现与编译验证完成，等待 Play Mode 视觉确认
- 更新时间：2026-07-31

## 用户意图

- 仔细对比 `Assets/UI/TempImages/成就.png` 和 `排行榜.png`，修正两个页面的 Item 间距。
- 调整必须以效果图和素材实际像素为准，不能只凭观感修改。

## 工作记录

- 确认两张效果图与项目 Canvas 均使用 `2560 x 1440` 设计分辨率，可直接逐像素比较。
- 成就页效果图中卡片为 `240 x 332`，水平和垂直步距分别为 `280`、`372`；现有 `GridLayoutGroup` 的 `CellSize=240 x 332`、`Spacing=40 x 40` 已准确匹配，因此未修改。
- 排行榜效果图中相邻条目中心步距为 `153px`；`RankItem` 根高度为 `148px`，因此将 `VerticalLayoutGroup.spacing` 从 `10` 调整为 `5`。
- 效果图前三名背景使用原生 `1646 x 148`，普通背景为 `1636 x 136`；前三名动态换图后调用 `SetNativeSize()`，避免被普通背景 RectTransform 拉伸成 `1636 x 136`。
- 本次未修改数据结构或持久化内容，无需删除本地数据。

## 修改文件

- `Assets/Scenes/RankScene.unity`
- `Assets/Scripts/Controller/RankScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 成就页现有间距已经匹配效果图，不为制造改动而调整正确参数。
- 排行榜使用 `148 + 5 = 153px` 的固定条目步距；前三名与普通条目的背景尺寸差异由素材原生尺寸保留。

## 验证

- 已对效果图和四张排行榜背景源图执行像素定位匹配：前三名顶部坐标依次为 `172 / 325 / 478`，普通条目中心继续以 `153px` 递增。
- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过，仅有仓库现有的 LF/CRLF 转换提示。
- Unity 批处理进程在写出任何编辑器日志前停住；已结束两次无效进程，未把该项视为场景验证通过或失败。
- 尚未在 Unity Play Mode 中确认最终视觉。

## 下一步

1. 在 Unity Play Mode 打开 AchieveScene，确认 6 列卡片横纵间距保持与效果图一致。
2. 打开 RankScene，确认前三名背景不再变形，全部条目按 `153px` 中心步距排列。
3. 检查滚动到底部时最后一条没有被 Content 或 Viewport 裁切。

## 恢复提示

继续 Puffies 当前任务。排行榜间距已改为 `5`，前三名背景已恢复原生尺寸；下一步在 Unity Play Mode 对比成就页和排行榜效果图。
