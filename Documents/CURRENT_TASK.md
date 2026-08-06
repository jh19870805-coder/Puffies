# 当前任务

- 任务：彻底清理旧卡包开包特效代码
- 状态：实现完成，等待 Unity 刷新正式项目文件
- 更新时间：2026-08-06

## 用户意图

- 当前卡包列表和选中态只使用静态图，不再播放卡包开包特效或撕包动画。
- 删除已经没有业务用途的 `CardFxRuntimeUtility` 及相关遗留实现。
- 保留玩家点击或滑动静态卡包后直接进入 GameScene 的流程。

## 工作记录

- 删除 `CardFxRuntimeUtility.cs/.meta`。
- 删除 `GameDefine` 中 PlaneGroup、3D CardPack、CardFx、拆包粒子和拖尾的全部废弃资源路径。
- 原 `GameAnimationUtility.cs` 共 2015 行，前 1650 行旧 3D 卡包模型、材质替换、Animator、拆包粒子与拖尾代码没有任何外部调用，已全部删除。
- 同文件中仍被 GameScene 和 MainScene 使用的结算奖励卡包飞行动画独立保留为 `CardPackRewardFlyTransition.cs`，实现未改变。
- 删除 MainScene 运行时创建的撕包引导 Canvas、移动圆点、横向闪光线、生成纹理及关联协程。
- MainScene 继续保留点击和横向滑动输入；有效输入不播放特效，直接执行 `PlaySelectedPackage` 进入 GameScene。
- 清理开始前工作区已有 `CardPacks.csv` 数值格式修改，本次未恢复或改写。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Model/CardPackRewardFlyTransition.cs`
- `Assets/Scripts/Model/CardPackRewardFlyTransition.cs.meta`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 删除文件

- `Assets/Scripts/Model/CardFxRuntimeUtility.cs`
- `Assets/Scripts/Model/CardFxRuntimeUtility.cs.meta`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs.meta`

## 决策

- “开包特效”与“结算奖励卡包飞回首页”是两个独立流程；只删除前者，保留后者。
- 开包舞台的静态背景、选中卡包静态图、点击/滑动输入和直接进入游戏逻辑继续保留。
- 不保留失效的 Resources 路径、兼容回退或空壳播放接口；以后重新制作开包动画时按新资源重新接入。

## 验证

- 全工程搜索不再存在 `CardFxRuntimeUtility`、CardFx、旧 3D CardPack、PlaneGroup 或拆包粒子资源路径引用。
- MainScene 不再创建 `CardPackTearGuideCanvas`、`CardPackTearGuide` 或 `CardPackTearFlash`。
- 使用 Unity 正式 `Assembly-CSharp.csproj` 的临时副本替换已删除/新增编译项后执行编译：成功，0 个警告、0 个错误；临时项目文件已删除。
- 正式 `Assembly-CSharp.csproj` 由 Unity 生成，未手工修改；当前打开的 Unity 尚未刷新它。

## 下一步

1. 切回 Unity 触发 AssetDatabase 刷新和 `.csproj` 重新生成。
2. 确认 Console 无编译错误，并测试首页静态卡包：点击玩进入开包舞台，再点击或滑动后直接进入 GameScene。
3. 完成一局触发新卡包奖励，确认 `CardPackRewardFlyTransition` 仍能把奖励卡包飞回首页列表。

## 恢复提示

旧卡包开包特效代码已彻底清理；只保留静态开包舞台输入和独立的结算奖励卡包飞行动画。下一步让 Unity 刷新项目文件后验证两个实际流程。
