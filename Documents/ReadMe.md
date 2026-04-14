# 功能说明（功能策划案 / 系统拆分）

> **目标**：把游戏按“系统”拆分，写清楚每个系统：做什么、不做什么、数据口径、对外接口、配置方式、与当前实现状态的差距。  
> **使用方式**：实现任何需求前，先定位该需求属于哪个系统；实现完成且你验收通过后，回写本文件对应章节。

---

## 当前开发总结（2026-04-14）

### 1) 目标确认
- 在保持场景脚本“初始化优先”的前提下，恢复 `MainScene` 背景图能力。
- 按统一资源入口规范，打通 `GameScene` 对 `GameManager` 的卡包资源读取链路。
- 当前阶段不引入拼图玩法交互，仅完成资源准备与基础校验。

### 2) 实施步骤
- 读取并确认 `Assets/Scripts/MainScene.cs` 与 `Assets/Scripts/GameScene.cs` 的初始化结构。
- 在 `MainScene` 中恢复背景图流程：
  - 增加 `System.IO` 引用以读取本地图片；
  - 在 `Start()` 中于相机初始化后调用 `CreateCenteredBackground()`；
  - 从 `Application.dataPath/Textures/MainBg.png` 读取资源并创建 `MainBackground`；
  - 使用 `FitSpriteToCamera()` 按相机可视区域等比适配并保持居中显示。
- 在 `GameScene` 中接入标准资源准备链路：
  - 通过 `GameManager.CreateInstance()` 获取实例；
  - 调用 `GetBagFolderPath()` 获取当前卡包目录；
  - 调用 `GetGameBoard()` 获取底图绝对路径；
  - 调用 `LoadBagPieces(bagFolderPath)` 获取碎片分组列表；
  - 新增 `CountPieces(List<List<string>>)` 统计碎片总数并输出启动日志。
- 补充资源有效性检查：
  - 碎片总数为 0 时告警；
  - 底图文件不存在时告警。

### 3) 变更文件
- `Assets/Scripts/MainScene.cs`
- `Assets/Scripts/GameScene.cs`
- `Documents/ReadMe.md`

### 4) 自检记录
- 对 `MainScene.cs` 与 `GameScene.cs` 执行诊断检查：未发现新增 linter 报错。
- 逻辑核对：
  - `MainScene` 已可创建并居中显示 `MainBackground`；
  - `GameScene` 已按 `GetBagFolderPath -> LoadBagPieces + GetGameBoard` 完成标准资源准备链路。

### 5) 当前状态与下一步
- 当前状态：主场景具备“初始化 + 背景图显示”，游戏场景具备“初始化 + 卡包资源准备”。
- 下一步建议：在 `GameScene` 资源准备层之上追加拼图对象生成与交互模块，避免资源加载与玩法逻辑耦合。

