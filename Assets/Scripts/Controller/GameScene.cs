using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float DefaultBoardScale = 1f;
    private const float WorldGameplayDepth = -0.5f;
    private const float GamePageCameraPadding = 0.3f;
    private const float DraggableLeftPadding = 0.2f;
    private const float DraggableHorizontalSpacingPixels = 20f;
    private const float TrayPieceReflowDuration = 0.5f;
    private const float PieceTrayMaxHeightRatio = 0.9f;
    private const float SnapDistanceMin = 0.2f;
    private const float SnapDistanceMax = 0.8f;
    private const float SnapDistanceSizeRatio = 0.22f;
    private const float PieceBgSlideDuration = 0.25f;
    private const float TaskProgressRollDuration = 0.8f;
    private const float SettlementBaseRollDuration = 1f;
    private const float SettlementStagePauseDuration = 0.28f;
    private const float SettlementFinalPauseDuration = 0.24f;
    private const float PieceBgSlideOutPadding = 0.15f;
    private const int PieceBgFillSortingOrder = 499;
    private const int PieceBgSortingOrder = 500;
    private const float PieceBgAlpha = 1f;
    private const float PieceBgFillAlpha = 0.3f;
    private const float GameEntranceBoardDuration = 0.46f;
    private const float GameEntranceTrayDelay = 0.12f;
    private const float GameEntranceTrayDuration = 0.38f;
    private const float GameEntrancePieceDelay = 0.28f;
    private const float GameEntrancePieceDuration = 0.34f;
    private const float GameEntrancePieceStagger = 0.035f;
    private const float GameEntranceControlDelay = 0.18f;
    private const float GameEntranceControlDuration = 0.24f;
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
    private const float TutorialPracticePromptGap = 24f;
    private const float TutorialPromptScreenMargin = 24f;
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
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string ActiveGroupOutlineRootObjectName = "ActiveGroupOutline";
    private const string LevelOutlineLayerObjectName = "LevelOutline";
    private const string StickerOutlineLayerObjectName = "StickerOutlines";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private const string TaskItemObjectName = "TaskItem";
    private const string TaskScoreTitlePath = "TaskBg2/TaskTitle2";
    private const string TaskScorePath = "TaskBg2/TaskScore";
    private const string TaskBagCountPath = "TaskBg2/TaskBagNum";
    private const string TaskRewardImgBagPath = "ImgBagBg/ImgBag";
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
    private static readonly Vector2 TutorialStrongPromptAnchor = new Vector2(0.5f, 0.7f);
    private static readonly Vector2 TutorialStrongPromptOffset = new Vector2(-30f, -50f);
    private static readonly Vector2 TutorialHintPromptAnchor = new Vector2(0.73f, 0.76f);

    private enum TutorialStage
    {
        None,
        StrongPlacement,
        TwoPiecePractice,
        HintIntroduction
    }

    private static bool sHookedSceneLoaded;
    private readonly BoardState _board = new BoardState();
    private readonly DragState _drag = new DragState();
    private readonly List<int> _settlementPackRewardIds = new List<int>();
    private readonly HashSet<int> _placedPieceNumbers = new HashSet<int>();
    private Vector3 _pieceBgOriginalPosition;
    private bool _hasPieceBgOriginalPosition;
    private bool _isPieceBgHidden;
    private Vector2 _pieceBoardOriginalAnchoredPosition;
    private bool _hasPieceBoardOriginalAnchoredPosition;
    private bool _isPieceBoardHidden;
    private Coroutine _pieceTraySlideCoroutine;
    private Coroutine _trayPieceReflowCoroutine;
    private bool _isTrayPieceReflowAnimating;
    private int _piecePlacementAnimationCount;
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
    private Transform _rewardTaskItem;
    private TMP_Text _settlementScoreTitleText;
    private TMP_Text _settlementScoreText;
    private TMP_Text _settlementBagCountText;
    private Image _taskRewardImage;
    private Button _finishButton;
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
    private Coroutine _activeGroupOutlineFadeCoroutine;
    private bool _didWarnMissingPuzzleOutlineTintShader;
    private bool _didWarnMissingPiecePlacementShineShader;
    private float _configuredBoardScale = DefaultBoardScale;
    private Vector3 _originalCardBagLocalScale = Vector3.one;
    private bool _hasOriginalCardBagLocalScale;
    private Vector2 _originalCardBagAnchoredPosition;
    private bool _hasOriginalCardBagAnchoredPosition;
    private DraggablePieceState _hintedPiece;
    private Quaternion _hintedPieceBaseRotation = Quaternion.identity;
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
    private Vector3 _dragStartPosition;

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
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneGame))
        {
            Destroy(gameObject);
            return;
        }

        var camera = Camera.main;
        if (camera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);
        }

        ConfigureGameplayCanvas(camera);
        InitializeScoringSession();
        var selectedBagId = GameManager.GetBagId();
        var playEntranceAnimation = GameManager.ConsumeGameEntranceAnimation();
        var isReplaySession = GameManager.ConsumeGameReplaySession();
        CardPackDataUtility.Initialize();
        _wasSelectedPackCompletedOnEntry = CardPackDataUtility.IsPackCompleted(selectedBagId);
        _isTutorialPending = ShouldOfferPiecePlacementTutorial(selectedBagId, isReplaySession);
        _didAdvanceTaskDuringSettlement = false;
        _didFailTaskAdvanceDuringSettlement = false;
        _didSavePackCompletion = false;
        _isSettlementReadyForFinish = false;
        _isFinishTransitionStarted = false;
        _settlementPackRewardIds.Clear();
        InitializeGameplay(selectedBagId);
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

        if (playEntranceAnimation && !_isGameFinished)
        {
            StartCoroutine(PlayGameEntranceAnimation());
        }
        else
        {
            TryStartPiecePlacementTutorial();
            FadeInActiveGroupOutline();
        }

        Debug.Log("GameScene bootstrap completed.");
    }

    private void OnDestroy()
    {
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
        HintDashedOutlineGraphic.ClearPathCache();
    }

    private void Update()
    {
        UpdatePieceHintAnimation();
        UpdateLoosePieceReminder();
        if (_isEntranceAnimating || _isGroupTransitionAnimating)
        {
            GameCursorUtility.SetDefault();
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            TryBeginDrag,
            UpdateDragging,
            OnPointerEnd);
        RefreshCursorForPointer(Input.mousePosition);
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

    private IEnumerator PlayGameEntranceAnimation()
    {
        _isEntranceAnimating = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testCompleteButton != null)
        {
            _testCompleteButton.interactable = false;
        }
#endif
        Canvas.ForceUpdateCanvases();

        var camera = Camera.main;
        var boardCenter = _board.GameBoardImage != null && camera != null
            ? GameCommonUtility.RectTransformToCameraWorld(
                _board.GameBoardImage.rectTransform,
                camera,
                WorldGameplayDepth)
            : Vector3.zero;
        var pieceEntranceOrigin = GetPreviousPackBottomWorldPosition(camera, boardCenter);

        var boardRect = _loadedCardBagRect;
        var boardTarget = boardRect != null
            ? boardRect.anchoredPosition
            : _hasOriginalCardBagAnchoredPosition
                ? _originalCardBagAnchoredPosition
                : Vector2.zero;
        var boardStart = boardTarget + Vector2.up * ReferenceHeight;
        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardStart;
        }

        var trayRect = _board.PieceBoardRect;
        var trayTarget = _hasPieceBoardOriginalAnchoredPosition
            ? _pieceBoardOriginalAnchoredPosition
            : trayRect != null ? trayRect.anchoredPosition : Vector2.zero;
        var trayOffset = trayRect != null
            ? trayRect.rect.height + 120f
            : 420f;
        var trayStart = trayTarget - Vector2.up * trayOffset;
        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayStart;
        }

        var returnCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(GameDefine.ReturnButtonObjectName));
        var hintCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(HintButtonObjectName));
        SetCanvasGroupAlpha(returnCanvasGroup, 0f);
        SetCanvasGroupAlpha(hintCanvasGroup, 0f);

        var pieceCount = _drag.CurrentGroupDraggables.Count;
        var pieceTargets = new Vector3[pieceCount];
        var pieceStarts = new Vector3[pieceCount];
        var pieceTargetScales = new Vector3[pieceCount];
        var pieceTargetRotations = new Quaternion[pieceCount];
        var pieceTargetColors = new Color[pieceCount];
        for (var i = 0; i < pieceCount; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            var renderer = state?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = renderer.transform.localScale;
            pieceTargetRotations[i] = renderer.transform.rotation;
            pieceTargetColors[i] = renderer.color;
            var angle = i * 137.5f * Mathf.Deg2Rad;
            var radius = 0.12f + (i % 4) * 0.07f;
            pieceStarts[i] = pieceEntranceOrigin + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f);
            pieceStarts[i].z = pieceTargets[i].z;
            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i] * 1.12f;
            renderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(angle) * 18f);
            var color = pieceTargetColors[i];
            color.a = 0f;
            renderer.color = color;
        }

        // Let the entrance start pose reach the screen before advancing its clock.
        for (var frame = 0; frame < GameEntranceWarmupFrameCount; frame++)
        {
            yield return null;
        }

        var totalDuration = Mathf.Max(
            GameEntranceBoardDuration,
            GameEntranceTrayDelay + GameEntranceTrayDuration,
            GameEntranceControlDelay + GameEntranceControlDuration,
            GameEntrancePieceDelay
                + Mathf.Max(0, pieceCount - 1) * GameEntrancePieceStagger
                + GameEntrancePieceDuration);
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
                    Mathf.Clamp01(elapsed / GameEntranceBoardDuration));
                boardRect.anchoredPosition = Vector2.LerpUnclamped(boardStart, boardTarget, boardT);
            }

            if (!didStartOutlineFade && elapsed >= GameEntranceBoardDuration)
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

            for (var i = 0; i < pieceCount; i++)
            {
                var state = _drag.CurrentGroupDraggables[i];
                var renderer = state?.PieceRenderer;
                if (renderer == null)
                {
                    continue;
                }

                var pieceT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (elapsed - GameEntrancePieceDelay - i * GameEntrancePieceStagger)
                        / GameEntrancePieceDuration));
                renderer.transform.position = Vector3.LerpUnclamped(
                    pieceStarts[i],
                    pieceTargets[i],
                    pieceT);
                renderer.transform.localScale = Vector3.LerpUnclamped(
                    pieceTargetScales[i] * 1.12f,
                    pieceTargetScales[i],
                    pieceT);
                renderer.transform.rotation = Quaternion.SlerpUnclamped(
                    Quaternion.Euler(0f, 0f, Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 18f),
                    pieceTargetRotations[i],
                    pieceT);
                var color = pieceTargetColors[i];
                color.a *= pieceT;
                renderer.color = color;
            }

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

    private static Vector3 GetPreviousPackBottomWorldPosition(
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
        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"GameScene: card bag prefab not found at Resources/{resourcePath}.");
            return;
        }

        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        var parent = canvas != null ? canvas.transform : null;
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
        Debug.Log($"GameScene: loaded card bag prefab Resources/{resourcePath}.");
    }

    private void ApplyCardBoardBackground()
    {
        var backgroundImage = _loadedCardBagRoot != null
            ? _loadedCardBagRoot.GetComponent<Image>()
            : null;
        if (backgroundImage == null)
        {
            Debug.LogWarning("GameScene: loaded CardBag root has no background Image.");
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
        backgroundImage.sprite = sprite;
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
        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
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

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceHeight * (16f / 9f), ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = PixelsPerUnit;
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
        SyncEditorLayoutToSprites();
        _board.IsBoardAndGroovesInitialized = true;
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

    private static Image FindSceneImage(string objectName)
    {
        var sceneObject = GameObject.Find(objectName);
        return sceneObject != null ? sceneObject.GetComponent<Image>() : null;
    }

    private static List<List<Image>> CollectEditorGrooveGroups()
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

    private static List<Image> CollectSortedEditorPieceGrooves()
    {
        var groovesByNumber = new Dictionary<int, Image>();
        var images = UnityEngine.Object.FindObjectsOfType<Image>(true);
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

    private void SyncEditorLayoutToSprites()
    {
        SyncImageSizeToSprite(_board.GameBoardImage);
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

    private Vector3 CalculatePieceScaleOnBoard(Image grooveImage)
    {
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

    private Vector3 CalculateTrayScaleForPiece(
        SpriteRenderer pieceRenderer,
        Bounds hostBounds,
        Vector3 dragScale)
    {
        var boardScale = dragScale / _configuredBoardScale;
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return boardScale;
        }

        var scaledHeight = pieceRenderer.sprite.bounds.size.y * boardScale.y;
        var maxHeight = Mathf.Max(0.0001f, hostBounds.size.y * PieceTrayMaxHeightRatio);
        if (scaledHeight <= maxHeight)
        {
            return boardScale;
        }

        return boardScale * (maxHeight / scaledHeight);
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
                SetImageAlpha(grooveImage, 1f);
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

            var dragScale = CalculatePieceScaleOnBoard(grooveImage);
            var trayScale = Vector3.Min(
                CalculateTrayScaleForPiece(pieceRenderer, hostBounds, dragScale),
                dragScale);
            pieceRenderer.transform.localScale = trayScale;
            var pieceCollider = CreateSpriteOverlapCollider(
                pieceRenderer.gameObject,
                pieceRenderer.sprite);
            var grooveProbeCollider = CreateGrooveOverlapProbe(
                pieceRenderer.sprite,
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
                IsOnTray = true,
                IsPlaced = false
            });
        }

        LayoutTrayPieces();
        CachePieceTrayOriginalPosition();
        TryRefreshActiveGroupOutline(
            groupIndex,
            startHidden: true,
            ignoreTutorialBlock: allowOutlineDuringTutorialTransition);
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

                if (isCompletedGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    SetImageAlpha(grooveImage, 1f);
                }
                else if (isActiveGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    SetImageAlpha(grooveImage, IsGroovePersistedAsPlaced(grooveImage) ? 1f : 0f);
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
        StopLoosePieceReminderShake();
        StopTrayPieceReflow();
        ClearPieceHint();
        _drag.DraggingPiece = null;
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
                "Run Puffies/Bake Outline Masks in the Unity Editor.");
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
                    + "placement shine skipped.");
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
        if (_runtimePiecePlacementShineMaterial == null)
        {
            return;
        }

        Destroy(_runtimePiecePlacementShineMaterial);
        _runtimePiecePlacementShineMaterial = null;
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
        EndDragging();
    }

    private void TryBeginDrag(Vector2 screenPosition)
    {
        if (_isGameFinished || _isPiecePlacementAnimating || _isTrayPieceReflowAnimating)
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
            return;
        }

        StopLoosePieceReminderShake();
        if ((_tutorialStage == TutorialStage.StrongPlacement && state == _tutorialPiece)
            || _tutorialStage == TutorialStage.TwoPiecePractice)
        {
            HideTutorialFocusPresentation();
        }

        var world = ToGameplayWorld(screenPosition);
        _drag.DraggingPiece = state;
        _dragStartPosition = state.PieceRenderer.transform.position;
        _drag.DragOffset = state.PieceRenderer.transform.position - world;
        if (state == _hintedPiece)
        {
            state.PieceRenderer.transform.rotation = _hintedPieceBaseRotation;
        }
        state.PieceRenderer.transform.localScale = state.DragScale;
        state.PieceRenderer.sortingOrder = PieceSortingOrder + 100;
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
        if (_drag.DraggingPiece != null)
        {
            GameCursorUtility.SetPieceDrag();
            return;
        }

        if (!_isGameFinished
            && !_isPiecePlacementAnimating
            && !_isTrayPieceReflowAnimating
            && FindDraggablePieceAt(screenPosition) != null)
        {
            GameCursorUtility.SetPieceHover();
            return;
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
        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var world = ToGameplayWorld(screenPosition);
        _drag.DraggingPiece.PieceRenderer.transform.position = new Vector3(
            world.x + _drag.DragOffset.x,
            world.y + _drag.DragOffset.y,
            WorldGameplayDepth);
    }

    private void EndDragging()
    {
        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var state = _drag.DraggingPiece;
        _drag.DraggingPiece = null;
        state.PieceRenderer.sortingOrder = PieceSortingOrder;
        var wasOnTray = state.IsOnTray;

        var groovePosition = GetGrooveSnapPosition(state.GrooveRect, Camera.main);
        UpdateGrooveOverlapProbe(state, groovePosition);
        Physics2D.SyncTransforms();
        var isWithinSnapDistance = state.GrooveRect != null
            && Vector3.Distance(state.PieceRenderer.transform.position, groovePosition)
            <= CalculateSnapDistance(state);
        if (isWithinSnapDistance)
        {
            state.IsOnTray = false;
            var displacedPieces = CollectLoosePiecesOverlappingCollider(
                state,
                state.GrooveProbeCollider);
            if (displacedPieces.Count > 0)
            {
                ReturnLoosePiecesToTray(displacedPieces);
            }

            state.IsPlaced = true;
            if (state == _hintedPiece)
            {
                ClearPieceHint();
            }
            StartGameplayTimerIfNeeded();
            RecordPlacedPiece(state);
            StartCoroutine(PlayPieceSnapAnimation(state, groovePosition));
            return;
        }

        if (CanReturnPieceToTray(state) && ShouldReturnPieceToTray(state.PieceRenderer))
        {
            ResetPieceTrayPosition(instant: true);
            state.IsOnTray = true;
            if (wasOnTray)
            {
                state.PieceRenderer.transform.localScale = state.TrayScale;
                LayoutTrayPieces(animate: true);
                RestorePiecePlacementTutorialPresentation(state);
            }
            else
            {
                LayoutTrayPieces(animate: true, excludedState: state);
                StartCoroutine(
                    PlayInvalidDropReturnAnimation(state, state.StartPosition));
            }
            return;
        }

        state.PieceRenderer.transform.localScale = state.DragScale;
        state.PieceRenderer.transform.position = ClampPieceToTableBounds(state.PieceRenderer);
        Physics2D.SyncTransforms();
        if (DoesPieceOverlapLoosePiece(state) || DoesPieceIntersectOwnGroove(state))
        {
            ReturnPieceAfterInvalidDrop(state, wasOnTray);
            return;
        }

        state.IsOnTray = false;

        RestorePiecePlacementTutorialPresentation(state);
    }

    private bool CanReturnPieceToTray(DraggablePieceState movingState)
    {
        if (movingState != null && movingState.IsOnTray && !IsPieceTrayHidden())
        {
            return true;
        }

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null
                && state != movingState
                && !state.IsPlaced
                && state.IsOnTray
                && state.PieceRenderer != null)
            {
                return true;
            }
        }

        return false;
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
        probeTransform.localScale = state.DragScale;
    }

    private List<DraggablePieceState> CollectLoosePiecesOverlappingCollider(
        DraggablePieceState movingState,
        Collider2D movingCollider)
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
        Collider2D movingCollider)
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

    private static bool DoesPieceIntersectOwnGroove(DraggablePieceState state)
    {
        return state != null
            && CollidersOverlap(state.PieceCollider, state.GrooveProbeCollider);
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

    private void ReturnPieceAfterInvalidDrop(
        DraggablePieceState state,
        bool wasOnTray)
    {
        if (state?.PieceRenderer == null)
        {
            return;
        }

        state.IsOnTray = wasOnTray;
        if (wasOnTray)
        {
            ResetPieceTrayPosition(instant: true);
            LayoutTrayPieces(animate: true, excludedState: state);
        }

        StartCoroutine(PlayInvalidDropReturnAnimation(state, _dragStartPosition));
    }

    private void ReturnLoosePiecesToTray(List<DraggablePieceState> states)
    {
        if (states == null || states.Count == 0)
        {
            return;
        }

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
        Vector3 returnPosition)
    {
        var renderer = state?.PieceRenderer;
        if (renderer == null)
        {
            yield break;
        }

        BeginPiecePlacementAnimation();
        var startPosition = renderer.transform.position;
        var startScale = renderer.transform.localScale;
        var returnScale = state.IsOnTray ? state.TrayScale : state.DragScale;
        var originalColor = renderer.color;
        var invalidColor = Color.LerpUnclamped(
            originalColor,
            InvalidDropTintColor,
            InvalidDropTintStrength);
        invalidColor.a = originalColor.a;
        renderer.sortingOrder = PieceSortingOrder + 100;

        var elapsed = 0f;
        var didEnterTray = false;
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
            if (!didEnterTray
                && state.IsOnTray
                && DoesPieceOverlapTray(renderer))
            {
                didEnterTray = true;
                renderer.color = invalidColor;
            }
            yield return null;
        }

        if (renderer != null)
        {
            renderer.transform.position = returnPosition;
            renderer.transform.localScale = returnScale;

            elapsed = 0f;
            while (didEnterTray
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
        _isPiecePlacementAnimating = true;
    }

    private void EndPiecePlacementAnimation()
    {
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

    private bool ShouldReturnPieceToTray(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        var pieceBounds = renderer.bounds;
        var trayBounds = GetPieceTrayBounds();
        if (pieceBounds.size.y <= 0f || trayBounds.size.sqrMagnitude <= 0f)
        {
            return false;
        }

        var horizontalOverlap = Mathf.Min(pieceBounds.max.x, trayBounds.max.x)
                                - Mathf.Max(pieceBounds.min.x, trayBounds.min.x);
        var verticalOverlap = Mathf.Min(pieceBounds.max.y, trayBounds.max.y)
                              - Mathf.Max(pieceBounds.min.y, trayBounds.min.y);
        return horizontalOverlap > 0f && verticalOverlap >= pieceBounds.size.y * 0.5f;
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
        SetImageAlpha(state.GrooveImage, 1f);
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
                state.DragScale,
                eased);
            yield return null;
        }

        if (renderer != null)
        {
            renderer.transform.position = groovePosition;
            renderer.transform.localScale = state.DragScale;
            renderer.sortingOrder = PieceSortingOrder;
        }

        CommitPlacedPieceToBoardImage(state);
        yield return PlayPiecePlacementSuccessShine(state.GrooveImage);
        EndPiecePlacementAnimation();

        var didAdvanceGroup = TryAdvanceGroup();
        if (!didAdvanceGroup && _tutorialStage == TutorialStage.TwoPiecePractice)
        {
            RefreshPiecePlacementTutorialPresentation();
        }
    }

    private IEnumerator PlayPiecePlacementSuccessShine(Image grooveImage)
    {
        if (grooveImage == null || grooveImage.sprite == null)
        {
            yield break;
        }

        var shineMaterial = GetOrCreatePiecePlacementShineMaterial();
        if (shineMaterial == null)
        {
            yield break;
        }

        var shineObject = CreatePiecePlacementShineOverlay(grooveImage, shineMaterial);
        if (shineObject == null
            || !TryGetRectTransformScreenRect(grooveImage.rectTransform, out var sourceScreenRect))
        {
            if (shineObject != null)
            {
                Destroy(shineObject);
            }

            yield break;
        }

        var shineObjects = new List<GameObject>(1) { shineObject };
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

        DestroyPlacementShineObjects(shineObjects);
    }

    private static GameObject CreatePiecePlacementShineOverlay(
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

    private static void DestroyPlacementShineObjects(List<GameObject> shineObjects)
    {
        for (var i = 0; i < shineObjects.Count; i++)
        {
            if (shineObjects[i] != null)
            {
                Destroy(shineObjects[i]);
            }
        }
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

        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null
                && !state.IsPlaced
                && !state.IsOnTray
                && state.PieceRenderer != null)
            {
                return true;
            }
        }

        return false;
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

            _loosePieceReminderStates.Add(state);
            _loosePieceReminderBaseRotations.Add(state.PieceRenderer.transform.rotation);
        }

        if (_loosePieceReminderStates.Count > 0)
        {
            _loosePieceReminderShakeCoroutine = StartCoroutine(AnimateLoosePieceReminderShake());
        }
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

        var horizontalSpacing = DraggableHorizontalSpacingPixels / PixelsPerUnit;
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

        unplaced.Sort((a, b) => GetPieceNumberFromState(a).CompareTo(GetPieceNumberFromState(b)));
        var trayCenterY = hostBounds.center.y;
        var animatedStates = animate ? new List<DraggablePieceState>() : null;
        var animatedTargets = animate ? new List<Vector3>() : null;
        for (var i = 0; i < unplaced.Count; i++)
        {
            var state = unplaced[i];
            var currentScale = state.PieceRenderer.transform.localScale;
            state.PieceRenderer.transform.localScale = state.TrayScale;
            var pieceWidth = Mathf.Max(0.01f, state.PieceRenderer.bounds.size.x);
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

        var removedPieceNumber = GetPieceNumberFromState(removedState);
        if (removedPieceNumber == int.MaxValue)
        {
            return false;
        }

        var shiftX = GameCommonUtility.GetPieceWidth(
            removedState.PieceRenderer,
            removedState.TrayScale) + DraggableHorizontalSpacingPixels / PixelsPerUnit;
        var states = new List<DraggablePieceState>();
        var targets = new List<Vector3>();
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null
                || state == _drag.DraggingPiece
                || GetPieceNumberFromState(state) <= removedPieceNumber)
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
        var renderedCenterOffset = renderer.bounds.center - renderer.transform.position;
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
            renderer.transform.localScale = pieceTargetScales[i] * 1.08f;
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
                renderer.transform.localScale = Vector3.LerpUnclamped(
                    pieceTargetScales[i] * 1.08f,
                    pieceTargetScales[i],
                    progress);
                renderer.transform.rotation = Quaternion.SlerpUnclamped(
                    Quaternion.Euler(0f, 0f, Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 12f),
                    pieceTargetRotations[i],
                    progress);
                var color = pieceTargetColors[i];
                color.a *= progress;
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
    }

    private void CacheRewardPanelReferences()
    {
        _rewardTaskItem = null;
        _settlementScoreTitleText = null;
        _settlementScoreText = null;
        _settlementBagCountText = null;
        _taskRewardImage = null;
        if (_rewardPanelRoot == null)
        {
            return;
        }

        _rewardTaskItem = _rewardPanelRoot.transform.Find(TaskItemObjectName);
        _settlementScoreTitleText = _rewardPanelRoot.transform.Find(TaskScoreTitlePath)?.GetComponent<TMP_Text>();
        _settlementScoreText = _rewardPanelRoot.transform.Find(TaskScorePath)?.GetComponent<TMP_Text>();
        _settlementBagCountText = _rewardPanelRoot.transform.Find(TaskBagCountPath)?.GetComponent<TMP_Text>();
        _taskRewardImage = _rewardPanelRoot.transform.Find(TaskRewardImgBagPath)?.GetComponent<Image>();

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

        if (_settlementScoreTitleText != null)
        {
            GameFontUtility.ApplyDefaultFont(_settlementScoreTitleText);
        }

        if (_settlementBagCountText == null)
        {
            Debug.LogWarning($"GameScene: card pack count text not found. Expected {TaskBagCountPath}.");
        }
        else
        {
            GameFontUtility.ApplyDefaultFont(_settlementBagCountText);
        }
    }

    private void ShowRewardPanel()
    {
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

        PrepareBoardForRewardPanel();
        _didSavePackCompletion = SaveCardPackAfterPuzzleComplete();
        _isSettlementReadyForFinish = false;
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        _rewardPanelRoot.SetActive(true);
        _rewardPanelRoot.transform.SetAsLastSibling();
        StartCoroutine(ProcessTaskSettlement());
        Debug.Log("GameScene: puzzle completed, RewardPanel shown.");
    }

    private void PrepareBoardForRewardPanel()
    {
        RemoveRuntimePuzzlePieces();
        RevealAllGroovesOnBoard();
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
            SetImageAlpha(grooveImage, 1f);
        }
    }

    private void RemoveRuntimePuzzlePieces()
    {
        StopLoosePieceReminderShake();
        StopTrayPieceReflow();
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

        _isFinishTransitionStarted = true;
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        if (_settlementPackRewardIds.Count == 0
            || _taskRewardImage == null
            || !CardPackRewardFlyTransition.TryStart(
                _taskRewardImage.rectTransform,
                _settlementPackRewardIds))
        {
            GameManager.EnterMainScene();
        }
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
        tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tutorialCanvas.overrideSorting = true;
        tutorialCanvas.sortingOrder = TutorialCanvasSortingOrder;

        var scaler = _tutorialCanvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceHeight * (16f / 9f), ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = PixelsPerUnit;
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
            CreateTutorialHintArrow(promptRect);
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

    private static void CreateTutorialHintArrow(RectTransform parent)
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

        var arrowImage = arrowObject.GetComponent<Image>();
        arrowImage.sprite = templateArrowImage.sprite;
        arrowImage.color = templateArrowImage.color;
        arrowImage.type = templateArrowImage.type;
        arrowImage.preserveAspect = templateArrowImage.preserveAspect;
        arrowImage.raycastTarget = false;
        arrowObject.GetComponent<TutorialHintArrowMotion>().Configure(arrowRect, arrowImage);
    }

    private Vector2 GetTutorialPromptPosition(
        RectTransform parent,
        TutorialStage stage,
        Vector2 promptSize)
    {
        if (stage == TutorialStage.TwoPiecePractice
            && TryGetCurrentTutorialGrooveBounds(parent, out var grooveBounds))
        {
            return ClampTutorialPromptPosition(
                parent.rect,
                new Vector2(
                    grooveBounds.center.x,
                    grooveBounds.yMax + TutorialPracticePromptGap + promptSize.y * 0.5f),
                promptSize);
        }

        var normalizedAnchor = stage == TutorialStage.StrongPlacement
            ? TutorialStrongPromptAnchor
            : TutorialHintPromptAnchor;
        var position = new Vector2(
            Mathf.Lerp(parent.rect.xMin, parent.rect.xMax, normalizedAnchor.x),
            Mathf.Lerp(parent.rect.yMin, parent.rect.yMax, normalizedAnchor.y));
        if (stage == TutorialStage.StrongPlacement)
        {
            position += Vector2.up * promptSize.y + TutorialStrongPromptOffset;
        }

        return ClampTutorialPromptPosition(
            parent.rect,
            position,
            promptSize);
    }

    private bool TryGetCurrentTutorialGrooveBounds(
        RectTransform canvasRect,
        out Rect grooveBounds)
    {
        grooveBounds = default;
        var hasBounds = false;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var grooveRect = _drag.CurrentGroupDraggables[i]?.GrooveRect;
            if (!TryGetRectTransformScreenRect(grooveRect, out var screenRect)
                || !TryScreenRectToCanvasRect(canvasRect, screenRect, out var canvasBounds))
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
        localRect = default;
        if (canvasRect == null
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenRect.min,
                null,
                out var min)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenRect.max,
                null,
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

        var target = FindHintTarget();
        if (target == null)
        {
            return;
        }

        _wasHintUsed = true;
        ShowPieceHint(target);
        Debug.Log("GameScene: hint used; no-hint score bonus disabled for this game.");
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

        _hintedPiece = state;
        _hintedPieceBaseRotation = state.PieceRenderer.transform.rotation;
        _hintShakeStartTime = Time.unscaledTime;
        _isHintPieceShaking = true;
        CreatePieceHintOutline(
            state,
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

        var renderer = _hintedPiece.PieceRenderer;
        if (_hintedPiece.IsPlaced || renderer == null)
        {
            ClearPieceHint();
            return;
        }

        if (_drag.DraggingPiece == _hintedPiece)
        {
            renderer.transform.rotation = _hintedPieceBaseRotation;
            return;
        }

        if (!_isHintPieceShaking)
        {
            return;
        }

        var elapsed = Time.unscaledTime - _hintShakeStartTime;
        if (elapsed >= HintShakeDuration)
        {
            renderer.transform.rotation = _hintedPieceBaseRotation;
            _isHintPieceShaking = false;
            return;
        }

        var angle = Mathf.Sin(
            elapsed * HintShakeCyclesPerSecond * Mathf.PI * 2f) * HintShakeAngle;
        renderer.transform.rotation = _hintedPieceBaseRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void CreatePieceHintOutline(DraggablePieceState state, Color outlineColor)
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

    private void ClearPieceHint()
    {
        if (_hintedPiece?.PieceRenderer != null)
        {
            _hintedPiece.PieceRenderer.transform.rotation = _hintedPieceBaseRotation;
        }

        _hintedPiece = null;
        _hintedPieceBaseRotation = Quaternion.identity;
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
            QueuePackReward(grantedPackId);
        }
    }

    private IEnumerator ProcessTaskSettlement()
    {
        yield return ProcessTaskSettlementCore();

        if (_didSavePackCompletion
            && !_didFailTaskAdvanceDuringSettlement
            && (!_wasSelectedPackCompletedOnEntry || _didAdvanceTaskDuringSettlement))
        {
            TryGrantPendingTaskPackReward(
                _didAdvanceTaskDuringSettlement ? "task completion" : "first-completion retry");
        }

        if (_didSavePackCompletion)
        {
            TryGrantFirstCompletionPackReward();
        }
        RefreshSettlementBagCount();
        _isSettlementReadyForFinish = true;
        if (_finishButton != null)
        {
            _finishButton.interactable = true;
        }
    }

    private IEnumerator ProcessTaskSettlementCore()
    {
        if (_rewardPanelRoot == null)
        {
            yield break;
        }

        SetSettlementScore(0);
        RefreshSettlementBagCount();

        var packId = GameManager.GetBagId();
        var scoreContext = new GameScoreContext
        {
            WasHintUsed = _wasHintUsed,
            IsLevelOutlineEnabled = _isLevelOutlineEnabled,
            IsStickerOutlineEnabled = _isStickerOutlineEnabled,
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
            $"GameScene: score calculated. base={scoreResult.BaseScore}, " +
            $"noHint=+{scoreResult.NoHintBonusPercent}%, " +
            $"levelOutlineOff=+{scoreResult.LevelOutlineDisabledBonusPercent}%, " +
            $"stickerOutlineOff=+{scoreResult.StickerOutlineDisabledBonusPercent}%, " +
            $"time=+{scoreResult.CompletionTimeBonusPercent}% ({scoreResult.CompletionTimeSeconds:F2}s), " +
            $"total=+{scoreResult.TotalBonusPercent}%, final={scoreResult.FinalScore}");

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
        if (!GameTaskUtility.ApplyCompletedPack(
                packId,
                stickerCount,
                settlementScore,
                _wasSelectedPackCompletedOnEntry,
                out var taskContribution))
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
                if (GameTaskUtility.TryCompleteAndAdvanceTask(
                        packId,
                        _wasSelectedPackCompletedOnEntry))
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

        RefreshSettlementBagCount();
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
        return true;
    }

    private void TryGrantPendingTaskPackReward(string source)
    {
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

        QueuePackReward(rewardPackId);
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
        SetSettlementScoreTitle("基础得分");
        yield return AnimateSettlementScoreRange(
            taskItem,
            task,
            progressBeforeSettlement,
            0,
            scoreResult.BaseScore,
            SettlementBaseRollDuration,
            syncTaskWithScore);

        var currentScore = scoreResult.BaseScore;
        var cumulativeBonusPercent = 0;
        if (scoreResult.NoHintBonusPercent > 0)
        {
            cumulativeBonusPercent += scoreResult.NoHintBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                cumulativeBonusPercent);
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                $"未使用提示 +{scoreResult.NoHintBonusPercent}%");
            currentScore = targetScore;
        }

        if (scoreResult.LevelOutlineDisabledBonusPercent > 0)
        {
            cumulativeBonusPercent += scoreResult.LevelOutlineDisabledBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                cumulativeBonusPercent);
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                $"关闭关卡描边 +{scoreResult.LevelOutlineDisabledBonusPercent}%");
            currentScore = targetScore;
        }

        if (scoreResult.StickerOutlineDisabledBonusPercent > 0)
        {
            cumulativeBonusPercent += scoreResult.StickerOutlineDisabledBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                cumulativeBonusPercent);
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                $"关闭贴纸描边 +{scoreResult.StickerOutlineDisabledBonusPercent}%");
            currentScore = targetScore;
        }

        if (scoreResult.CompletionTimeBonusPercent > 0)
        {
            cumulativeBonusPercent += scoreResult.CompletionTimeBonusPercent;
            var targetScore = CalculateSettlementStageScore(
                scoreResult.BaseScore,
                cumulativeBonusPercent);
            yield return AnimateSettlementBonusStage(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                targetScore,
                syncTaskWithScore,
                $"快速完成 +{scoreResult.CompletionTimeBonusPercent}%");
            currentScore = targetScore;
        }

        if (currentScore != scoreResult.FinalScore)
        {
            yield return AnimateSettlementScoreRange(
                taskItem,
                task,
                progressBeforeSettlement,
                currentScore,
                scoreResult.FinalScore,
                TaskProgressRollDuration,
                syncTaskWithScore);
        }

        SetSettlementScore(scoreResult.FinalScore);
        SetSettlementScoreTitle("最终得分");
        if (!syncTaskWithScore && progressAfterSettlement != progressBeforeSettlement)
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
        string title)
    {
        SetSettlementScoreTitle(title);
        yield return new WaitForSecondsRealtime(SettlementStagePauseDuration);
        yield return AnimateSettlementScoreRange(
            taskItem,
            task,
            progressBeforeSettlement,
            fromScore,
            toScore,
            TaskProgressRollDuration,
            syncTaskWithScore);
    }

    private IEnumerator AnimateSettlementScoreRange(
        Transform taskItem,
        TaskInstanceData? task,
        int progressBeforeSettlement,
        int fromScore,
        int toScore,
        float duration,
        bool syncTaskWithScore)
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
            yield break;
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
            yield return null;
        }

        SetSettlementScore(toScore);
        if (syncTaskWithScore)
        {
            SetSettlementTaskProgress(taskItem, task, progressBeforeSettlement + toScore);
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

    private static int CalculateSettlementStageScore(int baseScore, int cumulativeBonusPercent)
    {
        return (baseScore * (100 + cumulativeBonusPercent) + 99) / 100;
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

    private void SetSettlementScoreTitle(string title)
    {
        if (_settlementScoreTitleText != null)
        {
            _settlementScoreTitleText.text = title;
        }
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
        if (_settlementBagCountText == null)
        {
            return;
        }

        _settlementBagCountText.text = CardPackDataUtility.GetCompletedPackCount().ToString();
    }

    private void QueuePackReward(int rewardPackId)
    {
        if (rewardPackId > 0 && !_settlementPackRewardIds.Contains(rewardPackId))
        {
            _settlementPackRewardIds.Add(rewardPackId);
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

        _loadedCardBagRect.anchoredPosition = _originalCardBagAnchoredPosition
            + targetLocalCenter
            - groupLocalCenter;
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
        GameManager.EnterMainScene();
    }
}

internal sealed class TutorialHintArrowMotion : MonoBehaviour
{
    private const float RevealDelay = 0.34f;
    private const float RevealDuration = 0.22f;
    private const float PulseDuration = 0.72f;
    private static readonly Vector2 PulseOffset = new Vector2(14f, 8f);
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

internal sealed class HintDashedOutlineGraphic : MaskableGraphic
{
    private const byte AlphaThreshold = 26;
    private const float OutlineSimplifyTolerancePixels = 0.75f;
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

    private void Update()
    {
        if (_sourceSprite != null && _spritePaths.Count > 0 && Mathf.Abs(_scrollSpeed) > 0.001f)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        if (_sourceSprite == null || _spritePaths.Count == 0)
        {
            return;
        }

        var spriteBounds = _sourceSprite.bounds;
        if (spriteBounds.size.x <= 0.0001f || spriteBounds.size.y <= 0.0001f)
        {
            return;
        }

        var targetRect = GetDrawingRect(spriteBounds, rectTransform.rect);
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

    private Rect GetDrawingRect(Bounds spriteBounds, Rect targetRect)
    {
        if (!_preserveAspect)
        {
            return targetRect;
        }

        var spriteAspect = spriteBounds.size.x / spriteBounds.size.y;
        var rectAspect = targetRect.width / targetRect.height;
        if (spriteAspect > rectAspect)
        {
            var originalHeight = targetRect.height;
            targetRect.height = targetRect.width / spriteAspect;
            targetRect.y += (originalHeight - targetRect.height) * rectTransform.pivot.y;
        }
        else
        {
            var originalWidth = targetRect.width;
            targetRect.width = targetRect.height * spriteAspect;
            targetRect.x += (originalWidth - targetRect.width) * rectTransform.pivot.x;
        }

        return targetRect;
    }

    private static bool TryBuildAlphaPaths(Sprite sprite, List<List<Vector2>> outputPaths)
    {
        if (!TryReadSpritePixels(sprite, out var pixels, out var width, out var height))
        {
            return false;
        }

        var outgoingEdges = new Dictionary<Vector2Int, List<Vector2Int>>();
        var unusedEdges = new HashSet<PixelEdge>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!IsOpaque(pixels, width, height, x, y))
                {
                    continue;
                }

                if (!IsOpaque(pixels, width, height, x - 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }

                if (!IsOpaque(pixels, width, height, x, y + 1))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
                }

                if (!IsOpaque(pixels, width, height, x + 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));
                }

                if (!IsOpaque(pixels, width, height, x, y - 1))
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

    private static bool TryReadSpritePixels(
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

    private static bool IsOpaque(Color32[] pixels, int width, int height, int x, int y)
    {
        return x >= 0
            && x < width
            && y >= 0
            && y < height
            && pixels[y * width + x].a >= AlphaThreshold;
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
