using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    private static readonly ProfilerMarker SettlementEntryMarker =
        new ProfilerMarker("Puffies.Settlement.Entry");
    private static readonly ProfilerMarker SettlementBoardPreparationMarker =
        new ProfilerMarker("Puffies.Settlement.BoardPreparation");
    private static readonly ProfilerMarker SettlementPersistenceMarker =
        new ProfilerMarker("Puffies.Settlement.Persistence");
    private static readonly ProfilerMarker SettlementTaskDataMarker =
        new ProfilerMarker("Puffies.Settlement.TaskData");
    private static readonly ProfilerMarker SettlementRewardTransitionMarker =
        new ProfilerMarker("Puffies.Settlement.RewardTransition");

    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float DefaultBoardScale = 1f;
    private const float WorldGameplayDepth = -0.5f;
    private const float GamePageCameraPadding = 0.3f;
    private const int WindowResizeLayoutStabilizationFrameCount = 2;
    private const float MaxBoardToTrayGapViewportRatio = 0.1f;
    private const float DraggableLeftPadding = 0.6f;
    private const float DraggableHorizontalSpacingPixels = 40f;
    private const float TrayPieceReflowDuration = 0.5f;
    private const float PieceTrayMaxHeightRatio = 0.9f;
    private const float TrayScrollBoundsEpsilon = 0.001f;
    private const float SnapDistanceMin = 0.2f;
    private const float SnapDistanceMax = 0.8f;
    private const float SnapDistanceSizeRatio = 0.22f;
    private const float LooseClusterAdjacencySizeRatio = 0.035f;
    private const float LooseClusterAdjacencyMin = 0.015f;
    private const float LooseClusterAdjacencyMax = 0.12f;
    private const float LooseClusterAttachDistanceRatio = 0.24f;
    private const float LooseClusterAttachDistanceMin = 0.18f;
    private const float LooseClusterAttachDistanceMax = 0.7f;
    private const float PieceBgSlideDuration = 0.25f;
    private const float TaskProgressRollDuration = 0.8f;
    private const float SettlementBaseRollDuration = 1.2f;
    private const float SettlementBonusRollDuration = 1.08f;
    private const float SettlementStagePauseDuration = 0.16f;
    private const float SettlementFinalPauseDuration = 0.45f;
    private const float SettlementHeaderDropDuration = 0.52f;
    private const float SettlementHeaderDropOvershoot = 14f;
    private const float SettlementHeaderDropTravelRatio = 0.78f;
    private const float SettlementOffscreenMargin = 60f;
    private const float SettlementBagCountIncrementDuration = 0.68f;
    private const float SettlementBagCountIncrementRise = 64f;
    private const float SettlementRewardPanelSlideDuration = 0.42f;
    private const float SettlementRewardPopDuration = 0.36f;
    private const float SettlementTaskRewardFlyDuration = 0.52f;
    private const float SettlementTaskRewardDecorationFadeDuration = 0.2f;
    private const float SettlementRewardAnimationLead = 0.26f;
    private const float SettlementRewardAnimationStagger = 0.14f;
    private const float SettlementRewardAnimationTimeoutPadding = 1f;
    private const float SettlementRewardSlotOffset = 82f;
    private const float SettlementBoardViewportFill = 0.9f;
    private const float SettlementBoardFitDuration = 0.46f;
    private const float PieceBgSlideOutPadding = 0.15f;
    private const int PieceBgFillSortingOrder = 499;
    private const int PieceBgSortingOrder = 500;
    private const float PieceBgAlpha = 1f;
    private const float PieceBgFillAlpha = 0.3f;
    private const float GameTransitionDurationScale = 1.5f;
    private const float GameEntranceBoardDelay = 0f;
    private const float GameEntranceBoardDuration = 0.38f
                                                    * GameTransitionDurationScale
                                                    * GameDefine.NonDealTransitionDurationMultiplier;
    private const float GameEntranceTrayDelay = 0f;
    private const float GameEntranceTrayDuration = 0.22f
                                                   * GameTransitionDurationScale
                                                   * GameDefine.NonDealTransitionDurationMultiplier;
    private const float GameEntrancePieceSettleDuration = 0.46f * GameTransitionDurationScale;
    private const float GameEntrancePieceStagger = 0.018f * GameTransitionDurationScale;
    private const float TornPackPieceSettleReduction = 0.3f;
    private const float TornPackPieceStagger = GameEntrancePieceStagger;
    private const float GameEntranceControlDelay = 0f;
    private const float GameEntranceControlDuration = 0.22f
                                                      * GameTransitionDurationScale
                                                      * GameDefine.NonDealTransitionDurationMultiplier;
    private const int GameEntranceWarmupFrameCount = 2;
    private const float GameEntranceMaxFrameDelta = 1f / 30f;
    private const float GroupTransitionBoardDuration = 0.72f;
    private const float GroupTransitionPieceDuration = 0.38f;
    private const float GroupTransitionPieceStagger = 0.045f;
    private const float GroupTransitionPromptDelay = 0.22f;
    private const float GroupTransitionStrongHoldDuration = 0.3f;
    private const float GroupTransitionDefaultHoldDuration = 0.1f;
    private const float ActiveGroupOutlineFadeDuration = 0.5f;
    private const float PieceSnapDuration = 0.12f;
    private const float PiecePlacementShineDuration = 0.52f;
    private const float PiecePlacementShineBandWidth = 0.045f;
    private const float PiecePlacementLightPushPhaseRatio = 0.22f;
    private const float PiecePlacementLightMinBendPixels = 6f;
    private const float PiecePlacementLightMaxBendPixels = 14f;
    private const float PiecePlacementLightDistanceMultiplier = 3f;
    private const float PiecePlacementLightDurationMultiplier = 2f;
    private const float PiecePlacementLightMiddleStretch = 0.34f;
    private const float PieceLightMaximumWidthRatio = 0.7f;
    private const int PieceLightInteriorGridResolution = 32;
    private const float PieceLightPreferredClearanceRatio = 0.72f;
    private const int PiecePlacementLightSpriteCount = 4;
    private const int PiecePlacementLightMaxAffectedPieces = 7;
    private const float InvalidDropReturnDuration = 0.3f;
    private const float InvalidDropColorRestoreDuration = 0.1f;
    private const float InvalidDropTintStrength = 0.7f;
    private const int PieceSortingOrder = 520;
    private const float HintShakeAngle = 6f;
    private const float HintShakeCyclesPerSecond = 4.5f;
    private const float HintShakeDuration = 0.8f;
    private const float LoosePieceReminderInterval = 5f;
    private const float LoosePieceReminderShakeDuration = 0.55f;
    private const float LoosePieceReminderShakeAngle = 5f;
    private const float LoosePieceReminderShakeCyclesPerSecond = 5f;
    private const float HintDashLength = 20f;
    private const float HintDashGap = 15f;
    private const float HintOutlineWidth = 3f;
    private const float HintOutlineScrollSpeed = 60f;
    private const int TutorialCanvasSortingOrder = 30000;
    private const float TutorialArrowScale = 0.7f;
    private const float TutorialTargetPromptGap = 24f;
    private const float TutorialPromptScreenMargin = 24f;
    private const float TutorialHintPromptLeftOffset = 48f;
    private const float TutorialHintPromptDownOffset = 20f;
    private const float TutorialHintPromptButtonGap = 32f;
    private const float TutorialHintArrowButtonGap = 16f;
    private const float TutorialHintArrowTargetDownOffset = 20f;
    private const string TutorialInvalidLineStartCharacters = "，。！？；：、”’）】》";
    private const string TutorialCollection = "Tutorial";
    private const string PiecePlacementTutorialKey = "CardBag001TutorialCompleted";
    private const string TutorialCanvasObjectName = "PiecePlacementTutorialCanvas";
    private const string TutorialPieceObjectName = "TutorialPiece";
    private const string TutorialArrowObjectName = "TutorialArrow";
    private const string TutorialTextObjectName = "TutorialText";
    private const string TutorialTipTemplateObjectName = "GuideTip";
    private const string TutorialTipTextObjectName = "TextTips";
    private const string TutorialHintArrowObjectName = "Arrow";
    private const string TutorialArrowPath = GameDefine.UiRoot + "/GameScene/GuideArrow1.png";
    private const string TutorialTipBackgroundPath = GameDefine.UiRoot + "/GameScene/GuideTipBg.png";
    private const string TutorialStrongInstruction = "从托盘中选出匹配的贴纸，贴在板子的正确位置上。";
    private const string TutorialPracticeInstruction = "将两个贴纸贴在板子的合适位置上，完成关卡。";
    private const string TutorialHintInstruction = "攻克这一关！如果遇到困难，请使用“提示”按钮。";
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string PieceBgFillObjectName = "PieceBgFill";
    private const string PieceBgObjectName = "PieceBg";
    private const string PieceBgPath = GameDefine.UiRoot + "/BasicUI/ImgMaskBlack.png";
    private const string StandardCardBoardBackgroundPath =
        GameDefine.UiRoot + "/BasicUI/BgCardBoard1.png";
    private const string HighContrastCardBoardBackgroundPath =
        GameDefine.UiRoot + "/BasicUI/BgCardBoard2.png";
    private const string PuzzleOutlineTintShaderResourcesPath = "PuzzleOutlineTint";
    private const string PuzzleOutlineTintMaterialName = "PuzzleOutlineTint (Runtime)";
    private const string PiecePlacementShineShaderResourcesPath = "PuzzlePlacementShine";
    private const string PiecePlacementShineMaterialName = "PuzzlePlacementShine (Runtime)";
    private const string PiecePlacementLightPathPrefix =
        GameDefine.UiRoot + "/GameScene/PieceLight";
    private const string PiecePlacementLightMaterialResourcesPath = "PackHighlightAdditive";
    private const string PiecePlacementSpriteLightShaderResourcesPath =
        "PuzzlePieceLightAdditive";
    private const string BoardShadowMaterialResourcesPath = "IngameCoverShadow01";
    private const string LoosePieceShadowMaterialResourcesPath = "IngameCoverShadow02";
    private const string PlacedPieceShadowMaterialResourcesPath = "IngameCoverShadow03";
    private const string DefaultPieceShadowMaterialResourcesPath = "IngameCoverShadow04";
    private const string SpriteRendererShadowKeyword = "PACK_SHADOW_SPRITE_RENDERER";
    private const string ShadowOnlyKeyword = "PACK_SHADOW_ONLY";
    private const string LooseClusterShadowObjectName = "LoosePieceClusterShadow";
    private const int LooseClusterShadowMaxTextureSize = 2048;
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string BoardOccupancyProbeRootObjectName = "BoardOccupancyProbes";
    private const string GameBoardOpaqueProbeObjectName = "GameBoardOpaqueProbe";
    private const string ActiveGroupOutlineRootObjectName = "ActiveGroupOutline";
    private const string LevelOutlineLayerObjectName = "LevelOutline";
    private const string StickerOutlineLayerObjectName = "StickerOutlines";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private const string TaskItemObjectName = "TaskItem";
    private const string TaskBagCountTitlePath = "TaskBg2/TaskTitle2";
    private const string TaskBonusTitlePath = "TaskBg2/TaskTitle21";
    private const string TaskBonusScorePath = "TaskBg2/TaskTitle22";
    private const string TaskScorePath = "TaskBg2/TaskScore";
    private const string TaskBagCountPath = "TaskBg2/TaskBagNum";
    private const string TaskSummaryObjectName = "TaskBg2";
    private const string TaskRewardBagRootObjectName = "ImgBagBg";
    private const string TaskRewardItemCanvasPath = "ImgBagBg/BagRewardItem/Canvas";
    private const string TaskRewardImgBagPath =
        "ImgBagBg/BagRewardItem/Canvas/BagCover";
    private const string TaskRewardRevealEffectPath =
        "ImgBagBg/BagRewardItem/Canvas/FX_ui_jieSuo_w";
    private const string SecondaryRewardItemCanvasPath =
        "ImgBagBg/BagRewardItemSecondary/Canvas";
    private const string SecondaryRewardImgBagPath =
        "ImgBagBg/BagRewardItemSecondary/Canvas/BagCover";
    private const string SecondaryRewardRevealEffectPath =
        "ImgBagBg/BagRewardItemSecondary/Canvas/FX_ui_jieSuo_w";
    private const string TaskRewardSourceRootPath = "BagBg";
    private const string TaskRewardSourceIconPath = "BagBg/BagIcon";
    private const string TaskRewardSourceCountBackgroundPath = "BagBg/BagAddBg";
    private const string TaskRewardSourceCountPath = "BagBg/TextAddNum";
    private const string SettlementCameraButtonObjectName = "BtnCamera";
    private const string PackPhotoItemObjectName = "PackPhotoItem";
    private const string HintButtonObjectName = "BtnTips";
    private const string PieceHintOutlineObjectName = "PieceHintOutline";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const string TestCompleteButtonObjectName = "BtnCompleteAllTest";
    private const string TestCompleteButtonText = "一键完成";
#endif
    private static readonly Color PieceHintOutlineColor = new Color32(112, 151, 75, 255);
    private static readonly Color HighContrastPieceHintOutlineColor = new Color32(0xb1, 0xd7, 0x02, 0xff);
    private static readonly Color TutorialTargetOutlineColor = new Color32(80, 139, 230, 255);
    private static readonly Color InvalidDropTintColor = new Color32(255, 58, 58, 255);
    private static readonly Color StandardPuzzleOutlineColor = new Color32(0x3f, 0x42, 0x3e, 0xff);
    private static readonly Color PiecePlacementShineColor = new Color32(112, 151, 75, 230);
    private static readonly int ShineSweepAxisId = Shader.PropertyToID("_SweepAxis");
    private static readonly int ShineSweepCenterId = Shader.PropertyToID("_SweepCenter");
    private static readonly int ShineBandWidthId = Shader.PropertyToID("_BandWidth");
    private static readonly int ShineColorId = Shader.PropertyToID("_ShineColor");
    private static readonly int SpritePixelsPerUnitId = Shader.PropertyToID("_SpritePixelsPerUnit");
    private static readonly int ShadowColorId = Shader.PropertyToID("_ShadowColor");
    private const int OpeningPieceRenderQueue = 2000;
    private static readonly Vector2 TutorialHintPromptAnchor = new Vector2(0.73f, 0.76f);

    private enum TutorialStage
    {
        None,
        StrongPlacement,
        TwoPiecePractice,
        HintIntroduction
    }

    private enum PieceShadowStyle
    {
        Initial,
        Loose,
        Placed
    }

    private enum SettlementPackRewardSource
    {
        Completion,
        Task
    }

    private sealed class PiecePlacementLightFx
    {
        public AmbientPieceLightFx Light;
        public int AnimationVersion;
        public float Delay;
        public float Lifetime;
        public Vector2 StartBendOffset;
        public float StartMiddleStretch;
        public Vector2 BendDirection;
        public float BendDistance;
    }

    private sealed class AmbientPieceLightFx
    {
        public int PieceNumber;
        public GameObject Root;
        public Image Image;
        public SpriteRenderer Renderer;
        public SpriteRenderer SourceRenderer;
        public SpriteMask SpriteMask;
        public Transform Transform;
        public PieceLightDeformEffect Deformer;
        public int AnimationVersion;
    }

    private sealed class PieceLightState
    {
        public int SpriteIndex;
        public Vector2 NormalizedPosition;
        public float Rotation;
        public Vector2 Scale;
    }

    private sealed class LoosePieceCluster
    {
        public long CreatedOrder;
        public readonly List<DraggablePieceState> Members =
            new List<DraggablePieceState>();
        public GameObject ShadowRoot;
        public SpriteRenderer ShadowRenderer;
        public Texture2D ShadowTexture;
        public Sprite ShadowSprite;
        public PieceShadowStyle ShadowStyle = PieceShadowStyle.Loose;
    }

    private sealed class LooseClusterShadowSource
    {
        public SpriteRenderer Renderer;
        public Color32[] Pixels;
        public int Width;
        public int Height;
        public Bounds LocalBounds;
        public Bounds WorldBounds;
    }

    private struct PiecePlacementLightTarget
    {
        public Image Image;
        public Rect ScreenRect;
        public float Distance;
    }

    private static bool sHookedSceneLoaded;
    private readonly BoardState _board = new BoardState();
    private readonly DragState _drag = new DragState();
    private readonly List<int> _settlementPackRewardIds = new List<int>();
    private readonly HashSet<int> _placedPieceNumbers = new HashSet<int>();
    private readonly List<LoosePieceCluster> _loosePieceClusters =
        new List<LoosePieceCluster>();
    private readonly Dictionary<DraggablePieceState, LoosePieceCluster> _looseClusterByPiece =
        new Dictionary<DraggablePieceState, LoosePieceCluster>();
    private readonly Dictionary<DraggablePieceState, long> _loosePieceOrders =
        new Dictionary<DraggablePieceState, long>();
    private readonly List<DraggablePieceState> _activeDragMembers =
        new List<DraggablePieceState>();
    private readonly List<Vector3> _activeDragStartPositions = new List<Vector3>();
    private long _nextLoosePieceOrder;
    private readonly Dictionary<Image, Collider2D> _boardOccupancyProbes =
        new Dictionary<Image, Collider2D>();
    private Vector3 _pieceBgOriginalPosition;
    private bool _hasPieceBgOriginalPosition;
    private bool _isPieceBgHidden;
    private Vector2 _pieceBoardOriginalAnchoredPosition;
    private bool _hasPieceBoardOriginalAnchoredPosition;
    private bool _isPieceBoardHidden;
    private Rect _pieceTrayDropNormalizedViewportRect;
    private bool _hasPieceTrayDropNormalizedViewportRect;
    private Transform _boardOccupancyProbeRoot;
    private Collider2D _gameBoardOpaqueProbe;
    private Coroutine _pieceTraySlideCoroutine;
    private Coroutine _trayPieceReflowCoroutine;
    private bool _isTrayPieceReflowAnimating;
    private readonly List<DraggablePieceState> _trayScrollStates =
        new List<DraggablePieceState>();
    private readonly List<Vector3> _trayScrollStartPositions = new List<Vector3>();
    private readonly List<DraggablePieceState> _trayPickupRestoreStates =
        new List<DraggablePieceState>();
    private readonly List<Vector3> _trayPickupRestorePositions = new List<Vector3>();
    private DraggablePieceState _trayPickupRestorePiece;
    private bool _isTrayScrolling;
    private float _trayScrollStartWorldX;
    private float _trayScrollMinDeltaX;
    private float _trayScrollMaxDeltaX;
    private int _piecePlacementAnimationCount;
    private int _piecePlacementDragBlockCount;
    private Coroutine _loosePieceReminderShakeCoroutine;
    private readonly List<DraggablePieceState> _loosePieceReminderStates =
        new List<DraggablePieceState>();
    private readonly List<Quaternion> _loosePieceReminderBaseRotations =
        new List<Quaternion>();
    private float _nextLoosePieceReminderTime = -1f;
    private Vector2 _originalGameBoardAnchoredPosition;
    private bool _hasOriginalGameBoardAnchoredPosition;
    private bool _isGameFinished;
    private bool _isTaskTrackingActive;
    private bool _wasHintUsed;
    private bool _isLevelOutlineEnabled;
    private bool _isStickerOutlineEnabled;
    private bool _isHighContrastEnabled;
    private bool _hasGameplayTimerStarted;
    private float _gameplayStartRealtime;
    private float _completionTimeSeconds;
    private bool _wasSelectedPackCompletedOnEntry;
    private bool _didAdvanceTaskDuringSettlement;
    private bool _didFailTaskAdvanceDuringSettlement;
    private bool _didSavePackCompletion;
    private GameObject _rewardPanelRoot;
    private CanvasGroup _rewardPanelCanvasGroup;
    private Transform _rewardTaskItem;
    private RectTransform _settlementSummaryRect;
    private RectTransform _rewardTaskItemRect;
    private RectTransform _settlementRewardBagRect;
    private RectTransform _settlementFinishButtonRect;
    private RectTransform _settlementCameraButtonRect;
    private TMP_Text _settlementBagCountTitleText;
    private TMP_Text _settlementBonusTitleText;
    private TMP_Text _settlementBonusScoreText;
    private TMP_Text _settlementScoreText;
    private TMP_Text _settlementBagCountText;
    private Image _taskRewardImage;
    private Image _secondaryRewardImage;
    private Image _taskRewardSourceCircleImage;
    private Image _taskRewardSourceImage;
    private Image _taskRewardSourceCountBackgroundImage;
    private TMP_Text _taskRewardSourceCountText;
    private GameObject _settlementRewardRevealEffect;
    private GameObject _secondarySettlementRewardRevealEffect;
    private Image _completionRewardDisplayImage;
    private Image _taskRewardDisplayImage;
    private Vector2 _settlementSummaryTargetPosition;
    private Vector2 _rewardTaskItemTargetPosition;
    private Vector2 _settlementRewardBagTargetPosition;
    private Vector2 _taskRewardImageTargetPosition;
    private Vector2 _settlementFinishButtonTargetPosition;
    private Vector2 _settlementCameraButtonTargetPosition;
    private Sprite _taskRewardDefaultSprite;
    private Color _taskRewardDefaultColor = Color.white;
    private Color _taskRewardSourceCircleDefaultColor = Color.white;
    private Color _taskRewardSourceDefaultColor = Color.white;
    private Color _taskRewardSourceCountBackgroundDefaultColor = Color.white;
    private Color _taskRewardSourceCountDefaultColor = Color.white;
    private bool _hasTaskRewardSourceDefaultColors;
    private bool _hasSettlementLayoutTargets;
    private int _settlementBagCountBeforeCompletion;
    private int _settlementBagCountAfterCompletion;
    private int _settlementCompletionRewardPackId;
    private int _settlementTaskRewardPackId;
    private bool _didQueueTaskRewardDuringSettlement;
    private bool _isFirstCompletionSettlement;
    private bool _hasSettlementBoardFitTarget;
    private Vector2 _settlementBoardFitStartPosition;
    private Vector2 _settlementBoardFitTargetPosition;
    private Vector3 _settlementBoardFitStartScale;
    private Vector3 _settlementBoardFitTargetScale;
    private Button _finishButton;
    private Button _settlementCameraButton;
    private CardPackPhoto _cardPackPhoto;
    private bool _isSettlementReadyForFinish;
    private bool _isFinishTransitionStarted;
    private bool _isEntranceAnimating;
    private bool _isGroupTransitionAnimating;
    private bool _isPiecePlacementAnimating;
    private GameObject _loadedCardBagRoot;
    private RectTransform _loadedCardBagRect;
    private Sprite _runtimeCardBoardBackgroundSprite;
    private Material _runtimePuzzleOutlineTintMaterial;
    private Material _runtimePiecePlacementShineMaterial;
    private readonly List<Material> _activePiecePlacementShineMaterials = new List<Material>();
    private readonly List<Sprite> _piecePlacementLightSprites = new List<Sprite>();
    private readonly List<AmbientPieceLightFx> _ambientPieceLights =
        new List<AmbientPieceLightFx>();
    private readonly Dictionary<int, PieceLightState> _pieceLightStates =
        new Dictionary<int, PieceLightState>();
    private Material _piecePlacementLightMaterial;
    private Material _pieceSpriteLightMaterial;
    private Material _boardShadowMaterial;
    private Material _defaultPieceShadowMaterial;
    private Material _loosePieceShadowMaterial;
    private Material _placedPieceShadowMaterial;
    private Material _runtimeDefaultPieceShadowMaterial;
    private Material _runtimeLoosePieceShadowMaterial;
    private Material _runtimePlacedPieceShadowMaterial;
    private Material _runtimeClusterPieceMaterial;
    private Material _runtimeInitialClusterShadowMaterial;
    private Material _runtimeLooseClusterShadowMaterial;
    private readonly Dictionary<Sprite, Sprite> _fullRectPieceShadowSprites =
        new Dictionary<Sprite, Sprite>();
    private readonly HashSet<Sprite> _runtimeFullRectPieceShadowSprites = new HashSet<Sprite>();
    private MaterialPropertyBlock _pieceShadowPropertyBlock;
    private Coroutine _activeGroupOutlineFadeCoroutine;
    private bool _didWarnMissingPuzzleOutlineTintShader;
    private bool _didWarnMissingPiecePlacementShineShader;
    private bool _didWarnMissingPiecePlacementLightResources;
    private float _configuredBoardScale = DefaultBoardScale;
    private Vector3 _originalCardBagLocalScale = Vector3.one;
    private bool _hasOriginalCardBagLocalScale;
    private Vector2 _originalCardBagAnchoredPosition;
    private bool _hasOriginalCardBagAnchoredPosition;
    private DraggablePieceState _hintedPiece;
    private readonly List<DraggablePieceState> _hintedPieces =
        new List<DraggablePieceState>();
    private readonly List<Quaternion> _hintedPieceBaseRotations = new List<Quaternion>();
    private readonly List<Vector3> _hintedPieceBasePositions = new List<Vector3>();
    private LoosePieceCluster _hintedCluster;
    private Vector3 _hintedClusterCenter;
    private Vector3 _hintedClusterShadowBasePosition;
    private Quaternion _hintedClusterShadowBaseRotation = Quaternion.identity;
    private float _hintShakeStartTime;
    private bool _isHintPieceShaking;
    private GameObject _pieceHintOutlineRoot;
    private bool _shouldCompleteRestoredPuzzle;
    private Button _hintButton;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private Button _testCompleteButton;
#endif
    private bool _isTutorialPending;
    private TutorialStage _tutorialStage;
    private DraggablePieceState _tutorialPiece;
    private GameObject _tutorialCanvasRoot;
    private GameObject _tutorialFocusRoot;
    private Sprite _tutorialArrowSprite;
    private Sprite _tutorialTipBackgroundSprite;
    private int _appliedScreenWidth;
    private int _appliedScreenHeight;
    private bool _isWindowResizeLayoutPending;
    private int _windowResizeLayoutReadyFrame;

    private bool IsTutorialActive => _tutorialStage != TutorialStage.None;

    private bool IsTutorialFocusStage =>
        _tutorialStage == TutorialStage.StrongPlacement
        || _tutorialStage == TutorialStage.TwoPiecePractice;

    private bool IsTutorialBlockingOutline =>
        _tutorialStage == TutorialStage.StrongPlacement;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<GameScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneGame,
            BootstrapObjectName);
    }

    private void Start()
    {
        var bootstrapStartedAt = Time.realtimeSinceStartup;
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneGame))
        {
            Destroy(gameObject);
            return;
        }

        CardPackRewardFlyTransition.CancelPending();

        var camera = Camera.main;
        if (camera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);
        }

        ConfigureGameplayCanvas(camera);
        _appliedScreenWidth = Screen.width;
        _appliedScreenHeight = Screen.height;
        InitializeScoringSession();
        var selectedBagId = GameManager.GetBagId();
        AudioManager.Instance.PlayMusic(
            GameManager.GetPreparedGameplayMusicFileName(selectedBagId));
        var playEntranceAnimation = GameManager.ConsumeGameEntranceAnimation();
        var entrancePiecesAlreadyFanned =
            GameManager.ConsumeGameEntrancePiecesAlreadyFanned();
        var isReplaySession = GameManager.ConsumeGameReplaySession();
        Debug.Log(
            $"GameScene: entrance requested. play={playEntranceAnimation}, "
            + $"piecesAlreadyFanned={entrancePiecesAlreadyFanned}, "
            + $"transitionPending={CardPackGameEntranceTransition.IsPending}, "
            + $"replay={isReplaySession}");
        CardPackDataUtility.Initialize();
        _wasSelectedPackCompletedOnEntry = CardPackDataUtility.IsPackCompleted(selectedBagId);
        _settlementBagCountBeforeCompletion = CardPackDataUtility.GetCompletedPackCount();
        _settlementBagCountAfterCompletion = _settlementBagCountBeforeCompletion;
        _isTutorialPending = ShouldOfferPiecePlacementTutorial(selectedBagId, isReplaySession);
        _didAdvanceTaskDuringSettlement = false;
        _didFailTaskAdvanceDuringSettlement = false;
        _didSavePackCompletion = false;
        _isSettlementReadyForFinish = false;
        _isFinishTransitionStarted = false;
        _settlementPackRewardIds.Clear();
        _settlementCompletionRewardPackId = 0;
        _settlementTaskRewardPackId = 0;
        _didQueueTaskRewardDuringSettlement = false;
        _isFirstCompletionSettlement = false;
        InitializeGameplay(selectedBagId);
        AnalyticsManager.Instance.StartCardBag(
            selectedBagId,
            _wasSelectedPackCompletedOnEntry);
        GameManager.NotifyGameSceneLoaded();
        InitializeTaskTracking();
        ConfigureReturnButton();
        ConfigureHintButton();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ConfigureTestCompleteButton();
#endif
        ConfigureRewardPanel();
        if (_shouldCompleteRestoredPuzzle)
        {
            ShowRewardPanel();
        }

        CardPackGameEntranceTransition.NotifyGameSceneReady(camera);

        if (playEntranceAnimation && !_isGameFinished)
        {
            StartCoroutine(PlayGameEntranceAfterPackTransition(
                entrancePiecesAlreadyFanned));
        }
        else
        {
            TryStartPiecePlacementTutorial();
            FadeInActiveGroupOutline();
            StartCoroutine(RefreshCurrentGroupTrayScalesNextFrame());
        }

        Debug.Log(
            $"GameScene bootstrap completed in "
            + $"{(Time.realtimeSinceStartup - bootstrapStartedAt) * 1000f:F1}ms.");
    }

    private IEnumerator PlayGameEntranceAfterPackTransition(bool piecesAlreadyFanned)
    {
        Debug.Log(
            $"GameScene: entrance coroutine started. "
            + $"piecesAlreadyFanned={piecesAlreadyFanned}, "
            + $"transitionPending={CardPackGameEntranceTransition.IsPending}");
        yield return PlayGameEntranceAnimation(
            piecesAlreadyFanned,
            waitForPackTransition: CardPackGameEntranceTransition.IsPending);
        Debug.Log("GameScene: entrance coroutine completed.");
    }

    private void OnDestroy()
    {
        EndTrayScroll();
        StopLoosePieceReminderShake();
        StopTrayPieceReflow();
        GameCursorUtility.SetDefault();
        StopPiecePlacementTutorial(restoreLevelOutline: false);
        DestroyTutorialArrowSprite();
        DestroyTutorialTipBackgroundSprite();
        DestroyRuntimeCardBoardBackgroundSprite();
        DestroyRuntimePuzzleOutlineTintMaterial();
        DestroyRuntimePiecePlacementShineMaterial();
        ClearPieceHint();
        ClearLoosePieceClusters();
        DestroyRuntimePieceShadowResources();
        ClearAmbientPieceLights();
        DestroyPiecePlacementLightSprites();
        DestroyBoardOccupancyProbes();
        HintDashedOutlineGraphic.ClearPathCache();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            CancelActivePointerInteraction();
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            CancelActivePointerInteraction();
        }
    }

    private void Update()
    {
        RefreshForWindowSizeChange();
        EnsureDraggablePieceLights();
        UpdatePieceLightSorting();
        UpdatePieceHintAnimation();
        UpdateLoosePieceReminder();
        if (_isEntranceAnimating || _isGroupTransitionAnimating)
        {
            GameCursorUtility.SetDefault();
            return;
        }

        if (_isGameFinished)
        {
            GameCursorUtility.SetDefault();
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            TryBeginDrag,
            UpdateDragging,
            OnPointerEnd);
        UpdateLooseClusterShadows();
        RefreshCursorForPointer(Input.mousePosition);
    }

    private void RefreshForWindowSizeChange()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        if (Screen.width != _appliedScreenWidth || Screen.height != _appliedScreenHeight)
        {
            _appliedScreenWidth = Screen.width;
            _appliedScreenHeight = Screen.height;
            _isWindowResizeLayoutPending = true;
            _windowResizeLayoutReadyFrame = Time.frameCount
                                            + WindowResizeLayoutStabilizationFrameCount;
            RefreshGameplayViewport(Camera.main);
            GameCommonUtility.RefreshCanvasLayoutsForScreenSize();
        }

        if (!_isWindowResizeLayoutPending
            || Time.frameCount < _windowResizeLayoutReadyFrame
            || _isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating
            || _isPiecePlacementAnimating
            || _drag.DraggingPiece != null
            || _isTrayScrolling
            || _isTrayPieceReflowAnimating
            || _drag.CurrentGroupIndex < 0)
        {
            return;
        }

        RefreshGameplayViewport(Camera.main);
        GameCommonUtility.RefreshCanvasLayoutsForScreenSize();
        if (!IsGameplayCanvasLayoutReady())
        {
            _windowResizeLayoutReadyFrame = Time.frameCount + 1;
            return;
        }

        FitCameraToActiveGroup(_drag.CurrentGroupIndex);
        RefreshCurrentGroupTrayScalesAndLayout();
        GameCommonUtility.RefreshCanvasLayoutsForScreenSize();
        CachePieceTrayDropScreenRect();
        RefreshTutorialAfterWindowSizeChange();
        _isWindowResizeLayoutPending = false;
    }

    private void RefreshTutorialAfterWindowSizeChange()
    {
        if (!IsTutorialActive)
        {
            return;
        }

        if (!ShowPiecePlacementTutorialPresentation())
        {
            Debug.LogWarning(
                "GameScene: tutorial presentation could not be refreshed after window resize; tutorial stopped.");
            StopPiecePlacementTutorial(restoreLevelOutline: true);
        }
    }

    private void InitializeGameplay(int bagId)
    {
        GameManager.SetBagId(bagId);
        LoadConfiguredBoardScale(bagId);
        EnsureCardBagLoaded(bagId);
        EnsureBoardAndGroovesInitialized();
        if (_board.GameBoardImage == null)
        {
            Debug.LogWarning(
                $"GameBoard not found. Expected {GameDefine.FormatCardBagPrefabResourcesPath(bagId)} " +
                $"to contain an object named {GameDefine.GameBoardObjectName}.");
            return;
        }

        if (_board.GrooveImagesByGroup == null || _board.GrooveImagesByGroup.Count == 0)
        {
            Debug.LogWarning("GameScene: no editor groove images found. Expected objects named Piece01, Piece02, ...");
            return;
        }

        EnsureBackgroundCentered();
        EnsurePieceBoardInitialized();
        if (_board.PieceBoardRect == null)
        {
            _board.PieceBgRenderer = CreatePieceBackground();
        }

        var activeGroupIndex = RestorePuzzleSession(bagId);
        _isTutorialPending = _isTutorialPending && activeGroupIndex >= 0;
        if (activeGroupIndex >= 0)
        {
            CreateDraggableGroup(activeGroupIndex);
        }

        Debug.Log(
            $"GameScene ready. BagId={bagId}, Groups={_board.GrooveImagesByGroup.Count}, " +
            $"Pieces={CountGrooveImages(_board.GrooveImagesByGroup)}, " +
            $"RestoredPieces={_placedPieceNumbers.Count}, BoardScale={_configuredBoardScale:0.###}");
    }

    private int RestorePuzzleSession(int bagId)
    {
        _placedPieceNumbers.Clear();
        _shouldCompleteRestoredPuzzle = false;
        if (!CardPackDataUtility.TryEnsurePuzzleSession(bagId))
        {
            Debug.LogWarning($"GameScene: failed to create puzzle session. packId={bagId}");
        }
        else if (CardPackDataUtility.TryGetPlacedPieceNumbers(bagId, out var savedPieceNumbers))
        {
            _placedPieceNumbers.UnionWith(savedPieceNumbers);
        }

        MarkCurrentPackInProgress();
        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (!IsGrooveGroupPersistedAsComplete(group))
            {
                return groupIndex;
            }
        }

        RevealAllGroovesOnBoard();
        _shouldCompleteRestoredPuzzle = true;
        return -1;
    }

    private bool IsGrooveGroupPersistedAsComplete(List<Image> group)
    {
        if (group == null || group.Count == 0)
        {
            return false;
        }

        for (var i = 0; i < group.Count; i++)
        {
            if (!IsGroovePersistedAsPlaced(group[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsGroovePersistedAsPlaced(Image grooveImage)
    {
        var pieceNumber = GetPieceNumberFromImage(grooveImage);
        return pieceNumber != int.MaxValue && _placedPieceNumbers.Contains(pieceNumber);
    }

    private void LoadConfiguredBoardScale(int bagId)
    {
        _configuredBoardScale = DefaultBoardScale;
        if (!GameConfigRepository.TryGetCardPackConfig(bagId, out var config))
        {
            Debug.LogWarning(
                $"GameScene: card pack config not found; BoardScale defaults to 1. bagId={bagId}");
            return;
        }

        _configuredBoardScale = config.BoardScale;
    }

    private IEnumerator PlayGameEntranceAnimation(
        bool piecesAlreadyFanned,
        bool waitForPackTransition)
    {
        _isEntranceAnimating = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = false;
        }
#endif
        Canvas.ForceUpdateCanvases();

        var boardRect = _loadedCardBagRect;
        var boardTarget = boardRect != null
            ? boardRect.anchoredPosition
            : _hasOriginalCardBagAnchoredPosition
                ? _originalCardBagAnchoredPosition
                : Vector2.zero;
        var boardStart = boardTarget + Vector2.right * GameDefine.DesignWidth;

        var trayRect = _board.PieceBoardRect;
        var trayTarget = _hasPieceBoardOriginalAnchoredPosition
            ? _pieceBoardOriginalAnchoredPosition
            : trayRect != null ? trayRect.anchoredPosition : Vector2.zero;
        var trayOffset = trayRect != null
            ? trayRect.rect.height + 120f
            : 420f;
        var trayStart = trayTarget - Vector2.up * trayOffset;

        var returnCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(GameDefine.ReturnButtonObjectName));
        var hintCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(HintButtonObjectName));
        SetCanvasGroupAlpha(returnCanvasGroup, 0f);
        SetCanvasGroupAlpha(hintCanvasGroup, 0f);

        var isTornPackTransition = waitForPackTransition;
        var useTornPackPieceMotion = piecesAlreadyFanned || isTornPackTransition;
        var pieceStartSpreadMultiplier = useTornPackPieceMotion
            ? GameDefine.TornPackPieceStartSpreadMultiplier
            : 1f;
        var pieceSettleDuration = useTornPackPieceMotion
            ? Mathf.Max(
                0.01f,
                GameEntrancePieceSettleDuration - TornPackPieceSettleReduction)
            : GameEntrancePieceSettleDuration;
        var pieceStagger = useTornPackPieceMotion
            ? TornPackPieceStagger
            : GameEntrancePieceStagger;

        Coroutine pieceDealCoroutine = null;
        if (piecesAlreadyFanned)
        {
            pieceDealCoroutine = StartCoroutine(
                PlayCurrentGroupPieceDealAnimation(
                    waitForPackTransition: false,
                    startVisibleAtOpeningOrigin: true,
                    spreadAtOpeningOrigin: true,
                    boardRect,
                    boardTarget,
                    trayRect,
                    trayTarget,
                    pieceSettleDuration,
                    pieceStagger,
                    pieceStartSpreadMultiplier));
        }
        else if (waitForPackTransition)
        {
            // Cache tray targets and show the pieces before moving the tray off-screen.
            pieceDealCoroutine = StartCoroutine(
                PlayCurrentGroupPieceDealAnimation(
                    waitForPackTransition: true,
                    startVisibleAtOpeningOrigin: false,
                    spreadAtOpeningOrigin: isTornPackTransition,
                    boardRect,
                    boardTarget,
                    trayRect,
                    trayTarget,
                    pieceSettleDuration,
                    pieceStagger,
                    pieceStartSpreadMultiplier));
        }
        else
        {
            var pieceCount = _drag.CurrentGroupDraggables.Count;
            for (var i = 0; i < pieceCount; i++)
            {
                var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardStart;
        }

        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayStart;
        }

        // Let the board and tray start pose reach the screen before advancing its clock.
        for (var frame = 0; frame < GameEntranceWarmupFrameCount; frame++)
        {
            yield return null;
        }
        var totalDuration = Mathf.Max(
            GameEntranceBoardDelay + GameEntranceBoardDuration,
            GameEntranceTrayDelay + GameEntranceTrayDuration,
            GameEntranceControlDelay + GameEntranceControlDuration);
        var elapsed = 0f;
        var didStartOutlineFade = false;
        while (elapsed < totalDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            if (boardRect != null)
            {
                var boardT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (elapsed - GameEntranceBoardDelay) / GameEntranceBoardDuration));
                boardRect.anchoredPosition = Vector2.LerpUnclamped(boardStart, boardTarget, boardT);
            }

            if (!didStartOutlineFade
                && elapsed >= GameEntranceBoardDelay + GameEntranceBoardDuration)
            {
                didStartOutlineFade = true;
                FadeInActiveGroupOutline();
            }

            if (trayRect != null)
            {
                var trayT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (elapsed - GameEntranceTrayDelay) / GameEntranceTrayDuration));
                trayRect.anchoredPosition = Vector2.LerpUnclamped(trayStart, trayTarget, trayT);
            }

            var controlT = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    (elapsed - GameEntranceControlDelay) / GameEntranceControlDuration));
            SetCanvasGroupAlpha(returnCanvasGroup, controlT);
            SetCanvasGroupAlpha(hintCanvasGroup, controlT);

            yield return null;
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardTarget;
        }

        if (!didStartOutlineFade)
        {
            FadeInActiveGroupOutline();
        }

        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayTarget;
        }

        SetCanvasGroupAlpha(returnCanvasGroup, 1f);
        SetCanvasGroupAlpha(hintCanvasGroup, 1f);
        Debug.Log("GameScene: board and tray entrance completed.");

        if (pieceDealCoroutine != null)
        {
            yield return pieceDealCoroutine;
        }
        else
        {
            yield return PlayCurrentGroupPieceDealAnimation(
                waitForPackTransition: false,
                startVisibleAtOpeningOrigin: false,
                spreadAtOpeningOrigin: false,
                boardRect,
                boardTarget,
                trayRect,
                trayTarget,
                pieceSettleDuration,
                pieceStagger,
                1f);
        }

        _isEntranceAnimating = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = !_isGameFinished;
        }
#endif
        TryStartPiecePlacementTutorial();
    }

    private IEnumerator PlayCurrentGroupPieceDealAnimation(
        bool waitForPackTransition,
        bool startVisibleAtOpeningOrigin,
        bool spreadAtOpeningOrigin,
        RectTransform boardRect,
        Vector2 boardTarget,
        RectTransform trayRect,
        Vector2 trayTarget,
        float pieceSettleDuration,
        float pieceStagger,
        float pieceStartSpreadMultiplier)
    {
        SetInitialPieceRenderQueueBehindPack(startVisibleAtOpeningOrigin);
        var camera = Camera.main;
        var boardCenter = _board.GameBoardImage != null && camera != null
            ? GameCommonUtility.RectTransformToCameraWorld(
                _board.GameBoardImage.rectTransform,
                camera,
                WorldGameplayDepth)
            : Vector3.zero;
        var pieceEntranceOrigin = GetOpeningPackPieceOriginWorldPosition(camera, boardCenter);

        var pieceCount = _drag.CurrentGroupDraggables.Count;
        var pieceTargets = new Vector3[pieceCount];
        var pieceStarts = new Vector3[pieceCount];
        var pieceTargetScales = new Vector3[pieceCount];
        var pieceTargetRotations = new Quaternion[pieceCount];
        var pieceStartRotations = new Quaternion[pieceCount];
        var pieceTargetColors = new Color[pieceCount];
        for (var i = 0; i < pieceCount; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            var renderer = state?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            // Every card-pack entrance uses the initial loose-piece shadow while dealing.
            ApplyPieceRendererShadow(renderer, PieceShadowStyle.Initial);
            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = SanitizeTrayPieceScale(state.TrayScale);
            pieceTargetRotations[i] = renderer.transform.rotation;
            pieceTargetColors[i] = renderer.color;
            var scatterOffset = GameDefine.CalculatePieceDealScatterOffset(
                i,
                pieceStartSpreadMultiplier);
            var shouldSpreadPieceStart = spreadAtOpeningOrigin
                                         || (!waitForPackTransition
                                             && !startVisibleAtOpeningOrigin);
            pieceStarts[i] = shouldSpreadPieceStart
                ? pieceEntranceOrigin + new Vector3(
                    scatterOffset.x,
                    scatterOffset.y,
                    0f)
                : pieceEntranceOrigin;
            pieceStarts[i].z = pieceTargets[i].z;
            pieceStartRotations[i] = waitForPackTransition || startVisibleAtOpeningOrigin
                ? Quaternion.identity
                : Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 18f);
            renderer.enabled = true;
            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i];
            renderer.transform.rotation = pieceStartRotations[i];
            var color = pieceTargetColors[i];
            if (!waitForPackTransition && !startVisibleAtOpeningOrigin)
            {
                color.a = 0f;
            }
            renderer.color = color;
        }

        for (var frame = 0; frame < GameEntranceWarmupFrameCount; frame++)
        {
            yield return null;
        }

        CacheCurrentGroupPieceDealTargets(
            boardRect,
            boardTarget,
            trayRect,
            trayTarget,
            pieceStarts,
            pieceTargets,
            pieceTargetScales);

        if (startVisibleAtOpeningOrigin)
        {
            // The opening pack only needs to occlude the pieces while they are revealed at
            // the tear. Restore the gameplay queue before the pieces fly over the board UI.
            SetInitialPieceRenderQueueBehindPack(false);
        }

        Coroutine packExitCoroutine = null;
        if (waitForPackTransition)
        {
            Debug.Log(
                "GameScene: pieces created at the pack origin; "
                + "waiting for the pack to drop below them.");
            yield return CardPackGameEntranceTransition.WaitForPieceLaunch();
            if (CardPackGameEntranceTransition.IsPending)
            {
                packExitCoroutine = StartCoroutine(
                    CardPackGameEntranceTransition.FinishAfterPieceLaunch());
            }
        }

        if (pieceCount > 0)
        {
            AudioManager.Instance.PlaySfx("SFX_PieceDeal.mp3");
        }

        var totalDuration = Mathf.Max(0, pieceCount - 1) * pieceStagger
                            + pieceSettleDuration;
        var elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            for (var i = 0; i < pieceCount; i++)
            {
                var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
                if (renderer == null)
                {
                    continue;
                }

                var pieceDelay = i * pieceStagger;
                var flightT = Mathf.Clamp01(
                    (elapsed - pieceDelay)
                    / pieceSettleDuration);
                var flightEased = Mathf.SmoothStep(0f, 1f, flightT);
                renderer.transform.position = Vector3.LerpUnclamped(
                    pieceStarts[i],
                    pieceTargets[i],
                    flightEased);
                renderer.transform.localScale = pieceTargetScales[i];
                renderer.transform.rotation = Quaternion.SlerpUnclamped(
                    pieceStartRotations[i],
                    pieceTargetRotations[i],
                    flightEased);
                var color = pieceTargetColors[i];
                if (!waitForPackTransition && !startVisibleAtOpeningOrigin)
                {
                    color.a *= Mathf.Clamp01(flightT * 2.5f);
                }
                renderer.color = color;
            }

            yield return null;
        }

        for (var i = 0; i < pieceCount; i++)
        {
            var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            renderer.transform.position = pieceTargets[i];
            renderer.transform.localScale = pieceTargetScales[i];
            renderer.transform.rotation = pieceTargetRotations[i];
            renderer.color = pieceTargetColors[i];
            EnsureDraggablePieceLight(_drag.CurrentGroupDraggables[i]);
        }

        if (packExitCoroutine != null)
        {
            yield return packExitCoroutine;
        }

        SetInitialPieceRenderQueueBehindPack(false);
    }

    private void CacheCurrentGroupPieceDealTargets(
        RectTransform boardRect,
        Vector2 boardTarget,
        RectTransform trayRect,
        Vector2 trayTarget,
        IReadOnlyList<Vector3> pieceStarts,
        IList<Vector3> pieceTargets,
        IList<Vector3> pieceTargetScales)
    {
        var boardAnimationPosition = boardRect != null
            ? boardRect.anchoredPosition
            : Vector2.zero;
        var trayAnimationPosition = trayRect != null
            ? trayRect.anchoredPosition
            : Vector2.zero;

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardTarget;
        }

        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayTarget;
        }

        Canvas.ForceUpdateCanvases();
        Physics2D.SyncTransforms();
        RefreshCurrentGroupTrayScalesAndLayout();

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            var renderer = state?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = SanitizeTrayPieceScale(state.TrayScale);
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardAnimationPosition;
        }

        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayAnimationPosition;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            var renderer = state?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i];
        }

        Canvas.ForceUpdateCanvases();
        Physics2D.SyncTransforms();
    }

    private static Vector3 GetOpeningPackPieceOriginWorldPosition(
        Camera camera,
        Vector3 fallback)
    {
        if (camera == null)
        {
            return fallback;
        }

        if (!GameManager.TryConsumeOpeningPackExitPosition(out var normalizedScreenPosition))
        {
            return fallback;
        }

        var screenPosition = new Vector3(
            Screen.width * normalizedScreenPosition.x,
            Screen.height * normalizedScreenPosition.y,
            Mathf.Abs(WorldGameplayDepth - camera.transform.position.z));
        var worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = WorldGameplayDepth;
        return worldPosition;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        var canvasGroup = target.GetComponent<CanvasGroup>();
        return canvasGroup != null ? canvasGroup : target.AddComponent<CanvasGroup>();
    }

    private static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup == null)
        {
            return;
        }

        var clampedAlpha = Mathf.Clamp01(alpha);
        canvasGroup.alpha = clampedAlpha;
        canvasGroup.interactable = clampedAlpha >= 0.999f;
        canvasGroup.blocksRaycasts = clampedAlpha >= 0.999f;
    }

    private void EnsureCardBagLoaded(int bagId)
    {
        if (_loadedCardBagRoot != null)
        {
            return;
        }

        var resourcePath = GameDefine.FormatCardBagPrefabResourcesPath(bagId);
        var usedPreloadedPrefab = GameManager.TryGetPreloadedCardBagPrefab(
            bagId,
            out var preloadedPrefab);
        var prefab = usedPreloadedPrefab
            ? preloadedPrefab
            : Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"GameScene: card bag prefab not found at Resources/{resourcePath}.");
            return;
        }

        var canvas = FindGameplayCanvas();
        var parent = canvas != null ? canvas.transform : null;
        if (parent == null)
        {
            Debug.LogError(
                "GameScene: scene Canvas not found; CardBag prefab cannot be attached safely.");
            return;
        }

        _loadedCardBagRoot = Instantiate(prefab, parent, false);
        _loadedCardBagRoot.name = prefab.name;
        ApplyCardBoardBackground();

        var rectTransform = _loadedCardBagRoot.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            _loadedCardBagRect = rectTransform;
            _originalCardBagLocalScale = rectTransform.localScale;
            _hasOriginalCardBagLocalScale = true;
            ApplyConfiguredBoardScale();
            PlaceCardBagAfterBackground(rectTransform);
            _originalCardBagAnchoredPosition = rectTransform.anchoredPosition;
            _hasOriginalCardBagAnchoredPosition = true;
        }

        _board.IsBoardAndGroovesInitialized = false;
        Debug.Log(
            $"GameScene: loaded card bag prefab Resources/{resourcePath}. "
            + $"source={(usedPreloadedPrefab ? "preloaded" : "synchronous")}, "
            + $"canvasScene={canvas.gameObject.scene.name}, "
            + $"canvasScale={canvas.transform.localScale}");
    }

    private void ApplyCardBoardBackground()
    {
        var rootImage = _loadedCardBagRoot != null
            ? _loadedCardBagRoot.GetComponent<Image>()
            : null;
        if (rootImage == null)
        {
            Debug.LogWarning("GameScene: loaded CardBag root has no Image component.");
            return;
        }

        rootImage.sprite = null;
        var backgroundImages = _loadedCardBagRoot
            .GetComponentsInChildren<RawImage>(true)
            .Where(image => image.transform.parent == _loadedCardBagRoot.transform
                            && image.gameObject.name.StartsWith(
                                "BoardBg",
                                StringComparison.Ordinal))
            .ToArray();
        if (backgroundImages.Length == 0)
        {
            Debug.LogWarning("GameScene: loaded CardBag has no BoardBg RawImage nodes.");
            return;
        }

        var settings = GameSettingsUtility.GetSettings();
        var useHighContrast = settings != null && settings.IsHighContrastEnabled;
        var backgroundPath = useHighContrast
            ? HighContrastCardBoardBackgroundPath
            : StandardCardBoardBackgroundPath;
        var sprite = GameCommonUtility.LoadSpriteByPath(backgroundPath, PixelsPerUnit);
        if (sprite == null)
        {
            Debug.LogWarning(
                $"GameScene: failed to load CardBag background {backgroundPath}; keeping prefab background.");
            return;
        }

        DestroyRuntimeCardBoardBackgroundSprite();
        _runtimeCardBoardBackgroundSprite = sprite;
        for (var i = 0; i < backgroundImages.Length; i++)
        {
            backgroundImages[i].texture = sprite.texture;
        }
    }

    private void DestroyRuntimeCardBoardBackgroundSprite()
    {
        if (_runtimeCardBoardBackgroundSprite == null)
        {
            return;
        }

        var texture = _runtimeCardBoardBackgroundSprite.texture;
        Destroy(_runtimeCardBoardBackgroundSprite);
        _runtimeCardBoardBackgroundSprite = null;
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    private void ApplyConfiguredBoardScale()
    {
        if (_loadedCardBagRect == null || !_hasOriginalCardBagLocalScale)
        {
            return;
        }

        _loadedCardBagRect.localScale = _originalCardBagLocalScale * _configuredBoardScale;
        Canvas.ForceUpdateCanvases();
    }

    private static void PlaceCardBagAfterBackground(RectTransform cardBagRect)
    {
        var background = GameObject.Find(GameDefine.BackgroundObjectName);
        if (background == null || background.transform.parent != cardBagRect.parent)
        {
            cardBagRect.SetAsFirstSibling();
            return;
        }

        cardBagRect.SetSiblingIndex(background.transform.GetSiblingIndex() + 1);
    }

    private void ConfigureGameplayCanvas(Camera camera)
    {
        var canvas = FindGameplayCanvas();
        if (canvas == null)
        {
            Debug.LogError("GameScene: scene Canvas not found; gameplay UI cannot be configured.");
            return;
        }

        if (camera != null && canvas.transform.parent == camera.transform)
        {
            canvas.transform.SetParent(null, worldPositionStays: true);
        }

        var canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.localScale = Vector3.one;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (camera != null)
        {
            GameCommonUtility.ConfigureCanvasForGameplay(
                canvas,
                camera,
                ReferenceHeight * (16f / 9f),
                ReferenceHeight,
                PixelsPerUnit);
        }

        Canvas.ForceUpdateCanvases();
    }

    private static void RefreshGameplayViewport(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        GameCommonUtility.ConfigureFixedAspectViewport(
            camera,
            ReferenceHeight * (16f / 9f),
            ReferenceHeight);
    }

    private static bool IsGameplayCanvasLayoutReady()
    {
        var canvas = FindGameplayCanvas();
        var canvasRect = canvas != null
            ? canvas.rootCanvas.transform as RectTransform
            : null;
        return canvasRect != null
               && canvasRect.rect.width > 0.001f
               && canvasRect.rect.height > 0.001f;
    }

    private static Canvas FindGameplayCanvas()
    {
        var canvasObject = GameCommonUtility.FindSceneObject("Canvas");
        return canvasObject != null ? canvasObject.GetComponent<Canvas>() : null;
    }

    private void EnsureBoardAndGroovesInitialized()
    {
        if (_board.IsBoardAndGroovesInitialized)
        {
            return;
        }

        _board.BackgroundRect = FindBackgroundRect();
        _board.PieceBoardRect = FindPieceBoardRect();
        _board.GameBoardImage = FindSceneImage(GameDefine.GameBoardObjectName);
        if (_board.GameBoardImage == null)
        {
            return;
        }

        if (_board.GameBoardImage != null && !_hasOriginalGameBoardAnchoredPosition)
        {
            _originalGameBoardAnchoredPosition = _board.GameBoardImage.rectTransform.anchoredPosition;
            _hasOriginalGameBoardAnchoredPosition = true;
        }

        _board.GrooveImagesByGroup = CollectEditorGrooveGroups();
        ApplyCardBagUiShadowMaterials();
        SyncGrooveLayoutToSprites();
        _board.IsBoardAndGroovesInitialized = true;
    }

    private void ApplyCardBagUiShadowMaterials()
    {
        if (!EnsureCardBagShadowMaterials() || _loadedCardBagRoot == null)
        {
            return;
        }

        var images = _loadedCardBagRoot.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null)
            {
                continue;
            }

            if (image.gameObject.name == GameDefine.GameBoardObjectName
                || image.gameObject.name == "BoardTitle")
            {
                ApplyUiShadowMaterial(image, _boardShadowMaterial);
                continue;
            }

            if (TryParsePieceObjectName(image.gameObject.name, out _))
            {
                ApplyUiShadowMaterial(image, _placedPieceShadowMaterial);
            }
        }
    }

    private static void ApplyUiShadowMaterial(Image image, Material material)
    {
        if (image == null || material == null)
        {
            return;
        }

        image.material = material;
        if (image.GetComponent<PackCoverShadowEffect>() == null)
        {
            image.gameObject.AddComponent<PackCoverShadowEffect>();
        }
    }

    private void ApplyPlacedPieceImageShadow(Image image)
    {
        if (!EnsureCardBagShadowMaterials())
        {
            return;
        }

        ApplyUiShadowMaterial(image, _placedPieceShadowMaterial);
    }

    private bool EnsureCardBagShadowMaterials()
    {
        if (_boardShadowMaterial == null)
        {
            _boardShadowMaterial = Resources.Load<Material>(BoardShadowMaterialResourcesPath);
        }

        if (_defaultPieceShadowMaterial == null)
        {
            _defaultPieceShadowMaterial = Resources.Load<Material>(DefaultPieceShadowMaterialResourcesPath);
        }

        if (_loosePieceShadowMaterial == null)
        {
            _loosePieceShadowMaterial = Resources.Load<Material>(LoosePieceShadowMaterialResourcesPath);
        }

        if (_placedPieceShadowMaterial == null)
        {
            _placedPieceShadowMaterial = Resources.Load<Material>(PlacedPieceShadowMaterialResourcesPath);
        }

        return _boardShadowMaterial != null
               && _defaultPieceShadowMaterial != null
               && _loosePieceShadowMaterial != null
               && _placedPieceShadowMaterial != null;
    }

    private static RectTransform FindBackgroundRect()
    {
        var background = GameObject.Find(GameDefine.BackgroundObjectName);
        return background != null ? background.GetComponent<RectTransform>() : null;
    }

    private static RectTransform FindPieceBoardRect()
    {
        var pieceBoard = GameObject.Find(GameDefine.PieceBoardObjectName);
        return pieceBoard != null ? pieceBoard.GetComponent<RectTransform>() : null;
    }

    private void EnsurePieceBoardInitialized()
    {
        if (_board.PieceBoardRect == null)
        {
            _board.PieceBoardRect = FindPieceBoardRect();
        }

        if (_board.PieceBoardRect == null || _hasPieceBoardOriginalAnchoredPosition)
        {
            return;
        }

        _pieceBoardOriginalAnchoredPosition = _board.PieceBoardRect.anchoredPosition;
        _hasPieceBoardOriginalAnchoredPosition = true;
        _isPieceBoardHidden = false;
    }

    private Image FindSceneImage(string objectName)
    {
        if (_loadedCardBagRoot == null)
        {
            return null;
        }

        return _loadedCardBagRoot
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.gameObject.name == objectName);
    }

    private List<List<Image>> CollectEditorGrooveGroups()
    {
        var sortedGrooves = CollectSortedEditorPieceGrooves();
        for (var i = 0; i < sortedGrooves.Count; i++)
        {
            SetImageAlpha(sortedGrooves[i], 0f);
        }

        if (sortedGrooves.Count == 0)
        {
            return new List<List<Image>>();
        }

        return SplitGroovesIntoNumberedGroups(sortedGrooves);
    }

    private static List<List<Image>> SplitGroovesIntoNumberedGroups(List<Image> sortedGrooves)
    {
        var groupsByNumber = new SortedDictionary<int, List<Image>>();
        for (var i = 0; i < sortedGrooves.Count; i++)
        {
            var grooveImage = sortedGrooves[i];
            if (grooveImage == null || !TryGetNumberedGroup(grooveImage, out var groupNumber))
            {
                continue;
            }

            if (!groupsByNumber.TryGetValue(groupNumber, out var group))
            {
                group = new List<Image>();
                groupsByNumber[groupNumber] = group;
            }

            group.Add(grooveImage);
        }

        var groups = new List<List<Image>>();
        foreach (var group in groupsByNumber.Values)
        {
            group.Sort((a, b) => GetPieceNumberFromImage(a).CompareTo(GetPieceNumberFromImage(b)));
            groups.Add(group);
        }

        return groups;
    }

    private static int GetPieceNumberFromImage(Image image)
    {
        if (image == null || !TryParsePieceObjectName(image.gameObject.name, out var pieceNumber))
        {
            return int.MaxValue;
        }

        return pieceNumber;
    }

    private static int GetPieceNumberFromState(DraggablePieceState state)
    {
        if (state?.GrooveImage == null)
        {
            return int.MaxValue;
        }

        return GetPieceNumberFromImage(state.GrooveImage);
    }

    private static bool TryGetNumberedGroup(Image image, out int groupNumber)
    {
        groupNumber = 0;
        return image != null
               && GameDefine.TryParsePieceObjectName(
                   image.gameObject.name,
                   out groupNumber,
                   out _,
                   out _);
    }

    private List<Image> CollectSortedEditorPieceGrooves()
    {
        var groovesByNumber = new Dictionary<int, Image>();
        var images = _loadedCardBagRoot != null
            ? _loadedCardBagRoot.GetComponentsInChildren<Image>(true)
            : Array.Empty<Image>();
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null || !TryParsePieceObjectName(image.gameObject.name, out var pieceNumber))
            {
                continue;
            }

            groovesByNumber[pieceNumber] = image;
        }

        var numbers = new List<int>(groovesByNumber.Keys);
        numbers.Sort();
        var sortedGrooves = new List<Image>(numbers.Count);
        for (var i = 0; i < numbers.Count; i++)
        {
            sortedGrooves.Add(groovesByNumber[numbers[i]]);
        }

        return sortedGrooves;
    }

    private static bool TryParsePieceObjectName(string objectName, out int pieceNumber)
    {
        return GameDefine.TryParsePieceObjectName(objectName, out pieceNumber);
    }

    private void SyncGrooveLayoutToSprites()
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var i = 0; i < group.Count; i++)
            {
                SyncImageSizeToSprite(group[i]);
            }
        }
    }

    private static void SyncImageSizeToSprite(Image image)
    {
        if (image == null || image.sprite == null)
        {
            return;
        }

        var rectTransform = image.rectTransform;
        rectTransform.sizeDelta = image.sprite.rect.size;
    }

    private Vector2 GetBoardDisplayWorldSize()
    {
        if (_board.GameBoardImage == null)
        {
            return new Vector2(18f, 18.6f);
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return GameCommonUtility.GetRectTransformWorldBounds(_board.GameBoardImage.rectTransform).size;
        }

        return GameCommonUtility.GetRectTransformCameraWorldBounds(
            _board.GameBoardImage.rectTransform,
            camera).size;
    }

    /// <summary>
    /// 用途：计算 GameBoard 在场景中的显示缩放与贴图 PPU 世界尺寸之间的比例。返回：统一缩放系数。
    /// </summary>
    private float GetBoardToSpriteScaleFactor()
    {
        if (_board.GameBoardImage == null || _board.GameBoardImage.sprite == null)
        {
            return 1f;
        }

        var boardDisplaySize = GetBoardDisplayWorldSize();
        var boardTextureWorldSize = _board.GameBoardImage.sprite.rect.size / PixelsPerUnit;
        if (boardTextureWorldSize.x <= 0.001f || boardTextureWorldSize.y <= 0.001f)
        {
            return 1f;
        }

        var factorX = boardDisplaySize.x / boardTextureWorldSize.x;
        var factorY = boardDisplaySize.y / boardTextureWorldSize.y;
        return Mathf.Min(factorX, factorY);
    }

    private Vector3 CalculatePieceScaleOnBoard(
        Image grooveImage,
        SpriteRenderer pieceRenderer = null)
    {
        if (TryCalculatePieceScaleFromScreenRect(
                grooveImage,
                pieceRenderer,
                out var screenMatchedScale))
        {
            return screenMatchedScale;
        }

        if (grooveImage == null || grooveImage.sprite == null || Camera.main == null)
        {
            return Vector3.one * GetBoardToSpriteScaleFactor();
        }

        var sprite = grooveImage.sprite;
        var pixelsPerUnit = Mathf.Max(0.001f, sprite.pixelsPerUnit);
        var spriteWorldSize = sprite.rect.size / pixelsPerUnit;
        var grooveWorldSize = GameCommonUtility.GetRectTransformCameraWorldBounds(
            grooveImage.rectTransform,
            Camera.main,
            WorldGameplayDepth).size;
        if (spriteWorldSize.x <= 0.001f
            || spriteWorldSize.y <= 0.001f
            || grooveWorldSize.x <= 0.001f
            || grooveWorldSize.y <= 0.001f)
        {
            return Vector3.one * GetBoardToSpriteScaleFactor();
        }

        return new Vector3(
            grooveWorldSize.x / spriteWorldSize.x,
            grooveWorldSize.y / spriteWorldSize.y,
            1f);
    }

    private static bool TryCalculatePieceScaleFromScreenRect(
        Image grooveImage,
        SpriteRenderer pieceRenderer,
        out Vector3 scale)
    {
        scale = Vector3.one;
        var camera = Camera.main;
        if (grooveImage == null
            || grooveImage.sprite == null
            || pieceRenderer == null
            || pieceRenderer.sprite == null
            || camera == null)
        {
            return false;
        }

        var grooveScreenRect = GetRectTransformScreenRect(
            grooveImage.rectTransform,
            camera);
        if (!TryGetRendererUnitScreenSize(
                pieceRenderer,
                camera,
                out var rendererUnitScreenSize))
        {
            return false;
        }

        if (grooveScreenRect.width <= 0.001f
            || grooveScreenRect.height <= 0.001f
            || rendererUnitScreenSize.x <= 0.001f
            || rendererUnitScreenSize.y <= 0.001f)
        {
            return false;
        }

        scale = new Vector3(
            grooveScreenRect.width / rendererUnitScreenSize.x,
            grooveScreenRect.height / rendererUnitScreenSize.y,
            1f);
        return IsFinitePositiveScale(scale);
    }

    private static bool TryGetRendererUnitScreenSize(
        SpriteRenderer pieceRenderer,
        Camera camera,
        out Vector2 screenSize)
    {
        screenSize = Vector2.zero;
        if (pieceRenderer == null || pieceRenderer.sprite == null || camera == null)
        {
            return false;
        }

        var spriteLocalSize = pieceRenderer.sprite.bounds.size;
        var parent = pieceRenderer.transform.parent;
        var unitWidthWorld = parent != null
            ? parent.TransformVector(Vector3.right * spriteLocalSize.x)
            : Vector3.right * spriteLocalSize.x;
        var unitHeightWorld = parent != null
            ? parent.TransformVector(Vector3.up * spriteLocalSize.y)
            : Vector3.up * spriteLocalSize.y;
        var originWorld = pieceRenderer.transform.position;
        var originScreen = camera.WorldToScreenPoint(originWorld);
        var widthScreen = camera.WorldToScreenPoint(originWorld + unitWidthWorld);
        var heightScreen = camera.WorldToScreenPoint(originWorld + unitHeightWorld);
        screenSize = new Vector2(
            Vector2.Distance(originScreen, widthScreen),
            Vector2.Distance(originScreen, heightScreen));
        return screenSize.x > 0.001f && screenSize.y > 0.001f;
    }

    private static bool IsFinitePositiveScale(Vector3 scale)
    {
        return scale.x > 0f
               && scale.y > 0f
               && scale.z > 0f
               && !float.IsNaN(scale.x)
               && !float.IsNaN(scale.y)
               && !float.IsNaN(scale.z)
               && !float.IsInfinity(scale.x)
               && !float.IsInfinity(scale.y)
               && !float.IsInfinity(scale.z);
    }

    private Vector3 CalculateTrayScaleForPiece(
        SpriteRenderer pieceRenderer,
        Bounds hostBounds,
        Vector3 dragScale)
    {
        var trayScale = SanitizeTrayPieceScale(dragScale);
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return trayScale;
        }

        var camera = Camera.main;
        if (TryGetPieceTrayScreenRect(camera, out var trayScreenRect)
            && TryGetRendererUnitScreenSize(pieceRenderer, camera, out var unitScreenSize)
            && trayScreenRect.height > 0.0001f
            && unitScreenSize.y > 0.0001f)
        {
            var dragScreenHeight = unitScreenSize.y * trayScale.y;
            var maxTrayScreenHeight = trayScreenRect.height * PieceTrayMaxHeightRatio;
            return dragScreenHeight <= maxTrayScreenHeight
                ? trayScale
                : SanitizeTrayPieceScale(
                    trayScale * (maxTrayScreenHeight / dragScreenHeight));
        }

        var spriteLocalHeight = pieceRenderer.sprite.bounds.size.y;
        var parent = pieceRenderer.transform.parent;
        var unitWorldHeight = parent != null
            ? parent.TransformVector(Vector3.up * spriteLocalHeight).magnitude
            : spriteLocalHeight;
        if (unitWorldHeight <= 0.0001f || hostBounds.size.y <= 0.0001f)
        {
            return trayScale;
        }

        var dragWorldHeight = unitWorldHeight * trayScale.y;
        var maxTrayWorldHeight = hostBounds.size.y * PieceTrayMaxHeightRatio;
        return dragWorldHeight <= maxTrayWorldHeight
            ? trayScale
            : SanitizeTrayPieceScale(
                trayScale * (maxTrayWorldHeight / dragWorldHeight));
    }

    private static Vector3 SanitizeTrayPieceScale(Vector3 scale)
    {
        if (!IsFinitePositiveScale(scale))
        {
            return Vector3.one;
        }

        scale.z = 1f;
        return scale;
    }

    private void RefreshCurrentGroupTrayScalesAndLayout()
    {
        Canvas.ForceUpdateCanvases();
        Physics2D.SyncTransforms();
        var hostBounds = GetPieceTrayBounds();
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state?.PieceRenderer == null || state.IsPlaced)
            {
                continue;
            }

            state.DragScale = CalculatePieceScaleOnBoard(
                state.GrooveImage,
                state.PieceRenderer);
            state.BoardScale = state.DragScale;
            if (!state.IsOnTray)
            {
                continue;
            }

            state.TrayScale = CalculateTrayScaleForPiece(
                state.PieceRenderer,
                hostBounds,
                state.DragScale);
            state.PieceRenderer.transform.localScale = state.TrayScale;
        }

        LayoutTrayPieces();
        Physics2D.SyncTransforms();
    }

    private IEnumerator RefreshCurrentGroupTrayScalesNextFrame()
    {
        var expectedGroupIndex = _drag.CurrentGroupIndex;
        yield return null;
        if (_isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating
            || _drag.CurrentGroupIndex != expectedGroupIndex)
        {
            yield break;
        }

        RefreshCurrentGroupTrayScalesAndLayout();
    }

    private SpriteRenderer CreatePieceBackground()
    {
        if (_board.GameBoardImage == null)
        {
            return null;
        }

        var camera = Camera.main;
        var boardWorldSize = GetBoardDisplayWorldSize();
        var boardWidth = boardWorldSize.x;
        var bgHeight = boardWorldSize.y * 0.25f;
        var boardBounds = camera != null
            ? GameCommonUtility.GetRectTransformCameraWorldBounds(_board.GameBoardImage.rectTransform, camera)
            : GameCommonUtility.GetRectTransformWorldBounds(_board.GameBoardImage.rectTransform);
        var bottomEdge = camera != null ? camera.transform.position.y - camera.orthographicSize : boardBounds.min.y;
        var bgCenterY = bottomEdge + bgHeight * 0.5f;
        var bgCenterX = camera != null ? camera.transform.position.x : boardBounds.center.x;
        var bgPosition = new Vector3(bgCenterX, bgCenterY, WorldGameplayDepth);

        var renderer = CreateSpriteObject(
            PieceBgObjectName,
            PieceBgPath,
            PieceBgSortingOrder,
            parent: null,
            forceCreate: true);
        if (renderer == null)
        {
            var fallbackObject = new GameObject(PieceBgObjectName);
            renderer = fallbackObject.AddComponent<SpriteRenderer>();
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = GameCommonUtility.CreateSolidSprite(Color.white, PixelsPerUnit);
        }

        var slicedSprite = GameCommonUtility.CreateSlicedSpriteByPath(PieceBgPath, PixelsPerUnit, renderer.sprite);
        if (slicedSprite != null)
        {
            renderer.sprite = slicedSprite;
        }

        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(boardWidth, bgHeight);
        renderer.transform.position = bgPosition;
        renderer.color = new Color(0f, 0f, 0f, PieceBgAlpha);
        CreateOrUpdatePieceBgFill(renderer);
        CachePieceBgOriginalPosition();
        return renderer;
    }

    private void CreateDraggableGroup(
        int groupIndex,
        bool allowOutlineDuringTutorialTransition = false)
    {
        if (groupIndex > 0)
        {
            FinalizeCompletedGroup(groupIndex - 1);
        }

        PreparePieceTrayForGroupStart();
        ClearAmbientPieceLights();
        ClearCurrentDraggableGroup();
        _drag.CurrentGroupIndex = groupIndex;

        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var grooveGroup = _board.GrooveImagesByGroup[groupIndex];
        if (grooveGroup == null || grooveGroup.Count == 0)
        {
            return;
        }

        UpdateGrooveGroupVisibility(groupIndex);
        ResetBoardPanState();
        FitCameraToActiveGroup(groupIndex);

        var root = new GameObject(DraggableGroupRootObjectName);
        var hostBounds = GetPieceTrayBounds();

        for (var i = 0; i < grooveGroup.Count; i++)
        {
            var grooveImage = grooveGroup[i];
            if (grooveImage == null || grooveImage.sprite == null)
            {
                Debug.LogWarning($"GameScene: groove image missing sprite at index {i}.");
                continue;
            }

            if (IsGroovePersistedAsPlaced(grooveImage))
            {
                grooveImage.gameObject.SetActive(true);
                ApplyPlacedPieceImageShadow(grooveImage);
                SetImageAlpha(grooveImage, 1f);
                AddAmbientBoardPieceLights(grooveImage);
                continue;
            }

            var pieceRenderer = CreateDraggablePieceFromGroove(
                grooveImage,
                $"DraggablePiece_{groupIndex}_{i}",
                root.transform);
            if (pieceRenderer == null)
            {
                continue;
            }

            var pieceCollider = CreateSpriteOverlapCollider(
                pieceRenderer.gameObject,
                grooveImage.sprite);
            ApplyPieceRendererShadow(pieceRenderer, PieceShadowStyle.Initial);
            var dragScale = CalculatePieceScaleOnBoard(grooveImage, pieceRenderer);
            var boardScale = dragScale;
            var trayScale = CalculateTrayScaleForPiece(
                pieceRenderer,
                hostBounds,
                dragScale);
            pieceRenderer.transform.localScale = trayScale;
            var grooveProbeCollider = CreateGrooveOverlapProbe(
                grooveImage.sprite,
                root.transform,
                $"GrooveProbe_{groupIndex}_{i}");
            _drag.CurrentGroupDraggables.Add(new DraggablePieceState
            {
                PieceRenderer = pieceRenderer,
                PieceCollider = pieceCollider,
                GrooveProbeCollider = grooveProbeCollider,
                GrooveImage = grooveImage,
                GrooveRect = grooveImage.rectTransform,
                StartPosition = pieceRenderer.transform.position,
                TrayScale = trayScale,
                DragScale = dragScale,
                BoardScale = boardScale,
                IsOnTray = true,
                IsPlaced = false
            });
        }

        ShuffleCurrentGroupDraggables(groupIndex);
        AddAmbientLightsForCompletedGroups(groupIndex);

        LayoutTrayPieces();
        CachePieceTrayOriginalPosition();
        TryRefreshActiveGroupOutline(
            groupIndex,
            startHidden: true,
            ignoreTutorialBlock: allowOutlineDuringTutorialTransition);
    }

    private void ShuffleCurrentGroupDraggables(int groupIndex)
    {
        var pieces = _drag.CurrentGroupDraggables;
        if (pieces.Count < 2)
        {
            return;
        }

        var originalOrder = pieces.ToArray();
        var seed = unchecked(
            Environment.TickCount
            ^ (Time.frameCount * 397)
            ^ (GameManager.GetBagId() * 486187739)
            ^ (groupIndex * 16777619));
        var random = new System.Random(seed);
        for (var i = pieces.Count - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (pieces[i], pieces[swapIndex]) = (pieces[swapIndex], pieces[i]);
        }

        if (pieces.Where((piece, index) => piece != originalOrder[index]).Any())
        {
            return;
        }

        var forcedSwapIndex = random.Next(1, pieces.Count);
        (pieces[0], pieces[forcedSwapIndex]) = (pieces[forcedSwapIndex], pieces[0]);
    }

    private static SpriteRenderer CreateDraggablePieceFromGroove(Image grooveImage, string objectName, Transform parent)
    {
        return GameCommonUtility.CreateSpriteRendererFromSprite(
            objectName,
            grooveImage.sprite,
            PieceSortingOrder,
            parent,
            forceCreate: true);
    }

    private void ApplyPieceRendererShadow(SpriteRenderer renderer, PieceShadowStyle style)
    {
        if (renderer == null || renderer.sprite == null || !EnsureCardBagShadowMaterials())
        {
            return;
        }

        var material = GetOrCreatePieceRendererShadowMaterial(style);
        if (material == null)
        {
            return;
        }

        ApplyPieceRendererMaterial(renderer, material);
    }

    private void ApplyClusterMemberMaterial(SpriteRenderer renderer)
    {
        if (renderer == null || renderer.sprite == null || !EnsureCardBagShadowMaterials())
        {
            return;
        }

        if (_runtimeClusterPieceMaterial == null)
        {
            _runtimeClusterPieceMaterial = new Material(_loosePieceShadowMaterial)
            {
                name = "Loose Cluster Piece (No Shadow Runtime)"
            };
            _runtimeClusterPieceMaterial.EnableKeyword(SpriteRendererShadowKeyword);
            var shadowColor = _runtimeClusterPieceMaterial.GetColor(ShadowColorId);
            shadowColor.a = 0f;
            _runtimeClusterPieceMaterial.SetColor(ShadowColorId, shadowColor);
        }

        ApplyPieceRendererMaterial(renderer, _runtimeClusterPieceMaterial);
    }

    private void ApplyPieceRendererMaterial(SpriteRenderer renderer, Material material)
    {
        renderer.sprite = GetOrCreateFullRectShadowSprite(renderer.sprite);
        renderer.sharedMaterial = material;
        if (_pieceShadowPropertyBlock == null)
        {
            _pieceShadowPropertyBlock = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(_pieceShadowPropertyBlock);
        _pieceShadowPropertyBlock.SetFloat(
            SpritePixelsPerUnitId,
            Mathf.Max(1f, renderer.sprite.pixelsPerUnit));
        renderer.SetPropertyBlock(_pieceShadowPropertyBlock);
        _pieceShadowPropertyBlock.Clear();
    }

    private Material GetOrCreateClusterShadowMaterial(PieceShadowStyle style)
    {
        if (!EnsureCardBagShadowMaterials())
        {
            return null;
        }

        var source = style == PieceShadowStyle.Initial
            ? _defaultPieceShadowMaterial
            : _loosePieceShadowMaterial;
        var runtimeMaterial = style == PieceShadowStyle.Initial
            ? _runtimeInitialClusterShadowMaterial
            : _runtimeLooseClusterShadowMaterial;
        if (runtimeMaterial != null)
        {
            return runtimeMaterial;
        }

        runtimeMaterial = new Material(source)
        {
            name = $"{source.name} (Cluster Shadow Runtime)"
        };
        runtimeMaterial.EnableKeyword(SpriteRendererShadowKeyword);
        runtimeMaterial.EnableKeyword(ShadowOnlyKeyword);
        if (style == PieceShadowStyle.Initial)
        {
            _runtimeInitialClusterShadowMaterial = runtimeMaterial;
        }
        else
        {
            _runtimeLooseClusterShadowMaterial = runtimeMaterial;
        }

        return runtimeMaterial;
    }

    private Material GetOrCreatePieceRendererShadowMaterial(PieceShadowStyle style)
    {
        switch (style)
        {
            case PieceShadowStyle.Loose:
                return GetOrCreatePieceRendererShadowMaterial(
                    _loosePieceShadowMaterial,
                    ref _runtimeLoosePieceShadowMaterial);
            case PieceShadowStyle.Placed:
                return GetOrCreatePieceRendererShadowMaterial(
                    _placedPieceShadowMaterial,
                    ref _runtimePlacedPieceShadowMaterial);
            default:
                return GetOrCreatePieceRendererShadowMaterial(
                    _defaultPieceShadowMaterial,
                    ref _runtimeDefaultPieceShadowMaterial);
        }
    }

    private void SetInitialPieceRenderQueueBehindPack(bool enabled)
    {
        if (!EnsureCardBagShadowMaterials())
        {
            return;
        }

        var material = GetOrCreatePieceRendererShadowMaterial(PieceShadowStyle.Initial);
        if (material == null)
        {
            return;
        }

        material.renderQueue = enabled
            ? OpeningPieceRenderQueue
            : _defaultPieceShadowMaterial.renderQueue;
    }

    private static Material GetOrCreatePieceRendererShadowMaterial(
        Material source,
        ref Material runtimeMaterial)
    {
        if (runtimeMaterial != null || source == null)
        {
            return runtimeMaterial;
        }

        runtimeMaterial = new Material(source)
        {
            name = $"{source.name} (SpriteRenderer Runtime)"
        };
        runtimeMaterial.EnableKeyword(SpriteRendererShadowKeyword);
        return runtimeMaterial;
    }

    private Sprite GetOrCreateFullRectShadowSprite(Sprite source)
    {
        if (source == null || _runtimeFullRectPieceShadowSprites.Contains(source))
        {
            return source;
        }

        if (_fullRectPieceShadowSprites.TryGetValue(source, out var existing)
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
        fullRectSprite.name = $"{source.name} (Shadow FullRect Runtime)";
        _fullRectPieceShadowSprites[source] = fullRectSprite;
        _runtimeFullRectPieceShadowSprites.Add(fullRectSprite);
        return fullRectSprite;
    }

    private void DestroyRuntimePieceShadowResources()
    {
        DestroyRuntimeMaterial(ref _runtimeDefaultPieceShadowMaterial);
        DestroyRuntimeMaterial(ref _runtimeLoosePieceShadowMaterial);
        DestroyRuntimeMaterial(ref _runtimePlacedPieceShadowMaterial);
        DestroyRuntimeMaterial(ref _runtimeClusterPieceMaterial);
        DestroyRuntimeMaterial(ref _runtimeInitialClusterShadowMaterial);
        DestroyRuntimeMaterial(ref _runtimeLooseClusterShadowMaterial);

        foreach (var sprite in _runtimeFullRectPieceShadowSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }

        _runtimeFullRectPieceShadowSprites.Clear();
        _fullRectPieceShadowSprites.Clear();
        _pieceShadowPropertyBlock = null;
    }

    private static void DestroyRuntimeMaterial(ref Material material)
    {
        if (material == null)
        {
            return;
        }

        Destroy(material);
        material = null;
    }

    private void SetLooseClusterPresentation(
        LoosePieceCluster cluster,
        PieceShadowStyle style,
        bool rebuildShadow = false)
    {
        if (cluster == null || cluster.Members.Count < 2)
        {
            return;
        }

        for (var i = 0; i < cluster.Members.Count; i++)
        {
            var renderer = cluster.Members[i]?.PieceRenderer;
            if (renderer != null)
            {
                ApplyClusterMemberMaterial(renderer);
            }
        }

        if (rebuildShadow || cluster.ShadowRenderer == null)
        {
            RebuildLooseClusterShadow(cluster, style);
            return;
        }

        cluster.ShadowStyle = style;
        cluster.ShadowRenderer.sharedMaterial = GetOrCreateClusterShadowMaterial(style);
        ApplyClusterShadowPropertyBlock(cluster.ShadowRenderer);
        UpdateLooseClusterShadowTransform(cluster);
    }

    private void RebuildLooseClusterShadow(
        LoosePieceCluster cluster,
        PieceShadowStyle style)
    {
        DestroyLooseClusterShadow(cluster);
        if (cluster == null || cluster.Members.Count < 2)
        {
            return;
        }

        if (!TryBuildLooseClusterShadowSprite(
                cluster,
                out var texture,
                out var sprite,
                out var worldCenter))
        {
            for (var i = 0; i < cluster.Members.Count; i++)
            {
                var renderer = cluster.Members[i]?.PieceRenderer;
                if (renderer != null)
                {
                    ApplyPieceRendererShadow(renderer, style);
                }
            }
            return;
        }

        var shadowObject = new GameObject(LooseClusterShadowObjectName);
        shadowObject.transform.position = worldCenter;
        shadowObject.transform.rotation = Quaternion.identity;
        shadowObject.transform.localScale = Vector3.one;
        var shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = sprite;
        shadowRenderer.sharedMaterial = GetOrCreateClusterShadowMaterial(style);

        cluster.ShadowRoot = shadowObject;
        cluster.ShadowRenderer = shadowRenderer;
        cluster.ShadowTexture = texture;
        cluster.ShadowSprite = sprite;
        cluster.ShadowStyle = style;
        ApplyClusterShadowPropertyBlock(shadowRenderer);
        UpdateLooseClusterShadowTransform(cluster);
    }

    private static bool TryBuildLooseClusterShadowSprite(
        LoosePieceCluster cluster,
        out Texture2D texture,
        out Sprite sprite,
        out Vector3 worldCenter)
    {
        texture = null;
        sprite = null;
        worldCenter = Vector3.zero;
        if (cluster == null || cluster.Members.Count < 2)
        {
            return false;
        }

        var sources = new List<LooseClusterShadowSource>(cluster.Members.Count);
        var hasWorldBounds = false;
        var worldBounds = default(Bounds);
        var pixelsPerWorldUnit = 1f;
        for (var i = 0; i < cluster.Members.Count; i++)
        {
            var renderer = cluster.Members[i]?.PieceRenderer;
            var sourceSprite = renderer != null ? renderer.sprite : null;
            if (sourceSprite == null
                || !HintDashedOutlineGraphic.TryReadSpritePixels(
                    sourceSprite,
                    out var pixels,
                    out var width,
                    out var height))
            {
                continue;
            }

            var source = new LooseClusterShadowSource
            {
                Renderer = renderer,
                Pixels = pixels,
                Width = width,
                Height = height,
                LocalBounds = sourceSprite.bounds,
                WorldBounds = renderer.bounds
            };
            sources.Add(source);
            if (!hasWorldBounds)
            {
                worldBounds = source.WorldBounds;
                hasWorldBounds = true;
            }
            else
            {
                worldBounds.Encapsulate(source.WorldBounds);
            }

            var lossyScale = renderer.transform.lossyScale;
            var worldScaleX = Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f);
            var worldScaleY = Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f);
            pixelsPerWorldUnit = Mathf.Max(
                pixelsPerWorldUnit,
                sourceSprite.pixelsPerUnit / Mathf.Min(worldScaleX, worldScaleY));
        }

        if (!hasWorldBounds
            || sources.Count != cluster.Members.Count
            || worldBounds.size.x <= 0.0001f
            || worldBounds.size.y <= 0.0001f)
        {
            return false;
        }

        var largestPixelDimension = Mathf.Max(
            worldBounds.size.x * pixelsPerWorldUnit,
            worldBounds.size.y * pixelsPerWorldUnit);
        if (largestPixelDimension > LooseClusterShadowMaxTextureSize)
        {
            pixelsPerWorldUnit *= LooseClusterShadowMaxTextureSize / largestPixelDimension;
        }

        var textureWidth = Mathf.Max(1, Mathf.CeilToInt(worldBounds.size.x * pixelsPerWorldUnit));
        var textureHeight = Mathf.Max(1, Mathf.CeilToInt(worldBounds.size.y * pixelsPerWorldUnit));
        var rasterMinX = worldBounds.center.x
                         - textureWidth / (pixelsPerWorldUnit * 2f);
        var rasterMinY = worldBounds.center.y
                         - textureHeight / (pixelsPerWorldUnit * 2f);
        var outputPixels = new Color32[textureWidth * textureHeight];
        for (var sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
        {
            var source = sources[sourceIndex];
            var minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    (source.WorldBounds.min.x - rasterMinX) * pixelsPerWorldUnit),
                0,
                textureWidth - 1);
            var maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    (source.WorldBounds.max.x - rasterMinX) * pixelsPerWorldUnit),
                1,
                textureWidth);
            var minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    (source.WorldBounds.min.y - rasterMinY) * pixelsPerWorldUnit),
                0,
                textureHeight - 1);
            var maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    (source.WorldBounds.max.y - rasterMinY) * pixelsPerWorldUnit),
                1,
                textureHeight);
            for (var y = minY; y < maxY; y++)
            {
                var worldY = rasterMinY + (y + 0.5f) / pixelsPerWorldUnit;
                for (var x = minX; x < maxX; x++)
                {
                    var worldX = rasterMinX + (x + 0.5f) / pixelsPerWorldUnit;
                    var local = source.Renderer.transform.InverseTransformPoint(
                        new Vector3(worldX, worldY, source.Renderer.transform.position.z));
                    var normalizedX = Mathf.InverseLerp(
                        source.LocalBounds.min.x,
                        source.LocalBounds.max.x,
                        local.x);
                    var normalizedY = Mathf.InverseLerp(
                        source.LocalBounds.min.y,
                        source.LocalBounds.max.y,
                        local.y);
                    if (normalizedX < 0f
                        || normalizedX > 1f
                        || normalizedY < 0f
                        || normalizedY > 1f)
                    {
                        continue;
                    }

                    var sourceX = Mathf.Clamp(
                        Mathf.FloorToInt(normalizedX * source.Width),
                        0,
                        source.Width - 1);
                    var sourceY = Mathf.Clamp(
                        Mathf.FloorToInt(normalizedY * source.Height),
                        0,
                        source.Height - 1);
                    var alpha = source.Pixels[sourceY * source.Width + sourceX].a;
                    var outputIndex = y * textureWidth + x;
                    if (alpha > outputPixels[outputIndex].a)
                    {
                        outputPixels[outputIndex] = new Color32(255, 255, 255, alpha);
                    }
                }
            }
        }

        texture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false,
            true)
        {
            name = "Loose Piece Cluster Shadow Mask (Runtime)",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels32(outputPixels);
        texture.Apply(false, true);
        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            pixelsPerWorldUnit,
            0,
            SpriteMeshType.FullRect);
        sprite.name = "Loose Piece Cluster Shadow Mask (Runtime)";
        worldCenter = new Vector3(
            worldBounds.center.x,
            worldBounds.center.y,
            cluster.Members[0].PieceRenderer.transform.position.z);
        return true;
    }

    private void ApplyClusterShadowPropertyBlock(SpriteRenderer renderer)
    {
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        if (_pieceShadowPropertyBlock == null)
        {
            _pieceShadowPropertyBlock = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(_pieceShadowPropertyBlock);
        _pieceShadowPropertyBlock.SetFloat(
            SpritePixelsPerUnitId,
            Mathf.Max(1f, renderer.sprite.pixelsPerUnit));
        renderer.SetPropertyBlock(_pieceShadowPropertyBlock);
        _pieceShadowPropertyBlock.Clear();
    }

    private void UpdateLooseClusterShadows()
    {
        for (var i = 0; i < _loosePieceClusters.Count; i++)
        {
            var cluster = _loosePieceClusters[i];
            if (cluster == null
                || cluster.ShadowRenderer == null
                || (_isHintPieceShaking
                    && cluster == _hintedCluster
                    && !_activeDragMembers.Any(cluster.Members.Contains)))
            {
                continue;
            }

            UpdateLooseClusterShadowTransform(cluster);
        }
    }

    private static void UpdateLooseClusterShadowTransform(LoosePieceCluster cluster)
    {
        if (cluster?.ShadowRenderer == null || cluster.Members.Count < 2)
        {
            return;
        }

        var hasBounds = false;
        var worldBounds = default(Bounds);
        var sortingOrder = int.MaxValue;
        var sortingLayerId = 0;
        for (var i = 0; i < cluster.Members.Count; i++)
        {
            var renderer = cluster.Members[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                worldBounds = renderer.bounds;
                sortingLayerId = renderer.sortingLayerID;
                hasBounds = true;
            }
            else
            {
                worldBounds.Encapsulate(renderer.bounds);
            }

            sortingOrder = Mathf.Min(sortingOrder, renderer.sortingOrder);
        }

        if (!hasBounds)
        {
            return;
        }

        var position = cluster.ShadowRenderer.transform.position;
        position.x = worldBounds.center.x;
        position.y = worldBounds.center.y;
        cluster.ShadowRenderer.transform.position = position;
        cluster.ShadowRenderer.sortingLayerID = sortingLayerId;
        cluster.ShadowRenderer.sortingOrder = sortingOrder - 1;
    }

    private static void DestroyLooseClusterShadow(LoosePieceCluster cluster)
    {
        if (cluster == null)
        {
            return;
        }

        if (cluster.ShadowRoot != null)
        {
            Destroy(cluster.ShadowRoot);
        }
        if (cluster.ShadowSprite != null)
        {
            Destroy(cluster.ShadowSprite);
        }
        if (cluster.ShadowTexture != null)
        {
            Destroy(cluster.ShadowTexture);
        }

        cluster.ShadowRoot = null;
        cluster.ShadowRenderer = null;
        cluster.ShadowSprite = null;
        cluster.ShadowTexture = null;
    }

    private static Collider2D CreateGrooveOverlapProbe(
        Sprite sprite,
        Transform parent,
        string objectName)
    {
        var probeObject = new GameObject(objectName);
        probeObject.transform.SetParent(parent, false);
        return CreateSpriteOverlapCollider(probeObject, sprite);
    }

    private static Collider2D CreateSpriteOverlapCollider(GameObject host, Sprite sprite)
    {
        if (host == null || sprite == null)
        {
            return null;
        }

        var paths = new List<List<Vector2>>();
        var shapeCount = sprite.GetPhysicsShapeCount();
        for (var shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
        {
            var path = new List<Vector2>();
            sprite.GetPhysicsShape(shapeIndex, path);
            if (path.Count >= 3)
            {
                paths.Add(path);
            }
        }

        if (paths.Count > 0)
        {
            var polygonCollider = host.AddComponent<PolygonCollider2D>();
            polygonCollider.isTrigger = true;
            polygonCollider.pathCount = paths.Count;
            for (var pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                polygonCollider.SetPath(pathIndex, paths[pathIndex]);
            }

            return polygonCollider;
        }

        var boxCollider = host.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.offset = sprite.bounds.center;
        boxCollider.size = sprite.bounds.size;
        return boxCollider;
    }

    private void UpdateGrooveGroupVisibility(int activeGroupIndex)
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            var isActiveGroup = groupIndex == activeGroupIndex;
            var isCompletedGroup = groupIndex < activeGroupIndex;
            for (var i = 0; i < group.Count; i++)
            {
                var grooveImage = group[i];
                if (grooveImage == null)
                {
                    continue;
                }

                ApplyPlacedPieceImageShadow(grooveImage);

                if (isCompletedGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    SetImageAlpha(grooveImage, 1f);
                }
                else if (isActiveGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    var isPlaced = IsGroovePersistedAsPlaced(grooveImage);
                    SetImageAlpha(grooveImage, isPlaced ? 1f : 0f);
                }
                else
                {
                    grooveImage.gameObject.SetActive(false);
                }
            }
        }
    }

    private void ClearCurrentDraggableGroup()
    {
        EndTrayScroll();
        StopLoosePieceReminderShake();
        StopTrayPieceReflow();
        ClearPieceHint();
        _drag.DraggingPiece = null;
        ClearLoosePieceClusters();
        _drag.CurrentGroupDraggables.Clear();
        ClearActiveGroupOutline();

        var root = GameObject.Find(DraggableGroupRootObjectName);
        if (root != null)
        {
            Destroy(root);
        }
    }

    private void TryRefreshActiveGroupOutline(
        int groupIndex,
        bool startHidden = false,
        bool ignoreTutorialBlock = false)
    {
        if (_isTutorialPending || (IsTutorialBlockingOutline && !ignoreTutorialBlock))
        {
            ClearActiveGroupOutline();
            return;
        }

        try
        {
            RefreshActiveGroupOutline(groupIndex, startHidden);
        }
        catch (Exception exception)
        {
            ClearActiveGroupOutline();
            Debug.LogWarning($"GameScene: failed to create active group outline. {exception.Message}");
        }
    }

    private void RefreshActiveGroupOutline(int groupIndex, bool startHidden)
    {
        ClearActiveGroupOutline();
        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var grooveGroup = _board.GrooveImagesByGroup[groupIndex];
        if (grooveGroup == null || grooveGroup.Count == 0)
        {
            return;
        }

        if (!TryGetNumberedGroup(grooveGroup[0], out var groupNumber))
        {
            return;
        }

        var bagId = GameManager.GetBagId();
        var levelResourcePath = _isLevelOutlineEnabled
            ? GameDefine.FormatPuzzleLevelOutlineResourcesPath(bagId, groupNumber)
            : GameDefine.FormatPuzzleOutlineResourcesPath(bagId, groupNumber);
        var levelOutlineSprite = Resources.Load<Sprite>(levelResourcePath);
        if (levelOutlineSprite == null && _isLevelOutlineEnabled)
        {
            Debug.LogWarning(
                $"GameScene: full level outline is missing at Resources/{levelResourcePath}; " +
                "falling back to the connection outline.");
            levelResourcePath = GameDefine.FormatPuzzleOutlineResourcesPath(bagId, groupNumber);
            levelOutlineSprite = Resources.Load<Sprite>(levelResourcePath);
        }

        Sprite stickerOutlineSprite = null;
        var stickerResourcePath = string.Empty;
        if (_isStickerOutlineEnabled)
        {
            stickerResourcePath = GameDefine.FormatPuzzleStickerOutlineResourcesPath(
                bagId,
                groupNumber);
            stickerOutlineSprite = Resources.Load<Sprite>(stickerResourcePath);
            if (stickerOutlineSprite == null)
            {
                Debug.LogWarning(
                    $"GameScene: sticker outlines are missing at Resources/{stickerResourcePath}.");
            }
        }

        if (levelOutlineSprite == null && stickerOutlineSprite == null)
        {
            Debug.LogWarning(
                $"GameScene: baked puzzle outline is missing at Resources/{levelResourcePath}. " +
                "Run Puffies/Bake CardBag Outlines in the Unity Editor.");
            return;
        }

        var outlineObject = new GameObject(
            ActiveGroupOutlineRootObjectName,
            typeof(RectTransform),
            typeof(CanvasGroup));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(_board.GameBoardImage.rectTransform, false);
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.pivot = new Vector2(0.5f, 0.5f);
        outlineRect.anchoredPosition = Vector2.zero;
        outlineRect.offsetMin = Vector2.zero;
        outlineRect.offsetMax = Vector2.zero;
        outlineRect.localScale = Vector3.one;
        outlineObject.GetComponent<CanvasGroup>().alpha = startHidden ? 0f : 1f;

        CreateOutlineLayer(
            outlineRect,
            LevelOutlineLayerObjectName,
            levelOutlineSprite);
        CreateOutlineLayer(
            outlineRect,
            StickerOutlineLayerObjectName,
            stickerOutlineSprite);
    }

    private void CreateOutlineLayer(Transform parent, string objectName, Sprite sprite)
    {
        if (parent == null || sprite == null)
        {
            return;
        }

        var outlineObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(parent, false);
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.pivot = new Vector2(0.5f, 0.5f);
        outlineRect.anchoredPosition = Vector2.zero;
        outlineRect.offsetMin = Vector2.zero;
        outlineRect.offsetMax = Vector2.zero;
        outlineRect.localScale = Vector3.one;

        var outlineImage = outlineObject.GetComponent<Image>();
        outlineImage.sprite = sprite;
        var tintMaterial = GetOrCreatePuzzleOutlineTintMaterial();
        if (tintMaterial != null)
        {
            outlineImage.material = tintMaterial;
            outlineImage.color = StandardPuzzleOutlineColor;
        }
        else
        {
            outlineImage.color = Color.white;
        }
        outlineImage.raycastTarget = false;
        outlineImage.maskable = false;
        outlineImage.preserveAspect = false;
    }

    private Material GetOrCreatePuzzleOutlineTintMaterial()
    {
        if (_runtimePuzzleOutlineTintMaterial != null)
        {
            return _runtimePuzzleOutlineTintMaterial;
        }

        var shader = Resources.Load<Shader>(PuzzleOutlineTintShaderResourcesPath);
        if (shader == null)
        {
            if (!_didWarnMissingPuzzleOutlineTintShader)
            {
                _didWarnMissingPuzzleOutlineTintShader = true;
                Debug.LogWarning(
                    $"GameScene: puzzle outline tint shader is missing at " +
                    $"Resources/{PuzzleOutlineTintShaderResourcesPath}.shader; " +
                    "using baked outline colors.");
            }

            return null;
        }

        _runtimePuzzleOutlineTintMaterial = new Material(shader)
        {
            name = PuzzleOutlineTintMaterialName
        };
        return _runtimePuzzleOutlineTintMaterial;
    }

    private void DestroyRuntimePuzzleOutlineTintMaterial()
    {
        if (_runtimePuzzleOutlineTintMaterial == null)
        {
            return;
        }

        Destroy(_runtimePuzzleOutlineTintMaterial);
        _runtimePuzzleOutlineTintMaterial = null;
    }

    private Material GetOrCreatePiecePlacementShineMaterial()
    {
        if (_runtimePiecePlacementShineMaterial != null)
        {
            return _runtimePiecePlacementShineMaterial;
        }

        var shader = Resources.Load<Shader>(PiecePlacementShineShaderResourcesPath);
        if (shader == null)
        {
            if (!_didWarnMissingPiecePlacementShineShader)
            {
                _didWarnMissingPiecePlacementShineShader = true;
                Debug.LogWarning(
                    $"GameScene: piece placement shine shader is missing at "
                    + $"Resources/{PiecePlacementShineShaderResourcesPath}.shader; "
                    + "current-piece shine skipped.");
            }

            return null;
        }

        _runtimePiecePlacementShineMaterial = new Material(shader)
        {
            name = PiecePlacementShineMaterialName
        };
        return _runtimePiecePlacementShineMaterial;
    }

    private void DestroyRuntimePiecePlacementShineMaterial()
    {
        for (var i = 0; i < _activePiecePlacementShineMaterials.Count; i++)
        {
            if (_activePiecePlacementShineMaterials[i] != null)
            {
                Destroy(_activePiecePlacementShineMaterials[i]);
            }
        }

        _activePiecePlacementShineMaterials.Clear();
        if (_runtimePiecePlacementShineMaterial == null)
        {
            return;
        }

        Destroy(_runtimePiecePlacementShineMaterial);
        _runtimePiecePlacementShineMaterial = null;
    }

    private void DestroyPiecePlacementShineMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        _activePiecePlacementShineMaterials.Remove(material);
        Destroy(material);
    }

    private bool EnsurePiecePlacementLightResources()
    {
        if (_piecePlacementLightSprites.Count == PiecePlacementLightSpriteCount
            && _piecePlacementLightMaterial != null)
        {
            return true;
        }

        DestroyPiecePlacementLightSprites();
        for (var i = 1; i <= PiecePlacementLightSpriteCount; i++)
        {
            var sprite = GameCommonUtility.LoadSpriteByPath(
                $"{PiecePlacementLightPathPrefix}{i}{GameDefine.ImageExtPng}",
                PixelsPerUnit);
            if (sprite == null)
            {
                DestroyPiecePlacementLightSprites();
                WarnMissingPiecePlacementLightResources();
                return false;
            }

            _piecePlacementLightSprites.Add(sprite);
        }

        _piecePlacementLightMaterial = Resources.Load<Material>(
            PiecePlacementLightMaterialResourcesPath);
        if (_piecePlacementLightMaterial == null)
        {
            DestroyPiecePlacementLightSprites();
            WarnMissingPiecePlacementLightResources();
            return false;
        }

        return true;
    }

    private void WarnMissingPiecePlacementLightResources()
    {
        if (_didWarnMissingPiecePlacementLightResources)
        {
            return;
        }

        _didWarnMissingPiecePlacementLightResources = true;
        Debug.LogWarning(
            "GameScene: PieceLight1-4.png or PackHighlightAdditive.mat is missing; "
            + "placement light propagation skipped.");
    }

    private void DestroyPiecePlacementLightSprites()
    {
        for (var i = 0; i < _piecePlacementLightSprites.Count; i++)
        {
            var sprite = _piecePlacementLightSprites[i];
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
        }

        _piecePlacementLightSprites.Clear();
        _piecePlacementLightMaterial = null;
        if (_pieceSpriteLightMaterial != null)
        {
            Destroy(_pieceSpriteLightMaterial);
            _pieceSpriteLightMaterial = null;
        }
    }

    private void AddAmbientLightsForCompletedGroups(int activeGroupIndex)
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < activeGroupIndex; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var i = 0; i < group.Count; i++)
            {
                AddAmbientBoardPieceLights(group[i]);
            }
        }
    }

    private void AddAmbientBoardPieceLights(Image sourceImage)
    {
        if (sourceImage == null
            || sourceImage.sprite == null
            || !EnsurePiecePlacementLightResources())
        {
            return;
        }

        var pieceNumber = GetPieceNumberFromImage(sourceImage);
        if (pieceNumber == int.MaxValue)
        {
            return;
        }

        RemoveActivePieceLight(pieceNumber);
        var state = GetOrCreatePieceLightState(pieceNumber, sourceImage);
        var maskObject = CreatePiecePlacementLightMask(sourceImage);
        if (maskObject == null)
        {
            return;
        }

        maskObject.name = $"{sourceImage.gameObject.name}_AmbientLights";
        var maskRect = maskObject.GetComponent<RectTransform>();
        var sprite = _piecePlacementLightSprites[state.SpriteIndex];
        var lightObject = new GameObject(
            "PieceLight",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        lightObject.layer = sourceImage.gameObject.layer;
        var lightRect = lightObject.GetComponent<RectTransform>();
        lightRect.SetParent(maskRect, false);
        lightRect.anchorMin = lightRect.anchorMax = new Vector2(0.5f, 0.5f);
        lightRect.pivot = new Vector2(0.5f, 0.5f);
        lightRect.sizeDelta = sprite.rect.size;
        lightRect.anchoredPosition = new Vector2(
            sourceImage.rectTransform.rect.width * state.NormalizedPosition.x,
            sourceImage.rectTransform.rect.height * state.NormalizedPosition.y);
        lightRect.localRotation = Quaternion.Euler(0f, 0f, state.Rotation);
        lightRect.localScale = new Vector3(state.Scale.x, state.Scale.y, 1f);

        var lightImage = lightObject.GetComponent<Image>();
        lightImage.sprite = sprite;
        lightImage.material = _piecePlacementLightMaterial;
        lightImage.color = Color.white;
        lightImage.raycastTarget = false;
        lightImage.maskable = true;
        var deformer = lightObject.AddComponent<PieceLightDeformEffect>();
        _ambientPieceLights.Add(new AmbientPieceLightFx
        {
            PieceNumber = pieceNumber,
            Root = maskObject,
            Image = lightImage,
            Transform = lightRect,
            Deformer = deformer
        });
    }

    private void AddAmbientDraggablePieceLights(
        SpriteRenderer sourceRenderer,
        Image sourceImage)
    {
        if (sourceRenderer == null
            || sourceRenderer.sprite == null
            || !EnsurePiecePlacementLightResources()
            || !EnsurePieceSpriteLightMaterial())
        {
            return;
        }

        var pieceNumber = GetPieceNumberFromImage(sourceImage);
        if (pieceNumber == int.MaxValue)
        {
            return;
        }

        RemoveActivePieceLight(pieceNumber);
        var state = GetOrCreatePieceLightState(pieceNumber, sourceImage);
        var sourceBounds = sourceRenderer.sprite.bounds;
        var maskObject = new GameObject("AmbientPieceLightMask");
        maskObject.transform.SetParent(sourceRenderer.transform, false);
        var spriteMask = maskObject.AddComponent<SpriteMask>();
        spriteMask.sprite = sourceRenderer.sprite;
        spriteMask.alphaCutoff = 0.02f;
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = sourceRenderer.sortingLayerID;
        spriteMask.backSortingLayerID = sourceRenderer.sortingLayerID;
        spriteMask.frontSortingOrder = sourceRenderer.sortingOrder + 2;
        spriteMask.backSortingOrder = sourceRenderer.sortingOrder;
        var sprite = _piecePlacementLightSprites[state.SpriteIndex];
        var lightObject = new GameObject("PieceLight");
        lightObject.transform.SetParent(sourceRenderer.transform, false);
        lightObject.transform.localPosition = new Vector3(
            sourceBounds.center.x + sourceBounds.size.x * state.NormalizedPosition.x,
            sourceBounds.center.y + sourceBounds.size.y * state.NormalizedPosition.y,
            -0.01f);
        lightObject.transform.localRotation = Quaternion.Euler(0f, 0f, state.Rotation);
        lightObject.transform.localScale = new Vector3(state.Scale.x, state.Scale.y, 1f);

        var lightRenderer = lightObject.AddComponent<SpriteRenderer>();
        lightRenderer.sprite = sprite;
        lightRenderer.sharedMaterial = _pieceSpriteLightMaterial;
        lightRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        lightRenderer.sortingOrder = sourceRenderer.sortingOrder + 1;
        lightRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        lightRenderer.color = Color.white;
        _ambientPieceLights.Add(new AmbientPieceLightFx
        {
            PieceNumber = pieceNumber,
            Root = sourceRenderer.gameObject,
            Renderer = lightRenderer,
            SourceRenderer = sourceRenderer,
            SpriteMask = spriteMask,
            Transform = lightObject.transform
        });
    }

    private void EnsureDraggablePieceLights()
    {
        if (_isEntranceAnimating || _isGroupTransitionAnimating)
        {
            return;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            EnsureDraggablePieceLight(_drag.CurrentGroupDraggables[i]);
        }
    }

    private void EnsureDraggablePieceLight(DraggablePieceState state)
    {
        var renderer = state?.PieceRenderer;
        if (state == null
            || state.IsPlaced
            || renderer == null
            || !renderer.gameObject.activeInHierarchy
            || HasAmbientDraggablePieceLight(renderer))
        {
            return;
        }

        AddAmbientDraggablePieceLights(renderer, state.GrooveImage);
    }

    private bool HasAmbientDraggablePieceLight(SpriteRenderer sourceRenderer)
    {
        for (var i = 0; i < _ambientPieceLights.Count; i++)
        {
            var effect = _ambientPieceLights[i];
            if (effect != null
                && effect.SourceRenderer == sourceRenderer
                && effect.Renderer != null
                && effect.Transform != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool EnsurePieceSpriteLightMaterial()
    {
        if (_pieceSpriteLightMaterial != null)
        {
            return true;
        }

        var shader = Resources.Load<Shader>(
            PiecePlacementSpriteLightShaderResourcesPath);
        if (shader == null)
        {
            WarnMissingPiecePlacementLightResources();
            return false;
        }

        _pieceSpriteLightMaterial = new Material(shader)
        {
            name = "PuzzlePieceLightAdditive (Runtime)"
        };
        return true;
    }

    private PieceLightState GetOrCreatePieceLightState(int pieceNumber, Image sourceImage)
    {
        if (_pieceLightStates.TryGetValue(pieceNumber, out var state))
        {
            return state;
        }

        var pieceSize = sourceImage != null
            ? sourceImage.rectTransform.rect.size
            : Vector2.one;
        pieceSize = new Vector2(
            Mathf.Max(1f, Mathf.Abs(pieceSize.x)),
            Mathf.Max(1f, Mathf.Abs(pieceSize.y)));
        var random = new System.Random(pieceNumber * 486187739);
        var referenceSize = Mathf.Sqrt(pieceSize.x * pieceSize.y);
        var pieceAspect = pieceSize.x / pieceSize.y;
        var widePieceFactor = Mathf.InverseLerp(0.72f, 2.1f, pieceAspect);
        var lengthVariation = RandomRange(random, 0.92f, 1.24f)
                              * Mathf.Lerp(1f, 1.22f, widePieceFactor);
        var maximumWidthRatio = Mathf.Lerp(
            0.54f,
            PieceLightMaximumWidthRatio,
            widePieceFactor);
        var targetWidth = Mathf.Min(
            Mathf.Clamp(referenceSize * 0.22f * lengthVariation, 18f, 108f),
            pieceSize.x * maximumWidthRatio);
        var spriteIndex = SelectPieceLightSpriteIndex(pieceNumber);
        var lightSprite = _piecePlacementLightSprites[spriteIndex];
        var maximumHeight = Mathf.Max(
            8f,
            Mathf.Min(pieceSize.y * 0.34f, 42f));
        var uniformScale = Mathf.Min(
            Mathf.Max(10f, targetWidth) / Mathf.Max(1f, lightSprite.rect.width),
            maximumHeight / Mathf.Max(1f, lightSprite.rect.height));
        var normalizedPosition = CalculateInteriorPieceLightPosition(
            sourceImage != null ? sourceImage.sprite : null);
        state = new PieceLightState
        {
            SpriteIndex = spriteIndex,
            NormalizedPosition = normalizedPosition,
            Rotation = GetPieceLightRotation(spriteIndex, random),
            Scale = Vector2.one * uniformScale
        };
        _pieceLightStates[pieceNumber] = state;
        return state;
    }

    private int SelectPieceLightSpriteIndex(int pieceNumber)
    {
        if (_piecePlacementLightSprites.Count <= 0)
        {
            return 0;
        }

        var preferredIndex = Math.Abs(pieceNumber % _piecePlacementLightSprites.Count);
        for (var offset = 0; offset < _piecePlacementLightSprites.Count; offset++)
        {
            var index = (preferredIndex + offset) % _piecePlacementLightSprites.Count;
            if (_piecePlacementLightSprites[index] != null)
            {
                return index;
            }
        }

        return 0;
    }

    private static float GetPieceLightRotation(int spriteIndex, System.Random random)
    {
        switch (spriteIndex)
        {
            case 0:
                return RandomRange(random, -8f, 8f);
            case 1:
                return RandomRange(random, -18f, -6f);
            case 2:
                return RandomRange(random, -12f, 2f);
            default:
                return RandomRange(random, -4f, 8f);
        }
    }

    private static Vector2 CalculateInteriorPieceLightPosition(Sprite sprite)
    {
        var fallback = Vector2.zero;
        if (sprite == null
            || sprite.bounds.size.x <= 0.001f
            || sprite.bounds.size.y <= 0.001f)
        {
            return fallback;
        }

        var normalizedPaths = new List<List<Vector2>>();
        var path = new List<Vector2>();
        var shapeCount = sprite.GetPhysicsShapeCount();
        for (var shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
        {
            path.Clear();
            sprite.GetPhysicsShape(shapeIndex, path);
            if (path.Count < 3)
            {
                continue;
            }

            var normalizedPath = new List<Vector2>(path.Count);
            for (var pointIndex = 0; pointIndex < path.Count; pointIndex++)
            {
                var point = path[pointIndex];
                normalizedPath.Add(new Vector2(
                    (point.x - sprite.bounds.center.x) / sprite.bounds.size.x,
                    (point.y - sprite.bounds.center.y) / sprite.bounds.size.y));
            }

            normalizedPaths.Add(normalizedPath);
        }

        if (normalizedPaths.Count == 0)
        {
            return fallback;
        }

        var candidates = new List<Vector3>();
        var maximumClearance = 0f;
        var deepestPosition = fallback;
        for (var y = 0; y < PieceLightInteriorGridResolution; y++)
        {
            for (var x = 0; x < PieceLightInteriorGridResolution; x++)
            {
                var candidate = new Vector2(
                    (x + 0.5f) / PieceLightInteriorGridResolution - 0.5f,
                    (y + 0.5f) / PieceLightInteriorGridResolution - 0.5f);
                if (!TryGetPieceInteriorClearance(
                        normalizedPaths,
                        candidate,
                        out var clearance))
                {
                    continue;
                }

                candidates.Add(new Vector3(candidate.x, candidate.y, clearance));
                if (clearance > maximumClearance)
                {
                    maximumClearance = clearance;
                    deepestPosition = candidate;
                }
            }
        }

        if (candidates.Count == 0)
        {
            return fallback;
        }

        var minimumPreferredClearance = maximumClearance * PieceLightPreferredClearanceRatio;
        var preferredPosition = new Vector2(-0.18f, 0.18f);
        var selectedPosition = deepestPosition;
        var selectedDistance = float.PositiveInfinity;
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate.z < minimumPreferredClearance)
            {
                continue;
            }

            var position = new Vector2(candidate.x, candidate.y);
            var distance = (position - preferredPosition).sqrMagnitude;
            if (distance < selectedDistance)
            {
                selectedDistance = distance;
                selectedPosition = position;
            }
        }

        return selectedPosition;
    }

    private static bool TryGetPieceInteriorClearance(
        IReadOnlyList<List<Vector2>> paths,
        Vector2 point,
        out float clearance)
    {
        clearance = 0f;
        var isInside = false;
        for (var pathIndex = 0; pathIndex < paths.Count; pathIndex++)
        {
            var path = paths[pathIndex];
            if (!IsPointInsidePolygon(point, path))
            {
                continue;
            }

            isInside = true;
            var pathClearance = float.PositiveInfinity;
            for (var pointIndex = 0; pointIndex < path.Count; pointIndex++)
            {
                var start = path[pointIndex];
                var end = path[(pointIndex + 1) % path.Count];
                pathClearance = Mathf.Min(
                    pathClearance,
                    DistanceToSegment(point, start, end));
            }

            clearance = Mathf.Max(clearance, pathClearance);
        }

        return isInside;
    }

    private static bool IsPointInsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
    {
        var inside = false;
        for (int current = 0, previous = polygon.Count - 1;
             current < polygon.Count;
             previous = current++)
        {
            var currentPoint = polygon[current];
            var previousPoint = polygon[previous];
            if ((currentPoint.y > point.y) == (previousPoint.y > point.y))
            {
                continue;
            }

            var edgeX = (previousPoint.x - currentPoint.x)
                        * (point.y - currentPoint.y)
                        / (previousPoint.y - currentPoint.y)
                        + currentPoint.x;
            if (point.x < edgeX)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        var segment = end - start;
        var lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= 0.000001f)
        {
            return Vector2.Distance(point, start);
        }

        var projection = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * projection);
    }

    private void UpdatePieceLightSorting()
    {
        for (var i = _ambientPieceLights.Count - 1; i >= 0; i--)
        {
            var effect = _ambientPieceLights[i];
            if (effect == null
                || effect.Root == null
                || effect.Transform == null
                || !effect.Root.activeInHierarchy)
            {
                _ambientPieceLights.RemoveAt(i);
                continue;
            }

            if (effect.Renderer != null)
            {
                if (effect.SourceRenderer != null)
                {
                    var sortingOrder = effect.SourceRenderer.sortingOrder + 1;
                    effect.Renderer.sortingLayerID = effect.SourceRenderer.sortingLayerID;
                    effect.Renderer.sortingOrder = sortingOrder;
                    if (effect.SpriteMask != null)
                    {
                        effect.SpriteMask.frontSortingLayerID =
                            effect.SourceRenderer.sortingLayerID;
                        effect.SpriteMask.backSortingLayerID =
                            effect.SourceRenderer.sortingLayerID;
                        effect.SpriteMask.frontSortingOrder = sortingOrder + 1;
                        effect.SpriteMask.backSortingOrder = sortingOrder - 1;
                    }
                }
            }
        }
    }

    private void RemoveActivePieceLight(int pieceNumber)
    {
        for (var i = _ambientPieceLights.Count - 1; i >= 0; i--)
        {
            var effect = _ambientPieceLights[i];
            if (effect == null || effect.PieceNumber != pieceNumber)
            {
                continue;
            }

            if (effect.Root != null
                && effect.Root.name.EndsWith("_AmbientLights", StringComparison.Ordinal))
            {
                Destroy(effect.Root);
            }
            else if (effect.Transform != null)
            {
                Destroy(effect.Transform.gameObject);
            }

            _ambientPieceLights.RemoveAt(i);
        }
    }

    private void RemoveAmbientLightsForRoot(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        for (var i = _ambientPieceLights.Count - 1; i >= 0; i--)
        {
            if (_ambientPieceLights[i]?.Root == root)
            {
                _ambientPieceLights.RemoveAt(i);
            }
        }
    }

    private void ClearAmbientPieceLights()
    {
        var roots = new HashSet<GameObject>();
        for (var i = 0; i < _ambientPieceLights.Count; i++)
        {
            var root = _ambientPieceLights[i]?.Root;
            if (root != null)
            {
                roots.Add(root);
            }
        }

        foreach (var root in roots)
        {
            if (root != null && root.name.EndsWith("_AmbientLights", StringComparison.Ordinal))
            {
                Destroy(root);
            }
        }

        _ambientPieceLights.Clear();
    }

    private void ClearActiveGroupOutline()
    {
        if (_activeGroupOutlineFadeCoroutine != null)
        {
            StopCoroutine(_activeGroupOutlineFadeCoroutine);
            _activeGroupOutlineFadeCoroutine = null;
        }

        var root = GameObject.Find(ActiveGroupOutlineRootObjectName);
        if (root != null)
        {
            root.SetActive(false);
            Destroy(root);
        }
    }

    private void FadeInActiveGroupOutline()
    {
        var root = GameObject.Find(ActiveGroupOutlineRootObjectName);
        if (root == null)
        {
            return;
        }

        var canvasGroup = root.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = root.AddComponent<CanvasGroup>();
        }

        if (_activeGroupOutlineFadeCoroutine != null)
        {
            StopCoroutine(_activeGroupOutlineFadeCoroutine);
        }

        _activeGroupOutlineFadeCoroutine = StartCoroutine(
            PlayActiveGroupOutlineFade(canvasGroup));
    }

    private IEnumerator PlayActiveGroupOutlineFade(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        var elapsed = 0f;
        while (elapsed < ActiveGroupOutlineFadeDuration && canvasGroup != null)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / ActiveGroupOutlineFadeDuration));
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        _activeGroupOutlineFadeCoroutine = null;
    }

    private void OnPointerEnd(Vector2 screenPosition)
    {
        EndDragging(screenPosition);
    }

    private void TryBeginDrag(Vector2 screenPosition)
    {
        if (_isGameFinished || IsPiecePlacementDragBlocked || _isTrayPieceReflowAnimating)
        {
            return;
        }

        if (_drag.DraggingPiece != null)
        {
            return;
        }

        var state = FindDraggablePieceAt(screenPosition);
        if (state == null)
        {
            TryBeginTrayScroll(screenPosition);
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_PiecePickup.mp3");
        StopLoosePieceReminderShake();
        if ((_tutorialStage == TutorialStage.StrongPlacement && state == _tutorialPiece)
            || _tutorialStage == TutorialStage.TwoPiecePractice)
        {
            HideTutorialFocusPresentation();
        }

        var world = ToGameplayWorld(screenPosition);
        _drag.DraggingPiece = state;
        PopulateActiveDragMembers(state);
        if (state.IsOnTray)
        {
            CaptureTrayPickupLayout(state);
        }
        RestoreHintedPieceRotationsIfDragging();
        _drag.DragOffset = state.PieceRenderer.transform.position - world;
        for (var i = 0; i < _activeDragMembers.Count && i < _activeDragStartPositions.Count; i++)
        {
            var renderer = _activeDragMembers[i]?.PieceRenderer;
            if (renderer != null)
            {
                _activeDragStartPositions[i] = renderer.transform.position;
            }
        }
        for (var i = 0; i < _activeDragMembers.Count; i++)
        {
            var member = _activeDragMembers[i];
            var renderer = member?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            member.DragScale = CalculatePieceScaleOnBoard(
                member.GrooveImage,
                renderer);
            member.BoardScale = member.DragScale;
            if (member.IsOnTray)
            {
                member.TrayScale = CalculateTrayScaleForPiece(
                    renderer,
                    GetPieceTrayBounds(),
                    member.DragScale);
            }

            renderer.transform.localScale = member.DragScale;
            ApplyPieceRendererShadow(renderer, PieceShadowStyle.Initial);
            renderer.sortingOrder = PieceSortingOrder + 100 + i;
        }
        if (_looseClusterByPiece.TryGetValue(state, out var draggedCluster)
            && draggedCluster.Members.Count > 1)
        {
            SetLooseClusterPresentation(draggedCluster, PieceShadowStyle.Initial);
        }
        if (state.IsOnTray)
        {
            CompactFollowingTrayPieces(state);
        }
        if (state.IsOnTray && CountUnplacedTrayPieces() == 1)
        {
            SlidePieceTrayOutOfScreen();
        }
    }

    private void RefreshCursorForPointer(Vector2 screenPosition)
    {
        if (_drag.DraggingPiece != null || _isTrayScrolling)
        {
            GameCursorUtility.SetPieceDrag();
            return;
        }

        if (!_isGameFinished
            && !IsPiecePlacementDragBlocked
            && !_isTrayPieceReflowAnimating)
        {
            var hoveredPiece = FindDraggablePieceAt(screenPosition);
            if (hoveredPiece != null
                || (IsPointerInVisiblePieceTray(screenPosition)
                    && TryGetTrayScrollLimits(out _, out _)))
            {
                GameCursorUtility.SetPieceHover();
                return;
            }
        }

        GameCursorUtility.SetDefault();
    }

    private DraggablePieceState FindDraggablePieceAt(Vector2 screenPosition)
    {
        var world = ToGameplayWorld(screenPosition);
        for (var i = _drag.CurrentGroupDraggables.Count - 1; i >= 0; i--)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (_tutorialStage == TutorialStage.StrongPlacement && state != _tutorialPiece)
            {
                continue;
            }

            if (state != null
                && !state.IsPlaced
                && state.PieceRenderer != null
                && ContainsWorldXY(state.PieceRenderer.bounds, world))
            {
                return state;
            }
        }

        return null;
    }

    private void UpdateDragging(Vector2 screenPosition)
    {
        if (_isTrayScrolling)
        {
            UpdateTrayScroll(screenPosition);
            return;
        }

        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var world = ToGameplayWorld(screenPosition);
        var anchorRenderer = _drag.DraggingPiece.PieceRenderer;
        var anchorPosition = new Vector3(
            world.x + _drag.DragOffset.x,
            world.y + _drag.DragOffset.y,
            WorldGameplayDepth);
        var anchorStart = _activeDragStartPositions.Count > 0
            ? _activeDragStartPositions[0]
            : anchorRenderer.transform.position;
        var delta = anchorPosition - anchorStart;
        for (var i = 0; i < _activeDragMembers.Count; i++)
        {
            var renderer = _activeDragMembers[i]?.PieceRenderer;
            if (renderer == null || i >= _activeDragStartPositions.Count)
            {
                continue;
            }

            renderer.transform.position = _activeDragStartPositions[i] + delta;
        }

        ClampActiveDragMembersToTableBounds();
    }

    private bool TryBeginTrayScroll(Vector2 screenPosition)
    {
        if (!IsPointerInVisiblePieceTray(screenPosition)
            || !TryGetTrayScrollLimits(out var minDeltaX, out var maxDeltaX))
        {
            return false;
        }

        _trayScrollStates.Clear();
        _trayScrollStartPositions.Clear();
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null)
            {
                continue;
            }

            _trayScrollStates.Add(state);
            _trayScrollStartPositions.Add(state.PieceRenderer.transform.position);
        }

        if (_trayScrollStates.Count == 0)
        {
            return false;
        }

        _isTrayScrolling = true;
        _trayScrollStartWorldX = ToGameplayWorld(screenPosition).x;
        _trayScrollMinDeltaX = minDeltaX;
        _trayScrollMaxDeltaX = maxDeltaX;
        StopLoosePieceReminderShake();
        return true;
    }

    private void UpdateTrayScroll(Vector2 screenPosition)
    {
        var pointerWorldX = ToGameplayWorld(screenPosition).x;
        var deltaX = Mathf.Clamp(
            pointerWorldX - _trayScrollStartWorldX,
            _trayScrollMinDeltaX,
            _trayScrollMaxDeltaX);
        for (var i = 0; i < _trayScrollStates.Count; i++)
        {
            var state = _trayScrollStates[i];
            if (state?.PieceRenderer == null || state.IsPlaced || !state.IsOnTray)
            {
                continue;
            }

            var position = _trayScrollStartPositions[i];
            position.x += deltaX;
            state.PieceRenderer.transform.position = position;
            state.StartPosition = position;
        }
    }

    private void EndTrayScroll()
    {
        _isTrayScrolling = false;
        _trayScrollStates.Clear();
        _trayScrollStartPositions.Clear();
    }

    private void CaptureTrayPickupLayout(DraggablePieceState pickedState)
    {
        ClearTrayPickupLayoutSnapshot();
        if (pickedState == null || !pickedState.IsOnTray)
        {
            return;
        }

        _trayPickupRestorePiece = pickedState;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null)
            {
                continue;
            }

            _trayPickupRestoreStates.Add(state);
            _trayPickupRestorePositions.Add(state.PieceRenderer.transform.position);
        }
    }

    private void ClearTrayPickupLayoutSnapshot()
    {
        _trayPickupRestorePiece = null;
        _trayPickupRestoreStates.Clear();
        _trayPickupRestorePositions.Clear();
    }

    private bool TryRestoreTrayPickupLayout(
        DraggablePieceState pickedState,
        bool animate,
        bool animatePickedSeparately,
        out Vector3 pickedTarget)
    {
        pickedTarget = Vector3.zero;
        if (pickedState == null
            || pickedState != _trayPickupRestorePiece
            || _trayPickupRestoreStates.Count == 0
            || _trayPickupRestoreStates.Count != _trayPickupRestorePositions.Count)
        {
            return false;
        }

        ResetPieceTrayPosition(instant: true);
        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.gameObject.SetActive(true);
        }
        else if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.gameObject.SetActive(true);
            _board.PieceBgRenderer.enabled = true;
        }

        Canvas.ForceUpdateCanvases();
        StopTrayPieceReflow();
        var animatedStates = animate ? new List<DraggablePieceState>() : null;
        var animatedTargets = animate ? new List<Vector3>() : null;
        var foundPickedState = false;
        var trayBounds = GetPieceTrayBounds();
        for (var i = 0; i < _trayPickupRestoreStates.Count; i++)
        {
            var state = _trayPickupRestoreStates[i];
            if (state?.PieceRenderer == null || state.IsPlaced)
            {
                continue;
            }

            var target = _trayPickupRestorePositions[i];
            state.IsOnTray = true;
            state.StartPosition = target;
            state.TrayScale = CalculateTrayScaleForPiece(
                state.PieceRenderer,
                trayBounds,
                state.DragScale);
            var isPickedState = state == pickedState;
            if (isPickedState)
            {
                foundPickedState = true;
                pickedTarget = target;
            }

            if (isPickedState && animatePickedSeparately)
            {
                continue;
            }

            state.PieceRenderer.transform.localScale = state.TrayScale;
            if (!animate)
            {
                state.PieceRenderer.transform.position = target;
            }
            else if (Vector3.SqrMagnitude(
                         state.PieceRenderer.transform.position - target) > 0.000001f)
            {
                animatedStates.Add(state);
                animatedTargets.Add(target);
            }
        }

        StartTrayPieceReflow(animatedStates, animatedTargets);
        return foundPickedState;
    }

    private bool IsPointerInVisiblePieceTray(Vector2 screenPosition)
    {
        return !IsPieceTrayHidden()
               && TryGetPieceTrayDropScreenRect(out var trayScreenRect)
               && trayScreenRect.Contains(screenPosition);
    }

    private bool TryGetTrayScrollLimits(out float minDeltaX, out float maxDeltaX)
    {
        minDeltaX = 0f;
        maxDeltaX = 0f;
        if (IsPieceTrayHidden())
        {
            return false;
        }

        var trayBounds = GetPieceTrayBounds();
        var hasContentBounds = false;
        var contentBounds = default(Bounds);
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null)
            {
                continue;
            }

            if (!hasContentBounds)
            {
                contentBounds = state.PieceRenderer.bounds;
                hasContentBounds = true;
            }
            else
            {
                contentBounds.Encapsulate(state.PieceRenderer.bounds);
            }
        }

        if (!hasContentBounds || trayBounds.size.x <= DraggableLeftPadding * 2f)
        {
            return false;
        }

        var viewportMinX = trayBounds.min.x + DraggableLeftPadding;
        var viewportMaxX = trayBounds.max.x - DraggableLeftPadding;
        minDeltaX = Mathf.Min(0f, viewportMaxX - contentBounds.max.x);
        maxDeltaX = Mathf.Max(0f, viewportMinX - contentBounds.min.x);
        return minDeltaX < -TrayScrollBoundsEpsilon
               || maxDeltaX > TrayScrollBoundsEpsilon;
    }

    private void EndDragging(Vector2? releaseScreenPosition = null)
    {
        if (_isTrayScrolling)
        {
            EndTrayScroll();
            return;
        }

        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var state = _drag.DraggingPiece;
        var dragMembers = new List<DraggablePieceState>(_activeDragMembers);
        var dragStartPositions = new List<Vector3>(_activeDragStartPositions);
        if (dragMembers.Count == 0)
        {
            dragMembers.Add(state);
            dragStartPositions.Add(state.PieceRenderer.transform.position);
        }

        _drag.DraggingPiece = null;
        var wasOnTray = state.IsOnTray;
        SetPieceSortingOrders(dragMembers, PieceSortingOrder);

        if (releaseScreenPosition.HasValue
            && ShouldReturnPiecesToTray(releaseScreenPosition.Value, dragMembers))
        {
            ReturnDragMembersToTray(dragMembers, wasOnTray);
            ClearActiveDragMembers();
            return;
        }

        if (TryGetClusterBoardSnapTargets(dragMembers, out var groovePositions))
        {
            var draggedSet = new HashSet<DraggablePieceState>(dragMembers);
            // Accepted pieces must stop reserving tray slots before displaced pieces reflow.
            for (var i = 0; i < dragMembers.Count; i++)
            {
                if (dragMembers[i] != null)
                {
                    dragMembers[i].IsOnTray = false;
                }
            }

            var displacedPieces = CollectLoosePiecesOverlappingGrooves(
                dragMembers,
                draggedSet);
            if (displacedPieces.Count > 0)
            {
                DetachPiecesFromLooseClusters(displacedPieces);
                ReturnLoosePiecesToTray(displacedPieces);
            }

            if (dragMembers.Any(member => _hintedPieces.Contains(member)))
            {
                ClearPieceHint();
            }

            DetachPiecesFromLooseClusters(dragMembers);
            AudioManager.Instance.PlaySfx("SFX_PieceCorrect.mp3");
            StartGameplayTimerIfNeeded();
            for (var i = 0; i < dragMembers.Count; i++)
            {
                var member = dragMembers[i];
                if (member?.PieceRenderer == null)
                {
                    continue;
                }

                ApplyPieceRendererShadow(member.PieceRenderer, PieceShadowStyle.Placed);
                member.IsOnTray = false;
                member.IsPlaced = true;
                RecordPlacedPiece(member);
                StartCoroutine(PlayPieceSnapAnimation(member, groovePositions[i]));
            }

            ClearActiveDragMembers();
            return;
        }

        SetLoosePiecePresentation(dragMembers);
        Physics2D.SyncTransforms();
        if (!IsTutorialActive && TryAttachLoosePieces(dragMembers))
        {
            ClearActiveDragMembers();
            return;
        }

        var ignoredMembers = new HashSet<DraggablePieceState>(dragMembers);
        if (!IsLooseClusterPlacementAllowed(dragMembers, ignoredMembers))
        {
            AudioManager.Instance.PlaySfx("SFX_PieceWrongReturn.mp3");
            ReturnDragMembersAfterInvalidDrop(
                dragMembers,
                dragStartPositions,
                wasOnTray);
            ClearActiveDragMembers();
            return;
        }

        for (var i = 0; i < dragMembers.Count; i++)
        {
            var member = dragMembers[i];
            if (member == null)
            {
                continue;
            }

            member.IsOnTray = false;
            RegisterLoosePiece(member);
        }

        AudioManager.Instance.PlaySfx("SFX_PiecePlace.mp3");
        RestorePiecePlacementTutorialPresentation(state);
        ClearActiveDragMembers();
    }

    private void CancelActivePointerInteraction()
    {
        EndTrayScroll();
        var state = _drag.DraggingPiece;
        var dragMembers = new List<DraggablePieceState>(_activeDragMembers);
        var dragStartPositions = new List<Vector3>(_activeDragStartPositions);
        _drag.DraggingPiece = null;
        if (state?.PieceRenderer == null)
        {
            ClearActiveDragMembers();
            GameCursorUtility.SetDefault();
            return;
        }

        SetPieceSortingOrders(dragMembers, PieceSortingOrder);
        if (state.IsOnTray)
        {
            ReturnPieceToTray(
                state,
                wasOnTray: true,
                animateLayout: false);
        }
        else
        {
            for (var i = 0; i < dragMembers.Count; i++)
            {
                var member = dragMembers[i];
                if (member?.PieceRenderer == null || i >= dragStartPositions.Count)
                {
                    continue;
                }

                member.PieceRenderer.transform.position = dragStartPositions[i];
                member.PieceRenderer.transform.localScale = member.DragScale;
                ApplyPieceRendererShadow(member.PieceRenderer, PieceShadowStyle.Loose);
            }
            if (_looseClusterByPiece.TryGetValue(state, out var cancelledCluster)
                && cancelledCluster.Members.Count > 1)
            {
                SetLooseClusterPresentation(cancelledCluster, PieceShadowStyle.Loose);
            }
            Physics2D.SyncTransforms();
            RestorePiecePlacementTutorialPresentation(state);
        }

        ClearActiveDragMembers();
        GameCursorUtility.SetDefault();
    }

    private void PopulateActiveDragMembers(DraggablePieceState anchor)
    {
        ClearActiveDragMembers();
        if (anchor == null)
        {
            return;
        }

        _activeDragMembers.Add(anchor);
        if (!IsTutorialActive
            && _looseClusterByPiece.TryGetValue(anchor, out var cluster))
        {
            for (var i = 0; i < cluster.Members.Count; i++)
            {
                var member = cluster.Members[i];
                if (member != null && member != anchor && !member.IsPlaced)
                {
                    _activeDragMembers.Add(member);
                }
            }
        }

        for (var i = 0; i < _activeDragMembers.Count; i++)
        {
            var renderer = _activeDragMembers[i]?.PieceRenderer;
            _activeDragStartPositions.Add(
                renderer != null ? renderer.transform.position : Vector3.zero);
        }
    }

    private void ClearActiveDragMembers()
    {
        _activeDragMembers.Clear();
        _activeDragStartPositions.Clear();
        ClearTrayPickupLayoutSnapshot();
    }

    private void RestoreHintedPieceRotationsIfDragging()
    {
        if (_activeDragMembers.Any(_hintedPieces.Contains))
        {
            RestoreAllHintedPieceRotations();
            _isHintPieceShaking = false;
            _hintedCluster = null;
            _hintedPieceBasePositions.Clear();
            _hintedClusterCenter = Vector3.zero;
            _hintedClusterShadowBasePosition = Vector3.zero;
            _hintedClusterShadowBaseRotation = Quaternion.identity;
        }
    }

    private static void SetPieceSortingOrders(
        IReadOnlyList<DraggablePieceState> states,
        int sortingOrder)
    {
        for (var i = 0; i < states.Count; i++)
        {
            if (states[i]?.PieceRenderer != null)
            {
                states[i].PieceRenderer.sortingOrder = sortingOrder;
            }
        }
    }

    private void ClampActiveDragMembersToTableBounds()
    {
        if (_activeDragMembers.Count == 0 || !TryGetTableBounds(out var tableBounds))
        {
            return;
        }

        var hasBounds = false;
        var clusterBounds = default(Bounds);
        for (var i = 0; i < _activeDragMembers.Count; i++)
        {
            var renderer = _activeDragMembers[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                clusterBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                clusterBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return;
        }

        var delta = new Vector3(
            CalculateBoundsClampOffset(
                clusterBounds.min.x,
                clusterBounds.max.x,
                tableBounds.min.x,
                tableBounds.max.x,
                clusterBounds.center.x,
                tableBounds.center.x),
            CalculateBoundsClampOffset(
                clusterBounds.min.y,
                clusterBounds.max.y,
                tableBounds.min.y,
                tableBounds.max.y,
                clusterBounds.center.y,
                tableBounds.center.y),
            0f);
        if (delta.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        for (var i = 0; i < _activeDragMembers.Count; i++)
        {
            var renderer = _activeDragMembers[i]?.PieceRenderer;
            if (renderer != null)
            {
                renderer.transform.position += delta;
            }
        }
    }

    private bool TryGetTableBounds(out Bounds tableBounds)
    {
        var camera = Camera.main;
        if (_board.BackgroundRect != null && camera != null)
        {
            tableBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.BackgroundRect,
                camera,
                WorldGameplayDepth);
            return tableBounds.size.sqrMagnitude > 0f;
        }

        var bottomLeft = ToGameplayWorld(Vector2.zero);
        var topRight = ToGameplayWorld(new Vector2(Screen.width, Screen.height));
        tableBounds = new Bounds(
            (bottomLeft + topRight) * 0.5f,
            new Vector3(
                Mathf.Abs(topRight.x - bottomLeft.x),
                Mathf.Abs(topRight.y - bottomLeft.y),
                0f));
        return tableBounds.size.sqrMagnitude > 0f;
    }

    private bool ShouldReturnPiecesToTray(
        Vector2 releaseScreenPosition,
        IReadOnlyList<DraggablePieceState> states)
    {
        for (var i = 0; i < states.Count; i++)
        {
            var renderer = states[i]?.PieceRenderer;
            if (renderer != null
                && ShouldReturnPieceToTray(releaseScreenPosition, renderer))
            {
                return true;
            }
        }

        return false;
    }

    private void ReturnDragMembersToTray(
        List<DraggablePieceState> states,
        bool anchorWasOnTray)
    {
        if (states == null || states.Count == 0)
        {
            return;
        }

        if (states.Any(state => _hintedPieces.Contains(state)))
        {
            ClearPieceHint();
        }

        DetachPiecesFromLooseClusters(states);
        if (states.Count == 1 && anchorWasOnTray)
        {
            ReturnPieceToTray(states[0], wasOnTray: true);
            return;
        }

        ReturnLoosePiecesToTray(states);
    }

    private bool TryGetClusterBoardSnapTargets(
        IReadOnlyList<DraggablePieceState> states,
        out List<Vector3> groovePositions)
    {
        groovePositions = new List<Vector3>(states.Count);
        var bestDelta = Vector3.zero;
        var bestDistance = float.PositiveInfinity;
        DraggablePieceState closestState = null;
        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null || state.GrooveRect == null)
            {
                groovePositions.Clear();
                return false;
            }

            var groovePosition = GetGrooveSnapPosition(state.GrooveRect, Camera.main);
            groovePositions.Add(groovePosition);
            UpdateGrooveOverlapProbe(state, groovePosition);
            var distance = Vector3.Distance(state.PieceRenderer.transform.position, groovePosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDelta = groovePosition - state.PieceRenderer.transform.position;
                closestState = state;
            }
        }

        if (closestState == null || bestDistance > CalculateSnapDistance(closestState))
        {
            return false;
        }

        for (var i = 0; i < states.Count; i++)
        {
            var shiftedPosition = states[i].PieceRenderer.transform.position + bestDelta;
            if (Vector3.Distance(shiftedPosition, groovePositions[i]) > CalculateSnapDistance(states[i]))
            {
                return false;
            }
        }

        return true;
    }

    private List<DraggablePieceState> CollectLoosePiecesOverlappingGrooves(
        IReadOnlyList<DraggablePieceState> states,
        HashSet<DraggablePieceState> ignoredStates)
    {
        var result = new HashSet<DraggablePieceState>();
        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state == null)
            {
                continue;
            }

            UpdateGrooveOverlapProbe(
                state,
                GetGrooveSnapPosition(state.GrooveRect, Camera.main));
        }

        Physics2D.SyncTransforms();
        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state == null)
            {
                continue;
            }

            var overlaps = CollectLoosePiecesOverlappingCollider(
                state,
                state.GrooveProbeCollider,
                ignoredStates);
            for (var overlapIndex = 0; overlapIndex < overlaps.Count; overlapIndex++)
            {
                var overlap = overlaps[overlapIndex];
                if (_looseClusterByPiece.TryGetValue(overlap, out var cluster))
                {
                    for (var memberIndex = 0; memberIndex < cluster.Members.Count; memberIndex++)
                    {
                        if (cluster.Members[memberIndex] != null
                            && !ignoredStates.Contains(cluster.Members[memberIndex]))
                        {
                            result.Add(cluster.Members[memberIndex]);
                        }
                    }
                }
                else
                {
                    result.Add(overlap);
                }
            }
        }

        return result.ToList();
    }

    private void SetLoosePiecePresentation(IReadOnlyList<DraggablePieceState> states)
    {
        LoosePieceCluster cluster = null;
        if (states != null && states.Count > 1)
        {
            _looseClusterByPiece.TryGetValue(states[0], out cluster);
        }

        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null)
            {
                continue;
            }

            state.PieceRenderer.transform.localScale = state.DragScale;
            if (cluster == null)
            {
                ApplyPieceRendererShadow(state.PieceRenderer, PieceShadowStyle.Loose);
            }
        }

        if (cluster != null)
        {
            SetLooseClusterPresentation(cluster, PieceShadowStyle.Loose);
        }
    }

    private bool IsLooseClusterPlacementAllowed(
        IReadOnlyList<DraggablePieceState> states,
        HashSet<DraggablePieceState> ignoredStates)
    {
        if (!IsLooseClusterBoundsPlacementAllowed(states))
        {
            return false;
        }

        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null
                || DoesColliderOverlapLoosePiece(state, state.PieceCollider, ignoredStates)
                || !IsLoosePiecePlacementAllowed(state))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsLooseClusterBoundsPlacementAllowed(
        IReadOnlyList<DraggablePieceState> states)
    {
        if (states.Count <= 1 || _board.GameBoardImage == null || Camera.main == null)
        {
            return true;
        }

        var hasBounds = false;
        var clusterBounds = default(Bounds);
        for (var i = 0; i < states.Count; i++)
        {
            var renderer = states[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                clusterBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                clusterBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return false;
        }

        var camera = Camera.main;
        var boardBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
            _board.GameBoardImage.rectTransform,
            camera,
            WorldGameplayDepth);
        var fullyInsideBoard = clusterBounds.min.x >= boardBounds.min.x
                               && clusterBounds.max.x <= boardBounds.max.x
                               && clusterBounds.min.y >= boardBounds.min.y
                               && clusterBounds.max.y <= boardBounds.max.y;
        var fullyLeftOfBoard = clusterBounds.max.x <= boardBounds.min.x;
        var fullyRightOfBoard = clusterBounds.min.x >= boardBounds.max.x;
        return fullyInsideBoard
               || fullyLeftOfBoard
               || fullyRightOfBoard
               || IsPieceFullyInTableSpaceBelowBoard(clusterBounds, boardBounds, camera);
    }

    private void ReturnDragMembersAfterInvalidDrop(
        IReadOnlyList<DraggablePieceState> states,
        IReadOnlyList<Vector3> startPositions,
        bool anchorWasOnTray)
    {
        if (states.Count == 1 && anchorWasOnTray)
        {
            var state = states[0];
            state.IsOnTray = true;
            var returnPosition = startPositions[0];
            if (!TryRestoreTrayPickupLayout(
                    state,
                    animate: true,
                    animatePickedSeparately: true,
                    out returnPosition))
            {
                ResetPieceTrayPosition(instant: true);
                LayoutTrayPieces(animate: true, excludedState: state);
                returnPosition = state.StartPosition;
            }

            StartCoroutine(PlayInvalidDropReturnAnimation(
                state,
                returnPosition,
                showInvalidTintImmediately: false));
            return;
        }

        for (var i = 0; i < states.Count && i < startPositions.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null)
            {
                continue;
            }

            state.IsOnTray = false;
            StartCoroutine(PlayInvalidDropReturnAnimation(
                state,
                startPositions[i],
                showInvalidTintImmediately: true));
        }
    }

    private bool TryAttachLoosePieces(List<DraggablePieceState> movingStates)
    {
        if (movingStates == null || movingStates.Count == 0)
        {
            return false;
        }

        var movingSet = new HashSet<DraggablePieceState>(movingStates);
        DraggablePieceState bestMoving = null;
        DraggablePieceState bestStationary = null;
        var bestDelta = Vector3.zero;
        var bestDistance = float.PositiveInfinity;
        for (var movingIndex = 0; movingIndex < movingStates.Count; movingIndex++)
        {
            var moving = movingStates[movingIndex];
            if (moving?.PieceRenderer == null || moving.GrooveRect == null)
            {
                continue;
            }

            for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
            {
                var stationary = _drag.CurrentGroupDraggables[i];
                if (stationary == null
                    || movingSet.Contains(stationary)
                    || stationary.IsPlaced
                    || stationary.IsOnTray
                    || stationary.PieceRenderer == null
                    || stationary.GrooveRect == null
                    || !AreGroovesAdjacent(moving, stationary))
                {
                    continue;
                }

                var movingGroove = GetGrooveSnapPosition(moving.GrooveRect, Camera.main);
                var stationaryGroove = GetGrooveSnapPosition(stationary.GrooveRect, Camera.main);
                var desiredMovingPosition = stationary.PieceRenderer.transform.position
                                            + (movingGroove - stationaryGroove);
                var delta = desiredMovingPosition - moving.PieceRenderer.transform.position;
                var distance = delta.magnitude;
                if (distance > CalculateLooseClusterAttachDistance(moving, stationary)
                    || distance >= bestDistance)
                {
                    continue;
                }

                bestMoving = moving;
                bestStationary = stationary;
                bestDelta = delta;
                bestDistance = distance;
            }
        }

        if (bestMoving == null || bestStationary == null)
        {
            return false;
        }

        var startPositions = new List<Vector3>(movingStates.Count);
        for (var i = 0; i < movingStates.Count; i++)
        {
            startPositions.Add(movingStates[i].PieceRenderer.transform.position);
            movingStates[i].PieceRenderer.transform.position += bestDelta;
        }

        Physics2D.SyncTransforms();
        var stationaryMembers = GetLooseClusterMembers(bestStationary);
        var combinedSet = new HashSet<DraggablePieceState>(movingStates);
        combinedSet.UnionWith(stationaryMembers);
        if (!IsLooseClusterPlacementAllowed(movingStates, combinedSet))
        {
            for (var i = 0; i < movingStates.Count; i++)
            {
                movingStates[i].PieceRenderer.transform.position = startPositions[i];
            }

            Physics2D.SyncTransforms();
            return false;
        }

        for (var i = 0; i < movingStates.Count; i++)
        {
            movingStates[i].PieceRenderer.transform.position = startPositions[i];
            movingStates[i].IsOnTray = false;
            RegisterLoosePiece(movingStates[i]);
        }

        var cluster = MergeLoosePieceClusters(movingStates, stationaryMembers);
        if (_hintedPieces.Any(combinedSet.Contains))
        {
            ClearPieceHint();
        }

        AudioManager.Instance.PlaySfx("SFX_PiecePlace.mp3");
        StartCoroutine(PlayLooseClusterAttachAnimation(
            movingStates,
            startPositions,
            bestDelta,
            cluster));
        return true;
    }

    private bool AreGroovesAdjacent(
        DraggablePieceState first,
        DraggablePieceState second)
    {
        if (first?.GrooveProbeCollider == null
            || second?.GrooveProbeCollider == null
            || first.GrooveRect == null
            || second.GrooveRect == null)
        {
            return false;
        }

        UpdateGrooveOverlapProbe(
            first,
            GetGrooveSnapPosition(first.GrooveRect, Camera.main));
        UpdateGrooveOverlapProbe(
            second,
            GetGrooveSnapPosition(second.GrooveRect, Camera.main));
        Physics2D.SyncTransforms();
        var distance = first.GrooveProbeCollider.Distance(second.GrooveProbeCollider);
        if (!distance.isValid)
        {
            return false;
        }

        var firstSize = Mathf.Min(
            first.GrooveProbeCollider.bounds.size.x,
            first.GrooveProbeCollider.bounds.size.y);
        var secondSize = Mathf.Min(
            second.GrooveProbeCollider.bounds.size.x,
            second.GrooveProbeCollider.bounds.size.y);
        var threshold = Mathf.Clamp(
            Mathf.Min(firstSize, secondSize) * LooseClusterAdjacencySizeRatio,
            LooseClusterAdjacencyMin,
            LooseClusterAdjacencyMax);
        return distance.isOverlapped || distance.distance <= threshold;
    }

    private static float CalculateLooseClusterAttachDistance(
        DraggablePieceState first,
        DraggablePieceState second)
    {
        var firstSize = first?.PieceRenderer != null
            ? Mathf.Max(first.PieceRenderer.bounds.size.x, first.PieceRenderer.bounds.size.y)
            : 0f;
        var secondSize = second?.PieceRenderer != null
            ? Mathf.Max(second.PieceRenderer.bounds.size.x, second.PieceRenderer.bounds.size.y)
            : 0f;
        return Mathf.Clamp(
            Mathf.Min(firstSize, secondSize) * LooseClusterAttachDistanceRatio,
            LooseClusterAttachDistanceMin,
            LooseClusterAttachDistanceMax);
    }

    private IEnumerator PlayLooseClusterAttachAnimation(
        IReadOnlyList<DraggablePieceState> states,
        IReadOnlyList<Vector3> startPositions,
        Vector3 delta,
        LoosePieceCluster cluster)
    {
        BeginPiecePlacementAnimation();
        SetPieceSortingOrders(states, PieceSortingOrder + 100);
        var elapsed = 0f;
        while (elapsed < PieceSnapDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = Mathf.Clamp01(elapsed / PieceSnapDuration);
            var eased = 1f - Mathf.Pow(1f - progress, 3f);
            for (var i = 0; i < states.Count && i < startPositions.Count; i++)
            {
                if (states[i]?.PieceRenderer != null)
                {
                    states[i].PieceRenderer.transform.position =
                        startPositions[i] + delta * eased;
                }
            }

            yield return null;
        }

        for (var i = 0; i < states.Count && i < startPositions.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null)
            {
                continue;
            }

            state.PieceRenderer.transform.position = startPositions[i] + delta;
            state.PieceRenderer.sortingOrder = PieceSortingOrder;
        }

        Physics2D.SyncTransforms();
        SetLooseClusterPresentation(
            cluster,
            PieceShadowStyle.Loose,
            rebuildShadow: true);
        if (cluster != null)
        {
            yield return PlayLooseClusterAttachShine(cluster.Members);
        }
        EndPiecePlacementAnimation();
        if (cluster != null && cluster.Members.Count > 0)
        {
            RestorePiecePlacementTutorialPresentation(cluster.Members[0]);
        }
    }

    private IReadOnlyList<DraggablePieceState> GetLooseClusterMembers(
        DraggablePieceState state)
    {
        return state != null
               && _looseClusterByPiece.TryGetValue(state, out var cluster)
            ? cluster.Members
            : new[] { state };
    }

    private LoosePieceCluster MergeLoosePieceClusters(
        IReadOnlyList<DraggablePieceState> firstMembers,
        IReadOnlyList<DraggablePieceState> secondMembers)
    {
        LoosePieceCluster result = null;
        for (var i = 0; i < firstMembers.Count && result == null; i++)
        {
            _looseClusterByPiece.TryGetValue(firstMembers[i], out result);
        }
        for (var i = 0; i < secondMembers.Count && result == null; i++)
        {
            _looseClusterByPiece.TryGetValue(secondMembers[i], out result);
        }

        if (result != null)
        {
            DestroyLooseClusterShadow(result);
        }

        if (result == null)
        {
            result = new LoosePieceCluster
            {
                CreatedOrder = ++_nextLoosePieceOrder
            };
            _loosePieceClusters.Add(result);
        }

        AddMembersToLooseCluster(result, firstMembers);
        AddMembersToLooseCluster(result, secondMembers);

        var mergedClusters = new HashSet<LoosePieceCluster>();
        for (var i = 0; i < result.Members.Count; i++)
        {
            if (_looseClusterByPiece.TryGetValue(result.Members[i], out var cluster)
                && cluster != result)
            {
                mergedClusters.Add(cluster);
            }
        }

        foreach (var cluster in mergedClusters)
        {
            result.CreatedOrder = Math.Min(result.CreatedOrder, cluster.CreatedOrder);
            AddMembersToLooseCluster(result, cluster.Members);
            DestroyLooseClusterShadow(cluster);
            _loosePieceClusters.Remove(cluster);
        }

        for (var i = 0; i < result.Members.Count; i++)
        {
            _looseClusterByPiece[result.Members[i]] = result;
            if (result.Members[i]?.PieceRenderer != null)
            {
                ApplyClusterMemberMaterial(result.Members[i].PieceRenderer);
            }
        }

        return result;
    }

    private void AddMembersToLooseCluster(
        LoosePieceCluster cluster,
        IReadOnlyList<DraggablePieceState> members)
    {
        for (var i = 0; i < members.Count; i++)
        {
            var member = members[i];
            if (member != null && !cluster.Members.Contains(member))
            {
                cluster.Members.Add(member);
            }
        }
    }

    private void RegisterLoosePiece(DraggablePieceState state)
    {
        if (state != null)
        {
            GetOrCreateLoosePieceOrder(state);
        }
    }

    private long GetOrCreateLoosePieceOrder(DraggablePieceState state)
    {
        if (state == null)
        {
            return long.MaxValue;
        }

        if (!_loosePieceOrders.TryGetValue(state, out var order))
        {
            order = ++_nextLoosePieceOrder;
            _loosePieceOrders[state] = order;
        }

        return order;
    }

    private void DetachPiecesFromLooseClusters(IEnumerable<DraggablePieceState> states)
    {
        var affectedClusters = new HashSet<LoosePieceCluster>();
        foreach (var state in states)
        {
            if (state == null)
            {
                continue;
            }

            _loosePieceOrders.Remove(state);
            if (_looseClusterByPiece.TryGetValue(state, out var cluster))
            {
                affectedClusters.Add(cluster);
                cluster.Members.Remove(state);
                _looseClusterByPiece.Remove(state);
            }
        }

        foreach (var cluster in affectedClusters)
        {
            DestroyLooseClusterShadow(cluster);
            if (cluster.Members.Count >= 2)
            {
                SetLooseClusterPresentation(
                    cluster,
                    PieceShadowStyle.Loose,
                    rebuildShadow: true);
                continue;
            }

            _loosePieceClusters.Remove(cluster);
            for (var i = 0; i < cluster.Members.Count; i++)
            {
                var remainingMember = cluster.Members[i];
                _looseClusterByPiece.Remove(remainingMember);
                if (remainingMember?.PieceRenderer != null)
                {
                    ApplyPieceRendererShadow(
                        remainingMember.PieceRenderer,
                        remainingMember.IsPlaced
                            ? PieceShadowStyle.Placed
                            : remainingMember.IsOnTray
                                ? PieceShadowStyle.Initial
                                : PieceShadowStyle.Loose);
                }
            }
        }
    }

    private void ClearLoosePieceClusters()
    {
        for (var i = 0; i < _loosePieceClusters.Count; i++)
        {
            DestroyLooseClusterShadow(_loosePieceClusters[i]);
        }
        _loosePieceClusters.Clear();
        _looseClusterByPiece.Clear();
        _loosePieceOrders.Clear();
        _nextLoosePieceOrder = 0;
        ClearActiveDragMembers();
    }

    private bool IsLoosePiecePlacementAllowed(DraggablePieceState state)
    {
        if (state?.PieceRenderer == null || _board.GameBoardImage == null)
        {
            return true;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return true;
        }

        var pieceBounds = state.PieceRenderer.bounds;
        var boardBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
            _board.GameBoardImage.rectTransform,
            camera,
            WorldGameplayDepth);
        if (pieceBounds.size.sqrMagnitude <= 0f || boardBounds.size.sqrMagnitude <= 0f)
        {
            return true;
        }

        var fullyInsideBoard = pieceBounds.min.x >= boardBounds.min.x
                               && pieceBounds.max.x <= boardBounds.max.x
                               && pieceBounds.min.y >= boardBounds.min.y
                               && pieceBounds.max.y <= boardBounds.max.y;
        if (fullyInsideBoard)
        {
            if (DoesPieceOverlapOccupiedBoardArea(state))
            {
                return false;
            }

            if (CollidersOverlap(state.PieceCollider, state.GrooveProbeCollider))
            {
                return true;
            }

            return !DoesPieceCrossUnfilledBoardBoundary(state);
        }

        var fullyLeftOfBoard = pieceBounds.max.x <= boardBounds.min.x;
        var fullyRightOfBoard = pieceBounds.min.x >= boardBounds.max.x;
        if (fullyLeftOfBoard || fullyRightOfBoard)
        {
            return true;
        }

        return IsPieceFullyInTableSpaceBelowBoard(pieceBounds, boardBounds, camera);
    }

    private bool IsPieceFullyInTableSpaceBelowBoard(
        Bounds pieceBounds,
        Bounds boardBounds,
        Camera camera)
    {
        var horizontallyInsideBoard = pieceBounds.min.x >= boardBounds.min.x
                                      && pieceBounds.max.x <= boardBounds.max.x;
        if (!horizontallyInsideBoard
            || pieceBounds.max.y > boardBounds.min.y
            || !TryGetPieceTrayDropScreenRect(out var trayScreenRect))
        {
            return false;
        }

        var pieceBottomScreenY = camera.WorldToScreenPoint(new Vector3(
            pieceBounds.center.x,
            pieceBounds.min.y,
            WorldGameplayDepth)).y;
        return pieceBottomScreenY >= trayScreenRect.yMax;
    }

    private void ReturnPieceToTray(
        DraggablePieceState state,
        bool wasOnTray,
        bool animateLayout = true)
    {
        if (state?.PieceRenderer == null)
        {
            return;
        }

        ApplyPieceRendererShadow(state.PieceRenderer, PieceShadowStyle.Initial);

        if (wasOnTray
            && TryRestoreTrayPickupLayout(
                state,
                animateLayout,
                animatePickedSeparately: false,
                out _))
        {
            RestorePiecePlacementTutorialPresentation(state);
            return;
        }

        ResetPieceTrayPosition(instant: true);
        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.gameObject.SetActive(true);
        }
        else if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.gameObject.SetActive(true);
            _board.PieceBgRenderer.enabled = true;
        }

        Canvas.ForceUpdateCanvases();
        state.TrayScale = CalculateTrayScaleForPiece(
            state.PieceRenderer,
            GetPieceTrayBounds(),
            state.DragScale);
        state.IsOnTray = true;
        if (wasOnTray)
        {
            state.PieceRenderer.transform.localScale = state.TrayScale;
            LayoutTrayPieces(animate: animateLayout);
            RestorePiecePlacementTutorialPresentation(state);
            return;
        }

        LayoutTrayPieces(animate: animateLayout, excludedState: state);
        var trayPosition = state.StartPosition;
        StartCoroutine(PlayInvalidDropReturnAnimation(state, trayPosition));
    }

    private bool DoesPieceOverlapOccupiedBoardArea(DraggablePieceState movingState)
    {
        return DoesPieceOverlapBoardArea(movingState, occupiedArea: true);
    }

    private bool DoesPieceCrossUnfilledBoardBoundary(DraggablePieceState movingState)
    {
        if (!DoesPieceOverlapBoardArea(movingState, occupiedArea: false))
        {
            return false;
        }

        var gameBoardProbe = GetOrCreateGameBoardOpaqueProbe();
        if (gameBoardProbe == null || _board.GameBoardImage == null)
        {
            return false;
        }

        var probeTransform = gameBoardProbe.transform;
        probeTransform.position = GetGrooveSnapPosition(
            _board.GameBoardImage.rectTransform,
            Camera.main);
        probeTransform.rotation = _board.GameBoardImage.rectTransform.rotation;
        probeTransform.localScale = CalculatePieceScaleOnBoard(_board.GameBoardImage);
        Physics2D.SyncTransforms();
        return CollidersOverlap(movingState.PieceCollider, gameBoardProbe);
    }

    private bool DoesPieceOverlapBoardArea(
        DraggablePieceState movingState,
        bool occupiedArea)
    {
        if (movingState?.PieceCollider == null || _board.GrooveImagesByGroup == null)
        {
            return false;
        }

        var areaProbes = new List<Collider2D>();
        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var i = 0; i < group.Count; i++)
            {
                var grooveImage = group[i];
                if (grooveImage == null
                    || grooveImage.sprite == null)
                {
                    continue;
                }

                var isOccupied = grooveImage.gameObject.activeInHierarchy
                                 && grooveImage.color.a > 0.001f;
                if (isOccupied != occupiedArea)
                {
                    continue;
                }

                var probe = GetOrCreateBoardOccupancyProbe(grooveImage);
                if (probe == null)
                {
                    continue;
                }

                var probeTransform = probe.transform;
                probeTransform.position = GetGrooveSnapPosition(
                    grooveImage.rectTransform,
                    Camera.main);
                probeTransform.rotation = grooveImage.rectTransform.rotation;
                probeTransform.localScale = CalculatePieceScaleOnBoard(grooveImage);
                areaProbes.Add(probe);
            }
        }

        Physics2D.SyncTransforms();
        for (var i = 0; i < areaProbes.Count; i++)
        {
            if (CollidersOverlap(movingState.PieceCollider, areaProbes[i]))
            {
                return true;
            }
        }

        return false;
    }

    private Collider2D GetOrCreateBoardOccupancyProbe(Image grooveImage)
    {
        if (grooveImage == null || grooveImage.sprite == null)
        {
            return null;
        }

        if (_boardOccupancyProbes.TryGetValue(grooveImage, out var existingProbe)
            && existingProbe != null)
        {
            return existingProbe;
        }

        if (_boardOccupancyProbeRoot == null)
        {
            var rootObject = new GameObject(BoardOccupancyProbeRootObjectName);
            _boardOccupancyProbeRoot = rootObject.transform;
        }

        var probe = CreateGrooveOverlapProbe(
            grooveImage.sprite,
            _boardOccupancyProbeRoot,
            $"{grooveImage.gameObject.name}_OccupancyProbe");
        _boardOccupancyProbes[grooveImage] = probe;
        return probe;
    }

    private Collider2D GetOrCreateGameBoardOpaqueProbe()
    {
        if (_gameBoardOpaqueProbe != null)
        {
            return _gameBoardOpaqueProbe;
        }

        var sprite = _board.GameBoardImage != null
            ? _board.GameBoardImage.sprite
            : null;
        if (sprite == null || sprite.GetPhysicsShapeCount() <= 0)
        {
            return null;
        }

        if (_boardOccupancyProbeRoot == null)
        {
            var rootObject = new GameObject(BoardOccupancyProbeRootObjectName);
            _boardOccupancyProbeRoot = rootObject.transform;
        }

        var probeObject = new GameObject(GameBoardOpaqueProbeObjectName);
        probeObject.transform.SetParent(_boardOccupancyProbeRoot, false);
        _gameBoardOpaqueProbe = CreateSpriteOverlapCollider(probeObject, sprite);
        return _gameBoardOpaqueProbe;
    }

    private void DestroyBoardOccupancyProbes()
    {
        _boardOccupancyProbes.Clear();
        _gameBoardOpaqueProbe = null;
        if (_boardOccupancyProbeRoot == null)
        {
            return;
        }

        Destroy(_boardOccupancyProbeRoot.gameObject);
        _boardOccupancyProbeRoot = null;
    }

    private static void UpdateGrooveOverlapProbe(
        DraggablePieceState state,
        Vector3 groovePosition)
    {
        if (state?.GrooveProbeCollider == null)
        {
            return;
        }

        var probeTransform = state.GrooveProbeCollider.transform;
        probeTransform.position = groovePosition;
        probeTransform.rotation = state.GrooveRect != null
            ? state.GrooveRect.rotation
            : Quaternion.identity;
        probeTransform.localScale = state.BoardScale;
    }

    private List<DraggablePieceState> CollectLoosePiecesOverlappingCollider(
        DraggablePieceState movingState,
        Collider2D movingCollider,
        HashSet<DraggablePieceState> ignoredStates = null)
    {
        var overlappingStates = new List<DraggablePieceState>();
        if (movingState == null || movingCollider == null)
        {
            return overlappingStates;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state == movingState
                || (ignoredStates != null && ignoredStates.Contains(state))
                || state.IsPlaced
                || state.IsOnTray
                || state.PieceRenderer == null
                || state.PieceCollider == null)
            {
                continue;
            }

            if (CollidersOverlap(movingCollider, state.PieceCollider))
            {
                overlappingStates.Add(state);
            }
        }

        return overlappingStates;
    }

    private bool DoesPieceOverlapLoosePiece(DraggablePieceState state)
    {
        return DoesColliderOverlapLoosePiece(state, state?.PieceCollider);
    }

    private bool DoesColliderOverlapLoosePiece(
        DraggablePieceState movingState,
        Collider2D movingCollider,
        HashSet<DraggablePieceState> ignoredStates = null)
    {
        if (movingState == null || movingCollider == null)
        {
            return false;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state == movingState
                || (ignoredStates != null && ignoredStates.Contains(state))
                || state.IsPlaced
                || state.IsOnTray
                || state.PieceRenderer == null
                || state.PieceCollider == null)
            {
                continue;
            }

            if (CollidersOverlap(movingCollider, state.PieceCollider))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CollidersOverlap(Collider2D first, Collider2D second)
    {
        if (first == null
            || second == null
            || !first.enabled
            || !second.enabled
            || !first.gameObject.activeInHierarchy
            || !second.gameObject.activeInHierarchy)
        {
            return false;
        }

        var distance = first.Distance(second);
        return distance.isValid && distance.isOverlapped;
    }

    private void ReturnLoosePiecesToTray(List<DraggablePieceState> states)
    {
        if (states == null || states.Count == 0)
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_PieceWrongReturn.mp3");
        ResetPieceTrayPosition(instant: true);
        var excludedStates = new HashSet<DraggablePieceState>();
        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer == null || state.IsPlaced)
            {
                continue;
            }

            state.IsOnTray = true;
            ApplyPieceRendererShadow(state.PieceRenderer, PieceShadowStyle.Initial);
            excludedStates.Add(state);
        }

        LayoutTrayPieces(animate: true, excludedStates: excludedStates);
        foreach (var state in excludedStates)
        {
            StartCoroutine(
                PlayInvalidDropReturnAnimation(state, state.StartPosition));
        }
    }

    private IEnumerator PlayInvalidDropReturnAnimation(
        DraggablePieceState state,
        Vector3 returnPosition,
        bool showInvalidTintImmediately = false)
    {
        var renderer = state?.PieceRenderer;
        if (renderer == null)
        {
            yield break;
        }

        BeginPiecePlacementAnimation();
        var startPosition = renderer.transform.position;
        var startScale = renderer.transform.localScale;
        if (state.IsOnTray)
        {
            state.TrayScale = CalculateTrayScaleForPiece(
                renderer,
                GetPieceTrayBounds(),
                state.DragScale);
        }
        var returnScale = state.IsOnTray ? state.TrayScale : state.DragScale;
        var originalColor = renderer.color;
        var invalidColor = Color.LerpUnclamped(
            originalColor,
            InvalidDropTintColor,
            InvalidDropTintStrength);
        invalidColor.a = originalColor.a;
        renderer.sortingOrder = PieceSortingOrder + 100;

        var elapsed = 0f;
        var didShowInvalidTint = showInvalidTintImmediately;
        if (didShowInvalidTint)
        {
            renderer.color = invalidColor;
        }

        while (elapsed < InvalidDropReturnDuration && renderer != null)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = Mathf.Clamp01(elapsed / InvalidDropReturnDuration);
            var eased = 1f - Mathf.Pow(1f - progress, 3f);
            renderer.transform.position = Vector3.LerpUnclamped(
                startPosition,
                returnPosition,
                eased);
            renderer.transform.localScale = Vector3.LerpUnclamped(
                startScale,
                returnScale,
                eased);
            if (!didShowInvalidTint
                && state.IsOnTray
                && DoesPieceOverlapTray(renderer))
            {
                didShowInvalidTint = true;
                renderer.color = invalidColor;
            }
            yield return null;
        }

        if (renderer != null)
        {
            renderer.transform.position = returnPosition;
            renderer.transform.localScale = returnScale;

            elapsed = 0f;
            while (didShowInvalidTint
                   && elapsed < InvalidDropColorRestoreDuration
                   && renderer != null)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
                var progress = Mathf.Clamp01(elapsed / InvalidDropColorRestoreDuration);
                renderer.color = Color.LerpUnclamped(invalidColor, originalColor, progress);
                yield return null;
            }
        }

        if (renderer != null)
        {
            renderer.color = originalColor;
            renderer.sortingOrder = PieceSortingOrder;
        }

        EndPiecePlacementAnimation();
        RestorePiecePlacementTutorialPresentation(state);
    }

    private void BeginPiecePlacementAnimation()
    {
        _piecePlacementAnimationCount++;
        _piecePlacementDragBlockCount++;
        _isPiecePlacementAnimating = true;
    }

    private bool IsPiecePlacementDragBlocked => _piecePlacementDragBlockCount > 0;

    private void ReleasePiecePlacementDragBlock()
    {
        _piecePlacementDragBlockCount = Mathf.Max(0, _piecePlacementDragBlockCount - 1);
    }

    private void EndPiecePlacementAnimation(bool dragBlockAlreadyReleased = false)
    {
        if (!dragBlockAlreadyReleased)
        {
            ReleasePiecePlacementDragBlock();
        }

        _piecePlacementAnimationCount = Mathf.Max(0, _piecePlacementAnimationCount - 1);
        _isPiecePlacementAnimating = _piecePlacementAnimationCount > 0;
    }

    private void RecordPlacedPiece(DraggablePieceState state)
    {
        var pieceNumber = GetPieceNumberFromState(state);
        if (pieceNumber == int.MaxValue)
        {
            Debug.LogWarning("GameScene: placed Piece has an invalid numbered name; progress was not saved.");
            return;
        }

        _placedPieceNumbers.Add(pieceNumber);
        var packId = GameManager.GetBagId();
        if (!CardPackDataUtility.TryRecordPlacedPiece(packId, pieceNumber))
        {
            Debug.LogWarning(
                $"GameScene: failed to persist placed Piece. packId={packId}, pieceNumber={pieceNumber}");
        }
    }

    private bool ShouldReturnPieceToTray(
        Vector2 releaseScreenPosition,
        SpriteRenderer pieceRenderer)
    {
        if (DoesPieceOverlapTray(pieceRenderer))
        {
            return true;
        }

        if (!TryGetPieceTrayDropScreenRect(out var trayScreenRect))
        {
            return false;
        }

        if (trayScreenRect.Contains(releaseScreenPosition))
        {
            return true;
        }

        return TryGetRendererScreenRect(
                   pieceRenderer,
                   Camera.main,
                   out var pieceScreenRect)
               && trayScreenRect.Overlaps(pieceScreenRect);
    }

    private bool TryGetPieceTrayDropScreenRect(out Rect screenRect)
    {
        screenRect = default;
        if (!_hasPieceTrayDropNormalizedViewportRect
            || !TryGetGameplayViewportRect(out var viewport))
        {
            return false;
        }

        screenRect = Rect.MinMaxRect(
            viewport.xMin + _pieceTrayDropNormalizedViewportRect.xMin * viewport.width,
            viewport.yMin + _pieceTrayDropNormalizedViewportRect.yMin * viewport.height,
            viewport.xMin + _pieceTrayDropNormalizedViewportRect.xMax * viewport.width,
            viewport.yMin + _pieceTrayDropNormalizedViewportRect.yMax * viewport.height);
        return screenRect.width > 0f && screenRect.height > 0f;
    }

    private static bool TryGetGameplayViewportRect(out Rect viewport)
    {
        var camera = Camera.main;
        viewport = camera != null
            ? camera.pixelRect
            : Rect.MinMaxRect(0f, 0f, Screen.width, Screen.height);
        return viewport.width > 0.001f && viewport.height > 0.001f;
    }

    private bool DoesPieceOverlapTray(SpriteRenderer renderer)
    {
        if (renderer == null || IsPieceTrayHidden())
        {
            return false;
        }

        var pieceBounds = renderer.bounds;
        var trayBounds = GetPieceTrayBounds();
        if (pieceBounds.size.sqrMagnitude <= 0f || trayBounds.size.sqrMagnitude <= 0f)
        {
            return false;
        }

        return pieceBounds.max.x > trayBounds.min.x
            && pieceBounds.min.x < trayBounds.max.x
            && pieceBounds.max.y > trayBounds.min.y
            && pieceBounds.min.y < trayBounds.max.y;
    }

    private Vector3 ClampPieceToTableBounds(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return Vector3.zero;
        }

        var camera = Camera.main;
        Bounds tableBounds;
        if (_board.BackgroundRect != null && camera != null)
        {
            tableBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.BackgroundRect,
                camera,
                WorldGameplayDepth);
        }
        else
        {
            var bottomLeft = ToGameplayWorld(Vector2.zero);
            var topRight = ToGameplayWorld(new Vector2(Screen.width, Screen.height));
            tableBounds = new Bounds(
                (bottomLeft + topRight) * 0.5f,
                new Vector3(
                    Mathf.Abs(topRight.x - bottomLeft.x),
                    Mathf.Abs(topRight.y - bottomLeft.y),
                    0f));
        }

        var pieceBounds = renderer.bounds;
        var offsetX = CalculateBoundsClampOffset(
            pieceBounds.min.x,
            pieceBounds.max.x,
            tableBounds.min.x,
            tableBounds.max.x,
            pieceBounds.center.x,
            tableBounds.center.x);
        var offsetY = CalculateBoundsClampOffset(
            pieceBounds.min.y,
            pieceBounds.max.y,
            tableBounds.min.y,
            tableBounds.max.y,
            pieceBounds.center.y,
            tableBounds.center.y);
        var position = renderer.transform.position;
        position.x += offsetX;
        position.y += offsetY;
        position.z = WorldGameplayDepth;
        return position;
    }

    private static float CalculateBoundsClampOffset(
        float itemMin,
        float itemMax,
        float containerMin,
        float containerMax,
        float itemCenter,
        float containerCenter)
    {
        if (itemMax - itemMin >= containerMax - containerMin)
        {
            return containerCenter - itemCenter;
        }

        if (itemMin < containerMin)
        {
            return containerMin - itemMin;
        }

        return itemMax > containerMax ? containerMax - itemMax : 0f;
    }

    private void CommitPlacedPieceToBoardImage(DraggablePieceState state)
    {
        if (state?.GrooveImage == null || state.PieceRenderer == null)
        {
            return;
        }

        state.GrooveImage.gameObject.SetActive(true);
        ApplyPlacedPieceImageShadow(state.GrooveImage);
        SetImageAlpha(state.GrooveImage, 1f);
        RemoveAmbientLightsForRoot(state.PieceRenderer.gameObject);
        AddAmbientBoardPieceLights(state.GrooveImage);
        if (state.GrooveProbeCollider != null)
        {
            state.GrooveProbeCollider.gameObject.SetActive(false);
        }
        state.PieceRenderer.gameObject.SetActive(false);
        Destroy(state.PieceRenderer.gameObject);
        state.PieceRenderer = null;
    }

    private IEnumerator PlayPieceSnapAnimation(
        DraggablePieceState state,
        Vector3 groovePosition)
    {
        var renderer = state?.PieceRenderer;
        if (renderer == null)
        {
            yield break;
        }

        BeginPiecePlacementAnimation();
        var startPosition = renderer.transform.position;
        var startScale = renderer.transform.localScale;
        renderer.sortingOrder = PieceSortingOrder + 100;

        var elapsed = 0f;
        while (elapsed < PieceSnapDuration && renderer != null)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = Mathf.Clamp01(elapsed / PieceSnapDuration);
            var eased = 1f - Mathf.Pow(1f - progress, 3f);
            renderer.transform.position = Vector3.LerpUnclamped(
                startPosition,
                groovePosition,
                eased);
            renderer.transform.localScale = Vector3.LerpUnclamped(
                startScale,
                state.BoardScale,
                eased);
            yield return null;
        }

        if (renderer != null)
        {
            renderer.transform.position = groovePosition;
            renderer.transform.localScale = state.BoardScale;
            renderer.sortingOrder = PieceSortingOrder;
        }

        CommitPlacedPieceToBoardImage(state);
        ReleasePiecePlacementDragBlock();
        yield return PlayPiecePlacementSuccessShine(state.GrooveImage);
        EndPiecePlacementAnimation(dragBlockAlreadyReleased: true);

        if (_isPiecePlacementAnimating)
        {
            yield break;
        }

        var didAdvanceGroup = TryAdvanceGroup();
        if (!didAdvanceGroup && _tutorialStage == TutorialStage.TwoPiecePractice)
        {
            RefreshPiecePlacementTutorialPresentation();
        }
    }

    private IEnumerator PlayPiecePlacementSuccessShine(Image grooveImage)
    {
        var propagationComplete = false;
        StartCoroutine(PlayPiecePlacementLightPropagation(
            grooveImage,
            () => propagationComplete = true));
        yield return PlayCurrentPiecePlacementShine(grooveImage);
        while (!propagationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator PlayPiecePlacementLightPropagation(
        Image grooveImage,
        Action onComplete)
    {
        yield return PlayPiecePlacementLightPropagation(grooveImage);
        onComplete?.Invoke();
    }

    private IEnumerator PlayPiecePlacementLightPropagation(Image grooveImage)
    {
        if (grooveImage == null || grooveImage.sprite == null)
        {
            yield break;
        }

        if (!EnsurePiecePlacementLightResources()
            || !TryGetRectTransformScreenRect(grooveImage.rectTransform, out var sourceScreenRect))
        {
            yield break;
        }

        var targets = CollectPiecePlacementLightTargets(grooveImage, sourceScreenRect);
        var effects = new List<PiecePlacementLightFx>();
        for (var i = 0; i < targets.Count; i++)
        {
            CreatePiecePlacementLightAnimation(targets[i], sourceScreenRect, i, effects);
        }

        if (effects.Count == 0)
        {
            yield break;
        }

        var animationDuration = 0f;
        for (var i = 0; i < effects.Count; i++)
        {
            animationDuration = Mathf.Max(
                animationDuration,
                effects[i].Delay + effects[i].Lifetime);
        }

        var elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            for (var i = 0; i < effects.Count; i++)
            {
                UpdatePiecePlacementLight(effects[i], elapsed);
            }

            yield return null;
        }

        for (var i = 0; i < effects.Count; i++)
        {
            UpdatePiecePlacementLight(effects[i], animationDuration);
        }
    }

    private IEnumerator PlayCurrentPiecePlacementShine(Image grooveImage)
    {
        if (grooveImage == null || grooveImage.sprite == null)
        {
            yield break;
        }

        var shineMaterialTemplate = GetOrCreatePiecePlacementShineMaterial();
        if (shineMaterialTemplate == null)
        {
            yield break;
        }

        var shineMaterial = new Material(shineMaterialTemplate)
        {
            name = $"{PiecePlacementShineMaterialName} Instance"
        };
        _activePiecePlacementShineMaterials.Add(shineMaterial);

        var shineObject = CreateCurrentPiecePlacementShineOverlay(
            grooveImage,
            shineMaterial);
        if (shineObject == null
            || !TryGetRectTransformScreenRect(
                grooveImage.rectTransform,
                out var sourceScreenRect))
        {
            if (shineObject != null)
            {
                Destroy(shineObject);
            }

            DestroyPiecePlacementShineMaterial(shineMaterial);

            yield break;
        }

        var sweepAxis = new Vector2(-0.58f, 0.82f).normalized;
        GetScreenRectAxisRange(
            sourceScreenRect,
            sweepAxis,
            out var sweepStart,
            out var sweepEnd);
        sweepStart -= PiecePlacementShineBandWidth * 2f;
        sweepEnd += PiecePlacementShineBandWidth * 2f;

        shineMaterial.SetVector(ShineSweepAxisId, sweepAxis);
        shineMaterial.SetFloat(ShineBandWidthId, PiecePlacementShineBandWidth);
        shineMaterial.SetColor(ShineColorId, PiecePlacementShineColor);

        var elapsed = 0f;
        while (elapsed < PiecePlacementShineDuration && shineObject != null)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = Mathf.Clamp01(elapsed / PiecePlacementShineDuration);
            var eased = progress * progress * (3f - 2f * progress);
            shineMaterial.SetFloat(
                ShineSweepCenterId,
                Mathf.LerpUnclamped(sweepStart, sweepEnd, eased));
            yield return null;
        }

        if (shineObject != null)
        {
            Destroy(shineObject);
        }

        DestroyPiecePlacementShineMaterial(shineMaterial);
    }

    private IEnumerator PlayLooseClusterAttachShine(
        IReadOnlyList<DraggablePieceState> states)
    {
        if (states == null || states.Count == 0 || Camera.main == null)
        {
            yield break;
        }

        var shineMaterialTemplate = GetOrCreatePiecePlacementShineMaterial();
        if (shineMaterialTemplate == null)
        {
            yield break;
        }

        var shineMaterial = new Material(shineMaterialTemplate)
        {
            name = $"{PiecePlacementShineMaterialName} Loose Cluster Instance"
        };
        _activePiecePlacementShineMaterials.Add(shineMaterial);
        var shineObjects = new List<GameObject>(states.Count);
        var hasScreenRect = false;
        var clusterScreenRect = default(Rect);
        for (var i = 0; i < states.Count; i++)
        {
            var renderer = states[i]?.PieceRenderer;
            if (renderer == null || renderer.sprite == null)
            {
                continue;
            }

            var shineObject = CreateLoosePiecePlacementShineOverlay(
                renderer,
                shineMaterial);
            if (shineObject != null)
            {
                shineObjects.Add(shineObject);
            }

            if (!TryGetRendererScreenRect(renderer, Camera.main, out var screenRect))
            {
                continue;
            }

            clusterScreenRect = hasScreenRect
                ? UnionHintRects(clusterScreenRect, screenRect)
                : screenRect;
            hasScreenRect = true;
        }

        if (shineObjects.Count == 0 || !hasScreenRect)
        {
            DestroyLooseClusterShineObjects(shineObjects);
            DestroyPiecePlacementShineMaterial(shineMaterial);
            yield break;
        }

        var sweepAxis = new Vector2(-0.58f, 0.82f).normalized;
        GetScreenRectAxisRange(
            clusterScreenRect,
            sweepAxis,
            out var sweepStart,
            out var sweepEnd);
        sweepStart -= PiecePlacementShineBandWidth * 2f;
        sweepEnd += PiecePlacementShineBandWidth * 2f;
        shineMaterial.SetVector(ShineSweepAxisId, sweepAxis);
        shineMaterial.SetFloat(ShineBandWidthId, PiecePlacementShineBandWidth);
        shineMaterial.SetColor(ShineColorId, PiecePlacementShineColor);

        var elapsed = 0f;
        while (elapsed < PiecePlacementShineDuration && shineObjects.Count > 0)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = Mathf.Clamp01(elapsed / PiecePlacementShineDuration);
            var eased = progress * progress * (3f - 2f * progress);
            shineMaterial.SetFloat(
                ShineSweepCenterId,
                Mathf.LerpUnclamped(sweepStart, sweepEnd, eased));
            yield return null;
        }

        DestroyLooseClusterShineObjects(shineObjects);
        DestroyPiecePlacementShineMaterial(shineMaterial);
    }

    private static GameObject CreateLoosePiecePlacementShineOverlay(
        SpriteRenderer sourceRenderer,
        Material shineMaterial)
    {
        if (sourceRenderer == null
            || sourceRenderer.sprite == null
            || shineMaterial == null)
        {
            return null;
        }

        var shineObject = new GameObject(
            $"{sourceRenderer.gameObject.name}_ClusterAttachShine");
        shineObject.transform.SetParent(sourceRenderer.transform, false);
        shineObject.transform.localPosition = Vector3.zero;
        shineObject.transform.localRotation = Quaternion.identity;
        shineObject.transform.localScale = Vector3.one;
        var shineRenderer = shineObject.AddComponent<SpriteRenderer>();
        shineRenderer.sprite = sourceRenderer.sprite;
        shineRenderer.sharedMaterial = shineMaterial;
        shineRenderer.color = Color.white;
        shineRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        shineRenderer.sortingOrder = sourceRenderer.sortingOrder + 10;
        return shineObject;
    }

    private static void DestroyLooseClusterShineObjects(List<GameObject> shineObjects)
    {
        for (var i = 0; i < shineObjects.Count; i++)
        {
            if (shineObjects[i] != null)
            {
                Destroy(shineObjects[i]);
            }
        }

        shineObjects.Clear();
    }

    private static GameObject CreateCurrentPiecePlacementShineOverlay(
        Image sourceImage,
        Material shineMaterial)
    {
        if (sourceImage == null || sourceImage.sprite == null || shineMaterial == null)
        {
            return null;
        }

        var sourceRect = sourceImage.rectTransform;
        var shineObject = new GameObject(
            $"{sourceImage.gameObject.name}_PlacementShine",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        shineObject.layer = sourceImage.gameObject.layer;
        var shineRect = shineObject.GetComponent<RectTransform>();
        shineRect.SetParent(sourceRect.parent, false);
        shineRect.anchorMin = sourceRect.anchorMin;
        shineRect.anchorMax = sourceRect.anchorMax;
        shineRect.pivot = sourceRect.pivot;
        shineRect.anchoredPosition = sourceRect.anchoredPosition;
        shineRect.sizeDelta = sourceRect.sizeDelta;
        shineRect.localRotation = sourceRect.localRotation;
        shineRect.localScale = sourceRect.localScale;
        shineRect.SetAsLastSibling();

        var shineImage = shineObject.GetComponent<Image>();
        shineImage.sprite = sourceImage.sprite;
        shineImage.type = sourceImage.type;
        shineImage.preserveAspect = sourceImage.preserveAspect;
        shineImage.useSpriteMesh = sourceImage.useSpriteMesh;
        shineImage.fillCenter = sourceImage.fillCenter;
        shineImage.fillMethod = sourceImage.fillMethod;
        shineImage.fillAmount = sourceImage.fillAmount;
        shineImage.fillClockwise = sourceImage.fillClockwise;
        shineImage.fillOrigin = sourceImage.fillOrigin;
        shineImage.color = Color.white;
        shineImage.material = shineMaterial;
        shineImage.raycastTarget = false;
        shineImage.maskable = false;
        return shineObject;
    }

    private static Vector2 NormalizeScreenPoint(Vector2 screenPoint)
    {
        return new Vector2(
            Screen.width > 0 ? screenPoint.x / Screen.width : 0f,
            Screen.height > 0 ? screenPoint.y / Screen.height : 0f);
    }

    private static void GetScreenRectAxisRange(
        Rect screenRect,
        Vector2 axis,
        out float minimum,
        out float maximum)
    {
        var bottomLeft = NormalizeScreenPoint(new Vector2(screenRect.xMin, screenRect.yMin));
        var topLeft = NormalizeScreenPoint(new Vector2(screenRect.xMin, screenRect.yMax));
        var topRight = NormalizeScreenPoint(new Vector2(screenRect.xMax, screenRect.yMax));
        var bottomRight = NormalizeScreenPoint(new Vector2(screenRect.xMax, screenRect.yMin));
        minimum = Mathf.Min(
            Vector2.Dot(bottomLeft, axis),
            Vector2.Dot(topLeft, axis),
            Vector2.Dot(topRight, axis),
            Vector2.Dot(bottomRight, axis));
        maximum = Mathf.Max(
            Vector2.Dot(bottomLeft, axis),
            Vector2.Dot(topLeft, axis),
            Vector2.Dot(topRight, axis),
            Vector2.Dot(bottomRight, axis));
    }

    private List<PiecePlacementLightTarget> CollectPiecePlacementLightTargets(
        Image sourceImage,
        Rect sourceScreenRect)
    {
        var targets = new List<PiecePlacementLightTarget>
        {
            new PiecePlacementLightTarget
            {
                Image = sourceImage,
                ScreenRect = sourceScreenRect,
                Distance = 0f
            }
        };
        if (_board.GrooveImagesByGroup == null)
        {
            return targets;
        }

        var displayScale = Screen.height > 0
            ? Screen.height / (float)GameDefine.DesignHeight
            : 1f;
        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var i = 0; i < group.Count; i++)
            {
                var candidate = group[i];
                var pieceNumber = GetPieceNumberFromImage(candidate);
                if (candidate == null
                    || candidate == sourceImage
                    || !_placedPieceNumbers.Contains(pieceNumber)
                    || !candidate.gameObject.activeInHierarchy
                    || candidate.color.a <= 0.001f
                    || !TryGetRectTransformScreenRect(candidate.rectTransform, out var candidateRect))
                {
                    continue;
                }

                var edgeDistance = GetRectEdgeDistance(sourceScreenRect, candidateRect);
                var adjacencyDistance = Mathf.Max(
                    32f * displayScale,
                    Mathf.Min(
                        Mathf.Max(sourceScreenRect.width, sourceScreenRect.height),
                        Mathf.Max(candidateRect.width, candidateRect.height)) * 0.22f);
                if (edgeDistance > adjacencyDistance)
                {
                    continue;
                }

                targets.Add(new PiecePlacementLightTarget
                {
                    Image = candidate,
                    ScreenRect = candidateRect,
                    Distance = Vector2.Distance(sourceScreenRect.center, candidateRect.center)
                });
            }
        }

        targets.Sort((left, right) => left.Distance.CompareTo(right.Distance));
        if (targets.Count > PiecePlacementLightMaxAffectedPieces)
        {
            targets.RemoveRange(
                PiecePlacementLightMaxAffectedPieces,
                targets.Count - PiecePlacementLightMaxAffectedPieces);
        }

        return targets;
    }

    private static float GetRectEdgeDistance(Rect first, Rect second)
    {
        var x = Mathf.Max(
            0f,
            Mathf.Abs(first.center.x - second.center.x) - (first.width + second.width) * 0.5f);
        var y = Mathf.Max(
            0f,
            Mathf.Abs(first.center.y - second.center.y) - (first.height + second.height) * 0.5f);
        return Mathf.Sqrt(x * x + y * y);
    }

    private void CreatePiecePlacementLightAnimation(
        PiecePlacementLightTarget target,
        Rect sourceScreenRect,
        int targetIndex,
        List<PiecePlacementLightFx> effects)
    {
        if (target.Image == null || target.Image.sprite == null)
        {
            return;
        }

        var pieceNumber = GetPieceNumberFromImage(target.Image);
        var light = FindActivePieceLight(pieceNumber);
        if (light?.Transform == null || light.Image == null)
        {
            return;
        }

        var direction = targetIndex == 0
            ? new Vector2(0.82f, -0.57f).normalized
            : (target.ScreenRect.center - sourceScreenRect.center).normalized;
        var maxDistance = Mathf.Max(
            sourceScreenRect.width,
            sourceScreenRect.height) * 2f;
        var normalizedDistance = Mathf.Clamp01(target.Distance / Mathf.Max(1f, maxDistance));
        var targetDelay = targetIndex == 0 ? 0f : 0.07f + normalizedDistance * 0.16f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        if (light.Deformer == null || light.Image == null)
        {
            return;
        }

        var lightSize = light.Image.rectTransform.rect.size;
        var bendDistance = Mathf.Clamp(
            Mathf.Min(Mathf.Abs(lightSize.x), Mathf.Abs(lightSize.y)) * 0.28f,
            PiecePlacementLightMinBendPixels,
            PiecePlacementLightMaxBendPixels) * PiecePlacementLightDistanceMultiplier;
        light.AnimationVersion++;
        effects.Add(new PiecePlacementLightFx
        {
            Light = light,
            AnimationVersion = light.AnimationVersion,
            Delay = targetDelay,
            Lifetime = (targetIndex == 0 ? 0.48f : 0.42f)
                       * PiecePlacementLightDurationMultiplier,
            StartBendOffset = light.Deformer.BendOffset,
            StartMiddleStretch = light.Deformer.MiddleStretch,
            BendDirection = direction,
            BendDistance = bendDistance
        });
    }

    private static GameObject CreatePiecePlacementLightMask(Image sourceImage)
    {
        var maskObject = new GameObject(
            $"{sourceImage.gameObject.name}_PlacementLights",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Mask));
        maskObject.layer = sourceImage.gameObject.layer;
        var maskRect = maskObject.GetComponent<RectTransform>();
        maskRect.SetParent(sourceImage.rectTransform, false);
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;
        maskRect.localRotation = Quaternion.identity;
        maskRect.localScale = Vector3.one;
        maskRect.SetAsLastSibling();

        var maskImage = maskObject.GetComponent<Image>();
        maskImage.sprite = sourceImage.sprite;
        maskImage.type = sourceImage.type;
        maskImage.preserveAspect = sourceImage.preserveAspect;
        maskImage.useSpriteMesh = sourceImage.useSpriteMesh;
        maskImage.raycastTarget = false;
        maskImage.maskable = false;
        maskObject.GetComponent<Mask>().showMaskGraphic = false;
        return maskObject;
    }

    private static float RandomRange(System.Random random, float minimum, float maximum)
    {
        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }

    private static bool UpdatePiecePlacementLight(PiecePlacementLightFx effect, float elapsed)
    {
        if (effect?.Light?.Transform == null
            || effect.Light.Deformer == null
            || effect.AnimationVersion != effect.Light.AnimationVersion)
        {
            return false;
        }

        var progress = Mathf.Clamp01((elapsed - effect.Delay) / effect.Lifetime);
        if (elapsed < effect.Delay)
        {
            return true;
        }

        Vector2 bendOffset;
        float middleStretch;
        if (progress <= PiecePlacementLightPushPhaseRatio)
        {
            var pushProgress = Mathf.SmoothStep(
                0f,
                1f,
                progress / PiecePlacementLightPushPhaseRatio);
            bendOffset = Vector2.LerpUnclamped(
                effect.StartBendOffset,
                effect.BendDirection * effect.BendDistance,
                pushProgress);
            middleStretch = Mathf.LerpUnclamped(
                effect.StartMiddleStretch,
                PiecePlacementLightMiddleStretch,
                pushProgress);
        }
        else
        {
            var reboundProgress = Mathf.Clamp01(
                (progress - PiecePlacementLightPushPhaseRatio)
                / (1f - PiecePlacementLightPushPhaseRatio));
            var response = Mathf.Cos(reboundProgress * Mathf.PI * 3f)
                           * Mathf.Pow(1f - reboundProgress, 2f);
            bendOffset = effect.BendDirection * effect.BendDistance * response;
            middleStretch = Mathf.Abs(response) * PiecePlacementLightMiddleStretch;
        }

        effect.Light.Deformer.SetDeformation(bendOffset, middleStretch);
        return true;
    }

    private AmbientPieceLightFx FindActivePieceLight(int pieceNumber)
    {
        for (var i = _ambientPieceLights.Count - 1; i >= 0; i--)
        {
            var light = _ambientPieceLights[i];
            if (light != null
                && light.PieceNumber == pieceNumber
                && light.Transform != null)
            {
                return light;
            }
        }

        return null;
    }

    private int CountUnplacedTrayPieces()
    {
        var count = 0;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null && !state.IsPlaced && state.IsOnTray)
            {
                count++;
            }
        }

        return count;
    }

    private void UpdateLoosePieceReminder()
    {
        if (!ShouldRunLoosePieceReminder())
        {
            _nextLoosePieceReminderTime = -1f;
            StopLoosePieceReminderShake();
            return;
        }

        if (_nextLoosePieceReminderTime < 0f)
        {
            _nextLoosePieceReminderTime = Time.unscaledTime + LoosePieceReminderInterval;
            return;
        }

        if (_loosePieceReminderShakeCoroutine == null
            && Time.unscaledTime >= _nextLoosePieceReminderTime)
        {
            StartLoosePieceReminderShake();
            _nextLoosePieceReminderTime = Time.unscaledTime + LoosePieceReminderInterval;
        }
    }

    private bool ShouldRunLoosePieceReminder()
    {
        if (_isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating
            || _isPiecePlacementAnimating
            || _isTrayPieceReflowAnimating
            || _isHintPieceShaking
            || _drag.DraggingPiece != null
            || !IsPieceTrayHidden()
            || CountUnplacedTrayPieces() > 0)
        {
            return false;
        }

        return TryGetSingleLoosePiece(out _);
    }

    private bool TryGetSingleLoosePiece(out DraggablePieceState loosePiece)
    {
        loosePiece = null;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || state.IsOnTray
                || state.PieceRenderer == null)
            {
                continue;
            }

            if (loosePiece != null)
            {
                loosePiece = null;
                return false;
            }

            loosePiece = state;
        }

        return loosePiece != null;
    }

    private bool IsPieceTrayHidden()
    {
        return _board.PieceBoardRect != null
            ? _isPieceBoardHidden
            : _isPieceBgHidden;
    }

    private void StartLoosePieceReminderShake()
    {
        StopLoosePieceReminderShake();
        if (!TryGetSingleLoosePiece(out var state))
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_FinalPieceHint.mp3");
        _loosePieceReminderStates.Add(state);
        _loosePieceReminderBaseRotations.Add(state.PieceRenderer.transform.rotation);
        _loosePieceReminderShakeCoroutine = StartCoroutine(AnimateLoosePieceReminderShake());
    }

    private IEnumerator AnimateLoosePieceReminderShake()
    {
        var elapsed = 0f;
        while (elapsed < LoosePieceReminderShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var angle = Mathf.Sin(
                elapsed * LoosePieceReminderShakeCyclesPerSecond * Mathf.PI * 2f)
                * LoosePieceReminderShakeAngle;
            for (var i = 0; i < _loosePieceReminderStates.Count; i++)
            {
                var state = _loosePieceReminderStates[i];
                if (state?.PieceRenderer == null || state.IsPlaced || state.IsOnTray)
                {
                    continue;
                }

                state.PieceRenderer.transform.rotation =
                    _loosePieceReminderBaseRotations[i] * Quaternion.Euler(0f, 0f, angle);
            }

            yield return null;
        }

        RestoreLoosePieceReminderRotations();
        _loosePieceReminderShakeCoroutine = null;
    }

    private void StopLoosePieceReminderShake()
    {
        if (_loosePieceReminderShakeCoroutine != null)
        {
            StopCoroutine(_loosePieceReminderShakeCoroutine);
            _loosePieceReminderShakeCoroutine = null;
        }

        RestoreLoosePieceReminderRotations();
    }

    private void RestoreLoosePieceReminderRotations()
    {
        for (var i = 0; i < _loosePieceReminderStates.Count; i++)
        {
            var state = _loosePieceReminderStates[i];
            if (state?.PieceRenderer != null)
            {
                state.PieceRenderer.transform.rotation = _loosePieceReminderBaseRotations[i];
            }
        }

        _loosePieceReminderStates.Clear();
        _loosePieceReminderBaseRotations.Clear();
    }

    private Bounds GetPieceTrayBounds()
    {
        var camera = Camera.main;
        if (_board.PieceBoardRect != null && camera != null)
        {
            if (TryGetCanvasRectGameplayBounds(
                    _board.PieceBoardRect,
                    camera,
                    out var stableBounds))
            {
                return stableBounds;
            }

            return GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.PieceBoardRect,
                camera,
                WorldGameplayDepth);
        }

        if (_board.PieceBgRenderer != null)
        {
            return _board.PieceBgRenderer.bounds;
        }

        if (_board.GameBoardImage != null && camera != null)
        {
            return GameCommonUtility.GetRectTransformCameraWorldBounds(_board.GameBoardImage.rectTransform, camera);
        }

        return new Bounds(Vector3.zero, Vector3.one);
    }

    private float GetTrayHorizontalSpacingWorld(Bounds trayWorldBounds)
    {
        var fallbackSpacing = DraggableHorizontalSpacingPixels / PixelsPerUnit;
        var trayRect = _board.PieceBoardRect;
        var canvas = trayRect != null ? trayRect.GetComponentInParent<Canvas>() : null;
        var canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        if (trayRect == null
            || canvasRect == null
            || trayWorldBounds.size.x <= 0.0001f)
        {
            return fallbackSpacing;
        }

        var trayDesignBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            canvasRect,
            trayRect);
        var trayDesignWidth = Mathf.Abs(trayDesignBounds.size.x);
        if (trayDesignWidth <= 0.0001f)
        {
            return fallbackSpacing;
        }

        var spacing = DraggableHorizontalSpacingPixels
            * trayWorldBounds.size.x
            / trayDesignWidth;
        return spacing > 0f && !float.IsNaN(spacing) && !float.IsInfinity(spacing)
            ? spacing
            : fallbackSpacing;
    }

    private static bool TryGetCanvasRectGameplayBounds(
        RectTransform targetRect,
        Camera gameplayCamera,
        out Bounds gameplayBounds)
    {
        gameplayBounds = default;
        var canvas = targetRect != null ? targetRect.GetComponentInParent<Canvas>() : null;
        var canvasRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        if (targetRect == null
            || gameplayCamera == null
            || canvasRect == null
            || canvasRect.rect.width <= 0.001f
            || canvasRect.rect.height <= 0.001f
            || Screen.width <= 0
            || Screen.height <= 0)
        {
            return false;
        }

        var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            canvasRect,
            targetRect);
        var canvasLocalRect = canvasRect.rect;
        var viewport = gameplayCamera.pixelRect;
        if (viewport.width <= 0.001f || viewport.height <= 0.001f)
        {
            return false;
        }

        var screenMin = new Vector3(
            viewport.xMin
            + Mathf.InverseLerp(canvasLocalRect.xMin, canvasLocalRect.xMax, targetBounds.min.x)
                * viewport.width,
            viewport.yMin
            + Mathf.InverseLerp(canvasLocalRect.yMin, canvasLocalRect.yMax, targetBounds.min.y)
                * viewport.height,
            Mathf.Abs(gameplayCamera.transform.position.z - WorldGameplayDepth));
        var screenMax = new Vector3(
            viewport.xMin
            + Mathf.InverseLerp(canvasLocalRect.xMin, canvasLocalRect.xMax, targetBounds.max.x)
                * viewport.width,
            viewport.yMin
            + Mathf.InverseLerp(canvasLocalRect.yMin, canvasLocalRect.yMax, targetBounds.max.y)
                * viewport.height,
            screenMin.z);
        var worldMin = gameplayCamera.ScreenToWorldPoint(screenMin);
        var worldMax = gameplayCamera.ScreenToWorldPoint(screenMax);
        gameplayBounds = new Bounds(worldMin, Vector3.zero);
        gameplayBounds.Encapsulate(worldMax);
        gameplayBounds.center = new Vector3(
            gameplayBounds.center.x,
            gameplayBounds.center.y,
            WorldGameplayDepth);
        return gameplayBounds.size.x > 0.0001f && gameplayBounds.size.y > 0.0001f;
    }

    private void LayoutTrayPieces(
        bool animate = false,
        DraggablePieceState excludedState = null,
        HashSet<DraggablePieceState> excludedStates = null)
    {
        StopTrayPieceReflow();
        Canvas.ForceUpdateCanvases();
        var hostBounds = GetPieceTrayBounds();
        if (hostBounds.size.sqrMagnitude <= 0f)
        {
            return;
        }

        var horizontalSpacing = GetTrayHorizontalSpacingWorld(hostBounds);
        var nextCenterX = hostBounds.min.x + DraggableLeftPadding;
        var unplaced = new List<DraggablePieceState>();
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null
                || state == _drag.DraggingPiece)
            {
                continue;
            }

            unplaced.Add(state);
        }

        var trayCenterY = hostBounds.center.y;
        var animatedStates = animate ? new List<DraggablePieceState>() : null;
        var animatedTargets = animate ? new List<Vector3>() : null;
        for (var i = 0; i < unplaced.Count; i++)
        {
            var state = unplaced[i];
            var currentScale = state.PieceRenderer.transform.localScale;
            state.TrayScale = CalculateTrayScaleForPiece(
                state.PieceRenderer,
                hostBounds,
                state.DragScale);
            state.PieceRenderer.transform.localScale = state.TrayScale;
            var pieceWidth = GameCommonUtility.GetPieceWidth(
                state.PieceRenderer,
                state.TrayScale);
            var pieceHalfWidth = pieceWidth * 0.5f;
            var pieceCenterX = nextCenterX + pieceHalfWidth;
            var position = CalculateTrayPiecePosition(
                state.PieceRenderer,
                pieceCenterX,
                trayCenterY);
            state.StartPosition = position;
            var isExcluded = state == excludedState
                || (excludedStates != null && excludedStates.Contains(state));
            if (isExcluded)
            {
                state.PieceRenderer.transform.localScale = currentScale;
            }
            if (!animate)
            {
                state.PieceRenderer.transform.position = position;
            }
            else if (!isExcluded
                     && Vector3.SqrMagnitude(state.PieceRenderer.transform.position - position) > 0.000001f)
            {
                animatedStates.Add(state);
                animatedTargets.Add(position);
            }
            nextCenterX = pieceCenterX + pieceHalfWidth + horizontalSpacing;
        }

        StartTrayPieceReflow(animatedStates, animatedTargets);
    }

    private bool CompactFollowingTrayPieces(DraggablePieceState removedState)
    {
        if (removedState?.PieceRenderer == null)
        {
            return false;
        }

        var removedIndex = _drag.CurrentGroupDraggables.IndexOf(removedState);
        if (removedIndex < 0)
        {
            return false;
        }

        var trayBounds = GetPieceTrayBounds();
        removedState.TrayScale = CalculateTrayScaleForPiece(
            removedState.PieceRenderer,
            trayBounds,
            removedState.DragScale);
        var horizontalSpacing = GetTrayHorizontalSpacingWorld(trayBounds);
        var shiftX = GameCommonUtility.GetPieceWidth(
            removedState.PieceRenderer,
            removedState.TrayScale) + horizontalSpacing;
        var states = new List<DraggablePieceState>();
        var targets = new List<Vector3>();
        for (var i = removedIndex + 1; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null
                || state == _drag.DraggingPiece)
            {
                continue;
            }

            var target = state.PieceRenderer.transform.position;
            target.x -= shiftX;
            state.StartPosition = target;
            states.Add(state);
            targets.Add(target);
        }

        StartTrayPieceReflow(states, targets);
        return states.Count > 0;
    }

    private static Vector3 CalculateTrayPiecePosition(
        SpriteRenderer renderer,
        float centerX,
        float trayCenterY)
    {
        var spriteCenter = renderer != null && renderer.sprite != null
            ? renderer.sprite.bounds.center
            : Vector3.zero;
        var localCenterOffset = Vector3.Scale(
            spriteCenter,
            renderer != null ? renderer.transform.localScale : Vector3.one);
        var parent = renderer != null ? renderer.transform.parent : null;
        var renderedCenterOffset = parent != null
            ? parent.TransformVector(localCenterOffset)
            : localCenterOffset;
        return new Vector3(
            centerX - renderedCenterOffset.x,
            trayCenterY - renderedCenterOffset.y,
            WorldGameplayDepth);
    }

    private void StartTrayPieceReflow(
        List<DraggablePieceState> states,
        List<Vector3> targets)
    {
        if (states == null || targets == null || states.Count == 0 || states.Count != targets.Count)
        {
            return;
        }

        var starts = new List<Vector3>(states.Count);
        for (var i = 0; i < states.Count; i++)
        {
            starts.Add(states[i].PieceRenderer.transform.position);
        }

        _trayPieceReflowCoroutine = StartCoroutine(
            AnimateTrayPieceReflow(states, starts, targets));
    }

    private IEnumerator AnimateTrayPieceReflow(
        List<DraggablePieceState> states,
        List<Vector3> starts,
        List<Vector3> targets)
    {
        _isTrayPieceReflowAnimating = true;
        var elapsed = 0f;
        while (elapsed < TrayPieceReflowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / TrayPieceReflowDuration));
            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state?.PieceRenderer == null || state.IsPlaced || !state.IsOnTray)
                {
                    continue;
                }

                state.PieceRenderer.transform.position = Vector3.LerpUnclamped(
                    starts[i],
                    targets[i],
                    progress);
            }

            yield return null;
        }

        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state?.PieceRenderer != null && !state.IsPlaced && state.IsOnTray)
            {
                state.PieceRenderer.transform.position = targets[i];
            }
        }

        _isTrayPieceReflowAnimating = false;
        _trayPieceReflowCoroutine = null;
    }

    private void StopTrayPieceReflow()
    {
        if (_trayPieceReflowCoroutine != null)
        {
            StopCoroutine(_trayPieceReflowCoroutine);
            _trayPieceReflowCoroutine = null;
        }

        _isTrayPieceReflowAnimating = false;
    }

    private static Vector3 GetGrooveSnapPosition(RectTransform grooveRect, Camera camera)
    {
        if (grooveRect == null)
        {
            return Vector3.zero;
        }

        if (camera == null)
        {
            var position = grooveRect.position;
            position.z = WorldGameplayDepth;
            return position;
        }

        var worldPosition = GameCommonUtility.RectTransformToCameraWorld(
            grooveRect,
            camera,
            WorldGameplayDepth);
        worldPosition.z = WorldGameplayDepth;
        return worldPosition;
    }

    private static float CalculateSnapDistance(DraggablePieceState state)
    {
        if (state == null)
        {
            return SnapDistanceMin;
        }

        var referenceSize = 0f;
        if (state.GrooveRect != null)
        {
            var grooveBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                state.GrooveRect,
                Camera.main,
                WorldGameplayDepth);
            referenceSize = Mathf.Max(grooveBounds.size.x, grooveBounds.size.y);
        }

        if (referenceSize <= 0f && state.PieceRenderer != null)
        {
            referenceSize = Mathf.Max(state.PieceRenderer.bounds.size.x, state.PieceRenderer.bounds.size.y);
        }

        if (referenceSize <= 0f)
        {
            return SnapDistanceMin;
        }

        var adaptiveDistance = referenceSize * SnapDistanceSizeRatio;
        return Mathf.Clamp(adaptiveDistance, SnapDistanceMin, SnapDistanceMax);
    }

    private bool TryAdvanceGroup()
    {
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            if (!_drag.CurrentGroupDraggables[i].IsPlaced)
            {
                return false;
            }
        }

        var nextGroupIndex = _drag.CurrentGroupIndex + 1;
        if (_board.GrooveImagesByGroup != null && nextGroupIndex < _board.GrooveImagesByGroup.Count)
        {
            MarkCurrentPackInProgress();
            StartCoroutine(PlayGroupTransition(nextGroupIndex));
            return true;
        }

        AudioManager.Instance.PlaySfx("SFX_PuzzleComplete.mp3");
        ShowRewardPanel();
        return true;
    }

    private IEnumerator PlayGroupTransition(int nextGroupIndex)
    {
        _isGroupTransitionAnimating = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = false;
        }
#endif
        var wasTutorialActive = IsTutorialActive;
        var transitionHoldDuration = _tutorialStage == TutorialStage.StrongPlacement
            ? GroupTransitionStrongHoldDuration
            : GroupTransitionDefaultHoldDuration;
        if (transitionHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(transitionHoldDuration);
        }

        AudioManager.Instance.PlaySfx("SFX_RegionMove.mp3");
        HidePiecePlacementTutorialPresentation();

        var camera = Camera.main;
        var boardRect = _loadedCardBagRect;
        var boardStart = boardRect != null ? boardRect.anchoredPosition : Vector2.zero;
        var cameraStartSize = camera != null ? camera.orthographicSize : 0f;
        var trayRect = _board.PieceBoardRect;
        var trayStart = trayRect != null ? trayRect.anchoredPosition : Vector2.zero;
        var pieceBackground = _board.PieceBgRenderer;
        var pieceBackgroundStart = pieceBackground != null
            ? pieceBackground.transform.position
            : Vector3.zero;
        var pieceBgFillTransform = GetPieceBgFillTransform();

        CreateDraggableGroup(
            nextGroupIndex,
            allowOutlineDuringTutorialTransition: wasTutorialActive);

        var boardTarget = boardRect != null ? boardRect.anchoredPosition : boardStart;
        var cameraTargetSize = camera != null ? camera.orthographicSize : cameraStartSize;
        var trayTarget = trayRect != null ? trayRect.anchoredPosition : trayStart;
        var pieceBackgroundTarget = pieceBackground != null
            ? pieceBackground.transform.position
            : pieceBackgroundStart;
        var pieceCount = _drag.CurrentGroupDraggables.Count;
        var pieceTargets = new Vector3[pieceCount];
        var pieceStarts = new Vector3[pieceCount];
        var pieceTargetScales = new Vector3[pieceCount];
        var pieceTargetRotations = new Quaternion[pieceCount];
        var pieceTargetColors = new Color[pieceCount];
        var activeGroupBounds = BuildActiveGroupBounds(nextGroupIndex);
        var pieceSourceCenter = activeGroupBounds.size.sqrMagnitude > 0f
            ? activeGroupBounds.center
            : Vector3.zero;

        for (var i = 0; i < pieceCount; i++)
        {
            var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = renderer.transform.localScale;
            pieceTargetRotations[i] = renderer.transform.rotation;
            pieceTargetColors[i] = renderer.color;
            var angle = i * 137.5f * Mathf.Deg2Rad;
            var radius = 0.08f + (i % 3) * 0.055f;
            pieceStarts[i] = pieceSourceCenter + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f);
            pieceStarts[i].z = pieceTargets[i].z;
            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i];
            var hiddenColor = pieceTargetColors[i];
            hiddenColor.a = 0f;
            renderer.color = hiddenColor;
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardStart;
        }

        if (camera != null)
        {
            camera.orthographicSize = cameraStartSize;
        }

        if (trayRect != null)
        {
            ApplyPieceBoardSlidePosition(trayStart, pieceBgFillTransform);
        }
        else if (pieceBackground != null)
        {
            ApplyPieceBgSlidePosition(pieceBackgroundStart, pieceBgFillTransform);
        }

        var elapsed = 0f;
        while (elapsed < GroupTransitionBoardDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            var progress = SmootherStep01(elapsed / GroupTransitionBoardDuration);
            if (boardRect != null)
            {
                boardRect.anchoredPosition = Vector2.LerpUnclamped(boardStart, boardTarget, progress);
            }

            if (camera != null)
            {
                camera.orthographicSize = Mathf.LerpUnclamped(
                    cameraStartSize,
                    cameraTargetSize,
                    progress);
            }

            var trayProgress = SmootherStep01(
                (elapsed - GroupTransitionBoardDuration * 0.18f)
                / (GroupTransitionBoardDuration * 0.72f));
            if (trayRect != null)
            {
                ApplyPieceBoardSlidePosition(
                    Vector2.LerpUnclamped(trayStart, trayTarget, trayProgress),
                    pieceBgFillTransform);
            }
            else if (pieceBackground != null)
            {
                ApplyPieceBgSlidePosition(
                    Vector3.LerpUnclamped(
                        pieceBackgroundStart,
                        pieceBackgroundTarget,
                        trayProgress),
                    pieceBgFillTransform);
            }

            yield return null;
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardTarget;
        }

        if (camera != null)
        {
            camera.orthographicSize = cameraTargetSize;
        }

        if (trayRect != null)
        {
            ApplyPieceBoardSlidePosition(trayTarget, pieceBgFillTransform);
        }
        else if (pieceBackground != null)
        {
            ApplyPieceBgSlidePosition(pieceBackgroundTarget, pieceBgFillTransform);
        }

        RefreshCurrentGroupTrayScalesAndLayout();
        for (var i = 0; i < pieceCount; i++)
        {
            var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = renderer.transform.localScale;
            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i];
            renderer.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 12f);
            var hiddenColor = pieceTargetColors[i];
            hiddenColor.a = 0f;
            renderer.color = hiddenColor;
        }

        FadeInActiveGroupOutline();

        elapsed = 0f;
        var pieceAnimationDuration = GroupTransitionPieceDuration
            + Mathf.Max(0, pieceCount - 1) * GroupTransitionPieceStagger;
        while (elapsed < pieceAnimationDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            for (var i = 0; i < pieceCount; i++)
            {
                var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
                if (renderer == null)
                {
                    continue;
                }

                var progress = SmootherStep01(
                    (elapsed - i * GroupTransitionPieceStagger) / GroupTransitionPieceDuration);
                renderer.transform.position = Vector3.LerpUnclamped(
                    pieceStarts[i],
                    pieceTargets[i],
                    progress);
                renderer.transform.localScale = pieceTargetScales[i];
                renderer.transform.rotation = Quaternion.SlerpUnclamped(
                    Quaternion.Euler(0f, 0f, Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 12f),
                    pieceTargetRotations[i],
                    progress);
                var color = pieceTargetColors[i];
                color.a *= progress;
                renderer.color = color;
                if (progress >= 1f)
                {
                    EnsureDraggablePieceLight(_drag.CurrentGroupDraggables[i]);
                }
            }

            yield return null;
        }

        for (var i = 0; i < pieceCount; i++)
        {
            var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            renderer.transform.position = pieceTargets[i];
            renderer.transform.localScale = pieceTargetScales[i];
            renderer.transform.rotation = pieceTargetRotations[i];
            renderer.color = pieceTargetColors[i];
            EnsureDraggablePieceLight(_drag.CurrentGroupDraggables[i]);
        }

        if (wasTutorialActive)
        {
            yield return new WaitForSecondsRealtime(GroupTransitionPromptDelay);
            EnterTutorialStageForGroup(nextGroupIndex);
        }

        _isGroupTransitionAnimating = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = !_isGameFinished;
        }
#endif
    }

    private static float SmootherStep01(float value)
    {
        var t = Mathf.Clamp01(value);
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    private static void MarkCurrentPackInProgress()
    {
        var packId = GameManager.GetBagId();
        if (packId <= 0 || CardPackDataUtility.TryMarkPackInProgress(packId))
        {
            return;
        }

        Debug.LogWarning($"GameScene: failed to mark card pack in progress. packId={packId}");
    }

    private void ConfigureRewardPanel()
    {
        _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: reward panel not found. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        CacheRewardPanelReferences();
        SetSettlementInputLocked(true);
        _rewardPanelRoot.SetActive(false);
        _isGameFinished = false;

        var finishButtonObject = GameCommonUtility.FindSceneObject(GameDefine.FinishButtonObjectName);
        if (finishButtonObject == null)
        {
            Debug.LogWarning($"GameScene: finish button not found. Expected object named {GameDefine.FinishButtonObjectName}.");
            return;
        }

        _finishButton = finishButtonObject.GetComponent<Button>();
        if (_finishButton == null)
        {
            Debug.LogWarning($"GameScene: {GameDefine.FinishButtonObjectName} is missing Button component.");
            return;
        }

        _finishButton.interactable = false;
        _finishButton.onClick.RemoveListener(OnFinishButtonClicked);
        _finishButton.onClick.AddListener(OnFinishButtonClicked);

        if (_settlementCameraButton != null)
        {
            _settlementCameraButton.onClick.RemoveListener(OnSettlementCameraClicked);
            _settlementCameraButton.onClick.AddListener(OnSettlementCameraClicked);
        }

        var packPhotoItem = GameCommonUtility.FindSceneObject(PackPhotoItemObjectName);
        _cardPackPhoto = packPhotoItem != null
            ? packPhotoItem.GetComponent<CardPackPhoto>()
            : null;
        if (_cardPackPhoto == null || !_cardPackPhoto.Initialize())
        {
            Debug.LogWarning(
                "GameScene: reusable PackPhotoItem prefab is missing or incomplete.");
        }
    }

    private void CacheRewardPanelReferences()
    {
        _rewardPanelCanvasGroup = null;
        _rewardTaskItem = null;
        _settlementSummaryRect = null;
        _rewardTaskItemRect = null;
        _settlementRewardBagRect = null;
        _settlementFinishButtonRect = null;
        _settlementCameraButtonRect = null;
        _settlementCameraButton = null;
        _settlementBagCountTitleText = null;
        _settlementBonusTitleText = null;
        _settlementBonusScoreText = null;
        _settlementScoreText = null;
        _settlementBagCountText = null;
        _taskRewardImage = null;
        _secondaryRewardImage = null;
        _taskRewardSourceCircleImage = null;
        _taskRewardSourceImage = null;
        _taskRewardSourceCountBackgroundImage = null;
        _taskRewardSourceCountText = null;
        _settlementRewardRevealEffect = null;
        _secondarySettlementRewardRevealEffect = null;
        if (_rewardPanelRoot == null)
        {
            return;
        }

        _rewardPanelCanvasGroup = _rewardPanelRoot.GetComponent<CanvasGroup>();
        if (_rewardPanelCanvasGroup == null)
        {
            _rewardPanelCanvasGroup = _rewardPanelRoot.AddComponent<CanvasGroup>();
        }

        _rewardTaskItem = _rewardPanelRoot.transform.Find(TaskItemObjectName);
        _settlementSummaryRect = _rewardPanelRoot.transform.Find(TaskSummaryObjectName) as RectTransform;
        _rewardTaskItemRect = _rewardTaskItem as RectTransform;
        _settlementRewardBagRect = _rewardPanelRoot.transform.Find(
            TaskRewardBagRootObjectName) as RectTransform;
        _settlementFinishButtonRect = _rewardPanelRoot.transform.Find(
            GameDefine.FinishButtonObjectName) as RectTransform;
        _settlementCameraButtonRect = _rewardPanelRoot.transform.Find(
            SettlementCameraButtonObjectName) as RectTransform;
        _settlementCameraButton = _settlementCameraButtonRect != null
            ? _settlementCameraButtonRect.GetComponent<Button>()
            : null;
        _settlementBagCountTitleText = _rewardPanelRoot.transform.Find(TaskBagCountTitlePath)?.GetComponent<TMP_Text>();
        _settlementBonusTitleText = _rewardPanelRoot.transform.Find(TaskBonusTitlePath)?.GetComponent<TMP_Text>();
        _settlementBonusScoreText = _rewardPanelRoot.transform.Find(TaskBonusScorePath)?.GetComponent<TMP_Text>();
        _settlementScoreText = _rewardPanelRoot.transform.Find(TaskScorePath)?.GetComponent<TMP_Text>();
        _settlementBagCountText = _rewardPanelRoot.transform.Find(TaskBagCountPath)?.GetComponent<TMP_Text>();
        var rewardItemCanvas = _rewardPanelRoot.transform.Find(TaskRewardItemCanvasPath);
        if (rewardItemCanvas != null)
        {
            rewardItemCanvas.localScale = Vector3.one;
        }

        var secondaryRewardItemCanvas = _rewardPanelRoot.transform.Find(
            SecondaryRewardItemCanvasPath);
        if (secondaryRewardItemCanvas != null)
        {
            secondaryRewardItemCanvas.localScale = Vector3.one;
        }

        _taskRewardImage = _rewardPanelRoot.transform.Find(TaskRewardImgBagPath)?.GetComponent<Image>();
        _secondaryRewardImage = _rewardPanelRoot.transform.Find(
            SecondaryRewardImgBagPath)?.GetComponent<Image>();
        if (_rewardTaskItem != null)
        {
            _taskRewardSourceCircleImage = _rewardTaskItem.Find(
                TaskRewardSourceRootPath)?.GetComponent<Image>();
            _taskRewardSourceImage = _rewardTaskItem.Find(
                TaskRewardSourceIconPath)?.GetComponent<Image>();
            _taskRewardSourceCountBackgroundImage = _rewardTaskItem.Find(
                TaskRewardSourceCountBackgroundPath)?.GetComponent<Image>();
            _taskRewardSourceCountText = _rewardTaskItem.Find(
                TaskRewardSourceCountPath)?.GetComponent<TMP_Text>();
        }

        if (!_hasTaskRewardSourceDefaultColors && _taskRewardSourceImage != null)
        {
            _taskRewardSourceCircleDefaultColor = _taskRewardSourceCircleImage != null
                ? _taskRewardSourceCircleImage.color
                : Color.white;
            _taskRewardSourceDefaultColor = _taskRewardSourceImage.color;
            _taskRewardSourceCountBackgroundDefaultColor =
                _taskRewardSourceCountBackgroundImage != null
                    ? _taskRewardSourceCountBackgroundImage.color
                    : Color.white;
            _taskRewardSourceCountDefaultColor = _taskRewardSourceCountText != null
                ? _taskRewardSourceCountText.color
                : Color.white;
            _hasTaskRewardSourceDefaultColors = true;
        }

        _settlementRewardRevealEffect = FindSettlementRewardRevealEffect(
            rewardItemCanvas);
        _secondarySettlementRewardRevealEffect = FindSettlementRewardRevealEffect(
            secondaryRewardItemCanvas);
        StopAndHideSettlementRewardRevealEffect();
        if (!_hasSettlementLayoutTargets
            && _settlementSummaryRect != null
            && _rewardTaskItemRect != null
            && _settlementRewardBagRect != null
            && _taskRewardImage != null)
        {
            _settlementSummaryTargetPosition = _settlementSummaryRect.anchoredPosition;
            _rewardTaskItemTargetPosition = _rewardTaskItemRect.anchoredPosition;
            _settlementRewardBagTargetPosition = _settlementRewardBagRect.anchoredPosition;
            _taskRewardImageTargetPosition = _taskRewardImage.rectTransform.anchoredPosition;
            if (_settlementFinishButtonRect != null)
            {
                _settlementFinishButtonTargetPosition =
                    _settlementFinishButtonRect.anchoredPosition;
            }

            if (_settlementCameraButtonRect != null)
            {
                _settlementCameraButtonTargetPosition =
                    _settlementCameraButtonRect.anchoredPosition;
            }

            _taskRewardDefaultSprite = _taskRewardImage.sprite;
            _taskRewardDefaultColor = _taskRewardImage.color;
            _hasSettlementLayoutTargets = true;
        }

        if (_rewardTaskItem == null)
        {
            Debug.LogWarning($"GameScene: task reward UI not found. Expected {TaskItemObjectName} under RewardPanel.");
        }

        if (_settlementScoreText == null)
        {
            Debug.LogWarning($"GameScene: settlement score text not found. Expected {TaskScorePath}.");
        }
        else
        {
            GameFontUtility.ApplyDefaultFont(_settlementScoreText);
        }

        if (_settlementBagCountTitleText == null)
        {
            Debug.LogWarning($"GameScene: settlement bag count title not found. Expected {TaskBagCountTitlePath}.");
        }

        if (_settlementBonusTitleText == null)
        {
            Debug.LogWarning($"GameScene: settlement bonus title not found. Expected {TaskBonusTitlePath}.");
        }
        else
        {
            ConfigureSettlementSingleLineText(_settlementBonusTitleText);
        }

        if (_settlementBonusScoreText == null)
        {
            Debug.LogWarning($"GameScene: settlement bonus score not found. Expected {TaskBonusScorePath}.");
        }
        else
        {
            ConfigureSettlementSingleLineText(_settlementBonusScoreText);
        }

        HideSettlementTitles();

        if (_settlementBagCountText == null)
        {
            Debug.LogWarning($"GameScene: card pack count text not found. Expected {TaskBagCountPath}.");
        }
        else
        {
            GameFontUtility.ApplyDefaultFont(_settlementBagCountText);
        }

        if (_taskRewardImage == null)
        {
            Debug.LogWarning(
                $"GameScene: settlement reward cover not found. Expected {TaskRewardImgBagPath}.");
        }

        if (_settlementRewardRevealEffect == null)
        {
            Debug.LogWarning(
                $"GameScene: settlement reward reveal effect not found. "
                + $"Expected {TaskRewardRevealEffectPath}.");
        }

        if (_secondaryRewardImage == null
            || _secondarySettlementRewardRevealEffect == null)
        {
            Debug.LogWarning(
                "GameScene: secondary authored BagRewardItem is incomplete. "
                + $"Expected {SecondaryRewardImgBagPath} and "
                + $"{SecondaryRewardRevealEffectPath}.");
        }
    }

    private void PrepareSettlementVisualState()
    {
        _completionRewardDisplayImage = null;
        _taskRewardDisplayImage = null;
        StopAndHideSettlementRewardRevealEffect();
        SetSettlementScore(0);
        SetSettlementBagCount(_settlementBagCountBeforeCompletion);
        HideSettlementTitles();

        if (_settlementSummaryRect != null)
        {
            _settlementSummaryRect.gameObject.SetActive(true);
            _settlementSummaryRect.localScale = Vector3.one;
            _settlementSummaryRect.anchoredPosition = _settlementSummaryTargetPosition;
        }

        var task = default(TaskInstanceData);
        var showTask = _isTaskTrackingActive
                       && GameTaskUtility.TryGetCurrentTask(out task);
        SetTaskRewardSectionVisible(showTask);
        if (showTask && _rewardTaskItem != null)
        {
            TaskProgressUIUtility.RefreshTask(
                _rewardTaskItem,
                task,
                GameTaskUtility.GetCurrentCompleteValue(),
                GameTaskUtility.IsCurrentTaskCompleted());
            RestoreTaskRewardSourceVisuals();
        }

        if (_rewardTaskItemRect != null)
        {
            _rewardTaskItemRect.localScale = Vector3.one;
            _rewardTaskItemRect.anchoredPosition = _rewardTaskItemTargetPosition;
        }

        if (_settlementRewardBagRect != null)
        {
            _settlementRewardBagRect.anchoredPosition = _settlementRewardBagTargetPosition;
            _settlementRewardBagRect.localScale = Vector3.one;
            _settlementRewardBagRect.gameObject.SetActive(false);
        }

        PrepareSettlementActionButton(
            _settlementFinishButtonRect,
            _settlementFinishButtonTargetPosition);
        PrepareSettlementActionButton(
            _settlementCameraButtonRect,
            _settlementCameraButtonTargetPosition);
        if (_settlementCameraButton != null)
        {
            _settlementCameraButton.interactable = false;
        }

        if (_taskRewardImage != null)
        {
            _taskRewardImage.sprite = _taskRewardDefaultSprite;
            _taskRewardImage.color = _taskRewardDefaultColor;
            _taskRewardImage.rectTransform.anchoredPosition = _taskRewardImageTargetPosition;
            _taskRewardImage.rectTransform.localScale = Vector3.one;
            _taskRewardImage.gameObject.SetActive(false);
        }

        if (_secondaryRewardImage != null)
        {
            _secondaryRewardImage.sprite = _taskRewardDefaultSprite;
            _secondaryRewardImage.color = _taskRewardDefaultColor;
            _secondaryRewardImage.rectTransform.anchoredPosition =
                _taskRewardImageTargetPosition;
            _secondaryRewardImage.rectTransform.localScale = Vector3.one;
            _secondaryRewardImage.gameObject.SetActive(false);
        }

        Canvas.ForceUpdateCanvases();
        if (_settlementSummaryRect != null)
        {
            _settlementSummaryRect.anchoredPosition = CalculateSettlementOffscreenPosition(
                _settlementSummaryRect,
                _settlementSummaryTargetPosition,
                above: true);
        }

        if (_rewardTaskItemRect != null && _rewardTaskItemRect.gameObject.activeSelf)
        {
            _rewardTaskItemRect.anchoredPosition = CalculateSettlementOffscreenPosition(
                _rewardTaskItemRect,
                _rewardTaskItemTargetPosition,
                above: true);
        }
    }

    private static void PrepareSettlementActionButton(
        RectTransform buttonRect,
        Vector2 targetPosition)
    {
        if (buttonRect == null)
        {
            return;
        }

        buttonRect.gameObject.SetActive(true);
        buttonRect.anchoredPosition = CalculateSettlementOffscreenPosition(
            buttonRect,
            targetPosition,
            above: false);
    }

    private IEnumerator AnimateSettlementHeaderEntrance()
    {
        var summaryStart = _settlementSummaryRect != null
            ? _settlementSummaryRect.anchoredPosition
            : Vector2.zero;
        var taskStart = _rewardTaskItemRect != null
            ? _rewardTaskItemRect.anchoredPosition
            : Vector2.zero;
        var animateTask = _rewardTaskItemRect != null
                          && _rewardTaskItemRect.gameObject.activeSelf;
        var elapsed = 0f;
        while (elapsed < SettlementHeaderDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementHeaderDropDuration);
            Vector2 AnimateDrop(Vector2 start, Vector2 target)
            {
                var overshoot = target + Vector2.down * SettlementHeaderDropOvershoot;
                if (normalized < SettlementHeaderDropTravelRatio)
                {
                    var travel = Mathf.Clamp01(normalized / SettlementHeaderDropTravelRatio);
                    var easedTravel = 1f - Mathf.Pow(1f - travel, 3f);
                    return Vector2.LerpUnclamped(start, overshoot, easedTravel);
                }

                var settle = Mathf.InverseLerp(
                    SettlementHeaderDropTravelRatio,
                    1f,
                    normalized);
                return Vector2.LerpUnclamped(
                    overshoot,
                    target,
                    Mathf.SmoothStep(0f, 1f, settle));
            }

            if (_settlementSummaryRect != null)
            {
                _settlementSummaryRect.anchoredPosition = AnimateDrop(
                    summaryStart,
                    _settlementSummaryTargetPosition);
            }

            if (animateTask)
            {
                _rewardTaskItemRect.anchoredPosition = AnimateDrop(
                    taskStart,
                    _rewardTaskItemTargetPosition);
            }

            yield return null;
        }

        if (_settlementSummaryRect != null)
        {
            _settlementSummaryRect.anchoredPosition = _settlementSummaryTargetPosition;
        }

        if (animateTask)
        {
            _rewardTaskItemRect.anchoredPosition = _rewardTaskItemTargetPosition;
        }
    }

    private static Vector2 CalculateSettlementOffscreenPosition(
        RectTransform rect,
        Vector2 target,
        bool above)
    {
        var parentRect = rect != null ? rect.parent as RectTransform : null;
        if (rect == null || parentRect == null)
        {
            return target;
        }

        var anchorY = Mathf.Lerp(
            parentRect.rect.yMin,
            parentRect.rect.yMax,
            rect.anchorMin.y);
        var scaledHeight = rect.rect.height * Mathf.Abs(rect.localScale.y);
        var pivotLocalY = above
            ? parentRect.rect.yMax + scaledHeight * rect.pivot.y + SettlementOffscreenMargin
            : parentRect.rect.yMin
              - scaledHeight * (1f - rect.pivot.y)
              - SettlementOffscreenMargin;
        return new Vector2(target.x, pivotLocalY - anchorY);
    }

    private IEnumerator AnimateSettlementBagCountIncrement()
    {
        ShowSettlementBagCountTitle();
        if (_settlementBagCountText == null)
        {
            yield break;
        }

        var finalCount = _settlementBagCountAfterCompletion;
        var incrementObject = Instantiate(
            _settlementBagCountText.gameObject,
            _settlementBagCountText.transform.parent,
            false);
        incrementObject.name = "TaskBagNumIncrement";
        incrementObject.transform.SetAsLastSibling();
        var incrementText = incrementObject.GetComponent<TMP_Text>();
        var incrementRect = incrementObject.GetComponent<RectTransform>();
        incrementText.text = "+1";
        incrementText.fontSize = _settlementBagCountTitleText != null
            ? _settlementBagCountTitleText.fontSize
            : _settlementBagCountText.fontSize;
        incrementText.enableAutoSizing = false;
        incrementText.raycastTarget = false;
        incrementText.alpha = 0f;
        var countRect = _settlementBagCountText.rectTransform;
        var startPosition = countRect.anchoredPosition + new Vector2(
            countRect.rect.width * 0.45f,
            countRect.rect.height * 0.35f);
        var endPosition = startPosition + Vector2.up * SettlementBagCountIncrementRise;
        incrementRect.anchoredPosition = startPosition;
        incrementRect.localScale = Vector3.one * 0.8f;

        var elapsed = 0f;
        while (elapsed < SettlementBagCountIncrementDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementBagCountIncrementDuration);
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            incrementRect.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                endPosition,
                eased);
            incrementRect.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, eased);
            incrementText.alpha = normalized < 0.2f
                ? normalized / 0.2f
                : normalized > 0.65f
                    ? 1f - (normalized - 0.65f) / 0.35f
                    : 1f;
            SetSettlementBagCount(Mathf.RoundToInt(Mathf.Lerp(
                _settlementBagCountBeforeCompletion,
                finalCount,
                eased)));
            yield return null;
        }

        SetSettlementBagCount(finalCount);
        Destroy(incrementObject);
    }

    private IEnumerator AnimateSettlementPackRewards()
    {
        if (_settlementRewardBagRect == null || _taskRewardImage == null)
        {
            yield break;
        }

        PrepareSettlementRewardSlots();
        AudioManager.Instance.PlaySfx("SFX_CardPackAppear.mp3");
        _settlementRewardBagRect.gameObject.SetActive(true);
        _settlementRewardBagRect.anchoredPosition = CalculateSettlementOffscreenPosition(
            _settlementRewardBagRect,
            _settlementRewardBagTargetPosition,
            above: false);
        var panelComplete = false;
        var panelAnimation = StartCoroutine(RunSettlementAnimation(
            AnimateSettlementRewardPanelEntrance(),
            () => panelComplete = true));

        yield return new WaitForSecondsRealtime(SettlementRewardAnimationLead);

        var completionRewardComplete = _completionRewardDisplayImage == null;
        var completionRewardTargetPosition = _completionRewardDisplayImage != null
            ? _completionRewardDisplayImage.rectTransform.anchoredPosition
            : Vector2.zero;
        Coroutine completionRewardAnimation = null;
        if (_completionRewardDisplayImage != null)
        {
            completionRewardAnimation = StartCoroutine(RunSettlementAnimation(
                AnimateSettlementRewardPop(_completionRewardDisplayImage.rectTransform),
                () => completionRewardComplete = true));
        }

        var taskRewardComplete = _taskRewardDisplayImage == null;
        var taskRewardTargetPosition = _taskRewardDisplayImage != null
            ? _taskRewardDisplayImage.rectTransform.anchoredPosition
            : Vector2.zero;
        Coroutine taskRewardAnimation = null;
        if (_taskRewardDisplayImage != null)
        {
            if (_completionRewardDisplayImage != null)
            {
                yield return new WaitForSecondsRealtime(SettlementRewardAnimationStagger);
            }

            taskRewardAnimation = StartCoroutine(RunSettlementAnimation(
                AnimateTaskRewardIntoSettlementSlot(_taskRewardDisplayImage),
                () => taskRewardComplete = true));
        }

        var timeout = SettlementRewardAnimationLead
                      + (_completionRewardDisplayImage != null
                         && _taskRewardDisplayImage != null
                          ? SettlementRewardAnimationStagger
                          : 0f)
                      + Mathf.Max(
                          SettlementRewardPanelSlideDuration,
                          Mathf.Max(
                              SettlementRewardPopDuration,
                              SettlementTaskRewardFlyDuration))
                      + SettlementRewardAnimationTimeoutPadding;
        var waitStartedAt = Time.realtimeSinceStartup;
        while (!panelComplete || !completionRewardComplete || !taskRewardComplete)
        {
            if (Time.realtimeSinceStartup - waitStartedAt >= timeout)
            {
                if (!panelComplete)
                {
                    StopSettlementAnimation(panelAnimation);
                }

                if (!completionRewardComplete)
                {
                    StopSettlementAnimation(completionRewardAnimation);
                }

                if (!taskRewardComplete)
                {
                    StopSettlementAnimation(taskRewardAnimation);
                }

                Debug.LogWarning(
                    "GameScene: settlement reward animation timed out; "
                    + "restored the final reward state and continued settlement.");
                break;
            }

            yield return null;
        }

        RestoreSettlementRewardAnimationFinalState(
            _completionRewardDisplayImage,
            completionRewardTargetPosition,
            _taskRewardDisplayImage,
            taskRewardTargetPosition);
    }

    private void StopSettlementAnimation(Coroutine animation)
    {
        if (animation != null)
        {
            StopCoroutine(animation);
        }
    }

    private void RestoreSettlementRewardAnimationFinalState(
        Image completionReward,
        Vector2 completionTargetPosition,
        Image taskReward,
        Vector2 taskTargetPosition)
    {
        if (_settlementRewardBagRect != null)
        {
            _settlementRewardBagRect.gameObject.SetActive(true);
            _settlementRewardBagRect.anchoredPosition = _settlementRewardBagTargetPosition;
            _settlementRewardBagRect.localScale = Vector3.one;
        }

        RestoreSettlementReward(completionReward, completionTargetPosition);
        RestoreSettlementReward(taskReward, taskTargetPosition);
        if (taskReward != null)
        {
            if (_taskRewardSourceImage != null)
            {
                _taskRewardSourceImage.gameObject.SetActive(false);
            }

            SetGraphicColorAlpha(
                _taskRewardSourceCircleImage,
                _taskRewardSourceCircleDefaultColor,
                0f);
            SetGraphicColorAlpha(
                _taskRewardSourceCountBackgroundImage,
                _taskRewardSourceCountBackgroundDefaultColor,
                0f);
            SetGraphicColorAlpha(
                _taskRewardSourceCountText,
                _taskRewardSourceCountDefaultColor,
                0f);
        }
    }

    private static void RestoreSettlementReward(Image reward, Vector2 targetPosition)
    {
        if (reward == null)
        {
            return;
        }

        reward.gameObject.SetActive(true);
        reward.rectTransform.anchoredPosition = targetPosition;
        reward.rectTransform.localScale = Vector3.one;
        reward.rectTransform.localRotation = Quaternion.identity;
    }

    private IEnumerator AnimateSettlementRewardPanelEntrance()
    {
        var startPosition = _settlementRewardBagRect.anchoredPosition;
        var elapsed = 0f;
        while (elapsed < SettlementRewardPanelSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementRewardPanelSlideDuration);
            var eased = 1f - Mathf.Pow(1f - normalized, 3f);
            _settlementRewardBagRect.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                _settlementRewardBagTargetPosition,
                eased);
            yield return null;
        }

        _settlementRewardBagRect.anchoredPosition = _settlementRewardBagTargetPosition;
    }

    private static IEnumerator RunSettlementAnimation(
        IEnumerator animation,
        Action onComplete)
    {
        yield return animation;
        onComplete?.Invoke();
    }

    private IEnumerator AnimateSettlementActionButtonsEntrance()
    {
        var finishStart = _settlementFinishButtonRect != null
            ? _settlementFinishButtonRect.anchoredPosition
            : Vector2.zero;
        var cameraStart = _settlementCameraButtonRect != null
            ? _settlementCameraButtonRect.anchoredPosition
            : Vector2.zero;
        var elapsed = 0f;
        while (elapsed < SettlementRewardPanelSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(
                elapsed / SettlementRewardPanelSlideDuration);
            var eased = 1f - Mathf.Pow(1f - normalized, 3f);
            if (_settlementFinishButtonRect != null)
            {
                _settlementFinishButtonRect.anchoredPosition = Vector2.LerpUnclamped(
                    finishStart,
                    _settlementFinishButtonTargetPosition,
                    eased);
            }

            if (_settlementCameraButtonRect != null)
            {
                _settlementCameraButtonRect.anchoredPosition = Vector2.LerpUnclamped(
                    cameraStart,
                    _settlementCameraButtonTargetPosition,
                    eased);
            }

            yield return null;
        }

        if (_settlementFinishButtonRect != null)
        {
            _settlementFinishButtonRect.anchoredPosition =
                _settlementFinishButtonTargetPosition;
        }

        if (_settlementCameraButtonRect != null)
        {
            _settlementCameraButtonRect.anchoredPosition =
                _settlementCameraButtonTargetPosition;
        }

        if (_settlementCameraButton != null)
        {
            _settlementCameraButton.interactable = true;
        }
    }

    private void PrepareSettlementRewardSlots()
    {
        _completionRewardDisplayImage = null;
        _taskRewardDisplayImage = null;
        var showTaskReward = _settlementTaskRewardPackId > 0
                             || _didQueueTaskRewardDuringSettlement;
        var rewardCount = (_settlementCompletionRewardPackId > 0 ? 1 : 0)
                          + (showTaskReward ? 1 : 0);
        if (rewardCount <= 0 || _taskRewardImage == null)
        {
            return;
        }

        var slotIndex = 0;
        if (_settlementCompletionRewardPackId > 0)
        {
            _completionRewardDisplayImage = ConfigureSettlementRewardSlot(
                _taskRewardImage,
                slotIndex++,
                rewardCount);
        }

        if (showTaskReward)
        {
            var taskImage = slotIndex == 0
                ? _taskRewardImage
                : _secondaryRewardImage;
            _taskRewardDisplayImage = ConfigureSettlementRewardSlot(
                taskImage,
                slotIndex,
                rewardCount);
        }
    }

    private Image ConfigureSettlementRewardSlot(
        Image image,
        int slotIndex,
        int slotCount)
    {
        if (image == null)
        {
            return null;
        }

        image.raycastTarget = false;
        var rect = image.rectTransform;
        var offsetX = slotCount > 1
            ? (slotIndex == 0 ? -SettlementRewardSlotOffset : SettlementRewardSlotOffset)
            : 0f;
        rect.anchoredPosition = _taskRewardImageTargetPosition + Vector2.right * offsetX;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        image.gameObject.SetActive(false);
        return image;
    }

    private IEnumerator AnimateSettlementRewardPop(RectTransform rewardRect)
    {
        if (rewardRect == null)
        {
            yield break;
        }

        rewardRect.gameObject.SetActive(true);
        rewardRect.localScale = Vector3.zero;
        var elapsed = 0f;
        while (elapsed < SettlementRewardPopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementRewardPopDuration);
            float scale;
            if (normalized < 0.62f)
            {
                scale = Mathf.LerpUnclamped(
                    0f,
                    1.2f,
                    Mathf.SmoothStep(0f, 1f, normalized / 0.62f));
            }
            else
            {
                scale = Mathf.LerpUnclamped(
                    1.2f,
                    1f,
                    Mathf.SmoothStep(0f, 1f, (normalized - 0.62f) / 0.38f));
            }

            rewardRect.localScale = Vector3.one * scale;
            yield return null;
        }

        rewardRect.localScale = Vector3.one;
    }

    private IEnumerator AnimateTaskRewardIntoSettlementSlot(Image targetImage)
    {
        if (targetImage == null
            || _taskRewardSourceImage == null
            || !_taskRewardSourceImage.gameObject.activeInHierarchy)
        {
            if (targetImage != null)
            {
                yield return AnimateSettlementRewardPop(targetImage.rectTransform);
            }

            yield break;
        }

        var targetRect = targetImage.rectTransform;
        var targetParent = targetRect.parent as RectTransform;
        if (targetParent == null)
        {
            yield return AnimateSettlementRewardPop(targetRect);
            yield break;
        }

        targetImage.gameObject.SetActive(true);
        targetRect.localRotation = Quaternion.identity;
        targetRect.localScale = Vector3.one;
        Canvas.ForceUpdateCanvases();
        if (!TryGetScreenRectGeometry(
                _taskRewardSourceImage.rectTransform,
                out _,
                out var sourceScreenSize)
            || !TryGetScreenRectGeometry(
                targetRect,
                out _,
                out var targetScreenSize))
        {
            targetImage.gameObject.SetActive(false);
            yield return AnimateSettlementRewardPop(targetRect);
            yield break;
        }

        var targetAnchoredPosition = targetRect.anchoredPosition;
        var targetLocalPosition = targetRect.localPosition;
        var targetWorldPosition = targetParent.TransformPoint(targetLocalPosition);
        var sourceWorldPosition = _taskRewardSourceImage.rectTransform.TransformPoint(
            _taskRewardSourceImage.rectTransform.rect.center);
        sourceWorldPosition.z = targetWorldPosition.z;
        var sourceScale = CalculateUniformRectScale(targetScreenSize, sourceScreenSize);

        targetRect.position = sourceWorldPosition;
        targetRect.localScale = Vector3.one * sourceScale;
        _taskRewardSourceImage.gameObject.SetActive(false);
        StartCoroutine(AnimateTaskRewardSourceDecorationsFade());

        var elapsed = 0f;
        while (elapsed < SettlementTaskRewardFlyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementTaskRewardFlyDuration);
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            targetWorldPosition = targetParent.TransformPoint(targetLocalPosition);
            targetRect.position = Vector3.LerpUnclamped(
                sourceWorldPosition,
                targetWorldPosition,
                eased);
            targetRect.localScale = Vector3.one * Mathf.LerpUnclamped(
                sourceScale,
                1f,
                eased);
            targetRect.localRotation = Quaternion.identity;
            yield return null;
        }

        targetRect.anchoredPosition = targetAnchoredPosition;
        targetRect.localScale = Vector3.one;
        targetRect.localRotation = Quaternion.identity;
    }

    private IEnumerator AnimateTaskRewardSourceDecorationsFade()
    {
        var elapsed = 0f;
        while (elapsed < SettlementTaskRewardDecorationFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(
                elapsed / SettlementTaskRewardDecorationFadeDuration);
            var alpha = 1f - Mathf.SmoothStep(0f, 1f, normalized);
            SetGraphicColorAlpha(
                _taskRewardSourceCircleImage,
                _taskRewardSourceCircleDefaultColor,
                alpha);
            SetGraphicColorAlpha(
                _taskRewardSourceCountBackgroundImage,
                _taskRewardSourceCountBackgroundDefaultColor,
                alpha);
            SetGraphicColorAlpha(
                _taskRewardSourceCountText,
                _taskRewardSourceCountDefaultColor,
                alpha);
            yield return null;
        }

        SetGraphicColorAlpha(
            _taskRewardSourceCircleImage,
            _taskRewardSourceCircleDefaultColor,
            0f);
        SetGraphicColorAlpha(
            _taskRewardSourceCountBackgroundImage,
            _taskRewardSourceCountBackgroundDefaultColor,
            0f);
        SetGraphicColorAlpha(
            _taskRewardSourceCountText,
            _taskRewardSourceCountDefaultColor,
            0f);
    }

    private void RestoreTaskRewardSourceVisuals()
    {
        if (_taskRewardSourceCircleImage != null)
        {
            _taskRewardSourceCircleImage.gameObject.SetActive(true);
            _taskRewardSourceCircleImage.color = _taskRewardSourceCircleDefaultColor;
        }

        if (_taskRewardSourceImage != null)
        {
            _taskRewardSourceImage.gameObject.SetActive(true);
            _taskRewardSourceImage.color = _taskRewardSourceDefaultColor;
        }

        if (_taskRewardSourceCountBackgroundImage != null)
        {
            _taskRewardSourceCountBackgroundImage.gameObject.SetActive(true);
            _taskRewardSourceCountBackgroundImage.color =
                _taskRewardSourceCountBackgroundDefaultColor;
        }

        if (_taskRewardSourceCountText != null)
        {
            _taskRewardSourceCountText.gameObject.SetActive(true);
            _taskRewardSourceCountText.color = _taskRewardSourceCountDefaultColor;
        }
    }

    private static void SetGraphicColorAlpha(
        Graphic graphic,
        Color baseColor,
        float alphaMultiplier)
    {
        if (graphic == null)
        {
            return;
        }

        baseColor.a *= Mathf.Clamp01(alphaMultiplier);
        graphic.color = baseColor;
    }

    private void ShowRewardPanel()
    {
        using var settlementEntry = SettlementEntryMarker.Auto();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var entryStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
#endif
        if (_isGameFinished)
        {
            return;
        }

        if (GameManager.GetBagId() == GameDefine.DefaultBagId)
        {
            PersistPiecePlacementTutorialCompletion();
        }

        if (_isTutorialPending || IsTutorialActive)
        {
            StopPiecePlacementTutorial(restoreLevelOutline: false);
        }

        _isGameFinished = true;
        if (_hintButton != null)
        {
            _hintButton.interactable = false;
            _hintButton.gameObject.SetActive(false);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = false;
            _testCompleteButton.gameObject.SetActive(false);
        }
#endif
        StopGameplayTimer();
        EndDragging();

        if (_rewardPanelRoot == null)
        {
            _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
            CacheRewardPanelReferences();
        }

        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: cannot show reward panel. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        using (SettlementBoardPreparationMarker.Auto())
        {
            PrepareBoardForRewardPanel();
        }
        _settlementCompletionRewardPackId = 0;
        _settlementTaskRewardPackId = 0;
        _didQueueTaskRewardDuringSettlement = false;
        _settlementPackRewardIds.Clear();
        _isSettlementReadyForFinish = false;
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        SetSettlementInputLocked(true);
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        _rewardPanelRoot.SetActive(true);
        _rewardPanelRoot.transform.SetAsLastSibling();
        PrepareSettlementVisualState();
        StartCoroutine(ProcessTaskSettlement());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogSettlementPerformance("entry prepared", entryStartedAt);
#endif
        Debug.Log("GameScene: puzzle completed, RewardPanel shown.");
    }

    private void PrepareBoardForRewardPanel()
    {
        RemoveRuntimePuzzlePieces();
        RevealAllGroovesOnBoard();
        PrepareCompletedBoardForRewardPanelAnimation();
    }

    private void PrepareCompletedBoardForRewardPanelAnimation()
    {
        _hasSettlementBoardFitTarget = false;
        if (_loadedCardBagRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        var camera = Camera.main;
        var visibleRect = Rect.MinMaxRect(0f, 0f, Screen.width, Screen.height);
        if (_board.BackgroundRect != null)
        {
            var backgroundRect = GetRectTransformScreenRect(_board.BackgroundRect, camera);
            visibleRect = Rect.MinMaxRect(
                Mathf.Max(visibleRect.xMin, backgroundRect.xMin),
                Mathf.Max(visibleRect.yMin, backgroundRect.yMin),
                Mathf.Min(visibleRect.xMax, backgroundRect.xMax),
                Mathf.Min(visibleRect.yMax, backgroundRect.yMax));
        }

        var boardRect = GetRectTransformScreenRect(_loadedCardBagRect, camera);
        if (visibleRect.width <= 0.01f
            || visibleRect.height <= 0.01f
            || boardRect.width <= 0.01f
            || boardRect.height <= 0.01f)
        {
            return;
        }

        var parentRect = _loadedCardBagRect.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        var canvas = _loadedCardBagRect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera ?? camera : camera;

        var fitScale = Mathf.Min(
            visibleRect.width * SettlementBoardViewportFill / boardRect.width,
            visibleRect.height * SettlementBoardViewportFill / boardRect.height);
        fitScale = Mathf.Min(1f, fitScale);
        var startPosition = _loadedCardBagRect.anchoredPosition;
        var startScale = _loadedCardBagRect.localScale;
        var targetScale = startScale * fitScale;
        _loadedCardBagRect.localScale = targetScale;
        Canvas.ForceUpdateCanvases();
        boardRect = GetRectTransformScreenRect(_loadedCardBagRect, camera);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                boardRect.center,
                eventCamera,
                out var boardCenter)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                visibleRect.center,
                eventCamera,
                out var targetCenter))
        {
            _loadedCardBagRect.localScale = startScale;
            Canvas.ForceUpdateCanvases();
            return;
        }

        _settlementBoardFitStartPosition = startPosition;
        _settlementBoardFitTargetPosition = startPosition + targetCenter - boardCenter;
        _settlementBoardFitStartScale = startScale;
        _settlementBoardFitTargetScale = targetScale;
        _loadedCardBagRect.anchoredPosition = startPosition;
        _loadedCardBagRect.localScale = startScale;
        _hasSettlementBoardFitTarget = true;
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator AnimateCompletedBoardForRewardPanel()
    {
        if (!_hasSettlementBoardFitTarget || _loadedCardBagRect == null)
        {
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < SettlementBoardFitDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SettlementBoardFitDuration);
            var eased = normalized * normalized * (3f - 2f * normalized);
            _loadedCardBagRect.anchoredPosition = Vector2.LerpUnclamped(
                _settlementBoardFitStartPosition,
                _settlementBoardFitTargetPosition,
                eased);
            _loadedCardBagRect.localScale = Vector3.LerpUnclamped(
                _settlementBoardFitStartScale,
                _settlementBoardFitTargetScale,
                eased);
            yield return null;
        }

        _loadedCardBagRect.anchoredPosition = _settlementBoardFitTargetPosition;
        _loadedCardBagRect.localScale = _settlementBoardFitTargetScale;
        _hasSettlementBoardFitTarget = false;
    }

    private void FinalizeCompletedGroup(int completedGroupIndex)
    {
        RemovePlacedPiecesForGroup(completedGroupIndex);
    }

    private void RemovePlacedPiecesForGroup(int groupIndex)
    {
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || !state.IsPlaced || state.PieceRenderer == null)
            {
                continue;
            }

            state.PieceRenderer.gameObject.SetActive(false);
            Destroy(state.PieceRenderer.gameObject);
            state.PieceRenderer = null;
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot == null)
        {
            return;
        }

        var pieceNamePrefix = $"DraggablePiece_{groupIndex}_";
        for (var childIndex = placedRoot.transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            var child = placedRoot.transform.GetChild(childIndex);
            if (child.name.StartsWith(pieceNamePrefix, StringComparison.Ordinal))
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

    private void RevealGroovesForGroup(int groupIndex)
    {
        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var group = _board.GrooveImagesByGroup[groupIndex];
        if (group == null)
        {
            return;
        }

        for (var i = 0; i < group.Count; i++)
        {
            var grooveImage = group[i];
            if (grooveImage == null)
            {
                continue;
            }

            grooveImage.gameObject.SetActive(true);
            ApplyPlacedPieceImageShadow(grooveImage);
            SetImageAlpha(grooveImage, 1f);
        }
    }

    private void RemoveRuntimePuzzlePieces()
    {
        StopLoosePieceReminderShake();
        StopTrayPieceReflow();
        ClearAmbientPieceLights();
        ClearPieceHint();
        _drag.DraggingPiece = null;
        _drag.CurrentGroupDraggables.Clear();
        ClearActiveGroupOutline();

        var draggableRoot = GameObject.Find(DraggableGroupRootObjectName);
        if (draggableRoot != null)
        {
            draggableRoot.SetActive(false);
            Destroy(draggableRoot);
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot != null)
        {
            placedRoot.SetActive(false);
            Destroy(placedRoot);
        }

        if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.gameObject.SetActive(false);
        }

        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.gameObject.SetActive(false);
        }

        var fillTransform = GetPieceBgFillTransform();
        if (fillTransform != null)
        {
            fillTransform.gameObject.SetActive(false);
        }
    }

    private void RevealAllGroovesOnBoard()
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            RevealGroovesForGroup(groupIndex);
        }
    }

    private void OnFinishButtonClicked()
    {
        if (!_isSettlementReadyForFinish || _isFinishTransitionStarted)
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        _isFinishTransitionStarted = true;
        SetSettlementInputLocked(true);
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        StartCoroutine(PlaySettlementFinishTransition());
    }

    private void OnSettlementCameraClicked()
    {
        if (!_isSettlementReadyForFinish
            || _isFinishTransitionStarted
            || _cardPackPhoto == null
            || _cardPackPhoto.IsCapturing
            || _cardPackPhoto.IsPreviewVisible)
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        SetSettlementActionButtonsInteractable(false);
        if (!_cardPackPhoto.TryCapture(
                GameManager.GetBagId(),
                _ => { },
                RestoreSettlementActionButtons,
                RestoreSettlementActionButtons))
        {
            RestoreSettlementActionButtons();
        }
    }

    private void RestoreSettlementActionButtons()
    {
        SetSettlementActionButtonsInteractable(
            _isSettlementReadyForFinish && !_isFinishTransitionStarted);
    }

    private void SetSettlementActionButtonsInteractable(bool interactable)
    {
        if (_finishButton != null)
        {
            _finishButton.interactable = interactable;
        }

        if (_settlementCameraButton != null)
        {
            _settlementCameraButton.interactable = interactable;
        }
    }

    private IEnumerator PlaySettlementFinishTransition()
    {
        using (SettlementRewardTransitionMarker.Auto())
        {
            PrepareSettlementRewardImagesForExit();
        }
        yield return AnimateSettlementUiExit();

        CardPackRewardFlyTransition.CancelPending();
        bool transitionStarted;
        using (SettlementRewardTransitionMarker.Auto())
        {
            transitionStarted = CardPackRewardFlyTransition.TryStart(
                BuildSettlementRewardTransitionSources(),
                _settlementPackRewardIds,
                IsSettlementUiVisible(_settlementRewardBagRect)
                    ? _settlementRewardBagRect
                    : null);
        }

        if (!transitionStarted)
        {
            GameManager.EnterMainScene();
        }
    }

    private void PrepareSettlementRewardImagesForExit()
    {
        var rewardPanelRect = _rewardPanelRoot != null
            ? _rewardPanelRoot.transform as RectTransform
            : null;
        if (rewardPanelRect == null)
        {
            return;
        }

        var movedItems = new HashSet<Transform>();
        MoveRewardItem(_completionRewardDisplayImage);
        if (_settlementTaskRewardPackId > 0)
        {
            MoveRewardItem(_taskRewardDisplayImage);
        }

        void MoveRewardItem(Image image)
        {
            if (image == null
                || !image.gameObject.activeInHierarchy)
            {
                return;
            }

            var itemRoot = GetSettlementRewardItemRoot(image);
            if (itemRoot == null)
            {
                Debug.LogWarning(
                    $"GameScene: could not move complete settlement reward item. "
                    + $"BagCover={image.name}");
                return;
            }

            if (!movedItems.Add(itemRoot))
            {
                return;
            }

            if (!TryGetScreenRectGeometry(
                    image.rectTransform,
                    out var screenCenter,
                    out var screenSize))
            {
                Debug.LogWarning(
                    $"GameScene: could not preserve complete settlement reward geometry. "
                    + $"item={itemRoot.name}, BagCover={image.name}");
                return;
            }

            itemRoot.SetParent(rewardPanelRect, false);
            itemRoot.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            RestoreRewardItemScreenGeometry(
                itemRoot,
                image.rectTransform,
                rewardPanelRect,
                screenCenter,
                screenSize);
            TryGetScreenRectGeometry(
                image.rectTransform,
                out var restoredScreenCenter,
                out var restoredScreenSize);
            Debug.Log(
                $"GameScene: complete settlement reward item retained for exit. "
                + $"item={itemRoot.name}, BagCover={image.name}, "
                + $"center={screenCenter}->{restoredScreenCenter}, "
                + $"size={screenSize}->{restoredScreenSize}");
        }
    }

    private static void RestoreRewardItemScreenGeometry(
        Transform itemRoot,
        RectTransform coverRect,
        RectTransform parentRect,
        Vector2 targetScreenCenter,
        Vector2 targetScreenSize)
    {
        if (itemRoot == null || coverRect == null || parentRect == null)
        {
            return;
        }

        if (TryGetScreenRectGeometry(coverRect, out _, out var currentScreenSize))
        {
            var displayScale = CalculateUniformRectScale(currentScreenSize, targetScreenSize);
            itemRoot.localScale *= displayScale;
            Canvas.ForceUpdateCanvases();
        }

        if (!TryGetScreenRectGeometry(coverRect, out var currentScreenCenter, out _))
        {
            return;
        }

        var parentCanvas = parentRect.GetComponentInParent<Canvas>();
        var rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        var eventCamera = rootCanvas != null
                          && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera ?? Camera.main
            : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                targetScreenCenter,
                eventCamera,
                out var targetLocal)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                currentScreenCenter,
                eventCamera,
                out var currentLocal))
        {
            return;
        }

        itemRoot.localPosition += (Vector3)(targetLocal - currentLocal);
        Canvas.ForceUpdateCanvases();
    }

    private static bool TryGetScreenRectGeometry(
        RectTransform rect,
        out Vector2 screenCenter,
        out Vector2 screenSize)
    {
        screenCenter = Vector2.zero;
        screenSize = Vector2.zero;
        if (rect == null)
        {
            return false;
        }

        var sourceCanvas = rect.GetComponentInParent<Canvas>();
        var rootCanvas = sourceCanvas != null ? sourceCanvas.rootCanvas : null;
        var eventCamera = rootCanvas != null
                          && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera ?? Camera.main
            : null;
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var bottomLeft = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);
        screenCenter = (bottomLeft + topRight) * 0.5f;
        screenSize = new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y));
        return screenSize.x > 0.01f && screenSize.y > 0.01f;
    }

    private static float CalculateUniformRectScale(Vector2 currentSize, Vector2 targetSize)
    {
        var widthScale = currentSize.x > 0.01f ? targetSize.x / currentSize.x : 1f;
        var heightScale = currentSize.y > 0.01f ? targetSize.y / currentSize.y : 1f;
        var scale = Mathf.Min(widthScale, heightScale);
        return float.IsFinite(scale) && scale > 0.0001f ? scale : 1f;
    }

    private IEnumerator AnimateSettlementUiExit()
    {
        var summaryStart = GetAnchoredPosition(_settlementSummaryRect);
        var taskStart = GetAnchoredPosition(_rewardTaskItemRect);
        var finishStart = GetAnchoredPosition(_settlementFinishButtonRect);
        var cameraStart = GetAnchoredPosition(_settlementCameraButtonRect);
        var summaryEnd = CalculateSettlementOffscreenPosition(
            _settlementSummaryRect,
            summaryStart,
            above: true);
        var taskEnd = CalculateSettlementOffscreenPosition(
            _rewardTaskItemRect,
            taskStart,
            above: true);
        var finishEnd = CalculateSettlementOffscreenPosition(
            _settlementFinishButtonRect,
            finishStart,
            above: false);
        var cameraEnd = CalculateSettlementOffscreenPosition(
            _settlementCameraButtonRect,
            cameraStart,
            above: false);
        var animateSummary = IsSettlementUiVisible(_settlementSummaryRect);
        var animateTask = IsSettlementUiVisible(_rewardTaskItemRect);
        var animateFinish = IsSettlementUiVisible(_settlementFinishButtonRect);
        var animateCamera = IsSettlementUiVisible(_settlementCameraButtonRect);
        var duration = Mathf.Max(
            animateSummary || animateTask ? SettlementHeaderDropDuration : 0f,
            animateFinish || animateCamera
                ? SettlementRewardPanelSlideDuration
                : 0f);
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var headerNormalized = Mathf.Clamp01(elapsed / SettlementHeaderDropDuration);
            var bottomNormalized = Mathf.Clamp01(elapsed / SettlementRewardPanelSlideDuration);
            if (animateSummary)
            {
                _settlementSummaryRect.anchoredPosition = CalculateSettlementHeaderExitPosition(
                    summaryStart,
                    summaryEnd,
                    headerNormalized);
            }

            if (animateTask)
            {
                _rewardTaskItemRect.anchoredPosition = CalculateSettlementHeaderExitPosition(
                    taskStart,
                    taskEnd,
                    headerNormalized);
            }

            var bottomProgress = bottomNormalized * bottomNormalized * bottomNormalized;
            if (animateFinish)
            {
                _settlementFinishButtonRect.anchoredPosition = Vector2.LerpUnclamped(
                    finishStart,
                    finishEnd,
                    bottomProgress);
            }

            if (animateCamera)
            {
                _settlementCameraButtonRect.anchoredPosition = Vector2.LerpUnclamped(
                    cameraStart,
                    cameraEnd,
                    bottomProgress);
            }

            yield return null;
        }

        SetAnchoredPosition(_settlementSummaryRect, summaryEnd, animateSummary);
        SetAnchoredPosition(_rewardTaskItemRect, taskEnd, animateTask);
        SetAnchoredPosition(_settlementFinishButtonRect, finishEnd, animateFinish);
        SetAnchoredPosition(_settlementCameraButtonRect, cameraEnd, animateCamera);
    }

    private static Vector2 CalculateSettlementHeaderExitPosition(
        Vector2 start,
        Vector2 end,
        float normalized)
    {
        var anticipationRatio = 1f - SettlementHeaderDropTravelRatio;
        var anticipation = start + Vector2.down * SettlementHeaderDropOvershoot;
        if (normalized < anticipationRatio)
        {
            var progress = Mathf.Clamp01(normalized / anticipationRatio);
            return Vector2.LerpUnclamped(
                start,
                anticipation,
                Mathf.SmoothStep(0f, 1f, progress));
        }

        var exitProgress = Mathf.InverseLerp(anticipationRatio, 1f, normalized);
        return Vector2.LerpUnclamped(
            anticipation,
            end,
            exitProgress * exitProgress * exitProgress);
    }

    private static bool IsSettlementUiVisible(RectTransform rect)
    {
        return rect != null && rect.gameObject.activeInHierarchy;
    }

    private static Vector2 GetAnchoredPosition(RectTransform rect)
    {
        return rect != null ? rect.anchoredPosition : Vector2.zero;
    }

    private static void SetAnchoredPosition(
        RectTransform rect,
        Vector2 position,
        bool shouldSet)
    {
        if (rect != null && shouldSet)
        {
            rect.anchoredPosition = position;
        }
    }

    private List<RectTransform> BuildSettlementRewardTransitionSources()
    {
        var sources = new List<RectTransform>(_settlementPackRewardIds.Count);
        for (var i = 0; i < _settlementPackRewardIds.Count; i++)
        {
            var packId = _settlementPackRewardIds[i];
            RectTransform source = null;
            if (packId == _settlementCompletionRewardPackId
                && _completionRewardDisplayImage != null)
            {
                source = _completionRewardDisplayImage.rectTransform;
            }
            else if (packId == _settlementTaskRewardPackId
                     && _taskRewardDisplayImage != null)
            {
                source = _taskRewardDisplayImage.rectTransform;
            }

            if (source == null && _taskRewardImage != null)
            {
                source = _taskRewardImage.rectTransform;
            }

            sources.Add(source);
        }

        return sources;
    }

    private static Transform GetSettlementRewardItemRoot(Image image)
    {
        var rewardCanvas = image != null ? image.transform.parent : null;
        return rewardCanvas != null && rewardCanvas.GetComponent<Canvas>() != null
            ? rewardCanvas.parent
            : null;
    }

    private void StopAndHideSettlementRewardRevealEffect()
    {
        StopAndHideSettlementRewardRevealEffect(_settlementRewardRevealEffect);
        StopAndHideSettlementRewardRevealEffect(_secondarySettlementRewardRevealEffect);
    }

    private static GameObject FindSettlementRewardRevealEffect(Transform rewardItemCanvas)
    {
        if (rewardItemCanvas == null)
        {
            return null;
        }

        var descendants = rewardItemCanvas.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < descendants.Length; i++)
        {
            if (descendants[i] != null
                && descendants[i].name.Equals(
                    "FX_ui_jieSuo_w",
                    StringComparison.Ordinal))
            {
                return descendants[i].gameObject;
            }
        }

        return null;
    }

    private static void StopAndHideSettlementRewardRevealEffect(GameObject revealEffect)
    {
        if (revealEffect == null)
        {
            return;
        }

        var particleSystems = revealEffect.GetComponentsInChildren<
            ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        revealEffect.SetActive(false);
    }

    private void InitializeScoringSession()
    {
        var settings = GameSettingsUtility.GetSettings();
        _wasHintUsed = false;
        _isLevelOutlineEnabled = settings.IsLevelOutlineEnabled;
        _isStickerOutlineEnabled = settings.IsStickerOutlineEnabled;
        _isHighContrastEnabled = settings.IsHighContrastEnabled;
        _hasGameplayTimerStarted = false;
        _gameplayStartRealtime = 0f;
        _completionTimeSeconds = 0f;

        Debug.Log(
            $"GameScene: scoring session initialized. levelOutline={_isLevelOutlineEnabled}, " +
            $"stickerOutline={_isStickerOutlineEnabled}, " +
            $"highContrast={_isHighContrastEnabled}");
    }

    private bool ShouldOfferPiecePlacementTutorial(int bagId, bool isReplaySession)
    {
        if (bagId != GameDefine.DefaultBagId)
        {
            return false;
        }

        if (isReplaySession)
        {
            return true;
        }

        if (CardPackDataUtility.HasActivePuzzleSession(bagId))
        {
            return true;
        }

        if (_wasSelectedPackCompletedOnEntry)
        {
            return false;
        }

        return SqliteLocalStore.Initialize()
            && !SqliteLocalStore.Exists(TutorialCollection, PiecePlacementTutorialKey);
    }

    private void TryStartPiecePlacementTutorial()
    {
        if (!_isTutorialPending
            || _isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating)
        {
            return;
        }

        if (_tutorialArrowSprite == null)
        {
            _tutorialArrowSprite = GameCommonUtility.LoadSpriteByPath(
                TutorialArrowPath,
                PixelsPerUnit);
        }

        if (_tutorialArrowSprite == null)
        {
            Debug.LogWarning(
                $"GameScene: tutorial arrow is missing at {TutorialArrowPath}; tutorial skipped.");
            _isTutorialPending = false;
            SetHintButtonTutorialState();
            TryRefreshActiveGroupOutline(_drag.CurrentGroupIndex);
            return;
        }

        if (_tutorialTipBackgroundSprite == null)
        {
            _tutorialTipBackgroundSprite = GameCommonUtility.LoadSpriteByPath(
                TutorialTipBackgroundPath,
                PixelsPerUnit);
        }

        _isTutorialPending = false;
        EnterTutorialStageForGroup(_drag.CurrentGroupIndex);
    }

    private void EnterTutorialStageForGroup(int groupIndex)
    {
        _tutorialStage = groupIndex <= 0
            ? TutorialStage.StrongPlacement
            : groupIndex == 1
                ? TutorialStage.TwoPiecePractice
                : TutorialStage.HintIntroduction;
        _tutorialPiece = _tutorialStage == TutorialStage.StrongPlacement
            ? FindHintTarget()
            : null;

        if (_tutorialStage == TutorialStage.StrongPlacement && _tutorialPiece == null)
        {
            Debug.LogWarning("GameScene: strong tutorial has no available target; tutorial skipped.");
            StopPiecePlacementTutorial(restoreLevelOutline: true);
            return;
        }

        SetHintButtonTutorialState();
        if (IsTutorialBlockingOutline)
        {
            ClearActiveGroupOutline();
        }
        else if (GameObject.Find(ActiveGroupOutlineRootObjectName) == null)
        {
            TryRefreshActiveGroupOutline(groupIndex);
        }

        if (!ShowPiecePlacementTutorialPresentation())
        {
            Debug.LogWarning("GameScene: tutorial presentation could not be created; tutorial skipped.");
            StopPiecePlacementTutorial(restoreLevelOutline: true);
            return;
        }

        Debug.Log(
            $"GameScene: tutorial stage started. stage={_tutorialStage}, group={groupIndex}, "
            + $"piece={GetPieceNumberFromState(_tutorialPiece)}");
    }

    private bool ShowPiecePlacementTutorialPresentation()
    {
        HidePiecePlacementTutorialPresentation();
        if (!IsTutorialActive)
        {
            return false;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        _tutorialCanvasRoot = new GameObject(
            TutorialCanvasObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        var tutorialCanvas = _tutorialCanvasRoot.GetComponent<Canvas>();
        GameCommonUtility.ConfigureCanvasForGameplay(
            tutorialCanvas,
            camera,
            ReferenceHeight * (16f / 9f),
            ReferenceHeight,
            PixelsPerUnit);
        tutorialCanvas.overrideSorting = true;
        tutorialCanvas.sortingOrder = TutorialCanvasSortingOrder;
        Canvas.ForceUpdateCanvases();

        var canvasRect = _tutorialCanvasRoot.GetComponent<RectTransform>();
        CreateTutorialInstruction(canvasRect, _tutorialStage);
        if (_tutorialStage == TutorialStage.HintIntroduction)
        {
            return true;
        }

        return RebuildTutorialFocusPresentation();
    }

    private bool RebuildTutorialFocusPresentation()
    {
        HideTutorialFocusPresentation();
        ClearPieceHint();
        if (!IsTutorialFocusStage || _tutorialCanvasRoot == null)
        {
            return false;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        var canvasRect = _tutorialCanvasRoot.GetComponent<RectTransform>();
        _tutorialFocusRoot = new GameObject("TutorialFocus", typeof(RectTransform));
        var focusRect = _tutorialFocusRoot.GetComponent<RectTransform>();
        focusRect.SetParent(canvasRect, false);
        focusRect.anchorMin = Vector2.zero;
        focusRect.anchorMax = Vector2.one;
        focusRect.offsetMin = Vector2.zero;
        focusRect.offsetMax = Vector2.zero;

        if (_tutorialStage == TutorialStage.StrongPlacement)
        {
            if (_tutorialPiece?.PieceRenderer == null
                || _tutorialPiece.GrooveRect == null
                || !TryGetRendererScreenRect(
                    _tutorialPiece.PieceRenderer,
                    camera,
                    out var pieceScreenRect)
                || !TryScreenRectToCanvasRect(canvasRect, pieceScreenRect, out var pieceCanvasRect)
                || !TryGetRectTransformScreenCenter(
                    _tutorialPiece.GrooveRect,
                    out var grooveScreenCenter)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    grooveScreenCenter,
                    null,
                    out var grooveCanvasCenter))
            {
                return false;
            }

            CreateTutorialPieceCopy(focusRect, _tutorialPiece, pieceCanvasRect);
            CreateTutorialArrow(focusRect, pieceCanvasRect, grooveCanvasCenter);
            CreatePieceHintOutline(_tutorialPiece, TutorialTargetOutlineColor);
            return true;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || state.PieceRenderer == null
                || !TryGetRendererScreenRect(state.PieceRenderer, camera, out var pieceScreenRect)
                || !TryScreenRectToCanvasRect(canvasRect, pieceScreenRect, out var pieceCanvasRect))
            {
                continue;
            }

            CreateTutorialPieceCopy(focusRect, state, pieceCanvasRect);
        }

        return true;
    }

    private static void CreateTutorialPieceCopy(
        RectTransform parent,
        DraggablePieceState state,
        Rect pieceRect)
    {
        var pieceObject = new GameObject(
            TutorialPieceObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        var pieceImage = pieceObject.GetComponent<Image>();
        pieceImage.rectTransform.SetParent(parent, false);
        pieceImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        pieceImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        pieceImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        pieceImage.rectTransform.anchoredPosition = pieceRect.center;
        pieceImage.rectTransform.sizeDelta = pieceRect.size;
        pieceImage.rectTransform.localRotation = state.PieceRenderer.transform.rotation;
        pieceImage.sprite = state.PieceRenderer.sprite;
        pieceImage.color = state.PieceRenderer.color;
        pieceImage.preserveAspect = false;
        pieceImage.raycastTarget = false;
    }

    private void CreateTutorialInstruction(RectTransform parent, TutorialStage stage)
    {
        var promptObject = new GameObject(
            TutorialTextObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(TutorialPromptMotion));
        var promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.SetParent(parent, false);
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        var promptSize = stage == TutorialStage.TwoPiecePractice
            ? new Vector2(580f, 232f)
            : stage == TutorialStage.HintIntroduction
                ? GetTutorialHintPromptSize()
                : new Vector2(540f, 224f);
        var targetPosition = GetTutorialPromptPosition(parent, stage, promptSize);
        promptRect.anchoredPosition = targetPosition;
        promptRect.sizeDelta = promptSize;

        var promptImage = promptObject.GetComponent<Image>();
        promptImage.sprite = _tutorialTipBackgroundSprite;
        promptImage.color = _tutorialTipBackgroundSprite != null
            ? Color.white
            : new Color(0.24f, 0.36f, 0.68f, 0.96f);
        promptImage.preserveAspect = true;
        promptImage.raycastTarget = false;

        CreateTutorialTextFromTemplate(promptRect, stage);

        if (stage == TutorialStage.HintIntroduction)
        {
            CreateTutorialHintArrow(promptRect, parent);
        }

        var entranceOffset = stage == TutorialStage.StrongPlacement
            ? new Vector2(-72f, 0f)
            : stage == TutorialStage.TwoPiecePractice
                ? new Vector2(0f, -72f)
                : new Vector2(72f, 0f);
        promptObject.GetComponent<TutorialPromptMotion>().Configure(
            promptRect,
            promptObject.GetComponent<CanvasGroup>(),
            targetPosition,
            entranceOffset);
    }

    private static void CreateTutorialTextFromTemplate(
        RectTransform parent,
        TutorialStage stage)
    {
        var template = GameCommonUtility.FindSceneObject(TutorialTipTemplateObjectName);
        var templateText = template != null
            ? template.transform.Find(TutorialTipTextObjectName)?.GetComponent<TMP_Text>()
            : null;
        if (templateText == null)
        {
            Debug.LogWarning(
                $"GameScene: tutorial text template is missing. "
                + $"Expected {TutorialTipTemplateObjectName}/{TutorialTipTextObjectName}.");
            return;
        }

        var text = Instantiate(templateText, parent, false);
        text.name = TutorialTipTextObjectName;
        text.enableWordWrapping = false;
        text.enableAutoSizing = true;
        text.fontSizeMax = templateText.fontSize;
        text.fontSizeMin = Mathf.Min(templateText.fontSizeMin, text.fontSizeMax);
        text.maxVisibleLines = 2;
        text.text = FormatTutorialInstruction(
            text,
            GetTutorialInstruction(stage));
        text.raycastTarget = false;
        text.gameObject.SetActive(true);
    }

    private static string FormatTutorialInstruction(TMP_Text text, string instruction)
    {
        var availableWidth = text.rectTransform.rect.width;
        if (string.IsNullOrEmpty(instruction)
            || availableWidth <= 0f
            || text.GetPreferredValues(instruction).x <= availableWidth)
        {
            return instruction;
        }

        var bestSplit = 1;
        var bestWidth = float.MaxValue;
        for (var split = 1; split < instruction.Length; split++)
        {
            if (TutorialInvalidLineStartCharacters.IndexOf(instruction[split]) >= 0)
            {
                continue;
            }

            var firstLineWidth = text.GetPreferredValues(instruction.Substring(0, split)).x;
            var secondLineWidth = text.GetPreferredValues(instruction.Substring(split)).x;
            var widestLine = Mathf.Max(firstLineWidth, secondLineWidth);
            if (widestLine < bestWidth)
            {
                bestWidth = widestLine;
                bestSplit = split;
            }
        }

        return instruction.Insert(bestSplit, "\n");
    }

    private static Vector2 GetTutorialHintPromptSize()
    {
        var template = GameCommonUtility.FindSceneObject(TutorialTipTemplateObjectName);
        var templateRect = template != null ? template.GetComponent<RectTransform>() : null;
        return templateRect != null && templateRect.sizeDelta.sqrMagnitude > 0f
            ? templateRect.sizeDelta
            : new Vector2(540f, 224f);
    }

    private void CreateTutorialHintArrow(
        RectTransform parent,
        RectTransform canvasRect)
    {
        var template = GameCommonUtility.FindSceneObject(TutorialTipTemplateObjectName);
        var templateArrowRect = template != null
            ? template.transform.Find(TutorialHintArrowObjectName) as RectTransform
            : null;
        var templateArrowImage = templateArrowRect != null
            ? templateArrowRect.GetComponent<Image>()
            : null;
        if (templateArrowRect == null || templateArrowImage?.sprite == null)
        {
            Debug.LogWarning(
                $"GameScene: tutorial hint arrow template is missing. "
                + $"Expected {TutorialTipTemplateObjectName}/{TutorialHintArrowObjectName}.");
            return;
        }

        var arrowObject = new GameObject(
            TutorialHintArrowObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TutorialHintArrowMotion));
        arrowObject.layer = parent.gameObject.layer;
        var arrowRect = arrowObject.GetComponent<RectTransform>();
        arrowRect.SetParent(parent, false);
        arrowRect.anchorMin = templateArrowRect.anchorMin;
        arrowRect.anchorMax = templateArrowRect.anchorMax;
        arrowRect.pivot = templateArrowRect.pivot;
        arrowRect.anchoredPosition = templateArrowRect.anchoredPosition;
        arrowRect.sizeDelta = templateArrowRect.sizeDelta;
        arrowRect.localRotation = templateArrowRect.localRotation;
        arrowRect.localScale = templateArrowRect.localScale;

        if (TryGetTutorialHintArrowTargetCanvasPosition(canvasRect, out var arrowTargetPosition))
        {
            var targetWorld = canvasRect.TransformPoint(
                arrowTargetPosition - TutorialHintArrowMotion.PulseOffset);
            var targetLocal = parent.InverseTransformPoint(targetWorld);
            var arrowTipWorld = arrowRect.TransformPoint(
                new Vector3(arrowRect.rect.xMax, arrowRect.rect.center.y));
            var arrowTipLocal = parent.InverseTransformPoint(arrowTipWorld);
            arrowRect.anchoredPosition += (Vector2)(targetLocal - arrowTipLocal);
        }

        var arrowImage = arrowObject.GetComponent<Image>();
        arrowImage.sprite = templateArrowImage.sprite;
        arrowImage.color = templateArrowImage.color;
        arrowImage.type = templateArrowImage.type;
        arrowImage.preserveAspect = templateArrowImage.preserveAspect;
        arrowImage.raycastTarget = false;
        arrowObject.GetComponent<TutorialHintArrowMotion>().Configure(arrowRect, arrowImage);
        arrowObject.SetActive(true);
    }

    private Vector2 GetTutorialPromptPosition(
        RectTransform parent,
        TutorialStage stage,
        Vector2 promptSize)
    {
        if (stage != TutorialStage.HintIntroduction
            && (TryGetTutorialTargetGrooveBounds(parent, stage, out var grooveBounds)
                || TryGetTutorialBoardBounds(parent, out grooveBounds)))
        {
            return ClampTutorialPromptPosition(
                parent.rect,
                new Vector2(
                    grooveBounds.center.x,
                    grooveBounds.yMax + TutorialTargetPromptGap + promptSize.y * 0.5f),
                promptSize);
        }

        if (stage != TutorialStage.HintIntroduction)
        {
            return ClampTutorialPromptPosition(
                parent.rect,
                new Vector2(
                    parent.rect.center.x,
                    parent.rect.yMax - TutorialPromptScreenMargin - promptSize.y * 0.5f),
                promptSize);
        }

        if (stage == TutorialStage.HintIntroduction
            && TryGetTutorialHintButtonCanvasRect(parent, out var hintButtonRect))
        {
            var hintPromptPosition = hintButtonRect.center
                                     - GetTutorialHintArrowTipOffset()
                                     - TutorialHintArrowMotion.PulseOffset
                                     + Vector2.left * TutorialHintPromptLeftOffset
                                     + Vector2.down * TutorialHintPromptDownOffset;
            hintPromptPosition.x = Mathf.Min(
                hintPromptPosition.x,
                hintButtonRect.xMin - TutorialHintPromptButtonGap - promptSize.x * 0.5f);
            return ClampTutorialPromptPosition(
                parent.rect,
                hintPromptPosition,
                promptSize);
        }

        var normalizedAnchor = TutorialHintPromptAnchor;
        var position = new Vector2(
            Mathf.Lerp(parent.rect.xMin, parent.rect.xMax, normalizedAnchor.x),
            Mathf.Lerp(parent.rect.yMin, parent.rect.yMax, normalizedAnchor.y));

        return ClampTutorialPromptPosition(
            parent.rect,
            position,
            promptSize);
    }

    private bool TryGetTutorialHintButtonCanvasRect(
        RectTransform canvasRect,
        out Rect hintCanvasRect)
    {
        hintCanvasRect = default;
        var hintRect = _hintButton != null
            ? _hintButton.transform as RectTransform
            : null;
        return hintRect != null
               && TryGetRectTransformScreenRect(hintRect, out var hintScreenRect)
               && TryScreenRectToCanvasRectUsingCanvasCamera(
                   canvasRect,
                   hintScreenRect,
                   out hintCanvasRect);
    }

    private bool TryGetTutorialHintArrowTargetCanvasPosition(
        RectTransform canvasRect,
        out Vector2 position)
    {
        position = Vector2.zero;
        var hintRect = _hintButton != null
            ? _hintButton.transform as RectTransform
            : null;
        if (hintRect == null
            || !TryGetRectTransformScreenRect(hintRect, out var hintScreenRect)
            || !TryScreenRectToCanvasRectUsingCanvasCamera(
                canvasRect,
                hintScreenRect,
                out var hintCanvasRect))
        {
            return false;
        }

        position = new Vector2(
            hintCanvasRect.xMin - TutorialHintArrowButtonGap,
            hintCanvasRect.center.y - TutorialHintArrowTargetDownOffset);
        return true;
    }

    private static Vector2 GetTutorialHintArrowTipOffset()
    {
        var template = GameCommonUtility.FindSceneObject(TutorialTipTemplateObjectName);
        var templateRect = template != null ? template.GetComponent<RectTransform>() : null;
        var arrowRect = template != null
            ? template.transform.Find(TutorialHintArrowObjectName) as RectTransform
            : null;
        if (templateRect == null || arrowRect == null)
        {
            return Vector2.zero;
        }

        var tipWorld = arrowRect.TransformPoint(
            new Vector3(arrowRect.rect.xMax, arrowRect.rect.center.y));
        return templateRect.InverseTransformPoint(tipWorld);
    }

    private bool TryGetTutorialTargetGrooveBounds(
        RectTransform canvasRect,
        TutorialStage stage,
        out Rect grooveBounds)
    {
        grooveBounds = default;
        var hasBounds = false;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || (stage == TutorialStage.StrongPlacement && state != _tutorialPiece))
            {
                continue;
            }

            var grooveRect = state.GrooveRect;
            if (!TryGetRectTransformScreenRect(grooveRect, out var screenRect)
                || !TryScreenRectToCanvasRectUsingCanvasCamera(
                    canvasRect,
                    screenRect,
                    out var canvasBounds))
            {
                continue;
            }

            grooveBounds = hasBounds
                ? Rect.MinMaxRect(
                    Mathf.Min(grooveBounds.xMin, canvasBounds.xMin),
                    Mathf.Min(grooveBounds.yMin, canvasBounds.yMin),
                    Mathf.Max(grooveBounds.xMax, canvasBounds.xMax),
                    Mathf.Max(grooveBounds.yMax, canvasBounds.yMax))
                : canvasBounds;
            hasBounds = true;
        }

        return hasBounds;
    }

    private bool TryGetTutorialBoardBounds(
        RectTransform canvasRect,
        out Rect boardBounds)
    {
        boardBounds = default;
        var boardRect = _board.GameBoardImage != null
            ? _board.GameBoardImage.rectTransform
            : null;
        return TryGetRectTransformScreenRect(boardRect, out var screenRect)
               && TryScreenRectToCanvasRectUsingCanvasCamera(
                   canvasRect,
                   screenRect,
                   out boardBounds);
    }

    private static Vector2 ClampTutorialPromptPosition(
        Rect canvasRect,
        Vector2 position,
        Vector2 promptSize)
    {
        var halfSize = promptSize * 0.5f;
        return new Vector2(
            Mathf.Clamp(
                position.x,
                canvasRect.xMin + halfSize.x + TutorialPromptScreenMargin,
                canvasRect.xMax - halfSize.x - TutorialPromptScreenMargin),
            Mathf.Clamp(
                position.y,
                canvasRect.yMin + halfSize.y + TutorialPromptScreenMargin,
                canvasRect.yMax - halfSize.y - TutorialPromptScreenMargin));
    }

    private static string GetTutorialInstruction(TutorialStage stage)
    {
        return stage == TutorialStage.StrongPlacement
            ? TutorialStrongInstruction
            : stage == TutorialStage.TwoPiecePractice
                ? TutorialPracticeInstruction
                : TutorialHintInstruction;
    }

    private void CreateTutorialArrow(
        RectTransform parent,
        Rect pieceRect,
        Vector2 grooveCenter)
    {
        var arrowObject = new GameObject(
            TutorialArrowObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TutorialArrowMotion));
        var arrowRect = arrowObject.GetComponent<RectTransform>();
        arrowRect.SetParent(parent, false);
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0f);
        arrowRect.sizeDelta = _tutorialArrowSprite.rect.size * TutorialArrowScale;

        var arrowImage = arrowObject.GetComponent<Image>();
        arrowImage.sprite = _tutorialArrowSprite;
        arrowImage.color = Color.white;
        arrowImage.preserveAspect = true;
        arrowImage.raycastTarget = false;

        var tailStart = pieceRect.center;
        var direction = grooveCenter - tailStart;
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector2.up;
        }

        arrowRect.anchoredPosition = tailStart;
        arrowRect.localRotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

        var arrow = arrowObject.GetComponent<TutorialArrowMotion>();
        arrow.Configure(
            arrowRect,
            arrowImage,
            tailStart,
            grooveCenter - direction.normalized * arrowRect.sizeDelta.y);
    }

    private void HideTutorialFocusPresentation()
    {
        if (_tutorialFocusRoot != null)
        {
            _tutorialFocusRoot.SetActive(false);
            Destroy(_tutorialFocusRoot);
            _tutorialFocusRoot = null;
        }
    }

    private void HidePiecePlacementTutorialPresentation(bool clearHintOutline = true)
    {
        HideTutorialFocusPresentation();
        if (clearHintOutline)
        {
            ClearPieceHint();
        }

        if (_tutorialCanvasRoot != null)
        {
            _tutorialCanvasRoot.SetActive(false);
            Destroy(_tutorialCanvasRoot);
            _tutorialCanvasRoot = null;
        }
    }

    private void RestorePiecePlacementTutorialPresentation(DraggablePieceState state)
    {
        if (!IsTutorialActive
            || state == null
            || state.IsPlaced
            || (_tutorialStage == TutorialStage.StrongPlacement && state != _tutorialPiece)
            || _tutorialStage == TutorialStage.HintIntroduction)
        {
            return;
        }

        RefreshPiecePlacementTutorialPresentation();
    }

    private void RefreshPiecePlacementTutorialPresentation()
    {
        if (!RebuildTutorialFocusPresentation())
        {
            StopPiecePlacementTutorial(restoreLevelOutline: true);
        }
    }

    private void StopPiecePlacementTutorial(bool restoreLevelOutline)
    {
        var wasActive = IsTutorialActive || _isTutorialPending;
        _isTutorialPending = false;
        _tutorialStage = TutorialStage.None;
        HidePiecePlacementTutorialPresentation();
        _tutorialPiece = null;
        SetHintButtonTutorialState();

        if (restoreLevelOutline && wasActive && !_isGameFinished)
        {
            TryRefreshActiveGroupOutline(_drag.CurrentGroupIndex);
        }
    }

    private static void PersistPiecePlacementTutorialCompletion()
    {
        if (!SqliteLocalStore.Initialize()
            || !SqliteLocalStore.Upsert(
                TutorialCollection,
                PiecePlacementTutorialKey,
                "true"))
        {
            Debug.LogWarning("GameScene: failed to persist Piece placement tutorial completion.");
        }
    }

    private void SetHintButtonTutorialState()
    {
        if (_hintButton == null)
        {
            return;
        }

        var hiddenForTutorial = _isTutorialPending
            || _tutorialStage == TutorialStage.StrongPlacement
            || _tutorialStage == TutorialStage.TwoPiecePractice;
        _hintButton.gameObject.SetActive(!hiddenForTutorial);
        _hintButton.interactable = !hiddenForTutorial && !_isGameFinished;
    }

    private void DestroyTutorialArrowSprite()
    {
        if (_tutorialArrowSprite == null)
        {
            return;
        }

        var texture = _tutorialArrowSprite.texture;
        Destroy(_tutorialArrowSprite);
        _tutorialArrowSprite = null;
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    private void DestroyTutorialTipBackgroundSprite()
    {
        if (_tutorialTipBackgroundSprite == null)
        {
            return;
        }

        var texture = _tutorialTipBackgroundSprite.texture;
        Destroy(_tutorialTipBackgroundSprite);
        _tutorialTipBackgroundSprite = null;
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    private bool TryGetPieceTrayScreenRect(Camera camera, out Rect screenRect)
    {
        screenRect = default;
        if (_board.PieceBoardRect != null
            && TryGetRectTransformScreenRect(_board.PieceBoardRect, out screenRect))
        {
            return true;
        }

        return _board.PieceBgRenderer != null
            && TryGetWorldBoundsScreenRect(_board.PieceBgRenderer.bounds, camera, out screenRect);
    }

    private static bool TryGetRendererScreenRect(
        SpriteRenderer renderer,
        Camera camera,
        out Rect screenRect)
    {
        screenRect = default;
        return renderer != null
            && TryGetWorldBoundsScreenRect(renderer.bounds, camera, out screenRect);
    }

    private static bool TryGetWorldBoundsScreenRect(Bounds bounds, Camera camera, out Rect screenRect)
    {
        screenRect = default;
        if (camera == null || bounds.size.sqrMagnitude <= 0f)
        {
            return false;
        }

        var min = camera.WorldToScreenPoint(bounds.min);
        var max = camera.WorldToScreenPoint(bounds.max);
        screenRect = Rect.MinMaxRect(
            Mathf.Min(min.x, max.x),
            Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x),
            Mathf.Max(min.y, max.y));
        return screenRect.width > 0f && screenRect.height > 0f;
    }

    private static bool TryGetRectTransformScreenRect(RectTransform rect, out Rect screenRect)
    {
        screenRect = default;
        if (rect == null)
        {
            return false;
        }

        var canvas = rect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera : Camera.main;
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        for (var i = 0; i < corners.Length; i++)
        {
            var point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return screenRect.width > 0f && screenRect.height > 0f;
    }

    private static bool TryGetRectTransformScreenCenter(RectTransform rect, out Vector2 screenCenter)
    {
        screenCenter = Vector2.zero;
        if (rect == null)
        {
            return false;
        }

        var canvas = rect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera : Camera.main;
        screenCenter = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            rect.TransformPoint(rect.rect.center));
        return true;
    }

    private static bool TryScreenRectToCanvasRect(
        RectTransform canvasRect,
        Rect screenRect,
        out Rect localRect)
    {
        return TryScreenRectToCanvasRect(
            canvasRect,
            screenRect,
            null,
            out localRect);
    }

    private static bool TryScreenRectToCanvasRectUsingCanvasCamera(
        RectTransform canvasRect,
        Rect screenRect,
        out Rect localRect)
    {
        var canvas = canvasRect != null
            ? canvasRect.GetComponentInParent<Canvas>()
            : null;
        var eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        return TryScreenRectToCanvasRect(
            canvasRect,
            screenRect,
            eventCamera,
            out localRect);
    }

    private static bool TryScreenRectToCanvasRect(
        RectTransform canvasRect,
        Rect screenRect,
        Camera eventCamera,
        out Rect localRect)
    {
        localRect = default;
        if (canvasRect == null
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenRect.min,
                eventCamera,
                out var min)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenRect.max,
                eventCamera,
                out var max))
        {
            return false;
        }

        localRect = Rect.MinMaxRect(
            Mathf.Min(min.x, max.x),
            Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x),
            Mathf.Max(min.y, max.y));
        return localRect.width > 0f && localRect.height > 0f;
    }

    private void ConfigureHintButton()
    {
        var hintButtonObject = GameCommonUtility.FindSceneObject(HintButtonObjectName);
        if (hintButtonObject == null)
        {
            Debug.LogWarning($"GameScene: hint button not found. Expected object named {HintButtonObjectName}.");
            return;
        }

        _hintButton = hintButtonObject.GetComponent<Button>();
        if (_hintButton == null)
        {
            Debug.LogWarning($"GameScene: {HintButtonObjectName} is missing Button component.");
            return;
        }

        _hintButton.onClick.RemoveListener(OnHintButtonClicked);
        _hintButton.onClick.AddListener(OnHintButtonClicked);
        SetHintButtonTutorialState();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void ConfigureTestCompleteButton()
    {
        var buttonObject = GameCommonUtility.FindSceneObject(TestCompleteButtonObjectName);
        if (buttonObject == null)
        {
            buttonObject = CreateTestCompleteButton();
        }

        if (buttonObject == null)
        {
            Debug.LogWarning("GameScene: failed to create the test complete button.");
            return;
        }

        _testCompleteButton = buttonObject.GetComponent<Button>();
        if (_testCompleteButton == null)
        {
            Debug.LogWarning($"GameScene: {TestCompleteButtonObjectName} is missing Button component.");
            return;
        }

        _testCompleteButton.interactable = !_isGameFinished
            && !_isEntranceAnimating
            && !_isGroupTransitionAnimating;
        _testCompleteButton.onClick.RemoveListener(OnTestCompleteAllClicked);
        _testCompleteButton.onClick.AddListener(OnTestCompleteAllClicked);
    }

    private static GameObject CreateTestCompleteButton()
    {
        var hintObject = GameCommonUtility.FindSceneObject(HintButtonObjectName);
        var hintRect = hintObject != null ? hintObject.GetComponent<RectTransform>() : null;
        var parent = hintRect != null
            ? hintRect.parent as RectTransform
            : GameCommonUtility.FindSceneObject("Canvas")?.GetComponent<RectTransform>();
        if (parent == null)
        {
            return null;
        }

        var buttonObject = new GameObject(
            TestCompleteButtonObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.layer = parent.gameObject.layer;
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(210f, 68f);
        if (hintRect != null)
        {
            rect.anchorMin = hintRect.anchorMin;
            rect.anchorMax = hintRect.anchorMax;
            rect.anchoredPosition = hintRect.anchoredPosition + new Vector2(-170f, 0f);
        }
        else
        {
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = new Vector2(-300f, -90f);
        }
        rect.SetAsLastSibling();

        var image = buttonObject.GetComponent<Image>();
        var hintButton = hintObject != null ? hintObject.GetComponent<Button>() : null;
        var sourceImage = hintObject != null ? hintObject.GetComponent<Image>() : null;
        if (sourceImage == null && hintButton != null)
        {
            sourceImage = hintButton.targetGraphic as Image;
        }

        if (sourceImage != null)
        {
            image.sprite = sourceImage.sprite;
            image.type = sourceImage.type;
            image.material = sourceImage.material;
        }
        else
        {
            image.type = Image.Type.Simple;
        }
        image.color = new Color32(63, 66, 62, 242);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(232, 238, 226, 255);
        colors.pressedColor = new Color32(190, 202, 181, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color32(130, 130, 130, 128);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        var textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(Shadow));
        textObject.layer = buttonObject.layer;
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.rectTransform.SetParent(rect, false);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(10f, 4f);
        text.rectTransform.offsetMax = new Vector2(-10f, -4f);
        text.text = TestCompleteButtonText;
        text.fontSize = 28f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        GameFontUtility.ApplyDefaultFont(text);

        var shadow = textObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
        return buttonObject;
    }

    private void OnTestCompleteAllClicked()
    {
        if (_isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating
            || _isPiecePlacementAnimating
            || _drag.DraggingPiece != null
            || _board.GrooveImagesByGroup == null)
        {
            return;
        }

        var packId = GameManager.GetBagId();
        var allPieceNumbers = new HashSet<int>(_placedPieceNumbers);
        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var pieceIndex = 0; pieceIndex < group.Count; pieceIndex++)
            {
                var grooveImage = group[pieceIndex];
                var pieceNumber = GetPieceNumberFromImage(grooveImage);
                if (pieceNumber != int.MaxValue)
                {
                    allPieceNumbers.Add(pieceNumber);
                }
            }
        }

        if (packId <= 0 || allPieceNumbers.Count == 0)
        {
            Debug.LogWarning(
                $"GameScene: test completion skipped because no valid Pieces were found. packId={packId}");
            return;
        }

        _testCompleteButton.interactable = false;
        StopPiecePlacementTutorial(restoreLevelOutline: false);
        ClearPieceHint();
        StartGameplayTimerIfNeeded();

        _placedPieceNumbers.Clear();
        _placedPieceNumbers.UnionWith(allPieceNumbers);
        if (!CardPackDataUtility.TryRecordPlacedPieces(packId, allPieceNumbers))
        {
            Debug.LogWarning(
                $"GameScene: test completion could not persist every Piece before settlement. packId={packId}");
        }

        RevealAllGroovesOnBoard();
        ShowRewardPanel();
        Debug.Log(
            $"GameScene: test completion placed all Pieces and started settlement. "
            + $"packId={packId}, pieces={allPieceNumbers.Count}");
    }
#endif

    private void OnHintButtonClicked()
    {
        if (_isGameFinished
            || _isEntranceAnimating
            || _isGroupTransitionAnimating
            || _isPiecePlacementAnimating
            || _isTutorialPending
            || IsTutorialBlockingOutline
            || _drag.DraggingPiece != null)
        {
            return;
        }

        if (_hintedPiece != null)
        {
            ClearPieceHint();
            return;
        }

        var target = FindHintTargetByInteractionOrder();
        if (target == null)
        {
            return;
        }

        _wasHintUsed = true;
        ShowPieceHint(target);
        Debug.Log("GameScene: hint used; no-hint score bonus disabled for this game.");
    }

    private DraggablePieceState FindHintTargetByInteractionOrder()
    {
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null
                && !state.IsPlaced
                && state.IsOnTray
                && state.PieceRenderer != null
                && state.GrooveRect != null)
            {
                return state;
            }
        }

        LoosePieceCluster earliestCluster = null;
        for (var i = 0; i < _loosePieceClusters.Count; i++)
        {
            var cluster = _loosePieceClusters[i];
            if (cluster == null
                || cluster.Members.Count < 2
                || cluster.Members.All(member => member == null || member.IsPlaced))
            {
                continue;
            }

            if (earliestCluster == null || cluster.CreatedOrder < earliestCluster.CreatedOrder)
            {
                earliestCluster = cluster;
            }
        }

        if (earliestCluster != null)
        {
            return earliestCluster.Members.FirstOrDefault(
                member => member?.PieceRenderer != null && !member.IsPlaced);
        }

        DraggablePieceState earliestLoosePiece = null;
        var earliestOrder = long.MaxValue;
        foreach (var pair in _loosePieceOrders)
        {
            var state = pair.Key;
            if (state?.PieceRenderer == null
                || state.IsPlaced
                || state.IsOnTray
                || _looseClusterByPiece.ContainsKey(state)
                || pair.Value >= earliestOrder)
            {
                continue;
            }

            earliestLoosePiece = state;
            earliestOrder = pair.Value;
        }

        return earliestLoosePiece;
    }

    private DraggablePieceState FindHintTarget()
    {
        if (_hintedPiece != null
            && !_hintedPiece.IsPlaced
            && _hintedPiece.PieceRenderer != null)
        {
            return _hintedPiece;
        }

        DraggablePieceState target = null;
        var lowestPieceNumber = int.MaxValue;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || state.IsPlaced || state.PieceRenderer == null || state.GrooveRect == null)
            {
                continue;
            }

            var pieceNumber = GetPieceNumberFromState(state);
            if (pieceNumber >= lowestPieceNumber)
            {
                continue;
            }

            target = state;
            lowestPieceNumber = pieceNumber;
        }

        return target;
    }

    private void ShowPieceHint(DraggablePieceState state)
    {
        StopLoosePieceReminderShake();
        ClearPieceHint();
        if (state == null || state.PieceRenderer == null || state.GrooveImage == null || state.GrooveRect == null)
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_PieceShake.mp3");
        _hintedPiece = state;
        _hintedPieces.Clear();
        _hintedPieceBaseRotations.Clear();
        _hintedPieceBasePositions.Clear();
        _hintedCluster = null;
        var hintMembers = GetLooseClusterMembers(state);
        for (var i = 0; i < hintMembers.Count; i++)
        {
            var member = hintMembers[i];
            if (member?.PieceRenderer == null || member.IsPlaced)
            {
                continue;
            }

            _hintedPieces.Add(member);
            _hintedPieceBaseRotations.Add(member.PieceRenderer.transform.rotation);
            _hintedPieceBasePositions.Add(member.PieceRenderer.transform.position);
        }
        if (_hintedPieces.Count > 1
            && _looseClusterByPiece.TryGetValue(state, out var hintedCluster))
        {
            _hintedCluster = hintedCluster;
            if (TryGetPieceRendererBounds(_hintedPieces, out var hintedBounds))
            {
                _hintedClusterCenter = hintedBounds.center;
            }
            if (_hintedCluster.ShadowRenderer != null)
            {
                _hintedClusterShadowBasePosition =
                    _hintedCluster.ShadowRenderer.transform.position;
                _hintedClusterShadowBaseRotation =
                    _hintedCluster.ShadowRenderer.transform.rotation;
            }
        }
        _hintShakeStartTime = Time.unscaledTime;
        _isHintPieceShaking = true;
        CreatePieceHintOutline(
            _hintedPieces,
            _isHighContrastEnabled
                ? HighContrastPieceHintOutlineColor
                : PieceHintOutlineColor);
    }

    private void UpdatePieceHintAnimation()
    {
        if (_hintedPiece == null)
        {
            return;
        }

        if (_hintedPieces.Count == 0
            || _hintedPieces.Any(piece => piece == null || piece.IsPlaced || piece.PieceRenderer == null))
        {
            ClearPieceHint();
            return;
        }

        if (_activeDragMembers.Any(_hintedPieces.Contains))
        {
            RestoreAllHintedPieceRotations();
            return;
        }

        if (!_isHintPieceShaking)
        {
            return;
        }

        var elapsed = Time.unscaledTime - _hintShakeStartTime;
        if (elapsed >= HintShakeDuration)
        {
            RestoreAllHintedPieceRotations();
            _isHintPieceShaking = false;
            return;
        }

        var angle = Mathf.Sin(
            elapsed * HintShakeCyclesPerSecond * Mathf.PI * 2f) * HintShakeAngle;
        if (_hintedCluster != null && _hintedPieces.Count > 1)
        {
            var deltaRotation = Quaternion.Euler(0f, 0f, angle);
            for (var i = 0;
                 i < _hintedPieces.Count
                 && i < _hintedPieceBaseRotations.Count
                 && i < _hintedPieceBasePositions.Count;
                 i++)
            {
                var transform = _hintedPieces[i].PieceRenderer.transform;
                transform.position = _hintedClusterCenter
                    + deltaRotation * (_hintedPieceBasePositions[i] - _hintedClusterCenter);
                transform.rotation = deltaRotation * _hintedPieceBaseRotations[i];
            }

            if (_hintedCluster.ShadowRenderer != null)
            {
                var shadowTransform = _hintedCluster.ShadowRenderer.transform;
                shadowTransform.position = _hintedClusterCenter
                    + deltaRotation
                    * (_hintedClusterShadowBasePosition - _hintedClusterCenter);
                shadowTransform.rotation =
                    deltaRotation * _hintedClusterShadowBaseRotation;
            }
            return;
        }

        for (var i = 0; i < _hintedPieces.Count && i < _hintedPieceBaseRotations.Count; i++)
        {
            _hintedPieces[i].PieceRenderer.transform.rotation =
                _hintedPieceBaseRotations[i] * Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void CreatePieceHintOutline(DraggablePieceState state, Color outlineColor)
    {
        CreatePieceHintOutline(new[] { state }, outlineColor);
    }

    private void CreatePieceHintOutline(
        IReadOnlyList<DraggablePieceState> states,
        Color outlineColor)
    {
        var validStates = states
            .Where(state => state?.GrooveRect != null && state.GrooveImage?.sprite != null)
            .ToList();
        if (validStates.Count == 0)
        {
            return;
        }

        if (validStates.Count == 1)
        {
            CreateSinglePieceHintOutline(validStates[0], outlineColor);
            return;
        }

        var parent = _loadedCardBagRect != null
            ? (RectTransform)_loadedCardBagRect
            : validStates[0].GrooveRect.parent as RectTransform;
        if (parent == null)
        {
            CreateSinglePieceHintOutline(validStates[0], outlineColor);
            return;
        }

        var sourceRegions = new List<HintOutlineSpriteRegion>(validStates.Count);
        var unionRect = default(Rect);
        var hasUnion = false;
        for (var i = 0; i < validStates.Count; i++)
        {
            if (!TryGetRectInParent(validStates[i].GrooveRect, parent, out var sourceRect))
            {
                continue;
            }

            sourceRegions.Add(new HintOutlineSpriteRegion(
                validStates[i].GrooveImage.sprite,
                sourceRect,
                validStates[i].GrooveImage.preserveAspect));
            unionRect = hasUnion ? UnionHintRects(unionRect, sourceRect) : sourceRect;
            hasUnion = true;
        }

        if (!hasUnion || sourceRegions.Count < 2)
        {
            CreateSinglePieceHintOutline(validStates[0], outlineColor);
            return;
        }

        var outlineObject = new GameObject(
            PieceHintOutlineObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(HintDashedOutlineGraphic));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(parent, false);
        outlineRect.anchorMin = new Vector2(0.5f, 0.5f);
        outlineRect.anchorMax = new Vector2(0.5f, 0.5f);
        outlineRect.pivot = new Vector2(0.5f, 0.5f);
        outlineRect.sizeDelta = unionRect.size;
        outlineRect.localPosition = new Vector3(unionRect.center.x, unionRect.center.y, 0f);
        outlineRect.localRotation = Quaternion.identity;
        outlineRect.localScale = Vector3.one;
        outlineRect.SetAsLastSibling();

        for (var i = 0; i < sourceRegions.Count; i++)
        {
            sourceRegions[i] = sourceRegions[i].OffsetBy(-unionRect.center);
        }

        var outlineGraphic = outlineObject.GetComponent<HintDashedOutlineGraphic>();
        outlineGraphic.maskable = false;
        outlineGraphic.ConfigureCombined(
            sourceRegions,
            outlineColor,
            HintOutlineWidth,
            HintDashLength,
            HintDashGap,
            HintOutlineScrollSpeed);
        _pieceHintOutlineRoot = outlineObject;
    }

    private void CreateSinglePieceHintOutline(
        DraggablePieceState state,
        Color outlineColor)
    {
        var grooveRect = state.GrooveRect;
        var outlineObject = new GameObject(
            PieceHintOutlineObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(HintDashedOutlineGraphic));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(grooveRect.parent, false);
        outlineRect.anchorMin = grooveRect.anchorMin;
        outlineRect.anchorMax = grooveRect.anchorMax;
        outlineRect.pivot = grooveRect.pivot;
        outlineRect.anchoredPosition = grooveRect.anchoredPosition;
        outlineRect.sizeDelta = grooveRect.sizeDelta;
        outlineRect.localRotation = grooveRect.localRotation;
        outlineRect.localScale = grooveRect.localScale;
        outlineRect.SetAsLastSibling();

        var outlineGraphic = outlineObject.GetComponent<HintDashedOutlineGraphic>();
        outlineGraphic.maskable = false;
        outlineGraphic.Configure(
            state.GrooveImage.sprite,
            outlineColor,
            HintOutlineWidth,
            HintDashLength,
            HintDashGap,
            HintOutlineScrollSpeed,
            state.GrooveImage.preserveAspect);
        _pieceHintOutlineRoot = outlineObject;
    }

    private static bool TryGetRectInParent(
        RectTransform source,
        RectTransform parent,
        out Rect rect)
    {
        rect = default;
        if (source == null || parent == null)
        {
            return false;
        }

        var corners = new Vector3[4];
        source.GetWorldCorners(corners);
        var first = parent.InverseTransformPoint(corners[0]);
        var min = new Vector2(first.x, first.y);
        var max = min;
        for (var i = 1; i < corners.Length; i++)
        {
            var point = parent.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return rect.width > 0.001f && rect.height > 0.001f;
    }

    private static Rect UnionHintRects(Rect first, Rect second)
    {
        return Rect.MinMaxRect(
            Mathf.Min(first.xMin, second.xMin),
            Mathf.Min(first.yMin, second.yMin),
            Mathf.Max(first.xMax, second.xMax),
            Mathf.Max(first.yMax, second.yMax));
    }

    private void RestoreAllHintedPieceRotations()
    {
        for (var i = 0; i < _hintedPieces.Count && i < _hintedPieceBaseRotations.Count; i++)
        {
            if (_hintedPieces[i]?.PieceRenderer != null)
            {
                _hintedPieces[i].PieceRenderer.transform.rotation =
                    _hintedPieceBaseRotations[i];
                if (_hintedCluster != null && i < _hintedPieceBasePositions.Count)
                {
                    _hintedPieces[i].PieceRenderer.transform.position =
                        _hintedPieceBasePositions[i];
                }
            }
        }

        if (_hintedCluster?.ShadowRenderer != null)
        {
            _hintedCluster.ShadowRenderer.transform.position =
                _hintedClusterShadowBasePosition;
            _hintedCluster.ShadowRenderer.transform.rotation =
                _hintedClusterShadowBaseRotation;
        }
    }

    private static bool TryGetPieceRendererBounds(
        IReadOnlyList<DraggablePieceState> states,
        out Bounds bounds)
    {
        bounds = default;
        var hasBounds = false;
        for (var i = 0; i < states.Count; i++)
        {
            var renderer = states[i]?.PieceRenderer;
            if (renderer == null)
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

        return hasBounds;
    }

    private void ClearPieceHint()
    {
        RestoreAllHintedPieceRotations();

        _hintedPiece = null;
        _hintedPieces.Clear();
        _hintedPieceBaseRotations.Clear();
        _hintedPieceBasePositions.Clear();
        _hintedCluster = null;
        _hintedClusterCenter = Vector3.zero;
        _hintedClusterShadowBasePosition = Vector3.zero;
        _hintedClusterShadowBaseRotation = Quaternion.identity;
        _hintShakeStartTime = 0f;
        _isHintPieceShaking = false;
        if (_pieceHintOutlineRoot != null)
        {
            _pieceHintOutlineRoot.SetActive(false);
            Destroy(_pieceHintOutlineRoot);
            _pieceHintOutlineRoot = null;
        }
    }

    private void StartGameplayTimerIfNeeded()
    {
        if (_hasGameplayTimerStarted || _isGameFinished)
        {
            return;
        }

        _hasGameplayTimerStarted = true;
        _gameplayStartRealtime = Time.realtimeSinceStartup;
        Debug.Log("GameScene: gameplay score timer started after first Piece placement.");
    }

    private void StopGameplayTimer()
    {
        if (!_hasGameplayTimerStarted)
        {
            _completionTimeSeconds = 0f;
            return;
        }

        _completionTimeSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _gameplayStartRealtime);
        Debug.Log($"GameScene: gameplay score timer stopped at {_completionTimeSeconds:F2}s.");
    }

    private void InitializeTaskTracking()
    {
        var task = default(TaskInstanceData);
        _isTaskTrackingActive = GameTaskUtility.Initialize();
        if (_isTaskTrackingActive)
        {
            _isTaskTrackingActive = GameTaskUtility.TryGetCurrentTask(out task);
        }

        if (_isTaskTrackingActive)
        {
            Debug.Log(
                $"GameScene: task active. taskInstanceId={task.TaskInstanceId}, " +
                $"templateId={task.TemplateId}, taskType={task.TaskType}, " +
                $"requiredPackSize={task.RequiredPackSize}, target={task.CompleteValue}, " +
                $"progress={GameTaskUtility.GetCurrentCompleteValue()}");
        }
    }

    private bool SaveCardPackAfterPuzzleComplete()
    {
        var packId = GameManager.GetBagId();
        if (packId <= 0)
        {
            return false;
        }

        if (!CardPackDataUtility.TrySavePackAfterPuzzleComplete(packId))
        {
            Debug.LogWarning($"GameScene: failed to save card pack data after puzzle complete. packId={packId}");
            return false;
        }

        if (!CardPackDataUtility.TryClearPuzzleSession(packId))
        {
            Debug.LogWarning($"GameScene: failed to clear completed puzzle session. packId={packId}");
        }

        Debug.Log($"GameScene: card pack data saved after puzzle complete. packId={packId}");
        return true;
    }

    private void TryGrantFirstCompletionPackReward()
    {
        using var taskData = SettlementTaskDataMarker.Auto();
        var completedPackId = GameManager.GetBagId();
        if (_wasSelectedPackCompletedOnEntry)
        {
            Debug.Log($"GameScene: completion pack reward skipped for replay. packId={completedPackId}");
            return;
        }

        var granted = CardPackDistributionUtility.TryGrantFirstCompletionReward(
            completedPackId,
            out var grantedPackId,
            out var chapterId,
            out var decision);
        Debug.Log(
            $"GameScene: first-completion pack reward evaluated. completedPackId={completedPackId}, " +
            $"chapter={chapterId}, stage={decision.Stage}, R={decision.RemainingLockedCount}, " +
            $"held={decision.HeldPlayableCount}, maxHeldBeforeGrant={decision.MaximumHeldBeforeGrant}, " +
            $"expectedHeldAfterGrant={decision.ExpectedHeldAfterGrant}, granted={granted}, " +
            $"grantedPackId={grantedPackId}");
        if (granted)
        {
            QueuePackReward(grantedPackId, SettlementPackRewardSource.Completion);
        }
    }

    private IEnumerator ProcessTaskSettlement()
    {
        // Let the prepared RewardPanel reach the screen before synchronous local persistence.
        yield return null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var persistenceStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
#endif
        using (SettlementPersistenceMarker.Auto())
        {
            _didSavePackCompletion = SaveCardPackAfterPuzzleComplete();
            _isFirstCompletionSettlement = _didSavePackCompletion
                                           && !_wasSelectedPackCompletedOnEntry;
            _settlementBagCountAfterCompletion = _settlementBagCountBeforeCompletion
                                                 + (_isFirstCompletionSettlement ? 1 : 0);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogSettlementPerformance("completion persisted", persistenceStartedAt);
#endif

        var boardFitAnimation = StartCoroutine(AnimateCompletedBoardForRewardPanel());
        yield return AnimateSettlementHeaderEntrance();
        yield return boardFitAnimation;
        yield return ProcessTaskSettlementCore();

        if (_didSavePackCompletion
            && !_didFailTaskAdvanceDuringSettlement)
        {
            TryGrantPendingTaskPackReward(
                _didAdvanceTaskDuringSettlement
                    ? _wasSelectedPackCompletedOnEntry
                        ? "task completion during replay"
                        : "task completion"
                    : _wasSelectedPackCompletedOnEntry
                        ? "replay retry"
                        : "first-completion retry");
        }

        if (_didSavePackCompletion)
        {
            TryGrantFirstCompletionPackReward();
        }

        RebuildSettlementPackRewardIds();
        if (_isFirstCompletionSettlement)
        {
            yield return AnimateSettlementBagCountIncrement();
        }
        else
        {
            RefreshSettlementBagCount();
        }

        if (_settlementCompletionRewardPackId > 0
            || _settlementTaskRewardPackId > 0
            || _didQueueTaskRewardDuringSettlement)
        {
            yield return AnimateSettlementPackRewards();
        }

        yield return AnimateSettlementActionButtonsEntrance();

        _isSettlementReadyForFinish = true;
        SetSettlementInputLocked(false);
        SetSettlementActionButtonsInteractable(true);
    }

    private void SetSettlementInputLocked(bool locked)
    {
        if (_rewardPanelRoot == null)
        {
            return;
        }

        if (_rewardPanelCanvasGroup == null)
        {
            _rewardPanelCanvasGroup = _rewardPanelRoot.GetComponent<CanvasGroup>();
            if (_rewardPanelCanvasGroup == null)
            {
                _rewardPanelCanvasGroup = _rewardPanelRoot.AddComponent<CanvasGroup>();
            }
        }

        _rewardPanelCanvasGroup.interactable = !locked;
        _rewardPanelCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator ProcessTaskSettlementCore()
    {
        if (_rewardPanelRoot == null)
        {
            yield break;
        }

        SetSettlementScore(0);
        SetSettlementBagCount(_settlementBagCountBeforeCompletion);

        var packId = GameManager.GetBagId();
        var scoreContext = new GameScoreContext
        {
            WasHintUsed = _wasHintUsed,
            IsLevelOutlineEnabled = _isLevelOutlineEnabled,
            IsStickerOutlineEnabled = _isStickerOutlineEnabled,
            IsReplay = _wasSelectedPackCompletedOnEntry,
            CompletionTimeSeconds = _completionTimeSeconds
        };
        if (!GameScoreUtility.TryCalculateCardPackScore(packId, scoreContext, out var scoreResult))
        {
            Debug.LogWarning($"GameScene: cannot calculate settlement score. Invalid card pack config. packId={packId}");
            SetTaskRewardSectionVisible(false);
            yield break;
        }

        var settlementScore = scoreResult.FinalScore;
        Debug.Log(
            $"GameScene: score calculated. replay={_wasSelectedPackCompletedOnEntry}, " +
            $"originalBase={scoreResult.OriginalBaseScore}, base={scoreResult.BaseScore}, " +
            $"noHint=+{scoreResult.NoHintBonusPercent}%, " +
            $"levelOutlineOff=+{scoreResult.LevelOutlineDisabledBonusPercent}%, " +
            $"stickerOutlineOff=+{scoreResult.StickerOutlineDisabledBonusPercent}%, " +
            $"time=+{scoreResult.CompletionTimeBonusPercent}% ({scoreResult.CompletionTimeSeconds:F2}s), " +
            $"total=+{scoreResult.TotalBonusPercent}%, final={scoreResult.FinalScore}");

        if (_didSavePackCompletion)
        {
            AnalyticsManager.Instance.CompleteCardBag(
                packId,
                settlementScore,
                _settlementBagCountAfterCompletion);
        }

        if (!_isTaskTrackingActive
            || !GameTaskUtility.TryGetCurrentTask(out var task))
        {
            SetTaskRewardSectionVisible(false);
            yield return AnimateTaskSettlementProgress(
                null,
                null,
                0,
                0,
                false,
                scoreResult);
            yield break;
        }

        var progressBeforeSettlement = GameTaskUtility.GetCurrentCompleteValue();
        var stickerCount = CountGrooveImages(_board.GrooveImagesByGroup);
        bool taskApplied;
        int taskContribution;
        using (SettlementTaskDataMarker.Auto())
        {
            taskApplied = GameTaskUtility.ApplyCompletedPack(
                packId,
                stickerCount,
                settlementScore,
                _wasSelectedPackCompletedOnEntry,
                out taskContribution);
        }

        if (!taskApplied)
        {
            Debug.LogWarning(
                $"GameScene: failed to apply completed pack to current task. " +
                $"packId={packId}, score={settlementScore}, stickers={stickerCount}");
            SetTaskRewardSectionVisible(false);
            yield return AnimateTaskSettlementProgress(
                null,
                null,
                0,
                0,
                false,
                scoreResult);
            yield break;
        }

        var progressAfterSettlement = GameTaskUtility.GetCurrentCompleteValue();
        var isTaskCompleted = GameTaskUtility.IsCurrentTaskCompleted();
        Debug.Log(
            $"GameScene: completed pack applied to task. packId={packId}, " +
            $"taskType={task.TaskType}, contribution={taskContribution}, " +
            $"progress={progressAfterSettlement}/{task.CompleteValue}");

        SetTaskRewardSectionVisible(true);
        var taskItem = _rewardTaskItem;
        TaskProgressUIUtility.RefreshTask(
            taskItem,
            task,
            progressBeforeSettlement,
            isTaskCompleted);

        if (isTaskCompleted)
        {
            if (QueueTaskReward(task))
            {
                bool taskAdvanced;
                using (SettlementTaskDataMarker.Auto())
                {
                    taskAdvanced = GameTaskUtility.TryCompleteAndAdvanceTask(
                        packId,
                        _wasSelectedPackCompletedOnEntry);
                }

                if (taskAdvanced)
                {
                    _didAdvanceTaskDuringSettlement = true;
                    _isTaskTrackingActive = GameTaskUtility.TryGetCurrentTask(out var nextTask);
                    Debug.Log(
                        $"GameScene: task advanced. taskInstanceId={nextTask.TaskInstanceId}, " +
                        $"templateId={nextTask.TemplateId}");
                }
                else
                {
                    _didFailTaskAdvanceDuringSettlement = true;
                    Debug.LogError(
                        $"GameScene: task reward queued but task advance failed. " +
                        $"taskInstanceId={task.TaskInstanceId}");
                }
            }
        }

        yield return AnimateTaskSettlementProgress(
            taskItem,
            task,
            progressBeforeSettlement,
            progressAfterSettlement,
            task.TaskType == TaskType.AccumulateScore && taskContribution > 0,
            scoreResult);
    }

    private bool QueueTaskReward(TaskInstanceData task)
    {
        using var taskData = SettlementTaskDataMarker.Auto();
        var preferredPackId = task.RewardType == RewardType.CardPack
            ? task.RewardId
            : 0;
        if (!CardPackDistributionUtility.EnqueueTaskReward(task.TaskInstanceId, preferredPackId))
        {
            Debug.LogError(
                $"GameScene: failed to persist guaranteed task reward. " +
                $"taskInstanceId={task.TaskInstanceId}, preferredPackId={preferredPackId}");
            return false;
        }

        Debug.Log(
            $"GameScene: guaranteed task reward queued. taskInstanceId={task.TaskInstanceId}, " +
            $"templateId={task.TemplateId}, " +
            $"preferredPackId={preferredPackId}, " +
            $"pending={CardPackDistributionUtility.GetPendingTaskRewardCount()}");
        _didQueueTaskRewardDuringSettlement = true;
        return true;
    }

    private void TryGrantPendingTaskPackReward(string source)
    {
        using var taskData = SettlementTaskDataMarker.Auto();
        var granted = CardPackDistributionUtility.TryGrantPendingTaskReward(
            out var rewardPackId,
            out var chapterId,
            out var decision);
        Debug.Log(
            $"GameScene: pending task reward evaluated. source={source}, chapter={chapterId}, " +
            $"stage={decision.Stage}, R={decision.RemainingLockedCount}, " +
            $"held={decision.HeldPlayableCount}, maxHeldBeforeGrant={decision.MaximumHeldBeforeGrant}, " +
            $"granted={granted}, grantedPackId={rewardPackId}, " +
            $"pending={CardPackDistributionUtility.GetPendingTaskRewardCount()}");
        if (!granted)
        {
            return;
        }

        QueuePackReward(rewardPackId, SettlementPackRewardSource.Task);
    }

    private void SetTaskRewardSectionVisible(bool visible)
    {
        if (_rewardTaskItem != null)
        {
            _rewardTaskItem.gameObject.SetActive(visible);
        }
    }

    private IEnumerator AnimateTaskSettlementProgress(
        Transform taskItem,
        TaskInstanceData? task,
        int progressBeforeSettlement,
        int progressAfterSettlement,
        bool syncTaskWithScore,
        GameScoreResult scoreResult)
    {
        SetSettlementTaskProgress(taskItem, task, progressBeforeSettlement);
        SetSettlementScore(0);
        HideSettlementTitles();
        var animateIndependentTaskProgress = !syncTaskWithScore
                                             && taskItem != null
                                             && task.HasValue
                                             && progressAfterSettlement != progressBeforeSettlement;
        var hasNoHintBonus = scoreResult.NoHintBonusPercent > 0;
        var hasLevelOutlineBonus = scoreResult.LevelOutlineDisabledBonusPercent > 0;
        var hasStickerOutlineBonus = scoreResult.StickerOutlineDisabledBonusPercent > 0;
        var hasCompletionTimeBonus = scoreResult.CompletionTimeBonusPercent > 0;
        var hasAnyBonus = hasNoHintBonus
                          || hasLevelOutlineBonus
                          || hasStickerOutlineBonus
                          || hasCompletionTimeBonus;
        var didAnimateIndependentTaskProgress = animateIndependentTaskProgress && !hasAnyBonus;
        yield return AnimateSettlementScoreRange(
            taskItem,
            task,
            progressBeforeSettlement,
            0,
            scoreResult.BaseScore,
            SettlementBaseRollDuration,
            syncTaskWithScore,
            didAnimateIndependentTaskProgress,
            progressAfterSettlement);

        var currentScore = scoreResult.BaseScore;
        var cumulativeBonusPercent = 0;
        if (hasNoHintBonus)
        {
            cumulativeBonusPercent += scoreResult.NoHintBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                scoreResult.OriginalBaseScore,
                cumulativeBonusPercent);
            var animateTaskProgressHere = animateIndependentTaskProgress
                                          && !hasLevelOutlineBonus
                                          && !hasStickerOutlineBonus
                                          && !hasCompletionTimeBonus;
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                "未使用提示",
                targetScore - currentScore,
                animateTaskProgressHere,
                progressAfterSettlement);
            didAnimateIndependentTaskProgress |= animateTaskProgressHere;
            currentScore = targetScore;
        }

        if (hasLevelOutlineBonus)
        {
            cumulativeBonusPercent += scoreResult.LevelOutlineDisabledBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                scoreResult.OriginalBaseScore,
                cumulativeBonusPercent);
            var animateTaskProgressHere = animateIndependentTaskProgress
                                          && !hasStickerOutlineBonus
                                          && !hasCompletionTimeBonus;
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                "关闭关卡描边",
                targetScore - currentScore,
                animateTaskProgressHere,
                progressAfterSettlement);
            didAnimateIndependentTaskProgress |= animateTaskProgressHere;
            currentScore = targetScore;
        }

        if (hasStickerOutlineBonus)
        {
            cumulativeBonusPercent += scoreResult.StickerOutlineDisabledBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                scoreResult.OriginalBaseScore,
                cumulativeBonusPercent);
            var animateTaskProgressHere = animateIndependentTaskProgress
                                          && !hasCompletionTimeBonus;
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                "关闭贴纸描边",
                targetScore - currentScore,
                animateTaskProgressHere,
                progressAfterSettlement);
            didAnimateIndependentTaskProgress |= animateTaskProgressHere;
            currentScore = targetScore;
        }

        if (hasCompletionTimeBonus)
        {
            cumulativeBonusPercent += scoreResult.CompletionTimeBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                scoreResult.OriginalBaseScore,
                cumulativeBonusPercent);
            var animateTaskProgressHere = animateIndependentTaskProgress;
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                "快速完成",
                targetScore - currentScore,
                animateTaskProgressHere,
                progressAfterSettlement);
            didAnimateIndependentTaskProgress |= animateTaskProgressHere;
            currentScore = targetScore;
        }

        if (currentScore != scoreResult.FinalScore)
        {
            var animateTaskProgressHere = animateIndependentTaskProgress
                                          && !didAnimateIndependentTaskProgress;
            yield return AnimateSettlementScoreRange(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                scoreResult.FinalScore,
                TaskProgressRollDuration,
                syncTaskWithScore,
                animateTaskProgressHere,
                progressAfterSettlement);
            didAnimateIndependentTaskProgress |= animateTaskProgressHere;
        }

        SetSettlementScore(scoreResult.FinalScore);
        ShowSettlementBagCountTitle();
        if (animateIndependentTaskProgress && !didAnimateIndependentTaskProgress)
        {
            yield return AnimateSettlementTaskProgressRange(
                taskItem,
                task,
                progressBeforeSettlement,
                progressAfterSettlement,
                TaskProgressRollDuration);
        }

        SetSettlementTaskProgress(taskItem, task, progressAfterSettlement);
        yield return new WaitForSecondsRealtime(SettlementFinalPauseDuration);
    }

    private IEnumerator AnimateSettlementBonusStage(
        Transform taskItem,
        TaskInstanceData? task,
        int progressBeforeSettlement,
        int fromScore,
        int toScore,
        bool syncTaskWithScore,
        string title,
        int bonusScore,
        bool animateIndependentTaskProgress,
        int independentTaskProgressTo)
    {
        ShowSettlementBonus(title, bonusScore);
        yield return new WaitForSecondsRealtime(SettlementStagePauseDuration);
        yield return AnimateSettlementScoreRange(
            taskItem,
            task,
            progressBeforeSettlement,
            fromScore,
            toScore,
            SettlementBonusRollDuration,
            syncTaskWithScore,
            animateIndependentTaskProgress,
            independentTaskProgressTo);
    }

    private IEnumerator AnimateSettlementScoreRange(
        Transform taskItem,
        TaskInstanceData? task,
        int progressBeforeSettlement,
        int fromScore,
        int toScore,
        float duration,
        bool syncTaskWithScore,
        bool animateIndependentTaskProgress = false,
        int independentTaskProgressTo = 0)
    {
        if (duration <= 0f)
        {
            SetSettlementScore(toScore);
            if (syncTaskWithScore)
            {
                SetSettlementTaskProgress(
                    taskItem,
                    task,
                    progressBeforeSettlement + toScore);
            }
            else if (animateIndependentTaskProgress)
            {
                SetSettlementTaskProgress(
                    taskItem,
                    task,
                    independentTaskProgressTo);
            }
            yield break;
        }

        if (toScore != fromScore)
        {
            AudioManager.Instance.PlaySfx("SFX_ScoreIncrease.mp3");
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / duration);
            var easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            var animatedScore = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, easedTime));
            var animatedTaskProgress = progressBeforeSettlement + animatedScore;

            SetSettlementScore(animatedScore);
            if (syncTaskWithScore)
            {
                SetSettlementTaskProgress(taskItem, task, animatedTaskProgress);
            }
            else if (animateIndependentTaskProgress)
            {
                var taskProgressTime = Mathf.InverseLerp(
                    0.25f,
                    1f,
                    normalizedTime);
                var easedTaskProgressTime = Mathf.SmoothStep(
                    0f,
                    1f,
                    taskProgressTime);
                SetSettlementTaskProgress(
                    taskItem,
                    task,
                    Mathf.RoundToInt(Mathf.Lerp(
                        progressBeforeSettlement,
                        independentTaskProgressTo,
                        easedTaskProgressTime)));
            }
            yield return null;
        }

        SetSettlementScore(toScore);
        if (syncTaskWithScore)
        {
            SetSettlementTaskProgress(taskItem, task, progressBeforeSettlement + toScore);
        }
        else if (animateIndependentTaskProgress)
        {
            SetSettlementTaskProgress(taskItem, task, independentTaskProgressTo);
        }
    }

    private static IEnumerator AnimateSettlementTaskProgressRange(
        Transform taskItem,
        TaskInstanceData? task,
        int fromProgress,
        int toProgress,
        float duration)
    {
        if (duration <= 0f)
        {
            SetSettlementTaskProgress(taskItem, task, toProgress);
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / duration);
            var easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            var progress = Mathf.RoundToInt(Mathf.Lerp(fromProgress, toProgress, easedTime));
            SetSettlementTaskProgress(taskItem, task, progress);
            yield return null;
        }

        SetSettlementTaskProgress(taskItem, task, toProgress);
    }

    private static int CalculateSettlementStageScore(
        int awardedBaseScore,
        int originalBaseScore,
        int cumulativeBonusPercent)
    {
        var cumulativeBonusScore = originalBaseScore * cumulativeBonusPercent;
        cumulativeBonusScore = cumulativeBonusScore <= 0
            ? 0
            : (cumulativeBonusScore + 99) / 100;
        return awardedBaseScore + cumulativeBonusScore;
    }

    private static void SetSettlementTaskProgress(
        Transform taskItem,
        TaskInstanceData? task,
        int progress)
    {
        if (taskItem != null && task.HasValue)
        {
            TaskProgressUIUtility.SetProgress(taskItem, task.Value, progress);
        }
    }

    private void HideSettlementTitles()
    {
        if (_settlementBagCountTitleText != null)
        {
            _settlementBagCountTitleText.gameObject.SetActive(false);
        }

        HideSettlementBonusTexts();
    }

    private void ShowSettlementBonus(string title, int bonusScore)
    {
        if (_settlementBagCountTitleText != null)
        {
            _settlementBagCountTitleText.gameObject.SetActive(false);
        }

        if (_settlementBonusTitleText != null)
        {
            _settlementBonusTitleText.text = title;
            _settlementBonusTitleText.gameObject.SetActive(true);
        }

        if (_settlementBonusScoreText != null)
        {
            _settlementBonusScoreText.text = $"+{Mathf.Max(0, bonusScore)}分";
            _settlementBonusScoreText.gameObject.SetActive(true);
        }
    }

    private void ShowSettlementBagCountTitle()
    {
        HideSettlementBonusTexts();
        if (_settlementBagCountTitleText != null)
        {
            _settlementBagCountTitleText.gameObject.SetActive(true);
        }
    }

    private void HideSettlementBonusTexts()
    {
        if (_settlementBonusTitleText != null)
        {
            _settlementBonusTitleText.text = string.Empty;
            _settlementBonusTitleText.gameObject.SetActive(false);
        }

        if (_settlementBonusScoreText != null)
        {
            _settlementBonusScoreText.text = string.Empty;
            _settlementBonusScoreText.gameObject.SetActive(false);
        }
    }

    private static void ConfigureSettlementSingleLineText(TMP_Text text)
    {
        var configuredFontSize = text.fontSize;
        text.enableWordWrapping = false;
        text.enableAutoSizing = true;
        text.fontSizeMax = configuredFontSize;
        text.fontSizeMin = Mathf.Min(text.fontSizeMin, configuredFontSize);
    }

    private void SetSettlementScore(int score)
    {
        if (_settlementScoreText == null)
        {
            return;
        }

        _settlementScoreText.text = Mathf.Max(0, score).ToString();
    }

    private void RefreshSettlementBagCount()
    {
        SetSettlementBagCount(_settlementBagCountAfterCompletion);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static void LogSettlementPerformance(string operation, long startedAt)
    {
        var elapsedMilliseconds =
            (System.Diagnostics.Stopwatch.GetTimestamp() - startedAt)
            * 1000d
            / System.Diagnostics.Stopwatch.Frequency;
        Debug.Log($"GameScene: settlement performance. operation={operation}, elapsed={elapsedMilliseconds:F2}ms");
    }
#endif

    private void SetSettlementBagCount(int count)
    {
        if (_settlementBagCountText != null)
        {
            _settlementBagCountText.text = Mathf.Max(0, count).ToString();
        }
    }

    private void QueuePackReward(
        int rewardPackId,
        SettlementPackRewardSource source)
    {
        if (rewardPackId <= 0)
        {
            return;
        }

        if (source == SettlementPackRewardSource.Completion)
        {
            _settlementCompletionRewardPackId = rewardPackId;
        }
        else
        {
            _settlementTaskRewardPackId = rewardPackId;
        }
    }

    private void RebuildSettlementPackRewardIds()
    {
        _settlementPackRewardIds.Clear();
        if (_settlementCompletionRewardPackId > 0)
        {
            _settlementPackRewardIds.Add(_settlementCompletionRewardPackId);
        }

        if (_settlementTaskRewardPackId > 0
            && !_settlementPackRewardIds.Contains(_settlementTaskRewardPackId))
        {
            _settlementPackRewardIds.Add(_settlementTaskRewardPackId);
        }
    }

    private void FitCameraToActiveGroup(int activeGroupIndex)
    {
        var camera = Camera.main;
        if (camera == null || _board.GameBoardImage == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);

        var activeGroupBounds = BuildActiveGroupBounds(activeGroupIndex);
        var pageBounds = BuildPageBoundsForActiveGroup(activeGroupBounds);
        if (pageBounds.size.sqrMagnitude <= 0f)
        {
            return;
        }

        GameCommonUtility.FitOrthographicCameraSizeOnly(camera, GamePageCameraPadding, pageBounds);
        Canvas.ForceUpdateCanvases();
        AlignPieceTrayToPageBottom();
        CenterCardBagOnActivePage(camera, activeGroupIndex);
    }

    private void CenterCardBagOnActivePage(Camera camera, int activeGroupIndex)
    {
        if (_loadedCardBagRect == null
            || !_hasOriginalCardBagAnchoredPosition
            || !TryBuildActiveGroupScreenRect(camera, activeGroupIndex, out var groupScreenRect)
            || !TryGetAvailableBoardScreenCenter(camera, out var targetScreenCenter))
        {
            return;
        }

        var parentRect = _loadedCardBagRect.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        var canvas = _loadedCardBagRect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera ?? camera : camera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                groupScreenRect.center,
                eventCamera,
                out var groupLocalCenter)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                targetScreenCenter,
                eventCamera,
                out var targetLocalCenter))
        {
            return;
        }

        _loadedCardBagRect.anchoredPosition += targetLocalCenter - groupLocalCenter;
        Canvas.ForceUpdateCanvases();
        ClampBoardToTrayGap(camera, parentRect, eventCamera);
    }

    private void ClampBoardToTrayGap(
        Camera camera,
        RectTransform cardBagParent,
        Camera eventCamera)
    {
        var isTrayVisible = _board.PieceBoardRect != null
            ? _board.PieceBoardRect.gameObject.activeInHierarchy && !_isPieceBoardHidden
            : _board.PieceBgRenderer != null
              && _board.PieceBgRenderer.gameObject.activeInHierarchy
              && !_isPieceBgHidden;
        if (_loadedCardBagRect == null
            || _board.GameBoardImage == null
            || cardBagParent == null
            || !isTrayVisible
            || !TryGetPieceTrayScreenRect(camera, out var trayScreenRect))
        {
            return;
        }

        var boardScreenRect = GetRectTransformScreenRect(
            _board.GameBoardImage.rectTransform,
            camera);
        var backgroundScreenRect = _board.BackgroundRect != null
            ? GetRectTransformScreenRect(_board.BackgroundRect, camera)
            : Rect.MinMaxRect(0f, 0f, Screen.width, Screen.height);
        if (boardScreenRect.height <= 0f || backgroundScreenRect.height <= 0f)
        {
            return;
        }

        var currentGap = boardScreenRect.yMin - trayScreenRect.yMax;
        var maxGap = backgroundScreenRect.height * MaxBoardToTrayGapViewportRatio;
        var excessGap = currentGap - maxGap;
        if (excessGap <= 0.5f)
        {
            return;
        }

        var currentScreenPoint = boardScreenRect.center;
        var targetScreenPoint = currentScreenPoint - Vector2.up * excessGap;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cardBagParent,
                currentScreenPoint,
                eventCamera,
                out var currentLocalPoint)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cardBagParent,
                targetScreenPoint,
                eventCamera,
                out var targetLocalPoint))
        {
            return;
        }

        _loadedCardBagRect.anchoredPosition += targetLocalPoint - currentLocalPoint;
        Canvas.ForceUpdateCanvases();
    }

    private bool TryBuildActiveGroupScreenRect(
        Camera camera,
        int activeGroupIndex,
        out Rect screenRect)
    {
        screenRect = default;
        if (_board.GrooveImagesByGroup == null
            || activeGroupIndex < 0
            || activeGroupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return false;
        }

        var group = _board.GrooveImagesByGroup[activeGroupIndex];
        var hasRect = false;
        if (group != null)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var grooveImage = group[i];
                if (grooveImage == null || !grooveImage.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var grooveScreenRect = GetRectTransformScreenRect(grooveImage.rectTransform, camera);
                screenRect = hasRect ? UnionRects(screenRect, grooveScreenRect) : grooveScreenRect;
                hasRect = true;
            }
        }

        return hasRect;
    }

    private bool TryGetAvailableBoardScreenCenter(Camera camera, out Vector2 screenCenter)
    {
        var backgroundRect = _board.BackgroundRect != null
            ? GetRectTransformScreenRect(_board.BackgroundRect, camera)
            : Rect.MinMaxRect(0f, 0f, Screen.width, Screen.height);
        var availableBottom = backgroundRect.yMin;

        if (_board.PieceBoardRect != null
            && _board.PieceBoardRect.gameObject.activeInHierarchy
            && !_isPieceBoardHidden)
        {
            availableBottom = GetRectTransformScreenRect(_board.PieceBoardRect, camera).yMax;
        }
        else if (_board.PieceBgRenderer != null
                 && _board.PieceBgRenderer.gameObject.activeInHierarchy
                 && !_isPieceBgHidden)
        {
            availableBottom = camera.WorldToScreenPoint(
                new Vector3(
                    _board.PieceBgRenderer.bounds.center.x,
                    _board.PieceBgRenderer.bounds.max.y,
                    _board.PieceBgRenderer.bounds.center.z)).y;
        }

        availableBottom = Mathf.Clamp(availableBottom, backgroundRect.yMin, backgroundRect.yMax);
        screenCenter = new Vector2(
            backgroundRect.center.x,
            (availableBottom + backgroundRect.yMax) * 0.5f);
        return backgroundRect.width > 0f && backgroundRect.height > 0f;
    }

    private static Rect GetRectTransformScreenRect(RectTransform rectTransform, Camera fallbackCamera)
    {
        var canvas = rectTransform.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera ?? fallbackCamera : fallbackCamera;
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var first = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        var xMin = first.x;
        var xMax = first.x;
        var yMin = first.y;
        var yMax = first.y;
        for (var i = 1; i < corners.Length; i++)
        {
            var point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
            xMin = Mathf.Min(xMin, point.x);
            xMax = Mathf.Max(xMax, point.x);
            yMin = Mathf.Min(yMin, point.y);
            yMax = Mathf.Max(yMax, point.y);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Rect UnionRects(Rect left, Rect right)
    {
        return Rect.MinMaxRect(
            Mathf.Min(left.xMin, right.xMin),
            Mathf.Min(left.yMin, right.yMin),
            Mathf.Max(left.xMax, right.xMax),
            Mathf.Max(left.yMax, right.yMax));
    }

    private Bounds BuildActiveGroupBounds(int activeGroupIndex)
    {
        var camera = Camera.main;
        var hasBounds = false;
        var combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        if (_board.GrooveImagesByGroup != null && camera != null
            && activeGroupIndex >= 0
            && activeGroupIndex < _board.GrooveImagesByGroup.Count)
        {
            var group = _board.GrooveImagesByGroup[activeGroupIndex];
            if (group != null)
            {
                for (var i = 0; i < group.Count; i++)
                {
                    var grooveImage = group[i];
                    if (grooveImage == null || !grooveImage.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    var grooveBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                        grooveImage.rectTransform,
                        camera,
                        WorldGameplayDepth);
                    if (!hasBounds)
                    {
                        combinedBounds = grooveBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(grooveBounds);
                    }
                }
            }
        }

        return hasBounds ? combinedBounds : new Bounds(Vector3.zero, Vector3.zero);
    }

    private Bounds BuildPageBoundsForActiveGroup(Bounds activeGroupBounds)
    {
        var camera = Camera.main;
        var hasBounds = activeGroupBounds.size.sqrMagnitude > 0f;
        var combinedBounds = activeGroupBounds;
        if (!hasBounds && _board.GameBoardImage != null && camera != null)
        {
            combinedBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.GameBoardImage.rectTransform,
                camera,
                WorldGameplayDepth);
            hasBounds = true;
        }

        if (_board.PieceBoardRect != null && camera != null)
        {
            var pieceBoardBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.PieceBoardRect,
                camera,
                WorldGameplayDepth);
            if (!hasBounds)
            {
                combinedBounds = pieceBoardBounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(pieceBoardBounds);
            }
        }
        else if (_board.PieceBgRenderer != null)
        {
            if (!hasBounds)
            {
                combinedBounds = _board.PieceBgRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(_board.PieceBgRenderer.bounds);
            }
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot != null)
        {
            var placedRenderers = placedRoot.GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < placedRenderers.Length; i++)
            {
                var renderer = placedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return combinedBounds;
    }

    private void ResetBoardPanState()
    {
        RestoreGameBoardLayout();
        ResetPlacedPiecesRootPan();
    }

    private void RestoreGameBoardLayout()
    {
        if (_loadedCardBagRect != null && _hasOriginalCardBagAnchoredPosition)
        {
            _loadedCardBagRect.anchoredPosition = _originalCardBagAnchoredPosition;
        }

        if (!_hasOriginalGameBoardAnchoredPosition || _board.GameBoardImage == null)
        {
            return;
        }

        _board.GameBoardImage.rectTransform.anchoredPosition = _originalGameBoardAnchoredPosition;
    }

    private static void ResetPlacedPiecesRootPan()
    {
        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot == null)
        {
            return;
        }

        var panOffset = placedRoot.transform.position;
        if (panOffset.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        var renderers = placedRoot.GetComponentsInChildren<SpriteRenderer>(true);
        var worldPositions = new Vector3[renderers.Length];
        for (var i = 0; i < renderers.Length; i++)
        {
            worldPositions[i] = renderers[i].transform.position;
        }

        placedRoot.transform.position = Vector3.zero;
        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].transform.position = worldPositions[i];
            }
        }
    }

    private static int CountGrooveImages(List<List<Image>> grooveGroups)
    {
        if (grooveGroups == null)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < grooveGroups.Count; i++)
        {
            total += grooveGroups[i]?.Count ?? 0;
        }

        return total;
    }

    private SpriteRenderer CreateSpriteObject(
        string objectName,
        string spritePath,
        int sortingOrder,
        Transform parent,
        bool forceCreate = false)
    {
        return GameCommonUtility.CreateSpriteRendererObject(
            objectName,
            spritePath,
            sortingOrder,
            PixelsPerUnit,
            parent,
            forceCreate);
    }

    private static void CreateOrUpdatePieceBgFill(SpriteRenderer pieceBgRenderer)
    {
        if (pieceBgRenderer == null)
        {
            return;
        }

        var fillObject = GameObject.Find(PieceBgFillObjectName);
        SpriteRenderer fillRenderer;
        if (fillObject == null)
        {
            fillObject = new GameObject(PieceBgFillObjectName);
            fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        }
        else
        {
            fillRenderer = fillObject.GetComponent<SpriteRenderer>();
            if (fillRenderer == null)
            {
                fillRenderer = fillObject.AddComponent<SpriteRenderer>();
            }
        }

        fillRenderer.sprite = GameCommonUtility.CreateSolidSprite(Color.white, PixelsPerUnit);
        fillRenderer.drawMode = SpriteDrawMode.Sliced;
        fillRenderer.size = pieceBgRenderer.size;
        fillRenderer.sortingOrder = PieceBgFillSortingOrder;
        fillRenderer.color = new Color(0f, 0f, 0f, PieceBgFillAlpha);
        fillRenderer.transform.position = new Vector3(
            pieceBgRenderer.transform.position.x,
            pieceBgRenderer.transform.position.y,
            pieceBgRenderer.transform.position.z + 0.01f);
    }

    private void EnsureBackgroundCentered()
    {
        if (_board.BackgroundRect == null)
        {
            return;
        }

        var rect = _board.BackgroundRect;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    private static Vector3 ToGameplayWorld(Vector2 screenPosition)
    {
        var world = GameCommonUtility.ScreenToWorld(screenPosition);
        world.z = WorldGameplayDepth;
        return world;
    }

    private static bool ContainsWorldXY(Bounds bounds, Vector3 worldPosition)
    {
        return worldPosition.x >= bounds.min.x
            && worldPosition.x <= bounds.max.x
            && worldPosition.y >= bounds.min.y
            && worldPosition.y <= bounds.max.y;
    }

    private void CachePieceTrayOriginalPosition()
    {
        if (_board.PieceBoardRect != null)
        {
            _pieceBoardOriginalAnchoredPosition = _board.PieceBoardRect.anchoredPosition;
            _hasPieceBoardOriginalAnchoredPosition = true;
            _isPieceBoardHidden = false;
        }

        CachePieceBgOriginalPosition();
        CachePieceTrayDropScreenRect();
    }

    private void CachePieceTrayDropScreenRect()
    {
        if (!TryGetGameplayViewportRect(out var viewport)
            || !TryGetStablePieceTrayDropScreenRect(out var screenRect))
        {
            return;
        }

        _pieceTrayDropNormalizedViewportRect = Rect.MinMaxRect(
            (screenRect.xMin - viewport.xMin) / viewport.width,
            (screenRect.yMin - viewport.yMin) / viewport.height,
            (screenRect.xMax - viewport.xMin) / viewport.width,
            (screenRect.yMax - viewport.yMin) / viewport.height);
        _hasPieceTrayDropNormalizedViewportRect = true;
    }

    private bool TryGetStablePieceTrayDropScreenRect(out Rect screenRect)
    {
        screenRect = default;
        var camera = Camera.main;
        if (_board.PieceBoardRect != null
            && camera != null
            && TryGetCanvasRectGameplayBounds(
                _board.PieceBoardRect,
                camera,
                out var stableBounds)
            && TryGetWorldBoundsScreenRect(stableBounds, camera, out screenRect))
        {
            return true;
        }

        return TryGetPieceTrayScreenRect(camera, out screenRect);
    }

    private void CachePieceBgOriginalPosition()
    {
        if (_board.PieceBgRenderer == null)
        {
            _hasPieceBgOriginalPosition = false;
            return;
        }

        _pieceBgOriginalPosition = _board.PieceBgRenderer.transform.position;
        _hasPieceBgOriginalPosition = true;
        _isPieceBgHidden = false;
    }

    private void AlignPieceTrayToPageBottom()
    {
        if (_board.PieceBoardRect != null)
        {
            if (_isPieceBoardHidden)
            {
                return;
            }

            CachePieceTrayOriginalPosition();
            return;
        }

        AlignPieceBgToPageBottom();
    }

    private void AlignPieceBgToPageBottom()
    {
        if (_board.PieceBgRenderer == null || _isPieceBgHidden)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var halfHeight = _board.PieceBgRenderer.bounds.extents.y;
        var bottomEdge = camera.transform.position.y - camera.orthographicSize;
        var anchoredPosition = new Vector3(
            camera.transform.position.x,
            bottomEdge + halfHeight,
            _board.PieceBgRenderer.transform.position.z);

        ApplyPieceBgSlidePosition(anchoredPosition, GetPieceBgFillTransform());
        _pieceBgOriginalPosition = anchoredPosition;
        _hasPieceBgOriginalPosition = true;
        CachePieceTrayDropScreenRect();
    }

    private void SlidePieceTrayOutOfScreen()
    {
        if (_board.PieceBoardRect != null)
        {
            SlidePieceBoardOutOfScreen();
            return;
        }

        SlidePieceBgOutOfScreen();
    }

    private void PreparePieceTrayForGroupStart()
    {
        ResetPieceTrayPosition(instant: true);
    }

    private void ResetPieceTrayPosition(bool instant)
    {
        if (_board.PieceBoardRect != null)
        {
            if (_pieceTraySlideCoroutine != null)
            {
                StopCoroutine(_pieceTraySlideCoroutine);
                _pieceTraySlideCoroutine = null;
            }

            if (!_hasPieceBoardOriginalAnchoredPosition)
            {
                CachePieceTrayOriginalPosition();
            }

            if (instant || !_isPieceBoardHidden)
            {
                _board.PieceBoardRect.anchoredPosition = _pieceBoardOriginalAnchoredPosition;
                _isPieceBoardHidden = false;
            }
            else
            {
                SlidePieceBoardToOriginalPosition();
            }

            return;
        }

        if (instant)
        {
            if (_pieceTraySlideCoroutine != null)
            {
                StopCoroutine(_pieceTraySlideCoroutine);
                _pieceTraySlideCoroutine = null;
            }

            if (_board.PieceBgRenderer != null && _hasPieceBgOriginalPosition)
            {
                ApplyPieceBgSlidePosition(_pieceBgOriginalPosition, GetPieceBgFillTransform());
                _isPieceBgHidden = false;
            }
        }
        else
        {
            SlidePieceBgToOriginalPosition();
        }
    }

    private void SlidePieceBoardOutOfScreen()
    {
        if (_board.PieceBoardRect == null || _isPieceBoardHidden)
        {
            return;
        }

        if (!_hasPieceBoardOriginalAnchoredPosition)
        {
            CachePieceTrayOriginalPosition();
        }

        var slideDistance = _board.PieceBoardRect.rect.height + 80f;
        var from = _board.PieceBoardRect.anchoredPosition;
        var target = _pieceBoardOriginalAnchoredPosition - new Vector2(0f, slideDistance);
        StartPieceTraySlide(from, target, true, usePieceBoard: true);
    }

    private void SlidePieceBoardToOriginalPosition()
    {
        if (_board.PieceBoardRect == null || !_hasPieceBoardOriginalAnchoredPosition || !_isPieceBoardHidden)
        {
            return;
        }

        StartPieceTraySlide(
            _board.PieceBoardRect.anchoredPosition,
            _pieceBoardOriginalAnchoredPosition,
            false,
            usePieceBoard: true);
    }

    private void SlidePieceBgOutOfScreen()
    {
        if (_board.PieceBgRenderer == null || _isPieceBgHidden)
        {
            return;
        }

        if (!_hasPieceBgOriginalPosition)
        {
            CachePieceBgOriginalPosition();
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var halfHeight = _board.PieceBgRenderer.bounds.extents.y;
        var bottomEdge = camera.transform.position.y - camera.orthographicSize;
        var targetY = bottomEdge - halfHeight - PieceBgSlideOutPadding;
        var from = _board.PieceBgRenderer.transform.position;
        var target = new Vector3(from.x, targetY, from.z);
        StartPieceTraySlide(from, target, true, usePieceBoard: false);
    }

    private void SlidePieceBgToOriginalPosition()
    {
        if (_board.PieceBgRenderer == null || !_hasPieceBgOriginalPosition || !_isPieceBgHidden)
        {
            return;
        }

        StartPieceTraySlide(_board.PieceBgRenderer.transform.position, _pieceBgOriginalPosition, false, usePieceBoard: false);
    }

    private void StartPieceTraySlide(Vector2 fromAnchored, Vector2 toAnchored, bool willHidden, bool usePieceBoard)
    {
        if (_pieceTraySlideCoroutine != null)
        {
            StopCoroutine(_pieceTraySlideCoroutine);
            _pieceTraySlideCoroutine = null;
        }

        var fillTransform = GetPieceBgFillTransform();
        _pieceTraySlideCoroutine = StartCoroutine(
            AnimatePieceTraySlideAnchored(fromAnchored, toAnchored, fillTransform, willHidden, usePieceBoard));
    }

    private void StartPieceTraySlide(Vector3 from, Vector3 to, bool willHidden, bool usePieceBoard)
    {
        if (_pieceTraySlideCoroutine != null)
        {
            StopCoroutine(_pieceTraySlideCoroutine);
            _pieceTraySlideCoroutine = null;
        }

        var fillTransform = GetPieceBgFillTransform();
        _pieceTraySlideCoroutine = StartCoroutine(
            AnimatePieceTraySlideWorld(from, to, fillTransform, willHidden, usePieceBoard));
    }

    private IEnumerator AnimatePieceTraySlideAnchored(
        Vector2 from,
        Vector2 to,
        Transform fillTransform,
        bool willHidden,
        bool usePieceBoard)
    {
        var duration = Mathf.Max(0f, PieceBgSlideDuration);
        if (duration <= 0f)
        {
            ApplyPieceBoardSlidePosition(to, fillTransform);
            _isPieceBoardHidden = willHidden;
            _pieceTraySlideCoroutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var position = Vector2.LerpUnclamped(from, to, eased);
            ApplyPieceBoardSlidePosition(position, fillTransform);
            yield return null;
        }

        ApplyPieceBoardSlidePosition(to, fillTransform);
        _isPieceBoardHidden = willHidden;
        _pieceTraySlideCoroutine = null;
    }

    private IEnumerator AnimatePieceTraySlideWorld(
        Vector3 from,
        Vector3 to,
        Transform fillTransform,
        bool willHidden,
        bool usePieceBoard)
    {
        var duration = Mathf.Max(0f, PieceBgSlideDuration);
        if (duration <= 0f)
        {
            ApplyPieceBgSlidePosition(to, fillTransform);
            _isPieceBgHidden = willHidden;
            _pieceTraySlideCoroutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var position = Vector3.LerpUnclamped(from, to, eased);
            ApplyPieceBgSlidePosition(position, fillTransform);
            yield return null;
        }

        ApplyPieceBgSlidePosition(to, fillTransform);
        _isPieceBgHidden = willHidden;
        _pieceTraySlideCoroutine = null;
    }

    private void ApplyPieceBoardSlidePosition(Vector2 anchoredPosition, Transform fillTransform)
    {
        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.anchoredPosition = anchoredPosition;
        }
    }

    private void ApplyPieceBgSlidePosition(Vector3 pieceBgPosition, Transform fillTransform)
    {
        if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.transform.position = pieceBgPosition;
        }

        if (fillTransform != null)
        {
            fillTransform.position = new Vector3(
                pieceBgPosition.x,
                pieceBgPosition.y,
                pieceBgPosition.z + 0.01f);
        }
    }

    private static Transform GetPieceBgFillTransform()
    {
        var fillObject = GameObject.Find(PieceBgFillObjectName);
        return fillObject != null ? fillObject.transform : null;
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        var color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void ConfigureReturnButton()
    {
        var returnButtonObject = GameObject.Find(GameDefine.ReturnButtonObjectName);
        if (returnButtonObject == null)
        {
            Debug.LogWarning($"GameScene: return button not found. Expected object named {GameDefine.ReturnButtonObjectName}.");
            return;
        }

        var button = returnButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"GameScene: {GameDefine.ReturnButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnReturnButtonClicked);
        button.onClick.AddListener(OnReturnButtonClicked);
    }

    private void OnReturnButtonClicked()
    {
        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        var packId = GameManager.GetBagId();
        if (!_isGameFinished
            && packId > 0
            && !CardPackDataUtility.TryEnsurePuzzleSession(packId))
        {
            Debug.LogWarning(
                $"GameScene: failed to preserve puzzle session before returning. packId={packId}");
        }

        if (!_isGameFinished)
        {
            AnalyticsManager.Instance.ExitCardBag(
                packId,
                CardBagExitReason.ReturnButton);
        }

        GameManager.EnterMainScene();
    }
}

internal sealed class PieceLightDeformEffect : BaseMeshEffect
{
    private const int HorizontalSegmentCount = 8;

    public Vector2 BendOffset { get; private set; }
    public float MiddleStretch { get; private set; }

    public void SetDeformation(Vector2 bendOffset, float middleStretch)
    {
        middleStretch = Mathf.Clamp(middleStretch, 0f, 0.8f);
        if ((BendOffset - bendOffset).sqrMagnitude <= 0.000001f
            && Mathf.Abs(MiddleStretch - middleStretch) <= 0.0001f)
        {
            return;
        }

        BendOffset = bendOffset;
        MiddleStretch = middleStretch;
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper == null || graphic == null)
        {
            return;
        }

        var image = graphic as Image;
        var sprite = image != null ? image.overrideSprite : null;
        if (sprite == null)
        {
            return;
        }

        var rect = graphic.rectTransform.rect;
        var outerUv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
        var color = graphic.color;
        vertexHelper.Clear();

        for (var segment = 0; segment <= HorizontalSegmentCount; segment++)
        {
            var normalizedX = segment / (float)HorizontalSegmentCount;
            var middleWeight = Mathf.Sin(normalizedX * Mathf.PI);
            var x = Mathf.LerpUnclamped(rect.xMin, rect.xMax, normalizedX)
                    + BendOffset.x * middleWeight;
            var centerY = rect.center.y + BendOffset.y * middleWeight;
            var halfHeight = rect.height * 0.5f
                             * (1f + MiddleStretch * middleWeight);
            var uvX = Mathf.LerpUnclamped(outerUv.x, outerUv.z, normalizedX);

            vertexHelper.AddVert(CreateVertex(
                new Vector2(x, centerY - halfHeight),
                new Vector2(uvX, outerUv.y),
                color));
            vertexHelper.AddVert(CreateVertex(
                new Vector2(x, centerY + halfHeight),
                new Vector2(uvX, outerUv.w),
                color));
        }

        for (var segment = 0; segment < HorizontalSegmentCount; segment++)
        {
            var leftBottom = segment * 2;
            var leftTop = leftBottom + 1;
            var rightBottom = leftBottom + 2;
            var rightTop = leftBottom + 3;
            vertexHelper.AddTriangle(leftBottom, leftTop, rightTop);
            vertexHelper.AddTriangle(leftBottom, rightTop, rightBottom);
        }
    }

    private static UIVertex CreateVertex(Vector2 position, Vector2 uv, Color color)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.uv0 = uv;
        vertex.color = color;
        return vertex;
    }
}

internal sealed class TutorialHintArrowMotion : MonoBehaviour
{
    private const float RevealDelay = 0.34f;
    private const float RevealDuration = 0.22f;
    private const float PulseDuration = 0.72f;
    internal static readonly Vector2 PulseOffset = new Vector2(14f, 8f);
    private RectTransform _rectTransform;
    private Image _image;
    private Vector2 _basePosition;
    private Vector3 _baseScale;
    private Color _baseColor;
    private float _startTime;

    public void Configure(RectTransform rectTransform, Image image)
    {
        _rectTransform = rectTransform;
        _image = image;
        _basePosition = rectTransform.anchoredPosition;
        _baseScale = rectTransform.localScale;
        _baseColor = image.color;
        _startTime = Time.unscaledTime;
        Apply(0f, 0f);
    }

    private void Update()
    {
        if (_rectTransform == null || _image == null)
        {
            return;
        }

        var elapsed = Time.unscaledTime - _startTime;
        if (elapsed < RevealDelay)
        {
            Apply(0f, 0f);
            return;
        }

        var revealProgress = Mathf.Clamp01((elapsed - RevealDelay) / RevealDuration);
        var revealEased = SmootherStep01(revealProgress);
        if (revealProgress < 1f)
        {
            Apply(revealEased, 0f);
            return;
        }

        var phase = Mathf.Repeat(elapsed - RevealDelay - RevealDuration, PulseDuration)
            / PulseDuration;
        var pulse = Mathf.Sin(phase * Mathf.PI);
        Apply(1f, pulse * pulse);
    }

    private void Apply(float visibility, float pulse)
    {
        _rectTransform.anchoredPosition = _basePosition + PulseOffset * pulse;
        _rectTransform.localScale = _baseScale
            * Mathf.Lerp(0.82f, 1f + pulse * 0.06f, visibility);
        var color = _baseColor;
        color.a *= visibility;
        _image.color = color;
    }

    private static float SmootherStep01(float value)
    {
        var t = Mathf.Clamp01(value);
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }
}

internal sealed class TutorialPromptMotion : MonoBehaviour
{
    private const float Duration = 0.32f;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;
    private float _startTime;

    public void Configure(
        RectTransform rectTransform,
        CanvasGroup canvasGroup,
        Vector2 targetPosition,
        Vector2 entranceOffset)
    {
        _rectTransform = rectTransform;
        _canvasGroup = canvasGroup;
        _targetPosition = targetPosition;
        _startPosition = targetPosition + entranceOffset;
        _startTime = Time.unscaledTime;
        _rectTransform.anchoredPosition = _startPosition;
        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (_rectTransform == null || _canvasGroup == null)
        {
            return;
        }

        var progress = Mathf.Clamp01((Time.unscaledTime - _startTime) / Duration);
        var eased = progress * progress * (3f - 2f * progress);
        _rectTransform.anchoredPosition = Vector2.LerpUnclamped(
            _startPosition,
            _targetPosition,
            eased);
        _canvasGroup.alpha = eased;
        if (progress >= 1f)
        {
            enabled = false;
        }
    }
}

internal sealed class TutorialArrowMotion : MonoBehaviour
{
    private const float MoveDuration = 0.9f;
    private const float HoldDuration = 0.12f;
    private const float GapDuration = 0.2f;
    private RectTransform _rectTransform;
    private Image _image;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private float _animationStartTime;

    public void Configure(
        RectTransform rectTransform,
        Image image,
        Vector2 startPosition,
        Vector2 endPosition)
    {
        _rectTransform = rectTransform;
        _image = image;
        _startPosition = startPosition;
        _endPosition = endPosition;
        _animationStartTime = Time.unscaledTime;
        _rectTransform.localScale = Vector3.one;
        ApplyPosition(0f);
    }

    private void Update()
    {
        if (_rectTransform == null || _image == null)
        {
            return;
        }

        var cycleDuration = MoveDuration + HoldDuration + GapDuration;
        var phase = Mathf.Repeat(Time.unscaledTime - _animationStartTime, cycleDuration);
        if (phase >= MoveDuration + HoldDuration)
        {
            _image.enabled = false;
            return;
        }

        _image.enabled = true;
        var progress = phase < MoveDuration
            ? SmootherStep(phase / MoveDuration)
            : 1f;
        ApplyPosition(progress);
    }

    private static float SmootherStep(float value)
    {
        var t = Mathf.Clamp01(value);
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    private void ApplyPosition(float progress)
    {
        var normalized = Mathf.Clamp01(progress);
        _rectTransform.anchoredPosition = Vector2.LerpUnclamped(
            _startPosition,
            _endPosition,
            normalized);
    }
}

internal readonly struct HintOutlineSpriteRegion
{
    public HintOutlineSpriteRegion(Sprite sprite, Rect rect, bool preserveAspect)
    {
        Sprite = sprite;
        Rect = rect;
        PreserveAspect = preserveAspect;
    }

    public Sprite Sprite { get; }
    public Rect Rect { get; }
    public bool PreserveAspect { get; }

    public HintOutlineSpriteRegion OffsetBy(Vector2 offset)
    {
        var rect = Rect;
        rect.position += offset;
        return new HintOutlineSpriteRegion(Sprite, rect, PreserveAspect);
    }
}

internal sealed class HintDashedOutlineGraphic : MaskableGraphic
{
    private const byte AlphaThreshold = 26;
    private const float OutlineSimplifyTolerancePixels = 0.75f;
    private const int CombinedMaskMaxDimension = 2048;
    private static readonly Dictionary<Sprite, List<List<Vector2>>> sNormalizedPathCache =
        new Dictionary<Sprite, List<List<Vector2>>>();
    private readonly List<List<Vector2>> _spritePaths = new List<List<Vector2>>();
    private readonly List<Vector2> _mappedPath = new List<Vector2>();
    private Sprite _sourceSprite;
    private float _lineWidth = 3f;
    private float _dashLength = 5.6f;
    private float _dashGap = 4f;
    private float _scrollSpeed = 8f;
    private bool _preserveAspect;
    private bool _usesCombinedMask;

    public static void ClearPathCache()
    {
        sNormalizedPathCache.Clear();
    }

    public void Configure(
        Sprite sourceSprite,
        Color lineColor,
        float lineWidth,
        float dashLength,
        float dashGap,
        float scrollSpeed,
        bool preserveAspect)
    {
        _sourceSprite = sourceSprite;
        _usesCombinedMask = false;
        color = lineColor;
        _lineWidth = Mathf.Max(0.5f, lineWidth);
        _dashLength = Mathf.Max(1f, dashLength);
        _dashGap = Mathf.Max(1f, dashGap);
        _scrollSpeed = scrollSpeed;
        _preserveAspect = preserveAspect;
        raycastTarget = false;
        RebuildSpritePaths();
        SetAllDirty();
    }

    public void ConfigureCombined(
        IReadOnlyList<HintOutlineSpriteRegion> sourceRegions,
        Color lineColor,
        float lineWidth,
        float dashLength,
        float dashGap,
        float scrollSpeed)
    {
        _sourceSprite = null;
        _usesCombinedMask = true;
        color = lineColor;
        _lineWidth = Mathf.Max(0.5f, lineWidth);
        _dashLength = Mathf.Max(1f, dashLength);
        _dashGap = Mathf.Max(1f, dashGap);
        _scrollSpeed = scrollSpeed;
        _preserveAspect = false;
        raycastTarget = false;
        _spritePaths.Clear();
        if (!TryBuildCombinedAlphaPaths(sourceRegions, rectTransform.rect, _spritePaths))
        {
            Debug.LogWarning("GameScene: combined hint outline could not build a shared alpha mask.");
        }

        SetAllDirty();
    }

    private void Update()
    {
        if (_spritePaths.Count > 0 && Mathf.Abs(_scrollSpeed) > 0.001f)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        if (_spritePaths.Count == 0)
        {
            return;
        }

        var targetRect = rectTransform.rect;
        if (!_usesCombinedMask)
        {
            if (_sourceSprite == null)
            {
                return;
            }

            var spriteBounds = _sourceSprite.bounds;
            if (spriteBounds.size.x <= 0.0001f || spriteBounds.size.y <= 0.0001f)
            {
                return;
            }

            targetRect = GetDrawingRect(
                spriteBounds,
                targetRect,
                _preserveAspect,
                rectTransform.pivot);
        }

        for (var pathIndex = 0; pathIndex < _spritePaths.Count; pathIndex++)
        {
            AddDashedPath(vertexHelper, _spritePaths[pathIndex], targetRect);
        }
    }

    private void AddDashedPath(
        VertexHelper vertexHelper,
        List<Vector2> spritePath,
        Rect targetRect)
    {
        if (spritePath == null || spritePath.Count < 2)
        {
            return;
        }

        _mappedPath.Clear();
        var pathLength = 0f;
        for (var i = 0; i < spritePath.Count; i++)
        {
            _mappedPath.Add(MapNormalizedPoint(spritePath[i], targetRect));
        }

        for (var i = 0; i < _mappedPath.Count; i++)
        {
            pathLength += Vector2.Distance(_mappedPath[i], _mappedPath[(i + 1) % _mappedPath.Count]);
        }

        if (pathLength <= 0.001f)
        {
            return;
        }

        var dashLength = Mathf.Min(_dashLength, pathLength * 0.6f);
        var dashCount = Mathf.Max(1, Mathf.FloorToInt(pathLength / (_dashLength + _dashGap)));
        var period = pathLength / dashCount;
        var offset = Mathf.Repeat(Time.unscaledTime * _scrollSpeed, period);
        var pathDistance = 0f;
        for (var i = 0; i < _mappedPath.Count; i++)
        {
            var from = _mappedPath[i];
            var to = _mappedPath[(i + 1) % _mappedPath.Count];
            var edge = to - from;
            var edgeLength = edge.magnitude;
            if (edgeLength <= 0.001f)
            {
                continue;
            }

            var vertexPhase = Mathf.Repeat(pathDistance + offset, period);
            if (vertexPhase > 0.001f && vertexPhase < dashLength - 0.001f)
            {
                AddRoundJoin(vertexHelper, from);
            }

            var edgeDirection = edge / edgeLength;
            var edgeOffset = 0f;
            while (edgeOffset < edgeLength - 0.001f)
            {
                var phase = Mathf.Repeat(pathDistance + edgeOffset + offset, period);
                var inDash = phase < dashLength;
                var remainingPatternLength = inDash
                    ? dashLength - phase
                    : period - phase;
                var step = Mathf.Min(
                    Mathf.Max(remainingPatternLength, 0.001f),
                    edgeLength - edgeOffset);
                if (inDash)
                {
                    AddLineSegment(
                        vertexHelper,
                        from + edgeDirection * edgeOffset,
                        from + edgeDirection * (edgeOffset + step));
                }

                edgeOffset += step;
            }

            pathDistance += edgeLength;
        }
    }

    private void AddRoundJoin(VertexHelper vertexHelper, Vector2 center)
    {
        const int segmentCount = 8;
        var centerIndex = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, center);
        for (var i = 0; i <= segmentCount; i++)
        {
            var angle = i * Mathf.PI * 2f / segmentCount;
            var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (_lineWidth * 0.5f);
            AddVertex(vertexHelper, center + offset);
            if (i > 0)
            {
                vertexHelper.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }
    }

    private void AddLineSegment(VertexHelper vertexHelper, Vector2 from, Vector2 to)
    {
        var direction = to - from;
        if (direction.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        direction.Normalize();
        var normal = new Vector2(-direction.y, direction.x) * (_lineWidth * 0.5f);
        var startIndex = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, from - normal);
        AddVertex(vertexHelper, from + normal);
        AddVertex(vertexHelper, to + normal);
        AddVertex(vertexHelper, to - normal);
        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void AddVertex(VertexHelper vertexHelper, Vector2 position)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vertexHelper.AddVert(vertex);
    }

    private static Vector2 MapNormalizedPoint(Vector2 point, Rect targetRect)
    {
        return new Vector2(
            Mathf.Lerp(targetRect.xMin, targetRect.xMax, point.x),
            Mathf.Lerp(targetRect.yMin, targetRect.yMax, point.y));
    }

    private static Rect GetDrawingRect(
        Bounds spriteBounds,
        Rect targetRect,
        bool preserveAspect,
        Vector2 pivot)
    {
        if (!preserveAspect)
        {
            return targetRect;
        }

        var spriteAspect = spriteBounds.size.x / spriteBounds.size.y;
        var rectAspect = targetRect.width / targetRect.height;
        if (spriteAspect > rectAspect)
        {
            var originalHeight = targetRect.height;
            targetRect.height = targetRect.width / spriteAspect;
            targetRect.y += (originalHeight - targetRect.height) * pivot.y;
        }
        else
        {
            var originalWidth = targetRect.width;
            targetRect.width = targetRect.height * spriteAspect;
            targetRect.x += (originalWidth - targetRect.width) * pivot.x;
        }

        return targetRect;
    }

    private static bool TryBuildAlphaPaths(Sprite sprite, List<List<Vector2>> outputPaths)
    {
        if (!TryReadSpritePixels(sprite, out var pixels, out var width, out var height))
        {
            return false;
        }

        var mask = new bool[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            mask[i] = pixels[i].a >= AlphaThreshold;
        }

        return TryBuildMaskPaths(mask, width, height, outputPaths);
    }

    private static bool TryBuildCombinedAlphaPaths(
        IReadOnlyList<HintOutlineSpriteRegion> sourceRegions,
        Rect targetRect,
        List<List<Vector2>> outputPaths)
    {
        if (sourceRegions == null
            || sourceRegions.Count == 0
            || targetRect.width <= 0.001f
            || targetRect.height <= 0.001f)
        {
            return false;
        }

        var rasterScale = Mathf.Min(
            1f,
            CombinedMaskMaxDimension / Mathf.Max(targetRect.width, targetRect.height));
        var width = Mathf.Max(1, Mathf.CeilToInt(targetRect.width * rasterScale));
        var height = Mathf.Max(1, Mathf.CeilToInt(targetRect.height * rasterScale));
        var mask = new bool[width * height];
        var didRasterize = false;
        for (var regionIndex = 0; regionIndex < sourceRegions.Count; regionIndex++)
        {
            var region = sourceRegions[regionIndex];
            if (!TryReadSpritePixels(
                    region.Sprite,
                    out var pixels,
                    out var spriteWidth,
                    out var spriteHeight))
            {
                continue;
            }

            var drawingRect = GetDrawingRect(
                region.Sprite.bounds,
                region.Rect,
                region.PreserveAspect,
                new Vector2(0.5f, 0.5f));
            var minX = Mathf.Clamp(
                Mathf.FloorToInt((drawingRect.xMin - targetRect.xMin) * rasterScale),
                0,
                width - 1);
            var maxX = Mathf.Clamp(
                Mathf.CeilToInt((drawingRect.xMax - targetRect.xMin) * rasterScale),
                1,
                width);
            var minY = Mathf.Clamp(
                Mathf.FloorToInt((drawingRect.yMin - targetRect.yMin) * rasterScale),
                0,
                height - 1);
            var maxY = Mathf.Clamp(
                Mathf.CeilToInt((drawingRect.yMax - targetRect.yMin) * rasterScale),
                1,
                height);
            for (var y = minY; y < maxY; y++)
            {
                var localY = targetRect.yMin + (y + 0.5f) / rasterScale;
                var v = Mathf.InverseLerp(drawingRect.yMin, drawingRect.yMax, localY);
                var sourceY = Mathf.Clamp(Mathf.FloorToInt(v * spriteHeight), 0, spriteHeight - 1);
                for (var x = minX; x < maxX; x++)
                {
                    var localX = targetRect.xMin + (x + 0.5f) / rasterScale;
                    var u = Mathf.InverseLerp(drawingRect.xMin, drawingRect.xMax, localX);
                    var sourceX = Mathf.Clamp(Mathf.FloorToInt(u * spriteWidth), 0, spriteWidth - 1);
                    if (pixels[sourceY * spriteWidth + sourceX].a >= AlphaThreshold)
                    {
                        mask[y * width + x] = true;
                        didRasterize = true;
                    }
                }
            }
        }

        return didRasterize && TryBuildMaskPaths(mask, width, height, outputPaths);
    }

    private static bool TryBuildMaskPaths(
        bool[] mask,
        int width,
        int height,
        List<List<Vector2>> outputPaths)
    {
        if (mask == null || mask.Length != width * height)
        {
            return false;
        }

        var outgoingEdges = new Dictionary<Vector2Int, List<Vector2Int>>();
        var unusedEdges = new HashSet<PixelEdge>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!IsOpaque(mask, width, height, x, y))
                {
                    continue;
                }

                if (!IsOpaque(mask, width, height, x - 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }

                if (!IsOpaque(mask, width, height, x, y + 1))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
                }

                if (!IsOpaque(mask, width, height, x + 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));
                }

                if (!IsOpaque(mask, width, height, x, y - 1))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x + 1, y), new Vector2Int(x, y));
                }
            }
        }

        while (unusedEdges.Count > 0)
        {
            var startEdge = default(PixelEdge);
            foreach (var edge in unusedEdges)
            {
                startEdge = edge;
                break;
            }

            var pixelPath = TraceBoundary(startEdge, outgoingEdges, unusedEdges);
            if (pixelPath == null || pixelPath.Count < 3 || CalculateSignedArea(pixelPath) < 9f)
            {
                continue;
            }

            var simplifiedPath = SimplifyClosedPath(pixelPath, OutlineSimplifyTolerancePixels);
            if (simplifiedPath.Count < 3)
            {
                continue;
            }

            var normalizedPath = new List<Vector2>(simplifiedPath.Count);
            for (var pointIndex = 0; pointIndex < simplifiedPath.Count; pointIndex++)
            {
                var point = simplifiedPath[pointIndex];
                normalizedPath.Add(new Vector2(point.x / width, point.y / height));
            }

            outputPaths.Add(normalizedPath);
        }

        return outputPaths.Count > 0;
    }

    internal static bool TryReadSpritePixels(
        Sprite sprite,
        out Color32[] pixels,
        out int width,
        out int height)
    {
        pixels = null;
        width = 0;
        height = 0;
        if (sprite == null || sprite.texture == null || sprite.packed)
        {
            return false;
        }

        var texture = sprite.texture;
        var spriteRect = sprite.textureRect;
        var sourceX = Mathf.RoundToInt(spriteRect.x);
        var sourceY = Mathf.RoundToInt(spriteRect.y);
        width = Mathf.RoundToInt(spriteRect.width);
        height = Mathf.RoundToInt(spriteRect.height);
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        var previousRenderTexture = RenderTexture.active;
        var renderTexture = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        Texture2D readableTexture = null;
        try
        {
            renderTexture.filterMode = FilterMode.Point;
            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = renderTexture;
            readableTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            readableTexture.ReadPixels(new Rect(sourceX, sourceY, width, height), 0, 0, false);
            pixels = readableTexture.GetPixels32();
            return pixels.Length == width * height;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"GameScene: failed to read hint Sprite alpha. {exception.Message}");
            pixels = null;
            return false;
        }
        finally
        {
            RenderTexture.active = previousRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            if (readableTexture != null)
            {
                UnityEngine.Object.Destroy(readableTexture);
            }
        }
    }

    private static bool IsOpaque(bool[] mask, int width, int height, int x, int y)
    {
        return x >= 0
            && x < width
            && y >= 0
            && y < height
            && mask[y * width + x];
    }

    private static void AddBoundaryEdge(
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges,
        Vector2Int from,
        Vector2Int to)
    {
        var edge = new PixelEdge(from, to);
        if (!unusedEdges.Add(edge))
        {
            return;
        }

        if (!outgoingEdges.TryGetValue(from, out var destinations))
        {
            destinations = new List<Vector2Int>(2);
            outgoingEdges[from] = destinations;
        }

        destinations.Add(to);
    }

    private static List<Vector2> TraceBoundary(
        PixelEdge startEdge,
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges)
    {
        var path = new List<Vector2>();
        var currentEdge = startEdge;
        var maxSteps = unusedEdges.Count + 1;
        path.Add(currentEdge.From);
        for (var step = 0; step < maxSteps; step++)
        {
            if (!unusedEdges.Remove(currentEdge))
            {
                return null;
            }

            path.Add(currentEdge.To);
            if (currentEdge.To == startEdge.From)
            {
                path.RemoveAt(path.Count - 1);
                return path;
            }

            if (!TryFindNextEdge(currentEdge, outgoingEdges, unusedEdges, out currentEdge))
            {
                return null;
            }
        }

        return null;
    }

    private static bool TryFindNextEdge(
        PixelEdge incomingEdge,
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges,
        out PixelEdge nextEdge)
    {
        nextEdge = default;
        if (!outgoingEdges.TryGetValue(incomingEdge.To, out var destinations))
        {
            return false;
        }

        var incomingDirection = incomingEdge.To - incomingEdge.From;
        var found = false;
        var bestTurnAngle = float.PositiveInfinity;
        for (var i = 0; i < destinations.Count; i++)
        {
            var candidate = new PixelEdge(incomingEdge.To, destinations[i]);
            if (!unusedEdges.Contains(candidate))
            {
                continue;
            }

            var outgoingDirection = candidate.To - candidate.From;
            var cross = incomingDirection.x * outgoingDirection.y - incomingDirection.y * outgoingDirection.x;
            var dot = incomingDirection.x * outgoingDirection.x
                + incomingDirection.y * outgoingDirection.y;
            var turnAngle = Mathf.Atan2(cross, dot);
            if (!found || turnAngle < bestTurnAngle)
            {
                found = true;
                bestTurnAngle = turnAngle;
                nextEdge = candidate;
            }
        }

        return found;
    }

    private static float CalculateSignedArea(List<Vector2> path)
    {
        var twiceArea = 0f;
        for (var i = 0; i < path.Count; i++)
        {
            var current = path[i];
            var next = path[(i + 1) % path.Count];
            twiceArea += current.x * next.y - next.x * current.y;
        }

        return Mathf.Abs(twiceArea) * 0.5f;
    }

    private static List<Vector2> SimplifyClosedPath(List<Vector2> path, float tolerance)
    {
        if (path.Count < 5)
        {
            return path;
        }

        var firstIndex = 0;
        var secondIndex = FindFarthestPointIndex(path, firstIndex);
        firstIndex = FindFarthestPointIndex(path, secondIndex);
        secondIndex = FindFarthestPointIndex(path, firstIndex);
        var firstArc = BuildPathArc(path, firstIndex, secondIndex);
        var secondArc = BuildPathArc(path, secondIndex, firstIndex);
        var simplifiedFirstArc = SimplifyOpenPath(firstArc, tolerance);
        var simplifiedSecondArc = SimplifyOpenPath(secondArc, tolerance);
        var result = new List<Vector2>(simplifiedFirstArc.Count + simplifiedSecondArc.Count - 2);
        result.AddRange(simplifiedFirstArc);
        for (var i = 1; i < simplifiedSecondArc.Count - 1; i++)
        {
            result.Add(simplifiedSecondArc[i]);
        }

        return result.Count >= 3 ? result : path;
    }

    private static int FindFarthestPointIndex(List<Vector2> path, int sourceIndex)
    {
        var farthestIndex = sourceIndex;
        var farthestDistance = -1f;
        for (var i = 0; i < path.Count; i++)
        {
            var distance = (path[i] - path[sourceIndex]).sqrMagnitude;
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestIndex = i;
            }
        }

        return farthestIndex;
    }

    private static List<Vector2> BuildPathArc(List<Vector2> path, int startIndex, int endIndex)
    {
        var arc = new List<Vector2>();
        var index = startIndex;
        arc.Add(path[index]);
        while (index != endIndex)
        {
            index = (index + 1) % path.Count;
            arc.Add(path[index]);
        }

        return arc;
    }

    private static List<Vector2> SimplifyOpenPath(List<Vector2> path, float tolerance)
    {
        if (path.Count <= 2)
        {
            return path;
        }

        var keep = new bool[path.Count];
        keep[0] = true;
        keep[path.Count - 1] = true;
        var ranges = new Stack<Vector2Int>();
        ranges.Push(new Vector2Int(0, path.Count - 1));
        var toleranceSquared = tolerance * tolerance;
        while (ranges.Count > 0)
        {
            var range = ranges.Pop();
            var farthestIndex = -1;
            var farthestDistance = toleranceSquared;
            for (var i = range.x + 1; i < range.y; i++)
            {
                var distance = DistanceToSegmentSquared(path[i], path[range.x], path[range.y]);
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestIndex = i;
                }
            }

            if (farthestIndex < 0)
            {
                continue;
            }

            keep[farthestIndex] = true;
            ranges.Push(new Vector2Int(range.x, farthestIndex));
            ranges.Push(new Vector2Int(farthestIndex, range.y));
        }

        var result = new List<Vector2>();
        for (var i = 0; i < path.Count; i++)
        {
            if (keep[i])
            {
                result.Add(path[i]);
            }
        }

        return result;
    }

    private static float DistanceToSegmentSquared(Vector2 point, Vector2 from, Vector2 to)
    {
        var segment = to - from;
        if (segment.sqrMagnitude <= 0.000001f)
        {
            return (point - from).sqrMagnitude;
        }

        var t = Mathf.Clamp01(Vector2.Dot(point - from, segment) / segment.sqrMagnitude);
        return (point - (from + segment * t)).sqrMagnitude;
    }

    private void RebuildSpritePaths()
    {
        _spritePaths.Clear();
        if (_sourceSprite == null)
        {
            return;
        }

        if (sNormalizedPathCache.TryGetValue(_sourceSprite, out var cachedPaths))
        {
            _spritePaths.AddRange(cachedPaths);
            return;
        }

        if (TryBuildAlphaPaths(_sourceSprite, _spritePaths))
        {
            sNormalizedPathCache[_sourceSprite] = new List<List<Vector2>>(_spritePaths);
            return;
        }

        Debug.LogWarning(
            $"GameScene: hint outline could not read Sprite alpha; using simplified Physics Shape. "
            + $"sprite={_sourceSprite.name}");
        var bounds = _sourceSprite.bounds;
        var shapeCount = _sourceSprite.GetPhysicsShapeCount();
        for (var shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
        {
            var path = new List<Vector2>();
            _sourceSprite.GetPhysicsShape(shapeIndex, path);
            if (path.Count >= 2)
            {
                for (var pointIndex = 0; pointIndex < path.Count; pointIndex++)
                {
                    var point = path[pointIndex];
                    path[pointIndex] = new Vector2(
                        (point.x - bounds.min.x) / bounds.size.x,
                        (point.y - bounds.min.y) / bounds.size.y);
                }

                _spritePaths.Add(path);
            }
        }

        if (_spritePaths.Count == 0)
        {
            _spritePaths.Add(new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            });
        }

        sNormalizedPathCache[_sourceSprite] = new List<List<Vector2>>(_spritePaths);
    }

    private readonly struct PixelEdge : IEquatable<PixelEdge>
    {
        public PixelEdge(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }

        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public bool Equals(PixelEdge other)
        {
            return From == other.From && To == other.To;
        }

        public override bool Equals(object obj)
        {
            return obj is PixelEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode() * 397) ^ To.GetHashCode();
            }
        }
    }
}
