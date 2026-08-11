# 当前任务

- 任务：限制 Piece 自由放置区域并恢复托盘回收
- 状态：棋盘边界和托盘归位修正已完成，等待 Play Mode 验证
- 更新时间：2026-08-11

## 用户意图

- Piece 可以停在棋盘左右两侧的桌面。
- Piece 可以停在棋盘内 Alpha 为 `0`、没有已拼图占用的位置。
- Piece 不得停在棋盘正上方、正下方或已拼内容上。
- Piece 不得横跨棋盘边框；棋盘内和左右桌面的合法位置都必须容纳完整 Piece 渲染边界。
- 鼠标在棋盘下方的黑色托盘区域松手时，Piece 自动回托盘；托盘需要显示时自动恢复。

## 工作记录

- 松手流程保持正确吸附为最高优先级，随后按鼠标位置判断托盘回收。
- 缓存托盘原始归一化屏幕区域，托盘滑出或窗口尺寸变化后仍可判断底部回收热区。
- 移除“托盘必须仍有其他 Piece”与“Piece 必须进入托盘高度 50%”的旧回收门槛，改为鼠标进入托盘区域即可回收。
- 为 Alpha 大于 `0` 的已拼 Groove Image 延迟创建并复用 Physics Shape 探针；Alpha 为 `0` 的凹槽不阻挡自由放置。
- Piece 与棋盘相交时，只要实际轮廓压到已拼内容就回弹；未压到时，Piece 中心在棋盘内可停放。
- Piece 中心位于棋盘左右外侧时可停在桌面；中心位于棋盘正上方或正下方时回弹。
- 放置区域改按完整 Piece 渲染边界判断，任何跨越棋盘边框的状态均回弹。
- 底部回收统一先恢复并启用托盘、刷新 Canvas，再按编号计算新的托盘目标坐标并回弹到该位置。
- 保留外部 Piece 之间防重叠、背景可见范围限制、错误回弹和红色反馈。

## 修改文件

- `Assets/Scripts/Controller/GameScene.cs`
- `Documents/PROJECT_CONTEXT.md`
- `Documents/GAME_DESIGN_REQUIREMENTS.md`
- `Documents/CURRENT_TASK.md`
- `specs/spec-driven-development.md`

## 决策

- “凹槽透明度为 0”按运行时 `Image.color.a <= 0.001` 判断；已拼内容继续使用 Sprite Physics Shape，而不是矩形包围盒。
- 棋盘内、左侧桌面和右侧桌面按 Piece 完整渲染边界分类；跨越 GameBoard 任意边框时禁止放置，实际轮廓与已拼内容相交时同样禁止放置。
- 托盘回收按鼠标松手点判断，不要求 Piece 本身进入托盘 50%。
- 本次不修改场景、Prefab、切图、配置或持久化结构，不需要清理本地数据。

## 验证

- `dotnet build Assembly-CSharp.csproj --no-restore` 已通过，`0` 警告、`0` 错误。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 已通过，`0` 警告、`0` 错误。
- `git diff --check` 已通过，仅有仓库既有的 LF/CRLF 转换提示。
- 棋盘边界和托盘目标修正后，运行时与编辑器程序集已重新编译通过，均为 `0` 警告、`0` 错误。
- 待在 Play Mode 验证棋盘左右、棋盘空位、已拼内容、棋盘上下和隐藏托盘回收路径。

## 下一步

1. 在 Play Mode 覆盖五类松手位置和正确吸附优先级。
2. 回归最后一块拿起后托盘收下，再拖回底部区域时托盘恢复和重排。
3. 继续抽查 CardBag013 落位尺寸不再二次放大。

## 恢复提示

Piece 放置已限制为完整落在棋盘内未占用区域或完整落在棋盘左右桌面，横跨棋盘边框会回弹；底部托盘原始区域会恢复托盘并将 Piece 排回托盘内部。两个 C# 项目已编译通过，下一步进行 Play Mode 交互验证。
