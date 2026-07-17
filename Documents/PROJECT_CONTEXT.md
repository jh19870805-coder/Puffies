# Project Context

Unity **2022.3** / URP 2D project. Core loop: card pack opening -> puzzle drag/drop -> task reward. This is the stable project reference for requirements, scenes, data, assets, build rules, and naming.

Current work state is tracked in [CURRENT_TASK.md](CURRENT_TASK.md). Workflow rules are in [WORKFLOW.md](WORKFLOW.md). Confirmed long-term game-design rules are recorded in [GAME_DESIGN_REQUIREMENTS.md](GAME_DESIGN_REQUIREMENTS.md).

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
| MainScene | Refresh card pack list from `CardPacks.csv` plus SQLite unlock state; provide Rank, Achieve, Menu, and pack-opening entry points |
| GameScene | Load `CardBagNNN` prefab by selected pack id; organize puzzle pieces by `PieceNN` group-number naming; when a group completes, switch groups and clear previous pieces; show RewardPanel after all pieces complete |
| RankScene | Enter from Main and return to Main |
| AchieveScene | Currently displays mock achievements; replace the data source when Steam integration is added |
| effect | Preview and debug CardFx |

### Data And Reward Requirements

- Task config comes from `Resources/Configs/TaskConfig.csv`.
- Card pack config comes from `Resources/Configs/CardPacks.csv`.
- Accumulate-score task type is `AccumulateScore` (`TaskType=1`); completing a puzzle adds that game's settlement score once.
- Current settlement uses the card-pack base score: XS 60, S 80, M 100, L 120, XL 140, XXL 160, XXXL 200. Confirmed hint, outline, and time bonuses will be integrated separately.
- Completed tasks grant rewards and advance to the next task.
- Card pack unlock/play state is stored in SQLite table `CardPacks`.
- Task progress is stored in JSON root object `TaskProgressData`.
- Business progress must not use `PlayerPrefs`.

### Content Extension Requirements

- New card packs use the existing `Package001` template; `MainScene` dynamically creates runtime slots.
- New puzzles are created by adding `CardBagNNN` prefabs under `Resources/CardBagPrefabs/`; each prefab contains `GameBoard` and `Piece01`...`PieceNN`; do not create Package JSON.
- 3D card packs and CardFx assets live under `Resources/Effects/` and are loaded with `Resources.Load`.
- Before builds, run `Puffies -> Sync Build Resources` to sync `Assets/UI` to `StreamingAssets/UI`.

### Pending Or Unfinished Requirements

- Formal Rank page content.
- Steam achievement integration, replacing AchieveScene mock data.
- Formal build regression.
- Board sliding to slot center was discussed but not merged; if still needed, implement as a separate small task.

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
    CardBagPrefabs/ CardBagNNN gameplay prefabs loaded by GameScene
  Prefabs/          Shared UI prefabs
  StreamingAssets/  UI build-sync output
  Plugins/SQLite/   sqlite-net
```

| Phase | 2D UI | 3D / FX |
|------|-------|---------|
| Editor | `Assets/UI` (scene Images reference directly) | `Assets/Resources/Effects` |
| Build | `StreamingAssets/UI` (`ToDiskPath`) | `Resources.Load("Effects/...")` |

- Do not rename `Resources`; code has hardcoded resource paths.
- GameScene dynamically loads `Resources/CardBagPrefabs/CardBagNNN.prefab` by selected pack id; `UI/CardBag001/` is the current card pack puzzle texture source.
- 3D effects stay under `Resources/Effects/`; do not duplicate them into StreamingAssets.

---

## 3. Scenes And Navigation

```text
LoadingScene (2.5s, TextLoading 0% -> 100%)
  -> MainScene
      -> BtnRank     -> RankScene     -> BtnReturn -> Main
      -> BtnAchieve  -> AchieveScene  -> CloseBtn  -> Main
      -> BtnMenu     -> PanelMenu     -> BtnClose / BtnReturn -> close menu
                    -> BtnSet        -> PanelSet -> BtnClose / BtnReturn -> close settings
                    -> BtnUsable     -> PanelUsable -> BtnClose / BtnReturn -> close usable options
                    -> BtnData       -> PanelSave -> BtnClose / BtnReturn -> close save panel
      -> unlocked pack runtime slot -> pack animation -> GameScene
          -> BtnReturn -> Main
          -> RewardPanel / BtnFinish -> Main
effect (debug): CardFx preview, menu Puffies -> Preview CardFx Effects
```

| Scene | Script | Notes |
|------|--------|-------|
| LoadingScene | `LoadingScene.cs` | Initializes JSON / SQLite / `GameTaskUtility` / `CardPackDataUtility` |
| MainScene | `MainScene.cs` | Card pack UI; refreshes by unlock state; 3D opening or 2D fallback |
| GameScene | `GameScene.cs` | Puzzle grouping and RewardPanel; saves the pack, accumulates settlement score task progress, and settles task rewards |
| RankScene / AchieveScene | Scene scripts | Return to Main |
| effect | `CardFxPreviewScene.cs` | CardObtain / CardTrail preview |

**Build Settings**: `LoadingScene` must be Index **0**.

| Object Name | Purpose |
|-------------|---------|
| `BtnRank` / `BtnAchieve` | Main -> Rank / Achieve |
| `BtnMenu` | MainScene opens `PanelMenu` |
| `PanelMenu` | MainScene menu popup, hidden on startup |
| `PanelMenu/BtnClose` | Close MainScene menu |
| `PanelMenu/BtnSet` | Open MainScene `PanelSet` settings popup |
| `PanelMenu/BtnUsable` | Open MainScene `PanelUsable` usable-options popup |
| `PanelMenu/BtnData` | Open MainScene `PanelSave` popup |
| `PanelSet` | MainScene settings popup for music, effect, and windowed mode |
| `PanelSet/SliderMusic` | Music volume setting |
| `PanelSet/SliderEffect` | Effect volume setting |
| `PanelSet/ToggleFrame` | Windowed-mode setting |
| `PanelSet/BtnClose` / `PanelSet/BtnReturn` | Close MainScene settings popup |
| `PanelUsable` | MainScene usable-options popup |
| `PanelUsable/Toggle1` / `Toggle2` / `Toggle3` | Persisted usable option toggles |
| `PanelUsable/BtnClose` / `PanelUsable/BtnReturn` | Close MainScene usable-options popup |
| `PanelSave` | MainScene save/data popup, currently display/hide only |
| `PanelSave/BtnClose` / `PanelSave/BtnReturn` | Close MainScene save/data popup |
| `BtnReturn` | Rank / Game -> Main; under `PanelMenu`, closes the MainScene menu |
| `CloseBtn` | Achieve -> Main |
| `BtnFinish` | Game RewardPanel -> Main |
| `TextLoading` | Loading progress text; supports TextMeshPro `TMP_Text` and legacy `UnityEngine.UI.Text` |
| `CardBagNNN` | Runtime gameplay prefab loaded from `Resources/CardBagPrefabs/` |
| `GameBoard` / `Piece01`... | Board and slots inside a `CardBagNNN` prefab |
| `ActiveGroupOutline` | Runtime baked-outline UGUI Image under `GameBoard` |
| `PieceBoard` | Puzzle piece tray |
| `RewardPanel` | Puzzle completion reward panel |
| `TaskItem` | Shared `Assets/Prefabs/TaskItem.prefab` instance used by MainScene task progress and GameScene RewardPanel settlement |
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
- MainScene settings are stored in `AppRecords` as collection/key `GameSettings/Runtime`: music volume, effect volume, and windowed mode.
- MainScene usable option toggles are stored in the same `GameSettings/Runtime` record as `UsableOption1`, `UsableOption2`, and `UsableOption3`.
- MainScene and GameScene both reference the same `TaskItem.prefab` GUID. Their scene overrides only position the root (`MainScene`: `10,508`; `GameScene`: `-6,455`); child layout and visuals must be changed in the shared prefab.
- Shared TaskItem child names are `TaskContent`, `TextProgress`, `ProgressMask`, `BagIcon`, and `BagBg`. Task UI binding code should resolve these names relative to the TaskItem instance and must not use scene-specific suffixes.
- `TaskProgressUIUtility` is the shared runtime binding for both TaskItem instances. `TextProgress` displays `CurrentCompleteValue / TaskConfig.CompleteValue`, and the visible `ProgressMask` width uses the clamped ratio of those values.
- MainScene refreshes TaskItem from persisted task data during `Start`. GameScene settlement rolls `TaskScore`, `TextProgress`, and `ProgressMask` together over 0.8 seconds using unscaled time; task reward and advancement are persisted before the animation.
- MainScene `PanelSet/SliderMusic` and `PanelSet/SliderEffect` are hand-built fake sliders: root Image background plus `SliderFill` and `SliderHandle` children. Runtime uses `FakeSettingsSliderInput` to handle pointer drag, refresh visuals, and save values.
- Do not use `PlayerPrefs`.
- Initialization happens in `LoadingScene.Start` for `JsonLocalStore`, `SqliteLocalStore`, `GameTaskUtility`, and `CardPackDataUtility`.

---

## 6. Adding Content

### Card Packs

`MainScene.RefreshPackageList` dynamically creates slots for unlocked packs from the database. Do not manually duplicate `Package002`, `Package003`, etc. in the scene.

1. Keep exactly one scene template object: `Package001`.
2. Add a row to `CardPacks.csv` (`PackId`, `PackSize`).
3. Add the corresponding cover under `UI/PackImages/` using `PackIconNNN.png` names. `GameDefine.FormatPackImagePath` maps pack id `1` to `UI/PackImages/PackIcon001.png`.
4. Write unlock/play state through `CardPackDataUtility` into SQLite table `CardPacks`.
5. Optional 3D assets: `CardPackAni_00N.FBX`, `CardPackSkin_00N.prefab` -> `Resources/Effects/CardPack/`; if missing, use 2D fallback.

### Puzzles

1. Create a prefab named `CardBagNNN` under `Assets/Resources/CardBagPrefabs/` where `NNN` matches `PackId`.
2. Put one child object named `GameBoard` inside the prefab.
3. Add grouped piece objects under `GameBoard` as `Image` objects using `Piece11`, `Piece12`, ... for group 1; `Piece21`, `Piece22`, ... for group 2; `Piece31`, ... for group 3. The group number is `PieceNN / 10`, sorted ascending.
4. Store source textures under `Assets/UI/CardBag001/` using grouped names such as `Pieces11`...`Pieces14` and `Pieces21`...`Pieces25`.
5. Do not use `PieceGroup` parent nodes; grouping comes only from the number after `Piece`.
6. Do not create Package JSON; runtime data comes from the loaded prefab's Images.
7. Run **Puffies -> Puzzles -> Bake Outline Masks** after adding or changing a CardBag. The baker merges Piece Alpha in GameBoard coordinates, closes narrow gaps, flood-fills the complete puzzle exterior, and writes `Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`.
8. `GameScene` displays the baked `#3f423e` group Sprite as a non-interactive `GameBoard` child. This is the primary path and does not draw internal Piece/group seams. Do not author outline objects in prefabs.
9. If a generated Sprite is missing, runtime logs an authoring warning and continues gameplay without an outline. Re-run the baker before delivery.

### Puzzle Outline Rendering

- Puzzle outlines are generated offline by `PuzzleOutlineBakerEditor` and rendered with Unity UGUI `Image`.
- The project has no runtime outline Shader, Renderer Feature, or third-party outline package.
- Keep baked-outline loading isolated from puzzle interaction; a missing outline must not prevent draggable pieces from being created.

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
| Puffies -> Puzzles -> Bake Outline Masks | Rebuild per-group exterior outlines for every CardBag prefab |

---

## 10. Deprecated

- `Assets/ArtRes/`, `Assets/Configs/`
- `Resources/Config/Package001.json` and JSON puzzle config flow
- One-off migration scripts under `Tools/*.ps1`
