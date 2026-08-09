# 当前任务

- 任务：恢复 MainScene 撕包流程并校准横向撕口光效
- 状态：代码与编译完成，等待 Unity Play Mode 目视确认
- 更新时间：2026-08-09

## 用户意图

- 卡包仍然在 MainScene 原有木桌开包页面撕开，撕包动画完成后才进入 GameScene。
- 横向划开光效必须位于卡包实际撕口，目前位置略微偏下。
- 进入 GameScene 后，当前组贴纸从上一页卡包下沿对应位置继续出现，再依次进入下方暗色托盘。
- 不修改制作方卡包模型、材质、Shader、粒子参数或动画节奏。

## 工作记录

- 已撤销上一轮把撕包动画移动到 GameScene 并让 Piece 同步入槽的错误实现。
- 恢复 MainScene 原有流程：轻点或横划后播放 `CardPackOpeningModel_001-006`、`CardPackAnimation.controller` 和 `fx_chai_w_001`，等待动画结束后再进入 GameScene。
- 保持横向光在骨骼动画启动 `0.5s` 后播放。光效启动前只渲染当前动画帧的卡包正面透明蒙版，识别面积最大的下半包区域并读取其顶部边界中位高度，再把 `fx_chai_w_001` 根节点映射到该实际撕口；删除固定 `Y=1.14` 猜测值。蒙版查询失败时回退制作方场景原始 `(0,1,-1.5)`，Prefab 内部配置不变。
- GameScene 不创建或播放卡包模型。MainScene 在切场景前从 `SelectedCardPackImage` 的实际 RectTransform 记录卡包下沿归一化屏幕坐标，GameScene 将该真实坐标作为当前组 Piece 入场起点；Piece 仍按原有错峰节奏进入现有暗色托盘目标位置。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`

## 决策

- MainScene 撕包、GameScene 拼图入场是两个连续阶段，不合并到同一场景。
- 横向光位置必须从当前动画帧的卡包正面蒙版计算，不使用固定视觉猜测值；不改制作方资源本身。
- GameScene 只承接贴纸从卡包下沿到托盘的后半段，不重播撕包特效。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo` 通过，0 警告、0 错误。
- 待在 Unity Play Mode 确认六套随机撕包模型的蒙版都能被识别，黄色光贴合实际撕口。
- 待在 Unity Play Mode 目视确认横向光是否贴合撕口。

## 下一步

1. 在 MainScene 选择卡包并进入开包舞台。
2. 轻点或横划卡包，确认撕包仍完整发生在当前页面，横向光贴着撕口播放。
3. 确认撕包完成后才进入 GameScene，当前组贴纸从上一页卡包下沿位置出现并依次进入暗色托盘。

## 数据说明

- 本次没有修改 JSON、SQLite 或业务数据结构，不需要删除本地存储。
