# 当前任务

- 任务：恢复卡包撕开动画的横向白色光效
- 状态：已改为读取制作方蒙皮顶点边界，待 Play Mode 画面验证
- 更新时间：2026-08-02

## 用户意图

- 卡包撕开时恢复从左到右扫过的横向粗白光。
- 保留最新 EffectScene 卡包的原始材质、Animator 和粒子设置。

## 工作记录

- 确认 `PlaySelectedPackage()` 仍调用 `FX_ui_tuowei_w_001`，资源路径存在，Unity 导入日志无 Shader、材质或 Prefab 加载错误。
- 确认该 Prefab 本体主要是星点粒子，不包含可稳定覆盖 UI 开包舞台的粗横向核心光。
- 在现有顶层 `CardPackTearGuideCanvas` 创建独立横光，由白色核心、淡蓝白柔光和头部亮点组成。
- 用户画面验证确认 `Bone006` 的 Transform 原点位于卡包外侧；进一步检查确认它是动画骨架父节点，不是蒙皮撕口边界，已删除该错误定位方式。
- 直接解析制作方 `CardPackOpeningModel.FBX` 的 621 个顶点、蒙皮 Cluster 与骨骼权重：未撕开的主体由 `Dummy001` 主导，其最高受控顶点行为 FBX `Y=38.4792`；上封口骨骼从该边界向上控制网格。
- 首次运行时使用 `BakeMesh` 后又执行 Transform 转换，重复带入当前缩放，导致撕口坐标被二次放大并移动到卡包上方；用户第二次画面验证确认该方案错误，已删除 `BakeMesh` 路径。
- `GameAnimationUtility` 现在按 Unity 标准蒙皮公式 `renderer.worldToLocalMatrix * bone.localToWorldMatrix * bindpose` 计算闭合首帧顶点，只进行一次世界坐标转换；随后按每个顶点的最大蒙皮权重找出 `Dummy001` 主导主体的最高顶点行，并将该行中心缓存为卡包根节点局部撕口坐标。
- 运行时额外校验计算结果必须落在当前卡包 Renderer 的实际 Y 边界内；超出卡包立即拒绝并使用模型实测回退位置，避免指示再次出现在卡包外。
- 滑动指示、手势有效区域、UI 粗横光、原 `FX_ui_tuowei_w_001` 星点拖尾和 `fx_chai_w_001` 拆包粒子全部读取同一蒙皮边界；鼠标只负责输入，不改变撕口高度。
- 现有 23 个正式开包 Prefab 都包含 `Dummy001` 和对应 SkinnedMeshRenderer 骨骼引用。只有未来资源缺少该蒙皮边界时，才按当前模型实测位置回退并输出一次明确警告。
- 滑动指示原本使用相机空间 Canvas，虽然排序值高于卡包，但会被有厚度的 3D 卡包正面写入的深度遮挡；专用 Canvas 已改为 `ScreenSpaceOverlay`，确保指示圆和横光始终显示在卡包之上。
- 核心光高度由 `16px` 加粗到 `26px`，柔光由 `46px` 扩大到 `72px`，头部亮点由 `62px` 放大到 `82px`；柔光 Alpha 提高到 `0.78`，并缩短淡入淡出。
- 原 `FX_ui_tuowei_w_001` 继续同步播放；没有修改制作方卡包 Prefab、材质、Animator、Shader 或粒子参数。
- 保留未提交的新手引导修复：CardBag001 中途退出后按活动拼图会话恢复引导，教程完成标记只在整包结算时写入。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Assets/Scripts/Model/GameAnimationUtility.cs`
- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 粗横光属于项目侧撕包交互反馈，放在撕包 Canvas 保证层级稳定；制作方原始卡包和粒子保持不变。
- 横光和原星点拖尾并行播放，之后再启动卡包原 Animator 与拆包粒子。
- 撕口位置属于制作方蒙皮数据，以闭合首帧 `Dummy001` 主导主体区域的最高顶点边界为唯一来源，不使用骨骼 Transform 原点、手工高度比例或鼠标位置猜测。

## 验证

- `dotnet build Puffies.sln --no-restore`：通过，0 警告、0 错误。
- `git diff --check`：通过，仅有仓库既有的 LF/CRLF 转换提示。
- 扫描 `Assets/Resources/Effects/CardPack/CardBagPrefab` 下 23 个正式开包 Prefab：全部包含 `Dummy001` 和 SkinnedMeshRenderer 骨骼列表，缺失数为 0。
- 静态检查确认滑动指示、输入带、横光、星点拖尾和拆包粒子统一读取缓存的蒙皮边界世界坐标；代码中已无 `TearGuideVerticalPositionRatio`、`Bone006` 撕口定位或鼠标反写特效高度逻辑。
- 静态检查确认滑动指示 Canvas 使用 Overlay 模式，屏幕坐标转换在 Overlay 下使用空事件相机。
- 尚未在 MainScene Play Mode 检查横光实际亮度、粗细和节奏。

## 下一步

1. 在 MainScene 选择任意卡包，点击“玩”进入撕包舞台。
2. 确认滑动指示圆完整显示在卡包上层，并沿制作方模型真实撕口从左向右移动。
3. 在撕口横划，确认粗白光、星点拖尾和拆包粒子都严格沿同一位置播放，且亮度足够。
4. 检查横光结束后原卡包 Animator、拆包粒子和进入 GameScene 的流程不变。
5. 回归 CardBag001 中途退出后的新手引导恢复。

## 恢复提示

继续验证卡包撕开横光。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；粗横光已放在顶层撕包 Canvas 并与原粒子并行，下一步在 MainScene Play Mode 检查画面，不要修改 EffectScene 原始卡包和粒子参数。
