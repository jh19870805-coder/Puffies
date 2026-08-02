using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float PackageClickScaleRatio = 1.15f;
    private const float PackageClickAnimDuration = 0.12f;
    private const float PackageBreathMinScale = 0.98f;
    private const float PackageBreathMaxScale = 1.02f;
    private const float PackageBreathDuration = 2.4f;
    private const float PackageOpenScaleDuration = 0.3f;
    private const float PackageOpenWidth = 600f;
    private const float PackageOpenHeight = 680f;
    private const float PackageSlotWidth = 240f;
    private const float PackageSlotHeight = 272f;
    private const float PackageCoverWidth = 240f;
    private const float PackageCoverHeight = 272f;
    private const float PackageShadowHorizontalPadding = 8f;
    private const float PackageShadowVerticalPadding = 36f;
    private const float PackageShadowOffsetX = 0f;
    private const float PackageShadowOffsetY = -20f;
    private const float PackageShadowAlpha = 0.52f;
    private const int PackageShadowBlurPassCount = 3;
    private const int PackageShadowHorizontalBlurRadius = 2;
    private const int PackageShadowVerticalBlurRadius = 5;
    private const float PackageHorizontalSpacing = 20f;
    private const float PackageVerticalSpacing = 20f;
    private const float PackageContentHorizontalPadding = 16f;
    private const float DefaultPackagePageWidth = 1625f;
    private const float DefaultPackagePageHeight = 950f;
    private const int PackagesPerPageRowCount = 3;
    private const int PackagesPerPageColumnCount = 6;
    private const int PackagesPerPage = PackagesPerPageRowCount * PackagesPerPageColumnCount;
    private const int PackageListSortingOrderBase = 1000;
    private const int PackageSortingOrderStride = 2;
    private const int PackageSizeCanvasSortingOrder = 15000;
    private const int BagSelectPanelSortingOrder = 20000;
    private const int SelectedPackageSortingOrder = 30000;
    private const int TearGuideSortingOrder = 31000;
    private const int PhotoPanelSortingOrder = 32000;
    private const int PhotoFlashSortingOrder = 33000;
    private const int SelectedPackageRenderLayer = 29;
    private const int PhotoCaptureLayer = 30;
    private const int PhotoOutputSize = 1024;
    private const float PhotoPuzzleRotation = 7f;
    private const float PhotoPuzzleMaxSize = 920f;
    private const float PhotoPuzzleOffsetY = 8f;
    private const float PhotoFlashFadeInDuration = 0.06f;
    private const float PhotoFlashHoldDuration = 0.04f;
    private const float PhotoFlashFadeOutDuration = 0.16f;
    private const float BagSelectBackdropAlpha = 0.34f;
    private const float PackageSizeWorldDepth = -0.05f;
    private const float BagSelectPanelWorldDepth = -0.1f;
    private const float SelectedPackageWorldDepth = -0.2f;
    private const int BagSelectBlurDownsample = 2;
    private const int BagSelectBlurPyramidLevels = 3;
    private const float OpeningStageTransitionDuration = 0.28f;
    private const float OpeningStageSettleDuration = 0.22f;
    private const float OpeningStageScaleRatio = 0.92f;
    private const float OpeningStagePunchScaleRatio = 1.04f;
    private const float TearGuideTravelDuration = 0.85f;
    private const float TearGuidePauseDuration = 0.25f;
    private const float TearTrailTravelDuration = 0.42f;
    private const float TearGuideBandHeightRatio = 0.24f;
    private const float TearSwipeStartMaxRatio = 0.38f;
    private const float TearSwipeRequiredDistanceRatio = 0.5f;
    private const float TearSwipeMaxVerticalDriftRatio = 0.2f;
    private const float TearTapMaxTravelRatio = 0.06f;
    private const float TearTapMinTravelPixels = 18f;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string PackageScrollViewObjectName = "PackageScrollView";
    private const string PackagePageObjectPrefix = "Page_";
    private const string PackageFirstPageObjectName = "Page_1";
    private const string PackItemPrefabEditorPath = "Assets/Prefabs/PackItem.prefab";
    private const string PackItemPrefabResourcesPath = "PackItem";
    private const string PackItemTemplateObjectName = "PackItemTemplate";
    private const string PackShadowObjectName = "PackShadow";
    private const string PackCoverObjectName = "PackCover";
    private const string PackSizeObjectName = "PackSize";
    private const string PackageSizeCanvasObjectName = "CardPackSizeCanvas";
    private const string PackEffectObjectName = "CardPackEffect";
    private const string PackNameTextObjectName = "NameText";
    private const string MenuButtonObjectName = "BtnMenu";
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
    private const string SelectedPackageCameraObjectName = "SelectedCardPackCamera";
    private const string OpeningStageBackgroundObjectName = "CardPackOpeningStageBackground";
    private const string TearGuideCanvasObjectName = "CardPackTearGuideCanvas";
    private const string TearGuideObjectName = "CardPackTearGuide";
    private const string TearFlashObjectName = "CardPackTearFlash";
    private const string OpeningStageBackgroundPath = GameDefine.UiRoot + "/BasicUI/BgGame.png";
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
    private static bool sHookedSceneLoaded;

    [SerializeField] private GameObject mPackageItemPrefab;

    private readonly Dictionary<int, PackageEntry> mPackageSlotsById = new Dictionary<int, PackageEntry>();
    private readonly Dictionary<int, Sprite> mPackageShadowSpritesById = new Dictionary<int, Sprite>();
    private GameObject mPackageItemTemplate;
    private Image mLegacyPackageSlotTemplate;
    private RectTransform mPackageContentRoot;
    private RectTransform mPackagePageTemplate;
    private ScrollRect mPackageScrollRect;
    private GameObject mMenuPanelRoot;
    private GameObject mSettingsPanelRoot;
    private GameObject mUsablePanelRoot;
    private GameObject mSavePanelRoot;
    private GameObject mBagSelectPanelRoot;
    private Canvas mBagSelectOverlayCanvas;
    private Camera mSelectedPackageOverlayCamera;
    private Camera mSelectedPackageSourceCamera;
    private int mSelectedPackageSourceCullingMask;
    private bool mIsSelectedPackageRenderLayerActive;
    private Canvas mPackageSizeCanvas;
    private RectTransform mPackageSizeCanvasRect;
    private Material mPackageSizeOverlayMaterial;
    private Canvas mTearGuideCanvas;
    private CanvasGroup mMainCanvasGroup;
    private CanvasGroup mBagSelectPanelCanvasGroup;
    private RawImage mBagSelectBackdropImage;
    private Image mOpeningStageBackgroundImage;
    private Sprite mOpeningStageBackgroundSprite;
    private Sprite mTearGuideCircleSprite;
    private Texture2D mTearGuideCircleTexture;
    private RectTransform mTearGuideRect;
    private CanvasGroup mTearGuideCanvasGroup;
    private RectTransform mTearFlashRect;
    private RectTransform mTearFlashHaloRect;
    private RectTransform mTearFlashCoreRect;
    private RectTransform mTearFlashHeadRect;
    private CanvasGroup mTearFlashCanvasGroup;
    private RenderTexture mBagSelectBackdropTexture;
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
    private bool mUsesPagedPackageGrid;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private bool mIsApplyingSettingsToUi;
    private Coroutine mPlayAnimationCoroutine;
    private Coroutine mTearGuideCoroutine;
    private PackageEntry mSelectedPackageEntry;
    private Button mBagSelectPlayButton;
    private Button mBagSelectBackButton;
    private GameObject mBagSelectCameraButtonRoot;
    private Button mBagSelectCameraButton;
    private TMP_Text mBagSelectPlayLabel;
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
    private float mSelectedPackageStartScale;
    private float mSelectedPackageOpenScale;
    private Vector3 mSelectedPackageStartCenter;
    private Vector3 mSelectedPackageDisplayCenter;
    private float mSelectedPackageStageScale;
    private bool mIsAwaitingTearSwipe;
    private bool mIsTrackingTearSwipe;
    private bool mIsTrackingTearTap;
    private Vector2 mTearSwipeStartScreenPosition;
    private Rect mTearSwipeScreenRect;

    private sealed class PackageEntry
    {
        public int BagId;
        public GameObject Root;
        public Image Image;
        public Image ShadowImage;
        public Image SizeImage;
        public GameObject EffectRoot;
        public RectTransform RectTransform;
        public Vector2 SizeBaseAnchoredPosition;
        public Vector3 SizeBaseLocalScale;
        public GameAnimationUtility.CardPackIdleDisplay IdleDisplay;
        public float BreathPhase;
        public int RenderOrder;
        public bool SuppressDisplay;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<MainScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneMain,
            BootstrapObjectName);
    }

    private void OnDestroy()
    {
        DisableSelectedPackageRenderLayer();
        if (mSelectedPackageOverlayCamera != null)
        {
            Destroy(mSelectedPackageOverlayCamera.gameObject);
        }

        ReleaseBagSelectBackdropTexture();
        ReleaseGeneratedPhoto();
        ReleaseUsablePanelPreviewSprites();
        if (mOpeningStageBackgroundSprite != null)
        {
            var texture = mOpeningStageBackgroundSprite.texture;
            Destroy(mOpeningStageBackgroundSprite);
            if (texture != null)
            {
                Destroy(texture);
            }
        }

        if (mTearGuideCircleSprite != null)
        {
            Destroy(mTearGuideCircleSprite);
        }

        if (mTearGuideCircleTexture != null)
        {
            Destroy(mTearGuideCircleTexture);
        }

        if (mPackageSizeOverlayMaterial != null)
        {
            Destroy(mPackageSizeOverlayMaterial);
        }

        foreach (var pair in mPackageSlotsById)
        {
            ReleasePackageDisplay(pair.Value);
        }

        foreach (var pair in mPackageShadowSpritesById)
        {
            var shadowSprite = pair.Value;
            if (shadowSprite == null)
            {
                continue;
            }

            var shadowTexture = shadowSprite.texture;
            Destroy(shadowSprite);
            if (shadowTexture != null)
            {
                Destroy(shadowTexture);
            }
        }

        mPackageShadowSpritesById.Clear();
    }

    private void LateUpdate()
    {
        UpdatePackageDisplays();
    }

    private void Update()
    {
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

        mHasSwitchedToGameScene = false;
        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
        mTearGuideCoroutine = null;
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

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }
        if (!TryResolvePackageList())
        {
            Debug.LogWarning("MainScene: package list not found. Expected PackageScrollView/Page_1 with PackItem prefab, or legacy Package001.");
        }
        else
        {
            ConfigurePackageCanvas(targetCamera);
            CreatePackageSizeCanvas(targetCamera);
            RefreshPackageList();
        }

        ConfigureRankButton();
        ConfigureAchieveButton();
        ConfigureBagSelectPanel();
        ConfigureReplayPanel();
        ConfigurePhotoPanel();
        ConfigureMenuPanel();
        ConfigureSettingsPanel();
        ConfigureUsablePanel();
        ConfigureSavePanel();
        RefreshTaskProgressUI();
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
            && mSelectedPackageEntry == null;
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
                RectTransform = image.rectTransform,
                BreathPhase = resolvedBagId * 0.6180339f,
                RenderOrder = PackageListSortingOrderBase
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
            panelColor.a = BagSelectBackdropAlpha;
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
            SelectedPackageWorldDepth - 0.03f);
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
            SelectedPackageWorldDepth - 0.02f);
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

        CreateTearGuideCanvas(camera, sourceCanvas);
    }

    private void EnableSelectedPackageRenderLayer()
    {
        if (mIsSelectedPackageRenderLayerActive)
        {
            return;
        }

        var sourceCamera = Camera.main;
        if (sourceCamera == null
            || !GameAnimationUtility.SetPreparedCardPackRenderLayer(
                SelectedPackageRenderLayer))
        {
            Debug.LogWarning("MainScene: selected card pack render layer could not be enabled.");
            return;
        }

        if (mSelectedPackageOverlayCamera == null)
        {
            var cameraObject = new GameObject(
                SelectedPackageCameraObjectName,
                typeof(Camera));
            mSelectedPackageOverlayCamera = cameraObject.GetComponent<Camera>();
        }

        mSelectedPackageSourceCamera = sourceCamera;
        mSelectedPackageSourceCullingMask = sourceCamera.cullingMask;
        sourceCamera.cullingMask &= ~(1 << SelectedPackageRenderLayer);

        mSelectedPackageOverlayCamera.CopyFrom(sourceCamera);
        mSelectedPackageOverlayCamera.transform.SetPositionAndRotation(
            sourceCamera.transform.position,
            sourceCamera.transform.rotation);
        mSelectedPackageOverlayCamera.cullingMask = 1 << SelectedPackageRenderLayer;
        mSelectedPackageOverlayCamera.clearFlags = CameraClearFlags.Depth;
        mSelectedPackageOverlayCamera.depth = sourceCamera.depth + 10f;
        mSelectedPackageOverlayCamera.eventMask = 0;
        mSelectedPackageOverlayCamera.useOcclusionCulling = false;
        mSelectedPackageOverlayCamera.enabled = true;
        mIsSelectedPackageRenderLayerActive = true;
    }

    private void DisableSelectedPackageRenderLayer()
    {
        if (!mIsSelectedPackageRenderLayerActive)
        {
            return;
        }

        if (mSelectedPackageSourceCamera != null)
        {
            mSelectedPackageSourceCamera.cullingMask = mSelectedPackageSourceCullingMask;
        }

        GameAnimationUtility.RestorePreparedCardPackRenderLayers();
        if (mSelectedPackageOverlayCamera != null)
        {
            mSelectedPackageOverlayCamera.enabled = false;
        }

        mSelectedPackageSourceCamera = null;
        mIsSelectedPackageRenderLayerActive = false;
    }

    private void CreateOpeningStageBackground()
    {
        var backgroundObject = new GameObject(
            OpeningStageBackgroundObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        backgroundObject.layer = mBagSelectPanelRoot.layer;
        var rectTransform = backgroundObject.GetComponent<RectTransform>();
        StretchToParent(rectTransform, mBagSelectOverlayCanvas.transform);

        mOpeningStageBackgroundImage = backgroundObject.GetComponent<Image>();
        mOpeningStageBackgroundSprite = GameCommonUtility.LoadSpriteByPath(
            OpeningStageBackgroundPath,
            PixelsPerUnit);
        var fallbackBackground = GameCommonUtility.FindSceneObject(
            GameDefine.BackgroundObjectName)?.GetComponent<Image>();
        mOpeningStageBackgroundImage.sprite = mOpeningStageBackgroundSprite != null
            ? mOpeningStageBackgroundSprite
            : fallbackBackground != null ? fallbackBackground.sprite : null;
        mOpeningStageBackgroundImage.color = Color.white;
        mOpeningStageBackgroundImage.raycastTarget = true;
        backgroundObject.SetActive(false);
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

    private void CreateTearGuideCanvas(Camera camera, Canvas sourceCanvas)
    {
        var canvasObject = new GameObject(
            TearGuideCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.layer = mBagSelectPanelRoot.layer;
        mTearGuideCanvas = canvasObject.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mTearGuideCanvas,
            camera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            SelectedPackageWorldDepth - 0.01f);
        mTearGuideCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mTearGuideCanvas.worldCamera = null;
        mTearGuideCanvas.sortingLayerID = sourceCanvas.sortingLayerID;
        mTearGuideCanvas.sortingOrder = TearGuideSortingOrder;

        var guideObject = new GameObject(
            TearGuideObjectName,
            typeof(RectTransform),
            typeof(CanvasGroup));
        guideObject.layer = mBagSelectPanelRoot.layer;
        mTearGuideRect = guideObject.GetComponent<RectTransform>();
        mTearGuideRect.SetParent(mTearGuideCanvas.transform, false);
        mTearGuideRect.anchorMin = new Vector2(0.5f, 0.5f);
        mTearGuideRect.anchorMax = new Vector2(0.5f, 0.5f);
        mTearGuideRect.pivot = new Vector2(0.5f, 0.5f);
        mTearGuideRect.sizeDelta = new Vector2(112f, 112f);
        mTearGuideCanvasGroup = guideObject.GetComponent<CanvasGroup>();

        mTearGuideCircleSprite = CreateRuntimeCircleSprite(out mTearGuideCircleTexture);
        CreateTearGuideCircle("Halo", mTearGuideCircleSprite, 112f, new Color(0.55f, 0.92f, 1f, 0.42f));
        CreateTearGuideCircle("Core", mTearGuideCircleSprite, 52f, new Color(1f, 1f, 1f, 0.94f));
        guideObject.SetActive(false);

        CreateTearFlash();
    }

    private void CreateTearFlash()
    {
        var flashObject = new GameObject(
            TearFlashObjectName,
            typeof(RectTransform),
            typeof(CanvasGroup));
        flashObject.layer = mTearGuideCanvas.gameObject.layer;
        mTearFlashRect = flashObject.GetComponent<RectTransform>();
        mTearFlashRect.SetParent(mTearGuideCanvas.transform, false);
        mTearFlashRect.anchorMin = new Vector2(0.5f, 0.5f);
        mTearFlashRect.anchorMax = new Vector2(0.5f, 0.5f);
        mTearFlashRect.pivot = new Vector2(0.5f, 0.5f);
        mTearFlashRect.sizeDelta = Vector2.zero;
        mTearFlashCanvasGroup = flashObject.GetComponent<CanvasGroup>();
        mTearFlashCanvasGroup.interactable = false;
        mTearFlashCanvasGroup.blocksRaycasts = false;

        mTearFlashHaloRect = CreateTearFlashImage(
            "Halo",
            new Color(0.68f, 0.93f, 1f, 0.78f));
        mTearFlashCoreRect = CreateTearFlashImage(
            "Core",
            Color.white);
        mTearFlashHeadRect = CreateTearFlashImage(
            "Head",
            Color.white);
        flashObject.SetActive(false);
    }

    private RectTransform CreateTearFlashImage(string objectName, Color color)
    {
        var imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = mTearFlashRect.gameObject.layer;
        var imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(mTearFlashRect, false);
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);

        var image = imageObject.GetComponent<Image>();
        image.sprite = mTearGuideCircleSprite;
        image.color = color;
        image.raycastTarget = false;
        return imageRect;
    }

    private static Sprite CreateRuntimeCircleSprite(out Texture2D texture)
    {
        const int textureSize = 64;
        const float edgeSoftness = 1.5f;
        var center = (textureSize - 1f) * 0.5f;
        var radius = center - 1f;
        var pixels = new Color32[textureSize * textureSize];

        for (var y = 0; y < textureSize; y++)
        {
            for (var x = 0; x < textureSize; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                var alpha = Mathf.Clamp01((radius - distance) / edgeSoftness + 0.5f);
                pixels[y * textureSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "CardPackTearGuideCircleTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize);
        sprite.name = "CardPackTearGuideCircle";
        return sprite;
    }

    private void CreateTearGuideCircle(string objectName, Sprite sprite, float size, Color color)
    {
        var circleObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        circleObject.layer = mBagSelectPanelRoot.layer;
        var circleRect = circleObject.GetComponent<RectTransform>();
        circleRect.SetParent(mTearGuideRect, false);
        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.pivot = new Vector2(0.5f, 0.5f);
        circleRect.anchoredPosition = Vector2.zero;
        circleRect.sizeDelta = new Vector2(size, size);

        var image = circleObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
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

        if (ShouldConfirmReplay(mSelectedBagId) && mReplayPanelRoot != null)
        {
            ShowReplayConfirmation();
            return;
        }

        mIsSelectedPackageReplay = false;
        mPlayAnimationCoroutine = StartCoroutine(EnterCardPackOpeningStage());
    }

    private void ShowReplayConfirmation()
    {
        mIsReplayConfirmationVisible = true;
        SetBagSelectButtonsInteractable(false);
        SetUnselectedPackageVisualsVisible(false);
        GameAnimationUtility.SetPreparedCardPackVisible(false);
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
        if (!CardPackDataUtility.TryClearPuzzleSession(mSelectedBagId))
        {
            Debug.LogWarning(
                $"MainScene: failed to reset puzzle session before replay. packId={mSelectedBagId}");
        }

        GameAnimationUtility.SetPreparedCardPackVisible(true);
        mPlayAnimationCoroutine = StartCoroutine(EnterCardPackOpeningStage());
    }

    private void OnReplayCancelled()
    {
        if (!mIsReplayConfirmationVisible || mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mReplayPanelRoot, false);
        mIsReplayConfirmationVisible = false;
        GameAnimationUtility.SetPreparedCardPackVisible(true);
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

        GameAnimationUtility.SetPreparedCardPackVisible(false);
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
        GameAnimationUtility.SetPreparedCardPackVisible(true);
        SetBagSelectButtonsInteractable(true);
    }

    private bool TryResolvePackageList()
    {
        mPackageItemTemplate = null;
        mLegacyPackageSlotTemplate = null;
        mPackageContentRoot = null;
        mPackagePageTemplate = null;
        mPackageScrollRect = null;
        mUsesPagedPackageGrid = false;
        mPackageSlotsById.Clear();

        return TryResolvePagedPackageList() || TryResolveLegacyPackageList();
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

        mUsesPagedPackageGrid = true;
        mPackageScrollRect.horizontal = true;
        mPackageScrollRect.vertical = false;
        mPackagePageTemplate.gameObject.SetActive(true);
        NormalizePagedPackageLayout();
        return true;
    }

    private bool TryResolveLegacyPackageList()
    {
        var templateObject = GameObject.Find($"{GameDefine.PackageFilePrefix}{MainPackageBagId:D3}");
        if (templateObject == null)
        {
            var images = FindObjectsOfType<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image != null && TryParsePackageObjectName(image.gameObject.name, out _))
                {
                    templateObject = image.gameObject;
                    break;
                }
            }
        }

        if (templateObject == null || !templateObject.TryGetComponent(out mLegacyPackageSlotTemplate))
        {
            return false;
        }

        mPackageContentRoot = mLegacyPackageSlotTemplate.rectTransform.parent as RectTransform;
        mLegacyPackageSlotTemplate.gameObject.SetActive(false);
        return mPackageContentRoot != null;
    }

    private void RefreshPackageList()
    {
        if (mPackageContentRoot == null || (mPackageItemTemplate == null && mLegacyPackageSlotTemplate == null))
        {
            return;
        }

        if (!CardPackDataUtility.Initialize())
        {
            Debug.LogWarning("MainScene: CardPackDataUtility is not ready, package list refresh skipped.");
            return;
        }

        var unlockedPackIds = CardPackDataUtility.TakeMainSceneOrderedPackIds();
        ClearPackageSlots();

        for (var i = 0; i < unlockedPackIds.Count; i++)
        {
            var packId = unlockedPackIds[i];
            var entry = CreatePackageSlot(packId, i);
            if (entry.Image == null)
            {
                continue;
            }

            ApplyPackageSlotVisual(entry, packId);
            if (!mUsesPagedPackageGrid)
            {
                LayoutPackageSlot(entry.RectTransform, i);
            }

            entry.Root.SetActive(true);
            mPackageSlotsById[packId] = entry;
        }

        UpdatePackageContentWidth(unlockedPackIds.Count);
        if (mPackageScrollRect != null)
        {
            mPackageScrollRect.horizontalNormalizedPosition = 0f;
        }

        Debug.Log($"MainScene: package list refreshed. unlocked={unlockedPackIds.Count}");
    }

    private void ClearPackageSlots()
    {
        foreach (var pair in mPackageSlotsById)
        {
            ReleasePackageDisplay(pair.Value);
            ReleasePackageSizeVisual(pair.Value);
            if (pair.Value.Root != null)
            {
                Destroy(pair.Value.Root);
            }
        }

        mPackageSlotsById.Clear();
        if (!mUsesPagedPackageGrid || mPackageContentRoot == null)
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

    private PackageEntry CreatePackageSlot(int packId, int index)
    {
        if (mUsesPagedPackageGrid)
        {
            return CreatePagedPackageSlot(packId, index);
        }

        var slotObject = Instantiate(mLegacyPackageSlotTemplate.gameObject, mPackageContentRoot);
        slotObject.name = $"{GameDefine.PackageFilePrefix}{packId:D3}";
        var image = slotObject.GetComponent<Image>();
        EnsurePackageInteractionHandler(slotObject, image, packId);
        return new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = image,
            RectTransform = image != null ? image.rectTransform : null,
            BreathPhase = packId * 0.6180339f,
            RenderOrder = PackageListSortingOrderBase + index * PackageSortingOrderStride
        };
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

        var coverImage = FindChild(slotObject.transform, PackCoverObjectName)?.GetComponent<Image>() ?? rootImage;
        var shadowImage = FindChild(slotObject.transform, PackShadowObjectName)?.GetComponent<Image>();
        var sizeImage = FindChild(slotObject.transform, PackSizeObjectName)?.GetComponent<Image>();
        var effectRoot = FindChild(slotObject.transform, PackEffectObjectName)?.gameObject;
        if (effectRoot != null)
        {
            effectRoot.SetActive(false);
        }
        PreparePagedPackageItem(slotObject, rootRect, rootImage, coverImage, shadowImage, sizeImage);
        EnsurePackageInteractionHandler(slotObject, coverImage, packId);

        var entry = new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = coverImage,
            ShadowImage = shadowImage,
            SizeImage = sizeImage,
            EffectRoot = effectRoot,
            RectTransform = rootRect,
            SizeBaseAnchoredPosition = sizeImage != null
                ? sizeImage.rectTransform.anchoredPosition
                : Vector2.zero,
            SizeBaseLocalScale = sizeImage != null
                ? sizeImage.rectTransform.localScale
                : Vector3.one,
            BreathPhase = packId * 0.6180339f,
            RenderOrder = PackageListSortingOrderBase + index * PackageSortingOrderStride
        };
        AttachPackageSizeToCanvas(entry);
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
        entry.Image.raycastTarget = !mUsesPagedPackageGrid;
        var packImagePath = GameDefine.FormatPackImagePath(packId);
        var packSprite = GameCommonUtility.LoadSpriteByPath(packImagePath, PixelsPerUnit);
        if (packSprite != null)
        {
            entry.Image.sprite = packSprite;
        }

        if (entry.ShadowImage != null)
        {
            entry.ShadowImage.sprite = GetOrCreatePackageShadowSprite(packId, entry.Image.sprite);
            var showShadow = entry.ShadowImage.sprite != null;
            entry.ShadowImage.enabled = showShadow;
            entry.ShadowImage.gameObject.SetActive(showShadow);
        }

        ApplyPackageSizeVisual(entry.SizeImage, packId);
        ApplyPackageLifecycleVisual(entry, packId);
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

        if (!mUsesPagedPackageGrid && entry.RectTransform != null)
        {
            entry.RectTransform.sizeDelta = new Vector2(PackageSlotWidth, PackageSlotHeight);
        }

        EnsurePackageInteractionHandler(entry.Root, entry.Image, packId);
    }

    private void UpdatePackageDisplays()
    {
        if (mPackageSlotsById.Count == 0 || Camera.main == null)
        {
            return;
        }

        var viewport = mPackageScrollRect != null ? mPackageScrollRect.viewport : null;
        var clipRect = viewport != null
            ? GetScreenRect(viewport, Camera.main)
            : new Rect(0f, 0f, Screen.width, Screen.height);
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
            if (shouldRender && entry.IdleDisplay == null)
            {
                if (GameAnimationUtility.TryBindCardPackIdleDisplay(
                    entry.EffectRoot,
                    entry.BagId,
                    entry.Image.sprite,
                    entry.RenderOrder,
                    out var display))
                {
                    entry.IdleDisplay = display;
                    entry.EffectRoot = null;
                    SetPackageCoverAndShadowVisible(entry, false);
                }
            }

            if (entry.IdleDisplay != null && entry != mSelectedPackageEntry)
            {
                GameAnimationUtility.UpdateCardPackIdleDisplay(
                    entry.IdleDisplay,
                    anchor,
                    clipRect,
                    GetPackageBreathScale(entry),
                    shouldRender);
            }

            if (entry != mSelectedPackageEntry)
            {
                UpdatePackageSizeImage(entry, GetPackageBreathScale(entry), shouldRender);
            }
        }
    }

    private void UpdatePackageSizeImage(PackageEntry entry, float scaleMultiplier, bool visible)
    {
        if (entry?.SizeImage == null)
        {
            return;
        }

        var sizeImage = entry.SizeImage;
        var sizeRect = sizeImage.rectTransform;
        var multiplier = Mathf.Max(0.001f, scaleMultiplier);
        sizeImage.enabled = visible && sizeImage.sprite != null;
        if (!visible
            || mPackageSizeCanvasRect == null
            || entry.Image == null)
        {
            return;
        }

        var anchorRect = entry.Image.rectTransform;
        var sourceCanvas = anchorRect.GetComponentInParent<Canvas>();
        var sourceCamera = sourceCanvas != null
            && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;
        var screenCenter = RectTransformUtility.WorldToScreenPoint(
            sourceCamera,
            anchorRect.TransformPoint(anchorRect.rect.center));
        var targetCamera = mPackageSizeCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : mPackageSizeCanvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mPackageSizeCanvasRect,
                screenCenter,
                targetCamera,
                out var canvasCenter))
        {
            sizeImage.enabled = false;
            return;
        }

        var anchoredPosition = canvasCenter + entry.SizeBaseAnchoredPosition * multiplier;
        sizeRect.anchoredPosition3D = new Vector3(anchoredPosition.x, anchoredPosition.y, 0f);
        sizeRect.localScale = entry.SizeBaseLocalScale * multiplier;
    }

    private void CreatePackageSizeCanvas(Camera targetCamera)
    {
        if (targetCamera == null || mPackageSizeCanvas != null)
        {
            return;
        }

        var sourceCanvas = mPackageScrollRect != null
            ? mPackageScrollRect.GetComponentInParent<Canvas>()
            : null;
        var canvasObject = new GameObject(
            PackageSizeCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.layer = sourceCanvas != null ? sourceCanvas.gameObject.layer : 5;
        mPackageSizeCanvas = canvasObject.GetComponent<Canvas>();
        mPackageSizeCanvasRect = canvasObject.GetComponent<RectTransform>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            mPackageSizeCanvas,
            targetCamera,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit,
            PackageSizeWorldDepth);
        mPackageSizeCanvas.sortingLayerID = sourceCanvas != null
            ? sourceCanvas.sortingLayerID
            : 0;
        mPackageSizeCanvas.sortingOrder = PackageSizeCanvasSortingOrder;

        var uiShader = Shader.Find("UI/Default");
        if (uiShader == null)
        {
            Debug.LogWarning("MainScene: pack size icons could not create their UI material because UI/Default is missing.");
            return;
        }

        mPackageSizeOverlayMaterial = new Material(uiShader)
        {
            name = "CardPackSizeOverlayMaterial",
            hideFlags = HideFlags.DontSave
        };
        mPackageSizeOverlayMaterial.SetInt(
            "unity_GUIZTestMode",
            (int)CompareFunction.Always);
    }

    private void AttachPackageSizeToCanvas(PackageEntry entry)
    {
        if (entry?.SizeImage == null || mPackageSizeCanvasRect == null)
        {
            return;
        }

        var sizeRect = entry.SizeImage.rectTransform;
        sizeRect.SetParent(mPackageSizeCanvasRect, false);
        sizeRect.anchorMin = new Vector2(0.5f, 0.5f);
        sizeRect.anchorMax = new Vector2(0.5f, 0.5f);
        sizeRect.localRotation = Quaternion.identity;
        sizeRect.localScale = entry.SizeBaseLocalScale;
        if (mPackageSizeOverlayMaterial != null)
        {
            entry.SizeImage.material = mPackageSizeOverlayMaterial;
        }
    }

    private static void ReleasePackageSizeVisual(PackageEntry entry)
    {
        if (entry?.SizeImage == null)
        {
            return;
        }

        if (entry.Root == null || !entry.SizeImage.transform.IsChildOf(entry.Root.transform))
        {
            Destroy(entry.SizeImage.gameObject);
        }

        entry.SizeImage = null;
    }

    private bool IsAnyPackagePanelOpen()
    {
        return mMenuPanelRoot != null && mMenuPanelRoot.activeInHierarchy
            || mSettingsPanelRoot != null && mSettingsPanelRoot.activeInHierarchy
            || mUsablePanelRoot != null && mUsablePanelRoot.activeInHierarchy
            || mSavePanelRoot != null && mSavePanelRoot.activeInHierarchy;
    }

    private static float GetPackageBreathScale(PackageEntry entry)
    {
        var phase = entry != null ? entry.BreathPhase : 0f;
        var normalized = Mathf.Sin(
            (Time.unscaledTime / PackageBreathDuration + phase) * Mathf.PI * 2f) * 0.5f + 0.5f;
        return Mathf.Lerp(PackageBreathMinScale, PackageBreathMaxScale, normalized);
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

    private static void SetPackageCoverAndShadowVisible(PackageEntry entry, bool visible)
    {
        if (entry == null)
        {
            return;
        }

        if (entry.Image != null)
        {
            entry.Image.enabled = visible;
        }

        if (entry.ShadowImage != null)
        {
            entry.ShadowImage.enabled = visible && entry.ShadowImage.sprite != null;
        }
    }

    private static void SetPackageSizeImageVisible(PackageEntry entry, bool visible)
    {
        if (entry?.SizeImage != null)
        {
            entry.SizeImage.enabled = visible && entry.SizeImage.sprite != null;
        }
    }

    private static void ReleasePackageDisplay(PackageEntry entry)
    {
        if (entry == null || entry.IdleDisplay == null)
        {
            return;
        }

        GameAnimationUtility.DestroyCardPackIdleDisplay(entry.IdleDisplay);
        entry.IdleDisplay = null;
    }

    private static void ApplyPackageLifecycleVisual(PackageEntry entry, int packId)
    {
        if (entry.Image != null)
        {
            entry.Image.color = Color.white;
        }

        if (entry.SizeImage != null)
        {
            entry.SizeImage.color = Color.white;
        }
    }

    private static void ApplyPackageSizeVisual(Image sizeImage, int packId)
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

        var sizeSprite = GameCommonUtility.LoadSpriteByPath(
            GameDefine.FormatPackSizeImagePath(config.PackSize),
            PixelsPerUnit);
        if (sizeSprite == null)
        {
            sizeImage.gameObject.SetActive(false);
            return;
        }

        sizeImage.sprite = sizeSprite;
        sizeImage.enabled = true;
        sizeImage.gameObject.SetActive(true);
    }

    private void LayoutPackageSlot(RectTransform rectTransform, int index)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        var x = PackageContentHorizontalPadding
            + index * (PackageSlotWidth + PackageHorizontalSpacing)
            + PackageSlotWidth * 0.5f;
        rectTransform.anchoredPosition = new Vector2(x, 0f);
    }

    private void UpdatePackageContentWidth(int visibleCount)
    {
        if (mUsesPagedPackageGrid)
        {
            NormalizePagedPackageLayout();
            Canvas.ForceUpdateCanvases();
            if (mPackageContentRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(mPackageContentRoot);
            }

            return;
        }

        if (mPackageContentRoot == null || visibleCount <= 0)
        {
            return;
        }

        var contentWidth = PackageContentHorizontalPadding * 2f
            + visibleCount * PackageSlotWidth
            + Mathf.Max(0, visibleCount - 1) * PackageHorizontalSpacing;
        mPackageContentRoot.sizeDelta = new Vector2(contentWidth, mPackageContentRoot.sizeDelta.y);
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
        grid.childAlignment = TextAnchor.UpperLeft;
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

        visualImage.raycastTarget = !mUsesPagedPackageGrid;
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

    private void ConfigurePackageCanvas(Camera targetCamera)
    {
        if (targetCamera == null)
        {
            return;
        }

        Canvas canvas = null;
        if (mPackageScrollRect != null)
        {
            canvas = mPackageScrollRect.GetComponentInParent<Canvas>();
        }

        if (canvas == null && mLegacyPackageSlotTemplate != null)
        {
            canvas = mLegacyPackageSlotTemplate.canvas;
        }

        if (canvas != null)
        {
            GameCommonUtility.ConfigureCanvasForWorldCardPack(canvas, targetCamera);
        }
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
        Image shadowImage,
        Image sizeImage)
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
            ScaleOverlayWithCover(sizeImage != null ? sizeImage.rectTransform : null, coverRect.sizeDelta);
            ScaleOverlayWithCover(shadowImage != null ? shadowImage.rectTransform : null, coverRect.sizeDelta);
            coverRect.anchorMin = new Vector2(0.5f, 0.5f);
            coverRect.anchorMax = new Vector2(0.5f, 0.5f);
            coverRect.pivot = new Vector2(0.5f, 0.5f);
            coverRect.anchoredPosition = Vector2.zero;
            coverRect.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);
            coverRect.localScale = Vector3.one;
        }

        ConfigurePackShadow(shadowImage);

        var nameText = FindChild(itemObject.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
            nameText.raycastTarget = false;
            nameText.alignment = TextAlignmentOptions.Center;
            GameFontUtility.ApplyDefaultFont(nameText);
        }
    }

    private static void ConfigurePackShadow(Image shadowImage)
    {
        if (shadowImage == null)
        {
            return;
        }

        shadowImage.raycastTarget = false;
        shadowImage.preserveAspect = false;
        shadowImage.color = Color.white;
        shadowImage.material = null;
    }

    private Sprite GetOrCreatePackageShadowSprite(int packId, Sprite coverSprite)
    {
        if (mPackageShadowSpritesById.TryGetValue(packId, out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var shadowSprite = CreatePackageShadowSprite(packId, coverSprite);
        if (shadowSprite != null)
        {
            mPackageShadowSpritesById[packId] = shadowSprite;
        }

        return shadowSprite;
    }

    private static Sprite CreatePackageShadowSprite(int packId, Sprite coverSprite)
    {
        if (coverSprite == null || coverSprite.texture == null)
        {
            return null;
        }

        Color32[] sourcePixels;
        Rect sourceRect;
        try
        {
            sourcePixels = coverSprite.texture.GetPixels32();
            sourceRect = coverSprite.textureRect;
        }
        catch (UnityException exception)
        {
            Debug.LogWarning($"MainScene: pack shadow skipped for packId={packId}. {exception.Message}");
            return null;
        }

        var sourceTexture = coverSprite.texture;
        var contentWidth = Mathf.RoundToInt(PackageCoverWidth);
        var contentHeight = Mathf.RoundToInt(PackageCoverHeight);
        var horizontalPadding = Mathf.RoundToInt(PackageShadowHorizontalPadding);
        var verticalPadding = Mathf.RoundToInt(PackageShadowVerticalPadding);
        var shadowWidth = contentWidth + horizontalPadding * 2;
        var shadowHeight = contentHeight + verticalPadding * 2;
        var alpha = new float[shadowWidth * shadowHeight];
        SampleSpriteAlpha(
            sourcePixels,
            sourceTexture.width,
            sourceTexture.height,
            sourceRect,
            alpha,
            shadowWidth,
            contentWidth,
            contentHeight,
            horizontalPadding,
            verticalPadding);

        var scratch = new float[alpha.Length];
        for (var pass = 0; pass < PackageShadowBlurPassCount; pass++)
        {
            BoxBlurHorizontal(
                alpha,
                scratch,
                shadowWidth,
                shadowHeight,
                PackageShadowHorizontalBlurRadius);
            BoxBlurVertical(
                scratch,
                alpha,
                shadowWidth,
                shadowHeight,
                PackageShadowVerticalBlurRadius);
        }

        var shadowColor = new Color32(31, 41, 45, 0);
        var outputPixels = new Color32[alpha.Length];
        for (var i = 0; i < outputPixels.Length; i++)
        {
            shadowColor.a = (byte)Mathf.RoundToInt(
                Mathf.Clamp01(alpha[i]) * PackageShadowAlpha * byte.MaxValue);
            outputPixels[i] = shadowColor;
        }

        var shadowTexture = new Texture2D(shadowWidth, shadowHeight, TextureFormat.RGBA32, false)
        {
            name = $"PackShadow_{packId:D3}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        shadowTexture.SetPixels32(outputPixels);
        shadowTexture.Apply(false, true);

        var shadowSprite = Sprite.Create(
            shadowTexture,
            new Rect(0f, 0f, shadowWidth, shadowHeight),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        shadowSprite.name = shadowTexture.name;
        return shadowSprite;
    }

    private static void SampleSpriteAlpha(
        Color32[] sourcePixels,
        int sourceTextureWidth,
        int sourceTextureHeight,
        Rect sourceRect,
        float[] targetAlpha,
        int targetWidth,
        int contentWidth,
        int contentHeight,
        int horizontalPadding,
        int verticalPadding)
    {
        for (var y = 0; y < contentHeight; y++)
        {
            var sourceY = sourceRect.y + (y + 0.5f) * sourceRect.height / contentHeight - 0.5f;
            var y0 = Mathf.Clamp(Mathf.FloorToInt(sourceY), 0, sourceTextureHeight - 1);
            var y1 = Mathf.Min(y0 + 1, sourceTextureHeight - 1);
            var lerpY = sourceY - Mathf.Floor(sourceY);
            for (var x = 0; x < contentWidth; x++)
            {
                var sourceX = sourceRect.x + (x + 0.5f) * sourceRect.width / contentWidth - 0.5f;
                var x0 = Mathf.Clamp(Mathf.FloorToInt(sourceX), 0, sourceTextureWidth - 1);
                var x1 = Mathf.Min(x0 + 1, sourceTextureWidth - 1);
                var lerpX = sourceX - Mathf.Floor(sourceX);
                var alpha00 = sourcePixels[y0 * sourceTextureWidth + x0].a / 255f;
                var alpha10 = sourcePixels[y0 * sourceTextureWidth + x1].a / 255f;
                var alpha01 = sourcePixels[y1 * sourceTextureWidth + x0].a / 255f;
                var alpha11 = sourcePixels[y1 * sourceTextureWidth + x1].a / 255f;
                var lowerAlpha = Mathf.Lerp(alpha00, alpha10, lerpX);
                var upperAlpha = Mathf.Lerp(alpha01, alpha11, lerpX);
                targetAlpha[(y + verticalPadding) * targetWidth + x + horizontalPadding] = Mathf.Lerp(
                    lowerAlpha,
                    upperAlpha,
                    lerpY);
            }
        }
    }

    private static void BoxBlurHorizontal(float[] source, float[] target, int width, int height, int radius)
    {
        var sampleCount = radius * 2 + 1;
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;
            var sum = 0f;
            for (var sampleX = -radius; sampleX <= radius; sampleX++)
            {
                if (sampleX >= 0 && sampleX < width)
                {
                    sum += source[rowStart + sampleX];
                }
            }

            for (var x = 0; x < width; x++)
            {
                target[rowStart + x] = sum / sampleCount;
                var removeX = x - radius;
                var addX = x + radius + 1;
                if (removeX >= 0)
                {
                    sum -= source[rowStart + removeX];
                }

                if (addX < width)
                {
                    sum += source[rowStart + addX];
                }
            }
        }
    }

    private static void BoxBlurVertical(float[] source, float[] target, int width, int height, int radius)
    {
        var sampleCount = radius * 2 + 1;
        for (var x = 0; x < width; x++)
        {
            var sum = 0f;
            for (var sampleY = -radius; sampleY <= radius; sampleY++)
            {
                if (sampleY >= 0 && sampleY < height)
                {
                    sum += source[sampleY * width + x];
                }
            }

            for (var y = 0; y < height; y++)
            {
                target[y * width + x] = sum / sampleCount;
                var removeY = y - radius;
                var addY = y + radius + 1;
                if (removeY >= 0)
                {
                    sum -= source[removeY * width + x];
                }

                if (addY < height)
                {
                    sum += source[addY * width + x];
                }
            }
        }
    }

    private static void ScaleOverlayWithCover(RectTransform overlayRect, Vector2 sourceCoverSize)
    {
        if (overlayRect == null || sourceCoverSize.x <= 0f || sourceCoverSize.y <= 0f)
        {
            return;
        }

        var scale = new Vector2(
            PackageCoverWidth / sourceCoverSize.x,
            PackageCoverHeight / sourceCoverSize.y);
        overlayRect.anchoredPosition = Vector2.Scale(overlayRect.anchoredPosition, scale);
        overlayRect.sizeDelta = Vector2.Scale(overlayRect.sizeDelta, scale);
        overlayRect.localScale = Vector3.one;
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

    private static IEnumerator WaitForCardPackAnimation(Transform anchor)
    {
        var duration = GameAnimationUtility.GetCardPackPlayDuration(anchor);
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        yield return new WaitForSecondsRealtime(1.5f);
    }

    private static IEnumerator AnimatePreparedCardPack(
        float fromScale,
        float toScale,
        Vector3 fromCenter,
        Vector3 toCenter,
        float duration = PackageOpenScaleDuration)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            GameAnimationUtility.SetPreparedCardPackPose(
                Mathf.LerpUnclamped(fromScale, toScale, eased),
                Vector3.LerpUnclamped(fromCenter, toCenter, eased));
            yield return null;
        }

        GameAnimationUtility.SetPreparedCardPackPose(toScale, toCenter);
    }

    private Vector3 GetBagSelectCenterWorld(Vector3 fallback)
    {
        var camera = Camera.main;
        var panelRect = mBagSelectPanelRoot != null
            ? mBagSelectPanelRoot.transform as RectTransform
            : null;
        if (camera == null || panelRect == null)
        {
            return fallback;
        }

        return GameCommonUtility.RectTransformToCameraWorld(panelRect, camera, 0f);
    }

    private IEnumerator PlayPackageClickFallback(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            yield break;
        }

        var originalScale = rectTransform.localScale;
        var targetScale = originalScale * PackageClickScaleRatio;
        var elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            rectTransform.localScale = Vector3.LerpUnclamped(originalScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            rectTransform.localScale = Vector3.LerpUnclamped(targetScale, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    private static void SetPackageVisualsVisible(PackageEntry entry, bool visible)
    {
        if (entry == null)
        {
            return;
        }

        entry.SuppressDisplay = !visible;
        if (visible && entry.IdleDisplay != null && entry.IdleDisplay.IsValid)
        {
            SetPackageCoverAndShadowVisible(entry, false);
        }
        else
        {
            SetPackageCoverAndShadowVisible(entry, visible);
        }

        SetPackageSizeImageVisible(entry, visible);

        GameAnimationUtility.SetCardPackIdleDisplayVisible(entry.IdleDisplay, visible);
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
        var coverSprite = entry.Image != null ? entry.Image.sprite : null;
        var idleScale = GetPackageBreathScale(entry);
        SetPackageVisualsVisible(entry, false);
        yield return CaptureBagSelectBackdrop();
        var prepared = GameAnimationUtility.PrepareCardPackAnimation(
            entry.IdleDisplay,
            bagId,
            coverSprite,
            anchor,
            idleScale);
        if (prepared)
        {
            EnableSelectedPackageRenderLayer();
            GameAnimationUtility.TryGetPreparedCardPackCenter(out mSelectedPackageStartCenter);
            mSelectedPackageStartCenter.z = SelectedPackageWorldDepth;
            mSelectedPackageStartScale = idleScale;
            mSelectedPackageOpenScale = Mathf.Min(
                PackageOpenWidth / PackageCoverWidth,
                PackageOpenHeight / PackageCoverHeight);
            mSelectedPackageDisplayCenter = GetBagSelectCenterWorld(mSelectedPackageStartCenter);
            mSelectedPackageDisplayCenter.z = SelectedPackageWorldDepth;
            GameAnimationUtility.SetPreparedCardPackSortingOrder(SelectedPackageSortingOrder);
            GameAnimationUtility.SetPreparedCardPackPose(
                mSelectedPackageStartScale,
                mSelectedPackageStartCenter);
            RefreshBagSelectPackState(bagId);
            SetBagSelectBackdropVisible(true);
            SetBagSelectPanelVisible(true);
            SetBagSelectButtonsInteractable(false);
            yield return AnimatePreparedCardPack(
                mSelectedPackageStartScale,
                mSelectedPackageOpenScale,
                mSelectedPackageStartCenter,
                mSelectedPackageDisplayCenter);
            SetBagSelectButtonsInteractable(true);
        }

        if (!prepared)
        {
            Debug.LogWarning($"Card pack selection preview not prepared. packId={bagId}");
            SetBagSelectBackdropVisible(false);
            ReleaseBagSelectBackdropTexture();
            SetBagSelectPanelVisible(false);
            SetPackageVisualsVisible(entry, true);
            var fallbackRect = entry.RectTransform != null
                ? entry.RectTransform
                : anchor;
            if (fallbackRect != null)
            {
                yield return PlayPackageClickFallback(fallbackRect);
            }

            ClearPackageSelection();
            mIsPlayingAnimation = false;
            mPlayAnimationCoroutine = null;
            mHasSwitchedToGameScene = true;
            GameManager.EnterGameScene(bagId);
            yield break;
        }

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

        var blurLevels = new List<RenderTexture>(BagSelectBlurPyramidLevels);
        try
        {
            Texture blurSource = screenshot;
            for (var level = 0; level < BagSelectBlurPyramidLevels; level++)
            {
                var downsample = BagSelectBlurDownsample << level;
                var levelWidth = Mathf.Max(1, screenshot.width / downsample);
                var levelHeight = Mathf.Max(1, screenshot.height / downsample);
                var blurLevel = RenderTexture.GetTemporary(
                    levelWidth,
                    levelHeight,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Default);
                blurLevel.filterMode = FilterMode.Bilinear;
                blurLevel.wrapMode = TextureWrapMode.Clamp;
                Graphics.Blit(blurSource, blurLevel);
                blurLevels.Add(blurLevel);
                blurSource = blurLevel;
            }

            for (var level = blurLevels.Count - 2; level >= 0; level--)
            {
                Graphics.Blit(blurSource, blurLevels[level]);
                blurSource = blurLevels[level];
            }

            mBagSelectBackdropTexture = new RenderTexture(
                blurLevels[0].width,
                blurLevels[0].height,
                0,
                RenderTextureFormat.ARGB32)
            {
                name = "BagSelectBlurredBackdropTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            mBagSelectBackdropTexture.Create();
            Graphics.Blit(blurSource, mBagSelectBackdropTexture);
        }
        finally
        {
            for (var i = 0; i < blurLevels.Count; i++)
            {
                RenderTexture.ReleaseTemporary(blurLevels[i]);
            }

            Destroy(screenshot);
        }

        mBagSelectBackdropImage.texture = mBagSelectBackdropTexture;
        mBagSelectBackdropImage.color = Color.white;
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
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        SetUnselectedPackageVisualsVisible(false);
        if (mOpeningStageBackgroundImage != null)
        {
            mOpeningStageBackgroundImage.gameObject.SetActive(true);
            SetOpeningStageBackgroundAlpha(0f);
        }

        if (mBagSelectPanelCanvasGroup != null)
        {
            mBagSelectPanelCanvasGroup.alpha = 1f;
            mBagSelectPanelCanvasGroup.interactable = false;
            mBagSelectPanelCanvasGroup.blocksRaycasts = false;
        }

        mSelectedPackageStageScale = mSelectedPackageOpenScale * OpeningStageScaleRatio;
        var elapsed = 0f;
        while (elapsed < OpeningStageTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / OpeningStageTransitionDuration);
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            SetOpeningStageBackgroundAlpha(eased);
            if (mMainCanvasGroup != null)
            {
                mMainCanvasGroup.alpha = 1f - eased;
            }

            if (mBagSelectPanelCanvasGroup != null)
            {
                mBagSelectPanelCanvasGroup.alpha = 1f - eased;
            }

            if (mBagSelectBackdropImage != null)
            {
                var color = mBagSelectBackdropImage.color;
                color.a = 1f - eased;
                mBagSelectBackdropImage.color = color;
            }

            GameAnimationUtility.SetPreparedCardPackPose(
                Mathf.LerpUnclamped(mSelectedPackageOpenScale, mSelectedPackageStageScale, eased),
                mSelectedPackageDisplayCenter);
            yield return null;
        }

        SetOpeningStageBackgroundAlpha(1f);
        SetBagSelectPanelVisible(false);
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        if (mMainCanvasGroup != null)
        {
            mMainCanvasGroup.alpha = 0f;
            mMainCanvasGroup.interactable = false;
            mMainCanvasGroup.blocksRaycasts = false;
        }

        var punchScale = mSelectedPackageStageScale * OpeningStagePunchScaleRatio;
        var halfSettleDuration = OpeningStageSettleDuration * 0.5f;
        yield return AnimatePreparedCardPack(
            mSelectedPackageStageScale,
            punchScale,
            mSelectedPackageDisplayCenter,
            mSelectedPackageDisplayCenter,
            halfSettleDuration);
        yield return AnimatePreparedCardPack(
            punchScale,
            mSelectedPackageStageScale,
            mSelectedPackageDisplayCenter,
            mSelectedPackageDisplayCenter,
            halfSettleDuration);

        mIsPlayingAnimation = false;
        mIsAwaitingTearSwipe = true;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        mPlayAnimationCoroutine = null;
        StartTearGuide();
    }

    private IEnumerator PlaySelectedPackage()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        var selectedEntry = mSelectedPackageEntry;
        var selectedBagId = mSelectedBagId;
        var isReplaySession = mIsSelectedPackageReplay;
        var anchor = selectedEntry != null && selectedEntry.Image != null
            ? selectedEntry.Image.rectTransform
            : selectedEntry?.RectTransform;

        var tearFlashCoroutine = StartCoroutine(PlayTearFlashLine(TearTrailTravelDuration));
        yield return GameAnimationUtility.PlayPreparedCardPackTearTrailEffect(
            TearGuideSortingOrder + 20,
            TearTrailTravelDuration);
        if (tearFlashCoroutine != null)
        {
            yield return tearFlashCoroutine;
        }

        if (GameAnimationUtility.PlayPreparedCardPackAnimation())
        {
            GameAnimationUtility.PlayPreparedCardPackDismantleEffect(
                TearGuideSortingOrder + 10);
            yield return WaitForCardPackAnimation(anchor);
        }
        else
        {
            Debug.LogWarning($"Card pack animation not played. packId={selectedBagId}");
            GameAnimationUtility.SetPreparedCardPackVisible(false);
            SetBagSelectPanelVisible(false);
            SetPackageVisualsVisible(selectedEntry, true);
            if (anchor != null)
            {
                yield return PlayPackageClickFallback(anchor);
            }
        }

        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(
            selectedBagId,
            playEntranceAnimation: true,
            isReplaySession: isReplaySession);
    }

    private IEnumerator HidePackageSelection()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        SetBagSelectPanelVisible(false);
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        yield return AnimatePreparedCardPack(
            mSelectedPackageOpenScale,
            mSelectedPackageStartScale,
            mSelectedPackageDisplayCenter,
            mSelectedPackageStartCenter);

        var selectedEntry = mSelectedPackageEntry;
        GameAnimationUtility.SetPreparedCardPackVisible(false);
        SetPackageVisualsVisible(selectedEntry, true);
        ClearPackageSelection();
        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
    }

    private void SetBagSelectButtonsInteractable(bool interactable)
    {
        if (mBagSelectPlayButton != null)
        {
            mBagSelectPlayButton.interactable = interactable;
        }

        if (mBagSelectBackButton != null)
        {
            mBagSelectBackButton.interactable = interactable;
        }

        if (mBagSelectCameraButton != null)
        {
            mBagSelectCameraButton.interactable = interactable;
        }
    }

    private void RefreshBagSelectPackState(int bagId)
    {
        var isCompleted = CardPackDataUtility.IsPackCompleted(bagId);
        var shouldConfirmReplay = ShouldConfirmReplay(bagId);
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

    private static bool ShouldConfirmReplay(int bagId)
    {
        return CardPackDataUtility.IsPackCompleted(bagId)
            && !CardPackDataUtility.HasActivePuzzleSession(bagId);
    }

    private void SetBagSelectPanelVisible(bool visible)
    {
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

    private void SetOpeningStageBackgroundAlpha(float alpha)
    {
        if (mOpeningStageBackgroundImage == null)
        {
            return;
        }

        var color = mOpeningStageBackgroundImage.color;
        color.a = Mathf.Clamp01(alpha);
        mOpeningStageBackgroundImage.color = color;
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

    private void StartTearGuide()
    {
        StopTearGuide();
        mTearGuideCoroutine = StartCoroutine(AnimateTearGuide());
    }

    private void StopTearGuide()
    {
        if (mTearGuideCoroutine != null)
        {
            StopCoroutine(mTearGuideCoroutine);
            mTearGuideCoroutine = null;
        }

        if (mTearGuideRect != null)
        {
            mTearGuideRect.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayTearFlashLine(float duration)
    {
        if (mTearFlashRect == null
            || mTearFlashHaloRect == null
            || mTearFlashCoreRect == null
            || mTearFlashHeadRect == null
            || !TryRefreshTearSwipeGeometry(out var tearSeamScreenY))
        {
            yield break;
        }

        var startScreen = new Vector2(
            Mathf.Lerp(mTearSwipeScreenRect.xMin, mTearSwipeScreenRect.xMax, 0.04f),
            tearSeamScreenY);
        var endScreen = new Vector2(
            Mathf.Lerp(mTearSwipeScreenRect.xMin, mTearSwipeScreenRect.xMax, 0.96f),
            startScreen.y);
        if (!TryScreenPointToTearGuideLocal(startScreen, out var startLocal)
            || !TryScreenPointToTearGuideLocal(endScreen, out var endLocal))
        {
            yield break;
        }

        mTearFlashRect.gameObject.SetActive(true);
        var animationDuration = Mathf.Max(0.05f, duration);
        var elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / animationDuration);
            var headProgress = Mathf.SmoothStep(0f, 1f, normalized);
            var tailProgress = normalized <= 0.34f
                ? 0f
                : Mathf.SmoothStep(0f, 1f, (normalized - 0.34f) / 0.66f);
            var head = Vector2.LerpUnclamped(startLocal, endLocal, headProgress);
            var tail = Vector2.LerpUnclamped(startLocal, endLocal, tailProgress);
            var center = (head + tail) * 0.5f;
            var lineWidth = Mathf.Max(34f, head.x - tail.x + 34f);

            mTearFlashHaloRect.anchoredPosition = center;
            mTearFlashHaloRect.sizeDelta = new Vector2(lineWidth + 58f, 72f);
            mTearFlashCoreRect.anchoredPosition = center;
            mTearFlashCoreRect.sizeDelta = new Vector2(lineWidth + 12f, 26f);
            mTearFlashHeadRect.anchoredPosition = head;
            mTearFlashHeadRect.sizeDelta = new Vector2(82f, 82f);
            if (mTearFlashCanvasGroup != null)
            {
                var fadeIn = Mathf.Clamp01(normalized / 0.04f);
                var fadeOut = Mathf.Clamp01((1f - normalized) / 0.08f);
                mTearFlashCanvasGroup.alpha = Mathf.Min(fadeIn, fadeOut);
            }

            yield return null;
        }

        mTearFlashRect.gameObject.SetActive(false);
    }

    private IEnumerator AnimateTearGuide()
    {
        while (mIsAwaitingTearSwipe)
        {
            if (mIsTrackingTearSwipe
                || !TryRefreshTearSwipeGeometry(out var tearSeamScreenY))
            {
                if (mTearGuideRect != null)
                {
                    mTearGuideRect.gameObject.SetActive(false);
                }

                yield return null;
                continue;
            }

            var startScreen = new Vector2(
                Mathf.Lerp(mTearSwipeScreenRect.xMin, mTearSwipeScreenRect.xMax, 0.14f),
                tearSeamScreenY);
            var endScreen = new Vector2(
                Mathf.Lerp(mTearSwipeScreenRect.xMin, mTearSwipeScreenRect.xMax, 0.86f),
                startScreen.y);
            if (!TryScreenPointToTearGuideLocal(startScreen, out var startLocal)
                || !TryScreenPointToTearGuideLocal(endScreen, out var endLocal))
            {
                yield return null;
                continue;
            }

            mTearGuideRect.gameObject.SetActive(true);
            var elapsed = 0f;
            while (elapsed < TearGuideTravelDuration
                && mIsAwaitingTearSwipe
                && !mIsTrackingTearSwipe)
            {
                elapsed += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(elapsed / TearGuideTravelDuration);
                mTearGuideRect.anchoredPosition = Vector2.LerpUnclamped(
                    startLocal,
                    endLocal,
                    Mathf.SmoothStep(0f, 1f, normalized));
                if (mTearGuideCanvasGroup != null)
                {
                    var fadeIn = Mathf.Clamp01(normalized / 0.16f);
                    var fadeOut = Mathf.Clamp01((1f - normalized) / 0.16f);
                    mTearGuideCanvasGroup.alpha = Mathf.Min(fadeIn, fadeOut);
                }

                yield return null;
            }

            mTearGuideRect.gameObject.SetActive(false);
            if (!mIsAwaitingTearSwipe || mIsTrackingTearSwipe)
            {
                continue;
            }

            yield return new WaitForSecondsRealtime(TearGuidePauseDuration);
        }

        mTearGuideCoroutine = null;
    }

    private bool TryRefreshTearSwipeScreenRect()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        Bounds bounds;
        if (!GameAnimationUtility.TryGetPreparedCardPackWorldBounds(out bounds)
            || bounds.size.x <= 0.001f
            || bounds.size.y <= 0.001f)
        {
            var stageRatio = mSelectedPackageOpenScale > 0.001f
                ? mSelectedPackageStageScale / mSelectedPackageOpenScale
                : OpeningStageScaleRatio;
            var expectedWorldSize = new Vector3(
                PackageOpenWidth / PixelsPerUnit * stageRatio,
                PackageOpenHeight / PixelsPerUnit * stageRatio,
                0.01f);
            bounds = new Bounds(mSelectedPackageDisplayCenter, expectedWorldSize);
        }

        var minimum = camera.WorldToScreenPoint(new Vector3(
            bounds.min.x,
            bounds.min.y,
            bounds.center.z));
        var maximum = camera.WorldToScreenPoint(new Vector3(
            bounds.max.x,
            bounds.max.y,
            bounds.center.z));
        if (maximum.x <= minimum.x || maximum.y <= minimum.y)
        {
            return false;
        }

        mTearSwipeScreenRect = Rect.MinMaxRect(
            minimum.x,
            minimum.y,
            maximum.x,
            maximum.y);
        return true;
    }

    private bool TryRefreshTearSwipeGeometry(out float tearSeamScreenY)
    {
        tearSeamScreenY = 0f;
        var camera = Camera.main;
        if (camera == null
            || !TryRefreshTearSwipeScreenRect()
            || !GameAnimationUtility.TryGetPreparedCardPackTearSeamWorldPosition(
                out var seamWorldPosition))
        {
            return false;
        }

        tearSeamScreenY = camera.WorldToScreenPoint(seamWorldPosition).y;
        return true;
    }

    private bool TryScreenPointToTearGuideLocal(Vector2 screenPoint, out Vector2 localPoint)
    {
        localPoint = default;
        var canvasRect = mTearGuideCanvas != null
            ? mTearGuideCanvas.transform as RectTransform
            : null;
        var eventCamera = mTearGuideCanvas != null
            && mTearGuideCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? mTearGuideCanvas.worldCamera
                : null;
        return canvasRect != null
            && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                eventCamera,
                out localPoint);
    }

    private void OnTearSwipeBegin(Vector2 screenPosition)
    {
        if (!mIsAwaitingTearSwipe
            || !TryRefreshTearSwipeGeometry(out var tearSeamScreenY))
        {
            return;
        }

        mIsTrackingTearTap = mTearSwipeScreenRect.Contains(screenPosition);
        if (!mIsTrackingTearTap)
        {
            return;
        }

        mTearSwipeStartScreenPosition = screenPosition;

        var bandHalfHeight = mTearSwipeScreenRect.height * TearGuideBandHeightRatio * 0.5f;
        var bandCenterY = tearSeamScreenY;
        var maximumStartX = Mathf.Lerp(
            mTearSwipeScreenRect.xMin,
            mTearSwipeScreenRect.xMax,
            TearSwipeStartMaxRatio);
        if (screenPosition.x < mTearSwipeScreenRect.xMin
            || screenPosition.x > maximumStartX
            || screenPosition.y < bandCenterY - bandHalfHeight
            || screenPosition.y > bandCenterY + bandHalfHeight)
        {
            return;
        }

        mIsTrackingTearSwipe = true;
        if (mTearGuideRect != null)
        {
            mTearGuideRect.gameObject.SetActive(false);
        }
    }

    private void OnTearSwipeMove(Vector2 screenPosition)
    {
        if (mIsTrackingTearTap)
        {
            var maximumTapTravel = Mathf.Max(
                TearTapMinTravelPixels,
                Mathf.Min(mTearSwipeScreenRect.width, mTearSwipeScreenRect.height)
                    * TearTapMaxTravelRatio);
            if (Vector2.Distance(screenPosition, mTearSwipeStartScreenPosition) > maximumTapTravel)
            {
                mIsTrackingTearTap = false;
            }
        }

        if (!mIsTrackingTearSwipe)
        {
            return;
        }

        var horizontalDistance = screenPosition.x - mTearSwipeStartScreenPosition.x;
        var verticalDistance = Mathf.Abs(screenPosition.y - mTearSwipeStartScreenPosition.y);
        if (horizontalDistance < mTearSwipeScreenRect.width * TearSwipeRequiredDistanceRatio
            || verticalDistance > mTearSwipeScreenRect.height * TearSwipeMaxVerticalDriftRatio)
        {
            return;
        }

        CompleteTearSwipe();
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
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        if (shouldOpenFromTap)
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

        mIsAwaitingTearSwipe = false;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        StopTearGuide();
        mPlayAnimationCoroutine = StartCoroutine(PlaySelectedPackage());
    }

    private void ClearPackageSelection()
    {
        if (mReplayPanelRoot != null)
        {
            SetPanelVisible(mReplayPanelRoot, false);
        }

        mIsReplayConfirmationVisible = false;
        mIsSelectedPackageReplay = false;
        DisableSelectedPackageRenderLayer();
        mSelectedPackageEntry = null;
        mSelectedBagId = 0;
        mSelectedPackageStartScale = 0f;
        mSelectedPackageOpenScale = 0f;
        mSelectedPackageStageScale = 0f;
        mSelectedPackageStartCenter = default;
        mSelectedPackageDisplayCenter = default;
    }

}
