using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class DraggablePieceState
{
    public SpriteRenderer PieceRenderer;
    public Image GrooveImage;
    public RectTransform GrooveRect;
    public Vector3 StartPosition;
    public Vector3 TrayScale;
    public Vector3 DragScale;
    public bool IsOnTray;
    public bool IsPlaced;
}

public sealed class BoardState
{
    public Image GameBoardImage;
    public RectTransform BackgroundRect;
    public RectTransform PieceBoardRect;
    public SpriteRenderer PieceBgRenderer;
    public List<List<Image>> GrooveImagesByGroup = new List<List<Image>>();
    public bool IsBoardAndGroovesInitialized;
}

public sealed class DragState
{
    public readonly List<DraggablePieceState> CurrentGroupDraggables = new List<DraggablePieceState>();
    public int CurrentGroupIndex = -1;
    public DraggablePieceState DraggingPiece;
    public Vector3 DragOffset;
}

public static class GameDefine
{
    // Scene names
    public const string SceneLoading = "LoadingScene";
    public const string SceneMain = "MainScene";
    public const string SceneGame = "GameScene";
    public const string SceneRank = "RankScene";
    public const string SceneAchieve = "AchieveScene";
    public const string SceneEffect = "EffectScene001";

    public const string EffectsRoot = "Assets/Resources/Effects";
    public const string CardPackFolder = "CardPack";
    public const string PlaneGroupFolder = "PlaneGroup";
    public const string CardFxFolder = "CardFx";

    public const string PlaneGroupPrefabEditorPath = "Assets/Resources/Effects/PlaneGroup/PlaneGroup_001.prefab";
    public const string PlaneGroupMaterialEditorPath = "Assets/Resources/Effects/PlaneGroup/PlaneGroupLit.mat";
    public const string PlaneGroupPrefabResourcesPath = "Effects/PlaneGroup/PlaneGroup_001";
    public const string PlaneGroupMaterialResourcesPath = "Effects/PlaneGroup/PlaneGroupLit";
    public const string PlaneGroupFbxEditorFolder = "Assets/Resources/Effects/PlaneGroup";

    public const string CardPackPrefabResourcesFolder =
        "Effects/CardPack/CardBagPrefab/CardBag01/";
    public const string CardPackOpeningPrefabName = "CardPackOpening_001";
    public const string CardPackMaterialResourcesPath =
        "Effects/CardPack/ModTextures/Materials/CardPackOpeningMaterial";
    public const string CardPackPrefabEditorFolder =
        "Assets/Resources/Effects/CardPack/CardBagPrefab/CardBag01";
    public const string CardPackMaterialEditorPath =
        "Assets/Resources/Effects/CardPack/ModTextures/Materials/CardPackOpeningMaterial.mat";
    public const string CardPackDismantleResourcesPath =
        "Effects/CardFx/Profabs/fx_chai_w_001";
    public const string CardObtainPrefabName = "CardObtain_001";
    public const string CardTrailPrefabName = "CardTrail_001";

    public const string CardObtainPrefabEditorPath = "Assets/Resources/Effects/CardFx/CardObtain_001.prefab";
    public const string CardTrailPrefabEditorPath = "Assets/Resources/Effects/CardFx/CardTrail_001.prefab";
    public const string CardObtainPrefabResourcesPath = "Effects/CardFx/CardObtain_001";
    public const string CardTrailPrefabResourcesPath = "Effects/CardFx/CardTrail_001";
    public const string CardFxEditorFolder = "Assets/Resources/Effects/CardFx";

    public static string FormatCardBagPrefabResourcesPath(int bagId)
    {
        return $"{CardBagPrefabResourcesFolder}{CardBagPrefabPrefix}{bagId:D3}";
    }

    public static string FormatPuzzleOutlineResourcesPath(int bagId, int groupNumber)
    {
        return $"{PuzzleOutlineResourcesFolder}{CardBagPrefabPrefix}{bagId:D3}/Group{groupNumber:D2}";
    }

    public static string FormatPackImagePath(int packId)
    {
        return $"{UiRoot}/{PackImagesFolder}/{PackImageFilePrefix}{packId:D3}{ImageExtPng}";
    }

    public static string FormatPackSizeImagePath(CardPackSize packSize)
    {
        return $"{UiRoot}/{PackImagesFolder}/{PackSizeImageFilePrefix}{(int)packSize}{ImageExtPng}";
    }

    // Common path tokens
    public const string AssetsRoot = "Assets";
    public const string UiRoot = "UI";
    public const string ConfigsRoot = "Configs";
    public const string PackImagesFolder = "PackImages";
    public const string PackImageFilePrefix = "PackIcon";
    public const string PackSizeImageFilePrefix = "PackSize_";
    public const string CardBagPrefabResourcesFolder = "CardBagPrefabs/";
    public const string CardBagPrefabPrefix = "CardBag";
    public const string PuzzleOutlineResourcesFolder = "Generated/PuzzleOutlines/";

    public const string TaskConfigFileName = "TaskConfig.csv";
    public const string TaskConfigEditorPath = "Assets/Resources/Configs/TaskConfig.csv";
    public const string TaskConfigResourcesPath = "Configs/TaskConfig";
    public const int DefaultTaskId = 1;

    // Resource file names and suffixes
    public const string PackageFilePrefix = "Package";
    public const string RankButtonObjectName = "BtnRank";
    public const string AchieveButtonObjectName = "BtnAchieve";
    public const string ReturnButtonObjectName = "BtnReturn";
    public const string RewardPanelObjectName = "RewardPanel";
    public const string FinishButtonObjectName = "BtnFinish";
    public const string LoadingTextObjectName = "TextLoading";
    public const string LoadingTextFormat = "Loading... {0}%";
    public const float LoadingDurationSeconds = 2.5f;
    public const string GameBoardFileName = "GameBoard.png";
    public const string GameBoardObjectName = "GameBoard";
    public const string BackgroundObjectName = "Background";
    public const string PieceObjectPrefix = "Piece";
    public const string PieceBoardObjectName = "PieceBoard";
    public const string MainBackgroundFileName = "MainBg.png";
    public const string MainBackgroundPath = UiRoot + "/BasicUI/" + MainBackgroundFileName;
    public const string ImageExtPng = ".png";
    public const string ImageExtJpg = ".jpg";
    public const string ImageExtJpeg = ".jpeg";
    public const string ImageExtWebp = ".webp";

    // Default runtime values
    public const int DefaultBagId = 1;
    public const int InvalidId = -1;

    // Local persistence (runtime: persistentDataPath/LocalData.json & LocalData.db)
    public const string LocalDataBaseName = "LocalData";
    public const string LocalJsonFileName = LocalDataBaseName + ".json";
    public const string LocalSqliteFileName = LocalDataBaseName + ".db";
    public const string LocalSqliteCollectionTable = "AppRecords";
    public const string LocalSqliteCardPackTable = "CardPacks";
    public const string LocalSqliteCardPackPuzzleProgressTable = "CardPackPuzzleProgress";
    public const string LocalSqliteAppRecordsCollectionIndex = "IdxAppRecordsCollection";

    public const string CardPackConfigFileName = "CardPacks.csv";
    public const string CardPackConfigEditorPath = "Assets/Resources/Configs/CardPacks.csv";
    public const string CardPackConfigResourcesPath = "Configs/CardPacks";

    // Design resolution (2560×1440, PPU 100) — 新建 UI 场景/Canvas 须与此一致
    public const float DesignWidth = 2560f;
    public const float DesignHeight = 1440f;
    public const float PixelsPerUnit = 100f;

    // Default Chinese font (Noto Sans SC) — TMP SDF 由菜单 Puffies → Fonts → Setup Default Chinese Font 生成
    public const string DefaultChineseFontEditorPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansSC-Regular.ttf";
    public const string DefaultChineseTmpFontEditorPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansSC-Regular SDF.asset";
    public const string DefaultChineseFontResourcesPath = "Fonts & Materials/NotoSansSC-Regular";
    public const string DefaultChineseTmpFontResourcesPath = "Fonts & Materials/NotoSansSC-Regular SDF";
}

public static class GameManager
{
    private static int sBagId = GameDefine.DefaultBagId;
    private static bool sIsInitialized;
    private static bool sPlayGameEntranceAnimation;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (sIsInitialized)
        {
            return;
        }

        sBagId = GameDefine.DefaultBagId;
        sPlayGameEntranceAnimation = false;
        sIsInitialized = true;
        Debug.Log("GameManager initialized.");
    }

    public static int GetBagId()
    {
        return sBagId;
    }

    public static void SetBagId(int bagId)
    {
        sBagId = bagId;
    }

    public static void EnterGameScene(int bagId, bool playEntranceAnimation = false)
    {
        SetBagId(bagId);
        sPlayGameEntranceAnimation = playEntranceAnimation;
        SceneManager.LoadScene(GameDefine.SceneGame);
    }

    public static bool ConsumeGameEntranceAnimation()
    {
        var shouldPlay = sPlayGameEntranceAnimation;
        sPlayGameEntranceAnimation = false;
        return shouldPlay;
    }

    public static void EnterRankScene()
    {
        SceneManager.LoadScene(GameDefine.SceneRank);
    }

    public static void EnterAchieveScene()
    {
        SceneManager.LoadScene(GameDefine.SceneAchieve);
    }

    public static void EnterMainScene()
    {
        SceneManager.LoadScene(GameDefine.SceneMain);
    }
}
