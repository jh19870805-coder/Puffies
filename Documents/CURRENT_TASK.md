# 当前任务

- 任务：按参考视频实现首页卡包到游戏场景的连续流程
- 状态：代码已完成，等待可见 Unity Play Mode 视觉与手感验收
- 更新时间：2026-07-24

## 用户意图

- 参考 `微信视频2026-07-24_123407_055.mp4`，复刻从 MainScene 卡包列表、选择、手势拆包到 GameScene 拼图入场的状态顺序和节奏。
- 保持 Puffies 现有 `2560 x 1440` 横屏布局，不照搬参考视频的竖屏坐标。
- 玩家确认 Play/重玩后必须沿卡包顶部封口向右横划，不能自动开包。

## 工作记录

- 点击列表卡包后先截取首页，使用低分辨率双线性重采样形成柔化背景；卡包约 `0.3s` 居中放大，继续显示返回和 Play/重玩。
- Play/重玩后首页内容和选择操作退场，显示与 GameScene 同源的 `BgGame` 开包舞台，卡包轻微缩放定场。
- 运行时生成圆形手势提示 Sprite，不依赖 Unity 已移除的内置 `UI/Skin/Knob.psd`；提示沿顶部封口从左向右循环移动。
- 手势从左侧区域开始、向右移动超过卡包宽度 `50%` 且垂直偏移不超过高度 `20%` 时才生效；无效手势松开后恢复提示。
- 有效横划同步播放六层 `CardPackOpening` 和 `CardPackDismantle_001`；粒子短暂跨场景保留并自动释放动态材质。
- 正常拆包进入 GameScene 时播放一次入场：CardBag/棋盘从上方进入，PieceBoard 从下方进入，当前组 Piece 从棋盘附近错峰落入托盘，返回和提示按钮淡入；动画结束前屏蔽拖拽。
- 直接在编辑器启动 GameScene 或其他入口不播放入场，保持原有制作和调试方式。
- 修正 Piece 入场起点重复叠加 `WorldGameplayDepth` 的问题，入场全过程沿用 Piece 原目标 Z。
- 用于自动化排查的 `TemporaryCardPackFlowValidation` 已删除，不保留临时编辑器代码。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Model/CardFxRuntimeUtility.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/card-pack-effects.md`

## 决策

- 保持工程横屏设计分辨率，只复刻参考视频的交互状态、相对运动和节奏。
- 横划区域根据当前六层卡包 Renderer 的实际 Bounds 动态计算，不使用固定屏幕坐标。
- 选择页柔化背景只在打开时截取一次，离开选择或进入开包舞台后立即释放。
- 开包舞台和 GameScene 复用同一张 `BgGame`，拆包粒子跨场景延续，降低场景切换的视觉断点。
- 本次不修改场景和 Prefab 序列化文件，不改变 Canvas 设计尺寸。

## 验证

- 参考视频已按关键帧确认流程：卡包居中、背景退场、顶部横划提示、向右撕包、内容物爆出、棋盘和托盘依次入场。
- `dotnet build Puffies.sln --no-restore`：Assembly-CSharp-firstpass、Assembly-CSharp 和 Assembly-CSharp-Editor 均成功，`0` 警告、`0` 错误。
- `git diff --check`：通过，仅有工作区既有 LF/CRLF 转换提示。
- 自动验证器确认 MainScene 能进入选择流程；Unity Editor `-batchmode` 不产生 `WaitForEndOfFrame`，无法验证截屏柔化。普通 Editor 在当前隐藏会话未完成启动，因此未生成可靠的新流程截图。
- 尚未完成可见 Unity Play Mode 下的视觉、粒子层级、手势手感和 GameScene 入场位置验收。
- 不涉及持久化结构变化，无需删除 `LocalData.db` 或 `LocalData.json`。

## 下一步

1. 在 Unity 打开 MainScene 并进入 Play Mode，点击任一卡包，确认居中放大和柔化背景没有位置或尺寸跳变。
2. 点击 Play/重玩，确认首页平滑退场、`BgGame` 无闪屏、圆形提示位于卡包顶部封口。
3. 分别测试短划、明显斜划和有效向右横划，确认只有有效手势触发一次拆包动画和粒子。
4. 观察 GameScene 棋盘、托盘、Piece 和按钮入场，确认层级、位置和节奏后再按实际画面微调参数。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；代码已实现参考视频的首页选择、顶部横划拆包和 GameScene 入场，下一步是在可见 Unity Play Mode 完成视觉与手感验收并按结果微调。
