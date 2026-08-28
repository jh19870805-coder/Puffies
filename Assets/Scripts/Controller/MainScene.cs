using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float PackageOpenScaleDuration = 0.3f;
    private const float BagSelectButtonSlideDuration = PackageOpenScaleDuration * 1.3f;
    private const float BagSelectButtonEntranceBottomMargin = 24f;
    private const float PackageOpenWidth = 600f;
    private const float PackageOpenHeight = 680f;
    private const float PackageSlotWidth = 240f;
    private const float PackageSlotHeight = 272f;
    private const float PackageCoverWidth = 240f;
    internal const float PackageCoverHeight = 272f;
    private const float PackageHorizontalSpacing = 20f;
    private const float PackageVerticalSpacing = 20f;
    private const float PackagePageSnapDuration = 0.26f;
    private const float DefaultPackagePageWidth = 1625f;
    private const float DefaultPackagePageHeight = 950f;
    private const int PackagesPerPageRowCount = 3;
    private const int PackagesPerPageColumnCount = 6;
    private const int PackagesPerPage = PackagesPerPageRowCount * PackagesPerPageColumnCount;
    private const int PackageListBuildBatchSize = 4;
    private const int PackTornMaskCount = 6;
    private const int InProgressPackPieceCount = 3;
    internal const float InProgressPackPieceMaxSize = 86f;
    internal const float InProgressPackPieceScaleMultiplier = 1.4f;
    private const float InProgressPackPieceFloatDistance = 6f;
    private const float InProgressPackPieceFloatDuration = 6f;
    private const float InProgressPackPieceHorizontalMargin = 2f;
    private const float NormalPackBreathingSpeed = 1f;
    private const float CompletedPackBreathingSpeed = 1f / 3f;
    private const float PackBreathingPhaseStep = 0.61803398875f;
    private const int BagSelectPanelSortingOrder = 20000;
    private const int SelectedPackageSortingOrder = 30000;
    private const int PhotoPanelSortingOrder = 32000;
    private const int PhotoFlashSortingOrder = 33000;
    private const int PhotoCaptureLayer = 30;
    private const int PhotoOutputSize = 1024;
    private const float PhotoPuzzleRotation = 7f;
    private const float PhotoPuzzleMaxSize = 920f;
    private const float PhotoPuzzleOffsetY = 8f;
    private const float PhotoFlashFadeInDuration = 0.06f;
    private const float PhotoFlashHoldDuration = 0.04f;
    private const float PhotoFlashFadeOutDuration = 0.16f;
    private const float BagSelectPanelWorldDepth = -0.1f;
    private const float OverlayWorldDepth = -0.2f;
    private const float MainCanvasWorldDepth = 0f;
    private const float OpeningStageBackgroundWorldDepth = 0f;
    private const float BagSelectGaussianBlurRadius = 8f;
    private const float GameTransitionDurationScale = 1.5f;
    private const float OpeningStageTransitionDuration = 0.28f
                                                         * GameTransitionDurationScale
                                                         * GameDefine.NonDealTransitionDurationMultiplier;
    private const float TearGestureTravelRatio = 0.06f;
    private const float TearGestureMinTravelPixels = 18f;
    private const float InProgressGameTransitionHoldDuration = 0.17f * GameTransitionDurationScale;
    private const float InProgressGameTransitionPreloadTimeout = 5f;
    private const float InProgressPackExitDuration = 0.46f
                                                     * GameTransitionDurationScale
                                                     * GameDefine.NonDealTransitionDurationMultiplier;
    private const float TornPackTransitionHoldReduction = 0.3f;
    private const float TornPackExitSpeedMultiplier = 2f;
    private const float InProgressPackExitScreenHeightRatio = 0.62f;
    private const float InProgressPieceExitCompensation = 0.62f;
    private const float InProgressPieceExitHorizontalSpread = 90f;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string PackageScrollViewObjectName = "PackageScrollView";
    private const string PackagePageObjectPrefix = "Page_";
    private const string PackageFirstPageObjectName = "Page_1";
    private const string PackItemPrefabEditorPath = "Assets/Prefabs/PackItem.prefab";
    private const string PackItemPrefabResourcesPath = "PackItem";
    private const string PackItemTemplateObjectName = "PackItemTemplate";
    private const string PackNodeObjectName = "PackNode";
    private const string PackCoverObjectName = "PackCover";
    private const string PackBackgroundObjectName = "PackBg";
    private const string PackSizeObjectName = "PackSize";
    private const string PackLightObjectName = "ImgLight";
    private const string OpeningHintAnimationObjectName = "OpeningPackHintAnimation";
    private const string OpeningHintAnimationStateName = "PackAni";
    private const string PackBreathingAnimationStateName = "PackAniBreath";
    private const string InProgressPackPiecesObjectName = "ProgressPieces";
    private const string PackTornMaskFilePrefix = "PackMask";
    private const string PackNameTextObjectName = "NameText";
    private const string MenuButtonObjectName = "BtnMenu";
    private const string WishListButtonObjectName = "BtnWishList";
    private const string WishListUrl =
        "https://store.steampowered.com/app/4906510/?utm_source=InGame";
    private const string DiscordButtonObjectName = "BtnDiscord";
    private const string DiscordUrl = "https://discord.gg/sfmNFEF5ec";
    private const string QqButtonObjectName = "BtnQQ";
    private const string QqGroupUrl =
        "http://qm.qq.com/cgi-bin/qm/qr?_wv=1027"
        + "&k=Ke5OfLu0c2EBkNiyKug4DBbHYMlTTkWW"
        + "&authKey=CXj1XfLtp7Xv4hRHsSAyuXMEHCGPz45KKD4vM%2B7nyRyudAOG45KVzBN%2BS4SJjOZw"
        + "&noverify=0&group_code=1079431440";
    private const string MenuPanelObjectName = "PanelMenu";
    private const string MenuCloseButtonObjectName = "BtnClose";
    private const string SettingsPanelObjectName = "PanelSet";
    private const string SettingsButtonObjectName = "BtnSet";
    private const string MusicSliderObjectName = "SliderMusic";
    private const string EffectSliderObjectName = "SliderEffect";
    private const string WindowedToggleObjectName = "ToggleFrame";
    private const string UsablePanelObjectName = "PanelUsable";
    private const string UsableButtonObjectName = "BtnUsable";
    private const string UsableToggle1ObjectName = "Toggle1";
    private const string UsableToggle2ObjectName = "Toggle2";
    private const string UsableToggle3ObjectName = "Toggle3";
    private const string UsableContentBackgroundObjectName = "ImgContentBg";
    private const string UsableContentLineObjectName = "ImgContentLine";
    private const string UsableHighContrastOffPath = GameDefine.UiRoot + "/MainScene/MainSetHigh1.png";
    private const string UsableHighContrastOnPath = GameDefine.UiRoot + "/MainScene/MainSetHigh2.png";
    private const string UsableLineOffPath = GameDefine.UiRoot + "/MainScene/MainSetLine1.png";
    private const string UsableLevelOutlinePath = GameDefine.UiRoot + "/MainScene/MainSetLine2.png";
    private const string UsableStickerOutlinePath = GameDefine.UiRoot + "/MainScene/MainSetLine3.png";
    private const string SavePanelObjectName = "PanelSave";
    private const string SaveButtonObjectName = "BtnData";
    private const string BagSelectPanelObjectName = "PanelBagSelect";
    private const string BagSelectCanvasObjectName = "PanelBagSelectCanvas";
    private const string BagSelectBackdropObjectName = "PanelBagSelectBlurredBackdrop";
    private const string BagSelectBlurShaderResourcePath = "BagSelectGaussianBlur";
    private const string SelectedPackageCanvasObjectName = "SelectedCardPackCanvas";
    private const string SelectedPackageImageObjectName = "SelectedCardPackImage";
    private const string OpeningStageBackgroundObjectName = "CardPackOpeningStageBackground";
    private const string OpeningStageBackgroundPath = GameDefine.UiRoot + "/BasicUI/BgGame.png";
    private const int OpeningStageBackgroundRenderQueue = 1999;
    private const string BagSelectPlayButtonObjectName = "BtnPlay";
    private const string BagSelectBackButtonObjectName = "BtnBack";
    private const string BagSelectCameraButtonObjectName = "BtnCamera";
    private const string ReplayPanelObjectName = "PanelReplay";
    private const string ReplayConfirmButtonObjectName = "BtnReplay";
    private const string ReplayReturnButtonObjectName = "BtnReturn";
    private const string ReplayCloseButtonObjectName = "BtnClose";
    private const string PhotoPanelObjectName = "PanelPhoto";
    private const string PhotoPanelCanvasObjectName = "PanelPhotoCanvas";
    private const string PhotoImageObjectName = "Photo";
    private const string PhotoGameIconObjectName = "GameIcon";
    private const string PhotoOkButtonObjectName = "BtnOK";
    private const string PhotoFlashCanvasObjectName = "PhotoFlashCanvas";
    private const string BagSelectNewPackActionText = "玩";
    private const string BagSelectReplayActionText = "重玩";
    private const string TaskItemObjectName = "TaskItem";
    private static readonly Dictionary<int, Sprite> sPackageCoverSpriteCache =
        new Dictionary<int, Sprite>();
    private static readonly Dictionary<CardPackSize, Sprite> sPackageSizeSpriteCache =
        new Dictionary<CardPackSize, Sprite>();
    private static bool sPackageListVisualPreloadComplete;
    private static bool sHookedSceneLoaded;
    private static readonly int TornMaskTextureId = Shader.PropertyToID("_TornMaskTex");
    private static readonly int UseTornMaskId = Shader.PropertyToID("_UseTornMask");

    [SerializeField] private GameObject mPackageItemPrefab;

    private readonly Dictionary<int, PackageEntry> mPackageSlotsById = new Dictionary<int, PackageEntry>();
    private readonly Sprite[] mPackTornMaskSprites = new Sprite[PackTornMaskCount];
    private readonly Material[] mPackTornMaskMaterials = new Material[PackTornMaskCount];
    private readonly Material[] mPackCompletedTornMaskMaterials = new Material[PackTornMaskCount];
    private readonly bool[] mPackTornMaskLoadAttempted = new bool[PackTornMaskCount];
    private readonly System.Random mPackTornMaskRandom = new System.Random();
    private GameObject mPackageItemTemplate;
    private RectTransform mPackageContentRoot;
    private RectTransform mPackagePageTemplate;
    private ScrollRect mPackageScrollRect;
    private Coroutine mPackagePageSnapCoroutine;
    private GameObject mMenuPanelRoot;
    private GameObject mSettingsPanelRoot;
    private GameObject mUsablePanelRoot;
    private GameObject mSavePanelRoot;
    private GameObject mBagSelectPanelRoot;
    private Canvas mBagSelectOverlayCanvas;
    private Canvas mSelectedPackageOverlayCanvas;
    private CanvasGroup mSelectedPackageOverlayCanvasGroup;
    private Image mSelectedPackageOverlayImage;
    private RectTransform mSelectedPackageOverlayRect;
    private RectTransform mSelectedPackageVisualContent;
    private List<InProgressPackagePieceAnimation> mSelectedPackageProgressPieceAnimations;
    private GameObject mOpeningHintAnimationRoot;
    private CardPackOpeningEffect mCardPackOpeningEffect;
    private CanvasGroup mMainCanvasGroup;
    private CanvasGroup mBagSelectPanelCanvasGroup;
    private RawImage mBagSelectBackdropImage;
    private GameObject mOpeningStageBackgroundRoot;
    private SpriteRenderer mOpeningStageBackgroundRenderer;
    private Material mOpeningStageBackgroundMaterial;
    private Sprite mOpeningStageBackgroundSprite;
    private RenderTexture mBagSelectBackdropTexture;
    private Material mBagSelectBlurMaterial;
    private FakeSettingsSliderInput mMusicSlider;
    private FakeSettingsSliderInput mEffectSlider;
    private Toggle mWindowedToggle;
    private Toggle mUsableToggle1;
    private Toggle mUsableToggle2;
    private Toggle mUsableToggle3;
    private Image mUsableContentBackgroundImage;
    private Image mUsableContentLineImage;
    private Sprite mUsableHighContrastOffSprite;
    private Sprite mUsableHighContrastOnSprite;
    private Sprite mUsableLineOffSprite;
    private Sprite mUsableLevelOutlineSprite;
    private Sprite mUsableStickerOutlineSprite;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private bool mIsApplyingSettingsToUi;
    private Coroutine mPlayAnimationCoroutine;
    private PackageEntry mSelectedPackageEntry;
    private Button mBagSelectPlayButton;
    private Button mBagSelectBackButton;
    private GameObject mBagSelectCameraButtonRoot;
    private Button mBagSelectCameraButton;
    private TMP_Text mBagSelectPlayLabel;
    private RectTransform[] mBagSelectButtonRects = Array.Empty<RectTransform>();
    private Vector2[] mBagSelectButtonPositions = Array.Empty<Vector2>();
    private GameObject mReplayPanelRoot;
    private Button mReplayConfirmButton;
    private Button mReplayReturnButton;
    private Button mReplayCloseButton;
    private GameObject mPhotoPanelRoot;
    private Canvas mPhotoPanelCanvas;
    private Image mPhotoImage;
    private GameObject mPhotoGameIconRoot;
    private Sprite mPhotoBackgroundSprite;
    private Sprite mPhotoGameIconSprite;
    private Button mPhotoOkButton;
    private Canvas mPhotoFlashCanvas;
    private CanvasGroup mPhotoFlashCanvasGroup;
    private Texture2D mGeneratedPhotoTexture;
    private Sprite mGeneratedPhotoSprite;
    private bool mIsCapturingPhoto;
    private bool mIsReplayConfirmationVisible;
    private bool mIsSelectedPackageReplay;
    private int mSelectedBagId;
    private Vector2 mSelectedPackageStartPosition;
    private Vector2 mSelectedPackageStartSize;
    private Vector2 mSelectedPackageDisplayPosition;
    private Vector2 mSelectedPackageDisplaySize;
    private Vector2 mSelectedPackageStageSize;
    private bool mIsAwaitingTearSwipe;
    private bool mIsTrackingTearSwipe;
    private bool mIsTrackingTearTap;
    private bool mIsSelectedPackageProgressPieceTransitioning;
    private bool mDidWarnPackTornMaskUnavailable;
    private Vector2 mTearSwipeStartScreenPosition;
    private Rect mTearSwipeScreenRect;

    private enum PackageDisplayState
    {
        IntactColor,
        TornColorInProgress,
        TornCompleted
    }

    private sealed class PackageEntry
    {
        public int BagId;
        public GameObject Root;
        public Image Image;
        public Image BackgroundImage;
        public Image SizeImage;
        public PackCoverVisualSettings VisualSettings;
        public Animator PackAnimator;
        public GameObject ProgressPiecesRoot;
        public List<InProgressPackagePieceAnimation> ProgressPieceAnimations;
        public RectTransform RectTransform;
        public bool SuppressDisplay;
        public bool ShowTornBackground;
        public PackageDisplayState DisplayState;
    }

    private sealed class InProgressPackagePieceAnimation
    {
        public RectTransform RectTransform;
        public Vector2 BasePosition;
        public float FloatDistance;
        public float PhaseRadians;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<MainScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneMain,
            BootstrapObjectName);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPackageVisualCache()
    {
        sPackageCoverSpriteCache.Clear();
        sPackageSizeSpriteCache.Clear();
        sPackageListVisualPreloadComplete = false;
    }

    public static bool ArePackageListVisualsPreloaded => sPackageListVisualPreloadComplete;

    public static IEnumerator PreloadPackageListVisuals()
    {
        sPackageListVisualPreloadComplete = false;
        if (!CardPackDataUtility.Initialize())
        {
            sPackageListVisualPreloadComplete = true;
            yield break;
        }

        var records = CardPackDataUtility.GetAllPacks();
        var preloadedSizes = new HashSet<CardPackSize>();
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.LifecycleState == CardPackLifecycleState.Locked)
            {
                continue;
            }

            yield return PreloadPackageCoverSprite(record.PackId);
            if (GameConfigRepository.TryGetCardPackConfig(record.PackId, out var config)
                && config.PackSize >= CardPackSize.XS
                && config.PackSize <= CardPackSize.XXXL)
            {
                preloadedSizes.Add(config.PackSize);
            }

        }

        foreach (var packSize in preloadedSizes)
        {
            yield return PreloadPackageSizeSprite(packSize);
        }

        sPackageListVisualPreloadComplete = true;
    }

    private static IEnumerator PreloadPackageCoverSprite(int packId)
    {
        if (sPackageCoverSpriteCache.TryGetValue(packId, out var cachedSprite)
            && cachedSprite != null)
        {
            yield break;
        }

        Sprite loadedSprite = null;
        yield return LoadSpriteByPathAsync(
            GameDefine.FormatPackImagePath(packId),
            sprite => loadedSprite = sprite);
        if (loadedSprite != null)
        {
            sPackageCoverSpriteCache[packId] = loadedSprite;
        }
    }

    private static IEnumerator PreloadPackageSizeSprite(CardPackSize packSize)
    {
        if (sPackageSizeSpriteCache.TryGetValue(packSize, out var cachedSprite)
            && cachedSprite != null)
        {
            yield break;
        }

        Sprite loadedSprite = null;
        yield return LoadSpriteByPathAsync(
            GameDefine.FormatPackSizeImagePath(packSize),
            sprite => loadedSprite = sprite);
        if (loadedSprite != null)
        {
            sPackageSizeSpriteCache[packSize] = loadedSprite;
        }
    }

    private static IEnumerator LoadSpriteByPathAsync(
        string imageResourcePath,
        Action<Sprite> onCompleted)
    {
        var imagePathOnDisk = GameCommonUtility.ToDiskPath(imageResourcePath);
        if (!File.Exists(imagePathOnDisk))
        {
            onCompleted?.Invoke(null);
            yield break;
        }

        using (var request = UnityWebRequestTexture.GetTexture(
                   new Uri(imagePathOnDisk).AbsoluteUri,
                   nonReadable: true))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    $"MainScene: async package image preload failed. "
                    + $"path={imagePathOnDisk}, error={request.error}");
                onCompleted?.Invoke(null);
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture == null)
            {
                onCompleted?.Invoke(null);
                yield break;
            }

            texture.name = Path.GetFileNameWithoutExtension(imagePathOnDisk);
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            sprite.name = texture.name;
            onCompleted?.Invoke(sprite);
        }
    }

    private static Sprite GetOrLoadPackageCoverSprite(int packId)
    {
        if (sPackageCoverSpriteCache.TryGetValue(packId, out var sprite)
            && sprite != null)
        {
            return sprite;
        }

        sprite = GameCommonUtility.LoadSpriteByPath(
            GameDefine.FormatPackImagePath(packId),
            PixelsPerUnit);
        if (sprite != null)
        {
            sPackageCoverSpriteCache[packId] = sprite;
        }

        return sprite;
    }

    private static Sprite GetOrLoadPackageSizeSprite(CardPackSize packSize)
    {
        if (sPackageSizeSpriteCache.TryGetValue(packSize, out var sprite)
            && sprite != null)
        {
            return sprite;
        }

        sprite = GameCommonUtility.LoadSpriteByPath(
            GameDefine.FormatPackSizeImagePath(packSize),
            PixelsPerUnit);
        if (sprite != null)
        {
            sPackageSizeSpriteCache[packSize] = sprite;
        }

        return sprite;
    }

    private void OnDestroy()
    {
        StopPackagePageSnap();
        StopOpeningHintAnimation();
        if (mSelectedPackageOverlayCanvas != null)
        {
            Destroy(mSelectedPackageOverlayCanvas.gameObject);
        }

        if (mOpeningStageBackgroundRoot != null)
        {
            Destroy(mOpeningStageBackgroundRoot);
        }

        if (mOpeningStageBackgroundMaterial != null)
        {
            Destroy(mOpeningStageBackgroundMaterial);
        }

        ReleaseBagSelectBackdropTexture();
        if (mBagSelectBlurMaterial != null)
        {
            Destroy(mBagSelectBlurMaterial);
            mBagSelectBlurMaterial = null;
        }

        ReleaseGeneratedPhoto();
        ReleaseUsablePanelPreviewSprites();
        ReleasePackTornMaskResources();
        if (mOpeningStageBackgroundSprite != null)
        {
            var texture = mOpeningStageBackgroundSprite.texture;
            Destroy(mOpeningStageBackgroundSprite);
            if (texture != null)
            {
                Destroy(texture);
            }
        }

    }

    private void LateUpdate()
    {
        UpdatePackageDisplays();
    }

    private void Update()
    {
        UpdateInProgressPackagePieceAnimations();

        if (!mIsAwaitingTearSwipe)
        {
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            OnTearSwipeBegin,
            OnTearSwipeMove,
            OnTearSwipeEnd);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneMain))
        {
            Destroy(gameObject);
            return;
        }

        CardPackGameEntranceTransition.CancelPending();

        mHasSwitchedToGameScene = false;
        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
        mSelectedPackageEntry = null;
        mSelectedBagId = 0;
        mIsAwaitingTearSwipe = false;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        mIsCapturingPhoto = false;
        mIsReplayConfirmationVisible = false;

        GameManager.Initialize();
        if (!GameSettingsUtility.Initialize())
        {
            Debug.LogWarning("MainScene: GameSettingsUtility is not ready; settings will use defaults until SQLite is available.");
        }

        ConfigureMainCanvas();
        CardPackOpeningEffect.PrepareSceneLightEffect();

        if (!TryResolvePackageList())
        {
            Debug.LogWarning("MainScene: package list not found. Expected PackageScrollView/Page_1 with PackItem prefab.");
        }
        else
        {
            StartCoroutine(RefreshPackageList());
        }

        ConfigureRankButton();
        ConfigureAchieveButton();
        ConfigureWishListButton();
        ConfigureDiscordButton();
        ConfigureQqButton();
        ConfigureBagSelectPanel();
        ConfigureReplayPanel();
        ConfigurePhotoPanel();
        ConfigureMenuPanel();
        ConfigureSettingsPanel();
        ConfigureUsablePanel();
        ConfigureSavePanel();
        RefreshTaskProgressUI();
    }

    private static void ConfigureMainCanvas()
    {
        var camera = Camera.main;
        var canvasObject = GameCommonUtility.FindSceneObject("Canvas");
        var canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : null;
        if (camera == null || canvas == null)
        {
            Debug.LogWarning("MainScene: main Canvas could not be bound to Main Camera.");
            return;
        }

        GameCommonUtility.ConfigureCanvasForGameplay(
            canvas,
            camera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            MainCanvasWorldDepth);
        Canvas.ForceUpdateCanvases();
    }

    private static void RefreshTaskProgressUI()
    {
        var taskItemObject = GameCommonUtility.FindSceneObject(TaskItemObjectName);
        if (taskItemObject == null)
        {
            Debug.LogWarning($"MainScene: task UI not found. Expected object named {TaskItemObjectName}.");
            return;
        }

        if (!GameTaskUtility.Initialize()
            || !GameTaskUtility.TryGetCurrentTask(out var task))
        {
            taskItemObject.SetActive(false);
            return;
        }

        TaskProgressUIUtility.RefreshTask(
            taskItemObject.transform,
            task,
            GameTaskUtility.GetCurrentCompleteValue());
    }

    public bool CanAcceptPackageInput()
    {
        return !mHasSwitchedToGameScene
            && !mIsPlayingAnimation
            && mPackagePageSnapCoroutine == null
            && mSelectedPackageEntry == null;
    }

    public void HandlePackageListBeginDrag()
    {
        StopPackagePageSnap();
        mPackageScrollRect?.StopMovement();
    }

    public void HandlePackageListEndDrag()
    {
        if (mPackageScrollRect == null || !isActiveAndEnabled)
        {
            return;
        }

        StopPackagePageSnap();
        mPackagePageSnapCoroutine = StartCoroutine(SnapPackageListToNearestPage());
    }

    public bool TryGetPackageFlyTarget(int bagId, out RectTransform target)
    {
        target = null;
        Canvas.ForceUpdateCanvases();
        if (!mPackageSlotsById.TryGetValue(bagId, out var entry) || entry.Image == null)
        {
            return false;
        }

        target = entry.Image.rectTransform;
        return target != null && target.gameObject.activeInHierarchy;
    }

    public void RevealPackageFlyTarget(int bagId)
    {
        if (mPackageSlotsById.TryGetValue(bagId, out var entry))
        {
            SetPackageVisualsVisible(entry, true);
        }
    }

    public void HandlePackageGesture(int bagId, Image image)
    {
        if (!CanAcceptPackageInput() || image == null)
        {
            return;
        }

        var resolvedBagId = bagId > 0 ? bagId : MainPackageBagId;
        if (!CardPackDataUtility.IsPackUnlocked(resolvedBagId))
        {
            return;
        }

        if (mPlayAnimationCoroutine != null)
        {
            StopCoroutine(mPlayAnimationCoroutine);
        }

        if (!mPackageSlotsById.TryGetValue(resolvedBagId, out var entry))
        {
            entry = new PackageEntry
            {
                BagId = resolvedBagId,
                Root = image.gameObject,
                Image = image,
                RectTransform = image.rectTransform
            };
        }

        mPlayAnimationCoroutine = StartCoroutine(ShowPackageSelection(resolvedBagId, entry));
    }

    private void ConfigureBagSelectPanel()
    {
        mBagSelectPanelRoot = GameCommonUtility.FindSceneObject(BagSelectPanelObjectName);
        if (mBagSelectPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: bag selection panel not found. Expected {BagSelectPanelObjectName}.");
            return;
        }

        ConfigureBagSelectOverlayCanvas();
        var panelImage = mBagSelectPanelRoot.GetComponent<Image>();
        if (panelImage != null)
        {
            var panelColor = panelImage.color;
            panelColor.a = 0f;
            panelImage.color = panelColor;
        }

        mBagSelectPlayButton = FindChild(
            mBagSelectPanelRoot.transform,
            BagSelectPlayButtonObjectName)?.GetComponent<Button>();
        if (mBagSelectPlayButton == null)
        {
            Debug.LogWarning(
                $"MainScene: bag selection play button not found. Expected {BagSelectPlayButtonObjectName}.");
        }
        else
        {
            mBagSelectPlayLabel = mBagSelectPlayButton.GetComponentInChildren<TMP_Text>(true);
            if (mBagSelectPlayLabel == null)
            {
                Debug.LogWarning("MainScene: bag selection play button label not found.");
            }

            mBagSelectPlayButton.onClick.RemoveListener(OnBagSelectPlayClicked);
            mBagSelectPlayButton.onClick.AddListener(OnBagSelectPlayClicked);
        }

        var cameraTransform = FindChild(
            mBagSelectPanelRoot.transform,
            BagSelectCameraButtonObjectName);
        mBagSelectCameraButtonRoot = cameraTransform != null
            ? cameraTransform.gameObject
            : null;
        if (mBagSelectCameraButtonRoot == null)
        {
            Debug.LogWarning(
                $"MainScene: bag selection camera button not found. Expected {BagSelectCameraButtonObjectName}.");
        }
        else
        {
            mBagSelectCameraButton = mBagSelectCameraButtonRoot.GetComponent<Button>();
            if (mBagSelectCameraButton == null)
            {
                mBagSelectCameraButton = mBagSelectCameraButtonRoot.AddComponent<Button>();
                mBagSelectCameraButton.targetGraphic =
                    mBagSelectCameraButtonRoot.GetComponent<Graphic>();
            }

            mBagSelectCameraButton.onClick.RemoveListener(OnBagSelectCameraClicked);
            mBagSelectCameraButton.onClick.AddListener(OnBagSelectCameraClicked);
            mBagSelectCameraButtonRoot.SetActive(false);
        }

        var backTransform = FindChild(mBagSelectPanelRoot.transform, BagSelectBackButtonObjectName);
        if (backTransform != null)
        {
            mBagSelectBackButton = backTransform.GetComponent<Button>();
            if (mBagSelectBackButton == null)
            {
                mBagSelectBackButton = backTransform.gameObject.AddComponent<Button>();
                mBagSelectBackButton.targetGraphic = backTransform.GetComponent<Graphic>();
            }

            mBagSelectBackButton.onClick.RemoveListener(OnBagSelectBackClicked);
            mBagSelectBackButton.onClick.AddListener(OnBagSelectBackClicked);
        }
        else
        {
            Debug.LogWarning(
                $"MainScene: bag selection back button not found. Expected {BagSelectBackButtonObjectName}.");
        }

        CacheBagSelectButtonPositions();
        SetBagSelectPanelVisible(false);
    }

    private void ConfigureReplayPanel()
    {
        mReplayPanelRoot = GameCommonUtility.FindSceneObject(ReplayPanelObjectName);
        if (mReplayPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: replay panel not found. Expected {ReplayPanelObjectName}.");
            return;
        }

        if (mBagSelectOverlayCanvas != null)
        {
            StretchToParent(
                mReplayPanelRoot.GetComponent<RectTransform>(),
                mBagSelectOverlayCanvas.transform);
        }
        else
        {
            Debug.LogWarning("MainScene: replay panel could not be moved to the bag selection canvas.");
        }

        mReplayConfirmButton = FindChild(
            mReplayPanelRoot.transform,
            ReplayConfirmButtonObjectName)?.GetComponent<Button>();
        mReplayReturnButton = FindChild(
            mReplayPanelRoot.transform,
            ReplayReturnButtonObjectName)?.GetComponent<Button>();
        mReplayCloseButton = FindChild(
            mReplayPanelRoot.transform,
            ReplayCloseButtonObjectName)?.GetComponent<Button>();

        BindReplayButton(mReplayConfirmButton, OnReplayConfirmed, ReplayConfirmButtonObjectName);
        BindReplayButton(mReplayReturnButton, OnReplayCancelled, ReplayReturnButtonObjectName);
        BindReplayButton(mReplayCloseButton, OnReplayCancelled, ReplayCloseButtonObjectName);
        SetPanelVisible(mReplayPanelRoot, false);
    }

    private static void BindReplayButton(Button button, UnityEngine.Events.UnityAction action, string objectName)
    {
        if (button == null)
        {
            Debug.LogWarning($"MainScene: replay panel button not found. Expected {objectName}.");
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void ConfigurePhotoPanel()
    {
        mPhotoPanelRoot = GameCommonUtility.FindSceneObject(PhotoPanelObjectName);
        if (mPhotoPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: photo panel not found. Expected {PhotoPanelObjectName}.");
            return;
        }

        var camera = Camera.main;
        var sourceCanvas = mPhotoPanelRoot.GetComponentInParent<Canvas>();
        if (camera == null || sourceCanvas == null)
        {
            Debug.LogWarning("MainScene: photo panel canvas could not be configured.");
            return;
        }

        var canvasObject = new GameObject(
            PhotoPanelCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.layer = mPhotoPanelRoot.layer;
        mPhotoPanelCanvas = canvasObject.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mPhotoPanelCanvas,
            camera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            OverlayWorldDepth - 0.03f);
        mPhotoPanelCanvas.sortingLayerID = mBagSelectOverlayCanvas != null
            ? mBagSelectOverlayCanvas.sortingLayerID
            : sourceCanvas.sortingLayerID;
        mPhotoPanelCanvas.sortingOrder = PhotoPanelSortingOrder;
        StretchToParent(
            mPhotoPanelRoot.GetComponent<RectTransform>(),
            mPhotoPanelCanvas.transform);

        mPhotoImage = FindChild(mPhotoPanelRoot.transform, PhotoImageObjectName)?.GetComponent<Image>();
        if (mPhotoImage == null)
        {
            Debug.LogWarning($"MainScene: photo image not found. Expected {PhotoImageObjectName}.");
        }
        else
        {
            mPhotoBackgroundSprite = mPhotoImage.sprite;
        }

        var gameIconTransform = FindChild(mPhotoPanelRoot.transform, PhotoGameIconObjectName);
        mPhotoGameIconRoot = gameIconTransform != null ? gameIconTransform.gameObject : null;
        var gameIconImage = gameIconTransform != null ? gameIconTransform.GetComponent<Image>() : null;
        mPhotoGameIconSprite = gameIconImage != null ? gameIconImage.sprite : null;

        var okTransform = FindChild(mPhotoPanelRoot.transform, PhotoOkButtonObjectName);
        mPhotoOkButton = okTransform != null ? okTransform.GetComponent<Button>() : null;
        if (mPhotoOkButton == null)
        {
            Debug.LogWarning($"MainScene: photo OK button not found. Expected {PhotoOkButtonObjectName}.");
        }
        else
        {
            mPhotoOkButton.onClick.RemoveListener(OnPhotoOkClicked);
            mPhotoOkButton.onClick.AddListener(OnPhotoOkClicked);
        }

        CreatePhotoFlashCanvas();
        SetPanelVisible(mPhotoPanelRoot, false);
    }

    private void CreatePhotoFlashCanvas()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("MainScene: photo flash canvas could not be configured without a camera.");
            return;
        }

        var canvasObject = new GameObject(
            PhotoFlashCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup));
        canvasObject.layer = mBagSelectPanelRoot != null ? mBagSelectPanelRoot.layer : 5;
        mPhotoFlashCanvas = canvasObject.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mPhotoFlashCanvas,
            camera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            OverlayWorldDepth - 0.02f);
        mPhotoFlashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mPhotoFlashCanvas.worldCamera = null;
        mPhotoFlashCanvas.sortingLayerID = mBagSelectOverlayCanvas != null
            ? mBagSelectOverlayCanvas.sortingLayerID
            : 0;
        mPhotoFlashCanvas.sortingOrder = PhotoFlashSortingOrder;
        mPhotoFlashCanvasGroup = canvasObject.GetComponent<CanvasGroup>();
        mPhotoFlashCanvasGroup.alpha = 0f;

        var flashObject = new GameObject(
            "Flash",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        flashObject.layer = canvasObject.layer;
        StretchToParent(flashObject.GetComponent<RectTransform>(), canvasObject.transform);
        var flashImage = flashObject.GetComponent<Image>();
        flashImage.color = Color.white;
        flashImage.raycastTarget = true;
        canvasObject.SetActive(false);
    }

    private void ConfigureBagSelectOverlayCanvas()
    {
        var camera = Camera.main;
        var sourceCanvas = mBagSelectPanelRoot.GetComponentInParent<Canvas>();
        if (camera == null || sourceCanvas == null)
        {
            Debug.LogWarning("MainScene: bag selection overlay canvas could not be configured.");
            return;
        }

        mMainCanvasGroup = sourceCanvas.GetComponent<CanvasGroup>();
        if (mMainCanvasGroup == null)
        {
            mMainCanvasGroup = sourceCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        mMainCanvasGroup.alpha = 1f;
        mMainCanvasGroup.interactable = true;
        mMainCanvasGroup.blocksRaycasts = true;

        var canvasObject = new GameObject(
            BagSelectCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.layer = mBagSelectPanelRoot.layer;
        mBagSelectOverlayCanvas = canvasObject.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mBagSelectOverlayCanvas,
            camera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            BagSelectPanelWorldDepth);
        mBagSelectOverlayCanvas.sortingLayerID = sourceCanvas.sortingLayerID;
        mBagSelectOverlayCanvas.overrideSorting = true;
        mBagSelectOverlayCanvas.sortingOrder = BagSelectPanelSortingOrder;

        CreateOpeningStageBackground();
        CreateBagSelectBackdrop();
        mBagSelectPanelRoot.transform.SetParent(mBagSelectOverlayCanvas.transform, false);
        mBagSelectPanelRoot.transform.SetAsLastSibling();
        mBagSelectPanelCanvasGroup = mBagSelectPanelRoot.GetComponent<CanvasGroup>();
        if (mBagSelectPanelCanvasGroup == null)
        {
            mBagSelectPanelCanvasGroup = mBagSelectPanelRoot.AddComponent<CanvasGroup>();
        }

        CreateSelectedPackageOverlayCanvas(sourceCanvas);
    }

    private void SetSelectedPackageImageVisible(bool visible)
    {
        if (mSelectedPackageOverlayCanvas != null)
        {
            mSelectedPackageOverlayCanvas.gameObject.SetActive(visible);
        }
    }

    private void SetSelectedPackageImageAlpha(float alpha)
    {
        if (mSelectedPackageOverlayCanvasGroup == null)
        {
            return;
        }

        mSelectedPackageOverlayCanvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    private void CreateSelectedPackageOverlayCanvas(Canvas sourceCanvas)
    {
        if (mSelectedPackageOverlayCanvas != null || sourceCanvas == null)
        {
            return;
        }

        var canvasObject = new GameObject(
            SelectedPackageCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.layer = mBagSelectPanelRoot != null ? mBagSelectPanelRoot.layer : 5;
        mSelectedPackageOverlayCanvas = canvasObject.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mSelectedPackageOverlayCanvas,
            Camera.main,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            OverlayWorldDepth);
        mSelectedPackageOverlayCanvas.sortingLayerID = sourceCanvas.sortingLayerID;
        mSelectedPackageOverlayCanvas.overrideSorting = true;
        mSelectedPackageOverlayCanvas.sortingOrder = SelectedPackageSortingOrder;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(GameDefine.DesignWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = PixelsPerUnit;

        var imageObject = new GameObject(
            SelectedPackageImageObjectName,
            typeof(RectTransform),
            typeof(CanvasGroup));
        imageObject.layer = canvasObject.layer;
        mSelectedPackageOverlayRect = imageObject.GetComponent<RectTransform>();
        mSelectedPackageOverlayRect.SetParent(canvasObject.transform, false);
        mSelectedPackageOverlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.pivot = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.anchoredPosition = Vector2.zero;
        mSelectedPackageOverlayRect.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);
        mSelectedPackageOverlayCanvasGroup = imageObject.GetComponent<CanvasGroup>();
        mSelectedPackageOverlayCanvasGroup.alpha = 1f;
        mSelectedPackageOverlayCanvasGroup.interactable = false;
        mSelectedPackageOverlayCanvasGroup.blocksRaycasts = false;
        canvasObject.SetActive(false);
    }

    private bool CreateSelectedPackageVisual(PackageEntry entry)
    {
        ClearSelectedPackageVisual();
        if (entry?.Root == null || mSelectedPackageOverlayRect == null)
        {
            return false;
        }

        var sourcePackNode = FindChild(entry.Root.transform, PackNodeObjectName) as RectTransform;
        if (sourcePackNode == null)
        {
            return false;
        }

        var visualObject = Instantiate(
            sourcePackNode.gameObject,
            mSelectedPackageOverlayRect,
            false);
        visualObject.name = "SelectedPackNode";
        SetLayerRecursively(visualObject.transform, mSelectedPackageOverlayRect.gameObject.layer);
        mSelectedPackageVisualContent = visualObject.GetComponent<RectTransform>();
        if (mSelectedPackageVisualContent == null)
        {
            Destroy(visualObject);
            return false;
        }

        mSelectedPackageVisualContent.anchorMin = new Vector2(0.5f, 0.5f);
        mSelectedPackageVisualContent.anchorMax = new Vector2(0.5f, 0.5f);
        mSelectedPackageVisualContent.pivot = new Vector2(0.5f, 0.5f);
        mSelectedPackageVisualContent.anchoredPosition = Vector2.zero;
        mSelectedPackageVisualContent.localScale = Vector3.one;

        mSelectedPackageOverlayImage =
            FindChild(visualObject.transform, PackCoverObjectName)?.GetComponent<Image>();
        if (mSelectedPackageOverlayImage == null || mSelectedPackageOverlayImage.sprite == null)
        {
            ClearSelectedPackageVisual();
            return false;
        }

        var graphics = visualObject.GetComponentsInChildren<Graphic>(true);
        for (var i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }

        BindSelectedPackageProgressPieceAnimations(entry, visualObject.transform);

        SetSelectedPackageImageAlpha(1f);
        return true;
    }

    private void SyncSelectedPackageAnimator(PackageEntry entry)
    {
        var sourceAnimator = entry?.PackAnimator;
        var visualAnimator = mSelectedPackageVisualContent != null
            ? mSelectedPackageVisualContent.GetComponent<Animator>()
            : null;
        if (sourceAnimator == null
            || visualAnimator == null
            || !visualAnimator.isActiveAndEnabled
            || sourceAnimator.runtimeAnimatorController != visualAnimator.runtimeAnimatorController
            || sourceAnimator.layerCount <= 0)
        {
            return;
        }

        var sourceState = sourceAnimator.GetCurrentAnimatorStateInfo(0);
        visualAnimator.speed = sourceAnimator.speed;
        visualAnimator.Play(sourceState.fullPathHash, 0, sourceState.normalizedTime);
        visualAnimator.Update(0f);
    }

    private void ClearSelectedPackageVisual()
    {
        if (mSelectedPackageVisualContent != null)
        {
            Destroy(mSelectedPackageVisualContent.gameObject);
        }

        mSelectedPackageVisualContent = null;
        mSelectedPackageOverlayImage = null;
        mSelectedPackageProgressPieceAnimations = null;
        mIsSelectedPackageProgressPieceTransitioning = false;
        SetSelectedPackageImageAlpha(1f);
    }

    private void BindSelectedPackageProgressPieceAnimations(
        PackageEntry entry,
        Transform selectedVisualRoot)
    {
        var sourceAnimations = entry?.ProgressPieceAnimations;
        if (sourceAnimations == null || selectedVisualRoot == null)
        {
            return;
        }

        mSelectedPackageProgressPieceAnimations =
            new List<InProgressPackagePieceAnimation>(sourceAnimations.Count);
        for (var i = 0; i < sourceAnimations.Count; i++)
        {
            var sourceAnimation = sourceAnimations[i];
            if (sourceAnimation?.RectTransform == null)
            {
                continue;
            }

            var selectedPiece = FindChild(
                selectedVisualRoot,
                sourceAnimation.RectTransform.gameObject.name) as RectTransform;
            if (selectedPiece == null)
            {
                continue;
            }

            mSelectedPackageProgressPieceAnimations.Add(
                new InProgressPackagePieceAnimation
                {
                    RectTransform = selectedPiece,
                    BasePosition = sourceAnimation.BasePosition,
                    FloatDistance = sourceAnimation.FloatDistance,
                    PhaseRadians = sourceAnimation.PhaseRadians
                });
        }
    }

    private void SetSelectedPackageVisualSize(Vector2 displaySize)
    {
        if (mSelectedPackageOverlayRect == null)
        {
            return;
        }

        mSelectedPackageOverlayRect.sizeDelta = displaySize;
        if (mSelectedPackageVisualContent == null || mSelectedPackageOverlayImage == null)
        {
            return;
        }

        var sourceSize = mSelectedPackageOverlayImage.rectTransform.rect.size;
        if (sourceSize.x <= 0f || sourceSize.y <= 0f)
        {
            return;
        }

        var scale = Mathf.Min(
            displaySize.x / sourceSize.x,
            displaySize.y / sourceSize.y);
        mSelectedPackageVisualContent.localScale = Vector3.one * scale;
    }

    private bool TryGetSelectedOverlayRect(
        RectTransform source,
        out Vector2 anchoredPosition,
        out Vector2 size)
    {
        anchoredPosition = Vector2.zero;
        size = Vector2.zero;
        var overlayRect = mSelectedPackageOverlayCanvas != null
            ? mSelectedPackageOverlayCanvas.transform as RectTransform
            : null;
        if (source == null || overlayRect == null)
        {
            return false;
        }

        var sourceCanvas = source.GetComponentInParent<Canvas>();
        var sourceCamera = sourceCanvas != null
            && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;
        var overlayCamera = mSelectedPackageOverlayCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? mSelectedPackageOverlayCanvas.worldCamera
            : null;
        var corners = new Vector3[4];
        source.GetWorldCorners(corners);
        var minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (var i = 0; i < corners.Length; i++)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[i]);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    overlayRect,
                    screenPoint,
                    overlayCamera,
                    out var localPoint))
            {
                return false;
            }

            minimum = Vector2.Min(minimum, localPoint);
            maximum = Vector2.Max(maximum, localPoint);
        }

        size = maximum - minimum;
        anchoredPosition = (minimum + maximum) * 0.5f;
        return size.x > 0.001f && size.y > 0.001f;
    }

    private IEnumerator AnimateSelectedPackageImage(
        Vector2 fromPosition,
        Vector2 toPosition,
        Vector2 fromSize,
        Vector2 toSize,
        float duration = PackageOpenScaleDuration)
    {
        if (mSelectedPackageOverlayRect == null)
        {
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            mSelectedPackageOverlayRect.anchoredPosition = Vector2.LerpUnclamped(
                fromPosition,
                toPosition,
                eased);
            SetSelectedPackageVisualSize(Vector2.LerpUnclamped(
                fromSize,
                toSize,
                eased));

            yield return null;
        }

        mSelectedPackageOverlayRect.anchoredPosition = toPosition;
        SetSelectedPackageVisualSize(toSize);
    }

    private IEnumerator AnimateBagSelectButtons(bool entering)
    {
        SetBagSelectButtonEntranceProgress(entering ? 0f : 1f);
        var elapsed = 0f;
        while (elapsed < BagSelectButtonSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / BagSelectButtonSlideDuration);
            var progress = entering
                ? 1f - Mathf.Pow(1f - normalized, 3f)
                : 1f - Mathf.Pow(normalized, 3f);
            SetBagSelectButtonEntranceProgress(progress);
            yield return null;
        }

        SetBagSelectButtonEntranceProgress(entering ? 1f : 0f);
    }

    private void CreateOpeningStageBackground()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("MainScene: opening stage background could not be created without a camera.");
            return;
        }

        mOpeningStageBackgroundRoot = new GameObject(
            OpeningStageBackgroundObjectName,
            typeof(SpriteRenderer));
        mOpeningStageBackgroundRenderer =
            mOpeningStageBackgroundRoot.GetComponent<SpriteRenderer>();
        mOpeningStageBackgroundSprite = GameCommonUtility.LoadSpriteByPath(
            OpeningStageBackgroundPath,
            PixelsPerUnit);
        var fallbackBackground = GameCommonUtility.FindSceneObject(
            GameDefine.BackgroundObjectName)?.GetComponent<Image>();
        mOpeningStageBackgroundRenderer.sprite = mOpeningStageBackgroundSprite != null
            ? mOpeningStageBackgroundSprite
            : fallbackBackground != null ? fallbackBackground.sprite : null;
        var backgroundShader = Shader.Find("Sprites/Default");
        if (backgroundShader != null)
        {
            mOpeningStageBackgroundMaterial = new Material(backgroundShader)
            {
                name = "CardPackOpeningStageBackground (Runtime)",
                renderQueue = OpeningStageBackgroundRenderQueue
            };
            mOpeningStageBackgroundRenderer.sharedMaterial = mOpeningStageBackgroundMaterial;
        }
        mOpeningStageBackgroundRenderer.color = Color.white;
        FitOpeningStageBackgroundToCamera();
        mOpeningStageBackgroundRoot.SetActive(false);
    }

    private void FitOpeningStageBackgroundToCamera()
    {
        var camera = Camera.main;
        var sprite = mOpeningStageBackgroundRenderer != null
            ? mOpeningStageBackgroundRenderer.sprite
            : null;
        if (camera == null || sprite == null)
        {
            return;
        }

        var distance = Mathf.Abs(
            OpeningStageBackgroundWorldDepth - camera.transform.position.z);
        var screenCenter = camera.ScreenToWorldPoint(new Vector3(
            Screen.width * 0.5f,
            Screen.height * 0.5f,
            distance));
        screenCenter.z = OpeningStageBackgroundWorldDepth;
        mOpeningStageBackgroundRoot.transform.position = screenCenter;
        mOpeningStageBackgroundRoot.transform.rotation = Quaternion.identity;

        var visibleHeight = camera.orthographic
            ? camera.orthographicSize * 2f
            : 2f * distance * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        var visibleWidth = visibleHeight * camera.aspect;
        var spriteSize = sprite.bounds.size;
        var coverScale = Mathf.Max(
            visibleWidth / Mathf.Max(0.0001f, spriteSize.x),
            visibleHeight / Mathf.Max(0.0001f, spriteSize.y));
        mOpeningStageBackgroundRoot.transform.localScale = Vector3.one * coverScale;
    }

    private void CreateBagSelectBackdrop()
    {
        var backdropObject = new GameObject(
            BagSelectBackdropObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage));
        backdropObject.layer = mBagSelectPanelRoot.layer;
        var rectTransform = backdropObject.GetComponent<RectTransform>();
        StretchToParent(rectTransform, mBagSelectOverlayCanvas.transform);

        mBagSelectBackdropImage = backdropObject.GetComponent<RawImage>();
        mBagSelectBackdropImage.raycastTarget = false;
        backdropObject.SetActive(false);
    }

    private static void StretchToParent(RectTransform rectTransform, Transform parent)
    {
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private void OnBagSelectPlayClicked()
    {
        if (mIsPlayingAnimation
            || mIsReplayConfirmationVisible
            || mSelectedPackageEntry == null
            || mSelectedBagId <= 0)
        {
            return;
        }

        if (mSelectedPackageEntry.DisplayState == PackageDisplayState.TornCompleted)
        {
            if (mReplayPanelRoot != null)
            {
                ShowReplayConfirmation();
            }
            else
            {
                Debug.LogWarning("MainScene: completed pack replay requires PanelReplay.");
            }
            return;
        }

        mIsSelectedPackageReplay = false;
        if (mSelectedPackageEntry.DisplayState == PackageDisplayState.TornColorInProgress)
        {
            mPlayAnimationCoroutine = StartCoroutine(
                PlayTornPackageGameTransition(isReplaySession: false));
            return;
        }

        mPlayAnimationCoroutine = StartCoroutine(EnterCardPackOpeningStage());
    }

    private void ShowReplayConfirmation()
    {
        mIsReplayConfirmationVisible = true;
        SetBagSelectButtonsInteractable(false);
        SetUnselectedPackageVisualsVisible(false);
        SetSelectedPackageImageVisible(false);
        SetPanelVisible(mReplayPanelRoot, true);
    }

    private void OnReplayConfirmed()
    {
        if (!mIsReplayConfirmationVisible
            || mIsPlayingAnimation
            || mSelectedPackageEntry == null
            || mSelectedBagId <= 0)
        {
            return;
        }

        SetPanelVisible(mReplayPanelRoot, false);
        mIsReplayConfirmationVisible = false;
        mIsSelectedPackageReplay = true;
        RestoreCompletedReplayTransitionVisual();
        if (!CardPackDataUtility.TryClearPuzzleSession(mSelectedBagId))
        {
            Debug.LogWarning(
                $"MainScene: failed to reset puzzle session before replay. packId={mSelectedBagId}");
        }

        if (!CardPackDataUtility.TryEnsurePuzzleSession(mSelectedBagId))
        {
            Debug.LogWarning(
                $"MainScene: failed to create puzzle session for replay. packId={mSelectedBagId}");
        }

        mPlayAnimationCoroutine = StartCoroutine(
            PlayTornPackageGameTransition(isReplaySession: true));
    }

    private void RestoreCompletedReplayTransitionVisual()
    {
        if (mSelectedPackageVisualContent == null
            || mSelectedPackageOverlayImage == null
            || mSelectedPackageOverlayRect == null)
        {
            Debug.LogWarning(
                $"MainScene: completed replay visual could not be restored. "
                + $"packId={mSelectedBagId}");
            return;
        }

        mSelectedPackageOverlayRect.anchoredPosition = mSelectedPackageDisplayPosition;
        SetSelectedPackageVisualSize(mSelectedPackageDisplaySize);
        SetSelectedPackageImageAlpha(1f);
        SetSelectedPackageImageVisible(true);
        mIsSelectedPackageProgressPieceTransitioning = false;
    }

    private void OnReplayCancelled()
    {
        if (!mIsReplayConfirmationVisible || mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mReplayPanelRoot, false);
        mIsReplayConfirmationVisible = false;
        SetSelectedPackageImageVisible(true);
        SetUnselectedPackageVisualsVisible(true);
        SetBagSelectButtonsInteractable(true);
    }

    private void OnBagSelectBackClicked()
    {
        if (mIsPlayingAnimation || mSelectedPackageEntry == null)
        {
            return;
        }

        mPlayAnimationCoroutine = StartCoroutine(HidePackageSelection());
    }

    private void OnBagSelectCameraClicked()
    {
        if (mIsPlayingAnimation
            || mIsCapturingPhoto
            || mSelectedPackageEntry == null
            || mSelectedBagId <= 0
            || mPhotoPanelRoot == null
            || mPhotoImage == null)
        {
            return;
        }

        StartCoroutine(CaptureSelectedPackagePhoto());
    }

    private IEnumerator CaptureSelectedPackagePhoto()
    {
        mIsCapturingPhoto = true;
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        yield return PlayPhotoFlash();

        if (!TryCreatePhotoTexture(mSelectedBagId, out var photoTexture)
            || !TrySavePhotoToDesktop(photoTexture, mSelectedBagId, out var savedPath))
        {
            if (photoTexture != null)
            {
                Destroy(photoTexture);
            }

            mIsCapturingPhoto = false;
            mIsPlayingAnimation = false;
            SetBagSelectButtonsInteractable(true);
            yield break;
        }

        ApplyGeneratedPhoto(photoTexture);
        if (mPhotoGameIconRoot != null)
        {
            mPhotoGameIconRoot.SetActive(false);
        }

        SetSelectedPackageImageVisible(false);
        SetPanelVisible(mPhotoPanelRoot, true);
        mPhotoPanelRoot.transform.SetAsLastSibling();
        mIsCapturingPhoto = false;
        mIsPlayingAnimation = false;
        Debug.Log($"MainScene: photo saved to {savedPath}");
    }

    private IEnumerator PlayPhotoFlash()
    {
        if (mPhotoFlashCanvas == null || mPhotoFlashCanvasGroup == null)
        {
            yield break;
        }

        mPhotoFlashCanvas.gameObject.SetActive(true);
        yield return FadeCanvasGroup(
            mPhotoFlashCanvasGroup,
            0f,
            1f,
            PhotoFlashFadeInDuration);
        yield return new WaitForSecondsRealtime(PhotoFlashHoldDuration);
        yield return FadeCanvasGroup(
            mPhotoFlashCanvasGroup,
            1f,
            0f,
            PhotoFlashFadeOutDuration);
        mPhotoFlashCanvas.gameObject.SetActive(false);
    }

    private static IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float from,
        float to,
        float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        var elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            canvasGroup.alpha = Mathf.LerpUnclamped(from, to, normalized);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void OnPhotoOkClicked()
    {
        if (mIsCapturingPhoto || mPhotoPanelRoot == null)
        {
            return;
        }

        SetPanelVisible(mPhotoPanelRoot, false);
        SetSelectedPackageImageVisible(true);
        SetBagSelectButtonsInteractable(true);
    }

    private bool TryResolvePackageList()
    {
        mPackageItemTemplate = null;
        mPackageContentRoot = null;
        mPackagePageTemplate = null;
        mPackageScrollRect = null;
        mPackageSlotsById.Clear();

        return TryResolvePagedPackageList();
    }

    private bool TryResolvePagedPackageList()
    {
        var scrollViewObject = GameCommonUtility.FindSceneObject(PackageScrollViewObjectName);
        if (scrollViewObject == null || !scrollViewObject.TryGetComponent(out mPackageScrollRect))
        {
            return false;
        }

        if (mPackageScrollRect.content == null)
        {
            Debug.LogWarning("MainScene: PackageScrollView is missing ScrollRect.content.");
            return false;
        }

        mPackageContentRoot = mPackageScrollRect.content;
        mPackagePageTemplate = FindDirectChild(mPackageContentRoot, PackageFirstPageObjectName) as RectTransform;
        if (mPackagePageTemplate == null)
        {
            mPackagePageTemplate = FindFirstGridPage(mPackageContentRoot);
        }

        if (mPackagePageTemplate == null)
        {
            Debug.LogWarning($"MainScene: package page not found. Expected {PackageFirstPageObjectName} under Content.");
            return false;
        }

        mPackageItemTemplate = LoadPackItemPrefab();
        if (mPackageItemTemplate == null)
        {
            Debug.LogWarning(
                "MainScene: PackItem prefab is not assigned. Configure mPackageItemPrefab in MainScene.");
            return false;
        }

        mPackageScrollRect.horizontal = true;
        mPackageScrollRect.vertical = false;
        ConfigurePackagePageSnapInput(scrollViewObject);
        mPackagePageTemplate.gameObject.SetActive(true);
        NormalizePagedPackageLayout();
        return true;
    }

    private void ConfigurePackagePageSnapInput(GameObject scrollViewObject)
    {
        if (scrollViewObject == null)
        {
            return;
        }

        var eventTrigger = scrollViewObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = scrollViewObject.AddComponent<EventTrigger>();
        }

        if (eventTrigger.triggers == null)
        {
            eventTrigger.triggers = new List<EventTrigger.Entry>();
        }

        var beginDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        beginDragEntry.callback.AddListener(_ => HandlePackageListBeginDrag());
        eventTrigger.triggers.Add(beginDragEntry);

        var endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        endDragEntry.callback.AddListener(_ => HandlePackageListEndDrag());
        eventTrigger.triggers.Add(endDragEntry);
    }

    private IEnumerator SnapPackageListToNearestPage()
    {
        var pageCount = 0;
        if (mPackageContentRoot != null)
        {
            for (var i = 0; i < mPackageContentRoot.childCount; i++)
            {
                var page = mPackageContentRoot.GetChild(i);
                if (page.gameObject.activeSelf
                    && page.name.StartsWith(PackagePageObjectPrefix))
                {
                    pageCount++;
                }
            }
        }

        var maximumPageIndex = Mathf.Max(0, pageCount - 1);
        var currentPosition = Mathf.Clamp01(mPackageScrollRect.horizontalNormalizedPosition);
        var pagePosition = currentPosition * maximumPageIndex;
        var pageIndex = maximumPageIndex > 0
            ? Mathf.Clamp(Mathf.FloorToInt(pagePosition + 0.5f), 0, maximumPageIndex)
            : 0;
        var targetPosition = maximumPageIndex > 0
            ? pageIndex / (float)maximumPageIndex
            : 0f;

        mPackageScrollRect.StopMovement();
        if (Mathf.Abs(currentPosition - targetPosition) <= 0.0001f)
        {
            mPackageScrollRect.horizontalNormalizedPosition = targetPosition;
            mPackagePageSnapCoroutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < PackagePageSnapDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / PackagePageSnapDuration);
            var eased = 1f - Mathf.Pow(1f - normalized, 3f);
            mPackageScrollRect.StopMovement();
            mPackageScrollRect.horizontalNormalizedPosition = Mathf.LerpUnclamped(
                currentPosition,
                targetPosition,
                eased);
            yield return null;
        }

        mPackageScrollRect.StopMovement();
        mPackageScrollRect.horizontalNormalizedPosition = targetPosition;
        mPackagePageSnapCoroutine = null;
    }

    private void StopPackagePageSnap()
    {
        if (mPackagePageSnapCoroutine == null)
        {
            return;
        }

        StopCoroutine(mPackagePageSnapCoroutine);
        mPackagePageSnapCoroutine = null;
    }

    private IEnumerator RefreshPackageList()
    {
        if (mPackageContentRoot == null || mPackageItemTemplate == null)
        {
            yield break;
        }

        if (!CardPackDataUtility.Initialize())
        {
            Debug.LogWarning("MainScene: CardPackDataUtility is not ready, package list refresh skipped.");
            yield break;
        }

        var startedAt = Time.realtimeSinceStartup;
        var unlockedPackIds = CardPackDataUtility.TakeMainSceneOrderedPackIds();
        ClearPackageSlots();
        yield return null;

        for (var i = 0; i < unlockedPackIds.Count; i++)
        {
            var packId = unlockedPackIds[i];
            var entry = CreatePagedPackageSlot(packId, i);
            if (entry.Image == null)
            {
                continue;
            }

            ApplyPackageSlotVisual(entry, packId);
            entry.Root.SetActive(true);
            mPackageSlotsById[packId] = entry;
            if ((i + 1) % PackageListBuildBatchSize == 0)
            {
                yield return null;
            }
        }

        RefreshPackagePageLayout();
        if (mPackageScrollRect != null)
        {
            StopPackagePageSnap();
            mPackageScrollRect.StopMovement();
            mPackageScrollRect.horizontalNormalizedPosition = 0f;
        }

        Debug.Log(
            $"MainScene: package list refreshed. unlocked={unlockedPackIds.Count}, "
            + $"elapsed={(Time.realtimeSinceStartup - startedAt) * 1000f:F1}ms");
    }

    private void ClearPackageSlots()
    {
        foreach (var pair in mPackageSlotsById)
        {
            if (pair.Value.Root != null)
            {
                Destroy(pair.Value.Root);
            }
        }

        mPackageSlotsById.Clear();
        if (mPackageContentRoot == null)
        {
            return;
        }

        for (var i = mPackageContentRoot.childCount - 1; i >= 0; i--)
        {
            var page = mPackageContentRoot.GetChild(i);
            if (page == mPackagePageTemplate)
            {
                ClearPackagePage(page);
                page.gameObject.SetActive(true);
                continue;
            }

            if (page.name.StartsWith(PackagePageObjectPrefix))
            {
                Destroy(page.gameObject);
            }
        }
    }

    private void ClearPackagePage(Transform page)
    {
        if (page == null)
        {
            return;
        }

        for (var i = page.childCount - 1; i >= 0; i--)
        {
            var child = page.GetChild(i);
            if (child.gameObject == mPackageItemTemplate)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            if (TryParsePackageObjectName(child.name, out _) || child.name.StartsWith(PackItemTemplateObjectName))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private PackageEntry CreatePagedPackageSlot(int packId, int index)
    {
        var page = GetOrCreatePackagePage(index / PackagesPerPage);
        var slotObject = Instantiate(mPackageItemTemplate, page, false);
        slotObject.name = $"{GameDefine.PackageFilePrefix}{packId:D3}";
        slotObject.SetActive(true);

        var rootRect = slotObject.GetComponent<RectTransform>();
        var rootImage = slotObject.GetComponent<Image>();
        if (rootImage == null)
        {
            rootImage = slotObject.AddComponent<Image>();
        }

        var visualSettings = slotObject.GetComponent<PackCoverVisualSettings>();
        var coverImage = visualSettings != null && visualSettings.PackCover != null
            ? visualSettings.PackCover
            : FindChild(slotObject.transform, PackCoverObjectName)?.GetComponent<Image>() ?? rootImage;
        var backgroundImage = FindChild(slotObject.transform, PackBackgroundObjectName)?.GetComponent<Image>();
        var sizeImage = FindChild(slotObject.transform, PackSizeObjectName)?.GetComponent<Image>();
        var packNode = FindChild(slotObject.transform, PackNodeObjectName) as RectTransform;
        var packAnimator = packNode != null ? packNode.GetComponent<Animator>() : null;
        EnsurePackageBackgroundBehindCover(backgroundImage, coverImage);
        PreparePagedPackageItem(
            slotObject,
            rootRect,
            rootImage,
            coverImage,
            backgroundImage,
            packNode);
        EnsurePackageInteractionHandler(slotObject, coverImage, packId);

        var entry = new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = coverImage,
            BackgroundImage = backgroundImage,
            SizeImage = sizeImage,
            VisualSettings = visualSettings,
            PackAnimator = packAnimator,
            RectTransform = rootRect
        };
        return entry;
    }

    private RectTransform GetOrCreatePackagePage(int pageIndex)
    {
        if (pageIndex <= 0)
        {
            return mPackagePageTemplate;
        }

        var pageName = $"{PackagePageObjectPrefix}{pageIndex + 1}";
        var existing = FindDirectChild(mPackageContentRoot, pageName) as RectTransform;
        if (existing != null)
        {
            return existing;
        }

        var pageObject = Instantiate(mPackagePageTemplate.gameObject, mPackageContentRoot, false);
        pageObject.name = pageName;
        var pageRect = pageObject.GetComponent<RectTransform>();
        ClearPackagePage(pageRect);
        NormalizePackagePage(pageRect);
        pageObject.SetActive(true);
        return pageRect;
    }

    private void ApplyPackageSlotVisual(PackageEntry entry, int packId)
    {
        if (entry.Image == null || entry.Root == null)
        {
            return;
        }

        entry.Image.enabled = true;
        entry.Image.raycastTarget = false;
        var packSprite = GetOrLoadPackageCoverSprite(packId);
        if (packSprite != null)
        {
            entry.Image.sprite = packSprite;
        }

        var wasCompleted = CardPackDataUtility.TryGetPack(packId, out var record)
            && record.LifecycleState == CardPackLifecycleState.Completed;
        var hasActiveSession = CardPackDataUtility.HasActivePuzzleSession(packId);
        var showCompletedState = wasCompleted && !hasActiveSession;
        var hasCompletedFirstGroup = hasActiveSession
            && CardPackDataUtility.HasCompletedFirstPuzzleGroup(packId);
        entry.DisplayState = showCompletedState
            ? PackageDisplayState.TornCompleted
            : hasCompletedFirstGroup
                ? PackageDisplayState.TornColorInProgress
                : PackageDisplayState.IntactColor;
        var showTornState = entry.DisplayState != PackageDisplayState.IntactColor;
        ApplyPackageTornMask(
            entry.Image,
            entry.VisualSettings,
            showTornState,
            showCompletedState);
        entry.ShowTornBackground = showTornState;
        SetPackageBackgroundVisible(entry, true);
        ApplyPackageSizeVisual(entry.SizeImage, packId);
        ApplyPackageSizeMaterial(entry, showCompletedState);
        ApplyPackageBreathingAnimation(entry, showCompletedState);
        ApplyInProgressPackagePieces(entry, packId, hasCompletedFirstGroup);
        if (CardPackRewardFlyTransition.IsPackPending(packId))
        {
            SetPackageVisualsVisible(entry, false);
        }

        var nameText = FindChild(entry.Root.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            GameFontUtility.ApplyDefaultFont(nameText);
            nameText.text = $"Pack {packId:D3}";
            nameText.gameObject.SetActive(false);
        }

        EnsurePackageInteractionHandler(entry.Root, entry.Image, packId);
    }

    private void ApplyPackageTornMask(
        Image coverImage,
        PackCoverVisualSettings visualSettings,
        bool shouldShowTornState,
        bool isCompleted)
    {
        if (coverImage == null)
        {
            return;
        }

        var coverMaterial = visualSettings != null
            ? visualSettings.GetCoverMaterial(isCompleted)
            : coverImage.material;
        coverImage.material = coverMaterial;
        if (!shouldShowTornState)
        {
            return;
        }

        var randomStart = mPackTornMaskRandom.Next(PackTornMaskCount);
        for (var offset = 0; offset < PackTornMaskCount; offset++)
        {
            var maskIndex = (randomStart + offset) % PackTornMaskCount;
            var material = GetOrCreatePackTornMaskMaterial(
                maskIndex,
                coverMaterial,
                isCompleted);
            if (material != null)
            {
                coverImage.material = material;
                return;
            }
        }

        if (!mDidWarnPackTornMaskUnavailable)
        {
            mDidWarnPackTornMaskUnavailable = true;
            Debug.LogWarning("MainScene: no usable PackMask01-06.png torn mask was found.");
        }
    }

    private static void ApplyPackageBreathingAnimation(
        PackageEntry entry,
        bool isCompleted)
    {
        if (entry?.PackAnimator == null)
        {
            return;
        }

        entry.PackAnimator.speed = isCompleted
            ? CompletedPackBreathingSpeed
            : NormalPackBreathingSpeed;
        if (!entry.PackAnimator.isActiveAndEnabled)
        {
            return;
        }

        var normalizedStartTime = Mathf.Repeat(
            entry.BagId * PackBreathingPhaseStep,
            1f);
        entry.PackAnimator.Play(
            PackBreathingAnimationStateName,
            0,
            normalizedStartTime);
        entry.PackAnimator.Update(0f);
    }

    private Material GetOrCreatePackTornMaskMaterial(
        int maskIndex,
        Material coverMaterial,
        bool isCompleted)
    {
        if (maskIndex < 0 || maskIndex >= PackTornMaskCount)
        {
            return null;
        }

        var materialCache = isCompleted
            ? mPackCompletedTornMaskMaterials
            : mPackTornMaskMaterials;
        if (materialCache[maskIndex] != null)
        {
            return materialCache[maskIndex];
        }

        if (coverMaterial == null
            || !coverMaterial.HasProperty(TornMaskTextureId)
            || !coverMaterial.HasProperty(UseTornMaskId))
        {
            return null;
        }

        if (!mPackTornMaskLoadAttempted[maskIndex])
        {
            mPackTornMaskLoadAttempted[maskIndex] = true;
            var maskNumber = maskIndex + 1;
            var maskPath = $"{GameDefine.UiRoot}/PackImages/{PackTornMaskFilePrefix}{maskNumber:D2}.png";
            mPackTornMaskSprites[maskIndex] = GameCommonUtility.LoadSpriteByPath(
                maskPath,
                PixelsPerUnit);
            if (mPackTornMaskSprites[maskIndex] != null
                && mPackTornMaskSprites[maskIndex].texture != null)
            {
                mPackTornMaskSprites[maskIndex].texture.wrapMode = TextureWrapMode.Clamp;
            }
        }

        var maskSprite = mPackTornMaskSprites[maskIndex];
        if (maskSprite == null || maskSprite.texture == null)
        {
            return null;
        }

        var material = new Material(coverMaterial)
        {
            name = isCompleted
                ? $"PackCoverCompletedTornMask{maskIndex + 1:D2} (Runtime)"
                : $"PackCoverTornMask{maskIndex + 1:D2} (Runtime)",
            hideFlags = HideFlags.DontSave
        };
        material.SetTexture(TornMaskTextureId, maskSprite.texture);
        material.SetFloat(UseTornMaskId, 1f);
        materialCache[maskIndex] = material;
        return material;
    }

    private void ApplyInProgressPackagePieces(
        PackageEntry entry,
        int packId,
        bool shouldShowPieces)
    {
        if (entry == null || entry.Image == null || !shouldShowPieces)
        {
            return;
        }

        var cardBagPrefab = Resources.Load<GameObject>(
            GameDefine.FormatCardBagPrefabResourcesPath(packId));
        if (cardBagPrefab == null)
        {
            Debug.LogWarning(
                $"MainScene: in-progress pack pieces skipped; CardBag prefab not found. packId={packId}");
            return;
        }

        CardPackDataUtility.TryGetPlacedPieceNumbers(packId, out var placedPieceNumbers);
        var unplacedCandidates = new List<Image>();
        var placedCandidates = new List<Image>();
        var pieceImages = cardBagPrefab.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < pieceImages.Length; i++)
        {
            var pieceImage = pieceImages[i];
            if (pieceImage == null
                || pieceImage.sprite == null
                || !GameDefine.TryParsePieceObjectName(
                    pieceImage.gameObject.name,
                    out var pieceNumber))
            {
                continue;
            }

            if (placedPieceNumbers.Contains(pieceNumber))
            {
                placedCandidates.Add(pieceImage);
            }
            else
            {
                unplacedCandidates.Add(pieceImage);
            }
        }

        ShufflePackagePieceCandidates(unplacedCandidates);
        ShufflePackagePieceCandidates(placedCandidates);
        var selectedPieces = new List<Image>(InProgressPackPieceCount);
        AddPackagePieceCandidates(selectedPieces, unplacedCandidates);
        AddPackagePieceCandidates(selectedPieces, placedCandidates);
        if (selectedPieces.Count == 0)
        {
            Debug.LogWarning(
                $"MainScene: in-progress pack pieces skipped; no PieceGGII sprites found. packId={packId}");
            return;
        }

        var coverRect = entry.Image.rectTransform;
        var parent = coverRect.parent;
        if (parent == null)
        {
            return;
        }

        var progressVisualScale = Mathf.Min(
            coverRect.rect.width / PackageCoverWidth,
            coverRect.rect.height / PackageCoverHeight);
        if (float.IsNaN(progressVisualScale)
            || float.IsInfinity(progressVisualScale)
            || progressVisualScale <= 0f)
        {
            progressVisualScale = 1f;
        }

        EnsurePackageBackgroundBehindCover(entry.BackgroundImage, entry.Image);

        var piecesRootObject = new GameObject(
            InProgressPackPiecesObjectName,
            typeof(RectTransform));
        piecesRootObject.layer = entry.Image.gameObject.layer;
        var piecesRoot = piecesRootObject.GetComponent<RectTransform>();
        piecesRoot.SetParent(parent, false);
        piecesRoot.anchorMin = new Vector2(0.5f, 0.5f);
        piecesRoot.anchorMax = new Vector2(0.5f, 0.5f);
        piecesRoot.pivot = new Vector2(0.5f, 0.5f);
        piecesRoot.anchoredPosition = coverRect.anchoredPosition;
        piecesRoot.sizeDelta = coverRect.sizeDelta;
        piecesRoot.localScale = Vector3.one;
        piecesRoot.SetSiblingIndex(coverRect.GetSiblingIndex());
        entry.ProgressPiecesRoot = piecesRootObject;
        entry.ProgressPieceAnimations = new List<InProgressPackagePieceAnimation>(
            selectedPieces.Count);

        for (var i = 0; i < selectedPieces.Count; i++)
        {
            entry.ProgressPieceAnimations.Add(
                CreateInProgressPackagePiece(
                    piecesRoot,
                    selectedPieces[i].sprite,
                    packId,
                    i,
                    progressVisualScale));
        }
    }

    private void ShufflePackagePieceCandidates(List<Image> candidates)
    {
        for (var i = candidates.Count - 1; i > 0; i--)
        {
            var swapIndex = mPackTornMaskRandom.Next(i + 1);
            var candidate = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = candidate;
        }
    }

    private static void AddPackagePieceCandidates(
        List<Image> selectedPieces,
        List<Image> candidates)
    {
        for (var i = 0;
             i < candidates.Count && selectedPieces.Count < InProgressPackPieceCount;
             i++)
        {
            selectedPieces.Add(candidates[i]);
        }
    }

    private static InProgressPackagePieceAnimation CreateInProgressPackagePiece(
        RectTransform parent,
        Sprite sprite,
        int packId,
        int index,
        float progressVisualScale)
    {
        var pieceObject = new GameObject(
            $"ProgressPiece{index + 1:D2}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Shadow));
        pieceObject.layer = parent.gameObject.layer;
        var pieceRect = pieceObject.GetComponent<RectTransform>();
        pieceRect.SetParent(parent, false);
        pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
        pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
        pieceRect.pivot = new Vector2(0.5f, 0.5f);
        var rotationDegrees = GetInProgressPackagePieceRotation(index);
        pieceRect.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        pieceRect.localScale = Vector3.one;

        var spriteSize = sprite.rect.size;
        var targetMaxSize = InProgressPackPieceMaxSize * progressVisualScale;
        var scale = targetMaxSize / Mathf.Max(spriteSize.x, spriteSize.y, 1f);
        var previousSize = spriteSize * scale;
        var displayedSize = previousSize * InProgressPackPieceScaleMultiplier;
        var basePosition = GetInProgressPackagePiecePosition(index) * progressVisualScale;
        basePosition.y -= (displayedSize.y - previousSize.y) * 0.5f;
        var rotationRadians = rotationDegrees * Mathf.Deg2Rad;
        var rotatedHalfWidth = (
            Mathf.Abs(Mathf.Cos(rotationRadians)) * displayedSize.x
            + Mathf.Abs(Mathf.Sin(rotationRadians)) * displayedSize.y) * 0.5f;
        var minimumCenterX = parent.rect.xMin
            + rotatedHalfWidth
            + InProgressPackPieceHorizontalMargin * progressVisualScale;
        var maximumCenterX = parent.rect.xMax
            - rotatedHalfWidth
            - InProgressPackPieceHorizontalMargin * progressVisualScale;
        basePosition.x = minimumCenterX <= maximumCenterX
            ? Mathf.Clamp(basePosition.x, minimumCenterX, maximumCenterX)
            : parent.rect.center.x;
        pieceRect.anchoredPosition = basePosition;
        pieceRect.sizeDelta = displayedSize;

        var pieceImage = pieceObject.GetComponent<Image>();
        pieceImage.sprite = sprite;
        pieceImage.color = Color.white;
        pieceImage.preserveAspect = true;
        pieceImage.useSpriteMesh = false;
        pieceImage.raycastTarget = false;

        var shadow = pieceObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        shadow.effectDistance = new Vector2(2f, -3f) * progressVisualScale;
        shadow.useGraphicAlpha = true;

        var normalizedPhase = Mathf.Repeat(
            packId * PackBreathingPhaseStep
                + index / (float)InProgressPackPieceCount,
            1f);
        return new InProgressPackagePieceAnimation
        {
            RectTransform = pieceRect,
            BasePosition = basePosition,
            FloatDistance = InProgressPackPieceFloatDistance * progressVisualScale,
            PhaseRadians = normalizedPhase * Mathf.PI * 2f
        };
    }

    private void UpdateInProgressPackagePieceAnimations()
    {
        var cycleRadians = Time.unscaledTime
            * (Mathf.PI * 2f / InProgressPackPieceFloatDuration);
        foreach (var pair in mPackageSlotsById)
        {
            UpdateInProgressPackagePieceAnimations(
                pair.Value?.ProgressPieceAnimations,
                cycleRadians);
        }

        if (!mIsSelectedPackageProgressPieceTransitioning)
        {
            UpdateInProgressPackagePieceAnimations(
                mSelectedPackageProgressPieceAnimations,
                cycleRadians);
        }
    }

    private static void UpdateInProgressPackagePieceAnimations(
        List<InProgressPackagePieceAnimation> animations,
        float cycleRadians)
    {
        if (animations == null)
        {
            return;
        }

        for (var i = 0; i < animations.Count; i++)
        {
            var animation = animations[i];
            if (animation?.RectTransform == null)
            {
                continue;
            }

            var position = animation.BasePosition;
            position.y += Mathf.Sin(cycleRadians + animation.PhaseRadians)
                * animation.FloatDistance;
            animation.RectTransform.anchoredPosition = position;
        }
    }

    private static Vector2 GetInProgressPackagePiecePosition(int index)
    {
        switch (index)
        {
            case 0:
                return new Vector2(-66f, 68f);
            case 1:
                return new Vector2(0f, 82f);
            default:
                return new Vector2(66f, 70f);
        }
    }

    private static float GetInProgressPackagePieceRotation(int index)
    {
        switch (index)
        {
            case 0:
                return 344f;
            case 1:
                return 5f;
            default:
                return 18f;
        }
    }

    private void ReleasePackTornMaskResources()
    {
        for (var i = 0; i < PackTornMaskCount; i++)
        {
            if (mPackTornMaskMaterials[i] != null)
            {
                Destroy(mPackTornMaskMaterials[i]);
                mPackTornMaskMaterials[i] = null;
            }

            if (mPackCompletedTornMaskMaterials[i] != null)
            {
                Destroy(mPackCompletedTornMaskMaterials[i]);
                mPackCompletedTornMaskMaterials[i] = null;
            }

            var sprite = mPackTornMaskSprites[i];
            if (sprite == null)
            {
                continue;
            }

            var texture = sprite.texture;
            Destroy(sprite);
            if (texture != null)
            {
                Destroy(texture);
            }

            mPackTornMaskSprites[i] = null;
        }

    }

    private void UpdatePackageDisplays()
    {
        if (mPackageSlotsById.Count == 0)
        {
            return;
        }

        var viewport = mPackageScrollRect != null ? mPackageScrollRect.viewport : null;
        var panelsObscurePackages = IsAnyPackagePanelOpen();
        foreach (var pair in mPackageSlotsById)
        {
            var entry = pair.Value;
            if (entry == null || entry.Root == null || entry.Image == null)
            {
                continue;
            }

            var anchor = entry.Image.rectTransform;
            var shouldRender = !entry.SuppressDisplay
                && entry != mSelectedPackageEntry
                && !panelsObscurePackages
                && entry.Root.activeInHierarchy
                && IsRectVisibleInViewport(anchor, viewport);
            SetPackageCoverVisible(entry, shouldRender);
            SetPackageBackgroundVisible(entry, shouldRender);
            SetPackageSizeImageVisible(entry, shouldRender);
            SetPackageProgressPiecesVisible(entry, shouldRender);
        }
    }

    private bool IsAnyPackagePanelOpen()
    {
        return mMenuPanelRoot != null && mMenuPanelRoot.activeInHierarchy
            || mSettingsPanelRoot != null && mSettingsPanelRoot.activeInHierarchy
            || mUsablePanelRoot != null && mUsablePanelRoot.activeInHierarchy
            || mSavePanelRoot != null && mSavePanelRoot.activeInHierarchy;
    }

    private static bool IsRectVisibleInViewport(RectTransform target, RectTransform viewport)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (viewport == null)
        {
            return true;
        }

        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        var viewportRect = viewport.rect;
        return bounds.max.x >= viewportRect.xMin
            && bounds.min.x <= viewportRect.xMax
            && bounds.max.y >= viewportRect.yMin
            && bounds.min.y <= viewportRect.yMax;
    }

    private static Rect GetScreenRect(RectTransform rectTransform, Camera camera)
    {
        if (rectTransform == null)
        {
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var first = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        var min = first;
        var max = first;
        for (var i = 1; i < corners.Length; i++)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static Camera ResolveCanvasCamera(RectTransform rectTransform)
    {
        var canvas = rectTransform != null ? rectTransform.GetComponentInParent<Canvas>() : null;
        return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
    }

    private static void SetPackageCoverVisible(PackageEntry entry, bool visible)
    {
        if (entry == null)
        {
            return;
        }

        if (entry.Image != null)
        {
            entry.Image.enabled = visible;
        }
    }

    private static void SetPackageBackgroundVisible(PackageEntry entry, bool visible)
    {
        if (entry?.BackgroundImage == null)
        {
            return;
        }

        var backgroundObject = entry.BackgroundImage.gameObject;
        if (backgroundObject.activeSelf != entry.ShowTornBackground)
        {
            backgroundObject.SetActive(entry.ShowTornBackground);
        }

        entry.BackgroundImage.enabled = visible && entry.ShowTornBackground;
    }

    private static void SetPackageSizeImageVisible(PackageEntry entry, bool visible)
    {
        if (entry?.SizeImage != null)
        {
            entry.SizeImage.enabled = visible && entry.SizeImage.sprite != null;
        }
    }

    private static void SetPackageProgressPiecesVisible(PackageEntry entry, bool visible)
    {
        if (entry?.ProgressPiecesRoot != null)
        {
            entry.ProgressPiecesRoot.SetActive(visible);
        }
    }

    private void ApplyPackageSizeVisual(Image sizeImage, int packId)
    {
        if (sizeImage == null)
        {
            return;
        }

        sizeImage.raycastTarget = false;
        sizeImage.preserveAspect = true;
        if (!GameConfigRepository.TryGetCardPackConfig(packId, out var config)
            || config.PackSize < CardPackSize.XS
            || config.PackSize > CardPackSize.XXXL)
        {
            sizeImage.gameObject.SetActive(false);
            Debug.LogWarning($"MainScene: pack size icon skipped. Invalid PackSize for packId={packId}.");
            return;
        }

        var sizeSprite = GetOrLoadPackageSizeSprite(config.PackSize);
        if (sizeSprite == null)
        {
            sizeImage.gameObject.SetActive(false);
            return;
        }

        sizeImage.sprite = sizeSprite;
        sizeImage.enabled = true;
        sizeImage.gameObject.SetActive(true);
    }

    private static void ApplyPackageSizeMaterial(PackageEntry entry, bool isCompleted)
    {
        if (entry?.SizeImage == null || entry.VisualSettings == null)
        {
            return;
        }

        entry.SizeImage.material = entry.VisualSettings.GetSizeMaterial(isCompleted);
    }

    private void RefreshPackagePageLayout()
    {
        NormalizePagedPackageLayout();
        Canvas.ForceUpdateCanvases();
        if (mPackageContentRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(mPackageContentRoot);
        }
    }

    private void NormalizePagedPackageLayout()
    {
        if (mPackageScrollRect == null || mPackageContentRoot == null || mPackagePageTemplate == null)
        {
            return;
        }

        mPackageScrollRect.horizontal = true;
        mPackageScrollRect.vertical = false;

        NormalizePackageContent();
        for (var i = 0; i < mPackageContentRoot.childCount; i++)
        {
            NormalizePackagePage(mPackageContentRoot.GetChild(i) as RectTransform);
        }
    }

    private void NormalizePackageContent()
    {
        var contentRect = mPackageContentRoot;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        var viewportSize = GetPackageViewportSize();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, viewportSize.y);

        var horizontalLayout = contentRect.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout == null)
        {
            horizontalLayout = contentRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        horizontalLayout.padding.left = 0;
        horizontalLayout.padding.right = 0;
        horizontalLayout.padding.top = 0;
        horizontalLayout.padding.bottom = 0;
        horizontalLayout.spacing = 0f;
        horizontalLayout.childAlignment = TextAnchor.UpperLeft;
        horizontalLayout.childControlWidth = true;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.childForceExpandHeight = false;

        var contentSizeFitter = contentRect.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
        }

        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void NormalizePackagePage(RectTransform pageRect)
    {
        if (pageRect == null)
        {
            return;
        }

        var viewportSize = GetPackageViewportSize();
        pageRect.anchorMin = new Vector2(0f, 1f);
        pageRect.anchorMax = new Vector2(0f, 1f);
        pageRect.pivot = new Vector2(0f, 1f);
        pageRect.localScale = Vector3.one;
        pageRect.sizeDelta = viewportSize;

        var layout = pageRect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = pageRect.gameObject.AddComponent<LayoutElement>();
        }

        layout.minWidth = viewportSize.x;
        layout.minHeight = viewportSize.y;
        layout.preferredWidth = viewportSize.x;
        layout.preferredHeight = viewportSize.y;
        layout.flexibleWidth = -1f;
        layout.flexibleHeight = -1f;

        var grid = pageRect.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = pageRect.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.padding.left = 0;
        grid.padding.right = 0;
        grid.padding.top = 0;
        grid.padding.bottom = 0;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.cellSize = new Vector2(PackageSlotWidth, PackageSlotHeight);
        grid.spacing = new Vector2(PackageHorizontalSpacing, PackageVerticalSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = PackagesPerPageColumnCount;
    }

    private Vector2 GetPackageViewportSize()
    {
        var viewport = mPackageScrollRect != null ? mPackageScrollRect.viewport : null;
        if (viewport != null && viewport.rect.width > 0f && viewport.rect.height > 0f)
        {
            return viewport.rect.size;
        }

        var scrollRectTransform = mPackageScrollRect != null ? mPackageScrollRect.transform as RectTransform : null;
        if (scrollRectTransform != null && scrollRectTransform.rect.width > 0f && scrollRectTransform.rect.height > 0f)
        {
            return scrollRectTransform.rect.size;
        }

        return new Vector2(DefaultPackagePageWidth, DefaultPackagePageHeight);
    }

    private void EnsurePackageInteractionHandler(GameObject targetObject, Image visualImage, int bagId)
    {
        if (targetObject == null || visualImage == null)
        {
            return;
        }

        visualImage.raycastTarget = false;
        var clickImage = targetObject.GetComponent<Image>();
        if (clickImage != null)
        {
            clickImage.raycastTarget = true;
        }

        var handler = targetObject.GetComponent<PackageInteractionHandler>();
        if (handler == null)
        {
            handler = targetObject.AddComponent<PackageInteractionHandler>();
        }

        handler.Initialize(this, bagId, visualImage, mPackageScrollRect);
    }

    private void ConfigureRankButton()
    {
        var rankButtonObject = GameObject.Find(GameDefine.RankButtonObjectName);
        if (rankButtonObject == null)
        {
            Debug.LogWarning($"MainScene: rank button not found. Expected object named {GameDefine.RankButtonObjectName}.");
            return;
        }

        var button = rankButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"MainScene: {GameDefine.RankButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnRankButtonClicked);
        button.onClick.AddListener(OnRankButtonClicked);
    }

    private void OnRankButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        GameManager.EnterRankScene();
    }

    private void ConfigureAchieveButton()
    {
        var achieveButtonObject = GameObject.Find(GameDefine.AchieveButtonObjectName);
        if (achieveButtonObject == null)
        {
            Debug.LogWarning($"MainScene: achieve button not found. Expected object named {GameDefine.AchieveButtonObjectName}.");
            return;
        }

        var button = achieveButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"MainScene: {GameDefine.AchieveButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnAchieveButtonClicked);
        button.onClick.AddListener(OnAchieveButtonClicked);
    }

    private void OnAchieveButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        GameManager.EnterAchieveScene();
    }

    private void ConfigureWishListButton()
    {
        ConfigureExternalLinkButton(
            WishListButtonObjectName,
            OnWishListButtonClicked,
            "wish list");
    }

    private static void OnWishListButtonClicked()
    {
        Application.OpenURL(WishListUrl);
    }

    private void ConfigureDiscordButton()
    {
        ConfigureExternalLinkButton(
            DiscordButtonObjectName,
            OnDiscordButtonClicked,
            "Discord");
    }

    private static void OnDiscordButtonClicked()
    {
        Application.OpenURL(DiscordUrl);
    }

    private void ConfigureQqButton()
    {
        ConfigureExternalLinkButton(QqButtonObjectName, OnQqButtonClicked, "QQ");
    }

    private static void OnQqButtonClicked()
    {
        Application.OpenURL(QqGroupUrl);
    }

    private static void ConfigureExternalLinkButton(
        string objectName,
        UnityEngine.Events.UnityAction onClick,
        string displayName)
    {
        var buttonObject = GameCommonUtility.FindSceneObject(objectName);
        var button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        if (button == null)
        {
            Debug.LogWarning(
                $"MainScene: {displayName} button not found. Expected {objectName}.");
            return;
        }

        button.onClick.RemoveListener(onClick);
        button.onClick.AddListener(onClick);
    }

    private void ConfigureMenuPanel()
    {
        mMenuPanelRoot = GameCommonUtility.FindSceneObject(MenuPanelObjectName);
        if (mMenuPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: menu panel not found. Expected object named {MenuPanelObjectName}.");
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);

        var menuButton = GameCommonUtility.FindSceneObject(MenuButtonObjectName)?.GetComponent<Button>();
        if (menuButton == null)
        {
            Debug.LogWarning($"MainScene: menu button not found. Expected object named {MenuButtonObjectName}.");
        }
        else
        {
            menuButton.onClick.RemoveListener(OnMenuButtonClicked);
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        var closeButton = FindChild(mMenuPanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnMenuCloseButtonClicked);
            closeButton.onClick.AddListener(OnMenuCloseButtonClicked);
        }

        var returnButton = FindChild(mMenuPanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnMenuCloseButtonClicked);
            returnButton.onClick.AddListener(OnMenuCloseButtonClicked);
        }

        var settingsButton = FindChild(mMenuPanelRoot.transform, SettingsButtonObjectName)?.GetComponent<Button>();
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        var usableButton = FindChild(mMenuPanelRoot.transform, UsableButtonObjectName)?.GetComponent<Button>();
        if (usableButton != null)
        {
            usableButton.onClick.RemoveListener(OnUsableButtonClicked);
            usableButton.onClick.AddListener(OnUsableButtonClicked);
        }

        var saveButton = FindChild(mMenuPanelRoot.transform, SaveButtonObjectName)?.GetComponent<Button>();
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
    }

    private void OnMenuButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, true);
    }

    private void OnMenuCloseButtonClicked()
    {
        SetPanelVisible(mMenuPanelRoot, false);
    }

    private void ConfigureSettingsPanel()
    {
        mSettingsPanelRoot = GameCommonUtility.FindSceneObject(SettingsPanelObjectName);
        if (mSettingsPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: settings panel not found. Expected object named {SettingsPanelObjectName}.");
            return;
        }

        mMusicSlider = ConfigureSettingsSlider(MusicSliderObjectName);
        mEffectSlider = ConfigureSettingsSlider(EffectSliderObjectName);
        mWindowedToggle = FindChild(mSettingsPanelRoot.transform, WindowedToggleObjectName)?.GetComponent<Toggle>();

        ApplySettingsPanelValues(GameSettingsUtility.GetSettings());
        BindSettingsControls();
        SetPanelVisible(mSettingsPanelRoot, false);
    }

    private void BindSettingsControls()
    {
        var closeButton = FindChild(mSettingsPanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnSettingsCloseButtonClicked);
            closeButton.onClick.AddListener(OnSettingsCloseButtonClicked);
        }

        var returnButton = FindChild(mSettingsPanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnSettingsCloseButtonClicked);
            returnButton.onClick.AddListener(OnSettingsCloseButtonClicked);
        }

        if (mMusicSlider != null)
        {
            mMusicSlider.ValueChanged = OnMusicVolumeChanged;
        }
        else
        {
            Debug.LogWarning($"MainScene: settings music slider not found. Expected {MusicSliderObjectName} under {SettingsPanelObjectName}.");
        }

        if (mEffectSlider != null)
        {
            mEffectSlider.ValueChanged = OnEffectVolumeChanged;
        }
        else
        {
            Debug.LogWarning($"MainScene: settings effect slider not found. Expected {EffectSliderObjectName} under {SettingsPanelObjectName}.");
        }

        if (mWindowedToggle != null)
        {
            mWindowedToggle.onValueChanged.RemoveListener(OnWindowedToggleChanged);
            mWindowedToggle.onValueChanged.AddListener(OnWindowedToggleChanged);
        }
        else
        {
            Debug.LogWarning($"MainScene: settings windowed toggle not found. Expected {WindowedToggleObjectName} under {SettingsPanelObjectName}.");
        }
    }

    private void ApplySettingsPanelValues(GameSettingsData settings)
    {
        if (settings == null)
        {
            return;
        }

        mIsApplyingSettingsToUi = true;
        if (mMusicSlider != null)
        {
            mMusicSlider.SetValueWithoutNotify(settings.MusicVolume);
        }

        if (mEffectSlider != null)
        {
            mEffectSlider.SetValueWithoutNotify(settings.EffectVolume);
        }

        if (mWindowedToggle != null)
        {
            mWindowedToggle.SetIsOnWithoutNotify(settings.IsWindowed);
        }

        mIsApplyingSettingsToUi = false;
    }

    private FakeSettingsSliderInput ConfigureSettingsSlider(string objectName)
    {
        var rootRect = FindChild(mSettingsPanelRoot.transform, objectName) as RectTransform;
        if (rootRect == null)
        {
            return null;
        }

        return FakeSettingsSliderInput.Attach(rootRect);
    }

    private void OnSettingsButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mSettingsPanelRoot, true);
    }

    private void OnSettingsCloseButtonClicked()
    {
        SetPanelVisible(mSettingsPanelRoot, false);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetMusicVolume(value);
    }

    private void OnEffectVolumeChanged(float value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetEffectVolume(value);
    }

    private void OnWindowedToggleChanged(bool isWindowed)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetWindowed(isWindowed);
    }

    private void ConfigureUsablePanel()
    {
        mUsablePanelRoot = GameCommonUtility.FindSceneObject(UsablePanelObjectName);
        if (mUsablePanelRoot == null)
        {
            Debug.LogWarning($"MainScene: usable panel not found. Expected object named {UsablePanelObjectName}.");
            return;
        }

        mUsableToggle1 = FindChild(mUsablePanelRoot.transform, UsableToggle1ObjectName)?.GetComponent<Toggle>();
        mUsableToggle2 = FindChild(mUsablePanelRoot.transform, UsableToggle2ObjectName)?.GetComponent<Toggle>();
        mUsableToggle3 = FindChild(mUsablePanelRoot.transform, UsableToggle3ObjectName)?.GetComponent<Toggle>();
        mUsableContentBackgroundImage = FindChild(
            mUsablePanelRoot.transform,
            UsableContentBackgroundObjectName)?.GetComponent<Image>();
        mUsableContentLineImage = FindChild(
            mUsablePanelRoot.transform,
            UsableContentLineObjectName)?.GetComponent<Image>();

        LoadUsablePanelPreviewSprites();

        ApplyUsablePanelValues(GameSettingsUtility.GetSettings());
        BindUsableControls();
        SetPanelVisible(mUsablePanelRoot, false);
    }

    private void BindUsableControls()
    {
        var closeButton = FindChild(mUsablePanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnUsableCloseButtonClicked);
            closeButton.onClick.AddListener(OnUsableCloseButtonClicked);
        }

        var returnButton = FindChild(mUsablePanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnUsableCloseButtonClicked);
            returnButton.onClick.AddListener(OnUsableCloseButtonClicked);
        }

        if (mUsableToggle1 != null)
        {
            mUsableToggle1.onValueChanged.RemoveListener(OnUsableToggle1Changed);
            mUsableToggle1.onValueChanged.AddListener(OnUsableToggle1Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle1ObjectName} under {UsablePanelObjectName}.");
        }

        if (mUsableToggle2 != null)
        {
            mUsableToggle2.onValueChanged.RemoveListener(OnUsableToggle2Changed);
            mUsableToggle2.onValueChanged.AddListener(OnUsableToggle2Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle2ObjectName} under {UsablePanelObjectName}.");
        }

        if (mUsableToggle3 != null)
        {
            mUsableToggle3.onValueChanged.RemoveListener(OnUsableToggle3Changed);
            mUsableToggle3.onValueChanged.AddListener(OnUsableToggle3Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle3ObjectName} under {UsablePanelObjectName}.");
        }
    }

    private void OnUsableButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mUsablePanelRoot, true);
    }

    private void OnUsableCloseButtonClicked()
    {
        SetPanelVisible(mUsablePanelRoot, false);
    }

    private void OnUsableToggle1Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption1(value);
        RefreshUsablePanelPreview();
    }

    private void OnUsableToggle2Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption2(value);
        RefreshUsablePanelPreview();
    }

    private void OnUsableToggle3Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption3(value);
        RefreshUsablePanelPreview();
    }

    private void ConfigureSavePanel()
    {
        mSavePanelRoot = GameCommonUtility.FindSceneObject(SavePanelObjectName);
        if (mSavePanelRoot == null)
        {
            Debug.LogWarning($"MainScene: save panel not found. Expected object named {SavePanelObjectName}.");
            return;
        }

        BindSaveControls();
        SetPanelVisible(mSavePanelRoot, false);
    }

    private void BindSaveControls()
    {
        var closeButton = FindChild(mSavePanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnSaveCloseButtonClicked);
            closeButton.onClick.AddListener(OnSaveCloseButtonClicked);
        }

        var returnButton = FindChild(mSavePanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnSaveCloseButtonClicked);
            returnButton.onClick.AddListener(OnSaveCloseButtonClicked);
        }
    }

    private void OnSaveButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mSavePanelRoot, true);
    }

    private void OnSaveCloseButtonClicked()
    {
        SetPanelVisible(mSavePanelRoot, false);
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(visible);
        if (visible)
        {
            panel.transform.SetAsLastSibling();
        }
    }

    private void ApplyUsablePanelValues(GameSettingsData settings)
    {
        if (settings == null)
        {
            return;
        }

        mIsApplyingSettingsToUi = true;
        if (mUsableToggle1 != null)
        {
            mUsableToggle1.SetIsOnWithoutNotify(settings.UsableOption1);
        }

        if (mUsableToggle2 != null)
        {
            mUsableToggle2.SetIsOnWithoutNotify(settings.UsableOption2);
        }

        if (mUsableToggle3 != null)
        {
            mUsableToggle3.SetIsOnWithoutNotify(settings.UsableOption3);
        }

        mIsApplyingSettingsToUi = false;
        RefreshUsablePanelPreview(
            settings.UsableOption1,
            settings.UsableOption2,
            settings.UsableOption3);
    }

    private void LoadUsablePanelPreviewSprites()
    {
        mUsableHighContrastOffSprite = GameCommonUtility.LoadSpriteByPath(
            UsableHighContrastOffPath,
            PixelsPerUnit);
        mUsableHighContrastOnSprite = GameCommonUtility.LoadSpriteByPath(
            UsableHighContrastOnPath,
            PixelsPerUnit);
        mUsableLineOffSprite = GameCommonUtility.LoadSpriteByPath(
            UsableLineOffPath,
            PixelsPerUnit);
        mUsableLevelOutlineSprite = GameCommonUtility.LoadSpriteByPath(
            UsableLevelOutlinePath,
            PixelsPerUnit);
        mUsableStickerOutlineSprite = GameCommonUtility.LoadSpriteByPath(
            UsableStickerOutlinePath,
            PixelsPerUnit);

        if (mUsableContentBackgroundImage == null)
        {
            Debug.LogWarning(
                $"MainScene: usable preview image not found. Expected {UsableContentBackgroundObjectName} " +
                $"under {UsablePanelObjectName}.");
        }

        if (mUsableContentLineImage == null)
        {
            Debug.LogWarning(
                $"MainScene: usable preview image not found. Expected {UsableContentLineObjectName} " +
                $"under {UsablePanelObjectName}.");
        }
    }

    private void RefreshUsablePanelPreview()
    {
        RefreshUsablePanelPreview(
            mUsableToggle1 != null && mUsableToggle1.isOn,
            mUsableToggle2 != null && mUsableToggle2.isOn,
            mUsableToggle3 != null && mUsableToggle3.isOn);
    }

    private void RefreshUsablePanelPreview(
        bool levelOutlineEnabled,
        bool stickerOutlineEnabled,
        bool highContrastEnabled)
    {
        if (mUsableContentBackgroundImage != null)
        {
            var backgroundSprite = highContrastEnabled
                ? mUsableHighContrastOnSprite
                : mUsableHighContrastOffSprite;
            if (backgroundSprite != null)
            {
                mUsableContentBackgroundImage.sprite = backgroundSprite;
            }
        }

        if (mUsableContentLineImage != null)
        {
            var lineSprite = stickerOutlineEnabled
                ? mUsableStickerOutlineSprite
                : levelOutlineEnabled
                    ? mUsableLevelOutlineSprite
                    : mUsableLineOffSprite;
            if (lineSprite != null)
            {
                mUsableContentLineImage.sprite = lineSprite;
            }
        }
    }

    private void ReleaseUsablePanelPreviewSprites()
    {
        ReleaseRuntimeSprite(ref mUsableHighContrastOffSprite);
        ReleaseRuntimeSprite(ref mUsableHighContrastOnSprite);
        ReleaseRuntimeSprite(ref mUsableLineOffSprite);
        ReleaseRuntimeSprite(ref mUsableLevelOutlineSprite);
        ReleaseRuntimeSprite(ref mUsableStickerOutlineSprite);
    }

    private static void ReleaseRuntimeSprite(ref Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        var texture = sprite.texture;
        Destroy(sprite);
        if (texture != null)
        {
            Destroy(texture);
        }

        sprite = null;
    }

    private static bool TryParsePackageObjectName(string objectName, out int bagId)
    {
        bagId = 0;
        if (string.IsNullOrWhiteSpace(objectName)
            || !objectName.StartsWith(GameDefine.PackageFilePrefix))
        {
            return false;
        }

        var idText = objectName.Substring(GameDefine.PackageFilePrefix.Length);
        return int.TryParse(idText, out bagId) && bagId > 0;
    }

    private GameObject LoadPackItemPrefab()
    {
        if (mPackageItemPrefab != null)
        {
            return mPackageItemPrefab;
        }

#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PackItemPrefabEditorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(PackItemPrefabResourcesPath);
    }

    private static void PreparePagedPackageItem(
        GameObject itemObject,
        RectTransform rootRect,
        Image rootImage,
        Image coverImage,
        Image backgroundImage,
        RectTransform packNode)
    {
        if (rootRect != null)
        {
            rootRect.localScale = Vector3.one;
            if (rootRect.sizeDelta.x <= 0f || rootRect.sizeDelta.y <= 0f)
            {
                rootRect.sizeDelta = new Vector2(PackageSlotWidth, PackageSlotHeight);
            }
        }

        var layout = itemObject.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = itemObject.AddComponent<LayoutElement>();
        }

        layout.minWidth = PackageSlotWidth;
        layout.minHeight = PackageSlotHeight;

        if (rootImage != null)
        {
            rootImage.color = new Color(rootImage.color.r, rootImage.color.g, rootImage.color.b, 0f);
            rootImage.raycastTarget = true;
        }

        if (coverImage != null)
        {
            coverImage.raycastTarget = false;
            coverImage.preserveAspect = true;
            var coverRect = coverImage.rectTransform;
            var sourceCoverSize = coverRect.sizeDelta;
            if (packNode != null && sourceCoverSize.x > 0f && sourceCoverSize.y > 0f)
            {
                var listScale = Mathf.Min(
                    PackageCoverWidth / sourceCoverSize.x,
                    PackageCoverHeight / sourceCoverSize.y);
                packNode.localScale = Vector3.one * listScale;
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
        }

        var nameText = FindChild(itemObject.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
            nameText.raycastTarget = false;
            nameText.alignment = TextAlignmentOptions.Center;
            GameFontUtility.ApplyDefaultFont(nameText);
        }
    }

    private static RectTransform FindFirstGridPage(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.GetComponent<GridLayoutGroup>() != null)
            {
                return child as RectTransform;
            }
        }

        return null;
    }

    private static Transform FindDirectChild(Transform parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var result = FindChild(root.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void EnsurePackageBackgroundBehindCover(
        Image backgroundImage,
        Image coverImage)
    {
        if (backgroundImage == null || coverImage == null)
        {
            return;
        }

        var backgroundTransform = backgroundImage.rectTransform;
        var coverTransform = coverImage.rectTransform;
        if (backgroundTransform.parent != coverTransform.parent
            || backgroundTransform.GetSiblingIndex() < coverTransform.GetSiblingIndex())
        {
            return;
        }

        backgroundTransform.SetSiblingIndex(coverTransform.GetSiblingIndex());
    }

    private static void SetPackageVisualsVisible(PackageEntry entry, bool visible)
    {
        if (entry == null)
        {
            return;
        }

        entry.SuppressDisplay = !visible;
        SetPackageCoverVisible(entry, visible);
        SetPackageBackgroundVisible(entry, visible);
        SetPackageSizeImageVisible(entry, visible);
        SetPackageProgressPiecesVisible(entry, visible);
    }

    private void SetUnselectedPackageVisualsVisible(bool visible)
    {
        foreach (var pair in mPackageSlotsById)
        {
            var entry = pair.Value;
            if (entry != null && entry != mSelectedPackageEntry)
            {
                SetPackageVisualsVisible(entry, visible);
            }
        }
    }

    private IEnumerator ShowPackageSelection(int bagId, PackageEntry entry)
    {
        mIsPlayingAnimation = true;
        mSelectedPackageEntry = entry;
        mSelectedBagId = bagId;
        var anchor = entry.Image != null ? entry.Image.rectTransform : entry.RectTransform;
        Canvas.ForceUpdateCanvases();
        if (!TryGetSelectedOverlayRect(
                anchor,
                out mSelectedPackageStartPosition,
                out mSelectedPackageStartSize))
        {
            mSelectedPackageStartPosition = Vector2.zero;
            mSelectedPackageStartSize = new Vector2(PackageCoverWidth, PackageCoverHeight);
        }

        var panelRect = mBagSelectPanelRoot != null
            ? mBagSelectPanelRoot.transform as RectTransform
            : null;
        if (!TryGetSelectedOverlayRect(panelRect, out mSelectedPackageDisplayPosition, out _))
        {
            mSelectedPackageDisplayPosition = Vector2.zero;
        }

        mSelectedPackageDisplaySize = new Vector2(PackageOpenWidth, PackageOpenHeight);
        var didCreateSelectedVisual = CreateSelectedPackageVisual(entry);
        SetPackageVisualsVisible(entry, false);
        yield return CaptureBagSelectBackdrop();
        if (!didCreateSelectedVisual
            || mSelectedPackageOverlayImage == null
            || mSelectedPackageOverlayRect == null)
        {
            Debug.LogWarning($"MainScene: static card pack selection image is unavailable. packId={bagId}");
            SetBagSelectBackdropVisible(false);
            ReleaseBagSelectBackdropTexture();
            SetBagSelectPanelVisible(false);
            SetPackageVisualsVisible(entry, true);
            ClearPackageSelection();
            mIsPlayingAnimation = false;
            mPlayAnimationCoroutine = null;
            mHasSwitchedToGameScene = true;
            GameManager.EnterGameScene(bagId);
            yield break;
        }

        mSelectedPackageOverlayRect.anchoredPosition = mSelectedPackageStartPosition;
        SetSelectedPackageVisualSize(mSelectedPackageStartSize);
        SetSelectedPackageImageVisible(true);
        SyncSelectedPackageAnimator(entry);
        RefreshBagSelectPackState(bagId);
        SetBagSelectBackdropVisible(true);
        SetBagSelectButtonEntranceProgress(0f);
        SetBagSelectPanelVisible(true);
        SetBagSelectButtonsInteractable(false);
        var buttonEntranceAnimation = StartCoroutine(AnimateBagSelectButtons(entering: true));
        yield return AnimateSelectedPackageImage(
            mSelectedPackageStartPosition,
            mSelectedPackageDisplayPosition,
            mSelectedPackageStartSize,
            mSelectedPackageDisplaySize,
            PackageOpenScaleDuration);
        yield return buttonEntranceAnimation;
        SetBagSelectButtonsInteractable(true);

        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
    }

    private IEnumerator CaptureBagSelectBackdrop()
    {
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        if (mBagSelectBackdropImage == null)
        {
            yield break;
        }

        var wasCursorVisible = Cursor.visible;
        Texture2D screenshot;
        Cursor.visible = false;
        try
        {
            yield return new WaitForEndOfFrame();
            screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        }
        finally
        {
            Cursor.visible = wasCursorVisible;
        }

        if (screenshot == null)
        {
            yield break;
        }

        var blurWidth = screenshot.width;
        var blurHeight = screenshot.height;
        RenderTexture blurSource = null;
        RenderTexture horizontalBlur = null;
        try
        {
            blurSource = RenderTexture.GetTemporary(
                blurWidth,
                blurHeight,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);
            horizontalBlur = RenderTexture.GetTemporary(
                blurWidth,
                blurHeight,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);
            blurSource.filterMode = FilterMode.Bilinear;
            blurSource.wrapMode = TextureWrapMode.Clamp;
            horizontalBlur.filterMode = FilterMode.Bilinear;
            horizontalBlur.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(screenshot, blurSource);

            mBagSelectBackdropTexture = new RenderTexture(
                blurWidth,
                blurHeight,
                0,
                RenderTextureFormat.ARGB32)
            {
                name = "BagSelectGaussianBlurredBackdropTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            mBagSelectBackdropTexture.Create();

            if (TryGetBagSelectBlurMaterial(out var blurMaterial))
            {
                var sampleScale = BagSelectGaussianBlurRadius / 8f;
                blurMaterial.SetFloat("_SampleScale", sampleScale);
                blurMaterial.SetVector(
                    "_BlurDirection",
                    new Vector4(1f, 0f, 0f, 0f));
                blurMaterial.SetFloat("_ConvertOutputToLinear", 0f);
                Graphics.Blit(blurSource, horizontalBlur, blurMaterial);
                blurMaterial.SetVector(
                    "_BlurDirection",
                    new Vector4(0f, 1f, 0f, 0f));
                blurMaterial.SetFloat(
                    "_ConvertOutputToLinear",
                    QualitySettings.activeColorSpace == ColorSpace.Linear
                        ? 1f
                        : 0f);
                Graphics.Blit(
                    horizontalBlur,
                    mBagSelectBackdropTexture,
                    blurMaterial);
            }
            else
            {
                Graphics.Blit(blurSource, mBagSelectBackdropTexture);
            }
        }
        finally
        {
            if (horizontalBlur != null)
            {
                RenderTexture.ReleaseTemporary(horizontalBlur);
            }

            if (blurSource != null)
            {
                RenderTexture.ReleaseTemporary(blurSource);
            }

            Destroy(screenshot);
        }

        mBagSelectBackdropImage.texture = mBagSelectBackdropTexture;
        mBagSelectBackdropImage.color = Color.white;
    }

    private bool TryGetBagSelectBlurMaterial(out Material blurMaterial)
    {
        if (mBagSelectBlurMaterial != null)
        {
            blurMaterial = mBagSelectBlurMaterial;
            return true;
        }

        var blurShader = Resources.Load<Shader>(BagSelectBlurShaderResourcePath);
        if (blurShader == null || !blurShader.isSupported)
        {
            Debug.LogWarning(
                "MainScene: BagSelectGaussianBlur shader is missing or unsupported; using the downsampled backdrop without Gaussian blur.");
            blurMaterial = null;
            return false;
        }

        mBagSelectBlurMaterial = new Material(blurShader)
        {
            name = "BagSelectGaussianBlurRuntimeMaterial",
            hideFlags = HideFlags.HideAndDontSave
        };
        blurMaterial = mBagSelectBlurMaterial;
        return true;
    }

    private bool TryCreatePhotoTexture(int bagId, out Texture2D photoTexture)
    {
        photoTexture = null;
        if (mPhotoBackgroundSprite == null)
        {
            Debug.LogError("MainScene: photo background Sprite is missing from PanelPhoto/Photo.");
            return false;
        }

        var cardBagPrefab = Resources.Load<GameObject>(
            GameDefine.FormatCardBagPrefabResourcesPath(bagId));
        if (cardBagPrefab == null)
        {
            Debug.LogError(
                $"MainScene: photo capture CardBag prefab not found. bagId={bagId}");
            return false;
        }

        GameObject cameraObject = null;
        GameObject canvasObject = null;
        GameObject cardBagObject = null;
        RenderTexture renderTexture = null;
        var previousRenderTexture = RenderTexture.active;
        try
        {
            renderTexture = RenderTexture.GetTemporary(
                PhotoOutputSize,
                PhotoOutputSize,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            renderTexture.name = "CardBagPhotoRenderTexture";
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Clamp;

            cameraObject = new GameObject("CardBagPhotoCamera", typeof(Camera));
            cameraObject.layer = PhotoCaptureLayer;
            var photoCamera = cameraObject.GetComponent<Camera>();
            photoCamera.enabled = false;
            photoCamera.orthographic = true;
            photoCamera.orthographicSize = 5f;
            photoCamera.clearFlags = CameraClearFlags.SolidColor;
            photoCamera.backgroundColor = Color.black;
            photoCamera.cullingMask = 1 << PhotoCaptureLayer;
            photoCamera.allowHDR = false;
            photoCamera.allowMSAA = true;
            photoCamera.targetTexture = renderTexture;
            photoCamera.transform.position = new Vector3(0f, 0f, -10f);

            canvasObject = new GameObject(
                "CardBagPhotoCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.layer = PhotoCaptureLayer;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = photoCamera;
            canvas.planeDistance = 1f;
            canvas.pixelPerfect = false;
            var canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(PhotoOutputSize, PhotoOutputSize);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            var background = CreatePhotoCaptureImage(
                canvas.transform,
                "Background",
                mPhotoBackgroundSprite,
                new Vector2(PhotoOutputSize, PhotoOutputSize),
                Vector2.zero);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            cardBagObject = Instantiate(cardBagPrefab, canvas.transform, false);
            cardBagObject.name = $"PhotoCardBag{bagId:D3}";
            SetLayerRecursively(cardBagObject.transform, PhotoCaptureLayer);
            SetPhotoCardBagComplete(cardBagObject);
            var cardBagRect = cardBagObject.GetComponent<RectTransform>();
            var gameBoardTransform = FindChild(
                cardBagObject.transform,
                GameDefine.GameBoardObjectName) as RectTransform;
            if (cardBagRect == null || gameBoardTransform == null)
            {
                Debug.LogError(
                    $"MainScene: photo capture prefab is missing RectTransform/GameBoard. bagId={bagId}");
                return false;
            }

            var boardSize = gameBoardTransform.rect.size;
            if (boardSize.x <= 0f || boardSize.y <= 0f)
            {
                boardSize = gameBoardTransform.sizeDelta;
            }

            var rotationRadians = PhotoPuzzleRotation * Mathf.Deg2Rad;
            var cosine = Mathf.Abs(Mathf.Cos(rotationRadians));
            var sine = Mathf.Abs(Mathf.Sin(rotationRadians));
            var rotatedWidth = boardSize.x * cosine + boardSize.y * sine;
            var rotatedHeight = boardSize.x * sine + boardSize.y * cosine;
            var boardScale = Mathf.Min(
                PhotoPuzzleMaxSize / Mathf.Max(1f, rotatedWidth),
                PhotoPuzzleMaxSize / Mathf.Max(1f, rotatedHeight));
            cardBagRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardBagRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardBagRect.pivot = new Vector2(0.5f, 0.5f);
            cardBagRect.anchoredPosition = new Vector2(0f, PhotoPuzzleOffsetY);
            cardBagRect.localRotation = Quaternion.Euler(0f, 0f, PhotoPuzzleRotation);
            cardBagRect.localScale = Vector3.one * boardScale;

            var boardImage = gameBoardTransform.GetComponent<Image>();
            if (boardImage != null && boardImage.GetComponent<Shadow>() == null)
            {
                var boardShadow = boardImage.gameObject.AddComponent<Shadow>();
                boardShadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
                boardShadow.effectDistance = new Vector2(18f, -24f);
                boardShadow.useGraphicAlpha = true;
            }

            if (mPhotoGameIconSprite != null)
            {
                CreatePhotoCaptureImage(
                    canvas.transform,
                    PhotoGameIconObjectName,
                    mPhotoGameIconSprite,
                    new Vector2(145f, 139f),
                    new Vector2(-410f, -400f));
            }

            Canvas.ForceUpdateCanvases();
            photoCamera.targetTexture = renderTexture;
            photoCamera.Render();
            photoCamera.targetTexture = null;
            RenderTexture.active = renderTexture;
            photoTexture = new Texture2D(
                PhotoOutputSize,
                PhotoOutputSize,
                TextureFormat.RGBA32,
                false,
                false)
            {
                name = $"CardBagPhoto{bagId:D3}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            photoTexture.ReadPixels(
                new Rect(0f, 0f, PhotoOutputSize, PhotoOutputSize),
                0,
                0,
                false);
            photoTexture.Apply(false, false);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"MainScene: photo capture failed. {exception}");
            if (photoTexture != null)
            {
                Destroy(photoTexture);
                photoTexture = null;
            }

            return false;
        }
        finally
        {
            RenderTexture.active = previousRenderTexture;
            if (renderTexture != null)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            if (cameraObject != null)
            {
                Destroy(cameraObject);
            }

            if (canvasObject != null)
            {
                Destroy(canvasObject);
            }
        }
    }

    private static Image CreatePhotoCaptureImage(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 size,
        Vector2 anchoredPosition)
    {
        var imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = PhotoCaptureLayer;
        var rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        var image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static void SetPhotoCardBagComplete(GameObject cardBagObject)
    {
        var transforms = cardBagObject.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.SetActive(true);
        }

        var images = cardBagObject.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image.sprite != null)
            {
                var color = image.color;
                color.a = 1f;
                image.color = color;
            }

            image.raycastTarget = false;
        }
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.layer = layer;
        for (var i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private static bool TrySavePhotoToDesktop(
        Texture2D photoTexture,
        int bagId,
        out string savedPath)
    {
        savedPath = null;
        if (photoTexture == null)
        {
            return false;
        }

        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrWhiteSpace(desktopPath) || !Directory.Exists(desktopPath))
            {
                Debug.LogError($"MainScene: desktop folder is unavailable. path={desktopPath}");
                return false;
            }

            var productName = SanitizeFileName(Application.productName);
            var fileName = $"{productName}-{DateTime.Now:yyyy-MM-dd}-{bagId:D3}.png";
            savedPath = Path.Combine(desktopPath, fileName);
            File.WriteAllBytes(savedPath, photoTexture.EncodeToPNG());
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"MainScene: failed to save photo to desktop. {exception}");
            savedPath = null;
            return false;
        }
    }

    private static string SanitizeFileName(string value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value) ? "Puffies" : value.Trim();
        var invalidCharacters = Path.GetInvalidFileNameChars();
        for (var i = 0; i < invalidCharacters.Length; i++)
        {
            safeValue = safeValue.Replace(invalidCharacters[i], '_');
        }

        return safeValue;
    }

    private void ApplyGeneratedPhoto(Texture2D photoTexture)
    {
        ReleaseGeneratedPhoto();
        mGeneratedPhotoTexture = photoTexture;
        mGeneratedPhotoSprite = Sprite.Create(
            photoTexture,
            new Rect(0f, 0f, photoTexture.width, photoTexture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        mGeneratedPhotoSprite.name = photoTexture.name + "Sprite";
        mPhotoImage.sprite = mGeneratedPhotoSprite;
        mPhotoImage.color = Color.white;
        mPhotoImage.preserveAspect = true;
    }

    private void ReleaseGeneratedPhoto()
    {
        if (mPhotoImage != null && mPhotoImage.sprite == mGeneratedPhotoSprite)
        {
            mPhotoImage.sprite = mPhotoBackgroundSprite;
        }

        if (mGeneratedPhotoSprite != null)
        {
            Destroy(mGeneratedPhotoSprite);
            mGeneratedPhotoSprite = null;
        }

        if (mGeneratedPhotoTexture != null)
        {
            Destroy(mGeneratedPhotoTexture);
            mGeneratedPhotoTexture = null;
        }
    }

    private IEnumerator EnterCardPackOpeningStage()
    {
        StopOpeningHintAnimation();
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        StartCoroutine(GameManager.PreloadGameScene(mSelectedBagId));
        yield return PlayMainToGameBackgroundHandoff();
        mSelectedPackageStageSize = mSelectedPackageDisplaySize;
        StartOpeningHintAnimation();
        mIsPlayingAnimation = false;
        mIsAwaitingTearSwipe = true;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        mPlayAnimationCoroutine = null;
    }

    private IEnumerator PlayTornPackageGameTransition(
        bool isReplaySession,
        bool backgroundHandoffCompleted = false,
        bool piecesReadyToDealImmediately = false,
        bool openingEffectReadyForSceneHandoff = false)
    {
        StopOpeningHintAnimation();
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        StartCoroutine(GameManager.PreloadGameScene(mSelectedBagId));
        if (!backgroundHandoffCompleted)
        {
            yield return PlayMainToGameBackgroundHandoff();
        }

        var isTornPackTransition = !openingEffectReadyForSceneHandoff;
        var shouldRetractProgressPieces = isTornPackTransition && !isReplaySession;
        var holdElapsed = 0f;
        var requiredHoldDuration = openingEffectReadyForSceneHandoff
            ? 0f
            : isTornPackTransition
                ? Mathf.Max(
                    0f,
                    InProgressGameTransitionHoldDuration
                    - TornPackTransitionHoldReduction)
                : InProgressGameTransitionHoldDuration;
        while (holdElapsed < requiredHoldDuration
               || (!GameManager.IsGameSceneReadyForActivation(mSelectedBagId)
                   && holdElapsed < InProgressGameTransitionPreloadTimeout))
        {
            holdElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (openingEffectReadyForSceneHandoff)
        {
            mCardPackOpeningEffect?.PrepareForSceneHandoff();
            mPlayAnimationCoroutine = null;
            mHasSwitchedToGameScene = true;
            GameManager.EnterGameScene(
                mSelectedBagId,
                playEntranceAnimation: true,
                isReplaySession: isReplaySession,
                entrancePiecesAlreadyFanned: true);
            yield break;
        }

        var packExitDuration = isTornPackTransition
            ? InProgressPackExitDuration / TornPackExitSpeedMultiplier
            : InProgressPackExitDuration;
        var retractProgressPiecesOnDrop = shouldRetractProgressPieces;
        StoreOpeningPackExitPosition();
        var transitionPieceRects = new List<RectTransform>();
        if (mSelectedPackageProgressPieceAnimations != null)
        {
            for (var i = 0; i < mSelectedPackageProgressPieceAnimations.Count; i++)
            {
                var pieceRect = mSelectedPackageProgressPieceAnimations[i]?.RectTransform;
                if (pieceRect != null)
                {
                    transitionPieceRects.Add(pieceRect);
                }
            }
        }

        if (CardPackGameEntranceTransition.TryBegin(
                mSelectedPackageOverlayCanvas,
                mSelectedPackageOverlayRect,
                mSelectedPackageOverlayImage,
                transitionPieceRects,
                ReferenceHeight * InProgressPackExitScreenHeightRatio,
                InProgressPieceExitHorizontalSpread,
                InProgressPieceExitCompensation,
                packExitDuration,
                retractProgressPiecesOnDrop,
                useContinuousLinearDrop: isTornPackTransition))
        {
            mSelectedPackageOverlayCanvas = null;
            mSelectedPackageOverlayCanvasGroup = null;
            mSelectedPackageOverlayImage = null;
            mSelectedPackageOverlayRect = null;
            mSelectedPackageVisualContent = null;
            mSelectedPackageProgressPieceAnimations = null;
            mIsSelectedPackageProgressPieceTransitioning = false;
            mPlayAnimationCoroutine = null;
            mHasSwitchedToGameScene = true;
            GameManager.EnterGameScene(
                mSelectedBagId,
                playEntranceAnimation: true,
                isReplaySession: isReplaySession,
                entrancePiecesAlreadyFanned: piecesReadyToDealImmediately);
            yield break;
        }

        var packRect = mSelectedPackageOverlayRect;
        var packStart = packRect != null
            ? packRect.anchoredPosition
            : mSelectedPackageDisplayPosition;
        var packDropDistance = ReferenceHeight * InProgressPackExitScreenHeightRatio;
        var packTarget = packStart + Vector2.down * packDropDistance;
        var pieceAnimations = mSelectedPackageProgressPieceAnimations;
        var pieceStarts = pieceAnimations != null
            ? new Vector2[pieceAnimations.Count]
            : Array.Empty<Vector2>();
        var pieceTargets = new Vector2[pieceStarts.Length];
        for (var i = 0; i < pieceStarts.Length; i++)
        {
            var animation = pieceAnimations[i];
            if (animation?.RectTransform == null)
            {
                continue;
            }

            pieceStarts[i] = animation.RectTransform.anchoredPosition;
            var pieceRect = animation.RectTransform;
            var pieceParent = pieceRect.parent as RectTransform;
            var centeredIndex = i - (pieceStarts.Length - 1) * 0.5f;
            pieceTargets[i] = shouldRetractProgressPieces
                ? pieceStarts[i] + Vector2.down
                    * (pieceParent != null
                        ? pieceParent.rect.height
                        : packRect != null ? packRect.rect.height : ReferenceHeight)
                    * 0.28f
                : isTornPackTransition
                    ? pieceStarts[i]
                : pieceStarts[i] + new Vector2(
                    centeredIndex * InProgressPieceExitHorizontalSpread,
                    packDropDistance * InProgressPieceExitCompensation);
        }

        mIsSelectedPackageProgressPieceTransitioning = true;
        var exitElapsed = 0f;
        while (exitElapsed < packExitDuration)
        {
            exitElapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(exitElapsed / packExitDuration);
            var packT = isTornPackTransition
                ? normalized
                : Mathf.SmoothStep(0f, 1f, normalized);
            var pieceT = 1f - Mathf.Pow(1f - normalized, 3f);
            if (packRect != null)
            {
                packRect.anchoredPosition = Vector2.LerpUnclamped(
                    packStart,
                    packTarget,
                    packT);
            }

            for (var i = 0; i < pieceStarts.Length; i++)
            {
                var pieceRect = pieceAnimations[i]?.RectTransform;
                if (pieceRect != null)
                {
                    if (shouldRetractProgressPieces)
                    {
                        pieceRect.anchoredPosition = Vector2.LerpUnclamped(
                            pieceStarts[i],
                            pieceTargets[i],
                            Mathf.SmoothStep(0f, 1f, normalized));
                        if (normalized >= 1f)
                        {
                            pieceRect.gameObject.SetActive(false);
                        }
                    }
                    else if (isTornPackTransition)
                    {
                        continue;
                    }
                    else
                    {
                        pieceRect.anchoredPosition = Vector2.LerpUnclamped(
                            pieceStarts[i],
                            pieceTargets[i],
                            pieceT);
                    }
                }
            }

            yield return null;
        }

        if (packRect != null)
        {
            packRect.anchoredPosition = packTarget;
        }

        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(
            mSelectedBagId,
            playEntranceAnimation: true,
            isReplaySession: isReplaySession,
            entrancePiecesAlreadyFanned: piecesReadyToDealImmediately);
    }

    private IEnumerator PlayMainToGameBackgroundHandoff()
    {
        var gameBackgroundCenter = Vector3.zero;
        if (mOpeningStageBackgroundRoot != null)
        {
            FitOpeningStageBackgroundToCamera();
            mOpeningStageBackgroundRoot.SetActive(true);
            SetOpeningStageBackgroundAlpha(0f);
            gameBackgroundCenter = mOpeningStageBackgroundRoot.transform.position;
            mOpeningStageBackgroundRoot.transform.position = gameBackgroundCenter;
        }

        SetBagSelectBackdropAlpha(1f);

        if (mMainCanvasGroup != null)
        {
            mMainCanvasGroup.alpha = 1f;
            mMainCanvasGroup.interactable = false;
            mMainCanvasGroup.blocksRaycasts = false;
        }

        if (mBagSelectPanelCanvasGroup != null)
        {
            mBagSelectPanelCanvasGroup.alpha = 1f;
            mBagSelectPanelCanvasGroup.interactable = false;
            mBagSelectPanelCanvasGroup.blocksRaycasts = false;
        }

        SetBagSelectButtonEntranceProgress(1f);
        var elapsed = 0f;
        while (elapsed < OpeningStageTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / OpeningStageTransitionDuration);
            var eased = 1f - Mathf.Pow(1f - normalized, 3f);
            var buttonNormalized = Mathf.Clamp01(
                elapsed / BagSelectButtonSlideDuration);
            SetBagSelectButtonEntranceProgress(
                1f - Mathf.Pow(buttonNormalized, 3f));

            if (mMainCanvasGroup != null)
            {
                mMainCanvasGroup.alpha = 1f - eased;
            }

            SetBagSelectBackdropAlpha(1f - eased);
            SetOpeningStageBackgroundAlpha(eased);

            yield return null;
        }

        SetBagSelectButtonEntranceProgress(0f);
        SetBagSelectBackdropAlpha(0f);
        if (mOpeningStageBackgroundRoot != null)
        {
            mOpeningStageBackgroundRoot.transform.position = gameBackgroundCenter;
            SetOpeningStageBackgroundAlpha(1f);
        }

        SetBagSelectPanelVisible(false);
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        if (mMainCanvasGroup != null)
        {
            mMainCanvasGroup.alpha = 0f;
        }
    }

    private IEnumerator PlaySelectedPackage()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        var selectedBagId = mSelectedBagId;
        var isReplaySession = mIsSelectedPackageReplay;
        StoreOpeningPackExitPosition();

        var openingEffectStarted = false;
        var openingPieceSprites = LoadCurrentGroupUnplacedPieceSprites(selectedBagId);
        var packTexture = mSelectedPackageOverlayImage != null
            && mSelectedPackageOverlayImage.sprite != null
            ? mSelectedPackageOverlayImage.sprite.texture
            : null;
        if (packTexture != null
            && mSelectedPackageOverlayRect != null
            && mBagSelectOverlayCanvas != null)
        {
            if (mCardPackOpeningEffect == null)
            {
                mCardPackOpeningEffect = CardPackOpeningEffect.Attach(
                    mBagSelectOverlayCanvas.transform);
            }

            openingEffectStarted = mCardPackOpeningEffect != null
                && mCardPackOpeningEffect.Begin(
                    packTexture,
                    mSelectedPackageOverlayRect,
                    mSelectedPackageVisualContent != null
                        ? mSelectedPackageVisualContent.gameObject
                        : mSelectedPackageOverlayImage.gameObject,
                    openingPieceSprites);
        }

        SetBagSelectPanelVisible(false);
        if (openingEffectStarted)
        {
            yield return new WaitForEndOfFrame();
            mCardPackOpeningEffect.StartPlayback();
            yield return mCardPackOpeningEffect.WaitForGameEntranceHandoff();
        }
        else
        {
            SetSelectedPackageImageVisible(false);
            SetSelectedPackageImageAlpha(1f);
            Debug.LogWarning(
                $"MainScene: card pack opening effect unavailable; entering game directly. "
                + $"packId={selectedBagId}");
            yield return null;
        }

        var emergedPieceOrigin = default(Vector2);
        var piecesReadyToDealImmediately = openingEffectStarted
            && mCardPackOpeningEffect.TryGetEmergedPieceScreenOrigin(
                out emergedPieceOrigin);
        if (piecesReadyToDealImmediately)
        {
            GameManager.SetOpeningPackExitPosition(emergedPieceOrigin);
        }

        if (mSelectedPackageVisualContent != null)
        {
            mSelectedPackageVisualContent.gameObject.SetActive(false);
        }

        ClearSelectedPackageVisual();
        yield return PlayTornPackageGameTransition(
            isReplaySession,
            backgroundHandoffCompleted: true,
            piecesReadyToDealImmediately: piecesReadyToDealImmediately,
            openingEffectReadyForSceneHandoff: openingEffectStarted);
    }

    private static List<Sprite> LoadCurrentGroupUnplacedPieceSprites(int packId)
    {
        var sprites = new List<Sprite>();
        var cardBagPrefab = Resources.Load<GameObject>(
            GameDefine.FormatCardBagPrefabResourcesPath(packId));
        if (cardBagPrefab == null)
        {
            Debug.LogWarning(
                $"MainScene: opening pieces skipped; CardBag prefab not found. packId={packId}");
            return sprites;
        }

        CardPackDataUtility.TryGetPlacedPieceNumbers(packId, out var placedPieceNumbers);
        var spritesByGroup = new SortedDictionary<int, List<KeyValuePair<int, Sprite>>>();
        var pieceImages = cardBagPrefab.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < pieceImages.Length; i++)
        {
            var pieceImage = pieceImages[i];
            if (pieceImage == null
                || pieceImage.sprite == null
                || !GameDefine.TryParsePieceObjectName(
                    pieceImage.gameObject.name,
                    out var groupNumber,
                    out var indexInGroup,
                    out var pieceNumber)
                || placedPieceNumbers.Contains(pieceNumber))
            {
                continue;
            }

            if (!spritesByGroup.TryGetValue(groupNumber, out var groupSprites))
            {
                groupSprites = new List<KeyValuePair<int, Sprite>>();
                spritesByGroup[groupNumber] = groupSprites;
            }

            groupSprites.Add(new KeyValuePair<int, Sprite>(indexInGroup, pieceImage.sprite));
        }

        foreach (var groupEntry in spritesByGroup)
        {
            var groupSprites = groupEntry.Value;
            if (groupSprites == null || groupSprites.Count == 0)
            {
                continue;
            }

            groupSprites.Sort((left, right) => left.Key.CompareTo(right.Key));
            for (var i = 0; i < groupSprites.Count; i++)
            {
                sprites.Add(groupSprites[i].Value);
            }

            break;
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning(
                $"MainScene: opening pieces skipped; no unplaced group remains. packId={packId}");
        }

        return sprites;
    }

    private void StoreOpeningPackExitPosition()
    {
        if (mSelectedPackageOverlayRect == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        var corners = new Vector3[4];
        mSelectedPackageOverlayRect.GetWorldCorners(corners);
        var camera = ResolveCanvasCamera(mSelectedPackageOverlayRect);
        var bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        var packCenter = (bottomLeft + topRight) * 0.5f;
        GameManager.SetOpeningPackExitPosition(new Vector2(
            packCenter.x / Screen.width,
            packCenter.y / Screen.height));
    }

    private IEnumerator HidePackageSelection()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        var buttonExitAnimation = StartCoroutine(AnimateBagSelectButtons(entering: false));
        yield return AnimateSelectedPackageImage(
            mSelectedPackageDisplayPosition,
            mSelectedPackageStartPosition,
            mSelectedPackageDisplaySize,
            mSelectedPackageStartSize);
        yield return buttonExitAnimation;

        var selectedEntry = mSelectedPackageEntry;
        SetBagSelectPanelVisible(false);
        SetSelectedPackageImageVisible(false);
        SetPackageVisualsVisible(selectedEntry, true);
        ClearPackageSelection();
        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
    }

    private void CacheBagSelectButtonPositions()
    {
        var buttonRects = new List<RectTransform>(3);
        if (mBagSelectBackButton != null
            && mBagSelectBackButton.transform is RectTransform backRect)
        {
            buttonRects.Add(backRect);
        }

        if (mBagSelectPlayButton != null
            && mBagSelectPlayButton.transform is RectTransform playRect)
        {
            buttonRects.Add(playRect);
        }

        if (mBagSelectCameraButtonRoot != null
            && mBagSelectCameraButtonRoot.transform is RectTransform cameraRect)
        {
            buttonRects.Add(cameraRect);
        }

        mBagSelectButtonRects = buttonRects.ToArray();
        mBagSelectButtonPositions = new Vector2[mBagSelectButtonRects.Length];
        for (var i = 0; i < mBagSelectButtonRects.Length; i++)
        {
            mBagSelectButtonPositions[i] = mBagSelectButtonRects[i].anchoredPosition;
        }
    }

    private void SetBagSelectButtonEntranceProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        var offscreenY = ResolveBagSelectButtonOffscreenY();
        for (var i = 0; i < mBagSelectButtonRects.Length; i++)
        {
            SetBagSelectButtonEntrancePosition(
                mBagSelectButtonRects[i],
                mBagSelectButtonPositions[i],
                offscreenY,
                progress);
        }
    }

    private float ResolveBagSelectButtonOffscreenY()
    {
        var panelRect = mBagSelectPanelRoot != null
            ? mBagSelectPanelRoot.transform as RectTransform
            : null;
        var maximumButtonHeight = 0f;
        for (var i = 0; i < mBagSelectButtonRects.Length; i++)
        {
            maximumButtonHeight = Mathf.Max(
                maximumButtonHeight,
                mBagSelectButtonRects[i].rect.height);
        }

        var panelBottom = panelRect != null
            ? panelRect.rect.yMin
            : -GameDefine.DesignHeight * 0.5f;
        return panelBottom
               - maximumButtonHeight * 0.5f
               - BagSelectButtonEntranceBottomMargin;
    }

    private static void SetBagSelectButtonEntrancePosition(
        RectTransform buttonRect,
        Vector2 finalPosition,
        float offscreenY,
        float progress)
    {
        if (buttonRect == null)
        {
            return;
        }

        finalPosition.y = Mathf.LerpUnclamped(offscreenY, finalPosition.y, progress);
        buttonRect.anchoredPosition = finalPosition;
    }

    private void SetBagSelectButtonsInteractable(bool interactable)
    {
        SetBagSelectButtonInteractable(mBagSelectPlayButton, interactable);
        SetBagSelectButtonInteractable(mBagSelectBackButton, interactable);
        SetBagSelectButtonInteractable(mBagSelectCameraButton, interactable);
    }

    private static void SetBagSelectButtonInteractable(Button button, bool interactable)
    {
        if (button == null)
        {
            return;
        }

        var colors = button.colors;
        colors.disabledColor = colors.normalColor;
        button.colors = colors;
        button.interactable = interactable;
    }

    private void RefreshBagSelectPackState(int bagId)
    {
        var isCompleted = CardPackDataUtility.IsPackCompleted(bagId);
        var shouldConfirmReplay = mSelectedPackageEntry != null
            && mSelectedPackageEntry.DisplayState == PackageDisplayState.TornCompleted;
        if (mBagSelectPlayLabel != null)
        {
            mBagSelectPlayLabel.text = shouldConfirmReplay
                ? BagSelectReplayActionText
                : BagSelectNewPackActionText;
        }

        if (mBagSelectCameraButtonRoot != null)
        {
            mBagSelectCameraButtonRoot.SetActive(isCompleted);
        }
    }

    private void SetBagSelectPanelVisible(bool visible)
    {
        if (!visible)
        {
            SetBagSelectButtonEntranceProgress(1f);
        }

        if (visible && mBagSelectPanelCanvasGroup != null)
        {
            mBagSelectPanelCanvasGroup.alpha = 1f;
            mBagSelectPanelCanvasGroup.interactable = true;
            mBagSelectPanelCanvasGroup.blocksRaycasts = true;
        }

        SetPanelVisible(mBagSelectPanelRoot, visible);
    }

    private void SetBagSelectBackdropVisible(bool visible)
    {
        if (mBagSelectBackdropImage != null)
        {
            mBagSelectBackdropImage.gameObject.SetActive(
                visible && mBagSelectBackdropTexture != null);
        }
    }

    private void SetBagSelectBackdropAlpha(float alpha)
    {
        if (mBagSelectBackdropImage == null)
        {
            return;
        }

        var color = mBagSelectBackdropImage.color;
        color.a = Mathf.Clamp01(alpha);
        mBagSelectBackdropImage.color = color;
    }

    private void SetOpeningStageBackgroundAlpha(float alpha)
    {
        if (mOpeningStageBackgroundRenderer == null)
        {
            return;
        }

        var color = mOpeningStageBackgroundRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        mOpeningStageBackgroundRenderer.color = color;
    }

    private void ReleaseBagSelectBackdropTexture()
    {
        if (mBagSelectBackdropImage != null)
        {
            mBagSelectBackdropImage.texture = null;
        }

        if (mBagSelectBackdropTexture == null)
        {
            return;
        }

        mBagSelectBackdropTexture.Release();
        Destroy(mBagSelectBackdropTexture);
        mBagSelectBackdropTexture = null;
    }

    private bool TryRefreshTearSwipeScreenRect()
    {
        if (mSelectedPackageOverlayRect == null
            || !mSelectedPackageOverlayRect.gameObject.activeInHierarchy)
        {
            return false;
        }

        mTearSwipeScreenRect = GetScreenRect(
            mSelectedPackageOverlayRect,
            ResolveCanvasCamera(mSelectedPackageOverlayRect));
        if (mTearSwipeScreenRect.width <= 0.001f
            || mTearSwipeScreenRect.height <= 0.001f)
        {
            return false;
        }

        return true;
    }

    private void OnTearSwipeBegin(Vector2 screenPosition)
    {
        if (!mIsAwaitingTearSwipe
            || !TryRefreshTearSwipeScreenRect())
        {
            return;
        }

        mIsTrackingTearTap = mTearSwipeScreenRect.Contains(screenPosition);
        mIsTrackingTearSwipe = mIsTrackingTearTap;
        if (!mIsTrackingTearTap)
        {
            return;
        }

        mTearSwipeStartScreenPosition = screenPosition;
    }

    private void OnTearSwipeMove(Vector2 screenPosition)
    {
        if (!mIsTrackingTearSwipe)
        {
            return;
        }

        if (!mTearSwipeScreenRect.Contains(screenPosition))
        {
            mIsTrackingTearSwipe = false;
            mIsTrackingTearTap = false;
            return;
        }

        var minimumSwipeTravel = Mathf.Max(
            TearGestureMinTravelPixels,
            Mathf.Min(mTearSwipeScreenRect.width, mTearSwipeScreenRect.height)
                * TearGestureTravelRatio);
        if (Vector2.Distance(screenPosition, mTearSwipeStartScreenPosition) < minimumSwipeTravel)
        {
            return;
        }

        mIsTrackingTearTap = false;
    }

    private void OnTearSwipeEnd(Vector2 screenPosition)
    {
        if (!mIsTrackingTearSwipe && !mIsTrackingTearTap)
        {
            return;
        }

        OnTearSwipeMove(screenPosition);
        if (!mIsAwaitingTearSwipe)
        {
            return;
        }

        var shouldOpenFromTap = mIsTrackingTearTap
            && mTearSwipeScreenRect.Contains(screenPosition);
        var shouldOpenFromSwipe = mIsTrackingTearSwipe
            && !mIsTrackingTearTap
            && mTearSwipeScreenRect.Contains(screenPosition);
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        if (shouldOpenFromTap || shouldOpenFromSwipe)
        {
            CompleteTearSwipe();
        }
    }

    private void CompleteTearSwipe()
    {
        if (!mIsAwaitingTearSwipe || mIsPlayingAnimation)
        {
            return;
        }

        StopOpeningHintAnimation();
        mIsAwaitingTearSwipe = false;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        mPlayAnimationCoroutine = StartCoroutine(PlaySelectedPackage());
    }

    private void ClearPackageSelection()
    {
        StopOpeningHintAnimation();
        if (mReplayPanelRoot != null)
        {
            SetPanelVisible(mReplayPanelRoot, false);
        }

        mIsReplayConfirmationVisible = false;
        mIsSelectedPackageReplay = false;
        SetSelectedPackageImageVisible(false);
        ClearSelectedPackageVisual();

        mSelectedPackageEntry = null;
        mSelectedBagId = 0;
        mSelectedPackageStartPosition = default;
        mSelectedPackageStartSize = default;
        mSelectedPackageDisplayPosition = default;
        mSelectedPackageDisplaySize = default;
        mSelectedPackageStageSize = default;
    }

    private void StartOpeningHintAnimation()
    {
        StopOpeningHintAnimation();
        if (mPackageItemTemplate == null || mSelectedPackageOverlayRect == null)
        {
            Debug.LogWarning("MainScene: opening hint animation skipped; PackItem template or selected cover is missing.");
            return;
        }

        var packNodeTemplate = FindChild(mPackageItemTemplate.transform, PackNodeObjectName);
        if (packNodeTemplate == null)
        {
            Debug.LogWarning("MainScene: opening hint animation skipped; PackItem/PackNode is missing.");
            return;
        }

        var hintObject = Instantiate(
            packNodeTemplate.gameObject,
            mSelectedPackageOverlayRect,
            false);
        hintObject.name = OpeningHintAnimationObjectName;

        var hintRect = hintObject.GetComponent<RectTransform>();
        var sourceCoverRect = FindChild(packNodeTemplate, PackCoverObjectName) as RectTransform;
        if (hintRect == null || sourceCoverRect == null)
        {
            Destroy(hintObject);
            Debug.LogWarning("MainScene: opening hint animation skipped; PackNode RectTransform is incomplete.");
            return;
        }

        hintRect.anchorMin = new Vector2(0.5f, 0.5f);
        hintRect.anchorMax = new Vector2(0.5f, 0.5f);
        hintRect.pivot = new Vector2(0.5f, 0.5f);
        hintRect.anchoredPosition = Vector2.zero;
        var sourceSize = sourceCoverRect.rect.size;
        var displaySize = mSelectedPackageOverlayRect.rect.size;
        var hintScale = Mathf.Min(
            displaySize.x / Mathf.Max(1f, sourceSize.x),
            displaySize.y / Mathf.Max(1f, sourceSize.y));
        hintRect.localScale = Vector3.one * hintScale;
        hintRect.SetAsLastSibling();

        var cover = FindChild(hintObject.transform, PackCoverObjectName);
        if (cover != null)
        {
            cover.gameObject.SetActive(false);
        }

        var size = FindChild(hintObject.transform, PackSizeObjectName);
        if (size != null)
        {
            size.gameObject.SetActive(false);
        }

        var light = FindChild(hintObject.transform, PackLightObjectName);
        var animator = hintObject.GetComponent<Animator>();
        if (light == null || animator == null || animator.runtimeAnimatorController == null)
        {
            hintObject.SetActive(false);
            Destroy(hintObject);
            Debug.LogWarning("MainScene: opening hint animation skipped; PackItem ImgLight or Animator is missing.");
            return;
        }

        light.gameObject.SetActive(true);
        hintObject.SetActive(true);
        animator.enabled = true;
        animator.speed = NormalPackBreathingSpeed;
        animator.Rebind();
        animator.Play(OpeningHintAnimationStateName, 0, 0f);
        animator.Update(0f);
        mOpeningHintAnimationRoot = hintObject;
    }

    private void StopOpeningHintAnimation()
    {
        if (mOpeningHintAnimationRoot == null)
        {
            return;
        }

        mOpeningHintAnimationRoot.SetActive(false);
        Destroy(mOpeningHintAnimationRoot);
        mOpeningHintAnimationRoot = null;
    }

}

public sealed class CardPackOpeningEffect : MonoBehaviour
{
    private const int EffectLayer = 31;
    private const int ModelVariantCount = 6;
    private const float ReferenceModelScale = 2.63f;
    private const float ReferenceModelLocalZ = 0f;
    private const float ModelWorldDepth = -1f;
    private const double ModelAnimationHandoffOffset = 0.8d;
    private const double TimelineEndTolerance = 1d / 30d + 0.0001d;
    private const float PieceEmergeDuration = 0.32f;
    private const float PieceMaximumPackHeightRatio =
        MainScene.InProgressPackPieceMaxSize
        * MainScene.InProgressPackPieceScaleMultiplier
        / MainScene.PackageCoverHeight;
    private const float PieceRisePackHeightRatio = 0.08f;
    private const float PieceDepthBehindPack = 0.05f;
    private const int PieceSortingOrderOffsetBehindPack = -1;
    private const string ModelPathFormat = "Effects/CardPack/Models/CardPackOpeningModel_{0:D3}";
    private const string AnimatorControllerPath = "Effects/CardPack/Animations/CardPackAnimation";
    private const string OpeningTimelinePath = "Effects/CardFx/Animations/test";
    private const string ModelAnimationClipName = "Take 001";
    private const string LightControlClipName = "fx_chai_w_001";
    private const string FrontMaterialPath = "Effects/CardFx/Materials/test";
    private const string PieceShadowMaterialPath = "IngameCoverShadow04";
    private const string SpriteRendererShadowKeyword = "PACK_SHADOW_SPRITE_RENDERER";
    private const string SceneLightEffectParentName = "PackObject";
    private const string SceneLightEffectObjectName = "fx_chai_w_001";
    private const string CardRendererNamePrefix = "mesh_skin_cardPack_";
    private const int FrontRendererNumberLength = 3;
    private const int BackRendererNumberLength = 5;
    private static readonly int SpritePixelsPerUnitId =
        Shader.PropertyToID("_SpritePixelsPerUnit");

    private GameObject mWorldRoot;
    private GameObject mModelObject;
    private GameObject mLightEffectObject;
    private Camera mMainCamera;
    private Animator mAnimator;
    private PlayableDirector mDirector;
    private TimelineAsset mOpeningTimeline;
    private Material mFrontMaterial;
    private Material mPieceShadowMaterial;
    private MaterialPropertyBlock mPieceShadowPropertyBlock;
    private readonly Dictionary<Sprite, Sprite> mFullRectPieceSprites =
        new Dictionary<Sprite, Sprite>();
    private readonly HashSet<Sprite> mRuntimeFullRectPieceSprites = new HashSet<Sprite>();
    private readonly List<SpriteRenderer> mEmergedPieceRenderers =
        new List<SpriteRenderer>();
    private Vector3[] mEmergedPieceStartPositions = Array.Empty<Vector3>();
    private Vector3[] mEmergedPieceFinalPositions = Array.Empty<Vector3>();
    private Vector3 mEmergedPieceScatterCenter;
    private bool mHasEmergedPieceScatterCenter;
    private float mEmergedPieceFinalScale;
    private int mOriginalCameraCullingMask;
    private bool mDidOverrideCameraCullingMask;
    private Coroutine mPlaybackCoroutine;
    private double mPlaybackStartTime;
    private double mGameEntranceHandoffTime;
    private double mPieceEmergeTime;
    private bool mIsPlaying;
    private bool mIsPrepared;
    private bool mHasHandedOffToGameScene;
    private bool mIsWaitingForSceneCamera;
    private bool mIsPausedForSceneHandoff;
    private bool mIsStoppingDirectorForCleanup;

    public static void PrepareSceneLightEffect()
    {
        var lightEffect = FindSceneLightEffect();
        if (lightEffect == null)
        {
            Debug.LogWarning(
                $"CardPackOpeningEffect: scene light effect is missing: "
                + $"{SceneLightEffectParentName}/{SceneLightEffectObjectName}");
            return;
        }

        StopAndClearParticleSystems(lightEffect);
        lightEffect.SetActive(false);
    }

    public static CardPackOpeningEffect Attach(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        var effectObject = new GameObject(
            nameof(CardPackOpeningEffect),
            typeof(CardPackOpeningEffect));
        effectObject.transform.SetParent(parent, false);
        effectObject.transform.SetAsLastSibling();

        var effect = effectObject.GetComponent<CardPackOpeningEffect>();
        effectObject.SetActive(false);
        return effect;
    }

    public bool Begin(
        Texture packTexture,
        RectTransform displayedPackRect,
        GameObject displayedPackVisual,
        IReadOnlyList<Sprite> firstGroupPieceSprites)
    {
        CleanupPlaybackResources();
        if (packTexture == null || displayedPackRect == null)
        {
            Debug.LogError("CardPackOpeningEffect: pack texture or display rect is missing.");
            return false;
        }

        var variant = UnityEngine.Random.Range(1, ModelVariantCount + 1);
        var modelPrefab = Resources.Load<GameObject>(string.Format(ModelPathFormat, variant));
        var controller = Resources.Load<RuntimeAnimatorController>(AnimatorControllerPath);
        var frontMaterialTemplate = Resources.Load<Material>(FrontMaterialPath);
        mOpeningTimeline = Resources.Load<TimelineAsset>(OpeningTimelinePath);
        if (modelPrefab == null
            || controller == null
            || frontMaterialTemplate == null
            || mOpeningTimeline == null
            || displayedPackVisual == null)
        {
            Debug.LogError(
                $"CardPackOpeningEffect: required resource is missing. variant={variant}, "
                + $"model={modelPrefab != null}, controller={controller != null}, "
                + $"frontMaterial={frontMaterialTemplate != null}, "
                + $"timeline={mOpeningTimeline != null}, "
                + $"staticPack={displayedPackVisual != null}");
            return false;
        }

        if (!CreateRenderStage())
        {
            CleanupPlaybackResources();
            return false;
        }

        var stageRoot = new GameObject("CardPackOpeningStage").transform;
        stageRoot.SetParent(mWorldRoot.transform, false);
        stageRoot.localPosition = Vector3.zero;
        stageRoot.localRotation = Quaternion.identity;
        stageRoot.localScale = Vector3.one;
        stageRoot.gameObject.layer = EffectLayer;

        mModelObject = Instantiate(modelPrefab, stageRoot, false);
        mModelObject.name = modelPrefab.name;
        mModelObject.transform.localPosition = new Vector3(0f, 0f, ReferenceModelLocalZ);
        mModelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        mModelObject.transform.localScale = Vector3.one * ReferenceModelScale;
        SetLayerRecursively(mModelObject, EffectLayer);

        mFrontMaterial = new Material(frontMaterialTemplate)
        {
            name = frontMaterialTemplate.name + " (Runtime Pack)"
        };
        mFrontMaterial.mainTexture = packTexture;
        if (!ApplyCardPackMaterials(mModelObject, mFrontMaterial))
        {
            Debug.LogError(
                $"CardPackOpeningEffect: expected card renderers were not found in {modelPrefab.name}.");
            CleanupPlaybackResources();
            return false;
        }

        mAnimator = mModelObject.GetComponent<Animator>();
        if (mAnimator == null)
        {
            mAnimator = mModelObject.AddComponent<Animator>();
        }

        mAnimator.runtimeAnimatorController = controller;
        mAnimator.applyRootMotion = false;
        mAnimator.updateMode = AnimatorUpdateMode.Normal;
        mAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        mAnimator.Rebind();

        mLightEffectObject = FindSceneLightEffect();
        if (mLightEffectObject == null)
        {
            Debug.LogError(
                $"CardPackOpeningEffect: scene light effect is missing: "
                + $"{SceneLightEffectParentName}/{SceneLightEffectObjectName}");
            CleanupPlaybackResources();
            return false;
        }

        StopAndClearParticleSystems(mLightEffectObject);
        mLightEffectObject.SetActive(false);
        displayedPackVisual.SetActive(false);
        gameObject.SetActive(true);

        if (!ConfigureOpeningTimeline())
        {
            CleanupPlaybackResources();
            return false;
        }

        mDirector.time = mPlaybackStartTime;
        mDirector.Evaluate();
        if (!TryFitStageToDisplayedPack(stageRoot, displayedPackRect))
        {
            Debug.LogError("CardPackOpeningEffect: failed to calculate the opening model bounds.");
            CleanupPlaybackResources();
            return false;
        }

        PrepareEmergedPieces(firstGroupPieceSprites);
        mDirector.time = mPlaybackStartTime;
        mDirector.Evaluate();

        mIsPrepared = true;
        Debug.Log(
            $"CardPackOpeningEffect: prepared variant {variant:D3} with {packTexture.name}. "
            + $"timelineDuration={mDirector.duration:F3}s, "
            + $"start={mPlaybackStartTime:F3}s, "
            + $"handoff={mGameEntranceHandoffTime:F3}s, light=scene instance");
        return true;
    }

    private static GameObject FindSceneLightEffect()
    {
        var parent = GameCommonUtility.FindSceneObject(SceneLightEffectParentName);
        if (parent == null)
        {
            return null;
        }

        var lightEffect = parent.transform.Find(SceneLightEffectObjectName);
        return lightEffect != null ? lightEffect.gameObject : null;
    }

    public void StartPlayback()
    {
        if (!mIsPrepared || mDirector == null || mOpeningTimeline == null)
        {
            return;
        }

        mIsPrepared = false;
        mIsPlaying = true;
        mDirector.time = mPlaybackStartTime;
        mDirector.Play();
        mPlaybackCoroutine = StartCoroutine(MonitorTimelinePlayback());
        Debug.Log(
            $"CardPackOpeningEffect: opening timeline started, "
            + $"duration={mDirector.duration:F3}s.");
    }

    public IEnumerator WaitForGameEntranceHandoff()
    {
        while (mIsPlaying
               && mDirector != null
               && mDirector.time < mGameEntranceHandoffTime)
        {
            yield return null;
        }
    }

    public void PrepareForSceneHandoff()
    {
        if (!mIsPlaying || mHasHandedOffToGameScene)
        {
            return;
        }

        mHasHandedOffToGameScene = true;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        mIsWaitingForSceneCamera = true;
        PauseForSceneHandoff();
        for (var i = 0; i < mEmergedPieceRenderers.Count; i++)
        {
            if (mEmergedPieceRenderers[i] != null)
            {
                mEmergedPieceRenderers[i].enabled = false;
            }
        }

        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
        if (mWorldRoot != null)
        {
            mWorldRoot.transform.SetParent(transform, true);
        }

        if (mLightEffectObject != null)
        {
            var persistentParent = mWorldRoot != null
                ? mWorldRoot.transform
                : transform;
            mLightEffectObject.transform.SetParent(persistentParent, true);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!mHasHandedOffToGameScene || !mIsWaitingForSceneCamera)
        {
            return;
        }

        StartCoroutine(BindToLoadedSceneCamera(scene));
    }

    private IEnumerator BindToLoadedSceneCamera(Scene loadedScene)
    {
        Camera loadedCamera = null;
        while (loadedCamera == null)
        {
            var rootObjects = loadedScene.GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length && loadedCamera == null; i++)
            {
                var cameras = rootObjects[i].GetComponentsInChildren<Camera>(true);
                for (var cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
                {
                    if (cameras[cameraIndex].CompareTag("MainCamera"))
                    {
                        loadedCamera = cameras[cameraIndex];
                        break;
                    }
                }
            }

            if (loadedCamera == null)
            {
                yield return null;
            }
        }

        BindEffectLayerToCamera(loadedCamera);
        mIsWaitingForSceneCamera = false;
        // Scene activation can block the main thread for multiple seconds. Resume the
        // authored Timeline on the following rendered frame so it does not consume that stall.
        yield return null;
        ResumeAfterSceneHandoff();
    }

    private IEnumerator MonitorTimelinePlayback()
    {
        while (mIsPlaying && mDirector != null)
        {
            if (!mIsPausedForSceneHandoff)
            {
                UpdateEmergedPieces(mDirector.time);
                if (HasReachedTimelineEnd())
                {
                    CompleteOpeningTimelineAndRetain(
                        mDirector,
                        "held final frame");
                    break;
                }
            }

            yield return null;
        }

        mPlaybackCoroutine = null;
    }

    private bool ConfigureOpeningTimeline()
    {
        mDirector = GetComponent<PlayableDirector>();
        if (mDirector == null)
        {
            mDirector = gameObject.AddComponent<PlayableDirector>();
        }

        mDirector.playOnAwake = false;
        mDirector.extrapolationMode = DirectorWrapMode.Hold;
        mDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
        mDirector.playableAsset = mOpeningTimeline;
        mDirector.stopped -= HandleOpeningTimelineStopped;
        mDirector.stopped += HandleOpeningTimelineStopped;

        var animationTrackCount = 0;
        var modelActivationBound = false;
        var lightBound = false;
        foreach (var track in mOpeningTimeline.GetRootTracks())
        {
            if (track is AnimationTrack)
            {
                mDirector.SetGenericBinding(track, mAnimator);
                animationTrackCount++;
            }
            else if (track is ActivationTrack)
            {
                mDirector.SetGenericBinding(track, mModelObject);
                modelActivationBound = true;
            }

            foreach (var timelineClip in track.GetClips())
            {
                if (!(timelineClip.asset is ControlPlayableAsset controlAsset))
                {
                    continue;
                }

                GameObject target = null;
                switch (timelineClip.displayName)
                {
                    case LightControlClipName:
                        target = mLightEffectObject;
                        lightBound = true;
                        break;
                }

                if (target != null)
                {
                    mDirector.SetReferenceValue(
                        controlAsset.sourceGameObject.exposedName,
                        target);
                }
            }
        }

        mPlaybackStartTime = 0d;
        mPieceEmergeTime = ResolveTimelineClipStart(LightControlClipName);
        mGameEntranceHandoffTime = ResolveTimelineClipStart(ModelAnimationClipName)
            + ModelAnimationHandoffOffset;
        if (animationTrackCount < 1
            || !modelActivationBound
            || !lightBound
            || mPieceEmergeTime <= 0d
            || mGameEntranceHandoffTime <= mPlaybackStartTime
            || mGameEntranceHandoffTime >= mDirector.duration)
        {
            Debug.LogError(
                "CardPackOpeningEffect: opening timeline bindings are incomplete. "
                + $"animationTracks={animationTrackCount}, "
                + $"modelActivation={modelActivationBound}, light={lightBound}, "
                + $"start={mPlaybackStartTime:F3}, "
                + $"pieceTime={mPieceEmergeTime:F3}, "
                + $"handoff={mGameEntranceHandoffTime:F3}.");
            return false;
        }

        return true;
    }

    private double ResolveTimelineClipStart(string displayName)
    {
        if (mOpeningTimeline == null)
        {
            return 0d;
        }

        foreach (var track in mOpeningTimeline.GetRootTracks())
        {
            foreach (var timelineClip in track.GetClips())
            {
                if (timelineClip.displayName == displayName)
                {
                    return timelineClip.start;
                }
            }
        }

        return 0d;
    }

    private void HandleOpeningTimelineStopped(PlayableDirector director)
    {
        if (mIsStoppingDirectorForCleanup
            || director != mDirector
            || !mIsPlaying)
        {
            return;
        }

        CompleteOpeningTimelineAndRetain(director, "stopped callback fallback");
    }

    private bool HasReachedTimelineEnd()
    {
        if (mDirector == null
            || double.IsInfinity(mDirector.duration)
            || mDirector.duration <= 0d)
        {
            return false;
        }

        var remaining = mDirector.duration - mDirector.time;
        return remaining <= TimelineEndTolerance;
    }

    private void CompleteOpeningTimelineAndRetain(
        PlayableDirector director,
        string completionSource)
    {
        if (director == null || director != mDirector || !mIsPlaying)
        {
            return;
        }

        var duration = director.duration;
        if (!double.IsInfinity(duration) && duration > 0d)
        {
            director.time = duration;
            director.Evaluate();
        }

        mIsPlaying = false;
        if (director.state == PlayState.Playing)
        {
            director.Pause();
        }

        CompleteEmergedPieces();
        var completedSceneHandoff = mHasHandedOffToGameScene;
        Debug.Log(
            "CardPackOpeningEffect: opening timeline completed and retained. "
            + $"time={director.time:F3}s, duration={director.duration:F3}s, "
            + $"sceneHandoff={completedSceneHandoff}, source={completionSource}.");
    }

    public bool TryGetEmergedPieceScreenOrigin(out Vector2 normalizedScreenPosition)
    {
        normalizedScreenPosition = default;
        if (mMainCamera == null
            || !mHasEmergedPieceScatterCenter
            || Screen.width <= 0
            || Screen.height <= 0)
        {
            return false;
        }

        var screenCenter = mMainCamera.WorldToScreenPoint(mEmergedPieceScatterCenter);
        if (screenCenter.z <= 0f)
        {
            return false;
        }

        normalizedScreenPosition = new Vector2(
            Mathf.Clamp01(screenCenter.x / Screen.width),
            Mathf.Clamp01(screenCenter.y / Screen.height));
        return true;
    }

    private void PrepareEmergedPieces(IReadOnlyList<Sprite> pieceSprites)
    {
        mEmergedPieceRenderers.Clear();
        mEmergedPieceStartPositions = Array.Empty<Vector3>();
        mEmergedPieceFinalPositions = Array.Empty<Vector3>();
        mEmergedPieceScatterCenter = Vector3.zero;
        mHasEmergedPieceScatterCenter = false;
        if (pieceSprites == null || pieceSprites.Count == 0 || mWorldRoot == null)
        {
            return;
        }

        var frontRenderers = GetFrontCardRenderers(mModelObject);
        if (!TryGetRendererBounds(frontRenderers, out var packBounds))
        {
            return;
        }

        var packSortingLayerId = frontRenderers[0].sortingLayerID;
        var pieceSortingOrder = frontRenderers[0].sortingOrder;
        for (var i = 1; i < frontRenderers.Length; i++)
        {
            pieceSortingOrder = Mathf.Min(pieceSortingOrder, frontRenderers[i].sortingOrder);
        }
        pieceSortingOrder += PieceSortingOrderOffsetBehindPack;

        PreparePieceShadowMaterial();

        var largestPieceSide = 0f;
        for (var i = 0; i < pieceSprites.Count; i++)
        {
            var sprite = pieceSprites[i];
            if (sprite != null)
            {
                largestPieceSide = Mathf.Max(
                    largestPieceSide,
                    sprite.bounds.size.x,
                    sprite.bounds.size.y);
            }
        }

        if (largestPieceSide <= 0.0001f)
        {
            return;
        }

        var pieceRoot = new GameObject("OpeningFirstGroupPieces");
        pieceRoot.layer = EffectLayer;
        pieceRoot.transform.SetParent(mWorldRoot.transform, false);
        var pieceScale = packBounds.size.y
                         * PieceMaximumPackHeightRatio
                         / largestPieceSide;
        var tearPosition = mLightEffectObject.transform.position;
        tearPosition.z = packBounds.max.z + PieceDepthBehindPack;
        mEmergedPieceScatterCenter = tearPosition + new Vector3(
            0f,
            packBounds.size.y * PieceRisePackHeightRatio,
            0f);
        mHasEmergedPieceScatterCenter = true;
        var pieceCount = pieceSprites.Count;
        mEmergedPieceStartPositions = new Vector3[pieceCount];
        mEmergedPieceFinalPositions = new Vector3[pieceCount];
        for (var i = 0; i < pieceCount; i++)
        {
            var startPosition = tearPosition + new Vector3(
                0f,
                -packBounds.size.y * 0.015f,
                0f);
            var scatterOffset = GameDefine.CalculatePieceDealScatterOffset(
                i,
                GameDefine.TornPackPieceStartSpreadMultiplier);
            var finalPosition = mEmergedPieceScatterCenter + new Vector3(
                scatterOffset.x,
                scatterOffset.y,
                0f);
            mEmergedPieceStartPositions[i] = startPosition;
            mEmergedPieceFinalPositions[i] = finalPosition;

            var pieceObject = new GameObject($"OpeningPiece{i + 1:D2}");
            pieceObject.layer = EffectLayer;
            pieceObject.transform.SetParent(pieceRoot.transform, false);
            pieceObject.transform.position = startPosition;
            pieceObject.transform.localScale = Vector3.one * pieceScale;
            pieceObject.transform.rotation = Quaternion.identity;
            var renderer = pieceObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetOrCreateFullRectPieceSprite(pieceSprites[i]);
            renderer.sortingLayerID = packSortingLayerId;
            renderer.sortingOrder = pieceSortingOrder;
            renderer.color = Color.white;
            renderer.enabled = false;
            ApplyPieceShadowMaterial(renderer);
            mEmergedPieceRenderers.Add(renderer);
        }

        mEmergedPieceFinalScale = pieceScale;
    }

    private void PreparePieceShadowMaterial()
    {
        if (mPieceShadowMaterial != null)
        {
            return;
        }

        var template = Resources.Load<Material>(PieceShadowMaterialPath);
        if (template == null)
        {
            Debug.LogWarning(
                $"CardPackOpeningEffect: opening piece shadow material is missing: "
                + PieceShadowMaterialPath);
            return;
        }

        mPieceShadowMaterial = new Material(template)
        {
            name = template.name + " (Opening SpriteRenderer Runtime)"
        };
        mPieceShadowMaterial.EnableKeyword(SpriteRendererShadowKeyword);
        mPieceShadowMaterial.renderQueue = Mathf.Max(
            1000,
            mFrontMaterial != null ? mFrontMaterial.renderQueue - 1 : 2000);
    }

    private Sprite GetOrCreateFullRectPieceSprite(Sprite source)
    {
        if (source == null || mRuntimeFullRectPieceSprites.Contains(source))
        {
            return source;
        }

        if (mFullRectPieceSprites.TryGetValue(source, out var existing)
            && existing != null)
        {
            return existing;
        }

        var rect = source.rect;
        if (rect.width <= 0f || rect.height <= 0f || source.texture == null)
        {
            return source;
        }

        var pivot = new Vector2(source.pivot.x / rect.width, source.pivot.y / rect.height);
        var fullRectSprite = Sprite.Create(
            source.texture,
            rect,
            pivot,
            source.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            source.border);
        fullRectSprite.name = $"{source.name} (Opening Shadow FullRect Runtime)";
        mFullRectPieceSprites[source] = fullRectSprite;
        mRuntimeFullRectPieceSprites.Add(fullRectSprite);
        return fullRectSprite;
    }

    private void ApplyPieceShadowMaterial(SpriteRenderer renderer)
    {
        if (renderer == null || renderer.sprite == null || mPieceShadowMaterial == null)
        {
            return;
        }

        renderer.sharedMaterial = mPieceShadowMaterial;
        if (mPieceShadowPropertyBlock == null)
        {
            mPieceShadowPropertyBlock = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(mPieceShadowPropertyBlock);
        mPieceShadowPropertyBlock.SetFloat(
            SpritePixelsPerUnitId,
            Mathf.Max(1f, renderer.sprite.pixelsPerUnit));
        renderer.SetPropertyBlock(mPieceShadowPropertyBlock);
        mPieceShadowPropertyBlock.Clear();
    }

    private void UpdateEmergedPieces(double timelineTime)
    {
        if (mHasHandedOffToGameScene)
        {
            return;
        }

        for (var i = 0; i < mEmergedPieceRenderers.Count; i++)
        {
            var renderer = mEmergedPieceRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (timelineTime < mPieceEmergeTime)
            {
                renderer.enabled = false;
                continue;
            }

            renderer.enabled = true;
            var normalized = Mathf.Clamp01(
                (float)((timelineTime - mPieceEmergeTime) / PieceEmergeDuration));
            var eased = 1f - Mathf.Pow(1f - normalized, 3f);
            renderer.transform.position = Vector3.LerpUnclamped(
                mEmergedPieceStartPositions[i],
                mEmergedPieceFinalPositions[i],
                eased);
            renderer.transform.localScale = Vector3.one * mEmergedPieceFinalScale;
        }
    }

    private void CompleteEmergedPieces()
    {
        if (mHasHandedOffToGameScene)
        {
            return;
        }

        for (var i = 0; i < mEmergedPieceRenderers.Count; i++)
        {
            var renderer = mEmergedPieceRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.transform.position = mEmergedPieceFinalPositions[i];
            renderer.transform.localScale = Vector3.one * mEmergedPieceFinalScale;
            renderer.color = Color.white;
            renderer.enabled = true;
        }
    }

    private bool CreateRenderStage()
    {
        mMainCamera = Camera.main;
        if (mMainCamera == null)
        {
            Debug.LogError("CardPackOpeningEffect: Main Camera is missing.");
            return false;
        }

        mWorldRoot = new GameObject("CardPackOpeningEffectWorld");
        mWorldRoot.layer = EffectLayer;
        BindEffectLayerToCamera(mMainCamera);
        return true;
    }

    private void BindEffectLayerToCamera(Camera camera)
    {
        if (camera == null || camera == mMainCamera && mDidOverrideCameraCullingMask)
        {
            return;
        }

        RestoreCameraCullingMask();
        mMainCamera = camera;
        mOriginalCameraCullingMask = camera.cullingMask;
        camera.cullingMask |= 1 << EffectLayer;
        mDidOverrideCameraCullingMask = true;
    }

    private void RestoreCameraCullingMask()
    {
        if (mMainCamera != null && mDidOverrideCameraCullingMask)
        {
            mMainCamera.cullingMask = mOriginalCameraCullingMask;
        }

        mDidOverrideCameraCullingMask = false;
    }

    private bool TryFitStageToDisplayedPack(Transform stageRoot, RectTransform displayedPackRect)
    {
        var renderers = GetFrontCardRenderers(mModelObject);
        if (renderers.Length == 0)
        {
            return false;
        }

        var hasBounds = false;
        var bounds = default(Bounds);
        for (var i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled)
            {
                renderers[i].enabled = true;
            }

            if (!hasBounds)
            {
                bounds = renderers[i].bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        if (!hasBounds || bounds.size.y <= 0.0001f)
        {
            return false;
        }

        var canvas = displayedPackRect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera : mMainCamera;
        var corners = new Vector3[4];
        displayedPackRect.GetWorldCorners(corners);
        var minScreenY = float.PositiveInfinity;
        var maxScreenY = float.NegativeInfinity;
        for (var i = 0; i < corners.Length; i++)
        {
            var screenCorner = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
            minScreenY = Mathf.Min(minScreenY, screenCorner.y);
            maxScreenY = Mathf.Max(maxScreenY, screenCorner.y);
        }

        var targetScreenHeight = maxScreenY - minScreenY;
        if (targetScreenHeight <= 0.001f)
        {
            targetScreenHeight = Screen.height
                * displayedPackRect.rect.height
                / GameDefine.DesignHeight;
        }

        var targetWorldHeight = mMainCamera.orthographicSize
            * 2f
            * targetScreenHeight
            / Mathf.Max(1f, Screen.height);
        var uniformScale = targetWorldHeight / bounds.size.y;
        stageRoot.localScale = Vector3.one * uniformScale;
        if (!TryGetRendererBounds(renderers, out var scaledBounds))
        {
            return false;
        }

        var screenCenter = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            displayedPackRect.TransformPoint(displayedPackRect.rect.center));
        var distance = Mathf.Abs(ModelWorldDepth - mMainCamera.transform.position.z);
        var worldCenter = mMainCamera.ScreenToWorldPoint(
            new Vector3(screenCenter.x, screenCenter.y, distance));
        stageRoot.position += new Vector3(
            worldCenter.x - scaledBounds.center.x,
            worldCenter.y - scaledBounds.center.y,
            ModelWorldDepth - scaledBounds.center.z);
        Debug.Log(
            $"CardPackOpeningEffect: fitted stage. screen={Screen.width}x{Screen.height}, "
            + $"targetPixels={targetScreenHeight:F1}, frontBounds={bounds.size}, "
            + $"stageScale={uniformScale:F4}, centerPixels={screenCenter}.");
        return true;
    }

    private static bool TryGetRendererBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        var hasBounds = false;
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds && bounds.size.y > 0.0001f;
    }

    private static Renderer[] GetFrontCardRenderers(GameObject model)
    {
        if (model == null)
        {
            return Array.Empty<Renderer>();
        }

        var cardRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var frontRenderers = new List<Renderer>(cardRenderers.Length);
        for (var i = 0; i < cardRenderers.Length; i++)
        {
            var renderer = cardRenderers[i];
            if (renderer.name.StartsWith(CardRendererNamePrefix)
                && renderer.name.Length - CardRendererNamePrefix.Length
                    == FrontRendererNumberLength)
            {
                frontRenderers.Add(renderer);
            }
        }

        return frontRenderers.ToArray();
    }

    private static bool ApplyCardPackMaterials(
        GameObject model,
        Material frontMaterial)
    {
        var foundFront = false;
        var renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (!renderer.name.StartsWith(CardRendererNamePrefix))
            {
                continue;
            }

            var numberLength = renderer.name.Length - CardRendererNamePrefix.Length;
            if (numberLength == BackRendererNumberLength)
            {
                renderer.enabled = false;
            }
            else if (numberLength == FrontRendererNumberLength)
            {
                renderer.sharedMaterial = frontMaterial;
                foundFront = true;
            }
        }

        return foundFront;
    }

    private static void StopAndClearParticleSystems(GameObject effectRoot)
    {
        if (effectRoot == null)
        {
            return;
        }

        var rootParticleSystem = effectRoot.GetComponent<ParticleSystem>();
        if (rootParticleSystem != null)
        {
            rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void PauseForSceneHandoff()
    {
        if (mIsPausedForSceneHandoff)
        {
            return;
        }

        mIsPausedForSceneHandoff = true;
        if (mDirector != null
            && mDirector.state == PlayState.Playing)
        {
            mDirector.Pause();
        }

        Debug.Log(
            $"CardPackOpeningEffect: paused for scene handoff at "
            + $"{(mDirector != null ? mDirector.time : 0d):F3}s.");
    }

    private void ResumeAfterSceneHandoff()
    {
        if (!mIsPausedForSceneHandoff)
        {
            return;
        }

        mIsPausedForSceneHandoff = false;
        if (mDirector != null
            && mDirector.state == PlayState.Paused)
        {
            mDirector.Resume();
        }

        Debug.Log(
            $"CardPackOpeningEffect: resumed after scene handoff at "
            + $"{(mDirector != null ? mDirector.time : 0d):F3}s.");
    }

    private void ReleaseSceneLightEffect()
    {
        if (mLightEffectObject == null)
        {
            return;
        }

        StopAndClearParticleSystems(mLightEffectObject);
        mLightEffectObject.SetActive(false);
        mLightEffectObject = null;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layer;
        }
    }

    private void CleanupPlaybackResources()
    {
        if (mPlaybackCoroutine != null)
        {
            StopCoroutine(mPlaybackCoroutine);
            mPlaybackCoroutine = null;
        }

        if (mDirector != null)
        {
            mDirector.stopped -= HandleOpeningTimelineStopped;
            if (mDirector.state != PlayState.Paused)
            {
                mIsStoppingDirectorForCleanup = true;
                mDirector.Stop();
                mIsStoppingDirectorForCleanup = false;
            }

            mDirector.playableAsset = null;
        }

        mIsPlaying = false;
        mIsPrepared = false;
        mHasHandedOffToGameScene = false;
        mIsPausedForSceneHandoff = false;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        mIsWaitingForSceneCamera = false;
        RestoreCameraCullingMask();

        ReleaseSceneLightEffect();

        if (mWorldRoot != null)
        {
            Destroy(mWorldRoot);
            mWorldRoot = null;
        }

        if (mFrontMaterial != null)
        {
            Destroy(mFrontMaterial);
            mFrontMaterial = null;
        }

        if (mPieceShadowMaterial != null)
        {
            Destroy(mPieceShadowMaterial);
            mPieceShadowMaterial = null;
        }

        foreach (var sprite in mRuntimeFullRectPieceSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }

        mRuntimeFullRectPieceSprites.Clear();
        mFullRectPieceSprites.Clear();
        mPieceShadowPropertyBlock = null;

        mModelObject = null;
        mMainCamera = null;
        mAnimator = null;
        mDirector = null;
        mOpeningTimeline = null;
        mPlaybackStartTime = 0d;
        mGameEntranceHandoffTime = 0d;
        mPieceEmergeTime = 0d;
        mIsStoppingDirectorForCleanup = false;
        mEmergedPieceRenderers.Clear();
        mEmergedPieceStartPositions = Array.Empty<Vector3>();
        mEmergedPieceFinalPositions = Array.Empty<Vector3>();
        mEmergedPieceScatterCenter = Vector3.zero;
        mHasEmergedPieceScatterCenter = false;
        mEmergedPieceFinalScale = 0f;
    }

    private void OnDestroy()
    {
        CleanupPlaybackResources();
    }
}
