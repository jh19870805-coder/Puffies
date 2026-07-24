# 当前任务

- 任务：按参考视频实现游戏内碎片提示
- 状态：已实现，等待 Unity Play Mode 视觉复测
- 更新时间：2026-07-24

## 用户意图

- 参考 `微信视频2026-07-24_150146_085.mp4` 实现 `GameScene/BtnTips`。
- 点击提示后，对应的未完成碎片抖动，棋盘目标位置显示持续滚动的绿色虚线轮廓。
- 提示不依赖每个卡包额外制作蒙版，必须适用于后续大量 CardBag。

## 工作记录

- 使用 FFmpeg 按 `5fps` 和 `10fps` 抽取参考视频关键帧，确认视频约 `2.87s`。
- 视频规则为：点击右上角提示后，托盘中的目标碎片左右往复抖动；棋盘槽位显示绿色跑马灯虚线，并持续滚动。
- `BtnTips` 现在选择当前组 Piece 编号最小的未完成碎片；重复点击保持当前有效目标。
- 碎片以原旋转为基准进行 `6` 度、每秒 `4.5` 周期的左右抖动，约 `0.8s` 后停止并恢复原旋转，不修改托盘布局位置。
- 拖动目标碎片时立即恢复原旋转；成功放置、切换分组、进入结算或销毁场景时清理提示。
- 新增通用 `HintDashedOutline` UI Shader，直接从目标 Piece Sprite Alpha 计算内侧轮廓，并通过时间相位生成持续滚动的绿色虚线。
- 虚线对象运行时复制对应 `GrooveRect` 的布局并放到同级最上方，不修改 CardBag Prefab，也不需要新增或重新烘焙提示蒙版。
- 根据反馈将虚线段数量由 `20` 提高到 `40`、填充比例提高到 `0.85`，使线段更连续、间隔更短。
- 只有找到有效未完成碎片并显示提示后，才将 `_wasHintUsed` 设为 `true`，取消本局未使用提示的 5% 加成。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Resources/Effects/HintDashedOutline.shader`
- `Assets/Resources/Effects/HintDashedOutline.shader.meta`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 提示目标使用确定性的当前组最小未完成 Piece 编号，避免一次点击同时提示多个碎片。
- 提示轮廓由通用 Shader 实时读取 Sprite Alpha，不为每个卡包增加美术资源。
- 提示动画使用 `Time.unscaledTime`，不受游戏时间缩放影响。
- 当前组已有的分阶段实线描边与单 Piece 提示虚线相互独立。

## 验证

- `dotnet build Puffies.sln --no-restore`：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅有既有 LF/CRLF 转换提示。
- 静态检查确认提示在成功放置、切组、结算和场景销毁路径均会清理。
- 当前后台 Unity 进程未提供可见窗口，尚未完成 Shader 实际显示、虚线速度、线宽和抖动手感的 Play Mode 验收。
- 不涉及持久化结构变化，无需删除 `LocalData.db` 或 `LocalData.json`。

## 下一步

1. 等 Unity 完成 Shader 导入后重新进入 GameScene Play Mode。
2. 点击 `BtnTips`，确认当前组第一个未完成碎片左右抖动，棋盘对应位置显示绿色滚动虚线。
3. 拖动提示碎片并分别测试放置失败和成功，确认旋转恢复及提示清理正确。
4. 根据实际画面微调虚线颜色、线宽、间隔、滚动速度和碎片抖动幅度。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；游戏内提示已按参考视频实现通用滚动虚线和碎片抖动，下一步是在可见 Unity Play Mode 复测并调节视觉参数。
