# 当前任务

- 任务：清理无引用代码并合并重复编辑器逻辑
- 状态：已完成
- 更新时间：2026-08-07

## 用户意图

- 检查当前工程中可以合并或删除的冗余代码，并执行安全清理。
- 保持现有游戏流程、资源结构和持久化数据行为不变。

## 工作记录

- 删除 `GameCommonUtility` 中旧 3D 卡包流程遗留的相机适配、世界/UI 坐标换算、图片扩展名判断、Renderer 透明度和旧托盘缩放接口。
- 删除 `GameScene` 中两个无调用私有方法，以及 `MainScene` 中两个未使用阴影偏移常量。
- 删除卡包生成器早期仅支持 CardBag017 的命令行入口。
- 删除卡包生命周期、任务配置、任务实例和本地存储中没有调用方的旧公开接口及其专用数据行类型。
- 卡包生成器不再维护重复的描边目录删除实现，改为复用 `PuzzleOutlineBakerEditor.DeleteStaleOutlines`。
- 保留配置文本源注入、排行榜与成就 Demo、任务进度 UI、运行时动态挂载组件、构建同步和所有当前业务入口。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Scripts/Model/CardPackDataUtility.cs`
- `Assets/Scripts/Model/GameCommonUtility.cs`
- `Assets/Scripts/Model/GameConfigRepository.cs`
- `Assets/Scripts/Model/GameTaskUtility.cs`
- `Assets/Scripts/Model/LocalDataStore.cs`
- `Documents/CURRENT_TASK.md`

## 决策

- 只删除全工程无源码引用、无 Unity 序列化引用且没有当前工具入口的代码。
- 不把 `MainScene`、`GameScene` 或多个 MonoBehaviour 合并成更大的文件。
- 暂不抽取场景中少量重复的 `FindChild` 和返回按钮绑定逻辑，避免扩大公共万能工具类。
- 本次不修改数据库结构、JSON 格式或任何持久化数据，不需要清除本地数据。

## 验证

- 全工程复查已删除符号，无残留调用或场景/Prefab/Asset 序列化引用。
- 新一轮私有方法频次扫描只剩 Unity 生命周期函数与 Editor 特性回调，没有新增死方法。
- `git diff --check` 通过。
- 使用 Unity `2022.3.62f2c1` 无界面模式完成 AssetDatabase 刷新与正式脚本编译；日志无 C# 错误或警告，并以返回码 `0` 正常退出。
- 本次净删除 578 行代码；Unity 未修改场景、Prefab、资源或配置文件。

## 下一步

1. 在 Unity 中按正常流程测试 MainScene 卡包选择、进入 GameScene、完成结算和返回首页。
2. 使用卡包生成器覆盖生成一个测试卡包，确认未分组卡包仍会清理旧描边目录，正式分组卡包仍可正常烘焙描边。

## 恢复提示

无引用代码清理已完成并通过 Unity 编译。下一步仅需执行 MainScene、GameScene 和卡包生成器的 Play Mode/Editor 冒烟测试。
