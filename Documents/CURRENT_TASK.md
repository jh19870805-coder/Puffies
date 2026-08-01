# 当前任务

- 任务：扩展关卡描边与贴纸描边辅助开关
- 状态：代码与描边资源生成完成，待 Play Mode 四种开关组合画面验证
- 更新时间：2026-08-02

## 用户意图

- “关卡描边”打开时，显示当前待拼组的完整外描边；关闭时保持现有逻辑，只显示当前阶段连接区域。
- “贴纸描边”打开时，显示当前待拼组每一块凹槽的描边；关闭时不显示单块凹槽描边。
- 两个开关初始化默认都关闭，默认视觉与当前功能逻辑一致。

## 工作记录

- `PuzzleOutlineBakerEditor` 每组继续生成现有 `GroupNN.png` 连接区域，并新增：
  - `GroupNN_Level.png`：当前组所有 Piece Alpha 合并后的完整外边界。
  - `GroupNN_Stickers.png`：当前组每个 Piece Alpha 独立边界的合并图。
- 逐贴纸边界在每片蒙版生成后直接合并到组级结果，不额外长期保留每片全尺寸边界图，控制 CardBag022 等大棋盘的烘焙内存峰值。
- 逐片蒙版合并和边界提取只扫描该贴纸在棋盘上的包围盒，避免大型棋盘对每张贴纸重复遍历全部像素；输出结果不变。
- GameScene 始终创建当前阶段描边：关卡描边关闭加载 `GroupNN.png`，打开加载 `GroupNN_Level.png`；贴纸描边打开时叠加 `GroupNN_Stickers.png`。
- 新资源缺失时关卡完整外框回退到连接区域，贴纸轮廓单独跳过；缺失描边不阻断拼图创建。
- `GameSettingsData.UsableOption1` 和默认设置工厂均改为 `false`；`UsableOption2` 已为 `false`，因此两个开关新建设置时都关闭。
- 结算加成规则不变：关闭关卡描边仍加 `2%`，关闭贴纸描边仍加 `5%`。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Model/LocalDataStore.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 不用运行时 Shader 动态计算描边，继续沿用离线 PNG 烘焙和 UGUI Image，保证结果稳定并复用现有对齐规则。
- 完整关卡外框和逐贴纸轮廓分开烘焙，支持两个开关四种组合。
- 默认关卡描边关闭不是隐藏全部描边，而是保留旧 `GroupNN.png` 的连接区域提示。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- 静态检查确认两个新资源路径均位于现有 `Resources/Generated/PuzzleOutlines/CardBagNNN/`，无需修改构建同步。
- Unity 已刷新最新脚本并完成域重载，Editor 日志无 C# 编译错误。
- 已执行新版 **Bake Outline Masks**：CardBag001 到 CardBag021 共 93 个分组，基础 `GroupNN.png`、`GroupNN_Level.png`、`GroupNN_Stickers.png` 均各 93 张，数量一一对应并生成 Unity Meta。
- 抽查 CardBag001 第二组：`_Level` 只保留组级合并外边界，`_Stickers` 会额外显示组内每块贴纸之间的凹槽边界。
- CardBag022 当前 Prefab 仍使用自动生成过程的 `Piece001...` 顺序占位名，按烘焙器既有保护规则跳过并移除陈旧输出；需完成正确分组后再烘焙，不影响 CardBag001 到 CardBag021。
- 尚未在 Play Mode 实际切换四种组合，因此设置页到 GameScene 的最终画面仍待人工回归。

## 本地数据重置

- 已保存的 `GameSettings/Runtime` 会继续保留旧开关值。验证“首次初始化默认关闭”前，退出 Play Mode 后删除 `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db`。
- 未自动删除本地存档。

## 下一步

1. 在 GameScene 分别验证：全关、仅关卡描边、仅贴纸描边、两项全开。
2. 重点检查后续组与已完成区域的接触边和同组贴纸缝隙。
3. CardBag022 完成正式分组后重新执行 **Bake Outline Masks**，再检查其大棋盘耗时与内存。

## 恢复提示

继续 Puffies 描边辅助开关回归。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；CardBag001 到 CardBag021 的三类描边资源已生成，下一步在 Play Mode 验证四种组合，不要回退用户现有 CardBag、特效或场景修改。
