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
| MainScene | Refresh card pack list from `CardPacks.csv` plus SQLite unlock state; display card packs in pages of 6 columns x 3 rows (18 per page); play the generic 3D opening model with the selected pack's real cover; provide Rank, Achieve, and Menu entry points |
| GameScene | Load `CardBagNNN` prefab by selected pack id; organize puzzle pieces by `PieceNN` group-number naming; when a group completes, switch groups and clear previous pieces; show RewardPanel after all pieces complete |
| RankScene | Enter from Main and return to Main |
| AchieveScene | Currently displays 20 mock achievements: the first 5 are achieved and the remaining 15 are unachieved; replace the data source when Steam integration is added |
| effect | Preview and debug CardFx |

### Data And Reward Requirements

- Task config comes from `Resources/Configs/TaskConfig.csv`.
- Card pack config comes from `Resources/Configs/CardPacks.csv`.
- Accumulate-score task type is `AccumulateScore` (`TaskType=1`); completing a puzzle adds that game's settlement score once.
- Settlement starts from the card-pack base score (XS 60, S 80, M 100, L 120, XL 140, XXL 160, XXXL 200), adds every qualified bonus percentage, multiplies once, and rounds upward.
- Score bonuses are: no `BtnTips` click +5%, MainScene `Toggle1` level outline disabled +2%, `Toggle2` sticker outline disabled +5%, and completion time <=15 / <=30 / <=60 seconds +3% / +2% / +1%.
- Completed tasks grant rewards and advance to the next task.
- Completing a task always creates a persisted new-card-pack entitlement. If the chapter hand-count gate is closed, the reward remains pending and is retried later. First-time completion performs one deterministic stage-gated grant attempt; replaying an already `Completed` pack does not perform this attempt. A replay may still create a task entitlement.
- Card-pack distribution uses 8 internal, player-invisible chapters for approximately 150 total packs (18.75 per chapter on average). Chapters constrain the eligible locked-pack reward pool but are not shown in MainScene or other player-facing UI. Exact PackId allocation and chapter advancement rules remain pending.
- Internal chapter stage uses `R`, the number of still-`Locked` packs in the active chapter: initial `17..9`, mid-to-late `8..3`, final `2..1`. Held playable count means `Unlocked + InProgress` and targets approximately `5-6`, `2-3`, and `1` packs respectively. For chapters larger than 18 packs, extra `R` values above 17 are also initial-stage values.
- Current distribution gates are: `R>=9` allows `H<=5`; `R=8` allows `H<=3`; `R=7..3` allows `H<=2`; `R=2..1` allows `H<=1`. A blocked first-completion attempt is skipped, while a blocked task reward remains pending. Both sources may grant in one settlement. RewardPanel keeps its authored default `ImgBag` Sprite; after `BtnFinish`, every pack granted in that settlement flies from `ImgBag` to a centered row, pauses, survives the MainScene load, and then flies to its corresponding list slot.
- When an accumulate-score task advances to another accumulate-score task, progress above the completed target carries forward (`nextProgress = currentProgress - completedTarget`).
- Card pack lifecycle state is stored in SQLite table `CardPacks` as `Locked`, `Unlocked`, `InProgress`, or `Completed`.
- MainScene card-pack order is: newly granted since the previous list presentation (one presentation only, newest grant first), then `InProgress`, then `Unlocked` by ascending unlock time, then `Completed` by ascending first-completion time. PackId is the deterministic tie-breaker; daily challenge priority is deferred.
- MainScene lightly tints `Completed` card-pack covers and size icons gray while keeping them replayable.
- Task progress is stored in JSON root object `TaskProgressData`.
- Business progress must not use `PlayerPrefs`.

### Content Extension Requirements

- New card packs use the existing `Package001` template; `MainScene` dynamically creates runtime slots.
- New puzzles are created by adding `CardBagNNN` prefabs under `Resources/CardBagPrefabs/`; each prefab contains `GameBoard` and `Piece01`...`PieceNN`; do not create Package JSON.
- The generic 3D card-pack opening model and CardFx assets live under `Resources/Effects/` and are loaded with `Resources.Load`.
- Before builds, run `Puffies -> Sync Build Resources` to sync runtime disk-loaded UI folders to `StreamingAssets/UI`.

### Pending Or Unfinished Requirements

- Formal Rank page content.
- Steam achievement integration, replacing AchieveScene mock data.
- Sequential settlement presentation that reveals each qualified bonus and rolls through cumulative step scores; current runtime performs one 0-to-final-score roll.
- Final chapter PackId allocation, chapter advancement rules, empty-pool handling, and final card-pack selection policy.
- Full Play Mode regression for card-pack lifecycle/distribution, reward flight, list ordering/paging, stable tray positions, and staged outlines.
- Formal build regression.
- Board sliding to slot center was discussed but not merged; if still needed, implement as a separate small task.

---

## 2. Directory And Loading Strategy

```text
Assets/
  Scenes/           LoadingScene (startup), MainScene, GameScene, RankScene, AchieveScene, effect
  UI/               2D source textures (PackImages, CardBags/CardBagNNN, BasicUI...)
  Scripts/          MVC
    Model/          Intentionally flat: core, config, persistence, task/card-pack data, and runtime utilities
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
| Build | Runtime disk-loaded folders in `StreamingAssets/UI` (`ToDiskPath`) | `Resources.Load("Effects/...")` |

- Do not rename `Resources`; code has hardcoded resource paths.
- GameScene dynamically loads `Resources/CardBagPrefabs/CardBagNNN.prefab` by selected pack id; source textures live under `UI/CardBags/CardBagNNN/` and are included through prefab Sprite references rather than StreamingAssets.
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
| `BtnFinish` | Animate newly granted packs from RewardPanel into their MainScene list slots, then complete the return to Main |
| `TextLoading` | Loading progress text; supports TextMeshPro `TMP_Text` and legacy `UnityEngine.UI.Text` |
| `CardBagNNN` | Runtime gameplay prefab loaded from `Resources/CardBagPrefabs/` |
| `GameBoard` / `Piece01`... | Board and slots inside a `CardBagNNN` prefab |
| `ActiveGroupOutline` | Runtime baked-outline UGUI Image under `GameBoard` |
| `PieceBoard` | Puzzle piece tray |
| `RewardPanel` | Puzzle completion reward panel |
| `TaskItem` | Shared `Assets/Prefabs/TaskItem.prefab` instance used by MainScene task progress and GameScene RewardPanel settlement |
| `Package001` | MainScene card pack slot template, hidden and cloned at runtime |
| `PackItem/PackCover` | MainScene card pack cover Image; runtime assigns `PackIconNNN.png` |
| `PackItem/PackSize` | Card pack size icon; runtime selects `PackSize_1.png` through `PackSize_7.png` from the configured `CardPackSize` value |

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
| Card pack config (`PackId`, `PackSize`, `ChapterId`) | `GameConfigRepository` reads `Resources/Configs/CardPacks.csv` | Read-only |
| Card pack lifecycle state | `CardPackDataUtility` | `LocalData.db` table `CardPacks` |
| Generic collection + key storage | `SqliteLocalStore` API | `LocalData.db` table `AppRecords` |

- `GameConfigRepository` loads and caches task/card pack config. Current source is `ResourcesGameConfigTextSource`, which prefers `Resources.Load<TextAsset>` and falls back to editor disk path.
- `CsvTable` is the unified CSV parser with header access, quoted fields, and empty-line filtering; business code should not directly `Split(',')`.
- `JsonLocalStore` reads/writes one root object for the whole file, currently task progress.
- `SqliteLocalStore` uses collection/key records in `AppRecords`; card pack business state uses the dedicated `CardPacks` table.
- `CardPackLifecycleState` is `Locked=0`, `Unlocked=1`, `InProgress=2`, and `Completed=3`. Completing the first group of a multi-group pack marks it `InProgress`; completing the final group marks it `Completed`.
- The SQLite `CardPacks` table contains `PackId`, `PackSize`, `LifecycleState`, `UnlockTime`, and `CompletionTime`; the former `IsUnlocked` and `IsPlayed` columns are not retained. Unlock/completion timestamps use invariant `yyyy-MM-dd HH:mm:ss.fff` local time. `CompletionTime` is written on the first transition to `Completed` and is not changed by replay.
- `CardPackDistributionUtility` lives with `CardPackDataUtility` and owns chapter selection, `R` / held-count evaluation, deterministic locked-candidate selection, and first-completion grant attempts. Replays skip this attempt based on the lifecycle snapshot taken when GameScene starts.
- Pending task card-pack entitlements are stored in SQLite `AppRecords` under `CardPackDistribution/Progress`, deduplicated by TaskId.
- GameScene persists the task entitlement before advancing the task and only attempts delivery after task advancement succeeds, preventing duplicate grants when task-progress persistence fails.
- MainScene settings are stored in `AppRecords` as collection/key `GameSettings/Runtime`: music volume, effect volume, and windowed mode.
- MainScene usable option toggles are stored in the same `GameSettings/Runtime` record as `UsableOption1`, `UsableOption2`, and `UsableOption3`.
- `UsableOption1` is the level outer-frame toggle and defaults to enabled for newly created settings; `UsableOption2` is the sticker/full-contour toggle, and `UsableOption3` is high contrast. Persisted user choices remain authoritative.
- MainScene and GameScene both reference the same `TaskItem.prefab` GUID. Their scene overrides only position the root (`MainScene`: `10,508`; `GameScene`: `-6,455`); child layout and visuals must be changed in the shared prefab.
- Shared TaskItem child names are `TaskContent`, `TextProgress`, `ProgressMask`, `BagIcon`, and `BagBg`. Task UI binding code should resolve these names relative to the TaskItem instance and must not use scene-specific suffixes.
- `TaskProgressUIUtility` is the shared runtime binding for both TaskItem instances. `TextProgress` displays `CurrentCompleteValue / TaskConfig.CompleteValue`, and the visible `ProgressMask` width uses the clamped ratio of those values.
- MainScene refreshes TaskItem from persisted task data during `Start`. GameScene settlement rolls `TaskScore`, `TextProgress`, and `ProgressMask` together over 0.8 seconds using unscaled time; task reward and advancement are persisted before the animation.
- GameScene settlement summary binds `TaskBg2/TaskScore` to the current game's settlement score and `TaskBg2/TaskBagNum` to the current SQLite count of unlocked card packs. A pack unlocked by the current task reward is included immediately.
- GameScene snapshots outline settings on entry, marks hint use on `BtnTips` click, starts its unscaled score timer on the first successful Piece placement, and freezes time when RewardPanel settlement begins.
- MainScene `PanelSet/SliderMusic` and `PanelSet/SliderEffect` are hand-built fake sliders: root Image background plus `SliderFill` and `SliderHandle` children. Runtime uses `FakeSettingsSliderInput` to handle pointer drag, refresh visuals, and save values.
- Do not use `PlayerPrefs`.
- Initialization happens in `LoadingScene.Start` for `JsonLocalStore`, `SqliteLocalStore`, `GameTaskUtility`, and `CardPackDataUtility`.
- `Assets/Scripts/Model` intentionally remains a single flat folder. Related pure C# types are consolidated as follows: `GameManager` lives in `GameDefine.cs`, CSV parser types live in `GameConfigRepository.cs`, both `JsonLocalStore` and `SqliteLocalStore` live in `LocalDataStore.cs`, and score types/`GameScoreUtility` live in `GameTaskUtility.cs`. Public type names and call sites remain unchanged.
- MainScene card-pack opening always instantiates `Resources/Effects/CardPack/CardPackOpening.prefab`. `GameAnimationUtility` applies the selected entry's already-loaded `PackIconNNN` texture through `MaterialPropertyBlock`, center-crops its UV to the reference `1822 x 2301` cover aspect, and fits the model uniformly inside the clicked UI bounds. Shared `CardPackLit.mat` is not mutated.

### Development Persistence Policy

- Local persistence has no backward-compatibility guarantee during active development. Change data structures and SQLite field types directly to the current required shape; do not add migrations or legacy fallbacks unless explicitly requested.
- After an incompatible SQLite schema change, close Unity and delete `%USERPROFILE%/AppData/LocalLow/MainTown/Puffies/LocalData.db` before testing.
- Also delete `LocalData.json` when JSON task progress or behavior spanning both persistence stores changed. The assistant must identify the required files after each incompatible change and must not delete them without an explicit request.

---

## 6. Adding Content

### Card Packs

`MainScene.RefreshPackageList` dynamically creates slots for unlocked packs from the database. Do not manually duplicate `Package002`, `Package003`, etc. in the scene.

Shared size icons are `UI/PackImages/PackSize_1.png` through `PackSize_7.png`, matching the numeric values of `CardPackSize` (`XS=1` through `XXXL=7`). `PackItem` must contain Image children named `PackCover` and `PackSize`; MainScene assigns both Sprites at runtime and scales the size icon with the authored cover dimensions.

`PackItem/PackShadow` is a sibling Image rendered behind `PackCover`. MainScene samples the readable runtime cover texture, downsizes its alpha to the `240 x 272` display size, and applies three separable box-blur passes with horizontal radius 2 and vertical radius 5. It creates a cached `256 x 344` shadow Sprite at offset `(0,-20)`, so the directional projection appears below rather than to the right. Horizontal/vertical padding is `8/36` pixels. Shadow color is `#1f292d` with maximum alpha `0.52`. Generated shadow Sprites and textures are released when MainScene is destroyed. Keep `PackSize` above both images.

1. Keep exactly one scene template object: `Package001`.
2. Add a row to `CardPacks.csv` (`PackId`, `PackSize`, `ChapterId`).
3. Add the corresponding cover under `UI/PackImages/` using `PackIconNNN.png` names. `GameDefine.FormatPackImagePath` maps pack id `1` to `UI/PackImages/PackIcon001.png`.
4. Write lifecycle state through `CardPackDataUtility` into SQLite table `CardPacks`.
5. Do not create per-pack 3D assets. The runtime reuses `CardPackOpeningAnimation.FBX`, `CardPackOpening.prefab`, and `CardPackLit.mat`; the selected `PackIconNNN.png` becomes the animated model cover. If the generic assets are missing, MainScene uses the 2D fallback.

### Puzzles

1. Create a prefab named `CardBagNNN` under `Assets/Resources/CardBagPrefabs/` where `NNN` matches `PackId`.
2. Put one child object named `GameBoard` inside the prefab.
3. Add grouped piece objects under `GameBoard` as `Image` objects using `Piece11`, `Piece12`, ... for group 1; `Piece21`, `Piece22`, ... for group 2; `Piece31`, ... for group 3. The group number is `PieceNN / 10`, sorted ascending.
4. Store source textures under `Assets/UI/CardBags/CardBagNNN/` using grouped names such as `Pieces11`...`Pieces14` and `Pieces21`...`Pieces25`.
5. Do not use `PieceGroup` parent nodes; grouping comes only from the number after `Piece`.
6. Do not create Package JSON; runtime data comes from the loaded prefab's Images.
7. Run **Puffies -> Puzzles -> Bake Outline Masks** after adding or changing a CardBag. The baker merges Piece Alpha in GameBoard coordinates, closes narrow gaps, and writes `Resources/Generated/PuzzleOutlines/CardBagNNN/GroupNN.png`. Group 1 contains only its final-puzzle exterior. Every later image contains only the current group's final-puzzle exterior and its contact edges with lower-number completed groups.
8. `GameScene` displays the baked `#3f423e` current-group Sprite as a non-interactive `GameBoard` child. The mask excludes completed groups' unrelated boundaries, current-to-future-group edges, and individual Piece seams inside the same group. Do not author outline objects in prefabs.
9. If a generated Sprite is missing, runtime logs an authoring warning and continues gameplay without an outline. Re-run the baker before delivery.
- Draggable pieces are positioned once when a group is created. After a Piece is placed, every remaining tray Piece retains its established X and Y position; gaps are not compacted until the next group is created.

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
| Generic card pack prefab | `CardPackOpening` | `Resources/Effects/CardPack/` |
| Generic card pack controller | `CardPackOpening.controller` | Same |
| Generic pack animation | `CardPackOpeningAnimation.FBX` | Same |
| Generic card pack model | `CardPackOpeningModel.FBX` | Same |
| Default card pack cover | `CardPackDefaultCover.png` | Same |
| Material | `CardPackLit` | Same |
| Plane group | `PlaneGroup_001` | `Resources/Effects/PlaneGroup/` |
| New card obtain | `CardObtain_001` | `Resources/Effects/CardFx/` |
| Card trail | `CardTrail_001` | Same |

---

## 8. Build

Before building, run **Puffies -> Sync Build Resources**. It copies `PackImages`, `BasicUI`, `AchieveScene`, and `RankScene` to `StreamingAssets/UI`; CardBag source textures stay out because their Sprite references are included through gameplay prefabs.

Suggested Build Settings order: LoadingScene -> MainScene -> GameScene -> effect -> RankScene -> AchieveScene.

### Development Workstation

- Required command-line SDK: .NET 8 SDK. A specific patch version is not pinned.
- Required VS Code extensions: C# (`ms-dotnettools.csharp`), C# Dev Kit (`ms-dotnettools.csdevkit`), and Microsoft Unity (`visualstudiotoolsforunity.vstuc`).
- `.vscode/extensions.json` provides editor recommendations; extension binaries and the .NET SDK are installed separately on each device.
- On a new device, Codex should check these prerequisites and request approval to install missing items before investigating Unity C# project-load errors.
- `Assembly-CSharp*.csproj` files are generated by Unity and must not be manually converted or edited for VS Code compatibility.

---

## 9. Editor Menu Reference

| Menu | Purpose |
|------|---------|
| Puffies -> Sync Build Resources | Copy runtime disk-loaded UI folders to StreamingAssets |
| Puffies -> Canvas -> Apply Design Resolution | Apply Canvas resolution |
| Puffies -> Fonts -> Setup Default Chinese Font | Chinese font setup |
| Puffies -> Preview CardFx Effects | Open effect scene |
| Puffies -> Puzzles -> Bake Outline Masks | Rebuild per-group exterior outlines for every CardBag prefab |

---

## 10. Deprecated

- `Assets/ArtRes/`, `Assets/Configs/`
- `Resources/Config/Package001.json` and JSON puzzle config flow
- One-off migration scripts under `Tools/*.ps1`
