# 当前任务

- 任务：完善卡包选择确认页面的呼吸与渲染层级
- 状态：已完成，待 Unity Play Mode 视觉复测
- 更新时间：2026-07-23

## 用户意图

- 点击 MainScene 卡包后，卡包移动到屏幕中心并同步放大到 `600 x 680`，同时显示 `PanelBagSelect`。
- 点击 Play 后才播放开包动画，动画结束进入 GameScene。
- 点击返回后，卡包回到原列表位置并恢复原尺寸，`PanelBagSelect` 隐藏。

## 已完成

- 将原本点击后立即开包的流程拆分为选择预览、确认开包和取消返回三个状态。
- 选择预览复用现有六层 3D 卡包的闭合第一帧，在 `0.3s` 内同步移动到屏幕中心并等比放大到 `600 x 680`。
- 绑定 `PanelBagSelect/BtnPlay`，仅点击后启动开包 Animator，等待动画结束后进入 GameScene。
- 绑定 `PanelBagSelect/BtnBack`；该场景节点没有 Button 组件，运行时会补齐 Button 并使用现有 Image 作为点击目标。
- 返回时先关闭面板，再将卡包反向移动、缩放回原列表锚点，隐藏开包器并恢复列表呼吸特效。
- 选择面板显示期间，其他列表卡包继续保持原位置和呼吸动效；只有被选中的卡包由列表实例切换为居中开包器。
- 通用 3D 开包资源不可用时保留原有 2D 点击回退，并直接进入 GameScene，避免卡死在选择状态。
- 卡包尺寸图标改为围绕所属卡包中心应用相同呼吸倍率，位置和尺寸随卡包一起缩放。
- 3D 空闲卡包创建成功后隐藏原 UI 尺寸图标；只有 3D 显示不可用时才显示 UI 图标作为回退，避免静态图标跨卡包压层。
- 每个列表卡包使用相邻的本体/尺寸图标排序值，尺寸图标仅高于本卡包本体；选择面板继续使用场景原有主 Canvas 和末尾兄弟节点顺序。
- 运行时层级固定为选中卡包高于 `PanelBagSelect`，`PanelBagSelect` 高于其他列表卡包；选择与返回过程中保持该层级。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 不修改用户新搭建的 `PanelBagSelect` 布局和资源引用。
- 选择预览和正式开包复用同一个已准备的六层 3D 卡包实例，Play 前 Animator 保持暂停。
- 位移、缩放和等待动画均使用不受 `Time.timeScale` 影响的时间。
- 列表卡包和选中卡包使用独立排序值；`PanelBagSelect` 运行时迁移到独立的根级 Screen Space Camera Canvas，该 Canvas 位于普通列表卡包与选中卡包之间。

## 验证

- `dotnet build Puffies.sln --no-restore`：0 警告、0 错误。
- `git diff --check -- Assets/Scripts/Controller/MainScene.cs Assets/Scripts/Model/GameAnimationUtility.cs`：通过，仅有 LF/CRLF 转换提示。
- 已确认场景存在未激活的 `PanelBagSelect`、带 Button 的 `BtnPlay`，以及带 Image 但没有 Button 的 `BtnBack`；代码覆盖这两种情况。
- 已静态确认卡包尺寸图标使用与本体相同的呼吸倍率，并按列表项分配相邻排序值。
- 已移除会导致 `PanelBagSelect` 不显示的运行时嵌套 Canvas 和世界 Z 坐标改写。
- 根据 Play Mode 截图修正层级：普通列表卡包 `z=0`、`PanelBagSelect z=-0.1`、选中卡包 `z=-0.2`。
- 已确认普通和选中卡包由场景根节点下的世界空间 Renderer 创建，原主 Canvas 的兄弟节点顺序无法控制它们；因此不把卡包挂入 UI，而是为完整选择面板建立独立相机空间 Canvas 父节点。
- `PanelBagSelect` 根背景遮罩 Alpha 运行时设为 `0.92`，普通卡包继续可见但明显压暗，选中卡包保持最高层级和原亮度。
- 完整 `git diff --check` 仍会报告用户新增 `MainScene.unity` 中 Unity 自动序列化空字段的尾随空格，本次未重写场景文件。
- 不需要重置 JSON 或 SQLite 本地数据。

## 下一步

1. 在 Unity Play Mode 确认每个尺寸图标与所属卡包同步呼吸，且不会压在相邻卡包上。
2. 点击不同列表位置的卡包，确认视觉层级依次为选中卡包、`PanelBagSelect`、其他列表卡包。
3. 分别验证 BtnPlay 完整开包后进入 GameScene，以及 BtnBack 返回准确原位并恢复呼吸动效。

## 恢复提示

继续 Puffies 开发。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；MainScene 卡包点击已拆为居中选择、Play 开包和 Back 复原三段流程，尺寸图标已同步呼吸并建立三级渲染层级，下一步是在 Unity Play Mode 复测呼吸、遮挡、位置和按钮交互。
