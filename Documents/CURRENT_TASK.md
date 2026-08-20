# 当前任务

- 任务：卡包编号迁移完整性检查与收尾
- 状态：资源迁移主体已核对，待补齐配置并完成 Unity 验证
- 更新时间：2026-08-20

## 用户意图

- 将原 CardBag007 插入到 CardBag005 前，原 005、006 顺延为 006、007。
- 将原 CardBag010 迁移为 CardBag023，并重新制作新的 CardBag010。
- 确认源切图、Prefab、预览图、列表图标、描边和 `CardPacks.csv` 没有遗漏或编号错位。

## 已完成核对

- 当前分支为 `develop`，HEAD 为 `a1aed11 添加自动清理任务`，与 `origin/develop` 一致。
- 通过 Git Blob、Sprite GUID 和资源尺寸对比确认迁移主体：旧 007 -> 新 005、旧 005 -> 新 006、旧 006 -> 新 007、旧 010 -> 新 023；新 010 是重新制作的内容。
- `Assets/UI/CardBags` 源 PNG、`Previews`、`PackImages`、CardBag Prefab 的 Sprite GUID 和 Meta GUID 唯一性未发现编号遗漏。
- 当前源碎片数和 Prefab 节点数一致：005=19、006=29、007=35、010=25、023=28。
- 当前 Prefab 分组数：005=4、006=4、007=5、010=2、017=6、023=5；均使用正式 `PieceGGII` 命名。
- 当前工作区已经重新生成 005、007、010、017、023 对应组数的描边。006 目录比 Prefab 多一套 `Group05`，属于待清理的旧残留，不能作为有效第五组使用。
- CardBag017 当前实际为 `1316 x 1316`、38 片、6 组；稳定项目事实已同步修正。

## 配置待办

- `Assets/Resources/Configs/CardPacks.csv` 当前只有 22 行，最大 PackId 为 22，尚未添加 CardBag023。
- 迁移后的自动字段应更新为：005=`PackSize 1 / StickerCount 19 / BoardScale 0.75`；006=`2 / 29 / 0.78`；007=`3 / 35 / 1.10`；010=`2 / 25 / 0.78`。
- 需要先手工增加 `23,23,2,28,2,0.78,,1`，再运行 `Puffies -> Update Pack Sizes From Piece Counts`。当前工具只更新已有配置行，不会为无配置的源目录自动创建 PackId 23。
- 编号和配置调整会改变本地卡包状态映射。完成后测试前删除 `LocalData.db` 与 `LocalData.json`，按开发阶段规则重新初始化本地数据。

## 待确认

- `Assets/UI/CardBags/CardBag005/BoardTitle.png` 与 `美术切图/游戏内包头/PackTitle005.png` 尺寸同为 `1300 x 164`，但文件内容不同；006、007、010、023 的对应包头一致。修改前需要确认 CardBag005 应以哪一份为准。
- 当前大量描边 PNG 有 Unity 重新烘焙产生的修改，并新增 007/010/017/023 描边文件；这些都是现有工作区内容，不回滚、不覆盖。

## 本机维护同步

- 已运行 `ProjectMaintenance.ps1 -Audit`：`Library=4.95 GiB`、`Library/Bee=192.65 MiB`、临时目录 `10.70 MiB`、磁盘可用 `543.01 GiB`。
- Git 松散对象为 `9546 / 1023.85 MiB`，已达到维护阈值；检查时 Unity 与 `UnityCrashHandler64` 正在运行，未在本轮执行即时清理。
- 当前设备已安装计划任务 `Puffies Project Maintenance`，状态为 `Ready`，实际脚本路径为 `E:\MyWork\UnityProjects\Puffies\ProjectMaintenance.ps1`，下次运行时间为 2026-08-23 03:00。

## 下一步

1. 确认 CardBag005 包头采用工程内现有文件还是美术切图目录版本。
2. 补齐 `CardPacks.csv` 的 005/006/007/010/023，并运行尺寸更新工具复核。
3. 删除 CardBag006 无效的旧 `Group05` 描边，保留并检查其他本轮新烘焙描边。
4. 删除旧本地数据，在 Unity 中验证 005/006/007/010/023 的列表、开包、关卡加载、分组和描边。

## 恢复提示

继续 Puffies 工程。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和本文件；不要回滚当前描边修改。先确认 CardBag005 包头来源，再补齐 `CardPacks.csv` 和 CardBag006 残留描边，最后清理本地数据并执行 Unity 验证。
