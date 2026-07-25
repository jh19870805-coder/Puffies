# 当前任务

- 任务：实现 MainScene 重玩确认弹窗
- 状态：代码已实现，等待 Unity Play Mode 验证
- 更新时间：2026-07-25

## 用户意图

- 用户已在 `MainScene` 中新增 `PanelReplay`。
- 首次游玩的卡包点击 `Play` 时继续直接进入现有开包流程。
- 已玩过的卡包按钮显示“重玩”；点击时先显示 `PanelReplay`，不直接开包。
- `BtnReplay` 确认重玩并继续原开包动画与进入 GameScene 流程。
- `BtnReturn` 和 `BtnClose` 取消确认，关闭弹窗并返回卡包选择页。

## 工作记录

- 运行时读取用户在场景中搭建的 `PanelReplay`、`BtnReplay`、`BtnReturn` 和 `BtnClose`，不改写场景布局。
- 将 `PanelReplay` 迁入现有 `PanelBagSelectCanvas` 并保持全屏布局，使其作为卡包选择页的最上层子面板。
- `OnBagSelectPlayClicked` 使用与按钮文案相同的 `CardPackDataUtility.IsPackPlayed` 判定：首次 Play 直接继续，重玩显示确认弹窗。
- 弹窗显示期间锁定 Play、Back 和 Camera，并临时隐藏选中卡包及其他列表卡包的 Renderer/尺寸图标，避免任何卡包压住弹窗。
- 确认时关闭弹窗、恢复选中卡包并启动原 `EnterCardPackOpeningStage` 协程。
- 返回或关闭时关闭弹窗、恢复选中卡包和选择页按钮，不改变卡包状态。
- 清理卡包选择状态时同步关闭弹窗并重置确认状态，避免残留输入。
- 首次视觉验证发现未选中的列表卡包 Mesh Renderer 仍会绘制在弹窗上方；已补充弹窗期间统一隐藏、取消时统一恢复的处理。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- `PanelReplay/BtnReplay` 是确认按钮；`BtnReturn` 与右上角 `BtnClose` 都是取消按钮。
- 是否需要确认严格跟随当前按钮是否为“重玩”，不另建状态字段或持久化数据。
- 确认后复用现有开包流程，不复制开包动画和场景切换逻辑。
- 本次不涉及 JSON、SQLite 或 `PlayerPrefs`，无需删除本地数据。

## 验证

- 场景序列化检查确认 `PanelReplay` 初始隐藏，三个目标按钮均存在并带有 `Button` 组件。
- `dotnet build Puffies.sln --no-restore`：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check -- Assets/Scripts/Controller/MainScene.cs`：通过，仅有既有 LF/CRLF 转换提示。
- 首次 Play Mode 验证发现列表卡包压住弹窗；对应修复已编译通过，等待复测视觉层级和按钮交互。

## 下一步

1. 选择首次未玩的 `Unlocked` 卡包，确认点击 `Play` 不显示弹窗并继续开包。
2. 选择 `InProgress` 或 `Completed` 卡包，确认点击“重玩”显示 `PanelReplay`。
3. 验证 `BtnReturn` 和 `BtnClose` 关闭弹窗并恢复卡包与选择按钮。
4. 验证 `BtnReplay` 关闭弹窗并正常进入原开包舞台和 GameScene。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；重玩确认逻辑已实现，下一步是在 Unity Play Mode 验证 `PanelReplay` 三个按钮和视觉层级。
