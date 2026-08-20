# 当前任务

- 任务：Piece 左上光点与受挤压回弹动画
- 状态：代码与编译验证完成，等待 Play Mode 目视调参
- 更新时间：2026-08-20

## 用户意图

- 光点基本位于每个 Piece 的左上角，符合左上方向入射光。
- Piece 正确放下后，光点两端固定，中段被挤出、拉伸并弹回原形。
- 根据 Piece 实际尺寸选择四张光点图中的合适形状，必要时允许受控拉伸和弯曲。
- 不影响已设计完成的当前 Piece 绿色斜向滑光动画。

## 工作记录

- 光点资源不再按 Piece 编号随机选择，改为按 Piece 原生宽高计算目标尺寸和宽高比，再选择比例最接近的 `PieceLight1-4`。
- 光点位置优先使用 Sprite Physics Shape 的左上极值并向内部收回，缺少轮廓时回退固定左上位置；仅保留少量确定性位置和角度差异。
- 托盘 SpriteRenderer 与棋盘 UGUI 共用形状、非等比缩放、旋转和归一化位置，继续受 Piece Alpha/ SpriteMask 裁切。
- 新增 `PieceLightDeformEffect`，将棋盘光点横向细分为 8 段；两端不位移，中段按正弦权重弯曲和加厚。
- 落位反馈先在前 `22%` 时段推出中段，之后用衰减余弦产生小幅反向回弹，结束时强制恢复零形变和原位置。
- 保留当前块及最多六块实际相邻已拼 Piece 的筛选、错峰时序和并发版本接管；删除旧的永久平移与终点保存。
- 绿色 `PuzzlePlacementShine` Shader、材质实例、`0.52s` 时长和播放调用未修改；吸附后继续拿取下一块及切组防重逻辑保持不变。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/PROJECT_CONTEXT.md`
- `specs/spec-driven-development.md`

## 决策

- 不生成或修改光点 PNG，直接复用美术提供的四张资源。
- 形状选择由 Piece 宽高比决定，整体尺寸由 Piece 几何平均尺寸和宽高上限共同限制；偏高 Piece 选紧凑光点，偏宽 Piece 选长弧形。
- 只对棋盘 UGUI 光点执行回弹形变；托盘光点保持稳定，避免拖动时变形。
- 绿色确认滑光与暖白光点回弹保持两个独立渲染和动画路径。

## 验证

- 资源检查：`PieceLight1-4.png` 尺寸分别为 `35x31`、`47x38`、`73x40`、`49x30`，宽高比覆盖紧凑到长弧形。
- 尺寸抽样：小方块和普通方块选择 `PieceLight2`，偏宽块选择 `PieceLight4`，偏高及窄高块选择 `PieceLight1`；更宽比例会选择 `PieceLight3`。
- `dotnet build Assembly-CSharp.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：通过，`0` 警告、`0` 错误。
- 待在 Play Mode 检查实际 Alpha 轮廓内的左上位置、四种形状覆盖、推出幅度、反向回弹和连续快速落位。

## 关联待办：卡包编号迁移

- 已核对资源迁移主体：旧 007 -> 新 005、旧 005 -> 新 006、旧 006 -> 新 007、旧 010 -> 新 023；新 010 为重新制作内容。
- 当前源碎片数和 Prefab 节点数一致：005=19、006=29、007=35、010=25、023=28；Prefab 分组数分别为 4、4、5、2、5，均使用正式 `PieceGGII` 命名。
- `CardPacks.csv` 当前仍只有 22 行，需补齐 005=`1/19/0.75`、006=`2/29/0.78`、007=`3/35/1.10`、010=`2/25/0.78`，并新增 `23,23,2,28,2,0.78,,1`。
- CardBag006 描边目录比 Prefab 多一套无效 `Group05`，需要清理；其他本轮新烘焙描边保留并在 Unity 中检查。
- `CardBag005/BoardTitle.png` 与 `美术切图/游戏内包头/PackTitle005.png` 内容不同，修改前需确认采用哪一份。
- 配置修改后，测试前删除 `LocalData.db` 与 `LocalData.json`，按开发阶段规则重新初始化本地数据。

## 本机维护同步

- 当前设备已经安装每周日 `03:00` 运行的 `Puffies Project Maintenance`，脚本路径为 `E:\MyWork\UnityProjects\Puffies\ProjectMaintenance.ps1`。
- 最近审计发现 Git 松散对象达到维护阈值；检查时 Unity 正在运行，因此未执行即时清理。

## 下一步

1. 在小、宽、高三类 Piece 上目视检查光点位置、形状和回弹幅度，并确认绿色滑光正常。
2. 连续快速放置 Piece，确认回弹互不争抢且最后一块只触发一次切组或结算。
3. 完成当前视觉验证后，确认 CardBag005 包头来源并继续卡包编号迁移收尾。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；先在 Play Mode 验证 Piece 左上光点的尺寸选择、两端固定回弹，并确认绿色斜向滑光未受影响。随后处理本文件记录的卡包编号迁移待办，不要回滚现有描边资源。
