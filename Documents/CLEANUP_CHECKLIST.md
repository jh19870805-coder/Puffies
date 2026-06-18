# 清理检查清单

## 已完成

- [x] `Models/`、`Materials/`、`Prefabs/CardPack|PlaneGroup` 合并入 `Effects/`
- [x] 保留空 `Prefabs/` 供后期自定义预制体
- [x] `ArtRes/` 全部迁出并删除
- [x] `Configs/` → `Resources/Config/`
- [x] Scripts 按 MVC 扁平分类：Model / View / Controller / Editor（仅一层）
- [x] GameDefine 合并数据类型，不再拆 Core/Data
- [x] `GameDefine` 路径常量更新（`UiRoot`、`ConfigRoot`）
- [x] `BuildSync` 重写：UI/Config → StreamingAssets，Prefabs → Resources
- [x] `ToDiskPath` 兼容 UI/Config 与旧 ArtRes/Configs 路径

## 待验证

- [ ] Unity 打开工程后执行 **Puffies → Sync Build Resources**
- [ ] MainScene 卡包点击开包动画
- [ ] GameScene 拼图加载
- [ ] effect 场景 PlaneGroup 预览
- [ ] Build 后验证 StreamingAssets/UI 与 StreamingAssets/Config

## 待办

- [ ] 添加 `Resources/Config/Package002.json`（场景已有 Package002）
- [ ] 补充 CardPackAni_002+ 动画 FBX（当前仅 001，其余 fallback）
