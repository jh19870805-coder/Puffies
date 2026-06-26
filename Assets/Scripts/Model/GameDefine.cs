using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class DraggablePieceState
{
    public SpriteRenderer PieceRenderer;
    public Image GrooveImage;
    public RectTransform GrooveRect;
    public Vector3 StartPosition;
    public Vector3 TrayScale;
    public Vector3 DragScale;
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
    public const string SceneEffect = "effect";

    public const string EffectsRoot = "Assets/Resources/Effects";
    public const string CardPackFolder = "CardPack";
    public const string PlaneGroupFolder = "PlaneGroup";
    public const string CardFxFolder = "CardFx";

    public const string PlaneGroupPrefabEditorPath = "Assets/Resources/Effects/PlaneGroup/PlaneGroup_001.prefab";
    public const string PlaneGroupMaterialEditorPath = "Assets/Resources/Effects/PlaneGroup/PlaneGroupLit.mat";
    public const string PlaneGroupPrefabResourcesPath = "Effects/PlaneGroup/PlaneGroup_001";
    public const string PlaneGroupMaterialResourcesPath = "Effects/PlaneGroup/PlaneGroupLit";
    public const string PlaneGroupFbxEditorFolder = "Assets/Resources/Effects/PlaneGroup";

    public const string CardPackPrefabResourcesFolder = "Effects/CardPack/";
    public const string CardPackMaterialResourcesPath = "Effects/CardPack/CardPackLit";
    public const string CardPackPrefabEditorFolder = "Assets/Resources/Effects/CardPack";
    public const string CardPackMaterialEditorPath = "Assets/Resources/Effects/CardPack/CardPackLit.mat";
    public const string CardPackAnimationEditorFolder = "Assets/Resources/Effects/CardPack";
    public const string CardPackAniPrefix = "CardPackAni_";
    public const string CardPackSkinPrefix = "CardPackSkin_";
    public const string CardObtainPrefabName = "CardObtain_001";
    public const string CardTrailPrefabName = "CardTrail_001";

    public const string CardObtainPrefabEditorPath = "Assets/Resources/Effects/CardFx/CardObtain_001.prefab";
    public const string CardTrailPrefabEditorPath = "Assets/Resources/Effects/CardFx/CardTrail_001.prefab";
    public const string CardObtainPrefabResourcesPath = "Effects/CardFx/CardObtain_001";
    public const string CardTrailPrefabResourcesPath = "Effects/CardFx/CardTrail_001";
    public const string CardFxEditorFolder = "Assets/Resources/Effects/CardFx";

    public static string FormatCardPackAnimationFileName(int bagId)
    {
        return $"{CardPackAniPrefix}{bagId:D3}.FBX";
    }

    public static string FormatCardPackSkinPrefabName(int bagId)
    {
        return $"{CardPackSkinPrefix}{bagId:D3}";
    }

    // Common path tokens
    public const string AssetsRoot = "Assets";
    public const string UiRoot = "UI";
    public const string ConfigsRoot = "Configs";
    public const string PackImagesFolder = "PackImages";

    public const string TaskConfigFileName = "TaskConfig.csv";
    public const string TaskConfigEditorPath = "Assets/Resources/Configs/TaskConfig.csv";
    public const string TaskConfigResourcesPath = "Configs/TaskConfig";
    public const string TaskProgressJsonKey = "task_progress";
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
    public const string PieceGroupObjectPrefix = "PieceGroup";
    public const string PieceBoardObjectName = "PieceBoard";
    public const int DefaultFirstPuzzleGroupMaxPieceNumber = 4;
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
    public const int LocalStoreSchemaVersion = 1;
    public const string LocalSqliteCollectionTable = "app_records";

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
