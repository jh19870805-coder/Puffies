# 当前任务

- 任务：优化首次拆包进入 GameScene 的卡顿
- 状态：代码和编译验证完成，等待首次 Play Mode 实测
- 更新时间：2026-08-13

## 用户意图

- 第一次从 MainScene 播放拆包动画后进入 GameScene 时，不应在动画结束后异常停顿。
- 保留现有拆包动画、GameScene 棋盘/托盘/Piece 入场动画和玩法初始化结果。

## 工作记录

- 确认原流程在拆包动画结束后调用同步 `SceneManager.LoadScene(GameScene)`。
- 确认 GameScene 首次 `Start()` 还会同步加载当前 `CardBagNNN` Prefab、实例化完整棋盘、创建当前组 SpriteRenderer/Physics Shape Collider、加载描边和 Shader；首次资源读取未命中缓存，因此后续进入明显更快。
- 在 MainScene 完成开包舞台入场、开始等待玩家轻点或横划时，启动 `GameManager.PreloadGameScene(packId)`。
- 预加载先以低优先级异步读取当前 CardBag Prefab，再以低优先级将 GameScene 异步加载到待激活状态；`allowSceneActivation=false` 保证 MainScene 和拆包动画继续显示。
- 拆包动画结束后，`EnterGameScene` 对匹配的预加载请求只开放场景激活，不再重新同步加载场景。
- GameScene 实例化卡包时优先使用预加载 Prefab；没有预加载或预加载失败时继续沿用 `Resources.Load` 回退。
- 处理玩家快速划开的竞态：即使动画结束时 Prefab 尚未加载完成，也会记录激活请求，资源完成后立即继续预加载并激活 GameScene。
- 增加预加载完成、场景到达激活点、Prefab 来源和 GameScene 初始化总毫秒数日志，供首次实测定位剩余主线程成本。

## 修改文件

- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- 利用玩家等待划开和约 `1.833s` 拆包动画作为加载窗口，不添加纯等待页或延长动画。
- 只前移资源读取和场景反序列化；GameScene 对象实例化、Collider 创建、恢复进度及入场动画继续由 GameScene 按原顺序执行。
- 预加载使用当前选择的 PackId 隔离，只有 PackId 匹配时才复用；失败时回退现有同步路径，不能卡死在 MainScene。
- 不修改 CardBag Prefab、Sprite 导入、Collider 精度、描边资源或玩法表现来换取性能。

## 验证

- `Assembly-CSharp.csproj` 顺序编译通过，`0` 警告、`0` 错误。
- `Assembly-CSharp-Editor.csproj` 顺序编译通过，`0` 警告、`0` 错误。
- `git diff --check` 通过。
- 初次并行运行两个 dotnet build 时发生共享 `obj/Debug/Assembly-CSharp.dll` 文件锁；改为顺序编译后全部通过，该文件锁不是代码错误。
- 待从冷启动首次进入 MainScene 后实际拆包一次，确认卡顿改善并读取新增耗时日志。

## 下一步

1. 关闭并重新进入 Play Mode，选择一个未在本次运行加载过的卡包，完成拆包进入 GameScene。
2. 确认拆包动画结束后立即切入 GameScene，棋盘、托盘和 Piece 入场过程没有被吞帧。
3. 检查日志应包含 `card bag preload completed`、`GameScene preload reached activation point`、`source=preloaded` 和 `GameScene bootstrap completed in Nms`。
4. 若仍有明显卡顿，根据 `Nms` 判断是否需要将 GameScene 的 Collider/拖拽 Piece 创建进一步分帧。

## 恢复提示

当前已将 CardBag Prefab 和 GameScene 的首次读取前移到等待划开及拆包动画期间。下一步用冷 Play Mode 首次拆包验证，并根据新增毫秒日志判断剩余激活成本。
