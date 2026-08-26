using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float PackageOpenScaleDuration = 0.3f;
    private const float PackageOpenWidth = 600f;
    private const float PackageOpenHeight = 680f;
    private const float PackageSlotWidth = 240f;
    private const float PackageSlotHeight = 272f;
    private const float PackageCoverWidth = 240f;
    private const float PackageCoverHeight = 272f;
    private const float PackageHorizontalSpacing = 20f;
    private const float PackageVerticalSpacing = 20f;
    private const float DefaultPackagePageWidth = 1625f;
    private const float DefaultPackagePageHeight = 950f;
    private const int PackagesPerPageRowCount = 3;
    private const int PackagesPerPageColumnCount = 6;
    private const int PackagesPerPage = PackagesPerPageRowCount * PackagesPerPageColumnCount;
    private const int PackTornMaskCount = 6;
    private const int InProgressPackPieceCount = 3;
    private const float InProgressPackPieceMaxSize = 86f;
    private const float InProgressPackPieceScaleMultiplier = 1.4f;
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
    private const float OpeningStageTransitionDuration = 0.28f;
    private const float OpeningStageSettleDuration = 0.22f;
    private const float OpeningStageScaleRatio = 0.92f;
    private const float OpeningStagePunchScaleRatio = 1.04f;
    private const float OpeningModelHandoffHoldDuration = 0.06f;
    private const float OpeningModelHandoffFadeDuration = 0.12f;
    private const float TearGestureTravelRatio = 0.06f;
    private const float TearGestureMinTravelPixels = 18f;
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
    private GameObject mMenuPanelRoot;
    private GameObject mSettingsPanelRoot;
    private GameObject mUsablePanelRoot;
    private GameObject mSavePanelRoot;
    private GameObject mBagSelectPanelRoot;
    private Canvas mBagSelectOverlayCanvas;
    private Canvas mSelectedPackageOverlayCanvas;
    private Image mSelectedPackageOverlayImage;
    private RectTransform mSelectedPackageOverlayRect;
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
    private bool mDidWarnPackTornMaskUnavailable;
    private Vector2 mTearSwipeStartScreenPosition;
    private Rect mTearSwipeScreenRect;

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
    }

    private sealed class InProgressPackagePieceAnimation
    {
        public RectTransform RectTransform;
        public Vector2 BasePosition;
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

    private void OnDestroy()
    {
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

        if (mSelectedPackageOverlayImage != null)
        {
            mSelectedPackageOverlayImage.enabled = visible
                && mSelectedPackageOverlayImage.sprite != null;
        }
    }

    private void SetSelectedPackageImageAlpha(float alpha)
    {
        if (mSelectedPackageOverlayImage == null)
        {
            return;
        }

        var color = mSelectedPackageOverlayImage.color;
        color.a = Mathf.Clamp01(alpha);
        mSelectedPackageOverlayImage.color = color;
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
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = canvasObject.layer;
        mSelectedPackageOverlayRect = imageObject.GetComponent<RectTransform>();
        mSelectedPackageOverlayRect.SetParent(canvasObject.transform, false);
        mSelectedPackageOverlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.pivot = new Vector2(0.5f, 0.5f);
        mSelectedPackageOverlayRect.anchoredPosition = Vector2.zero;
        mSelectedPackageOverlayRect.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);

        mSelectedPackageOverlayImage = imageObject.GetComponent<Image>();
        mSelectedPackageOverlayImage.color = Color.white;
        mSelectedPackageOverlayImage.preserveAspect = true;
        mSelectedPackageOverlayImage.raycastTarget = false;
        canvasObject.SetActive(false);
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
            mSelectedPackageOverlayRect.sizeDelta = Vector2.LerpUnclamped(
                fromSize,
                toSize,
                eased);
            yield return null;
        }

        mSelectedPackageOverlayRect.anchoredPosition = toPosition;
        mSelectedPackageOverlayRect.sizeDelta = toSize;
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
                renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry
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

        SetSelectedPackageImageVisible(true);
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
        mPackagePageTemplate.gameObject.SetActive(true);
        NormalizePagedPackageLayout();
        return true;
    }

    private void RefreshPackageList()
    {
        if (mPackageContentRoot == null || mPackageItemTemplate == null)
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
            var entry = CreatePagedPackageSlot(packId, i);
            if (entry.Image == null)
            {
                continue;
            }

            ApplyPackageSlotVisual(entry, packId);
            entry.Root.SetActive(true);
            mPackageSlotsById[packId] = entry;
        }

        RefreshPackagePageLayout();
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
        var packImagePath = GameDefine.FormatPackImagePath(packId);
        var packSprite = GameCommonUtility.LoadSpriteByPath(packImagePath, PixelsPerUnit);
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
        var showTornState = showCompletedState || hasCompletedFirstGroup;
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
                    i));
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
        int index)
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
        var scale = InProgressPackPieceMaxSize / Mathf.Max(spriteSize.x, spriteSize.y, 1f);
        var previousSize = spriteSize * scale;
        var displayedSize = previousSize * InProgressPackPieceScaleMultiplier;
        var basePosition = GetInProgressPackagePiecePosition(index);
        basePosition.y -= (displayedSize.y - previousSize.y) * 0.5f;
        var rotationRadians = rotationDegrees * Mathf.Deg2Rad;
        var rotatedHalfWidth = (
            Mathf.Abs(Mathf.Cos(rotationRadians)) * displayedSize.x
            + Mathf.Abs(Mathf.Sin(rotationRadians)) * displayedSize.y) * 0.5f;
        var minimumCenterX = parent.rect.xMin
            + rotatedHalfWidth
            + InProgressPackPieceHorizontalMargin;
        var maximumCenterX = parent.rect.xMax
            - rotatedHalfWidth
            - InProgressPackPieceHorizontalMargin;
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
        shadow.effectDistance = new Vector2(2f, -3f);
        shadow.useGraphicAlpha = true;

        var normalizedPhase = Mathf.Repeat(
            packId * PackBreathingPhaseStep
                + index / (float)InProgressPackPieceCount,
            1f);
        return new InProgressPackagePieceAnimation
        {
            RectTransform = pieceRect,
            BasePosition = basePosition,
            PhaseRadians = normalizedPhase * Mathf.PI * 2f
        };
    }

    private void UpdateInProgressPackagePieceAnimations()
    {
        if (mPackageSlotsById.Count == 0)
        {
            return;
        }

        var cycleRadians = Time.unscaledTime
            * (Mathf.PI * 2f / InProgressPackPieceFloatDuration);
        foreach (var pair in mPackageSlotsById)
        {
            var animations = pair.Value?.ProgressPieceAnimations;
            if (animations == null)
            {
                continue;
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
                    * InProgressPackPieceFloatDistance;
                animation.RectTransform.anchoredPosition = position;
            }
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
        var coverSprite = entry.Image != null ? entry.Image.sprite : null;
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
        SetPackageVisualsVisible(entry, false);
        yield return CaptureBagSelectBackdrop();
        if (mSelectedPackageOverlayImage == null || mSelectedPackageOverlayRect == null)
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

        mSelectedPackageOverlayImage.sprite = coverSprite;
        mSelectedPackageOverlayRect.anchoredPosition = mSelectedPackageStartPosition;
        mSelectedPackageOverlayRect.sizeDelta = mSelectedPackageStartSize;
        SetSelectedPackageImageVisible(true);
        RefreshBagSelectPackState(bagId);
        SetBagSelectBackdropVisible(true);
        SetBagSelectPanelVisible(true);
        SetBagSelectButtonsInteractable(false);
        yield return AnimateSelectedPackageImage(
            mSelectedPackageStartPosition,
            mSelectedPackageDisplayPosition,
            mSelectedPackageStartSize,
            mSelectedPackageDisplaySize);
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
        SetUnselectedPackageVisualsVisible(false);
        if (mOpeningStageBackgroundRoot != null)
        {
            FitOpeningStageBackgroundToCamera();
            mOpeningStageBackgroundRoot.SetActive(true);
            SetOpeningStageBackgroundAlpha(0f);
        }

        if (mBagSelectPanelCanvasGroup != null)
        {
            mBagSelectPanelCanvasGroup.alpha = 1f;
            mBagSelectPanelCanvasGroup.interactable = false;
            mBagSelectPanelCanvasGroup.blocksRaycasts = false;
        }

        mSelectedPackageStageSize = mSelectedPackageDisplaySize * OpeningStageScaleRatio;
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

            if (mSelectedPackageOverlayRect != null)
            {
                mSelectedPackageOverlayRect.sizeDelta = Vector2.LerpUnclamped(
                    mSelectedPackageDisplaySize,
                    mSelectedPackageStageSize,
                    eased);
            }
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

        var punchSize = mSelectedPackageStageSize * OpeningStagePunchScaleRatio;
        var halfSettleDuration = OpeningStageSettleDuration * 0.5f;
        yield return AnimateSelectedPackageImage(
            mSelectedPackageDisplayPosition,
            mSelectedPackageDisplayPosition,
            mSelectedPackageStageSize,
            punchSize,
            halfSettleDuration);
        yield return AnimateSelectedPackageImage(
            mSelectedPackageDisplayPosition,
            mSelectedPackageDisplayPosition,
            punchSize,
            mSelectedPackageStageSize,
            halfSettleDuration);

        StartCoroutine(GameManager.PreloadGameScene(mSelectedBagId));
        StartOpeningHintAnimation();
        mIsPlayingAnimation = false;
        mIsAwaitingTearSwipe = true;
        mIsTrackingTearSwipe = false;
        mIsTrackingTearTap = false;
        mPlayAnimationCoroutine = null;
    }

    private IEnumerator PlaySelectedPackage()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        var selectedBagId = mSelectedBagId;
        var isReplaySession = mIsSelectedPackageReplay;
        StoreOpeningPackExitPosition();

        var openingEffectStarted = false;
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
                && mCardPackOpeningEffect.Begin(packTexture, mSelectedPackageOverlayRect);
        }

        SetBagSelectPanelVisible(false);
        if (openingEffectStarted)
        {
            // Keep the static cover over the prepared animation frame until the model has
            // reached the render loop. Its opaque hold masks the clip's initial mesh settle.
            yield return new WaitForEndOfFrame();
            mCardPackOpeningEffect.StartPlayback();
            var handoffHoldElapsed = 0f;
            while (handoffHoldElapsed < OpeningModelHandoffHoldDuration)
            {
                handoffHoldElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            var handoffElapsed = 0f;
            while (handoffElapsed < OpeningModelHandoffFadeDuration)
            {
                handoffElapsed += Time.unscaledDeltaTime;
                var normalized = Mathf.Clamp01(
                    handoffElapsed / OpeningModelHandoffFadeDuration);
                SetSelectedPackageImageAlpha(1f - Mathf.SmoothStep(0f, 1f, normalized));
                yield return null;
            }

            SetSelectedPackageImageVisible(false);
            SetSelectedPackageImageAlpha(1f);
            yield return mCardPackOpeningEffect.WaitForCompletion();
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

        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(
            selectedBagId,
            playEntranceAnimation: true,
            isReplaySession: isReplaySession);
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
        var bottomRight = RectTransformUtility.WorldToScreenPoint(camera, corners[3]);
        var bottomCenter = (bottomLeft + bottomRight) * 0.5f;
        GameManager.SetOpeningPackExitPosition(new Vector2(
            bottomCenter.x / Screen.width,
            bottomCenter.y / Screen.height));
    }

    private IEnumerator HidePackageSelection()
    {
        mIsPlayingAnimation = true;
        SetBagSelectButtonsInteractable(false);
        SetBagSelectPanelVisible(false);
        SetBagSelectBackdropVisible(false);
        ReleaseBagSelectBackdropTexture();
        yield return AnimateSelectedPackageImage(
            mSelectedPackageDisplayPosition,
            mSelectedPackageStartPosition,
            mSelectedPackageDisplaySize,
            mSelectedPackageStartSize);

        var selectedEntry = mSelectedPackageEntry;
        SetSelectedPackageImageVisible(false);
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
        if (mSelectedPackageOverlayImage != null)
        {
            mSelectedPackageOverlayImage.sprite = null;
        }

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
    private const float ModelVisibleDuration = 1.6f;
    private const float LightEffectDelay = 0.5f;
    private const float LightEffectVisibleDuration = 1.1f;
    private const uint LightEffectRandomSeed = 1u;
    private const float FallbackAnimationDuration = 1.8333334f;
    private const string ModelPathFormat = "Effects/CardPack/Models/CardPackOpeningModel_{0:D3}";
    private const string AnimatorControllerPath = "Effects/CardPack/Animations/CardPackAnimation";
    private const string AnimationStateName = "Take 001";
    private const string FrontMaterialPath = "Effects/CardFx/Materials/test";
    private const string SceneLightEffectParentName = "PackObject";
    private const string SceneLightEffectObjectName = "fx_chai_w_001";
    private const string CardRendererNamePrefix = "mesh_skin_cardPack_";
    private const int FrontRendererNumberLength = 3;
    private const int BackRendererNumberLength = 5;

    private GameObject mWorldRoot;
    private GameObject mModelObject;
    private GameObject mLightEffectObject;
    private Camera mMainCamera;
    private Animator mAnimator;
    private Material mFrontMaterial;
    private int mOriginalCameraCullingMask;
    private bool mDidOverrideCameraCullingMask;
    private float mAnimationDuration;
    private float mPlaybackStartTime;
    private bool mIsPlaying;
    private bool mIsPrepared;

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

    public bool Begin(Texture packTexture, RectTransform displayedPackRect)
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
        if (modelPrefab == null
            || controller == null
            || frontMaterialTemplate == null)
        {
            Debug.LogError(
                $"CardPackOpeningEffect: required resource is missing. variant={variant}, "
                + $"model={modelPrefab != null}, controller={controller != null}, "
                + $"frontMaterial={frontMaterialTemplate != null}");
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
        mAnimator.Play(AnimationStateName, 0, 0f);
        mAnimator.Update(0f);
        mAnimator.speed = 0f;
        mAnimationDuration = ResolveAnimationDuration(controller);

        if (!TryFitStageToDisplayedPack(stageRoot, displayedPackRect))
        {
            Debug.LogError("CardPackOpeningEffect: failed to calculate the opening model bounds.");
            CleanupPlaybackResources();
            return false;
        }

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

        gameObject.SetActive(true);
        mIsPrepared = true;
        Debug.Log(
            $"CardPackOpeningEffect: prepared variant {variant:D3} with {packTexture.name}. "
            + $"duration={mAnimationDuration:F3}s, lightDelay={LightEffectDelay:F3}s, "
            + "light=scene instance");
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
        if (!mIsPrepared || mAnimator == null)
        {
            return;
        }

        mIsPrepared = false;
        mAnimator.speed = 1f;
        mPlaybackStartTime = Time.time;
        mIsPlaying = true;
    }

    public IEnumerator WaitForCompletion()
    {
        if (!mIsPlaying)
        {
            yield break;
        }

        var elapsed = Mathf.Max(0f, Time.time - mPlaybackStartTime);
        var lightStarted = false;
        var playbackDuration = Mathf.Max(
            Mathf.Min(mAnimationDuration, ModelVisibleDuration),
            LightEffectDelay + LightEffectVisibleDuration);
        while (elapsed < playbackDuration)
        {
            elapsed += Time.deltaTime;
            if (!lightStarted && elapsed >= LightEffectDelay)
            {
                lightStarted = true;
                StartLightEffect();
            }

            yield return null;
        }

        ReleaseSceneLightEffect();
        mIsPlaying = false;
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
        mOriginalCameraCullingMask = mMainCamera.cullingMask;
        mMainCamera.cullingMask |= 1 << EffectLayer;
        mDidOverrideCameraCullingMask = true;
        return true;
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
        var foundBack = false;
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
                foundBack = true;
            }
            else if (numberLength == FrontRendererNumberLength)
            {
                renderer.sharedMaterial = frontMaterial;
                foundFront = true;
            }
        }

        return foundFront && foundBack;
    }

    private static float ResolveAnimationDuration(RuntimeAnimatorController controller)
    {
        var duration = 0f;
        var clips = controller.animationClips;
        for (var i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null)
            {
                duration = Mathf.Max(duration, clips[i].length);
            }
        }

        return duration > 0f ? duration : FallbackAnimationDuration;
    }

    private void StartLightEffect()
    {
        if (mLightEffectObject == null)
        {
            return;
        }

        var rootParticleSystem = mLightEffectObject.GetComponent<ParticleSystem>();
        if (rootParticleSystem == null)
        {
            Debug.LogError("CardPackOpeningEffect: scene light effect root has no ParticleSystem.");
            return;
        }

        ApplyTimelineParticleSeeds(mLightEffectObject);
        mLightEffectObject.SetActive(true);
        rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rootParticleSystem.Play(true);
    }

    private static void ApplyTimelineParticleSeeds(GameObject effectRoot)
    {
        var particleSystems = effectRoot.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (!particleSystem.useAutoRandomSeed)
            {
                continue;
            }

            particleSystem.useAutoRandomSeed = false;
            particleSystem.randomSeed = LightEffectRandomSeed;
        }
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
        mIsPlaying = false;
        mIsPrepared = false;

        if (mMainCamera != null && mDidOverrideCameraCullingMask)
        {
            mMainCamera.cullingMask = mOriginalCameraCullingMask;
        }

        mDidOverrideCameraCullingMask = false;

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

        mModelObject = null;
        mMainCamera = null;
        mAnimator = null;
        mAnimationDuration = 0f;
        mPlaybackStartTime = 0f;
    }

    private void OnDestroy()
    {
        CleanupPlaybackResources();
    }
}
