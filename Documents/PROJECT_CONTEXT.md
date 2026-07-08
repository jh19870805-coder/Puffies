# Project Context

Unity **2022.3** / URP 2D project. Core loop: card pack opening -> puzzle drag/drop -> task reward. This is the stable project reference for requirements, scenes, data, assets, build rules, and naming.

Current work state is tracked in [CURRENT_TASK.md](CURRENT_TASK.md). Workflow rules are in [WORKFLOW.md](WORKFLOW.md).

---

## 1. Feature Requirements

### Core Loop

1. `LoadingScene` initializes local data, task config, card pack config, and persistent storage.
2. `MainScene` displays playable card packs based on unlock state.
3. Clicking an unlocked pack plays pack-opening presentation and enters `GameScene`.
4. The player drags puzzle pieces to complete the selected pack puzzle.
5. Puzzle completion shows `RewardPanel`, settles task progress, and saves card pack state.
6. `BtnFinish` returns to `MainScene`, where the card pack list refreshes from latest unlock state.

### Scene Requirements

| Scene | Requirements |
|------|--------------|
| LoadingScene | Initialize JSON, SQLite, task data, and card pack data; enter MainScene after loading |
| MainScene | Refresh card pack list from `CardPacks.csv` plus SQLite unlock state; provide Rank, Achieve, and pack-opening entry points |
| GameScene | Organize puzzle pieces by `PieceGroup` or default grouping; when a group completes, switch groups and clear previous pieces; show RewardPanel after all pieces complete |
| RankScene | Enter from Main and return to Main |
| AchieveScene | Currently displays mock achievements; replace the data source when Steam integration is added |
| effect | Preview and debug CardFx |

### Data And Reward Requirements

- Task config comes from `Resources/Configs/TaskConfig.csv`.
- Card pack config comes from `Resources/Configs/CardPacks.csv`.
- Collect-puzzle task type is `CollectPuzzle` (`TaskType=1`); each placed puzzle piece adds +1 progress.
- Completed tasks grant rewards and advance to the next task.
- Card pack unlock/play state is stored in SQLite table `CardPacks`.
- Task progress is stored in JSON root object `TaskProgressData`.
- Business progress must not use `PlayerPrefs`.

### Content Extension Requirements

- New card packs use the existing `Package001` template; `MainScene` dynamically creates runtime slots.
- New puzzles are created by adding `Piece01`...`PieceNN` under `GameBoard`; do not create Package JSON.
- 3D card packs and CardFx assets live under `Resources/Effects/` and are loaded with `Resources.Load`.
- Before builds, run `Puffies -> Sync Build Resources` to sync `Assets/UI` to `StreamingAssets/UI`.

### Pending Or Unfinished Requirements

- Formal Rank page content.
- Steam achievement integration, replacing AchieveScene mock data.
- Formal build regression.
- Board sliding to slot center and slot outline rendering were discussed but not merged; if still needed, implement as separate small tasks.

---

## 2. Directory And Loading Strategy

```text
Assets/
  Scenes/           LoadingScene (startup), MainScene, GameScene, RankScene, AchieveScene, effect
  UI/               2D source textures (PackImages, CardBag001, BasicUI...)
  Scripts/          MVC
    Model/          GameDefine, GameManager, utilities, local storage, task/card pack data
    View/           PackageInteractionHandler
    Controller/     Scene scripts
    Editor/         Build sync, Canvas resolution, Chinese font, CardFx preview
  Resources/
    Configs/        TaskConfig.csv, CardPacks.csv
    Effects/
      CardPack/     3D card packs
      PlaneGroup/
      CardFx/       Card obtain/trail prefabs plus Materials/Textures/Meshes/Shaders
  Prefabs/          Reserved
  StreamingAssets/  UI build-sync output
  Plugins/SQLite/   sqlite-net
```

| Phase | 2D UI | 3D / FX |
|------|-------|---------|
| Editor | `Assets/UI` (scene Images reference directly) | `Assets/Resources/Effects` |
| Build | `StreamingAssets/UI` (`ToDiskPath`) | `Resources.Load("Effects/...")` |

- Do not rename `Resources`; code has hardcoded resource paths.
- GameScene puzzles are based on scene `Image` objects; `UI/CardBag001/` is the current card pack puzzle texture source.
- 3D effects stay under `Resources/Effects/`; do not duplicate them into StreamingAssets.

---

## 3. Scenes And Navigation

```text
LoadingScene (2.5s, TextLoading 0% -> 100%)
  -> MainScene
      -> BtnRank     -> RankScene     -> BtnReturn -> Main
      -> BtnAchieve  -> AchieveScene  -> CloseBtn  -> Main
      -> unlocked pack runtime slot -> pack animation -> GameScene
          -> BtnReturn -> Main
          -> RewardPanel / BtnFinish -> Main
effect (debug): CardFx preview, menu Puffies -> Preview CardFx Effects
```

| Scene | Script | Notes |
|------|--------|-------|
| LoadingScene | `LoadingScene.cs` | Initializes JSON / SQLite / `GameTaskUtility` / `CardPackDataUtility` |
| MainScene | `MainScene.cs` | Card pack UI; refreshes by unlock state; 3D opening or 2D fallback |
| GameScene | `GameScene.cs` | Puzzle grouping and RewardPanel; collects puzzle task progress; saves pack and settles task |
| RankScene / AchieveScene | Scene scripts | Return to Main |
| effect | `CardFxPreviewScene.cs` | CardObtain / CardTrail preview |

**Build Settings**: `LoadingScene` must be Index **0**.

| Object Name | Purpose |
|-------------|---------|
| `BtnRank` / `BtnAchieve` | Main -> Rank / Achieve |
| `BtnReturn` | Rank / Game -> Main |
| `CloseBtn` | Achieve -> Main |
| `BtnFinish` | Game RewardPanel -> Main |
| `TextLoading` | Loading progress text |
| `GameBoard` / `Piece01`... | GameScene board and slots |
| `PieceGroup01`... | Optional grouping parent nodes |
| `PieceBoard` | Puzzle piece tray |
| `RewardPanel` | Puzzle completion reward panel |
| `Package001` | MainScene card pack slot template, hidden and cloned at runtime |

---

## 4. Design Resolution And Fonts

| Item | Value |
|------|-------|
| Design resolution | **2560 x 1440** |
| PPU | 100 (`GameDefine.PixelsPerUnit`) |

| Menu | Purpose |
|------|---------|
| **Puffies -> Canvas -> Apply Design Resolution** | Apply 2560 x 1440 in bulk |
| **Puffies -> Fonts -> Setup Default Chinese Font** | Noto Sans SC TMP + UI Text |

New `CanvasScaler` values are written by `CanvasDesignResolutionEditor.cs`. Use `GameFontUtility` in code; do not hardcode font paths.

---

## 5. Data And Config

| Data | Source | Runtime Persistence |
|------|--------|---------------------|
| Task config | `GameConfigRepository` reads `Resources/Configs/TaskConfig.csv` | Read-only |
| Task progress | `GameTaskUtility` | `persistentDataPath/LocalData.json` root object `TaskProgressData` |
| Card pack config | `GameConfigRepository` reads `Resources/Configs/CardPacks.csv` | Read-only |
| Card pack unlock/play state | `CardPackDataUtility` | `LocalData.db` table `CardPacks` |
| Generic collection + key storage | `SqliteLocalStore` API | `LocalData.db` table `AppRecords` |

- `GameConfigRepository` loads and caches task/card pack config. Current source is `ResourcesGameConfigTextSource`, which prefers `Resources.Load<TextAsset>` and falls back to editor disk path.
- `CsvTable` is the unified CSV parser with header access, quoted fields, and empty-line filtering; business code should not directly `Split(',')`.
- `JsonLocalStore` reads/writes one root object for the whole file, currently task progress.
- `SqliteLocalStore` uses collection/key records in `AppRecords`; card pack business state uses the dedicated `CardPacks` table.
- Do not use `PlayerPrefs`.
- Initialization happens in `LoadingScene.Start` for `JsonLocalStore`, `SqliteLocalStore`, `GameTaskUtility`, and `CardPackDataUtility`.

---

## 6. Adding Content

### Card Packs

`MainScene.RefreshPackageList` dynamically creates slots for unlocked packs from the database. Do not manually duplicate `Package002`, `Package003`, etc. in the scene.

1. Keep exactly one scene template object: `Package001`.
2. Add a row to `CardPacks.csv` (`PackId`, `PackSize`).
3. Add the corresponding cover under `UI/PackImages/` using `GameDefine.FormatPackImagePath`.
4. Write unlock/play state through `CardPackDataUtility` into SQLite table `CardPacks`.
5. Optional 3D assets: `CardPackAni_00N.FBX`, `CardPackSkin_00N.prefab` -> `Resources/Effects/CardPack/`; if missing, use 2D fallback.

### Puzzles

1. Add `Piece01`...`PieceNN` under the scene `GameBoard` as `Image` objects.
2. Store source textures under `Assets/UI/CardBag001/` using grouped names such as `Pieces11`...`Pieces14` and `Pieces21`...`Pieces25`.
3. Use `PieceGroup01`... parent nodes for explicit grouping, or rely on default `Piece01-04` / `Piece05+` grouping.
4. Do not create Package JSON; runtime data comes from scene Images.

### CardFx

Prefabs and dependencies go under `Resources/Effects/CardFx/`, for example `CardObtain_001` and `CardTrail_001`.

---

## 7. Naming

| Type | Name | Path |
|------|------|------|
| Card pack skin | `CardPackSkin_001` | `Resources/Effects/CardPack/` |
| Pack animation | `CardPackAni_001.FBX` | Same |
| Material | `CardPackLit` | Same |
| Plane group | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| New card obtain | `CardObtain_001` | `Resources/Effects/CardFx/` |
| Card trail | `CardTrail_001` | Same |

---

## 8. Build

Before building, run **Puffies -> Sync Build Resources** (`UI` -> `StreamingAssets/UI`).

Suggested Build Settings order: LoadingScene -> MainScene -> GameScene -> effect -> RankScene -> AchieveScene.

---

## 9. Editor Menu Reference

| Menu | Purpose |
|------|---------|
| Puffies -> Sync Build Resources | UI -> StreamingAssets |
| Puffies -> Canvas -> Apply Design Resolution | Apply Canvas resolution |
| Puffies -> Fonts -> Setup Default Chinese Font | Chinese font setup |
| Puffies -> Preview CardFx Effects | Open effect scene |

---

## 10. Deprecated

- `Assets/ArtRes/`, `Assets/Configs/`
- `Resources/Config/Package001.json` and JSON puzzle config flow
- One-off migration scripts under `Tools/*.ps1`
