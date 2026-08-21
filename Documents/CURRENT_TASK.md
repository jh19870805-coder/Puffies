# 当前任务

- 任务：拼图片常驻光点与吸附回弹样式调整
- 状态：代码已完成并通过编译，等待 GameScene 目视验证
- 更新时间：2026-08-21

## 用户意图

- 托盘 Piece 左右可见间距必须真正按 `40` 设计像素显示；不能只修改常量后因 GameScene 相机自适配而在屏幕上被缩小，首次布局、拿起补位和回收重排必须一致。
- CardBag010 的正确吸附 Piece 也必须显示与其他卡包一致的 `IngameCoverShadow03` 投影；修复不能改变 Piece 的棋盘尺寸、位置、缩放或碰撞。
- Piece 完整处于棋盘灰色拼图区内部时允许自由放置；只有同时压到灰色区域与 GameBoard 非灰色区域、即横跨灰色边缘时才判定放错并回归托盘。与自己的凹槽相交但未达到吸附标准仍判定放错；正确吸附、托盘优先、已拼块和外部 Piece 防重叠、其他棋盘空位与桌面自由放置规则保持不变。
- 正确吸附 Piece 后，当前块及相邻已拼块上的常驻光点移动距离调整为原来的 3 倍。
- 光点移动时间调整为原来的 2 倍。
- 每块拼图片完成落入托盘后才创建常驻光点；入场聚集和飞行阶段不显示。历史已拼 Piece 初始化时同样存在。
- 光点样式需要有稳定的多变性，并根据不同 Piece 的宽高适当拉长，优先体现现有光点资源中的弧形轮廓。
- 不改变绿色斜向 ADD 滑光和其他拼图反馈。

## 工作记录

- 已定位托盘间距虽然是 `DraggableHorizontalSpacingPixels=40`，但此前始终按 `40 / PPU = 0.4` 固定世界单位使用；`FitCameraToActiveGroup` 改变正交相机尺寸时，Piece 会根据屏幕凹槽重新计算世界缩放，固定世界间距却不会同步变化，导致实际屏幕间距明显小于 40 设计像素。现改为使用 `PieceBoard` 在根 Canvas 中的设计宽度与其当前世界宽度计算换算比例；首次排列和拿起后的后序补位共用同一世界间距，回收重排继续走首次排列路径。
- 已确认 CardBag010 的 25 个正式 Piece 均绑定 `IngameCoverShadow03` 并挂有 `PackCoverShadowEffect`，运行时正确吸附后也会重新应用 03；缺影不是 Prefab 漏绑或吸附流程覆盖。CardBag010 的 25 张 `249 x 249` Piece PNG 均为全不透明矩形，而 CardBag009/011 的 Piece 平均约有 `25.7%/28.6%` 透明区域；03 原先 `PaddingX/Y=0`，因此全不透明 Piece 的 2px Alpha 投影全部落在 UGUI 网格外并被裁掉。现将 03 的 X/Y 留白改为与现有 `BlurX/Y=2` 一致的 `2px`，不改变颜色、透明度、模糊、偏移、Piece RectTransform 或碰撞。
- 已撤销把整片灰色未完成区域设为禁放区的错误实现。棋盘灰色边缘改为双侧判定：Piece 必须同时与未完成 Groove Physics Shape 和 GameBoard 不透明 Physics Shape 相交才视为横跨边缘；只命中任一侧继续允许。正确吸附仍在自由放置之前处理，并显式恢复自身凹槽相交但未吸附时的错误回弹。
- 附带统一 Unity 编辑器菜单显示名称：Prefab 生成入口为 `Generate CardBag Prefabs`，描边入口为 `Bake CardBag Outlines`，配置更新入口为 `Update CardBag Configs`；同步修改代码提示和相关文档，不改变工具执行逻辑与排列优先级。
- 已从 `Puffies` 菜单移除不再需要的 `Apply CardBag Hierarchy` 手工入口；保留新卡包生成、脚本重载处理和布局更新校验仍依赖的内部层级规范化代码。布局更新遇到非标准结构时，提示改为通过 `Generate CardBag Prefabs` 重新生成对应 Prefab。
- `Apply CardBag Shadow Materials` 菜单已改名为 `Apply CardBag Shadows`，优先级设为 `23`，固定显示在优先级 `22` 的 `Update CardBag Configs` 下方。
- `Update CardBag Configs` 的 `AutoUpdate` 语义已收窄为只控制 `BoardScale`：所有匹配源资源的配置行仍更新 `StickerCount` 和 `PackSize`，`AutoUpdate=0` 仅保留手工棋盘缩放。结果窗口不再显示“整行跳过”，改为列出保留 `BoardScale` 的 PackId。
- `PieceLight1-4` 光点形变推出距离在原 `6~14px` 结果上乘 3，实际范围为 `18~42px`。
- 当前吸附块的光点运动时长从 `0.48s` 改为 `0.96s`；相邻已拼块从 `0.42s` 改为 `0.84s`。
- 相邻光点原有 `0.07~0.23s` 错峰延迟保持不变。
- 光点传播协程不再使用固定 `0.72s` 收尾时间，改为按所有光点中最大的 `Delay + Lifetime` 等待，避免运动结束后空等或提前切组。
- `CreateDraggableGroup` 不再提前创建托盘光点；首次入场和切组入场均在每块 Piece 的落地进度达到 `1` 时单独创建，避免多个 Piece 聚在起点时光点重叠。
- 入场和切组动画期间暂停通用补建；动画结束后，`Update` 仍只对“活动、未放置且确实缺少光点”的 Piece 幂等补建，直接启动 GameScene 和异常漏失场景仍可恢复。
- 托盘 `SpriteMask` 的自定义排序范围明确设置为 Piece 本体 `sortingOrder` 到 `sortingOrder + 2`，光点固定处于中间的 `sortingOrder + 1`，避免光点落在遮罩范围边界上被全部裁掉。
- 样式继续复用 `PieceLight1-4.png`：按 Piece 宽高计算目标比例，并在误差允许时以稳定的 `42%` 概率选择次匹配样式，避免相近尺寸总是使用同一张图。
- 宽 Piece 的光点目标长度额外增加，最大可占 Piece 宽度 `70%`，目标宽高比上限从 `1.85` 放宽到 `2.45`；旋转范围从 `-14~-4` 调整为 `-18~4`，使 `PieceLight2/3` 的弧形更容易显现。
- 所有变化使用 Piece 正式编号作为随机种子；同一 Piece 在托盘、棋盘和重新进入关卡时保持相同 Sprite、长度、旋转和位置。
- 绿色 ADD 滑光的 `0.52s` 时长、Shader、颜色、宽度和扫光范围均未修改。

## 修改文件

- `Assets/Resources/IngameCoverShadow03.mat`
- `Assets/Scripts/Controller/GameScene.cs`
- `Assets/Scripts/Editor/CardBagPrefabGeneratorEditor.cs`
- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`
- `specs/puzzle-outline.md`

## 决策

- 灰色拼图区内部本身不是禁放区。灰色边缘定义为未完成 Groove 区域与 GameBoard 不透明区域的交界；只有 Piece 实际轮廓同时覆盖两侧才拒绝。GameBoard 缺少可用 Physics Shape 时该新增检查安全放行，不改变原有自由放置规则。
- “移动距离”只作用于 `PieceLightDeformEffect` 的 `BendDistance`，不移动 Piece、光点根节点或常驻初始位置。
- “移动时间”只放大单个光点的推出和回弹生命周期，不放大相邻块的错峰启动延迟。
- 常驻光点的多变性只调整现有四张 Sprite 的稳定选择、非等比缩放和旋转，不创建新纹理、不改变 Piece 图片本身。
- 切组和结算继续等待全部光点反馈完成，但等待时长以真实动画结束时间为准。

## 验证

- 静态检查确认托盘间距不再固定使用 `DraggableHorizontalSpacingPixels / PixelsPerUnit`；首次布局通过当前 `PieceBoard` 设计宽度与世界宽度换算 40 设计像素，拿起补位复用同一方法，回收重排仍调用统一 `LayoutTrayPieces`。尚需在不同卡包及发生相机自适配的组中目视确认间距。
- 静态核对 CardBag009/010/011 的 Prefab：正式 Piece 数量分别为 `36/25/33`，03 材质绑定数量分别为 `36/25/33`，不存在 CardBag010 漏绑；投影组件数量均覆盖全部 Piece。材质留白仅扩展渲染网格，不修改 RectTransform、Sprite、吸附坐标或 Physics Shape。仍需在 GameScene 目视确认 CardBag010 正确吸附后的 2px 投影清晰度。
- 静态检查确认松手处理顺序仍为托盘回收、正确吸附、外部 Piece 重叠/棋盘合法性；新增灰色边缘双侧检查仅位于棋盘内自由放置分支，只有同时命中灰区和非灰区才复用 `ReturnPieceAfterInvalidDrop`，完整位于任一侧均放行。
- 本轮 `dotnet build Assembly-CSharp.csproj --no-restore` 与 `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，均为 `0` 警告、`0` 错误。
- 静态检查确认三个新菜单名称各只有一个 `MenuItem` 入口，旧菜单名称已从代码提示与相关文档移除；工具方法和菜单优先级未修改。
- 静态检查确认 `Apply CardBag Hierarchy` 不再注册 Unity 菜单，内部 `ApplyAll`、`ApplyToHierarchy` 与 `ValidateHierarchy` 调用链继续保留。
- 静态检查确认 `Apply CardBag Shadows` 使用菜单优先级 `23`，与 `Update CardBag Configs` 的优先级 `22` 连续排列。
- 静态检查确认 `AutoUpdate=0` 不再提前跳过配置行；`PackSize` 和 `StickerCount` 在两种取值下共用同一更新路径，只有 `BoardScale` 写入受该字段控制。
- 当前 `AutoUpdate=0` 的 CardBag001（8 片、XS、手工 `BoardScale=1.3`）和 CardBag018（196 片、XXXL、手工 `BoardScale=0.7`）已核对：新逻辑会继续同步片数和尺寸，同时保留这两个手工缩放值。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- 临时运行时诊断日志和临时 Shader 增亮已删除，`PuzzlePieceLightAdditive.shader` 保持原有美术参数。
- 代码路径确认：首次入场使用 `pieceT >= 1` 创建对应 Piece 光点；切组入场使用 `progress >= 1` 创建；两个动画标记有效期间通用补建直接返回。
- 代码差异确认只涉及光点常驻样式、距离、时长和传播协程收尾计算。
- 当前 Unity Editor 已刷新，Editor 日志未发现本轮 C# 编译错误。
- 尚未在 GameScene 目视验证 3 倍位移是否超出小型 Piece 的可见裁切范围。

## 关联待办

- CardBag 扁平层级与棋盘背景平铺已完成结构校验，仍需在普通/高对比模式下目视检查背景接缝。
- Piece 拿起尺寸与 01/02/03/04 投影状态仍需完整 Play Mode 回归。

## 下一步

1. 进入 Piece 数量较多且触发相机自适配的组，确认托盘相邻 Piece 的屏幕间距明显为 40 设计像素；拿起非队尾 Piece 后，后序 Piece 保持同样间距完成补位。
2. 在 CardBag010 正确吸附几块方形 Piece，确认其四周能看到 03 的轻投影，且 Piece 尺寸、位置和拼接缝没有变化。
3. 进入 GameScene 后确认 Piece 聚集和飞向托盘期间没有光点；每块 Piece 落稳后才出现一个常驻光点，且不同 Piece 的样式、长度和角度存在变化。
4. 连续拼入当前块和邻接块，确认光点推出距离明显增大、回弹完整且不会永久偏移；检查窄小 Piece 是否被 Alpha Mask 裁掉过多。
5. 完成一组最后一块，确认系统等待光点结束后正常切组且没有额外静止停顿。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；在 GameScene 验证托盘 Piece 落稳后才创建常驻光点、稳定多样的拉长弧形样式，以及正确吸附后的 3 倍移动距离、2 倍运动时长和切组等待。不要自动提交，用户明确要求提交时再提交并推送。
