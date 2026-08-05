# 当前任务

- 任务：移除卡包特效并恢复静态卡包流程
- 状态：实现完成，等待 Unity Play Mode 视觉验收
- 更新时间：2026-08-05

## 用户意图

- 删除此前导入的所有卡包展示和开包特效资源。
- MainScene 桌面列表和点击放大状态都改用静态 `PackIconNNN.png`。
- 点击玩后仍进入开包舞台并等待再次点击或横划；撕包动画暂时留空，有效操作直接进入 GameScene。
- 保留卡包选择、返回、重玩确认、拍照和进入游戏等现有业务流程。

## 工作记录

- `MainScene` 不再创建、控制或渲染 3D 卡包实例；列表只显示 `PackCover`、`PackShadow` 和 `PackSize`。
- `PackSize` 保留在 `PackItem` 内按 Prefab 既有层级显示，不再迁入旧特效方案的独立相机 Canvas。
- 新增顶层静态卡包 Image，按列表实际位置和尺寸移动到屏幕中心 `600 x 680`；返回时反向回到列表位置。
- 点击玩或确认重玩后进入 `BgGame` 等待阶段；玩家再次轻点卡包或横划时不播放特效，下一帧直接进入 GameScene。
- 从 `PackItem.prefab` 删除嵌套 `CardPackEffect`。
- 删除 `Assets/Resources/Effects/`、`EffectScene001.unity` 和 `CardPackListUnlit.shader`。
- MainScene 移除制作方 Skybox、Directional Light 和 Screen Space Camera Canvas 配置，恢复普通 2D UI 场景设置。
- 保留 `CardPackRewardFlyTransition` 使用的 2D 结算飞行动画，以及 GameScene 的 `CardBagPrefabs` 拼图资源。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Editor/BuildSync.cs`
- `Assets/Scripts/Model/GameDefine.cs`
- `Assets/Prefabs/PackItem.prefab`
- `Assets/Scenes/MainScene.unity`
- `Assets/Resources/Effects/`（删除）
- `Assets/Scenes/EffectScene001.unity`（删除）
- `Assets/Resources/CardPackListUnlit.shader`（删除）
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`

## 决策

- 静态卡包封面继续使用现有 `GameDefine.FormatPackImagePath` 和 `GameCommonUtility.LoadSpriteByPath` 加载，不增加新的资源格式。
- 暂不删除 `GameAnimationUtility.cs` 整个文件，因为其中的 `CardPackRewardFlyTransition` 仍被结算流程使用；旧卡包特效方法已无运行时调用。
- 保留轻点和横划输入边界，后续只需在 `PlaySelectedPackage` 中插入正式撕包动画，再进入 GameScene。

## 验证

- `dotnet build Puffies.sln --no-restore`：成功，0 个警告、0 个错误。
- `PackIcon001.png` 至 `PackIcon022.png` 均存在。
- 已按本次删除资源的全部 `.meta` GUID 扫描当前 Unity YAML，未发现残留序列化引用。
- `MainScene.cs` 已无 `GameAnimationUtility`、3D 卡包显示、独立特效相机或 RenderTexture 调用。
- 尚未执行 Unity Play Mode 视觉与交互验收。

## 下一步

1. Unity 完成资源刷新后，从 LoadingScene 进入 MainScene 验证列表静态图、点击放大、返回、拍照和重玩确认。
2. 点击玩后分别验证轻点卡包和横划都可直接进入 GameScene。
3. 获取正式撕包动画资源与节奏后，在现有输入边界接入动画，不恢复旧特效资源。

## 恢复提示

卡包特效资源和场景引用已删除，MainScene 已改为静态列表与静态选中图。下一步在 Unity Play Mode 验证完整选择流程；正式撕包动画暂空，轻点或横划后直接进游戏。
