# 当前任务

- 任务：实现卡包选中后的拍照保存功能
- 状态：已修复预览层级并加入 BagId 文件名，等待 Unity Play Mode 复测
- 更新时间：2026-07-24

## 用户意图

- 只有进入过游戏的卡包在 `PanelBagSelect` 显示 `BtnCamera`。
- 点击拍照按钮时，全屏模拟相机闪光一次。
- 自动生成与示意图相同结构的 `1024 x 1024` PNG：木纹底图、当前卡包完整拼图和左下角游戏 Logo。
- PNG 保存到桌面，命名为 `游戏名-YYYY-MM-DD-BagId.png`。
- 保存后显示 `PanelPhoto`，并将 `Photo` 替换为刚生成的图片。
- 点击 `PanelPhoto/BtnOK` 关闭照片面板。

## 工作记录

- 复用编辑器中新建的 `PanelPhoto/PhotoFrame`、`Photo`、`GameIcon` 和 `BtnOK`，不改写当前未提交的 `MainScene.unity` 布局。
- `BtnCamera` 已绑定拍照协程，拍照期间统一锁定 Play、Back 和 Camera 输入。
- 新增独立全屏白色闪光 Canvas，使用不受 `Time.timeScale` 影响的淡入、短暂停留和淡出。
- 使用独立 Camera、RenderTexture 和 `1024 x 1024` 离屏 Canvas 合成图片，不截取首页或选择面板 UI；通过 URP 14 的 `RenderPipeline.SubmitRenderRequest` 提交离屏渲染，避免 `Camera.Render()` 在 SRP 下不可靠。
- 合成底图使用 `PanelPhoto/Photo` 初始配置的 `MainPhotoBg`，Logo 使用 `PanelPhoto/GameIcon` 初始配置的 `MainGameIcon`。
- 运行时加载当前 `CardBagNNN` Prefab，将 `GameBoard` 和全部 Piece Image 恢复为完整显示，旋转并等比适配到方形画布。
- 使用 `Application.productName`、当前日期和三位卡包 ID 生成文件名，例如 `Puffies-2026-07-24-001.png`；同一天同一卡包再次拍照覆盖同名文件。
- 生成的 Texture 同时创建运行时 Sprite 并赋给 `PanelPhoto/Photo`；照片面板显示时隐藏单独的 `GameIcon`，避免生成图中的 Logo 重复显示。
- 离开 MainScene 时释放运行时照片 Texture 和 Sprite。
- 首次 Play Mode 测试确认桌面 PNG 已成功生成且内容完整，但 `PanelPhoto` 未显示；原因集中在预览面板仍依附首页主 Canvas，无法稳定覆盖独立的卡包选择 Canvas 和 Renderer。
- 新增独立顶层 `PanelPhotoCanvas`，将编辑器中的 `PanelPhoto` 在运行时迁入该 Canvas，沿用原布局并固定使用 `32000` 排序层级。
- 复测发现选中卡包的 Mesh Renderer 仍显示在预览之上；打开预览时临时隐藏选中卡包，点击 `BtnOK` 后恢复，确保预览始终位于最上层。

## 修改文件

- `Assets/Scripts/Controller/MainScene.cs`
- `Documents/CURRENT_TASK.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/PROJECT_CONTEXT.md`

## 决策

- 旧卡包没有独立 Preview，照片内容统一从 `Resources/CardBagPrefabs/CardBagNNN` 还原，不要求每个卡包增加拍照资源。
- 文件名严格遵循 `游戏名-YYYY-MM-DD-BagId.png`，BagId 使用三位编号；同一天同一卡包重复保存时覆盖，不同卡包分别保留。
- 桌面不可用或写入失败时记录错误、恢复按钮，不显示无效照片面板。
- 拍照功能不写入 JSON、SQLite 或 `PlayerPrefs`。

## 验证

- `dotnet build Puffies.sln --no-restore`：三个程序集成功，`0` 警告、`0` 错误。
- `git diff --check -- Assets/Scripts/Controller/MainScene.cs`：通过，仅有既有 LF/CRLF 转换提示。
- Play Mode 已确认旧命名版本成功生成有效的 `1024 x 1024` PNG，包含木纹、完整拼图和 Logo；新命名格式等待复测。
- `PanelPhoto` 首次测试未显示；已改为独立顶层 Canvas，代码编译通过，等待复测面板显示和 `BtnOK`。
- 不涉及持久化结构变化，无需删除 `LocalData.db` 或 `LocalData.json`。

## 下一步

1. 重新进入 Unity Play Mode，选择一个已玩过的卡包并再次拍照。
2. 确认独立顶层 `PanelPhotoCanvas` 显示照片、相框和 `BtnOK`，且选中卡包不会覆盖预览。
3. 确认桌面文件名包含当前三位卡包 ID，例如 `Puffies-2026-07-24-001.png`。
4. 点击 `BtnOK`，确认面板关闭、选中卡包恢复并返回卡包选择页。

## 恢复提示

继续 Puffies 当前任务。先阅读 `AGENTS.md`、`Documents/WORKFLOW.md` 和 `Documents/CURRENT_TASK.md`；拍照保存代码已实现，下一步是在 Unity Play Mode 验证输出图片和面板显示效果。
