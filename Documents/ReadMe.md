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
- 针对“启动后看不到背景图”做兼容修复：
  - 放宽 `IsMainScene` 判定（场景名忽略大小写，路径先做斜杠标准化）；
  - 增加场景命中失败日志，便于定位是否误判导致初始化被跳过。
- 针对“仍然看不到背景图”继续做可见性修复：
  - 移除 `Start()` 的场景销毁分支，避免因场景命名偏差导致初始化提前退出；
  - 背景图 `z` 坐标改为 `camera.z + 10`，确保位于相机前方可见范围；
  - 增加背景创建成功日志（含贴图尺寸与 z 值），便于运行时验证是否创建成功。
- 问题定位后进行代码收敛，移除排查期冗余逻辑：
  - 恢复 `Start()` 的场景二次校验与非目标场景自销毁；
  - 移除相机兜底查找与调试日志；
  - 背景对象位置恢复为 `Vector3.zero`，保留基础相机适配。
- 在 `GameScene` 中接入标准资源准备链路：
  - 通过 `GameManager.CreateInstance()` 获取实例；
  - 调用 `GetBagFolderPath()` 获取当前卡包目录；
  - 在 `GameManager` 新增 `GetBagPackagePath()`，按 `bagId` 动态返回 `Textures/PackImages/Package{bagId:D3}.png`；
  - 调用 `GetGameBoard()` 获取底图绝对路径；
  - 调用 `LoadBagPieces(bagFolderPath)` 获取碎片分组列表；
  - 新增 `CountPieces(List<List<string>>)` 统计碎片总数并输出启动日志。
- 补充资源有效性检查：
  - 碎片总数为 0 时告警；
  - 底图文件不存在时告警。
- 在 `Assets/Models` 新增 `GameDefine` 基础定义文件：
  - 新建 `GameDefine` 静态类，集中声明常用常量（场景名、资源目录、文件名、图片后缀、默认 `BagId`）；
  - 按当前阶段需求精简为“仅保留宏定义常量”，移除结构体与额外数据声明，降低前置耦合。

### 3) 变更文件
- `Assets/Scripts/MainScene.cs`
- `Assets/Scripts/GameScene.cs`
- `Assets/Models/GameManager.cs`
- `Assets/Models/GameDefine.cs`
- `Documents/ReadMe.md`

### 4) 自检记录
- 对 `MainScene.cs`、`GameScene.cs` 与 `GameManager.cs` 执行诊断检查：未发现新增 linter 报错。
- 对 `GameDefine.cs` 执行诊断检查：未发现新增 linter 报错。
- 逻辑核对：
  - `MainScene` 已可创建并居中显示 `MainBackground`；
  - `MainScene` 仅在目标场景执行并保持初始化逻辑简洁；
  - `GameScene` 已按 `GetBagFolderPath -> LoadBagPieces + GetGameBoard` 完成标准资源准备链路；
  - `GameDefine` 当前仅提供可复用的宏定义入口，结构体与额外数据定义已移除。

### 5) 当前状态与下一步
- 当前状态：主场景具备“初始化 + 背景图显示”，游戏场景具备“初始化 + 卡包资源准备”，模型层新增统一定义入口 `GameDefine`。
- 下一步建议：在 `GameScene` 资源准备层之上追加拼图对象生成与交互模块，避免资源加载与玩法逻辑耦合。

