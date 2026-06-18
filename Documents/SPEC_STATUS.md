# SPEC 状态面板

- Task: Puffies 新阶段开发准备
- Status: Ready
- Updated At: 2026-05-29
- Previous Phase: 工程目录重组（已完成并验证）

## 基线快照（可在此基础上开发）

### 目录结构

```
Assets/
  Scenes/              MainScene、GameScene、effect
  UI/                  2D 贴图源（PackImages、Game001、BasicUI）
  Resources/
    Config/            PackageXXX.json
    Effects/
      CardPack/        3D 卡包
      PlaneGroup/      平面组特效
  Scripts/             MVC
    Model/             GameDefine、GameManager、工具类
    View/              PackageInteractionHandler
    Controller/        MainScene、GameScene、EffectScene
    Editor/            BuildSync
  Prefabs/             预留自定义预制体
  StreamingAssets/     UI、Config（构建同步产物）
```

### 已验证

- [x] 编辑器场景 UI 显示正常（GUID 已修复）
- [x] MainScene / GameScene Play 流程正常（用户确认）

### 加载方式

| 资源 | 编辑器 | 运行时 |
|------|--------|--------|
| 2D 贴图 | `Assets/UI` | `StreamingAssets/UI` |
| 配置 JSON | `Assets/Resources/Config` | `StreamingAssets/Config` |
| 3D 特效 | `Assets/Resources/Effects` | `Resources.Load("Effects/...")` |

### 构建前

菜单：**Puffies → Sync Build Resources**

---

## 新阶段待办（按优先级）

1. [ ] `Resources/Config/Package002.json`（场景已有 Package002 卡包）
2. [ ] CardPackAni_002+ 动画 FBX（当前仅 001，其余走 fallback）
3. [ ] 多页卡包翻页（如需要）
4. [ ] 打包构建回归测试

## Next Action

确定新阶段第一个功能目标，然后从上面待办或新需求开始实现。

## Resume Prompt

`继续 Puffies 新阶段开发，请先读取 Documents/SPEC_STATUS.md`
