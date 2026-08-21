# 当前任务

- 任务：描边烘焙全局质量修复
- 状态：通用算法与全量烘焙已完成，等待 Play Mode 逐组视觉验收
- 更新时间：2026-08-21

## 用户意图

- 系统性解决烘焙描边在实际游戏中仍会出现断裂、错误线段和边界归属错误的问题。
- 修复应适用于全部现有及后续 `CardBagNNN`，不能逐卡包手工修图。
- 保持现有分阶段显示规则、关卡描边和贴纸描边开关，以及运行时加载方式不变。

## 已确认问题

1. 当前组最终外轮廓来自 `GameBoard` 挖空，已完成组接触边来自 Piece Alpha；两套蒙版栅格不完全重合时，真实交点附近可能出现几像素断口。
2. 边界归属如果只依赖最近距离，会把切线方向或边界另一侧的相邻区域误判为当前组，表现为端点延长、多余尾巴或错误描边。
3. 为修补断口而跨组件强制连线会制造长斜线或梯状短线。内部独立接触边本来可以不连接最终外轮廓，不能把所有组件都拉向外边界。
4. 默认 `GroupNN.png` 必须严格排除同组 Piece 接缝、当前组与未来组之间的边、已完成组的无关边界，以及之前阶段的整张描边；否则会出现棋盘中间不应存在的线。
5. 当前烘焙器只记录每张输出的非透明像素总数，没有检查孤立短线、异常分支、断裂组件、过长桥接或边界归属，因此“烘焙成功且没有异常”不能证明结果正确。
6. `ContactSearchRadius=6`、`FinalBoundaryAssignmentRadius=12`、法线采样、桥接长度和线宽都是固定源像素值；不同 GameBoard 分辨率下实际视觉尺度不同，需要验证固定阈值是否会造成小图误连或大图断裂，不能未经数据验证直接改为更大常量。

## 当前实现事实

- 编辑器入口为 **Puffies -> Bake CardBag Outlines**，核心代码在 `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`。
- `GroupNN.png` 使用当前组最终外边界和与低编号已完成组的接触边；`GroupNN_Level.png` 是当前组并集外边界；`GroupNN_Stickers.png` 是当前组各 Piece 边界并集。
- 现有代码已经加入局部法线方向校验：最终外轮廓要求当前组位于内侧，已完成组接触边要求当前组位于旧组边界外侧。
- 现有桥接只允许沿真实边界走廊修补最多 `4px` 的栅格断口；这属于已有保护，不能视为全卡包验证完成。
- 2026-08-20 的描边相关提交主要重新生成了输出 PNG，没有继续修改 `PuzzleOutlineBakerEditor.cs`，因此系统性风险仍需单独处理。
- 运行时只显示烘焙结果，不实时识别轮廓；项目没有运行时描边第三方插件、Renderer Feature 或运行时轮廓 Shader。

## 需求与验收标准

1. WHEN 烘焙任意正式分组卡包 THEN 系统 SHALL 为每张默认连接图输出可审计的边界组件统计，并识别孤立短线、异常分支和超过允许长度的桥接。
2. WHEN 当前组外轮廓与已完成组接触边在真实几何交点相遇 THEN 系统 SHALL 只修补栅格化造成的微小断口，最终显示连续且不得新增跨区域连线。
3. IF 两段边界属于不同独立组件且没有真实交点 THEN 系统 SHALL 保持它们独立，不得为了视觉连续而强制连接。
4. WHEN 判断最终外轮廓或接触边归属 THEN 系统 SHALL 同时校验距离、目标所在法线方向和对应组身份，切线邻近不得延长端点。
5. WHEN 生成 `GroupNN.png` THEN 系统 SHALL 只包含当前组最终外边界和与低编号已完成组的真实接触边，不得包含同组接缝、未来组边界或已完成组无关边界。
6. WHEN 对不同分辨率 GameBoard 烘焙 THEN 系统 SHALL 使用经过样本验证的尺度规则，保证判定半径和输出线宽的屏幕视觉一致性。
7. WHEN 自动质量检查失败 THEN 烘焙器 SHALL 明确报告 CardBag、Group、异常类型、像素位置或组件边界，不得只输出总像素数后视为成功。

## 建议实施顺序

- [x] 1. 为现有输出增加只读拓扑诊断：8 邻域连通组件、端点/分支点、组件间最短距离、桥接像素来源和噪声位置。
- [x] 2. 对全部 CardBag 生成诊断报告并定位孤立噪声样本。
- [x] 3. 将默认连接图拆成“当前最终外轮廓”“已完成组接触边”“桥接补点”三个可单独诊断的中间蒙版。
- [x] 4. 使用真实交点邻域和组身份约束桥接；保持最大 `4px` 和边界走廊限制，禁止跨独立组件寻路。
- [x] 5. 按 GameBoard 宽度相对 `1300px` 缩放判定参数，并限制在 `0.9~1.1`。
- [x] 6. 加入孤立噪声清理、边界归属和可定位日志；保留真实独立接触边。
- [ ] 7. 已重新烘焙全部有效 CardBag 并编译 Editor 程序集；仍需在 Play Mode 逐组核对默认、关卡和贴纸三种描边。

## 不得回归

- 不得恢复运行时实时识别轮廓。
- 不得通过增大搜索半径或全局膨胀掩盖断口。
- 不得叠加上一组整张 `GroupNN.png`。
- 不得修改 CardBag Prefab 的 Transform、Canvas 尺寸、Piece 分组或 Sprite 引用。
- 缺少描边资源时仍只警告，不得阻止 Piece 创建和正常游戏。

## 关联文件

- `Assets/Scripts/Editor/PuzzleOutlineBakerEditor.cs`
- `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN_Level.png`
- `Assets/Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN_Stickers.png`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 当前验证状态

- 最终外边界像素现在由所有组共同竞争且唯一归属；已完成区域接触边同时比较当前组和未来组，排除错误侧与切线邻近误判。
- 桥接只允许接触边端点连接当前外轮廓端点，路径最长约 `4px`，并限制在真实边界走廊内。
- 默认连接图会清理小于约 `8px` 的孤立原始边界；贴到纹理画布边缘的孤立段使用约 `12px` 阈值。关卡和贴纸独立描边不参与该清理。
- 117 张旧默认输出基线曾发现 9 组 `9~18px` 孤立块；重新烘焙后扫描 112 张现有默认输出，只剩 `CardBag020/Group02` 的 `48px` 组件，叠加核对确认它是两组贴纸真实相接边，已保留。
- 全量烘焙完成：扫描 23 个 CardBag Prefab，有效生成 108 个分组；`CardBag019` 当前 `GameBoard` 没有 Sprite，保留原有 4 组输出并记录警告。
- `CardBag003`、`CardBag006` 已按当前 Prefab 重新生成；`CardBag022` 当前只有 3 个正式分组，烘焙器已清理旧 `Group04~14` 输出。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- 尚未完成 Play Mode 下默认、关卡和贴纸三种描边的逐组视觉验收。

## 工作区提醒

- 工作区同时包含用户正在制作的 CardBag Prefab、切图 Meta 和本轮重新生成的描边资源，不得回退用户修改。
- 本轮修改未提交；用户未要求提交时不要自动提交或推送。

## 下一步

1. 在 Play Mode 重点核对 `CardBag005/Group02`、`CardBag016/Group04`、`CardBag020/Group02` 和 `CardBag022/Group01~03` 的逐组切换。
2. 分别打开默认连接描边、关卡描边和贴纸描边，确认三种模式未互相污染。
3. `CardBag019` 的 GameBoard Sprite 补齐后重新执行 **Puffies -> Bake CardBag Outlines**。

## 恢复提示

继续 Puffies 的“描边烘焙全局质量修复”。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md`、`Documents/CURRENT_TASK.md` 和 `specs/spec-driven-development.md` 中“描边烘焙全局质量修复”章节；先增加诊断并定位实际异常组，不要直接放宽搜索半径或批量重烘焙来掩盖问题。未经用户明确要求不要自动提交；用户要求提交时同时推送。
