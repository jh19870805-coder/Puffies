# 清理检查清单

## 已完成

- [x] 资源：`UI/`、`Resources/Effects/{CardPack,PlaneGroup,CardFx}`
- [x] Scripts MVC（Model / View / Controller / Editor）
- [x] `BuildSync`：UI → StreamingAssets
- [x] 场景跳转：Loading / Main / Game / Rank / Achieve
- [x] 本地存储骨架：`JsonLocalStore` + `SqliteLocalStore`
- [x] 设计分辨率 2560×1440 + Canvas 工具
- [x] CardFx 预览（effect 场景）
- [x] 删除一次性迁移脚本 `Tools/*.ps1`

## 待验证 / 待做

- [ ] Play 全场景跳转回归
- [ ] Build 后 `StreamingAssets/UI` 回归
- [ ] 业务数据写入本地存储
- [ ] Rank / Achieve 页面功能填充
- [ ] `CardPackAni_002+` 动画资源（可选）

## 已废弃（勿再创建）

- `Assets/ArtRes/`、`Assets/Configs/`（Package JSON 配置流）
- `Resources/Config/Package001.json` 拼图配置方案（改为 GameScene 编辑器摆位）
